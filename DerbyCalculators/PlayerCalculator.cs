using System.Collections.Generic;
using System.Data.SqlClient;

using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    public class PlayerCalculator
    {
        private string _connectionString;

        public PlayerCalculator(string connString)
        {
            _connectionString = connString;
        }

        public IList<Player> GetAllPlayers()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            SqlTransaction transaction = connection.BeginTransaction();

            var list = new PlayerGateway(connection, transaction).GetAllPlayers();

            transaction.Commit();
            connection.Close();

            return list;
        }
    }
}
