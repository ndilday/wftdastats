using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace StatsReader
{
    public class TeamData
    {
        public string Name { get; set; }
        public float RankingScore { get; set; }
    }

    public class StatsScraper
    {
        private const string STATS_WFTDA_RANKINGS_LIVE = "https://stats.wftda.com/rankings-live";
        private static HttpClient _httpClient = new HttpClient();
        private ConcurrentDictionary<string, float> teamRankingScoreMap = new ConcurrentDictionary<string, float>();

        public void BuildPlayoffRankings()
        {
            var teamUrls = GetLiveTopTeams();
            Parallel.ForEach(teamUrls, ProcessTeamPage);
        }

        private void ProcessTeamPage(string teamUrl)
        {
            HtmlNode recentResultSection = null;
            Task task = GetTeamPage(teamUrl, recentResultSection);
            task.Wait();
            float multiplier = 0;
            float games = 0;
            float runningTotal = 0;
            foreach(HtmlNode div in recentResultSection.SelectNodes("//div"))
            {
                var classes = div.GetClasses();
                if (classes.Contains("gameRow--gameDate"))
                {
                    var gameDates = Convert.ToDateTime(div.SelectSingleNode("//div.gameDateRow").InnerHtml.Trim());
                    if((gameDates.Year < DateTime.Now.Year - 1) || (gameDates.Year == DateTime.Now.Year - 1 && gameDates.Month < 7))
                    {
                        multiplier = 0;
                    }
                    else if(gameDates.Year == DateTime.Now.Year - 1)
                    {
                        multiplier = 0.5F;
                    }
                    else if(gameDates.Year == DateTime.Now.Year)
                    {
                        multiplier = 1;
                    }
                }
                else if(multiplier == 0)
                {
                    continue;
                }
                else if (classes.Contains("resultsForDate"))
                {
                    var results = div.SelectNodes("//a.resultRow");
                    foreach(var row in results)
                    {
                        var pointsData = div.SelectSingleNode("/div.resultRow--left/div.gameRow--segment[0]/div.resultRow--leagueTitle/span.gameRow--gamePoints");
                        runningTotal += Convert.ToSingle(pointsData.InnerHtml.Trim().Replace("GP", ""));
                        games += multiplier;
                    }
                }
            }
        }

        private async Task GetTeamPage(string teamUrl, HtmlNode recentResultSection)
        {
            using (var response = await _httpClient.GetAsync(teamUrl))
            {
                using (var content = response.Content)
                {
                    // read answer in non-blocking way
                    string result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    recentResultSection = document.DocumentNode
                        .SelectSingleNode("//section.recentResults");
                }
            }
        }

        private List<string> GetLiveTopTeams()
        {
            List<string> teamUrls = null;
            Task task = GetTeamUrls(teamUrls, 80);
            task.Wait();
            return teamUrls; 
        }

        private async Task GetTeamUrls(List<string> output, int teamCount)
        {
            using (var response = await _httpClient.GetAsync(STATS_WFTDA_RANKINGS_LIVE))
            {
                using (var content = response.Content)
                {
                    // read answer in non-blocking way
                    string result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    output = ProcessRankingsHtml(document, teamCount);
                }
            }
        }

        private List<string> ProcessRankingsHtml(HtmlDocument document, int teamCount)
        {
            List<string> teamUrls = new List<string>();
            var tableBody = document.DocumentNode
                .SelectSingleNode("//table.rankingsTable/tbody");
            var rows = tableBody.SelectNodes("tr");
            for(int i = 0; i < teamCount; i++)
            {
                string teamUrl = rows[i].SelectSingleNode("td.rankingsTable--leagueTitleColumn").SelectSingleNode("a").GetAttributeValue("href", null);

                if (teamUrl != null)
                {
                    teamUrls.Add(teamUrl);
                }
                i++;
            }
            return teamUrls;
        }

    }
}
