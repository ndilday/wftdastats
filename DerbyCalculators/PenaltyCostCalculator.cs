using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using DerbyCalculators.Models;
using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    class PenaltyCostCalculator
    {
        string _connectionString;
        public PenaltyCostCalculator(string connString)
        {
            _connectionString = connString;
        }

        //public Dictionary<int, double> GetPenaltyPointCosts()
        //{
        //    // pull data
        //    SqlConnection connection = new SqlConnection(_connectionString);
        //    connection.Open();
        //    SqlTransaction transaction = connection.BeginTransaction();
        //    var jamData = new JamDataGateway(connection, transaction).GetAllJamData().ToDictionary(jd => jd.JamID);
        //    var pgs = new PenaltyGroupGateway(connection, transaction).GetPenaltyGroups();
        //    Dictionary<int, int> boxTimeEstimates = new BoxTimeEstimateGateway(connection, transaction).GetAllBoxTimeEstimates();
        //    Dictionary<FoulComparison, Dictionary<int, float>> sss = new SituationalScoreGateway(connection, transaction).GetAllSituationalScores();
        //    transaction.Commit();
        //    connection.Close();
        //    return CalculatePointCosts(jamData, pgs, boxTimeEstimates, sss);
        //}

        //public Dictionary<int, double> GetPenaltyValueCosts()
        //{
        //    // pull data
        //    SqlConnection connection = new SqlConnection(_connectionString);
        //    connection.Open();
        //    SqlTransaction transaction = connection.BeginTransaction();
        //    var jamData = new JamDataGateway(connection, transaction).GetAllJamData().ToDictionary(jd => jd.JamID);
        //    var pgs = new PenaltyGroupGateway(connection, transaction).GetPenaltyGroups();
        //    var jteMap = new JamTeamEffectivenessGateway(connection, transaction).GetAllJamTeamEffectiveness();
        //    Dictionary<int, int> boxTimeEstimates = new BoxTimeEstimateGateway(connection, transaction).GetAllBoxTimeEstimates();
        //    Dictionary<FoulComparison, Dictionary<int, float>> sss = new SituationalScoreGateway(connection, transaction).GetAllSituationalScores();
        //    transaction.Commit();
        //    connection.Close();
        //    return CalculateValueCosts(jamData, pgs, boxTimeEstimates, sss);
        //}

        public Dictionary<int, double> GetPenaltyPointCostsForTeam(int teamID)
        {
            // pull data
            SqlConnection connection = new SqlConnection(_connectionString);
            SqlTransaction transaction = connection.BeginTransaction();
            var jamData = new JamDataGateway(connection, transaction).GetJamDataForTeam(teamID).ToDictionary(jd => jd.JamID);
            var pgs = new PenaltyGroupGateway(connection, transaction).GetPenaltyGroupsForTeam(teamID);
            Dictionary<int, int> boxTimeEstimates = new BoxTimeEstimateGateway(connection, transaction).GetAllBoxTimeEstimates();
            Dictionary<FoulComparison, Dictionary<int, float>> sss = new SituationalScoreGateway(connection, transaction).GetAllSituationalScores();
            transaction.Commit();
            connection.Close();
            return CalculatePointCostsForTeam(jamData, pgs, boxTimeEstimates, sss);
        }

        public Dictionary<int, double> GetValueCostsForTeam(int teamID)
        {
            // pull data
            SqlConnection connection = new SqlConnection(_connectionString);
            SqlTransaction transaction = connection.BeginTransaction();
            var jamData = new JamDataGateway(connection, transaction).GetJamDataForTeam(teamID).ToDictionary(jd => jd.JamID);
            var pgs = new PenaltyGroupGateway(connection, transaction).GetPenaltyGroupsForTeam(teamID);
            Dictionary<int, int> boxTimeEstimates = new BoxTimeEstimateGateway(connection, transaction).GetAllBoxTimeEstimates();
            Dictionary<FoulComparison, Dictionary<int, float>> sss = new SituationalScoreGateway(connection, transaction).GetAllSituationalScores();
            Dictionary<int, double> jte = new JamTeamEffectivenessGateway(connection, transaction).GetJamTeamEffectivenessForTeam(teamID);
            transaction.Commit();
            connection.Close();
            return CalculateValueCostsForTeam(jamData, pgs, boxTimeEstimates, sss, jte);
        }

        internal Dictionary<int, double> CalculatePointCostsForTeam(Dictionary<int, JamTeamData> jamData, 
                                                   IList<PenaltyGroup> pgs, 
                                                   Dictionary<int, int> boxTimeEstimates, 
                                                   Dictionary<FoulComparison, Dictionary<int, float>> sss)
        {
            Dictionary<int, double> groupPenaltyCostMap = new Dictionary<int, double>();

            foreach (PenaltyGroup group in pgs)
            {
                double totalCost = 0;
                // calculate the cost of each box time in the group
                foreach (BoxTime boxTime in group.BoxTimes)
                {
                    int estimateTime = boxTimeEstimates[boxTime.BoxTimeID];
                    JamTeamData thisJamData = jamData[boxTime.JamID];
                    // determine how big the swing would be if there had been no box time served
                    double pointDiff = GetEstimatedPointsWithoutBoxTime(sss, thisJamData, boxTime.IsJammer) - thisJamData.PointDelta;

                    double totalPenaltyTime = boxTime.IsJammer ?
                        thisJamData.JammerBoxTime :
                        thisJamData.BlockerBoxTime;
                    // modify this by how much of the total penalty time this player contributed

                    // doing the rough estimate version for now
                    totalCost += pointDiff * boxTimeEstimates[boxTime.BoxTimeID] / totalPenaltyTime;
                }
                groupPenaltyCostMap[group.GroupID] = totalCost / group.Penalties.Count;
            }

            return groupPenaltyCostMap;
        }

        internal Dictionary<int, double> CalculateValueCostsForTeam(Dictionary<int, JamTeamData> jamData,
                                                   IList<PenaltyGroup> pgs,
                                                   Dictionary<int, int> boxTimeEstimates,
                                                   Dictionary<FoulComparison, Dictionary<int, float>> sss,
                                                   Dictionary<int, double> jamValueEstimates)
        {
            Dictionary<int, double> groupPenaltyCostMap = new Dictionary<int, double>();

            foreach (PenaltyGroup group in pgs)
            {
                double totalCost = 0;
                // calculate the cost of each box time in the group
                foreach (BoxTime boxTime in group.BoxTimes)
                {
                    int estimateTime = boxTimeEstimates[boxTime.BoxTimeID];
                    JamTeamData thisJamData = jamData[boxTime.JamID];
                    // determine how big the swing would be if there had been no box time served
                    double valueDiff = GetEstimatedValueWithoutBoxTime(sss, thisJamData, boxTime.IsJammer) - jamValueEstimates[boxTime.JamID];

                    double totalPenaltyTime = boxTime.IsJammer ?
                        thisJamData.JammerBoxTime :
                        thisJamData.BlockerBoxTime;
                    // modify this by how much of the total penalty time this player contributed

                    // doing the rough estimate version for now
                    totalCost += valueDiff * boxTimeEstimates[boxTime.BoxTimeID] / totalPenaltyTime;
                }
                groupPenaltyCostMap[group.GroupID] = totalCost / group.Penalties.Count;
            }

            return groupPenaltyCostMap;
        }

        internal static Dictionary<int, double> CalculatePointCosts(Dictionary<int, Dictionary<int, JamTeamData>> jamData,
                                                   Dictionary<int,Dictionary<int,JamPlayer>> jamPlayerMap,
                                                   IList<PenaltyGroup> pgs,
                                                   Dictionary<int, int> boxTimeEstimates,
                                                   Dictionary<FoulComparison, Dictionary<int, float>> sss)
        {
            Dictionary<int, double> groupPenaltyCostMap = new Dictionary<int, double>();

            foreach (PenaltyGroup group in pgs)
            {
                double totalCost = 0;
                int teamID = jamPlayerMap[group.Penalties[0].JamID][group.Penalties[0].PlayerID].TeamID;
                // calculate the cost of each box time in the group
                foreach (BoxTime boxTime in group.BoxTimes)
                {
                    if(!jamData.ContainsKey(boxTime.JamID))
                    {
                        continue;
                    }
                    int estimateTime = boxTimeEstimates[boxTime.BoxTimeID];
                    JamTeamData thisJamData = jamData[boxTime.JamID][teamID];
                    // determine how big the swing would be if there had been no box time served
                    double pointDiff = GetEstimatedPointsWithoutBoxTime(sss, thisJamData, boxTime.IsJammer) - thisJamData.PointDelta;

                    double totalPenaltyTime = boxTime.IsJammer ?
                        thisJamData.JammerBoxTime :
                        thisJamData.BlockerBoxTime;
                    // modify this by how much of the total penalty time this player contributed

                    // doing the rough estimate version for now
                    totalCost += pointDiff * boxTimeEstimates[boxTime.BoxTimeID] / totalPenaltyTime;
                }
                groupPenaltyCostMap[group.GroupID] = totalCost / group.Penalties.Count;
            }

            return groupPenaltyCostMap;
        }

        internal static Dictionary<int, double> CalculateValueCosts(Dictionary<int, Dictionary<int, JamTeamData>> jamData,
                                                   Dictionary<int, Dictionary<int, JamPlayer>> jamPlayerMap,
                                                   IList<PenaltyGroup> pgs,
                                                   Dictionary<int, int> boxTimeEstimates,
                                                   Dictionary<FoulComparison, Dictionary<int, float>> sss,
                                                   Dictionary<int, Dictionary<int, float>> jteMap)
        {
            Dictionary<int, double> groupPenaltyCostMap = new Dictionary<int, double>();

            foreach (PenaltyGroup group in pgs)
            {
                double totalCost = 0;
                int jamID = group.Penalties[0].JamID;
                if(!jteMap.ContainsKey(jamID))
                {
                    continue;
                }
                int teamID = jamPlayerMap[jamID][group.Penalties[0].PlayerID].TeamID;
                double percent = jteMap[jamID][teamID];
                // calculate the cost of each box time in the group
                foreach (BoxTime boxTime in group.BoxTimes)
                {
                    int estimateTime = boxTimeEstimates[boxTime.BoxTimeID];
                    JamTeamData thisJamData = jamData[boxTime.JamID][teamID];
                    // determine how big the swing would be if there had been no box time served
                    double valueDiff = GetEstimatedValueWithoutBoxTime(sss, thisJamData, boxTime.IsJammer) - percent;

                    double totalPenaltyTime = boxTime.IsJammer ?
                        thisJamData.JammerBoxTime :
                        thisJamData.BlockerBoxTime;
                    // modify this by how much of the total penalty time this player contributed

                    // doing the rough estimate version for now
                    totalCost += valueDiff * boxTimeEstimates[boxTime.BoxTimeID] / totalPenaltyTime;
                }
                groupPenaltyCostMap[group.GroupID] = totalCost / group.Penalties.Count;
            }

            return groupPenaltyCostMap;
        }

        internal static double GetEstimatedPointsWithoutBoxTime(Dictionary<FoulComparison, Dictionary<int, float>> sss, JamTeamData jamData, bool isJammer)
        {
            float basePercentile;
            if (!sss.ContainsKey(jamData.FoulComparison))
            {
                throw new InvalidOperationException("This is bad data");
            }
            else if (!sss[jamData.FoulComparison].ContainsKey(jamData.PointDelta))
            {
                basePercentile = GetPercentileForScore(sss[jamData.FoulComparison], jamData.PointDelta);
            }
            else
            {
                basePercentile = sss[jamData.FoulComparison][jamData.PointDelta];
            }

            // figure out the foul differential if this team did not commit fouls of this type this jam
            int jammerPenaltyDiff = (isJammer ? 0 : jamData.JammerBoxTime) - jamData.OppJammerBoxTime;
            int blockerPenaltyDiff = (isJammer ? jamData.BlockerBoxTime : 0) - jamData.OppBlockerBoxTime;
            double jammerBoxComp = Math.Round(jammerPenaltyDiff / 15.0, MidpointRounding.AwayFromZero) / 2.0;
            double blockerBoxComp = Math.Round(blockerPenaltyDiff / 15.0, MidpointRounding.AwayFromZero) / 2.0;
            FoulComparison foul = new FoulComparison
            {
                JammerBoxComparison = jammerBoxComp,
                BlockerBoxComparison = blockerBoxComp
            };

            if (!sss.ContainsKey(foul))
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
                double baseDelta = GetScoreForPercentile(sss[baseJamData.FoulComparison], basePercentile);

                // pull team 1 values
                baseJamData.BlockerBoxTime = isJammer ? jamData.BlockerBoxTime : 0;
                baseJamData.JammerBoxTime = isJammer ? 0 : jamData.JammerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0;
                }
                if(!sss.ContainsKey(baseJamData.FoulComparison))
                {
                    sss[baseJamData.FoulComparison] = new Dictionary<int, float>();
                    int pointDifferential = (int)((((baseJamData.OppBlockerBoxTime - baseJamData.BlockerBoxTime)*2.5)+((baseJamData.OppJammerBoxTime - baseJamData.JammerBoxTime)*10))/30.0);
                    sss[baseJamData.FoulComparison][pointDifferential] = 0.5f;
                }
                double score1 = GetScoreForPercentile(sss[baseJamData.FoulComparison], basePercentile);

                // pull team 2 blocker
                baseJamData.BlockerBoxTime = 0;
                baseJamData.JammerBoxTime = 0;
                baseJamData.OppBlockerBoxTime = jamData.OppBlockerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0;
                }
                if (!sss.ContainsKey(baseJamData.FoulComparison))
                {
                    sss[baseJamData.FoulComparison] = new Dictionary<int, float>();
                    int pointDifferential = (int)((((baseJamData.OppBlockerBoxTime - baseJamData.BlockerBoxTime) * 2.5) + ((baseJamData.OppJammerBoxTime - baseJamData.JammerBoxTime) * 10)) / 30.0);
                    sss[baseJamData.FoulComparison][pointDifferential] = 0.5f;
                }
                double score2 = GetScoreForPercentile(sss[baseJamData.FoulComparison], basePercentile);

                // pull team 2 jammer
                baseJamData.OppBlockerBoxTime = 0;
                baseJamData.OppJammerBoxTime = jamData.OppJammerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0;
                }
                if (!sss.ContainsKey(baseJamData.FoulComparison))
                {
                    sss[baseJamData.FoulComparison] = new Dictionary<int, float>();
                    int pointDifferential = (int)((((baseJamData.OppBlockerBoxTime - baseJamData.BlockerBoxTime) * 2.5) + ((baseJamData.OppJammerBoxTime - baseJamData.JammerBoxTime) * 10)) / 30.0);
                    sss[baseJamData.FoulComparison][pointDifferential] = 0.5f;
                }
                double score3 = GetScoreForPercentile(sss[baseJamData.FoulComparison], basePercentile);

                return score1 + score2 + score3 - 2 * baseDelta;
            }
            else
            {
                return GetScoreForPercentile(sss[foul], basePercentile);
            }
        }

        internal static double GetEstimatedValueWithoutBoxTime(Dictionary<FoulComparison, Dictionary<int, float>> sss, JamTeamData jamData, bool isJammer)
        {
            if (!sss.ContainsKey(jamData.FoulComparison))
            {
                throw new InvalidOperationException("This is bad data");
            }

            // figure out the foul differential if this team did not commit fouls of this type this jam
            int jammerPenaltyDiff = (isJammer ? 0 : jamData.JammerBoxTime) - jamData.OppJammerBoxTime;
            int blockerPenaltyDiff = (isJammer ? jamData.BlockerBoxTime : 0) - jamData.OppBlockerBoxTime;
            double jammerBoxComp = Math.Round(jammerPenaltyDiff / 15.0, MidpointRounding.AwayFromZero) / 2.0;
            double blockerBoxComp = Math.Round(blockerPenaltyDiff / 15.0, MidpointRounding.AwayFromZero) / 2.0;
            FoulComparison foul = new FoulComparison
            {
                JammerBoxComparison = jammerBoxComp,
                BlockerBoxComparison = blockerBoxComp
            };

            if (!sss.ContainsKey(foul))
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
                double baseDelta = GetPercentileForScore(sss[baseJamData.FoulComparison], jamData.PointDelta);

                // pull team 1 values
                baseJamData.BlockerBoxTime = isJammer ? jamData.BlockerBoxTime : 0;
                baseJamData.JammerBoxTime = isJammer ? 0 : jamData.JammerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0;
                }
                double score1 = GetPercentileForScore(sss[baseJamData.FoulComparison], jamData.PointDelta);

                // pull team 2 blocker
                baseJamData.BlockerBoxTime = 0;
                baseJamData.JammerBoxTime = 0;
                baseJamData.OppBlockerBoxTime = jamData.OppBlockerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0;
                }
                // special case, for now
                if (baseJamData.OppBlockerBoxTime > 135)
                {
                    // just treat it as 135 for now
                    baseJamData.OppBlockerBoxTime = 135;
                }
                if (baseJamData.OppBlockerBoxTime < -135)
                {
                    baseJamData.OppBlockerBoxTime = -135;
                }
                double score2 = GetPercentileForScore(sss[baseJamData.FoulComparison], jamData.PointDelta);

                // pull team 2 jammer
                baseJamData.OppBlockerBoxTime = 0;
                baseJamData.OppJammerBoxTime = jamData.OppJammerBoxTime;
                if (baseJamData.FoulComparison.Equals(foul))
                {
                    return 0;
                }
                double score3 = GetPercentileForScore(sss[baseJamData.FoulComparison], jamData.PointDelta);

                return score1 + score2 + score3 - 2 * baseDelta;
            }
            else
            {
                return GetPercentileForScore(sss[foul], jamData.PointDelta);
            }
        }

        internal static float GetPercentileForScore(Dictionary<int, float> scorePercentiles, int score)
        {
            var kvp_exact = scorePercentiles.Where(dic => dic.Key == score);
            if (kvp_exact.Any())
            {
                return kvp_exact.First().Value;
            }
            else
            {
                var under = scorePercentiles.Where(dic => dic.Key < score);
                float underValue;
                int underPoints;
                if (under.Any())
                {
                    underPoints = under.Select(dic => dic.Key).Max();
                    underValue = scorePercentiles[underPoints];
                }
                else
                {
                    underValue = 0;
                    underPoints = -36;
                }
                var over = scorePercentiles.Where(dic => dic.Key > score);
                float overValue;
                int overPoints;
                if (over.Any())
                {
                    overPoints = over.Select(dic => dic.Key).Min();
                    overValue = scorePercentiles[overPoints];
                }
                else
                {
                    overValue = 1;
                    overPoints = 36;
                }
                float diff = score - underPoints;
                float totalDiff = overPoints - underPoints;
                float ratio = diff / totalDiff;
                return underValue + (ratio * (overValue - underValue));
            }
        }

        internal static float GetScoreForPercentile(Dictionary<int, float> scorePercentiles, float percentile)
        {
            var kvp_exact = scorePercentiles.Where(dic => dic.Value == percentile);
            if (kvp_exact.Any())
            {
                return kvp_exact.First().Key;
            }
            else
            {
                var under = scorePercentiles.Where(dic => dic.Value < percentile);
                var over = scorePercentiles.Where(dic => dic.Value > percentile);
                float underValue = 0;
                float underPoints = 0;
                float overValue = 0;
                float overPoints = 0;
                if(!under.Any() || !over.Any())
                {
                    float median;
                    // at the ends of the distribution, we'll have to do a bit more work
                    if(scorePercentiles.Values.Count == 1)
                    {
                        median = scorePercentiles.Values.First();
                    }
                    else if(percentile == 0.5f)
                    {
                        throw new InvalidOperationException("What happened?");
                    }
                    else
                    {
                        median = GetScoreForPercentile(scorePercentiles, 0.5f);
                    }
                    if(!under.Any())
                    {
                        underPoints = median - 30;
                        underValue = 0;
                    }
                    else
                    {
                        underValue = under.Select(dic => dic.Value).Max();
                        underPoints = scorePercentiles.Where(dic => dic.Value == underValue).First().Key;
                    }
                    if(!over.Any())
                    {
                        overPoints = median + 30;
                        overValue = 1;
                    }
                    else
                    {
                        overValue = over.Select(dic => dic.Value).Min();
                        overPoints = scorePercentiles.Where(dic => dic.Value == overValue).First().Key;
                    }
                }
                else
                {
                    underValue = under.Select(dic => dic.Value).Max();
                    underPoints = scorePercentiles.Where(dic => dic.Value == underValue).First().Key;

                    overValue = over.Select(dic => dic.Value).Min();
                    overPoints = scorePercentiles.Where(dic => dic.Value == overValue).First().Key;
                }
                float diff = percentile - underValue;
                float totalDiff = overValue - underValue;
                float ratio = diff / totalDiff;
                return underPoints + (ratio * (overPoints - underPoints));
            }
        }
    }
}
