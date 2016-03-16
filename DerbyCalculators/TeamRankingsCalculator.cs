using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using HtmlAgilityPack;

using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    public class TeamRankingsCalculator
    {
        private const string s_ftsUrl = "http://flattrackstats.com/rankings";
        private const string s_wftdaUrl = "http://wftda.com/rankings";
        private string _connectionString;

        public TeamRankingsCalculator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<TeamRating> GetTeamRatings()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            // attempt to get the rankings data from the db
            TeamRatingGateway trGateway = new TeamRatingGateway(connection, transaction);
            TeamGateway teamGateway = new TeamGateway(connection, transaction);
            LeagueGateway leagueGateway = new LeagueGateway(connection, transaction);
            var teamRankings = trGateway.GetCurrentTeamRatings();
            if (!teamRankings.Any() || DateTime.Now.Subtract(teamRankings[0].AddedDate).TotalDays > 30 )
            {
                var teams = teamGateway.GetAllWftdaTeams();
                var leagues = leagueGateway.GetAllLeagues();
                var teamMapper = new TeamMapperGateway(connection, transaction).GetAllTeamMappers();
                var wftdaData = GetWftdaRankingsData();
                var ftsData = GetFtsRankingsData();
                teamRankings = new List<TeamRating>(250);
                var leftOut = new List<WftdaRankingData>();
                foreach(WftdaRankingData wftda in wftdaData)
                {
                    var fts = ftsData.FirstOrDefault(f => string.Equals(wftda.TeamName.Substring(0, 10), f.TeamName.Substring(0, 10), StringComparison.OrdinalIgnoreCase));
                    if(fts != null)
                    {
                        teamRankings.Add(new TeamRating
                            {
                                FtsRank = fts.Rank,
                                FtsScore = fts.Rating,
                                TeamID = 0,
                                TeamName = wftda.TeamName,
                                WftdaRank = wftda.Rank,
                                WftdaScore = wftda.RatingScore,
                                WftdaStrength = wftda.Strength
                            });
                    }
                    else
                    {
                        // try the team mapper?
                        TeamMapper map = teamMapper.FirstOrDefault(tm => string.Equals(tm.TeamSpelling, wftda.TeamName, StringComparison.OrdinalIgnoreCase));
                        if(map != null)
                        {
                            var otherMaps = teamMapper.Where(tm => tm.TeamID == map.TeamID).Select(tm => tm.TeamSpelling);
                            fts = ftsData.FirstOrDefault(f => otherMaps.Contains(f.TeamName));
                            if (fts != null)
                            {
                                teamRankings.Add(new TeamRating
                                {
                                    FtsRank = fts.Rank,
                                    FtsScore = fts.Rating,
                                    TeamID = map.TeamID,
                                    TeamName = wftda.TeamName,
                                    WftdaRank = wftda.Rank,
                                    WftdaScore = wftda.RatingScore,
                                    WftdaStrength = wftda.Strength
                                });
                            }
                            else
                            {
                                leftOut.Add(wftda);
                            }
                        }
                        
                    }
                }

                List<TeamRating> leftOvers = new List<TeamRating>();
                foreach(TeamRating teamRating in teamRankings)
                {
                    if (teamRating.TeamID > 0) continue;
                    var team = teams.FirstOrDefault(t => string.Equals(t.Name,teamRating.TeamName, StringComparison.OrdinalIgnoreCase));
                    if(team != null)
                    {
                        teamRating.TeamID = team.ID;
                    }
                    else
                    {
                        var league = leagues.FirstOrDefault(l => string.Equals(l.Name, teamRating.TeamName, StringComparison.OrdinalIgnoreCase));
                        if (league != null)
                        {
                            team = teams.First(t => t.LeagueID == league.ID);
                            teamRating.TeamID = team.ID;
                        }
                        else
                        {
                            // try the team mapper?
                            TeamMapper map = teamMapper.FirstOrDefault(tm => string.Equals(tm.TeamSpelling, teamRating.TeamName, StringComparison.OrdinalIgnoreCase));
                            if (map != null)
                            {
                                teamRating.TeamID = map.TeamID;
                            }
                            // TODO: else, create the League and the team? The nature of leagueID makes that tough...
                        }
                    }
                }
                trGateway.InsertTeamRatings(teamRankings.Where(tr => tr.TeamID != 0).ToList());
            }
            transaction.Commit();
            connection.Close();
            return teamRankings;
        }

        private List<FtsRankingData> GetFtsRankingsData()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(s_ftsUrl);
            // get the first table from the page
            HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//td[@class='rankingscontainer rightflush']/table/tbody/tr");
            return ProcessFtsRows(rows);
        }

        private List<FtsRankingData> ProcessFtsRows(HtmlNodeCollection rows)
        {
            List<FtsRankingData> dataList = new List<FtsRankingData>(300);
            foreach (HtmlNode row in rows)
            {
                string name;
                var tds = row.ChildNodes.Where(n => n.Name == "td").ToList();
                var nameNode = tds[2].ChildNodes.First().ChildNodes.First();
                name = nameNode.GetAttributeValue("title", null);
                if(name == null)
                {
                    name = nameNode.InnerText;
                }
                FtsRankingData data = new FtsRankingData
                {
                    Rank = Convert.ToInt32(tds[0].InnerText.TrimEnd('.')),
                    TeamName = name,
                    Rating = Convert.ToDouble(tds[3].InnerText)
                };
                dataList.Add(data);
            }
            return dataList;
        }

        private List<WftdaRankingData> GetWftdaRankingsData()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(s_wftdaUrl);
            // get the first table from the page
            HtmlNode containingDiv = doc.DocumentNode.SelectSingleNode("//div[@id='pageContent']/div[@class='markdown'][1]");
            DateTime rankingsDate = Convert.ToDateTime(containingDiv.SelectSingleNode("//h2[1]").InnerText);
            HtmlNodeCollection rows = containingDiv.SelectNodes("//table[1]/tbody/tr");
            return ProcessWftdaRows(rows.Skip(1));
        }

        private List<WftdaRankingData> ProcessWftdaRows(IEnumerable<HtmlNode> rows)
        {
            List<WftdaRankingData> dataList = new List<WftdaRankingData>(300);
            foreach(HtmlNode row in rows)
            {
                var tds = row.ChildNodes.Where(n => n.Name == "td").ToList();
                WftdaRankingData data = new WftdaRankingData
                {
                    Rank = Convert.ToInt32(tds[0].ChildNodes.First(n => n.Name == "div").InnerText),
                    TeamName = tds[2].InnerText,
                    Wins = Convert.ToInt32(tds[3].ChildNodes.First(n => n.Name == "div").InnerText),
                    Losses = Convert.ToInt32(tds[4].ChildNodes.First(n => n.Name == "div").InnerText),
                    Strength = Convert.ToDouble(tds[5].ChildNodes.First(n => n.Name == "div").InnerText),
                    RatingScore = Convert.ToDouble(tds[6].ChildNodes.First(n => n.Name == "div").InnerText)
                };
                dataList.Add(data);
            }
            return dataList;
        }
    }
}
