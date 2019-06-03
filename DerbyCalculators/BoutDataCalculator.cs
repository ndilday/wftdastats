using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    public class BoutDataCalculator
    {
        private string _connectionString;
        private IList<JamTeamData> _jamTeamData = null;
        private Dictionary<FoulComparison, Dictionary<int, float>> _sss = null;
        private IList<JamTeamEffectiveness> _jamTeamEffectiveness = null;
        private IList<JamPlayer> _jamPlayers = null;
        private IList<PenaltyGroup> _penaltyGroups = null;
        private Dictionary<int, int> _boxTimeEstimates = null;
        static private int _year;

        public BoutDataCalculator(string connectionString,
                                  Dictionary<FoulComparison, Dictionary<int, float>> sss,
                                  IList<JamTeamData> jamTeamData,
                                  int year)
        {
            _connectionString = connectionString;
            _sss = sss;
            _jamTeamData = jamTeamData;
            _year = year;
        }

        public void CalculateSecondaryTables()
        {
            Stopwatch timer = new Stopwatch();
            //new PlayerTrueSkillCalculator(connString).CalculateTrueSkills();
            Console.WriteLine("Calculating Jam Team Effectiveness");
            timer.Restart();
            CalculateJamTeamEffectiveness();
            timer.Stop();
            Console.WriteLine("Finished Calculating Jam Team Effectiveness: " + timer.Elapsed.TotalSeconds);

            Console.WriteLine("Calculating Player Effectiveness");
            timer.Restart();
            CalculatePlayerEffectiveness();
            timer.Stop();
            Console.WriteLine("Finished Calculating Player Effectiveness: " + timer.Elapsed.TotalSeconds);

            Console.WriteLine("Calculating Average Penalty Cost");
            timer.Restart();
            CalculateAveragePenaltyCosts();
            timer.Stop();
            Console.WriteLine("Finished Calculating Average Penalty Cost: " + timer.Elapsed.TotalSeconds);
        }

        private void CalculateJamTeamEffectiveness()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            _jamTeamEffectiveness = new List<JamTeamEffectiveness>();
            foreach (JamTeamData jamData in _jamTeamData)
            {
                _jamTeamEffectiveness.Add(new JamTeamEffectiveness
                {
                    JamID = jamData.JamID,
                    TeamID = jamData.TeamID,
                    Percentile = _sss[jamData.FoulComparison][jamData.PointDelta]
                });
            }

            new JamTeamEffectivenessGateway(connection, transaction).InsertJamTeamEffectiveness(_jamTeamEffectiveness);
            transaction.Commit();
            connection.Close();
        }

        private void CalculatePlayerEffectiveness()
        {
            if (_jamTeamEffectiveness == null)
            {
                CalculateJamTeamEffectiveness();
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            _jamPlayers = new JamPlayerGateway(connection, transaction).GetJamPlayers();
            _penaltyGroups = new PenaltyGroupGateway(connection, transaction).GetAllPenaltyGroups();

            // these three get mapped by jam
            var jteMap = _jamTeamEffectiveness.GroupBy(jte => jte.JamID).ToDictionary(g => g.Key);
            var jamDataMap = _jamTeamData.GroupBy(jf => jf.JamID).ToDictionary(g => g.Key);
            var jpJamMap = _jamPlayers.GroupBy(jp => jp.JamID).ToDictionary(g => g.Key);
            var jamTimeMap = new JamTimeLimitGateway(connection, transaction).GetAllJamTimeEstimates().ToDictionary(jte => jte.JamID);
            //var jpPlayerMap = players.GroupBy(jp => jp.PlayerID).ToDictionary(g => g.Key);
            
            // the best we can do with this is map it by player
            var pgMap = _penaltyGroups.GroupBy(pg => pg.PlayerID).ToDictionary(g => g.Key, g => g.ToList());

            Dictionary<int, List<JamPlayerEffectiveness>> pjeMap = new Dictionary<int, List<JamPlayerEffectiveness>>();
            Dictionary<int, List<PenaltyGroup>> jamBoxTimeMap1 = new Dictionary<int, List<PenaltyGroup>>();
            Dictionary<int, List<PenaltyGroup>> jamBoxTimeMap2 = new Dictionary<int, List<PenaltyGroup>>();
            _boxTimeEstimates = new BoxTimeEstimateGateway(connection, transaction).GetAllBoxTimeEstimates();
            int maxJamID = jteMap.Keys.Max();
            int jamID = 1;
            while(jamID <= maxJamID)
            {
                if(!jteMap.ContainsKey(jamID))
                {
                    jamID++;
                    continue;
                }
                var tempJte = jteMap[jamID].OrderBy(jte => jte.TeamID);
                JamTeamEffectiveness jte1 = tempJte.First();
                JamTeamEffectiveness jte2 = tempJte.Last();
                var team1Players = jpJamMap[jamID].Where(jp => jp.TeamID == jte1.TeamID);
                var team2Players = jpJamMap[jamID].Where(jp => jp.TeamID == jte2.TeamID);
                List<JamPlayerEffectiveness> pjeList = new List<JamPlayerEffectiveness>(5);
                int jamTime = jamTimeMap[jamID].Estimate;
                
                // handle each player
                foreach (JamPlayer player in team1Players)
                {
                    ProcessJamPlayer(pgMap, jamBoxTimeMap1, _boxTimeEstimates, jamID, jte1, pjeList, jamTime, player);
                }

                List<JamPlayerEffectiveness> pjeList2 = new List<JamPlayerEffectiveness>(5);
                // handle each player
                foreach (JamPlayer player in team2Players)
                {
                    ProcessJamPlayer(pgMap, jamBoxTimeMap2, _boxTimeEstimates, jamID, jte2, pjeList, jamTime, player);
                }

                pjeMap[jamID] = pjeList;
                pjeMap[jamID].AddRange(pjeList2);
                jamID++;
            }

            jamID = 1;
            while(jamID <= maxJamID)
            {
                if (!jteMap.ContainsKey(jamID))
                {
                    jamID++;
                    continue;
                }
                var tempJte = jteMap[jamID].OrderBy(jte => jte.TeamID);
                JamTeamEffectiveness jte1 = tempJte.First();
                JamTeamEffectiveness jte2 = tempJte.Last();
                // apply box time costs to players with correlated penalties
                if (jamBoxTimeMap1.ContainsKey(jamID))
                {
                    AssignPenaltyCosts(jamDataMap, jamTimeMap, pjeMap, jamBoxTimeMap1, _boxTimeEstimates, jamID, jte1);
                }
                // apply box time costs to players with correlated penalties
                if (jamBoxTimeMap2.ContainsKey(jamID))
                {
                    AssignPenaltyCosts(jamDataMap, jamTimeMap, pjeMap, jamBoxTimeMap2, _boxTimeEstimates, jamID, jte2);
                }
                jamID++;
            }
            new JamPlayerEffectivenessGateway(connection, transaction).InsertJamPlayerEffectiveness(pjeMap);
            transaction.Commit();
            connection.Close();
        }

        public void CalculateAveragePenaltyCosts()
        {
            // pull data
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            var jamData = _jamTeamData
                .GroupBy(jd => jd.JamID)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(g2 => g2.TeamID, g2 => g2)
                );
            var jamPlayerMap = _jamPlayers
                .GroupBy(jp => jp.JamID)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(g2 => g2.PlayerID, g2 => g2)
                );

            Dictionary<FoulComparison, Dictionary<int, float>> sss = new SituationalScoreGateway(connection, transaction).GetAllSituationalScores();
            var penaltyGroupCost = PenaltyCostCalculator.CalculatePointCosts(jamData, jamPlayerMap, _penaltyGroups, _boxTimeEstimates, sss);
            var jteMap = _jamTeamEffectiveness.GroupBy(jte => jte.JamID).ToDictionary(g => g.Key, g => g.ToDictionary(g2 => g2.TeamID, g2 => g2.Percentile));
            var penaltyGroupValueCost = PenaltyCostCalculator.CalculateValueCosts(jamData, jamPlayerMap, _penaltyGroups, _boxTimeEstimates, sss, jteMap);
            double jammerPointCost = 0;
            double blockerPointCost = 0;
            double jammerValueCost = 0;
            double blockerValueCost = 0;
            foreach (PenaltyGroup pg in _penaltyGroups)
            {
                if(!penaltyGroupCost.ContainsKey(pg.GroupID) || !penaltyGroupValueCost.ContainsKey(pg.GroupID))
                {
                    continue;
                }
                // determine whether this penalty group is for a jammer or blocker
                var penalty = pg.Penalties[0];
                if (jamPlayerMap[penalty.JamID][penalty.PlayerID].IsJammer)
                {
                    jammerPointCost += penaltyGroupCost[pg.GroupID] * pg.Penalties.Count;
                    jammerValueCost += penaltyGroupValueCost[pg.GroupID] * pg.Penalties.Count;
                }
                else
                {
                    blockerPointCost += penaltyGroupCost[pg.GroupID] * pg.Penalties.Count;
                    blockerValueCost += penaltyGroupValueCost[pg.GroupID] * pg.Penalties.Count;
                }
            }
            new AveragePenaltyCostGateway(connection, transaction).InsertAveragePenaltyCost( new AveragePenaltyCostPerJam
            {
                BlockerPointCost = blockerPointCost / (8 * jamData.Keys.Count),
                BlockerValueCost = blockerValueCost / (8 * jamData.Keys.Count),
                JammerPointCost = jammerPointCost / (2 * jamData.Keys.Count),
                JammerValueCost = jammerValueCost / (2 * jamData.Keys.Count)
            });
            transaction.Commit();
            connection.Close();
        }


        private void AssignPenaltyCosts(Dictionary<int, IGrouping<int, JamTeamData>> jamDataMap, Dictionary<int, JamTimeEstimate> jamTimeMap, 
                                        Dictionary<int, List<JamPlayerEffectiveness>> pjeMap, Dictionary<int, List<PenaltyGroup>> jamBoxTimeMap, 
                                        Dictionary<int, int> boxTimeEstimates, int jamID, JamTeamEffectiveness jte)
        {
            List<PenaltyGroup> jamPenaltyGroups = jamBoxTimeMap[jamID];
            JamTeamData thisJamData = jamDataMap[jamID].First(jd => jd.TeamID == jte.TeamID);
            foreach (PenaltyGroup group in jamPenaltyGroups)
            {
                int penaltyPlayerID = group.Penalties[0].PlayerID;
                foreach (BoxTime boxTime in group.BoxTimes)
                {
                    if (boxTime.JamID == jamID)
                    {
                        // determine how big the swing would be if there had been no box time served
                        double qualityDifference = DetermineQualityWithoutPenalty(thisJamData, boxTime.IsJammer) - jte.Percentile;

                        // the cost of the box time is how bad that player would have to be in that jam to have an equivalent effect
                        // we estimate this based on how important the player is in the jam, by position
                        double penaltyCost = boxTime.IsJammer ? qualityDifference * 2 : qualityDifference * 8;
                        double totalPenaltyTime = boxTime.IsJammer ?
                            thisJamData.JammerBoxTime :
                            thisJamData.BlockerBoxTime;
                        // modify this by how much of the total penalty time this player contributed

                        // doing the rough estimate version for now
                        double penaltyPortion = boxTimeEstimates[boxTime.BoxTimeID] / totalPenaltyTime;

                        // a portion of that cost goes to each penalty that factors into this box time
                        int penaltyCount = group.Penalties.Count;
                        foreach (Penalty penalty in group.Penalties)
                        {
                            var penaltyPJE = pjeMap[penalty.JamID].First(pje => pje.PlayerID == penalty.PlayerID);
                            penaltyPJE.PenaltyCost += penaltyPortion * penaltyCost / penaltyCount;
                            if(double.IsNaN(penaltyPJE.PenaltyCost))
                            {
                                throw new Exception(penalty.JamID + ": bad penalty data");
                            }
                        }

                        // for a given penalty group, there should only ever be 
                        // a single box time in a given jam
                        break;
                    }
                }
            }
        }

        private static void ProcessJamPlayer(Dictionary<int, List<PenaltyGroup>> pgMap, Dictionary<int, List<PenaltyGroup>> jamBoxTimeMap, Dictionary<int, int> boxTimeEstimates, 
                                      int jamID, JamTeamEffectiveness jte1, List<JamPlayerEffectiveness> pjeList, int jamTime, JamPlayer player)
        {
            // handle penalties
            var playerPenaltyGroups = pgMap.ContainsKey(player.PlayerID) ? pgMap[player.PlayerID] : null;
            ProcessPlayerJamPenalties(jamBoxTimeMap, jamID, playerPenaltyGroups);

            // try to estimate what portion of a jam someone missed via time in the box
            int timeInBox = 0;

            if (jamBoxTimeMap.ContainsKey(jamID))
            {
                foreach (PenaltyGroup group in jamBoxTimeMap[jamID])
                {
                    foreach (BoxTime bt in group.BoxTimes)
                    {
                        if (bt.PlayerID == player.PlayerID && bt.JamID == jamID)
                        {
                            // factor in estimated box time
                            timeInBox += boxTimeEstimates[bt.BoxTimeID];
                        }
                    }
                }
            }

            JamPlayerEffectiveness pje = new JamPlayerEffectiveness
            {
                PlayerID = player.PlayerID,
                TeamID = player.TeamID,
                JamPortion = ((double)jamTime - timeInBox) / jamTime,
                BaseQuality = jte1.Percentile,
                JamID = jamID,
                IsJammer = player.IsJammer,
                PenaltyCost = 0
            };
            pjeList.Add(pje);
        }

        private double DetermineQualityWithoutPenalty(JamTeamData jamData, bool isJammer)
        {
            // figure out the foul differential if this team did not commit fouls of this time this jam
            int jammerPenaltyDiff = (isJammer ? 0 : jamData.JammerBoxTime) - jamData.OppJammerBoxTime;
            int blockerPenaltyDiff = (isJammer ? jamData.BlockerBoxTime : 0) - jamData.OppBlockerBoxTime;
            double jammerBoxComp = Math.Round(jammerPenaltyDiff / 15.0, MidpointRounding.AwayFromZero) / 2.0;
            double blockerBoxComp = Math.Round(blockerPenaltyDiff / 15.0, MidpointRounding.AwayFromZero) / 2.0;
            FoulComparison foul = new FoulComparison
            {
                Year = jamData.Year,
                JammerBoxComparison = jammerBoxComp,
                BlockerBoxComparison = blockerBoxComp
            };

            if (!_sss.ContainsKey(foul))
            {
                // lacking anything better, sum the distance of each factor from the percentile of the base
                JamTeamData baseJamData = new JamTeamData
                {
                    Year = jamData.Year,
                    BlockerBoxTime = 0,
                    JammerBoxTime = 0,
                    OppBlockerBoxTime = 0,
                    OppJammerBoxTime = 0,
                    PointDelta = jamData.PointDelta
                };
                double baseQuality = DetermineQualityWithoutPenalty(baseJamData, isJammer);

                // pull team 1 values
                baseJamData.BlockerBoxTime = isJammer ? jamData.BlockerBoxTime : 0;
                baseJamData.JammerBoxTime = isJammer ? 0 : jamData.JammerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0.5;
                }
                double quality1 = DetermineQualityWithoutPenalty(baseJamData, isJammer) - baseQuality;

                // pull team 2 blocker
                baseJamData.BlockerBoxTime = 0;
                baseJamData.JammerBoxTime = 0;
                baseJamData.OppBlockerBoxTime= jamData.OppBlockerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0.5;
                }
                double quality2 = DetermineQualityWithoutPenalty(baseJamData, isJammer) - baseQuality;

                // pull team 2 jammer
                baseJamData.OppBlockerBoxTime = 0;
                baseJamData.OppJammerBoxTime = jamData.OppJammerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0.5;
                }
                double quality3 = DetermineQualityWithoutPenalty(baseJamData, isJammer) - baseQuality;

                return baseQuality + quality1 + quality2 + quality3;
            }
            else if(!_sss[foul].ContainsKey(jamData.PointDelta))
            {
                // extrapolate from the points we do have
                var bottomList = _sss[foul].Keys.Where(k => k < jamData.PointDelta);
                int bottom = bottomList.Any() ? bottomList.Max() : -55;
                double bottomPercentile =bottomList.Any() ?  _sss[foul][bottom] : 0;

                var topList = _sss[foul].Keys.Where(k => k > jamData.PointDelta);
                int top = topList.Any() ? topList.Min() : 55;
                double topPercentile = topList.Any() ? _sss[foul][top] : 1;
                int distance = top - bottom;
                int portion = jamData.PointDelta - bottom;
                double ratio = ((double)portion) / distance;
                return bottomPercentile + ((topPercentile - bottomPercentile) * ratio);
            }
            else
            {
                return _sss[foul][jamData.PointDelta];
            }
        }

        private static void ProcessPlayerJamPenalties(Dictionary<int, List<PenaltyGroup>> jamBoxTimeMap, int jamID, List<PenaltyGroup> playerPenaltyGroups)
        {
            // see if this player committed any penalties this jam
            if (playerPenaltyGroups != null)
            {
                var pertinentPenaltyGroups = playerPenaltyGroups.Where(pg => pg.Penalties.Any(p => p.JamID == jamID));
                if (pertinentPenaltyGroups.Any())
                {
                    // add the penalty group to the jams of box time service
                    // so that we can come back and add to the penalty cost
                    // when we process the jam box times
                    foreach (PenaltyGroup pg in pertinentPenaltyGroups)
                    {
                        foreach (BoxTime bt in pg.BoxTimes)
                        {
                            if (!jamBoxTimeMap.ContainsKey(bt.JamID))
                            {
                                jamBoxTimeMap[bt.JamID] = new List<PenaltyGroup>();
                            }
                            var jbt = jamBoxTimeMap[bt.JamID];
                            if (!jbt.Contains(pg))
                            {
                                jbt.Add(pg);
                            }
                        }
                    }
                }
            }
        }
    }
}
