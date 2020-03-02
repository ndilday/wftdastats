using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace StatsSiteReader
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
            /*foreach(string teamUrl in teamUrls)
            {
                ProcessTeamPage(teamUrl);
            }*/
            GenerateCsvFromMap("E:\\Projects\\playoffs.csv");
        }

        private void GenerateCsvFromMap(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(file);
            foreach (KeyValuePair<string, float> kvp in teamRankingScoreMap)
            {
                string line = string.Format("{0},{1}", kvp.Key, kvp.Value);
                sw.WriteLine(line);
            }
            sw.Flush();
            file.Close();
        }

        private void ProcessTeamPage(string teamUrl)
        {
            Task<HtmlNode> task = GetTeamPage(teamUrl);
            task.Wait();
            float multiplier = 0;
            float games = 0;
            float runningTotal = 0;
            string leagueName = task.Result.SelectSingleNode("//div[@class=\"leagueMainStatsInner\"]/h1").InnerHtml.Trim();
            foreach (HtmlNode div in task.Result.SelectNodes("./div"))
            {
                var classes = div.GetClasses();
                if (classes.Contains("gameRow--gameDate"))
                {
                    var gameDates = Convert.ToDateTime(div.SelectSingleNode("./div/div[@class=\"col gameDateRow\"]").InnerHtml.Trim());
                    if ((gameDates.Year < DateTime.Now.Year - 1) || (gameDates.Year == DateTime.Now.Year - 1 && gameDates.Month < 7))
                    {
                        multiplier = 0;
                    }
                    else if (gameDates.Year == DateTime.Now.Year - 1)
                    {
                        multiplier = 0.5F;
                    }
                    else if (gameDates.Year == DateTime.Now.Year)
                    {
                        multiplier = 1;
                    }
                }
                else if (multiplier == 0)
                {
                    continue;
                }
                else if (classes.Contains("resultsForDate"))
                {
                    var results = div.SelectNodes("//a[@class=\"gameRow resultRow\"]");
                    foreach (var row in results)
                    {
                        var pointsData = row.SelectSingleNode("./div[@class=\"gameRow--left resultRow--left\"]/div[@class=\"gameRow--segment\"]//span[@class=\"gameRow--gamePoints\"]");
                        runningTotal += Convert.ToSingle(pointsData.InnerHtml.Trim().Replace("GP", ""));
                        games += multiplier;
                    }
                }
            }
            teamRankingScoreMap[leagueName] = runningTotal / games;
        }

        private async Task<HtmlNode> GetTeamPage(string teamUrl)
        {
            using (var response = await _httpClient.GetAsync(teamUrl))
            {
                using (var content = response.Content)
                {
                    // read answer in non-blocking way
                    string result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    return document.DocumentNode.SelectSingleNode("//section[@class=\"recentResults segment responsive gamesAndResultsOnLeagues\"]");
                }
            }
        }

        private List<string> GetLiveTopTeams()
        {
            List<string> teamUrls = new List<string>();
            Task<List<string>> task = GetTeamUrls(80);
            task.Wait();
            return task.Result;
        }

        private async Task<List<string>> GetTeamUrls(int teamCount)
        {
            using (var response = await _httpClient.GetAsync(STATS_WFTDA_RANKINGS_LIVE))
            {
                using (var content = response.Content)
                {
                    // read answer in non-blocking way
                    string result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    return ProcessRankingsHtml(document, teamCount);
                }
            }
        }

        private List<string> ProcessRankingsHtml(HtmlDocument document, int teamCount)
        {
            List<string> teamUrls = new List<string>();
            var tableBody = document.DocumentNode
                .SelectSingleNode("//table[@class=\"rankingsTable\"]/tbody");
            var rows = tableBody.SelectNodes("tr");
            for (int i = 0; i < teamCount; i++)
            {
                string teamUrl = rows[i].SelectSingleNode("td[@class=\"rankingsTable--leagueTitleColumn\"]").SelectSingleNode("a").GetAttributeValue("href", null);

                if (teamUrl != null)
                {
                    teamUrls.Add(teamUrl);
                }
            }
            return teamUrls;
        }

    }
}
