using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DerbyDataAccessLayer
{
    public class BoxTimeEstimateGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_DeleteBoxTimeEstimate = "DELETE FROM BoxTimeEstimate\n";
        private const string s_InsertBoxTimeEstimateQueryBase = "INSERT INTO BoxTimeEstimate VALUES\n";
        private const string s_InsertBoxTimeEstimateParameter = "\n({0}, {1}),";
        private const string s_GetAllBoxTimeEstimatesQuery = "SELECT * FROM BoxTimeEstimate";
        #endregion

        public BoxTimeEstimateGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public void InsertBoxTimeEstimates(Dictionary<int, int> boxTimeEstimates)
        {
            int counter = 0;
            string query = s_DeleteBoxTimeEstimate + s_InsertBoxTimeEstimateQueryBase;
            foreach (KeyValuePair<int, int> kvp in boxTimeEstimates)
            {
                query += String.Format(s_InsertBoxTimeEstimateParameter,
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
                    query = s_InsertBoxTimeEstimateQueryBase;
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

        public Dictionary<int, int> GetAllBoxTimeEstimates()
        {
            var data = new Dictionary<int, int>();
            using (var cmd = new SqlCommand(s_GetAllBoxTimeEstimatesQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int boxTimeID = reader.GetInt32(reader.GetOrdinal("BoxTimeID"));
                        int estimate = reader.GetInt32(reader.GetOrdinal("Estimate"));
                        data[boxTimeID] = estimate;
                    }
                }
            }
            return data;
        }

    }
}
