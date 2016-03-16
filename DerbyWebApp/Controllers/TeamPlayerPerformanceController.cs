using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

using DerbyCalculators;
using DerbyDataModels;

namespace WftdaStatsWebApp.Controllers
{
    [RoutePrefix("api/teamPlayerPerformance")]
    public class TeamPlayerPerformanceController : ApiController
    {
        [Route("points/{id:int}")]
        public IList<PlayerPerformance> GetPlayerPointPerformanceForTeam(int id)
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;
            return new PlayerPerformanceCalculator(connString).GetPlayerPointPerformancesForTeam(id);
        }

        [Route("value/{id:int}")]
        public IList<PlayerPerformance> GetPlayerValuePerformanceForTeam(int id)
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;
            return new PlayerPerformanceCalculator(connString).GetPlayerValuePerformancesForTeam(id);
        }
    }
}
