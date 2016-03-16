using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DerbyCalculators;
using DerbyCalculators.Models;

namespace DerbyWebApp.Controllers
{
    [RoutePrefix("api/teams")]
    public class TeamController : ApiController
    {
        [Route("")]
        public IList<TeamData> GetTeams()
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;
            return new TeamDataCalculator(connString).GetTeamData();
        }

        [Route("{id:int}")]
        public TeamData GetTeam(int teamID)
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;
            return new TeamDataCalculator(connString).GetTeamData().FirstOrDefault(t => t.TeamID == teamID);
        }
    }
}
