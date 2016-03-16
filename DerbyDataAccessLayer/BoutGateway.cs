using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class BoutGateway : DerbyGatewayBase
    {
        #region Queries
        internal const string s_GetBoutQuery = @"
SELECT * 
FROM Bout
WHERE 
    HomeTeamID = @HomeTeamID AND
    AwayTeamID = @AwayTeamID AND
    PlayDate = @BoutDate";

        internal const string s_GetBoutByIdQuery = "SELECT * FROM Bout WHERE RinxterID = @ID";

        internal const string s_GetBoutsQuery = "SELECT * FROM Bout";

        internal const string s_AddBoutQuery = @"INSERT INTO Bout Values (@ID, @HomeTeamID, @AwayTeamID, @BoutDate)";

        internal const string s_AddBoutQuery2 = @"INSERT INTO Bout Values (NULL, @HomeTeamID, @AwayTeamID, @BoutDate)";

        internal const string s_AddBoutWithNamesQuery = @"
INSERT INTO BOUT
SELECT
    @ID,
    ht.ID,
    at.ID,
    @BoutDate
FROM Team ht, Team at
WHERE
    ht.Name = @HomeTeam AND
    at.Name = @AwayTeam
";
        #endregion

        private IList<Bout> _boutList;

        public BoutGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) 
        {
            _boutList = GetBouts();
        }

        public Bout GetRinxterBout(int id, int homeTeamID, int awayTeamID, DateTime boutDate)
        {
            using (var cmd = new SqlCommand(s_GetBoutQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@HomeTeamID", SqlDbType.Int).Value = homeTeamID;
                cmd.Parameters.Add("@AwayTeamID", SqlDbType.Int).Value = awayTeamID;
                cmd.Parameters.Add("@BoutDate", SqlDbType.DateTime).Value = boutDate;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        return AddAndGetRinxterBout(id, homeTeamID, awayTeamID, boutDate);
                    }
                    return ReadData(reader);
                }
            }
        }

        public bool DoesBoutExist(int homeTeamID, int awayTeamID, DateTime boutDate)
        {
            return _boutList.Any(b => b.AwayTeamID == awayTeamID && b.HomeTeamID == homeTeamID && b.BoutDate == boutDate);
        }

        public Bout GetBout(int homeTeamID, int awayTeamID, DateTime boutDate)
        {
            using (var cmd = new SqlCommand(s_GetBoutQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@HomeTeamID", SqlDbType.Int).Value = homeTeamID;
                cmd.Parameters.Add("@AwayTeamID", SqlDbType.Int).Value = awayTeamID;
                cmd.Parameters.Add("@BoutDate", SqlDbType.DateTime).Value = boutDate;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        return AddInternalBout(homeTeamID, awayTeamID, boutDate);
                    }
                    Bout bout = ReadData(reader);
                    _boutList.Add(bout);
                    return bout;
                }
            }
        }

        public Bout GetRinxterBout(int id)
        {
            using (var cmd = new SqlCommand(s_GetBoutByIdQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        public IList<Bout> GetBouts()
        {
            IList<Bout> bouts = new List<Bout>();
            using (var cmd = new SqlCommand(s_GetBoutsQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    while(reader.Read())
                    {
                        bouts.Add(ReadData(reader));
                    }
                    reader.Close();
                }
            }
            return bouts;
        }

        internal Bout AddAndGetRinxterBout(int id, int homeTeamID, int awayTeamID, DateTime boutDate)
        {
            using (var cmd = new SqlCommand(s_AddBoutQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@HomeTeamID", SqlDbType.Int).Value = homeTeamID;
                cmd.Parameters.Add("@AwayTeamID", SqlDbType.Int).Value = awayTeamID;
                cmd.Parameters.Add("@BoutDate", SqlDbType.DateTime).Value = boutDate;
                cmd.ExecuteNonQuery();
                return GetRinxterBout(id, homeTeamID, awayTeamID, boutDate);
            }
        }

        internal Bout AddInternalBout(int homeTeamID, int awayTeamID, DateTime boutDate)
        {
            using (var cmd = new SqlCommand(s_AddBoutQuery2, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@HomeTeamID", SqlDbType.Int).Value = homeTeamID;
                cmd.Parameters.Add("@AwayTeamID", SqlDbType.Int).Value = awayTeamID;
                cmd.Parameters.Add("@BoutDate", SqlDbType.DateTime).Value = boutDate;
                cmd.ExecuteNonQuery();
                return GetBout(homeTeamID, awayTeamID, boutDate);
            }
        }

        internal Bout ReadData(SqlDataReader reader)
        {
            Bout bout = new Bout();
            bout.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            bout.HomeTeamID = reader.GetInt32(reader.GetOrdinal("HomeTeamID"));
            bout.AwayTeamID = reader.GetInt32(reader.GetOrdinal("AwayTeamID"));
            bout.BoutDate = reader.GetDateTime(reader.GetOrdinal("PlayDate"));
            bout.RinxterID = reader.IsDBNull(reader.GetOrdinal("RinxterID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("RinxterID"));
            return bout;
        }
    }
}
