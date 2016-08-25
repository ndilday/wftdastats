using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using StatbookReader.Models;
using StatbookReader.Models.Rinxter;

namespace StatbookReader
{
    public class RinxterDataImporter
    {
        //private string _statbookDownloadDirectory;
        public void Import(string connectionString, bool assumeATeams)
        {
            // figure out bouts to import
            List<int> rinxterBoutIdList = GetRinxterBoutList();

            foreach(int rinxterBoutId in rinxterBoutIdList)
            {
                StatbookModel statbookModel = null;
                Task t1 = Task.Run(() => {
                    // download statbook
                    string path = DownloadRinxterStatbook(rinxterBoutId);
                    // process statbook via the IGRF translator
                    statbookModel = StatbookReader.ReadStatbook(path);
                });

                List<JamScoreModel> jamScoreModel = null;
                // fire off thread to get real scoring from rinxter
                Task t2 = Task.Run(() => {
                    RinxterScoresModel rinxterModel = GetRinxterScoringData(rinxterBoutId);
                    jamScoreModel = TranslateRinxterScoringData(rinxterModel);
                });

                // wait for both processes to complete
                Task.WaitAll(t1, t2);
                
                // delete scoring data from statbook object
                IntegrateStarPasses(statbookModel, jamScoreModel);
                // inject translated rinxter stat data
                 Console.WriteLine("Processing " + rinxterBoutId.ToString());
                new DerbyDataImporter().Import(connectionString, statbookModel, assumeATeams);
                Console.WriteLine(" Finished processing " + rinxterBoutId.ToString());
                Console.WriteLine("====================");
            }
        }

        private void IntegrateStarPasses(StatbookModel statbookModel, List<JamScoreModel> jamScoreModelList)
        {
            if(statbookModel.Scores.Count != jamScoreModelList.Count)
            {
                // this is unexpected
                throw new FormatException("score list length mismatch");
            }
            for(int i = 0; i < jamScoreModelList.Count; i++)
            {
                if(jamScoreModelList[i].AwayStarPass != null)
                {
                    jamScoreModelList[i].AwayStarPass.PlayerNumber = 
                        statbookModel.Lineups[i].AwayLineup.First(l => l.IsPivot).PlayerNumber;
                    statbookModel.Scores[i].AwayJammer.JamTotal = jamScoreModelList[i].AwayJammer.JamTotal;
                    statbookModel.Scores[i].AwayStarPass = jamScoreModelList[i].AwayStarPass;
                }
                if (jamScoreModelList[i].HomeStarPass != null)
                {
                    jamScoreModelList[i].HomeStarPass.PlayerNumber =
                        statbookModel.Lineups[i].HomeLineup.First(l => l.IsPivot).PlayerNumber;
                    statbookModel.Scores[i].HomeJammer.JamTotal = jamScoreModelList[i].HomeJammer.JamTotal;
                    statbookModel.Scores[i].HomeStarPass = jamScoreModelList[i].HomeStarPass;
                }
            }
        }

        private List<int> GetRinxterBoutList()
        {
            string url = "http://stats-repo.wftda.com/rx/ds";
            string parameters = "?type=boutList&tournamentId={0}&output=tab";
            // 25, 26, 27
            int[] tournamentList = { 25, 26, 27 };
            List<int> boutIdList = new List<int>(102);
            List<int> teamIdList = new List<int>(16);
            List<int> boutIgnoreList = new List<int>();

            boutIgnoreList.Add(785);
            boutIgnoreList.Add(777);
            boutIgnoreList.Add(786);
            boutIgnoreList.Add(783);
            boutIgnoreList.Add(782);
            boutIgnoreList.Add(775);
            boutIgnoreList.Add(773);
            boutIgnoreList.Add(768);
            boutIgnoreList.Add(758);
            boutIgnoreList.Add(761);
            boutIgnoreList.Add(772);
            boutIgnoreList.Add(745);
            boutIgnoreList.Add(751);
            boutIgnoreList.Add(789);
            boutIgnoreList.Add(760);
            boutIgnoreList.Add(766);
            boutIgnoreList.Add(762);
            boutIgnoreList.Add(752);
            boutIgnoreList.Add(748);
            boutIgnoreList.Add(820);
            boutIgnoreList.Add(815);
            boutIgnoreList.Add(816);
            boutIgnoreList.Add(813);
            boutIgnoreList.Add(822);
            boutIgnoreList.Add(817);
            boutIgnoreList.Add(804);
            boutIgnoreList.Add(801);
            boutIgnoreList.Add(796);
            boutIgnoreList.Add(805);
            boutIgnoreList.Add(810);
            boutIgnoreList.Add(790);

            // TEMPORARY REMOVALS
            boutIgnoreList.Add(807);

            foreach (int tournamentId in tournamentList)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(url);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // List data response.
                HttpResponseMessage response = client.GetAsync(string.Format(parameters, tournamentId)).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    var tournamentModel = response.Content.ReadAsAsync<RinxterTournamentModel>().Result;
                    string parameters2 = "?type=bout&boutId={0}&output=obj";
                    foreach(RinxterBoutModel model in tournamentModel.rows)
                    {
                        if (boutIgnoreList.Contains(model.id)) continue;
                        response = client.GetAsync(string.Format(parameters2, model.id)).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var boutData = response.Content.ReadAsAsync<RinxterBoutData[]>().Result;
                            boutIdList.Add(model.id);
                            /*if (teamIdList.Contains(boutData[0].team1Id) || teamIdList.Contains(boutData[0].team2Id))
                            {
                                boutIdList.Add(model.id);
                            }*/
                        }
                    }
                }
            }
            return boutIdList;
        }

        private List<JamScoreModel> TranslateRinxterScoringData(RinxterScoresModel rinxterModel)
        {
            List<JamScoreModel> modelList = new List<JamScoreModel>();
            foreach(RinxterScoreRowModel row in rinxterModel.rows)
            {
                bool homeSP = row.data[3].ToString().Contains("*");
                string[] homeScores = row.data[3].ToString().Split(' ');
                int homeJammerScore = 0;
                int homePivotScore = 0;
                
                int processLimit = homeScores.Length - 1;
                for(int i = 0; i < processLimit; i++)
                {
                    if(homeScores[i].Contains("*"))
                    {
                        homeScores[i] = homeScores[i].Substring(0, homeScores[i].Length - 1);
                        homePivotScore += Convert.ToInt16(homeScores[i]);
                    }
                    else
                    {
                        homeJammerScore += Convert.ToInt16(homeScores[i]);
                    }

                }

                bool awaySP = row.data[8].ToString().Contains("*");
                string[] awayScores = row.data[8].ToString().Split(' ');
                int awayJammerScore = 0;
                int awayPivotScore = 0;
                processLimit = awayScores.Length - 1;
                for (int i = 0; i < processLimit; i++)
                {
                    if (awayScores[i].Contains("*"))
                    {
                        awayScores[i] = awayScores[i].Substring(0, awayScores[i].Length - 1);
                        awayPivotScore += Convert.ToInt16(awayScores[i]);
                    }
                    else
                    {
                        awayJammerScore += Convert.ToInt16(awayScores[i]);
                    }

                }

                JamScoreModel jamScoreModel = new JamScoreModel
                {
                    IsFirstHalf = true,
                    JamNumber = row.id + 1,
                    AwayJammer = new ScoreModel
                    {
                        JamTotal = awayJammerScore,
                    },
                    HomeJammer = new ScoreModel 
                    {
                        JamTotal = homeJammerScore
                    }
                };
                if(homeSP)
                {
                    jamScoreModel.HomeStarPass = new ScoreModel
                    {
                        JamTotal = homePivotScore
                    };
                }
                if(awaySP)
                {
                    jamScoreModel.AwayStarPass = new ScoreModel
                    {
                        JamTotal = awayPivotScore
                    };
                }
                modelList.Add(jamScoreModel);
            }
            return modelList;
        }

        private string DownloadRinxterStatbook(int boutId)
        {
            string url = "http://stats-repo.wftda.com/rx/xl?command=exportBoutXLStatsBook&boutId=" + boutId.ToString();
            string path = "C:\\temp\\statbooks\\rinxter\\" + boutId.ToString() + ".xlsx";
            if (!File.Exists(path))
            {
                new WebClient().DownloadFile(url, path);
            }
            return path;
        }

        public RinxterScoresModel GetRinxterScoringData(int rinxterBoutId)
        {
            const string url = "http://stats-repo.wftda.com/rx/ds";
            const string parameters = "?type=boutScores&boutId={0}&output=tab";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(string.Format(parameters, rinxterBoutId)).Result;
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                return response.Content.ReadAsAsync<RinxterScoresModel>().Result;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                throw new HttpRequestException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
        }
    }
}
