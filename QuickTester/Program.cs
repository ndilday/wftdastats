using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DerbyCalculators;
using DerbyDataModels;
using StatbookReader;
using StatbookReader.Models;

namespace QuickTester
{
    class Program
    {
        static void Main(string[] args)
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;
            //RinxterDataImporter importer = new RinxterDataImporter();
            //importer.Import(connString, false);
            
            //string basicConnString = ConfigurationManager.ConnectionStrings["basicderby"].ConnectionString;
            //BasicProcessStatsheetDirectory(basicConnString, args[0]);
            ProcessStatsheetDirectory(connString, args[0], true);
            SetUpCalculatedTables(connString);
            CreatePlayerPerformanceCsv(connString);
            /*int iterations = 10;
            var foo = new PlayerPerformanceCalculator(connString).GetAllPlayerPointPerformances(iterations);
            var sorted = foo[0].Keys.OrderBy(k => k);
            StreamWriter output = new StreamWriter("e:\\projects\\apvm-blocker.csv");
            foreach(int playerID in sorted)
            {
                if(foo[0][playerID].BlockerPerformance == null || foo[0][playerID].BlockerPerformance.TotalJamPortions == 0) continue;

                string line = playerID.ToString();
                
                for(int i = 0; i < iterations; i++)
                {
                    var pp = foo[i][playerID];
                    double apvm = pp.BlockerPerformance.TotalPointsVersusMedian / pp.BlockerPerformance.TotalJamPortions;
                    line += "," + apvm;
                }
                output.WriteLine(line);
            }
            output.Close();*/
        }

        static void ProcessStatsheetDirectory(string connString, string directoryPath, bool assumeATeams)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine(directoryPath + " does not exist!");
            }
            else
            {
                Stopwatch timer = new Stopwatch();
                DerbyDataImporter importer = new DerbyDataImporter();
                foreach (string path in Directory.GetFiles(directoryPath, "*.xlsx"))
                {
                    Console.WriteLine("--------------------");
                    Console.WriteLine("Processing " + path);
                    timer.Restart();
                    StatbookModel model = StatbookReader.StatbookReader.ReadStatbook(path);
                    importer.Import(connString, model, assumeATeams);
                    timer.Stop();
                    Console.WriteLine("Finished Processing " + path + ": " + timer.Elapsed.TotalSeconds);
                }
            }
        }

        static void BasicProcessStatsheetDirectory(string connString, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine(directoryPath + " does not exist!");
            }
            else
            {
                Stopwatch timer = new Stopwatch();
                BasicDataImporter importer = new BasicDataImporter();
                foreach (string path in Directory.GetFiles(directoryPath, "*.xlsx"))
                {
                    Console.WriteLine("--------------------");
                    Console.WriteLine("BASIC PROCESS: " + path);
                    timer.Restart();
                    StatbookModel model = StatbookReader.StatbookReader.ReadStatbook(path);
                    importer.Import(connString, model);
                    timer.Stop();
                    Console.WriteLine("Finished BASIC PROCESS " + path + ": " + timer.Elapsed.TotalSeconds);
                }
            }
        }

        private static void CreatePlayerPerformanceCsv(string connString)
        {
            var ppc = new PlayerPerformanceCalculator(connString);
            var ppcList = ppc.GetPlayerPointPerformancesForTeam(24);
            List<List<object>> ratingOverTime = new List<List<object>>();
            /*foreach (PlayerPerformance pp in ppcList)
            {
                List<object> list = new List<object>();
                ratingOverTime.Add(list);
                //list.Add(pp.Player.Name);
                //int count = 0;

                foreach(BoutPerformance bp in pp.Bouts)
                {
                    foreach(JamPerformance jp in bp.Jams)
                    {
                        
                    }
                }
            }*/
        }

        private static void SetUpCalculatedTables(string connString)
        {
            Stopwatch timer = new Stopwatch();
            //new PlayerTrueSkillCalculator(connString).CalculateTrueSkills();
            Console.WriteLine("Calculating Durations");
            timer.Restart();
            new DurationEstimatesCalculator(connString).CalculateDurationEstimates();
            timer.Stop();
            Console.WriteLine("Finished Calculating Durations: " + timer.Elapsed.TotalSeconds);
            
            Console.WriteLine("Calculating Situational Scores");
            timer.Restart();
            IList<JamTeamData> jamData;
            var sss = new SituationalScoreCalculator(connString).CalculateSituationalScores(out jamData);
            timer.Stop();
            Console.WriteLine("Finished Calculating SituationalScores: " + timer.Elapsed.TotalSeconds);
            
            Console.WriteLine("Calculating Secondary Tables");
            timer.Restart();
            new BoutDataCalculator(connString, sss, jamData).CalculateSecondaryTables();
            timer.Stop();
            Console.WriteLine("Finished Calculating Secondary Tables: " + timer.Elapsed.TotalSeconds);
        }
    }
}
