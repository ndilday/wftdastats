﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using DerbyDataModels;
using HtmlAgilityPack;

namespace FTSReader
{
    public enum TeamLevel
    {
        Unknown = 0,
        A = 1,
        B = 2,
        C = 3,
        HT = 4
    }

    public class TeamData
    {
        public int id;
        public string name;
        public string town;
        public TeamLevel level;
        public double rankingScore;
    }

    public class FTSScraper
    {
        private const string FTS_WFTDA_TEAM_LIST = "http://flattrackstats.com/teams/results/wftda?page={0}";
        private const string FTS_B_TEAM_LIST = "http://flattrackstats.com/teams/results/b-team?page={0}";
        private const string FTS_WFTDA_RANKING = "http://flattrackstats.com/rankings";
        private const string FTS_NA_RANKING = "http://flattrackstats.com/rankings/northamerica";
        private static HttpClient _httpClient = new HttpClient();
        private ConcurrentDictionary<int, TeamData> teamDataMap = new ConcurrentDictionary<int, TeamData>();

        public void PopulateMap()
        {
            // get team data
            Task task = ReadWftdaTeamList();
            Task task2 = ReadBTeamList();
            task2.Wait();
            task.Wait();
            // get ranking data
            task = ReadNorthAmericanRankings();
            task.Wait();
            GenerateCsvFromMap("C:\\Derby\\DB\\fts.csv");
        }

        private void GenerateCsvFromMap(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(file);
            foreach(KeyValuePair<int, TeamData> kvp in teamDataMap)
            {
                if (kvp.Value.rankingScore != 0)
                {
                    string line = string.Format("{0},{1},{2},{3},{4}", kvp.Value.id, kvp.Value.name, kvp.Value.town.Replace(',', ' '), kvp.Value.level, kvp.Value.rankingScore);
                    sw.WriteLine(line);
                }
            }
            sw.Flush();
            file.Close();
        }

        private async Task ReadWftdaRankings()
        {
            using (var response = await _httpClient.GetAsync(FTS_WFTDA_RANKING))
            {
                using (var content = response.Content)
                {
                    // read answer in non-blocking way
                    string result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    ProcessRankingsHtml(document);
                }
            }
        }

        private async Task ReadNorthAmericanRankings()
        {
            using (var response = await _httpClient.GetAsync(FTS_NA_RANKING))
            {
                using (var content = response.Content)
                {
                    // read answer in non-blocking way
                    string result = await content.ReadAsStringAsync();
                    var document = new HtmlDocument();
                    document.LoadHtml(result);
                    ProcessRankingsHtml(document);
                }
            }
        }

        private void ProcessRankingsHtml(HtmlDocument document)
        {
            var body = document.DocumentNode
                .SelectSingleNode("//td[@class='rankingscontainer rightflush']/table/tbody");
            foreach (var row in body.SelectNodes("tr"))
            {
                // nid on the first td is the team id
                int teamFtsId = Convert.ToInt32(row.SelectSingleNode("td[1]").GetAttributeValue("nid", "0"));

                // fourth td has team ranking score as its innerhtml
                double teamScore = Convert.ToDouble(row.SelectSingleNode("td[4]").InnerHtml);
                if (teamDataMap.ContainsKey(teamFtsId) && teamDataMap[teamFtsId].rankingScore == 0)
                {
                    teamDataMap[teamFtsId].rankingScore = teamScore;
                }
            }
        }

        private async Task ReadWftdaTeamList()
        {
            for(int i = 0; i < 5; i++)
            {
                string url = string.Format(FTS_WFTDA_TEAM_LIST, i);
                using (var response = await _httpClient.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        // read answer in non-blocking way
                        string result = await content.ReadAsStringAsync();
                        var document = new HtmlDocument();
                        document.LoadHtml(result);
                        ProcessTeamListHtml(document, TeamLevel.A);
                    }
                }
            }
        }

        private async Task ReadBTeamList()
        {
            for (int i = 0; i < 8; i++)
            {
                string url = string.Format(FTS_B_TEAM_LIST, i);
                using (var response = await _httpClient.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        // read answer in non-blocking way
                        string result = await content.ReadAsStringAsync();
                        var document = new HtmlDocument();
                        document.LoadHtml(result);
                        ProcessTeamListHtml(document, TeamLevel.B);
                    }
                }
            }
        }

        private void ProcessTeamListHtml(HtmlDocument document, TeamLevel level = TeamLevel.Unknown)
        {
            try
            {
                var body = document.DocumentNode
                    .SelectSingleNode("//div[@class='view-content']/table/tbody");
                var rows = body.SelectNodes("tr");
                Console.WriteLine(rows.Count);
                foreach (var row in rows)
                {
                    // second td is league name
                    var anchor = row.SelectSingleNode("td[2]/div/a");
                    string leagueName = anchor.InnerHtml.Trim();
                    int leagueFtsId = Convert.ToInt32(anchor.GetAttributeValue("href", "").Split('/')[2]);
                    // third td is city
                    string homeTown = row.SelectSingleNode("td[3]").InnerHtml.Trim();
                    // fourth td is team type
                    teamDataMap[leagueFtsId] = new TeamData
                    {
                        id = leagueFtsId,
                        name = leagueName,
                        level = level,
                        town = homeTown,
                        rankingScore = 0
                    };
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private async Task<HttpResponseMessage> RetrievePage()
        {
            var client = new HttpClient();
            var result = await client.GetAsync(FTS_WFTDA_TEAM_LIST);
            return result;
        }
    }
}
