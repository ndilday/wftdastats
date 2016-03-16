using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class JamTimeLimitGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_DeleteJamTimeEstimate = "DELETE FROM JamTimeEstimate\n";
        private const string s_InsertJamTimeEstimateQueryBase = "INSERT INTO JamTimeEstimate VALUES\n";
        private const string s_InsertJamTimeEstimateParameter = "\n({0}, {1}, {2}, {3}),";
        private const string s_GetAllJamTimeEstimatesQuery = "SELECT * FROM JamTimeEstimate";
        #endregion

        public JamTimeLimitGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public void InsertJamTimeEstimates(IEnumerable<JamTimeEstimate> jamTimeEstimates)
        {
            int counter = 0;
            string query = s_DeleteJamTimeEstimate + s_InsertJamTimeEstimateQueryBase;
            foreach (JamTimeEstimate jamTimeEstimate in jamTimeEstimates)
            {
                query += String.Format(s_InsertJamTimeEstimateParameter,
                                        jamTimeEstimate.JamID,
                                        jamTimeEstimate.Estimate,
                                        jamTimeEstimate.Minimum,
                                        jamTimeEstimate.Maximum);
                counter++;
                if (counter > 990)
                {
                    // to avoid ever hitting the 1000 limit, run now and start a new query for the rest
                    using (var cmd = new SqlCommand(query.TrimEnd(','), _connection, _transaction))
                    {
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();
                    }
                    query = s_InsertJamTimeEstimateQueryBase;
                    counter = 0;
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

        public IList<JamTimeEstimate> GetAllJamTimeEstimates()
        {
            var dataList = new List<JamTimeEstimate>();
            using (var cmd = new SqlCommand(s_GetAllJamTimeEstimatesQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = ReadEstimateData(reader);
                        dataList.Add(data);
                    }
                }
            }
            return dataList;
        }

        internal JamTimeEstimate ReadEstimateData(SqlDataReader reader)
        {
            JamTimeEstimate jam = new JamTimeEstimate();
            jam.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            jam.Estimate = reader.GetInt32(reader.GetOrdinal("Seconds"));
            jam.Minimum = reader.GetInt32(reader.GetOrdinal("Minimum"));
            jam.Maximum = reader.GetInt32(reader.GetOrdinal("Maximum"));
            return jam;
        }
    }
}
