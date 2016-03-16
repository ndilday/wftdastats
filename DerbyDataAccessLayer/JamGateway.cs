using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class JamGateway : DerbyGatewayBase
    {
        #region Queries
        internal const string s_GetJamQuery = @"
SELECT *
FROM Jam
WHERE
    BoutID = @BoutID AND
    IsFirstHalf = @IsFirstHalf AND
    JamNum = @Number
";

        internal const string s_GetJamsForRinxterBoutQuery = @"
SELECT j.*
FROM Jam j
JOIN Bout b ON b.ID = j.BoutID
WHERE
    b.RinxterID = @BoutID";

        internal const string s_GetJamsForBoutQuery = @"
SELECT *
FROM Jam
WHERE
    BoutID = @BoutID";

        internal const string s_GetJamsForTeamQuery = @"
SELECT j.*
FROM Jam j
JOIN Bout b ON j.BoutID = b.ID
WHERE
    b.HomeTeamID = @TeamID OR
    b.AwayTeamID = @TeamID";

        internal const string s_GetJamsForTeamAndYearQuery = @"
SELECT j.*
FROM Jam j
JOIN Bout b ON j.BoutID = b.ID
WHERE
    b.PlayDate > @BoutDate AND
    (b.HomeTeamID = @TeamID OR
    b.AwayTeamID = @TeamID)";

        internal const string s_AddJamQuery = @"
INSERT INTO JAM VALUES (@IsFirstHalf, @Number, @BoutID)
";
        internal const string s_GetAllJamsQuery = "SELECT * FROM Jam";
        #endregion

        public JamGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public Jam GetJam(int boutID, bool isFirstHalf, int jamNumber)
        {
            using (var cmd = new SqlCommand(s_GetJamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@BoutID", SqlDbType.Int).Value = boutID;
                cmd.Parameters.Add("@IsFirstHalf", SqlDbType.Bit).Value = isFirstHalf;
                cmd.Parameters.Add("@Number", SqlDbType.Int).Value = jamNumber;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        return AddJam(boutID, isFirstHalf, jamNumber);
                    }
                    return ReadData(reader);
                }
            }
        }

        public IList<Jam> GetJamsForBout(int boutID)
        {
            var dataList = new List<Jam>();
            using (var cmd = new SqlCommand(s_GetJamsForBoutQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@BoutID", SqlDbType.Int).Value = boutID;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = ReadData(reader);
                        dataList.Add(data);
                    }
                }
            }
            return dataList;
        }

        public IList<Jam> GetJamsForTeam(int teamID)
        {
            var dataList = new List<Jam>();
            using (var cmd = new SqlCommand(s_GetJamsForTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = ReadData(reader);
                        dataList.Add(data);
                    }
                }
            }
            return dataList;
        }

        public IList<Jam> GetJamsForTeamAfterDate(int teamID, DateTime dateTime)
        {
            var dataList = new List<Jam>();
            using (var cmd = new SqlCommand(s_GetJamsForTeamAndYearQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                cmd.Parameters.Add("@BoutDate", SqlDbType.DateTime).Value = dateTime;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = ReadData(reader);
                        dataList.Add(data);
                    }
                }
            }
            return dataList;
        }

        public IList<Jam> GetAllJams()
        {
            var dataList = new List<Jam>();
            using (var cmd = new SqlCommand(s_GetAllJamsQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = ReadData(reader);
                        dataList.Add(data);
                    }
                }
            }
            return dataList;
        }

        public IList<Jam> GetJamsForRinxterBout(int boutID)
        {
            var dataList = new List<Jam>();
            using (var cmd = new SqlCommand(s_GetJamsForRinxterBoutQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@BoutID", SqlDbType.Int).Value = boutID;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = ReadData(reader);
                        dataList.Add(data);
                    }
                }
            }
            return dataList;
        }

        public Jam AddJam(int boutID, bool isFirstHalf, int jamNumber)
        {
            // create the player
            using (var cmd = new SqlCommand(s_AddJamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@BoutID", SqlDbType.Int).Value = boutID;
                cmd.Parameters.Add("@IsFirstHalf", SqlDbType.Bit).Value = isFirstHalf;
                cmd.Parameters.Add("@Number", SqlDbType.Int).Value = jamNumber;
                cmd.ExecuteNonQuery();
            }
            return GetJam(boutID, isFirstHalf, jamNumber);
        }

        internal Jam ReadData(SqlDataReader reader)
        {
            Jam jam = new Jam();
            jam.BoutID = reader.GetInt32(reader.GetOrdinal("BoutID"));
            jam.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            jam.IsFirstHalf = reader.GetBoolean(reader.GetOrdinal("IsFirstHalf"));
            jam.JamNumber = reader.GetInt32(reader.GetOrdinal("JamNum"));
            return jam;
        }
    }
}
