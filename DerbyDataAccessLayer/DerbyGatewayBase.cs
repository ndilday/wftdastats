using System.Data.SqlClient;

namespace DerbyDataAccessLayer
{
    public class DerbyGatewayBase
    {
        protected readonly SqlConnection _connection;
        protected readonly SqlTransaction _transaction;

        public DerbyGatewayBase(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }
    }
}
