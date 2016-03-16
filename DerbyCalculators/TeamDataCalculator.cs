using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DerbyCalculators.Models;
using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    public class TeamDataCalculator
    {
        private string _connectionString;
        public TeamDataCalculator(string connString)
        {
            _connectionString = connString;
        }

        public IList<TeamData> GetTeamData()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            var teams = new TeamGateway(connection, transaction).GetTeamsWithBouts();
            var leagues = new LeagueGateway(connection, transaction).GetAllLeagues().ToDictionary(l => l.ID);
            transaction.Commit();
            connection.Close();

            List<TeamData> list = new List<TeamData>(200);
            foreach(Team team in teams)
            {
                list.Add(new TeamData
                    {
                        LeagueID = team.LeagueID,
                        LeagueName = leagues[team.LeagueID].Name,
                        TeamID = team.ID,
                        TeamName = team.Name,
                        TeamType = team.TeamType
                    });
            }
            return list;
        }
    }
}
