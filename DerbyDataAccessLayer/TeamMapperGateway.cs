using System.Collections.Generic;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class TeamMapperGateway : DerbyGatewayBase
    {
        #region Queries
        internal const string s_GetAllTeamMappersQuery = @"SELECT * FROM TeamNameMapper";
        #endregion

        public TeamMapperGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public IList<TeamMapper> GetAllTeamMappers()
        {
            var data = new List<TeamMapper>(250);
            using (var cmd = new SqlCommand(s_GetAllTeamMappersQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(ReadData(reader));
                    }
                }
            }
            return data;
        }

        internal TeamMapper ReadData(SqlDataReader reader)
        {
            TeamMapper team = new TeamMapper();
            team.TeamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
            team.TeamSpelling = reader.GetString(reader.GetOrdinal("NameSpelling"));
            return team;
        }
    }
}
