using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class AveragePenaltyCostGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_DeleteAveragePenaltyCostQuery = "DELETE FROM AveragePenaltyCost\n";
        private const string s_InsertAveragePenaltyCostQuery = "INSERT INTO AveragePenaltyCost VALUES(@BlockerPointCost, @JammerPointCost, @BlockerValueCost, @JammerValueCost)";
        private const string s_GetAveragePenaltyCostQuery = "SELECT * FROM AveragePenaltyCost";
        #endregion

        public AveragePenaltyCostGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public void InsertAveragePenaltyCost(AveragePenaltyCostPerJam average)
        {
            string query = s_DeleteAveragePenaltyCostQuery + s_InsertAveragePenaltyCostQuery;
            using (var cmd = new SqlCommand(query, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@JammerPointCost", SqlDbType.Float).Value = average.JammerPointCost;
                cmd.Parameters.Add("@BlockerPointCost", SqlDbType.Float).Value = average.BlockerPointCost;
                cmd.Parameters.Add("@JammerValueCost", SqlDbType.Float).Value = average.JammerValueCost;
                cmd.Parameters.Add("@BlockerValueCost", SqlDbType.Float).Value = average.BlockerValueCost;
                cmd.ExecuteNonQuery();
            }
        }

        public AveragePenaltyCostPerJam GetAveragePenaltyCost()
        {
            using (var cmd = new SqlCommand(s_GetAveragePenaltyCostQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AveragePenaltyCostPerJam
                        {
                            BlockerPointCost = reader.GetDouble(reader.GetOrdinal("BlockerPointCost")),
                            JammerPointCost = reader.GetDouble(reader.GetOrdinal("JammerPointCost")),
                            BlockerValueCost = reader.GetDouble(reader.GetOrdinal("BlockerValueCost")),
                            JammerValueCost = reader.GetDouble(reader.GetOrdinal("JammerValueCost"))
                        };
                    }
                }
            }
            return null;
        }
    }
}
