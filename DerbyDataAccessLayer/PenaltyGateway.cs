using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class PenaltyGateway : DerbyGatewayBase
    {
        #region Queries

        const string s_AddBoxTimeQuery = @"
INSERT INTO BoxTime 
SELECT jp.ID, @IsJammer, @StartedInBox, @EndedInBox
FROM Team_Player tp
JOIN Jam_Player jp ON jp.Team_PlayerID = tp.ID
WHERE
    jp.JamID = @JamID AND
    tp.PlayerID = @PlayerID";

        const string s_AddPenaltyQuery = @"
INSERT INTO Penalty
SELECT ft.ID, jp1.ID
FROM FoulType ft,
     Team_Player tp
JOIN Jam_Player jp1 ON jp1.Team_PlayerID = tp.ID
WHERE
    tp.PlayerID = @PlayerID AND
    jp1.JamID = @JamID AND
    ft.Code = @Code";

        const string s_AddBasicPenaltyQuery = @"
INSERT INTO Penalty(FoulTypeID, TeamPlayerID, JamID)
SELECT ft.ID, tp.ID, @JamID
FROM FoulType ft,
     Team_Player tp
JOIN Bout b ON b.HomeTeamID = tp.TeamID OR b.AwayTeamID = tp.TeamID
JOIN Jam j ON j.BoutID = b.ID
WHERE
    tp.PlayerID = @PlayerID AND
    j.ID = @JamID AND
    ft.Code = @Code";

        const string s_AddPenaltyParameterizedQuery = @"
INSERT INTO Penalty
SELECT ft.ID, jp1.ID
FROM FoulType ft,
     Team_Player tp
JOIN Jam_Player jp1 ON jp1.Team_PlayerID = tp.ID
WHERE
    tp.PlayerID = {0} AND
    jp1.JamID = {1} AND
    ft.Code = '{2}'
";
        const string s_AddBoxTimeParameterizedQuery = @"
INSERT INTO BoxTime 
SELECT jp.ID, {2}, {3}, {4}
FROM Team_Player tp
JOIN Jam_Player jp ON jp.Team_PlayerID = tp.ID
WHERE
    jp.JamID = {1} AND
    tp.PlayerID = {0}
";
        const string s_AddPenalty_BoxTimeMagicQuery = @"
INSERT INTO Penalty_BoxTime
SELECT p.ID, bt.ID
FROM BoxTime bt,
Penalty p
WHERE bt.ID > @maxBoxTimeID AND
p.ID > @maxPenaltyID
";
        const string s_AddPenalty_BoxTimeMagicQuery2 = @"
INSERT INTO Penalty_BoxTime
SELECT t.ID + 1, p.ID, bt.ID
FROM BoxTime bt,
Penalty p,
tmpVar t
WHERE bt.ID > t.bt AND
p.ID > t.p
";
        const string s_UpdateTmpVarQuery = @"
UPDATE tmpVar SET bt = (SELECT ISNULL(MAX(ID),0) FROM BoxTime)
UPDATE tmpVar SET p = (SELECT ISNULL(MAX(ID),0) FROM Penalty)
UPDATE tmpVar SET ID = (SELECT ISNULL(MAX(GroupID),0) FROM Penalty_BoxTime)
";

        const string s_AddPenalty_BoxTimeBasicQuery = @"
INSERT INTO Penalty_BoxTime
SELECT MAX(p.ID), MAX(bt.ID)
FROM Penalty p, BoxTime bt
";

        const string s_GetAllPenaltiesQuery = @"
SELECT tp.PlayerID, jp.JamID, ft.Code
FROM Penalty p
JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN FoulType ft ON p.FoulTypeID = ft.ID";

        const string s_GetPenaltiesForTeamQuery = @"
SELECT tp.PlayerID, jp.JamID, ft.Code
FROM Penalty p
JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN FoulType ft ON p.FoulTypeID = ft.ID
WHERE tp.TeamID = @TeamID";

        const string s_AddPenalty_BoxTimeQuery = @"INSERT INTO Penalty_BoxTime VALUES(@penaltyID, @boxTimeID)";

        const string s_GetMostRecentBoxTimeIDQuery = "SELECT ID FROM BoxTime WHERE ID = (SELECT MAX(ID) FROM BoxTime)";

        const string s_GetMostRecentPenaltyIDQuery = "SELECT ID FROM Penalty WHERE ID = (SELECT MAX(ID) FROM Penalty)";
        #endregion

        public PenaltyGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public void AddPenalties(IList<PenaltyService> services)
        {
            foreach (PenaltyService service in services)
            {
                string query = s_UpdateTmpVarQuery;
                foreach (Penalty penalty in service.Penalties)
                {
                    query += String.Format(s_AddPenaltyParameterizedQuery, penalty.PlayerID, penalty.JamID, penalty.PenaltyCode);
                }
                foreach (BoxTime boxTime in service.BoxTimes)
                {
                    query += String.Format(s_AddBoxTimeParameterizedQuery, boxTime.PlayerID, boxTime.JamID, boxTime.IsJammer ? 1 : 0, boxTime.StartedJamInBox == true ? 1 : 0, boxTime.EndedJamInBox ? 1 : 0);
                }
                using (var cmd = new SqlCommand(query, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand(s_AddPenalty_BoxTimeMagicQuery2, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddPenalties(PenaltyService service)
        {
            if (service.Penalties.Count == 1 && service.BoxTimes.Count == 1)
            {
                AddPenalty(service.Penalties[0], service.BoxTimes[0]);
            }
            else if (service.BoxTimes.Count == 1)
            {
                AddMultiPenalty(service.Penalties, service.BoxTimes[0]);
            }
            else if (service.Penalties.Count == 1)
            {
                AddMultiBoxTime(service.Penalties[0], service.BoxTimes);
            }
            else
            {
                int boxTimeID = GetMostRecentBoxTimeID();
                int penaltyID = GetMostRecentPenaltyID();
                foreach(BoxTime boxTime in service.BoxTimes)
                {
                    AddBoxTime(boxTime.PlayerID, boxTime.JamID, boxTime.IsJammer, boxTime.StartedJamInBox == true, boxTime.EndedJamInBox);
                }
                foreach(Penalty penalty in service.Penalties)
                {
                    AddPenalty(penalty);
                }
                using (var cmd = new SqlCommand(s_AddPenalty_BoxTimeMagicQuery, _connection, _transaction))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("@maxBoxTimeID", SqlDbType.Int).Value = boxTimeID;
                    cmd.Parameters.Add("@maxPenaltyID", SqlDbType.Int).Value = penaltyID;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void AddPenalty(Penalty penalty, BoxTime boxTime)
        {
            AddBoxTime(boxTime.PlayerID, boxTime.JamID, boxTime.IsJammer, boxTime.StartedJamInBox == true, boxTime.EndedJamInBox);
            AddPenalty(penalty.PlayerID, penalty.JamID, penalty.PenaltyCode);
            AddPenalty_BoxTimeBasic();
        }

        public void AddPenalty(Penalty penalty)
        {
            AddPenalty(penalty.PlayerID, penalty.JamID, penalty.PenaltyCode);
        }

        public void AddBasicPenalty(Penalty penalty)
        {
            using (var cmd = new SqlCommand(s_AddBasicPenaltyQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = penalty.PlayerID;
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = penalty.JamID;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar).Value = penalty.PenaltyCode;
                cmd.ExecuteNonQuery();
            }
        }

        public void AddMultiPenalty(IEnumerable<Penalty> penalties, BoxTime boxTime)
        {
            AddBoxTime(boxTime.PlayerID, boxTime.JamID, boxTime.IsJammer, boxTime.StartedJamInBox == true, boxTime.EndedJamInBox);
            foreach (Penalty penalty in penalties)
            {
                AddPenalty(penalty.PlayerID, penalty.JamID, penalty.PenaltyCode);
                AddPenalty_BoxTimeBasic();
            }
        }

        public void AddMultiBoxTime(Penalty penalty, IEnumerable<BoxTime> boxTimes)
        {
            AddPenalty(penalty);
            foreach (BoxTime boxTime in boxTimes)
            {
                AddBoxTime(boxTime.PlayerID, boxTime.JamID, boxTime.IsJammer, boxTime.StartedJamInBox == true, boxTime.EndedJamInBox);
                AddPenalty_BoxTimeBasic();
            }
        }

        public IList<Penalty> GetAllPenalties()
        {
            List<Penalty> list = new List<Penalty>();
            using (var cmd = new SqlCommand(s_GetAllPenaltiesQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        list.Add(ReadPenaltyData(reader));
                    }
                    return list;
                }
            }
        }

        public IList<Penalty> GetPenaltiesForTeam(int teamID)
        {
            List<Penalty> list = new List<Penalty>();
            using (var cmd = new SqlCommand(s_GetPenaltiesForTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;

                using (var reader = cmd.ExecuteReader())
                {
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        list.Add(ReadPenaltyData(reader));
                    }
                    return list;
                }
            }
        }

        private void AddBoxTime(int playerID, int jamID, bool isJammer, bool startedInBox, bool endedInBox)
        {
            using (var cmd = new SqlCommand(s_AddBoxTimeQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = playerID;
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@IsJammer", SqlDbType.Bit).Value = isJammer;
                cmd.Parameters.Add("@StartedInBox", SqlDbType.Bit).Value = startedInBox;
                cmd.Parameters.Add("@EndedInBox", SqlDbType.Bit).Value = endedInBox;
                cmd.ExecuteNonQuery();
            }
        }

        private void AddPenalty(int playerID, int jamID, string foulCode)
        {
            using (var cmd = new SqlCommand(s_AddPenaltyQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@PlayerID", SqlDbType.Int).Value = playerID;
                cmd.Parameters.Add("@JamID", SqlDbType.Int).Value = jamID;
                cmd.Parameters.Add("@Code", SqlDbType.NVarChar).Value = foulCode;
                cmd.ExecuteNonQuery();
            }
        }

        private void AddPenalty_BoxTimeBasic()
        {
            using (var cmd = new SqlCommand(s_AddPenalty_BoxTimeBasicQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.ExecuteNonQuery();
            }
        }

        private void AddPenalty_BoxTime(int penaltyID, int boxTimeID)
        {
            using (var cmd = new SqlCommand(s_AddPenalty_BoxTimeQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@boxTimeID", SqlDbType.Int).Value = boxTimeID;
                cmd.Parameters.Add("@penaltyID", SqlDbType.Int).Value = penaltyID;
                cmd.ExecuteNonQuery();
            }
        }

        internal BoxTime ReadBoxTimeData(SqlDataReader reader)
        {
            BoxTime boxTime = new BoxTime();
            boxTime.PlayerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
            boxTime.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            boxTime.IsJammer = reader.GetBoolean(reader.GetOrdinal("IsJammer"));
            boxTime.StartedJamInBox = reader.GetBoolean(reader.GetOrdinal("StartedInBox"));
            boxTime.EndedJamInBox = reader.GetBoolean(reader.GetOrdinal("EndedInBox"));
            return boxTime;
        }

        internal Penalty ReadPenaltyData(SqlDataReader reader)
        {
            Penalty penalty = new Penalty();
            penalty.PlayerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
            penalty.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            penalty.PenaltyCode = reader.GetString(reader.GetOrdinal("Code"));
            return penalty;
        }

        private int GetMostRecentBoxTimeID()
        {
            using (var cmd = new SqlCommand(s_GetMostRecentBoxTimeIDQuery, _connection, _transaction))
            {
                return (int)cmd.ExecuteScalar();
            }
        }

        private int GetMostRecentPenaltyID()
        {
            using (var cmd = new SqlCommand(s_GetMostRecentPenaltyIDQuery, _connection, _transaction))
            {
                return (int)cmd.ExecuteScalar();
            }
        }
    }
}
