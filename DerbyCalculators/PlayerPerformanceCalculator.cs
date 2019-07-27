using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DerbyCalculators.Models;
using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    public class PlayerPerformanceCalculator
    {
        static DateTime STATS_START_DATE = new DateTime(2019, 1, 1);
        string _connectionString;
        public PlayerPerformanceCalculator(string connString)
        {
            _connectionString = connString;
        }

        public IList<PlayerPerformance> GetPlayerPointPerformancesForTeam(int teamID)
        {
            // pull data
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            var players = new PlayerGateway(connection, transaction).GetPlayersForTeam(teamID).ToDictionary(p => p.ID);
            var jams = new JamGateway(connection, transaction).GetJamsForTeamAfterDate(teamID, STATS_START_DATE).OrderBy(j => j.ID);
            var jamBoutMap = jams.ToDictionary(j => j.ID, j => j.BoutID);
            var jpe = new JamPlayerEffectivenessGateway(connection, transaction).GetJamPlayerEffectivenessForTeam(teamID);
            var jdg = new JamDataGateway(connection, transaction);
            var jamTeamData = jdg.GetJamDataForTeam(teamID).ToDictionary(jd => jd.JamID);
            var jamData = jdg.GetAllJamData().ToDictionary(jd => jd.JamID);
            foreach(JamTeamData jtd in jamTeamData.Values)
            {
                jtd.Year = jamData[jtd.JamID].PlayDate.Year;
            }
            var teams = new TeamGateway(connection, transaction).GetAllTeams().ToDictionary(t => t.ID);
            var bouts = new BoutGateway(connection, transaction).GetBouts().ToDictionary(t => t.ID);
            var penalties = new PenaltyGateway(connection, transaction).GetPenaltiesForTeam(teamID)
                .GroupBy(p => p.JamID)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(g2 => g2.PlayerID).ToDictionary(g3 => g3.Key, g3 => g3.ToList()));
            var pgs = new PenaltyGroupGateway(connection, transaction).GetPenaltyGroupsForTeamAfterDate(teamID, STATS_START_DATE);
            Dictionary<int, int> boxTimeEstimates = new BoxTimeEstimateGateway(connection, transaction).GetAllBoxTimeEstimates();
            Dictionary<FoulComparison, Dictionary<int, float>> sss = new SituationalScoreGateway(connection, transaction).GetAllSituationalScores();
            AveragePenaltyCostPerJam avgPenCost = new AveragePenaltyCostGateway(connection, transaction).GetAveragePenaltyCost();
            transaction.Commit();
            connection.Close();

            Dictionary<FoulComparison, float> medians = CalculateMedianScores(sss);
            PenaltyCostCalculator ppcCalc = new PenaltyCostCalculator(_connectionString);
            Dictionary<int, double> groupPenaltyCostMap = ppcCalc.CalculatePointCostsForTeam(jamTeamData, pgs, boxTimeEstimates, sss);
            Dictionary<int, PlayerPerformance> pps = new Dictionary<int, PlayerPerformance>(25);
            Dictionary<int, double> jamTotalPortionMap = new Dictionary<int, double>(300);

            // we use "jam portions" to divide up the total value of the jam among participating players
            // we give jammers an additional multiplier to their jam portions, based on year, to represent their greater impact on team success

            foreach(Jam jam in jams)
            {
                var pe = jpe[jam.ID];
                foreach(JamPlayerEffectiveness eff in pe.Values)
                {
                    // get/set PlayerPerformance object
                    if(!pps.ContainsKey(eff.PlayerID))
                    {
                        pps[eff.PlayerID] = new PlayerPerformance
                        {
                            Player = players[eff.PlayerID],
                            Bouts = new List<BoutPerformance>()
                        };
                    }
                    PlayerPerformance curPP = pps[eff.PlayerID];

                    // get/set BoutPerformance object
                    Bout bout = bouts[jam.BoutID];
                    BoutPerformance bp = null;
                    if (!curPP.Bouts.Any() || 
                        curPP.Bouts.Last().BoutID != bout.ID)
                    {
                        bp = new BoutPerformance
                        {
                            BoutID = bout.ID,
                            AwayTeamName = teams[bout.AwayTeamID].Name,
                            HomeTeamName = teams[bout.HomeTeamID].Name,
                            BoutDate = bout.BoutDate,
                            Jams = new List<JamPerformance>()
                        };
                        curPP.Bouts.Add(bp);
                    }
                    else
                    {
                        bp = curPP.Bouts.Last();
                    }

                    JamTeamData jd = jamTeamData[jam.ID];
                    int penaltyCount = penalties.ContainsKey(jam.ID) && penalties[jam.ID].ContainsKey(eff.PlayerID) ? penalties[jam.ID][eff.PlayerID].Count() : 0;
                    JamPerformance jp = new JamPerformance
                    {
                        BlockerJamPercentage = eff.IsJammer ? 0 : eff.JamPortion,
                        DeltaPercentile = eff.BaseQuality,
                        IsFirstHalf = jam.IsFirstHalf,
                        JamID = jam.ID,
                        JammerJamPercentage = eff.IsJammer ? eff.JamPortion : 0,
                        JamNumber = jam.JamNumber,
                        JamPenalties = penaltyCount,
                        MedianDelta = medians[jd.FoulComparison],
                        PenaltyCost = 0,
                        PointDelta = jd.PointDelta
                    };
                    double jammerRatio = bouts[jamBoutMap[jam.ID]].BoutDate.Year == 2019 ? 2.0 : 4.0;
                    if (jamTotalPortionMap.ContainsKey(jam.ID))
                    {
                        jamTotalPortionMap[jam.ID] += eff.IsJammer ? eff.JamPortion * jammerRatio : eff.JamPortion;
                    }
                    else 
                    {
                        jamTotalPortionMap[jam.ID] = eff.IsJammer ? eff.JamPortion * jammerRatio : eff.JamPortion;
                    }
                    bp.Jams.Add(jp);
                }
            }

            foreach(PenaltyGroup pg in pgs)
            {
                foreach(Penalty p in pg.Penalties)
                {
                    if (jams.Any(j => j.ID == p.JamID))
                    {
                        JamPerformance jp = pps[p.PlayerID].Bouts.SelectMany(b => b.Jams).Where(j => j.JamID == p.JamID).First();
                        jp.PenaltyCost += groupPenaltyCostMap[pg.GroupID];
                    }
                }
            }

            foreach(PlayerPerformance pp in pps.Values)
            {
                RollUpPlayerPerformance(avgPenCost, jamTotalPortionMap, pp);
            }

            CalculateTeamAverages(pps, bouts);
            foreach(PlayerPerformance pp in pps.Values)
            {
                pp.BlockerPerformance.PlayerValueVersusTeamAverage = pp.Bouts.Sum(b => b.BlockerPerformance.PlayerValueVersusTeamAverage);
                pp.JammerPerformance.PlayerValueVersusTeamAverage = pp.Bouts.Sum(b => b.JammerPerformance.PlayerValueVersusTeamAverage);
            }

            return pps.Values.ToList();
        }

        public IList<PlayerPerformance> GetPlayerValuePerformancesForTeam(int teamID)
        {
            // pull data
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            var players = new PlayerGateway(connection, transaction).GetPlayersForTeam(teamID).ToDictionary(p => p.ID);
            var jams = new JamGateway(connection, transaction).GetJamsForTeamAfterDate(teamID, new DateTime(2016, 1, 1)).OrderBy(j => j.ID);
            var jamBoutMap = jams.ToDictionary(j => j.ID, j => j.BoutID);
            var jpe = new JamPlayerEffectivenessGateway(connection, transaction).GetJamPlayerEffectivenessForTeam(teamID);
            var jamData = new JamDataGateway(connection, transaction).GetJamDataForTeam(teamID).ToDictionary(jd => jd.JamID);
            var teams = new TeamGateway(connection, transaction).GetAllTeams().ToDictionary(t => t.ID);
            var bouts = new BoutGateway(connection, transaction).GetBouts().ToDictionary(t => t.ID);
            var penalties = new PenaltyGateway(connection, transaction).GetPenaltiesForTeam(teamID)
                .GroupBy(p => p.JamID)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(g2 => g2.PlayerID).ToDictionary(g3 => g3.Key, g3 => g3.ToList()));
            var pgs = new PenaltyGroupGateway(connection, transaction).GetPenaltyGroupsForTeam(teamID);
            Dictionary<int, int> boxTimeEstimates = new BoxTimeEstimateGateway(connection, transaction).GetAllBoxTimeEstimates();
            Dictionary<FoulComparison, Dictionary<int, float>> sss = new SituationalScoreGateway(connection, transaction).GetAllSituationalScores();
            Dictionary<int, double> jte = new JamTeamEffectivenessGateway(connection, transaction).GetJamTeamEffectivenessForTeam(teamID);
            AveragePenaltyCostPerJam avgPenCost = new AveragePenaltyCostGateway(connection, transaction).GetAveragePenaltyCost();
            transaction.Commit();
            connection.Close();

            Dictionary<FoulComparison, float> medians = CalculateMedianScores(sss);
            PenaltyCostCalculator pcCalc = new PenaltyCostCalculator(_connectionString);
            Dictionary<int, double> groupPenaltyCostMap = pcCalc.CalculateValueCostsForTeam(jamData, pgs, boxTimeEstimates, sss, jte);
            Dictionary<int, PlayerPerformance> ppMap = new Dictionary<int, PlayerPerformance>(25);
            Dictionary<int, double> jamTotalPortionMap = new Dictionary<int, double>(300);

            foreach (Jam jam in jams)
            {
                var pe = jpe[jam.ID];
                foreach (JamPlayerEffectiveness eff in pe.Values)
                {
                    // get/set PlayerPerformance object
                    if (!ppMap.ContainsKey(eff.PlayerID))
                    {
                        ppMap[eff.PlayerID] = new PlayerPerformance
                        {
                            Player = players[eff.PlayerID],
                            Bouts = new List<BoutPerformance>()
                        };
                    }
                    PlayerPerformance curPP = ppMap[eff.PlayerID];

                    // get/set BoutPerformance object
                    Bout bout = bouts[jam.BoutID];
                    BoutPerformance bp = null;
                    if (!curPP.Bouts.Any() ||
                        curPP.Bouts.Last().BoutDate != bout.BoutDate ||
                        curPP.Bouts.Last().HomeTeamName != teams[bout.HomeTeamID].Name ||
                        curPP.Bouts.Last().AwayTeamName != teams[bout.AwayTeamID].Name)
                    {
                        bp = new BoutPerformance
                        {
                            AwayTeamName = teams[bout.AwayTeamID].Name,
                            HomeTeamName = teams[bout.HomeTeamID].Name,
                            BoutDate = bout.BoutDate,
                            Jams = new List<JamPerformance>()
                        };
                        curPP.Bouts.Add(bp);
                    }
                    else
                    {
                        bp = curPP.Bouts.Last();
                    }

                    JamTeamData jd = jamData[jam.ID];
                    int penaltyCount = penalties.ContainsKey(jam.ID) && penalties[jam.ID].ContainsKey(eff.PlayerID) ? penalties[jam.ID][eff.PlayerID].Count() : 0;
                    JamPerformance jp = new JamPerformance
                    {
                        BlockerJamPercentage = eff.IsJammer ? 0 : eff.JamPortion,
                        DeltaPercentile = eff.BaseQuality,
                        IsFirstHalf = jam.IsFirstHalf,
                        JamID = jam.ID,
                        JammerJamPercentage = eff.IsJammer ? eff.JamPortion : 0,
                        JamNumber = jam.JamNumber,
                        JamPenalties = penaltyCount,
                        MedianDelta = medians[jd.FoulComparison],
                        PenaltyCost = 0,
                        PointDelta = jd.PointDelta
                    };
                    if (jamTotalPortionMap.ContainsKey(jam.ID))
                    {
                        jamTotalPortionMap[jam.ID] += eff.IsJammer ? eff.JamPortion * 4 : eff.JamPortion;
                    }
                    else
                    {
                        jamTotalPortionMap[jam.ID] = eff.IsJammer ? eff.JamPortion * 4 : eff.JamPortion;
                    }
                    bp.Jams.Add(jp);
                }
            }

            foreach (PenaltyGroup pg in pgs)
            {
                foreach (Penalty p in pg.Penalties)
                {
                    JamPerformance jp = ppMap[p.PlayerID].Bouts.SelectMany(b => b.Jams).Where(j => j.JamID == p.JamID).First();
                    jp.PenaltyCost += groupPenaltyCostMap[pg.GroupID];
                }
            }

            foreach (PlayerPerformance pp in ppMap.Values)
            {
                pp.BlockerPerformance = new RolledUpPerformanceData();
                pp.JammerPerformance = new RolledUpPerformanceData();
                foreach (BoutPerformance bp in pp.Bouts)
                {
                    bp.BlockerPerformance = new RolledUpPerformanceData();
                    bp.JammerPerformance = new RolledUpPerformanceData();
                    foreach (JamPerformance jp in bp.Jams)
                    {
                        double averagePenaltyCost = jp.JammerJamPercentage > 0 ? avgPenCost.JammerValueCost : avgPenCost.BlockerValueCost;
                        double jamShare = (jp.JammerJamPercentage * 4 + jp.BlockerJamPercentage) / jamTotalPortionMap[jp.JamID];
                        jp.DeltaPortionVersusMedian = (jp.DeltaPercentile - 0.5) * jamShare;
                        jp.PlayerValue = jp.DeltaPortionVersusMedian + jp.PenaltyCost - averagePenaltyCost;
                        var rollUp = jp.JammerJamPercentage > 0 ? bp.JammerPerformance : bp.BlockerPerformance;
                        rollUp.TotalJamPortions += jp.JammerJamPercentage + jp.BlockerJamPercentage;
                        rollUp.TotalPenalties += jp.JamPenalties;
                        rollUp.TotalPenaltyCost += jp.PenaltyCost;
                        rollUp.TotalPointsVersusMedian += jp.DeltaPortionVersusMedian;
                        rollUp.TotalPlayerValue += jp.PlayerValue;
                    }

                    pp.BlockerPerformance.TotalJamPortions += bp.BlockerPerformance.TotalJamPortions;
                    pp.BlockerPerformance.TotalPenalties += bp.BlockerPerformance.TotalPenalties;
                    pp.BlockerPerformance.TotalPenaltyCost += bp.BlockerPerformance.TotalPenaltyCost;
                    pp.BlockerPerformance.TotalPointsVersusMedian += bp.BlockerPerformance.TotalPointsVersusMedian;
                    pp.BlockerPerformance.TotalPlayerValue += bp.BlockerPerformance.TotalPlayerValue;

                    pp.JammerPerformance.TotalJamPortions += bp.JammerPerformance.TotalJamPortions;
                    pp.JammerPerformance.TotalPenalties += bp.JammerPerformance.TotalPenalties;
                    pp.JammerPerformance.TotalPenaltyCost += bp.JammerPerformance.TotalPenaltyCost;
                    pp.JammerPerformance.TotalPointsVersusMedian += bp.JammerPerformance.TotalPointsVersusMedian;
                    pp.JammerPerformance.TotalPlayerValue += bp.JammerPerformance.TotalPlayerValue;
                }
            }

            return ppMap.Values.ToList();
        }

        public IList<Dictionary<int, PlayerPerformance>> GetAllPlayerPointPerformances(int numberOfIterations)
        {
            // pull data
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            var players = new PlayerGateway(connection, transaction).GetAllPlayers().GroupBy(p => p.ID).ToDictionary(g => g.Key, g => g.ToDictionary(g2 => g2.TeamID));
            var jams = new JamGateway(connection, transaction).GetAllJams().OrderBy(j => j.ID);
            var jamBoutMap = jams.ToDictionary(j => j.ID, j => j.BoutID);
            var jpe = new JamPlayerEffectivenessGateway(connection, transaction).GetAllJamPlayerEffectiveness();
            var jamData = new JamDataGateway(connection, transaction).GetJamTeamDataForYear(STATS_START_DATE.Year)
                .GroupBy(jd => jd.JamID)
                .ToDictionary(g => g.Key, g => g.ToDictionary(g2 => g2.TeamID));
            var teams = new TeamGateway(connection, transaction).GetAllTeams().ToDictionary(t => t.ID);
            var bouts = new BoutGateway(connection, transaction).GetBouts().ToDictionary(t => t.ID);
            var penalties = new PenaltyGateway(connection, transaction).GetAllPenalties()
                .GroupBy(p => p.JamID)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(g2 => g2.PlayerID).ToDictionary(g3 => g3.Key, g3 => g3.ToList()));
            var pgs = new PenaltyGroupGateway(connection, transaction).GetAllPenaltyGroups();
            Dictionary<int, int> boxTimeEstimates = new BoxTimeEstimateGateway(connection, transaction).GetAllBoxTimeEstimates();
            Dictionary<FoulComparison, Dictionary<int, float>> sss = new SituationalScoreGateway(connection, transaction).GetAllSituationalScores();
            AveragePenaltyCostPerJam avgPenCost = new AveragePenaltyCostGateway(connection, transaction).GetAveragePenaltyCost();
            Dictionary<int, Dictionary<int, JamPlayer>> jamPlayerMap = new JamPlayerGateway(connection, transaction).GetJamPlayers()
                .GroupBy(jp => jp.JamID)
                .ToDictionary(g => g.Key, g => g.ToDictionary(g2 => g2.PlayerID));
            transaction.Commit();
            connection.Close();

            Dictionary<FoulComparison, float> medians = CalculateMedianScores(sss);
            PenaltyCostCalculator ppcCalc = new PenaltyCostCalculator(_connectionString);
            Dictionary<int, double> groupPenaltyCostMap = PenaltyCostCalculator.CalculatePointCosts(jamData, jamPlayerMap, pgs, boxTimeEstimates, sss);
            Dictionary<int, double> jamTotalPortionMap = new Dictionary<int, double>(300);

            var previousResult = GenerateInitialPlayerPerformance(players, jams, jpe, jamData, teams, bouts, penalties, pgs, avgPenCost, medians, groupPenaltyCostMap, jamTotalPortionMap);

            List<Dictionary<int, PlayerPerformance>> result = new List<Dictionary<int, PlayerPerformance>>(numberOfIterations);
            result.Add(previousResult);
            int i = 0;
            while(i < numberOfIterations)
            {
                previousResult = GeneratePlayerPerformanceIteration(previousResult, jams, jamData, jamTotalPortionMap, jpe, avgPenCost);
                result.Add(previousResult);
                i++;
            }

            return result;
        }

        private static Dictionary<int, PlayerPerformance> GeneratePlayerPerformanceIteration(Dictionary<int, PlayerPerformance> previousIteration,
                                                                                  IOrderedEnumerable<Jam> jams,
                                                                                  Dictionary<int, Dictionary<int, JamTeamData>> jamData,
                                                                                  Dictionary<int, double> jamTotalPortionMap,
                                                                                  Dictionary<int, Dictionary<int, JamPlayerEffectiveness>> jpe,
                                                                                  AveragePenaltyCostPerJam avgPenCost)
        {
            Dictionary<int, PlayerPerformance> pps = new Dictionary<int, PlayerPerformance>();
            foreach(Jam jam in jams)
            {
                var pe = jpe[jam.ID];
                var grouped = pe.Values.GroupBy(p => p.TeamID);
                var team1Players = grouped.First().Select(p => previousIteration[p.PlayerID]);
                var team2Players = grouped.Last().Select(p => previousIteration[p.PlayerID]);
                IEnumerable<JamPlayerEffectiveness> jammers = pe.Values.Where(eff => eff.IsJammer);
                double team1Score = CalculateExpectedScore(team1Players, jammers, jam.BoutID);
                int team1ID = grouped.First().Key;
                double team2Score = CalculateExpectedScore(team2Players, jammers, jam.BoutID);
                
                foreach(var eff in pe.Values)
                {
                    var previousPerformance = previousIteration[eff.PlayerID];
                    // get/set PlayerPerformance object
                    if (!pps.ContainsKey(eff.PlayerID))
                    {
                        pps[eff.PlayerID] = new PlayerPerformance
                        {
                            Player = previousPerformance.Player,
                            Bouts = new List<BoutPerformance>()
                        };
                    }
                    PlayerPerformance curPP = pps[eff.PlayerID];

                    // get/set BoutPerformance object
                    var prevBout = previousPerformance.Bouts.First(b => b.BoutID == jam.BoutID);
                    BoutPerformance bp = null;
                    if (!curPP.Bouts.Any() ||
                        curPP.Bouts.Last().BoutID != prevBout.BoutID )
                    {
                        bp = new BoutPerformance
                        {
                            BoutID = prevBout.BoutID,
                            AwayTeamName = prevBout.AwayTeamName,
                            HomeTeamName = prevBout.HomeTeamName,
                            BoutDate = prevBout.BoutDate,
                            Jams = new List<JamPerformance>()
                        };
                        if (prevBout.BlockerPerformance != null)
                        {
                            bp.BlockerPerformance = new RolledUpPerformanceData
                            {
                                TotalJamPortions = prevBout.BlockerPerformance.TotalJamPortions,
                                TotalPenalties = prevBout.BlockerPerformance.TotalPenalties,
                                TotalPenaltyCost = prevBout.BlockerPerformance.TotalPenaltyCost
                            };
                        }
                        if(prevBout.JammerPerformance != null)
                        {
                            bp.JammerPerformance = new RolledUpPerformanceData
                            {
                                TotalJamPortions = prevBout.JammerPerformance.TotalJamPortions,
                                TotalPenalties = prevBout.JammerPerformance.TotalPenalties,
                                TotalPenaltyCost = prevBout.JammerPerformance.TotalPenaltyCost
                            };
                        }
                        curPP.Bouts.Add(bp);
                    }
                    else
                    {
                        bp = curPP.Bouts.Last();
                    }

                    JamTeamData jd = jamData[jam.ID][eff.TeamID];
                    JamPerformance prevJamPerformance = prevBout.Jams.First(j => j.JamID == jam.ID);
                    double pointDifferential = 0;
                    var playerPerf = prevJamPerformance.JammerJamPercentage > 0 ? previousPerformance.JammerPerformance : previousPerformance.BlockerPerformance;
                    double playerScore = playerPerf.TotalPointsVersusMedian / playerPerf.TotalJamPortions;
                    if(curPP.Player.TeamID == team1ID)
                    {
                        pointDifferential = jd.PointDelta - team1Score + team2Score + playerScore;
                    }
                    else
                    {
                        pointDifferential = jd.PointDelta - team2Score + team1Score + playerScore;
                    }
                    JamPerformance jp = new JamPerformance
                    {
                        BlockerJamPercentage = prevJamPerformance.BlockerJamPercentage,
                        DeltaPercentile = prevJamPerformance.DeltaPercentile,
                        IsFirstHalf = prevJamPerformance.IsFirstHalf,
                        JamID = prevJamPerformance.JamID,
                        JammerJamPercentage = prevJamPerformance.JammerJamPercentage,
                        JamNumber = prevJamPerformance.JamNumber,
                        JamPenalties = prevJamPerformance.JamPenalties,
                        MedianDelta = prevJamPerformance.MedianDelta,
                        PenaltyCost = prevJamPerformance.PenaltyCost,
                        PointDelta = jd.PointDelta,
                        DeltaPortionVersusMedian = pointDifferential
                        //PlayerValue
                    };
                    bp.Jams.Add(jp);
                }
            }

            Parallel.ForEach(pps.Values, pp =>
            {
                RollUpIteratedPlayerPerformance(avgPenCost, jamTotalPortionMap, pp);
            });

            return pps;
        }

        private static double CalculateExpectedScore(IEnumerable<PlayerPerformance> teamPlayers, IEnumerable<JamPlayerEffectiveness> jammers, int boutID)
        {
            double result = 0;
            var jammerID =
                (from tp in teamPlayers
                join j in jammers on tp.Player.ID equals j.PlayerID
                select j.PlayerID).First();
            var jamID = jammers.First().JamID;
            foreach(PlayerPerformance pp in teamPlayers)
            {
                var jamPerf = pp.Bouts.First(b => b.BoutID == boutID).Jams.First(j => j.JamID == jamID);
                if(pp.Player.ID == jammerID)
                {
                    result += jamPerf.JammerJamPercentage * pp.JammerPerformance.TotalPointsVersusMedian / pp.JammerPerformance.TotalJamPortions;
                }
                else
                {
                    result += jamPerf.BlockerJamPercentage * pp.BlockerPerformance.TotalPointsVersusMedian / pp.BlockerPerformance.TotalJamPortions;
                }
            }
            return result;
        }

        private static Dictionary<int, PlayerPerformance> GenerateInitialPlayerPerformance(Dictionary<int, Dictionary<int, Player>> players, 
                                                                                IOrderedEnumerable<Jam> jams, 
                                                                                Dictionary<int, Dictionary<int, JamPlayerEffectiveness>> jpe, 
                                                                                Dictionary<int, Dictionary<int, JamTeamData>> jamData, 
                                                                                Dictionary<int, Team> teams, 
                                                                                Dictionary<int, Bout> bouts, 
                                                                                Dictionary<int, Dictionary<int, List<Penalty>>> penalties, 
                                                                                IList<PenaltyGroup> pgs, 
                                                                                AveragePenaltyCostPerJam avgPenCost, 
                                                                                Dictionary<FoulComparison, float> medians, 
                                                                                Dictionary<int, double> groupPenaltyCostMap, 
                                                                                Dictionary<int, double> jamTotalPortionMap)
        {
            Dictionary<int, PlayerPerformance> pps = new Dictionary<int, PlayerPerformance>();
            foreach (Jam jam in jams)
            {
                var pe = jpe[jam.ID];
                foreach(var eff in pe.Values)
                {
                    // get/set PlayerPerformance object
                    if (!pps.ContainsKey(eff.PlayerID))
                    {
                        pps[eff.PlayerID] = new PlayerPerformance
                        {
                            Player = players[eff.PlayerID][eff.TeamID],
                            Bouts = new List<BoutPerformance>()
                        };
                    }
                    PlayerPerformance curPP = pps[eff.PlayerID];

                    // get/set BoutPerformance object
                    Bout bout = bouts[jam.BoutID];
                    BoutPerformance bp = null;
                    if (!curPP.Bouts.Any() ||
                        curPP.Bouts.Last().BoutDate != bout.BoutDate ||
                        curPP.Bouts.Last().HomeTeamName != teams[bout.HomeTeamID].Name ||
                        curPP.Bouts.Last().AwayTeamName != teams[bout.AwayTeamID].Name)
                    {
                        bp = new BoutPerformance
                        {
                            BoutID = bout.ID,
                            AwayTeamName = teams[bout.AwayTeamID].Name,
                            HomeTeamName = teams[bout.HomeTeamID].Name,
                            BoutDate = bout.BoutDate,
                            Jams = new List<JamPerformance>()
                        };
                        curPP.Bouts.Add(bp);
                    }
                    else
                    {
                        bp = curPP.Bouts.Last();
                    }

                    JamTeamData jd = jamData[jam.ID][eff.TeamID];
                    int penaltyCount = penalties.ContainsKey(jam.ID) && penalties[jam.ID].ContainsKey(eff.PlayerID) ? penalties[jam.ID][eff.PlayerID].Count() : 0;
                    JamPerformance jp = new JamPerformance
                    {
                        BlockerJamPercentage = eff.IsJammer ? 0 : eff.JamPortion,
                        DeltaPercentile = eff.BaseQuality,
                        IsFirstHalf = jam.IsFirstHalf,
                        JamID = jam.ID,
                        JammerJamPercentage = eff.IsJammer ? eff.JamPortion : 0,
                        JamNumber = jam.JamNumber,
                        JamPenalties = penaltyCount,
                        MedianDelta = medians[jd.FoulComparison],
                        PenaltyCost = 0,
                        PointDelta = jd.PointDelta
                    };
                    if (jamTotalPortionMap.ContainsKey(jam.ID))
                    {
                        jamTotalPortionMap[jam.ID] += eff.IsJammer ? eff.JamPortion * 4 : eff.JamPortion;
                    }
                    else
                    {
                        jamTotalPortionMap[jam.ID] = eff.IsJammer ? eff.JamPortion * 4 : eff.JamPortion;
                    }
                    bp.Jams.Add(jp);
                };
            }

            foreach (PenaltyGroup pg in pgs)
            {
                foreach (Penalty p in pg.Penalties)
                {
                    JamPerformance jp = pps[p.PlayerID].Bouts.SelectMany(b => b.Jams).Where(j => j.JamID == p.JamID).First();
                    jp.PenaltyCost += groupPenaltyCostMap[pg.GroupID];
                }
            }

            Parallel.ForEach(pps.Values, pp =>
            {
                RollUpPlayerPerformance(avgPenCost, jamTotalPortionMap, pp);
            });

            return pps;
        }

        private static void RollUpPlayerPerformance(AveragePenaltyCostPerJam avgPenCost, Dictionary<int, double> jamTotalPortionMap, PlayerPerformance pp)
        {
            pp.BlockerPerformance = new RolledUpPerformanceData();
            pp.JammerPerformance = new RolledUpPerformanceData();
            foreach (BoutPerformance bp in pp.Bouts)
            {
                double jammerShare = bp.BoutDate.Year == 2019 ? (12.0 / 7.0) : 4.0;
                bp.BlockerPerformance = new RolledUpPerformanceData();
                bp.JammerPerformance = new RolledUpPerformanceData();
                foreach (JamPerformance jp in bp.Jams)
                {
                    double averagePenaltyCost = jp.JammerJamPercentage > 0 ? avgPenCost.JammerPointCost : avgPenCost.BlockerPointCost;
                    double jamShare = (jp.JammerJamPercentage * jammerShare + jp.BlockerJamPercentage) / jamTotalPortionMap[jp.JamID];
                    jp.DeltaPortionVersusMedian = (jp.PointDelta - jp.MedianDelta) * jamShare;
                    jp.PlayerValue = jp.DeltaPortionVersusMedian - jp.PenaltyCost + averagePenaltyCost;
                    var rollUp = jp.JammerJamPercentage > 0 ? bp.JammerPerformance : bp.BlockerPerformance;
                    rollUp.TotalJamPortions += jp.JammerJamPercentage + jp.BlockerJamPercentage;
                    rollUp.TotalPenalties += jp.JamPenalties;
                    rollUp.TotalPenaltyCost += jp.PenaltyCost;
                    rollUp.TotalPointsVersusMedian += jp.DeltaPortionVersusMedian;
                    rollUp.TotalPlayerValue += jp.PlayerValue;

                }

                pp.BlockerPerformance.TotalJamPortions += bp.BlockerPerformance.TotalJamPortions;
                pp.BlockerPerformance.TotalPenalties += bp.BlockerPerformance.TotalPenalties;
                pp.BlockerPerformance.TotalPenaltyCost += bp.BlockerPerformance.TotalPenaltyCost;
                pp.BlockerPerformance.TotalPointsVersusMedian += bp.BlockerPerformance.TotalPointsVersusMedian;
                pp.BlockerPerformance.TotalPlayerValue += bp.BlockerPerformance.TotalPlayerValue;

                pp.JammerPerformance.TotalJamPortions += bp.JammerPerformance.TotalJamPortions;
                pp.JammerPerformance.TotalPenalties += bp.JammerPerformance.TotalPenalties;
                pp.JammerPerformance.TotalPenaltyCost += bp.JammerPerformance.TotalPenaltyCost;
                pp.JammerPerformance.TotalPointsVersusMedian += bp.JammerPerformance.TotalPointsVersusMedian;
                pp.JammerPerformance.TotalPlayerValue += bp.JammerPerformance.TotalPlayerValue;
            }
        }

        private static void RollUpIteratedPlayerPerformance(AveragePenaltyCostPerJam avgPenCost, Dictionary<int, double> jamTotalPortionMap, PlayerPerformance pp)
        {
            pp.BlockerPerformance = new RolledUpPerformanceData();
            pp.JammerPerformance = new RolledUpPerformanceData();
            foreach (BoutPerformance bp in pp.Bouts)
            {
                bp.BlockerPerformance = new RolledUpPerformanceData();
                bp.JammerPerformance = new RolledUpPerformanceData();
                foreach (JamPerformance jp in bp.Jams)
                {
                    double averagePenaltyCost = jp.JammerJamPercentage > 0 ? avgPenCost.JammerPointCost : avgPenCost.BlockerPointCost;
                    jp.PlayerValue = jp.DeltaPortionVersusMedian - jp.PenaltyCost + averagePenaltyCost;
                    var rollUp = jp.JammerJamPercentage > 0 ? bp.JammerPerformance : bp.BlockerPerformance;
                    rollUp.TotalJamPortions += jp.JammerJamPercentage + jp.BlockerJamPercentage;
                    rollUp.TotalPenalties += jp.JamPenalties;
                    rollUp.TotalPenaltyCost += jp.PenaltyCost;
                    rollUp.TotalPointsVersusMedian += jp.DeltaPortionVersusMedian;
                    rollUp.TotalPlayerValue += jp.PlayerValue;
                }

                pp.BlockerPerformance.TotalJamPortions += bp.BlockerPerformance.TotalJamPortions;
                pp.BlockerPerformance.TotalPenalties += bp.BlockerPerformance.TotalPenalties;
                pp.BlockerPerformance.TotalPenaltyCost += bp.BlockerPerformance.TotalPenaltyCost;
                pp.BlockerPerformance.TotalPointsVersusMedian += bp.BlockerPerformance.TotalPointsVersusMedian;
                pp.BlockerPerformance.TotalPlayerValue += bp.BlockerPerformance.TotalPlayerValue;

                pp.JammerPerformance.TotalJamPortions += bp.JammerPerformance.TotalJamPortions;
                pp.JammerPerformance.TotalPenalties += bp.JammerPerformance.TotalPenalties;
                pp.JammerPerformance.TotalPenaltyCost += bp.JammerPerformance.TotalPenaltyCost;
                pp.JammerPerformance.TotalPointsVersusMedian += bp.JammerPerformance.TotalPointsVersusMedian;
                pp.JammerPerformance.TotalPlayerValue += bp.JammerPerformance.TotalPlayerValue;
            }
        }

        private void CalculateTeamAverages(Dictionary<int, PlayerPerformance> pps, Dictionary<int, Bout> bouts)
        {
            Dictionary<int, List<RolledUpPerformanceData>> boutJammerPerformanceMap, boutBlockerPerformanceMap;
            boutJammerPerformanceMap = new Dictionary<int, List<RolledUpPerformanceData>>();
            boutBlockerPerformanceMap = new Dictionary<int, List<RolledUpPerformanceData>>();
            // for each bout, populate all of the performance rollups
            foreach(PlayerPerformance pp in pps.Values)
            {
                foreach(BoutPerformance bp in pp.Bouts)
                {
                    if(bp.JammerPerformance.TotalJamPortions > 0)
                    {
                        if(!boutJammerPerformanceMap.ContainsKey(bp.BoutID))
                        {
                            boutJammerPerformanceMap[bp.BoutID] = new List<RolledUpPerformanceData>();
                        }
                        boutJammerPerformanceMap[bp.BoutID].Add(bp.JammerPerformance);
                    }
                    if(bp.BlockerPerformance.TotalJamPortions > 0)
                    {
                        if (!boutBlockerPerformanceMap.ContainsKey(bp.BoutID))
                        {
                            boutBlockerPerformanceMap[bp.BoutID] = new List<RolledUpPerformanceData>();
                        }
                        boutBlockerPerformanceMap[bp.BoutID].Add(bp.BlockerPerformance);
                    }
                }
            }
            foreach(List<RolledUpPerformanceData> boutRollups in boutJammerPerformanceMap.Values)
            {
                double jams = boutRollups.Sum(br => br.TotalJamPortions);
                double totalPVM = boutRollups.Sum(br => br.TotalPointsVersusMedian);
                double totalValue = boutRollups.Sum(br => br.TotalPlayerValue);
                double averagePVM = totalPVM / jams;
                double averageValue = totalValue / jams;
                foreach(RolledUpPerformanceData rollup in boutRollups)
                {
                    rollup.PlayerValueVersusTeamAverage = rollup.TotalPlayerValue - (averageValue * rollup.TotalJamPortions);
                }
            }
            foreach (List<RolledUpPerformanceData> boutRollups in boutBlockerPerformanceMap.Values)
            {
                double jams = boutRollups.Sum(br => br.TotalJamPortions);
                double totalPVM = boutRollups.Sum(br => br.TotalPointsVersusMedian);
                double totalValue = boutRollups.Sum(br => br.TotalPlayerValue);
                double averagePVM = totalPVM / jams;
                double averageValue = totalValue / jams;
                foreach (RolledUpPerformanceData rollup in boutRollups)
                {
                    rollup.PlayerValueVersusTeamAverage = rollup.TotalPlayerValue - (averageValue * rollup.TotalJamPortions);
                }
            }
        }

        private Dictionary<FoulComparison, float> CalculateMedianScores(Dictionary<FoulComparison, Dictionary<int, float>> sss)
        {
            Dictionary<FoulComparison, float> result = new Dictionary<FoulComparison, float>();
            foreach(KeyValuePair<FoulComparison, Dictionary<int, float>> kvp in sss)
            {
                result[kvp.Key] = GetScoreForPercentile(kvp.Value, 0.5f);                
            }
            return result;
        }

        private float GetScoreForPercentile(Dictionary<int, float> scorePercentiles, float percentile)
        {
            var kvp_exact = scorePercentiles.Where(dic => dic.Value == percentile);
            if (kvp_exact.Any())
            {
                return kvp_exact.First().Key;
            }
            else
            {
                float underValue = scorePercentiles.Where(dic => dic.Value < 0.5).Select(dic => dic.Value).Max();
                int underPoints = scorePercentiles.Where(dic => dic.Value == underValue).First().Key;
                float overValue = scorePercentiles.Where(dic => dic.Value > 0.5).Select(dic => dic.Value).Min();
                int overPoints = scorePercentiles.Where(dic => dic.Value == overValue).First().Key;
                float diff50 = percentile - underValue;
                float totalDiff = overValue - underValue;
                float ratio = diff50 / totalDiff;
                return underPoints + (ratio * (overPoints - underPoints));
            }
        }

    }
}
