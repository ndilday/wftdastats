using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

using DerbyCalculators;
using DerbyDataModels;

namespace WftdaStatsWebApp.Controllers
{
    [RoutePrefix("api/teamRatings")]
    public class TeamRatingController : ApiController
    {
        [Route("")]
        public IList<TeamRating> GetAllTeamRatings()
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;
            return new TeamRankingsCalculator(connString).GetTeamRatings();
        }

        [Route("{id:int}")]
        public TeamRating GetCurrentTeamRating(int id)
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;
            return new TeamRankingsCalculator(connString).GetTeamRatings().First(tr => tr.TeamID == id);
        }
    }
}
