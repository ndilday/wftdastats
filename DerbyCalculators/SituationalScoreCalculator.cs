using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    public class SituationalScoreCalculator
    {
        private string _connectionString;
        private IList<JamTeamData> _jamTeamData = null;

        public SituationalScoreCalculator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SituationalScoreCalculator(string connectionString, IList<JamTeamData> jamTeamData) : this(connectionString)
        {
            _jamTeamData = jamTeamData;
        }

        public Dictionary<FoulComparison, Dictionary<int, float>> CalculateSituationalScores(int year, out IList<JamTeamData> jamTeamData, out Dictionary<int, JamData> jamDataMap)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            jamTeamData = null;

            JamDataGateway gateway = new JamDataGateway(connection, transaction);
            jamDataMap = gateway.GetAllJamData().ToDictionary(m => m.JamID, m => m);
            if (_jamTeamData == null)
            {
                
                _jamTeamData = gateway.GetJamTeamDataForYear(year);
                jamTeamData = _jamTeamData;
            }

            Dictionary<FoulComparison, SortedList<int, int>> bigMap = CreateBigMap(jamDataMap);
            Dictionary<FoulComparison, Dictionary<int, float>> sss = CreateSituationalScores(bigMap);
            new SituationalScoreGateway(connection, transaction).InsertSituationalScoresForYear(year, sss);
            transaction.Commit();
            connection.Close();
            return sss;
        }

        private Dictionary<FoulComparison, SortedList<int, int>> CreateBigMap(Dictionary<int, JamData> jamDataMap)
        {
            Dictionary<FoulComparison, SortedList<int, int>> bigMap = new Dictionary<FoulComparison, SortedList<int, int>>();
            foreach (JamTeamData jamTeamData in _jamTeamData)
            {
                jamTeamData.Year = jamDataMap[jamTeamData.JamID].PlayDate.Year;
                /*if (jamDataMap[jamTeamData.JamID].HomeTeamType != 1 || jamDataMap[jamTeamData.JamID].AwayTeamType != 1)
                {
                    continue;
                }*/

                if (!bigMap.ContainsKey(jamTeamData.FoulComparison))
                {
                    
                    bigMap[jamTeamData.FoulComparison] = new SortedList<int, int>();
                }

                SortedList<int, int> innerList = bigMap[jamTeamData.FoulComparison];

                if (!innerList.ContainsKey(jamTeamData.PointDelta))
                {
                    innerList[jamTeamData.PointDelta] = 0;
                }

                innerList[jamTeamData.PointDelta]++;
            }
            return bigMap;
        }

        private Dictionary<FoulComparison, Dictionary<int, float>> CreateSituationalScores(Dictionary<FoulComparison, SortedList<int, int>> bigMap)
        {
            var sss = new Dictionary<FoulComparison, Dictionary<int, float>>();
            foreach (KeyValuePair<FoulComparison, SortedList<int, int>> kvp in bigMap)
            {
                sss[kvp.Key] = new Dictionary<int, float>();
                int runningTotal = 0;
                int total = kvp.Value.Values.Sum();
                foreach (KeyValuePair<int, int> ii in kvp.Value)
                {
                    float half = ii.Value / 2.0f;
                    sss[kvp.Key][ii.Key] = (runningTotal + half) / (float)(total);
                    runningTotal += ii.Value;
                }
            }
            return sss;
        }
    }
}
