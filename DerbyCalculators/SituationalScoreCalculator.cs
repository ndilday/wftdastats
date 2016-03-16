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
        private IList<JamTeamData> _jamData = null;

        public SituationalScoreCalculator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SituationalScoreCalculator(string connectionString, IList<JamTeamData> jamTeamData) : this(connectionString)
        {
            _jamData = jamTeamData;
        }

        public Dictionary<FoulComparison, Dictionary<int, float>> CalculateSituationalScores(out IList<JamTeamData> jamData)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            jamData = null;

            if (_jamData == null)
            {
                JamDataGateway gateway = new JamDataGateway(connection, transaction);
                _jamData = gateway.GetAllJamData();
                jamData = _jamData;
            }

            Dictionary<FoulComparison, SortedList<int, int>> bigMap = CreateBigMap();
            Dictionary<FoulComparison, Dictionary<int, float>> sss = CreateSituationalScores(bigMap);
            new SituationalScoreGateway(connection, transaction).InsertSituationalScores(sss);
            transaction.Commit();
            connection.Close();
            return sss;
        }

        private Dictionary<FoulComparison, SortedList<int, int>> CreateBigMap()
        {
            Dictionary<FoulComparison, SortedList<int, int>> bigMap = new Dictionary<FoulComparison, SortedList<int, int>>();
            foreach (JamTeamData jamData in _jamData)
            {
                if (!bigMap.ContainsKey(jamData.FoulComparison))
                {
                    bigMap[jamData.FoulComparison] = new SortedList<int, int>();
                }
                SortedList<int, int> innerList = bigMap[jamData.FoulComparison];
                if (!innerList.ContainsKey(jamData.PointDelta))
                {
                    innerList[jamData.PointDelta] = 0;
                }
                innerList[jamData.PointDelta]++;
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
