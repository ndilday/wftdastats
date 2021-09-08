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
        const string Rinxter_Url = "http://stats-repo.wftda.com/rx/ds";
        //const string Rinxter_Url = "http://rinxter-test.cloudapp.net/rx/ds";

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
            
            string parameters = "?type=boutList&tournamentId={0}&output=tab";
            int[] tournamentList = { 46, 47, 48, 49 };
            List<int> boutIdList = new List<int>(102);
            List<int> teamIdList = new List<int>(16);
            List<int> boutIgnoreList = new List<int>();
            teamIdList.Add(44); //Angel
            teamIdList.Add(64); //Arch
            teamIdList.Add(92); //Boston
            teamIdList.Add(94); //Crime
            teamIdList.Add(34); //Denver
            teamIdList.Add(33); //Gotham
            //teamIdList.Add(97); //Helsinki
            teamIdList.Add(40); //London
            //teamIdList.Add(61); //Mad
            teamIdList.Add(67); //Minnesota
            teamIdList.Add(106);    //Montreal
            //teamIdList.Add(37); //Philly
            teamIdList.Add(42); //Rat
            teamIdList.Add(157);    //Rose
            //teamIdList.Add(38); //Rocky
            //teamIdList.Add(105);    //Terminal
            teamIdList.Add(35); //Texas
            //teamIdList.Add(211);    //Tri-City
            teamIdList.Add(108);    //Victorian
            //teamIdList.Add(62); //Windy
            //teamIdList.Add(235);    //Calgary
            //teamIdList.Add(93); //Charlottesville
            //teamIdList.Add(188);    //Houston
            //teamIdList.Add(284);    //2x4


            /*boutIgnoreList.Add(1235);
            boutIgnoreList.Add(1226);
            boutIgnoreList.Add(1231);
            boutIgnoreList.Add(1228);
            boutIgnoreList.Add(1225);
            boutIgnoreList.Add(1218);
            boutIgnoreList.Add(1214);
            boutIgnoreList.Add(1219);
            boutIgnoreList.Add(1221);
            boutIgnoreList.Add(1217);
            boutIgnoreList.Add(1209);
            boutIgnoreList.Add(1211);*/

            // TEMPORARY REMOVALS

            foreach (int tournamentId in tournamentList)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(Rinxter_Url);

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
            string url = "http://rinxter-test.cloudapp.net/rx/xl?command=exportBoutXLStatsBook&boutId=" + boutId.ToString();
            string path = "C:\\derby\\statbooks\\rinxter\\" + boutId.ToString() + ".xlsx";
            if (!File.Exists(path))
            {
                new WebClient().DownloadFile(url, path);
            }
            return path;
        }

        public RinxterScoresModel GetRinxterScoringData(int rinxterBoutId)
        {
            const string parameters = "?type=boutScores&boutId={0}&output=tab";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Rinxter_Url);

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
