using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class SituationalScoreGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_DeleteSituationalScore = "DELETE FROM SituationalScore\n";
        private const string s_DeleteSituationalScoreByYear = "DELETE FROM SituationalScore WHERE Year = {0}";
        private const string s_InsertSituationalScoreBase = "INSERT INTO SituationalScore VALUES";
        private const string s_InsertSituationalScoreParameter = "\n({0}, {1}, {2}, {3}, {4}),";
        private const string s_GetAllSituationalScoresQuery = "SELECT * FROM SituationalScore";
        #endregion

        public SituationalScoreGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public void InsertSituationalScores(Dictionary<int, Dictionary<FoulComparison, Dictionary<int, float>>> sss)
        {
            int counter = 0;
            string query = s_DeleteSituationalScore + s_InsertSituationalScoreBase;
            foreach (KeyValuePair<int, Dictionary<FoulComparison, Dictionary<int, float>>> ss in sss)
            {
                foreach (KeyValuePair<FoulComparison, Dictionary<int, float>> annualKvp in ss.Value)
                {
                    foreach (KeyValuePair<int, float> kvp in annualKvp.Value)
                    {
                        query += String.Format(s_InsertSituationalScoreParameter,
                                               ss.Key,
                                               annualKvp.Key.JammerBoxComparison,
                                               annualKvp.Key.BlockerBoxComparison,
                                               kvp.Key,
                                               kvp.Value);
                        counter++;
                        if (counter > 990)
                        {
                            // to avoid ever hitting the 1000 limit, run now and start a new query for the rest
                            using (var cmd = new SqlCommand(query.TrimEnd(','), _connection, _transaction))
                            {
                                cmd.Parameters.Clear();
                                cmd.ExecuteNonQuery();
                            }
                            query = s_InsertSituationalScoreBase;
                            counter = 0;
                        }
                    }
                }
            }
            if (counter > 0)
            {
                query = query.TrimEnd(',');
                using (var cmd = new SqlCommand(query, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertSituationalScores(Dictionary<FoulComparison, Dictionary<int, float>> sss)
        {
            int counter = 0;
            string query = s_DeleteSituationalScore + s_InsertSituationalScoreBase;
            foreach (KeyValuePair<FoulComparison, Dictionary<int, float>> ss in sss)
            {
                foreach (KeyValuePair<int, float> kvp in ss.Value)
                {
                    query += String.Format(s_InsertSituationalScoreParameter,
                                           ss.Key.Year,
                                           ss.Key.JammerBoxComparison,
                                           ss.Key.BlockerBoxComparison,
                                           kvp.Key,
                                           kvp.Value);
                    counter++;
                    if (counter > 990)
                    {
                        // to avoid ever hitting the 1000 limit, run now and start a new query for the rest
                        using (var cmd = new SqlCommand(query.TrimEnd(','), _connection, _transaction))
                        {
                            cmd.Parameters.Clear();
                            cmd.ExecuteNonQuery();
                        }
                        query = s_InsertSituationalScoreBase;
                        counter = 0;
                    }
                }
            }
            if (counter > 0)
            {
                query = query.TrimEnd(',');
                using (var cmd = new SqlCommand(query, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertSituationalScoresForYear(int year, Dictionary<FoulComparison, Dictionary<int, float>> sss)
        {
            int counter = 0;
            string query = String.Format(s_DeleteSituationalScore, year);
            using (var cmd = new SqlCommand(query, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.ExecuteNonQuery();
            }
            query = s_InsertSituationalScoreBase;
            foreach (KeyValuePair<FoulComparison, Dictionary<int, float>> ss in sss)
            {
                foreach (KeyValuePair<int, float> kvp in ss.Value)
                {
                    query += String.Format(s_InsertSituationalScoreParameter,
                                           ss.Key.Year,
                                           ss.Key.JammerBoxComparison,
                                           ss.Key.BlockerBoxComparison,
                                           kvp.Key,
                                           kvp.Value);
                    counter++;
                    if (counter > 990)
                    {
                        // to avoid ever hitting the 1000 limit, run now and start a new query for the rest
                        using (var cmd = new SqlCommand(query.TrimEnd(','), _connection, _transaction))
                        {
                            cmd.Parameters.Clear();
                            cmd.ExecuteNonQuery();
                        }
                        query = s_InsertSituationalScoreBase;
                        counter = 0;
                    }
                }
            }
            if (counter > 0)
            {
                query = query.TrimEnd(',');
                using (var cmd = new SqlCommand(query, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Dictionary<FoulComparison, Dictionary<int, float>> GetAllSituationalScores()
        {
            Dictionary<FoulComparison, Dictionary<int, float>> data = new Dictionary<FoulComparison,Dictionary<int,float>>();
            using (var cmd = new SqlCommand(s_GetAllSituationalScoresQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    while (reader.Read())
                    {
                        double jammerDiff = reader.GetDouble(reader.GetOrdinal("JammerBoxComparison"));
                        double blockerDiff = reader.GetDouble(reader.GetOrdinal("BlockerBoxComparison"));
                        int pointDelta = reader.GetInt32(reader.GetOrdinal("PointDelta"));
                        double percentile = reader.GetDouble(reader.GetOrdinal("Percentile"));
                        int year = reader.GetInt32(reader.GetOrdinal("Year"));
                        FoulComparison fc = new FoulComparison
                        {
                            Year = year,
                            BlockerBoxComparison = blockerDiff,
                            JammerBoxComparison = jammerDiff
                        };
                        if(!data.ContainsKey(fc))
                        {
                            data[fc] = new Dictionary<int, float>();
                        }
                        data[fc][pointDelta] = (float)percentile;
                    }
                    reader.Close();
                }
            }
            return data;
        }
    }
}
