using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class PlayerGateway : DerbyGatewayBase
    {
        #region Queries
        internal const string s_GetPlayerByRinxterIDQuery = @"
SELECT p.*, tp.TeamID, t.RinxterID
FROM Player p
JOIN Team_Player tp ON tp.PlayerID = p.ID
JOIN Team t ON t.ID = tp.TeamID
WHERE 
    p.RinxterID = @ID";

        internal const string s_GetRinxterPlayerQuery = @"
SELECT p.*, tp.TeamID, t.RinxterID
FROM Player p
JOIN Team_Player tp ON tp.PlayerID = p.ID
JOIN Team t ON t.ID = tp.TeamID
WHERE 
    t.RinxterID = @TeamID AND
    p.RinxterID = @PlayerID";

        internal const string s_GetTeamPlayerQuery = @"
SELECT p.*, tp.TeamID
FROM Player p
JOIN Team_Player tp ON tp.PlayerID = p.ID
WHERE 
    tp.TeamID = @TeamID AND
    p.Name = @PlayerName AND
    p.Number = @PlayerNumber";

        internal const string s_GetPlayerQuery = @"
SELECT p.*, -1 AS TeamID
FROM Player p
WHERE
    p.Name = @PlayerName AND
    p.Number = @PlayerNumber";

        internal const string s_GetPlayerByNameAndTeamQuery = @"
SELECT p.*, @TeamID AS TeamID
FROM Player p
JOIN Team_Player tp ON tp.PlayerID = p.ID
WHERE
    p.Name = @PlayerName AND
    tp.TeamID = @TeamID";

        internal const string s_GetPlayerByNumberAndTeamQuery = @"
SELECT p.*, @TeamID AS TeamID
FROM Player p
JOIN Team_Player tp ON tp.PlayerID = p.ID
WHERE
    p.Number = @PlayerNumber AND
    tp.TeamID = @TeamID";

        internal const string s_GetAllPlayersQuery = @"
SELECT p.*, tp.TeamID
FROM Player p
JOIN Team_Player tp ON tp.PlayerID = p.ID
JOIN Team t ON t.ID = tp.TeamID";

        internal const string s_GetPlayersForTeamQuery = @"
SELECT p.*, tp.TeamID
FROM Player p
JOIN Team_Player tp ON tp.PlayerID = p.ID
JOIN Team t ON t.ID = tp.TeamID
WHERE 
    t.ID = @TeamID";

        internal const string s_AddRinxterTeamPlayerQuery =
@"INSERT INTO Team_Player 
SELECT t.ID, p.ID
FROM Player p, Team t
WHERE 
    p.RinxterID = @PlayerID AND
    t.RinxterID = @TeamID";

        internal const string s_AddTeamPlayerQuery =
@"INSERT INTO Team_Player
SELECT @TeamID, ID
FROM Player
WHERE 
    Name = @Name AND
    Number = @Number";

        internal const string s_AddPlayerQuery =
@"INSERT INTO Player Values (@ID, @Name, @Number)";
        #endregion

        public PlayerGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public Player GetRinxterPlayer(int playerID, string number, string name, int teamID)
        {
            using (var cmd = new SqlCommand(s_GetRinxterPlayerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = playerID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        return AddRinxterPlayer(playerID, number, name, teamID);
                    }
                    return ReadData(reader);
                }
            }
        }

        public Player GetPlayer(string number, string name, int teamID)
        {
            using (var cmd = new SqlCommand(s_GetTeamPlayerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                cmd.Parameters.Add("@PlayerName", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@PlayerNumber", SqlDbType.NVarChar).Value = number;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        reader.Close();
                        return AddPlayer(number, name, teamID);
                    }
                    return ReadData(reader);
                }
            }
        }

        public Player GetPlayer(string number, string name)
        {
            using (var cmd = new SqlCommand(s_GetPlayerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@PlayerName", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@PlayerNumber", SqlDbType.NVarChar).Value = number;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        public Player GetPlayerByName(string name, int teamID)
        {
            using (var cmd = new SqlCommand(s_GetPlayerByNameAndTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@PlayerName", SqlDbType.NVarChar).Value = name;
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        public Player GetPlayerByNumber(string number, int teamID)
        {
            using (var cmd = new SqlCommand(s_GetPlayerByNumberAndTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@PlayerNumber", SqlDbType.NVarChar).Value = number;
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        public IList<Player> GetAllPlayers()
        {
            var dataList = new List<Player>();
            using (var cmd = new SqlCommand(s_GetAllPlayersQuery, _connection, _transaction))
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

        public IList<Player> GetPlayersForTeam(int teamID)
        {
            var dataList = new List<Player>();
            using (var cmd = new SqlCommand(s_GetPlayersForTeamQuery, _connection, _transaction))
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

        public Player AddRinxterPlayer(int rinxterID, string number, string name, int teamID)
        {
            // see if the player already exists
            Player player = GetPlayerByRinxterID(rinxterID);
            if (player == null)
            {
                // create the player
                using (var cmd = new SqlCommand(s_AddPlayerQuery, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = rinxterID;
                    cmd.Parameters.Add("@Number", SqlDbType.NVarChar).Value = number;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
                    cmd.ExecuteNonQuery();
                }
            }

            // see if the team_player already exists
            using (var cmd = new SqlCommand(s_AddRinxterTeamPlayerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = rinxterID;
                cmd.ExecuteNonQuery();
            }

            return GetRinxterPlayer(rinxterID, number, name, teamID);
        }

        public Player AddPlayer(string number, string name, int teamID)
        {
            // see if there is a player with this name affiliated with this team
            Player namedPlayer = GetPlayerByName(name, teamID);
            if(namedPlayer != null)
            {
                Console.WriteLine(string.Format("Assuming {0} {1} is the same player as {2} {3}", namedPlayer.Number, namedPlayer.Name, number, name));
                return namedPlayer;
            }

            Player numPlayer = GetPlayerByNumber(number, teamID);
            if (numPlayer != null)
            {
                Console.WriteLine(string.Format("Assuming {0} {1} is the same player as {2} {3}", numPlayer.Number, numPlayer.Name, number, name));
                return numPlayer;
            }

            // create the player
            if (GetPlayer(number, name) == null)
            {
                using (var cmd = new SqlCommand(s_AddPlayerQuery, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@Number", SqlDbType.NVarChar).Value = number;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine(string.Format("Player {0} {1} is new on team {2}", number, name, teamID));
            using (var cmd = new SqlCommand(s_AddTeamPlayerQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                cmd.Parameters.Add("@Number", SqlDbType.NVarChar).Value = number;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
                cmd.ExecuteNonQuery();
            }

            return GetPlayer(number, name, teamID);
        }

        private Player GetPlayerByRinxterID(int ID)
        {
            using (var cmd = new SqlCommand(s_GetPlayerByRinxterIDQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = ID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, add it
                    if (!reader.Read())
                    {
                        // throw exception?
                        reader.Close();
                        return null;
                    }
                    return ReadData(reader);
                }
            }
        }

        internal Player ReadData(SqlDataReader reader)
        {
            Player player = new Player();
            player.ID = reader.GetInt32(reader.GetOrdinal("ID"));
            player.TeamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
            player.Number = reader.GetString(reader.GetOrdinal("Number"));
            player.Name = reader.GetString(reader.GetOrdinal("Name"));
            player.RinxterID = reader.IsDBNull(reader.GetOrdinal("RinxterID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("RinxterID"));
            return player;
        }
    }
}
