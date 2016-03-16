using System.Configuration;
using System.Web.Http;

using DerbyCalculators;

namespace WftdaStatsWebApp.Controllers
{
    [RoutePrefix("api/players")]
    public class PlayerController : ApiController
    {
        [Route("")]
        public dynamic GetAllPlayers()
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;
            return new { data = new PlayerCalculator(connString).GetAllPlayers() };
        }
    }
}
