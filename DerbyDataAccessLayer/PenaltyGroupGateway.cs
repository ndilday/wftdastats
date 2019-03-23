using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class PenaltyGroupGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_GetPenaltiesByGroupsQuery = @"
SELECT DISTINCT pb.GroupID, tp.PlayerID, jp.JamID, ft.Code, p.ID
FROM Penalty p
JOIN FoulType ft ON ft.ID = p.FoulTypeID
JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Penalty_BoxTime pb ON pb.PenaltyID = p.ID
";
        private const string s_GetBoxTimesByGroupsQuery = @"
SELECT DISTINCT pb.GroupID, tp.PlayerID, jp.JamID, bt.StartedInBox, bt.EndedInBox, bt.IsJammer, bt.ID
FROM BoxTime bt
JOIN Jam_Player jp ON jp.ID = bt.JamPlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Penalty_BoxTime pb ON pb.BoxTimeID = bt.ID
";

        private const string s_GetPenaltiesByGroupsForTeamQuery = @"
SELECT DISTINCT pb.GroupID, tp.PlayerID, jp.JamID, ft.Code, p.ID
FROM Penalty p
JOIN FoulType ft ON ft.ID = p.FoulTypeID
JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Penalty_BoxTime pb ON pb.PenaltyID = p.ID
WHERE
    tp.TeamID = @TeamID
";

        private const string s_GetPenaltiesByGroupsForTeamAfterDateQuery = @"
SELECT DISTINCT pb.GroupID, tp.PlayerID, jp.JamID, ft.Code, p.ID
FROM Penalty p
JOIN FoulType ft ON ft.ID = p.FoulTypeID
JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Penalty_BoxTime pb ON pb.PenaltyID = p.ID
JOIN Jam j ON j.ID = jp.JamId
JOIN Bout b ON b.ID = j.BoutId
WHERE
    tp.TeamID = @TeamID AND
    b.PlayDate >= @date
";

        private const string s_GetBoxTimesByGroupsForTeamQuery = @"
SELECT DISTINCT pb.GroupID, tp.PlayerID, jp.JamID, bt.StartedInBox, bt.EndedInBox, bt.IsJammer, bt.ID
FROM BoxTime bt
JOIN Jam_Player jp ON jp.ID = bt.JamPlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Penalty_BoxTime pb ON pb.BoxTimeID = bt.ID
WHERE
    tp.TeamID = @TeamID
";

        private const string s_GetBoxTimesByGroupsForTeamAfterDateQuery = @"
SELECT DISTINCT pb.GroupID, tp.PlayerID, jp.JamID, bt.StartedInBox, bt.EndedInBox, bt.IsJammer, bt.ID
FROM BoxTime bt
JOIN Jam_Player jp ON jp.ID = bt.JamPlayerID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Penalty_BoxTime pb ON pb.BoxTimeID = bt.ID
JOIN Jam j ON j.ID = jp.JamId
JOIN Bout b ON b.ID = j.BoutId
WHERE
    tp.TeamID = @TeamID AND
    b.PlayDate >= @date
";
        #endregion

        public PenaltyGroupGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public IList<PenaltyGroup> GetAllPenaltyGroups()
        {
            var gps = GetGroupPenalties();
            var gbts = GetGroupBoxTimes();

            List<PenaltyGroup> list = new List<PenaltyGroup>();
            foreach (KeyValuePair<int, List<Penalty>> group in gps)
            {
                var penalties = group.Value;
                PenaltyGroup pg = new PenaltyGroup
                {
                    Penalties = penalties,
                    PlayerID = penalties.First().PlayerID,
                    GroupID = group.Key
                };
                if(gbts.Any(bts => bts.Key == group.Key))
                {
                    pg.BoxTimes = gbts[group.Key];
                }
                else
                {
                    pg.BoxTimes = null;
                }
                list.Add(pg);
            }
            return list;
        }

        private Dictionary<int, List<Penalty>> GetGroupPenalties()
        {
            Dictionary<int, List<Penalty>> groupPenaltyMap = new Dictionary<int, List<Penalty>>();
            using (var cmd = new SqlCommand(s_GetPenaltiesByGroupsQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                using (var reader = cmd.ExecuteReader())
                {
                    int groupID;
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        var penalty = ReadPenaltyData(reader, out groupID);
                        if (!groupPenaltyMap.ContainsKey(groupID))
                        {
                            groupPenaltyMap[groupID] = new List<Penalty>();
                        }
                        groupPenaltyMap[groupID].Add(penalty);
                    }
                    return groupPenaltyMap;
                }
            }
        }

        private Dictionary<int, List<BoxTime>> GetGroupBoxTimes()
        {
            Dictionary<int, List<BoxTime>> groupBoxTimeMap = new Dictionary<int, List<BoxTime>>();
            using (var cmd = new SqlCommand(s_GetBoxTimesByGroupsQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    int groupID;
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        var boxTime = ReadBoxTimeData(reader, out groupID);
                        if (!groupBoxTimeMap.ContainsKey(groupID))
                        {
                            groupBoxTimeMap[groupID] = new List<BoxTime>();
                        }
                        groupBoxTimeMap[groupID].Add(boxTime);
                    }
                    return groupBoxTimeMap;
                }
            }
        }

        public List<PenaltyGroup> GetPenaltyGroupsForTeam(int teamID)
        {
            var gps = GetGroupPenaltiesForTeam(teamID);
            var gbts = GetGroupBoxTimesForTeam(teamID);

            List<PenaltyGroup> list = new List<PenaltyGroup>();
            foreach (KeyValuePair<int, List<Penalty>> group in gps)
            {
                var penalties = group.Value;
                PenaltyGroup pg = new PenaltyGroup
                {
                    Penalties = penalties,
                    PlayerID = penalties.First().PlayerID,
                    GroupID = group.Key
                };
                if (gbts.Any(bts => bts.Key == group.Key))
                {
                    pg.BoxTimes = gbts[group.Key];
                }
                else
                {
                    pg.BoxTimes = null;
                }
                list.Add(pg);
            }
            return list;
        }

        public List<PenaltyGroup> GetPenaltyGroupsForTeamAfterDate(int teamID, DateTime date)
        {
            var gps = GetGroupPenaltiesForTeamAfterDate(teamID, date);
            var gbts = GetGroupBoxTimesForTeamAfterDate(teamID, date);

            List<PenaltyGroup> list = new List<PenaltyGroup>();
            foreach (KeyValuePair<int, List<Penalty>> group in gps)
            {
                var penalties = group.Value;
                PenaltyGroup pg = new PenaltyGroup
                {
                    Penalties = penalties,
                    PlayerID = penalties.First().PlayerID,
                    GroupID = group.Key
                };
                if (gbts.Any(bts => bts.Key == group.Key))
                {
                    pg.BoxTimes = gbts[group.Key];
                }
                else
                {
                    pg.BoxTimes = null;
                }
                list.Add(pg);
            }
            return list;
        }

        private Dictionary<int, List<Penalty>> GetGroupPenaltiesForTeam(int teamID)
        {
            Dictionary<int, List<Penalty>> groupPenaltyMap = new Dictionary<int, List<Penalty>>();
            using (var cmd = new SqlCommand(s_GetPenaltiesByGroupsForTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                using (var reader = cmd.ExecuteReader())
                {
                    int groupID;
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        var penalty = ReadPenaltyData(reader, out groupID);
                        if (!groupPenaltyMap.ContainsKey(groupID))
                        {
                            groupPenaltyMap[groupID] = new List<Penalty>();
                        }
                        groupPenaltyMap[groupID].Add(penalty);
                    }
                    return groupPenaltyMap;
                }
            }
        }

        private Dictionary<int, List<Penalty>> GetGroupPenaltiesForTeamAfterDate(int teamID, DateTime date)
        {
            Dictionary<int, List<Penalty>> groupPenaltyMap = new Dictionary<int, List<Penalty>>();
            using (var cmd = new SqlCommand(s_GetPenaltiesByGroupsForTeamAfterDateQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                cmd.Parameters.Add("@date", SqlDbType.DateTime).Value = date;
                using (var reader = cmd.ExecuteReader())
                {
                    int groupID;
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        var penalty = ReadPenaltyData(reader, out groupID);
                        if (!groupPenaltyMap.ContainsKey(groupID))
                        {
                            groupPenaltyMap[groupID] = new List<Penalty>();
                        }
                        groupPenaltyMap[groupID].Add(penalty);
                    }
                    return groupPenaltyMap;
                }
            }
        }

        private Dictionary<int, List<BoxTime>> GetGroupBoxTimesForTeam(int teamID)
        {
            Dictionary<int, List<BoxTime>> groupBoxTimeMap = new Dictionary<int, List<BoxTime>>();
            using (var cmd = new SqlCommand(s_GetBoxTimesByGroupsForTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                using (var reader = cmd.ExecuteReader())
                {
                    int groupID;
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        var boxTime = ReadBoxTimeData(reader, out groupID);
                        if (!groupBoxTimeMap.ContainsKey(groupID))
                        {
                            groupBoxTimeMap[groupID] = new List<BoxTime>();
                        }
                        groupBoxTimeMap[groupID].Add(boxTime);
                    }
                    return groupBoxTimeMap;
                }
            }
        }

        private Dictionary<int, List<BoxTime>> GetGroupBoxTimesForTeamAfterDate(int teamID, DateTime date)
        {
            Dictionary<int, List<BoxTime>> groupBoxTimeMap = new Dictionary<int, List<BoxTime>>();
            using (var cmd = new SqlCommand(s_GetBoxTimesByGroupsForTeamAfterDateQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;
                cmd.Parameters.Add("@date", SqlDbType.DateTime).Value = date;
                using (var reader = cmd.ExecuteReader())
                {
                    int groupID;
                    // if the team record doesn't exist, we can't fix it here
                    while (reader.Read())
                    {
                        var boxTime = ReadBoxTimeData(reader, out groupID);
                        if (!groupBoxTimeMap.ContainsKey(groupID))
                        {
                            groupBoxTimeMap[groupID] = new List<BoxTime>();
                        }
                        groupBoxTimeMap[groupID].Add(boxTime);
                    }
                    return groupBoxTimeMap;
                }
            }
        }

        internal BoxTime ReadBoxTimeData(SqlDataReader reader, out int groupID)
        {
            BoxTime boxTime = new BoxTime();
            groupID = reader.GetInt32(reader.GetOrdinal("GroupID"));
            boxTime.PlayerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
            boxTime.BoxTimeID = reader.GetInt32(reader.GetOrdinal("ID"));
            boxTime.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            boxTime.IsJammer = reader.GetBoolean(reader.GetOrdinal("IsJammer"));
            boxTime.StartedJamInBox = reader.GetBoolean(reader.GetOrdinal("StartedInBox"));
            boxTime.EndedJamInBox = reader.GetBoolean(reader.GetOrdinal("EndedInBox"));
            //boxTime.BoxSeconds = reader.GetDouble(reader.GetOrdinal("BoxSeconds"));
            return boxTime;
        }

        internal Penalty ReadPenaltyData(SqlDataReader reader, out int groupID)
        {
            Penalty penalty = new Penalty();
            groupID = reader.GetInt32(reader.GetOrdinal("GroupID"));
            penalty.PlayerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
            penalty.JamID = reader.GetInt32(reader.GetOrdinal("JamID"));
            penalty.PenaltyCode = reader.GetString(reader.GetOrdinal("Code"));
            return penalty;
        }
    }
}
