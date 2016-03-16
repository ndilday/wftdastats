using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using DerbyDataModels;

namespace DerbyDataAccessLayer
{
    public class JamTeamEffectivenessGateway : DerbyGatewayBase
    {
        #region Queries
        private const string s_DeleteJamTeamEffectivenessQuery = "DELETE FROM Jam_Team_Effectiveness\n";
        private const string s_InsertJamTeamEffectivenessBase = "INSERT INTO Jam_Team_Effectiveness VALUES";
        private const string s_InsertJamTeamEffectivenessParameter = "\n({0}, {1}, {2}),";
        private const string s_GetAllJamTeamEffectivenessQuery = "SELECT * FROM Jam_Team_Effectiveness";
        private const string s_GetJamTeamEffectivenessForTeamQuery = "SELECT * FROM Jam_Team_Effectiveness WHERE TeamID = @TeamID";
        #endregion

        public JamTeamEffectivenessGateway(SqlConnection connection, SqlTransaction transaction) : base(connection, transaction) { }

        public void InsertJamTeamEffectiveness(IList<JamTeamEffectiveness> jtes)
        {
            int counter = 0;
            string query = s_DeleteJamTeamEffectivenessQuery + s_InsertJamTeamEffectivenessBase;
            foreach (JamTeamEffectiveness jte in jtes)
            {
                query += String.Format(s_InsertJamTeamEffectivenessParameter,
                                        jte.JamID,
                                        jte.TeamID,
                                        jte.Percentile);
                counter++;
                if (counter > 990)
                {
                    // to avoid ever hitting the 1000 limit, run now and start a new query for the rest
                    using (var cmd = new SqlCommand(query.TrimEnd(','), _connection, _transaction))
                    {
                        cmd.Parameters.Clear();
                        cmd.ExecuteNonQuery();
                    }
                    query = s_InsertJamTeamEffectivenessBase;
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

        public Dictionary<int, Dictionary<int, double>> GetAllJamTeamEffectiveness()
        {
            var data = new Dictionary<int, Dictionary<int, double>>();
            using (var cmd = new SqlCommand(s_GetAllJamTeamEffectivenessQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int jamID = reader.GetInt32(reader.GetOrdinal("JamID"));
                        int teamID = reader.GetInt32(reader.GetOrdinal("TeamID"));
                        double percentile = reader.GetDouble(reader.GetOrdinal("Percentile"));
                        if(!data.ContainsKey(jamID))
                        {
                            data[jamID] = new Dictionary<int, double>();
                        }
                        data[jamID][teamID] = percentile;
                    }
                }
            }
            return data;
        }

        public Dictionary<int, double> GetJamTeamEffectivenessForTeam(int teamID)
        {
            var data = new Dictionary<int, double>();
            using (var cmd = new SqlCommand(s_GetJamTeamEffectivenessForTeamQuery, _connection, _transaction))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add("@TeamID", SqlDbType.Int).Value = teamID;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int jamID = reader.GetInt32(reader.GetOrdinal("JamID"));
                        double percentile = reader.GetDouble(reader.GetOrdinal("Percentile"));
                        data[jamID] = percentile;
                    }
                }
            }
            return data;
        }
    }
}
