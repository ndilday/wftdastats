﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DerbyCalculators;
using DerbyDataModels;
using FTSReader;
using StatbookReader;
using StatbookReader.Models;
using StatsSiteReader;

namespace QuickTester
{
    class Program
    {
        static void Main(string[] args)
        {
            string connString = ConfigurationManager.ConnectionStrings["derby"].ConnectionString;

            /*
            FTSScraper scraper = new FTSScraper();
            scraper.PopulateMap();
            */

            //StatsScraper statsScraper = new StatsScraper();
            //statsScraper.BuildPlayoffRankings();

            RinxterDataImporter importer = new RinxterDataImporter();
            importer.Import(connString, true);

            //string basicConnString = ConfigurationManager.ConnectionStrings["basicderby"].ConnectionString;
            //BasicProcessStatsheetDirectory(basicConnString, args[0]);
            
            HashSet<int> years;
            ProcessStatsheetDirectory(connString, args[0], true, out years);
            SetUpCalculatedTables(connString, years);
            
        }

        static void ProcessStatsheetDirectory(string connString, string directoryPath, bool assumeATeams, out HashSet<int> years)
        {
            years = new HashSet<int>();
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
                    if(path.Contains('~'))
                    {
                        continue;
                    }
                    Console.WriteLine("--------------------");
                    Console.WriteLine("Processing " + path);
                    timer.Restart();
                    StatbookModel model = StatbookReader.StatbookReader.ReadStatbook(path);
                    years.Add(model.Date.Year);
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

        private static void SetUpCalculatedTables(string connString, HashSet<int> years)
        {
            Stopwatch timer = new Stopwatch();
            //new PlayerTrueSkillCalculator(connString).CalculateTrueSkills();
            Console.WriteLine("Calculating Durations");
            timer.Restart();
            new DurationEstimatesCalculator(connString).CalculateNewEstimates();
            timer.Stop();
            Console.WriteLine("Finished Calculating Durations: " + timer.Elapsed.TotalSeconds);
            
            Console.WriteLine("Calculating Annual Data");
            
            foreach (int year in years)
            {
                Console.WriteLine(year);
                Console.WriteLine("Calculating SituationalScores");
                timer.Restart();
                var sss = new SituationalScoreCalculator(connString).CalculateSituationalScores(year, out IList<JamTeamData> jamTeamData, out Dictionary<int, JamData> jamDataMap);
                timer.Stop();
                Console.WriteLine("Finished Calculating SituationalScores: " + timer.Elapsed.TotalSeconds);
                Console.WriteLine("Calculating Secondary Tables");
                timer.Restart();
                new BoutDataCalculator(connString, sss, jamTeamData, year).CalculateSecondaryTables();
                timer.Stop();
                Console.WriteLine("Finished Calculating Secondary Tables: " + timer.Elapsed.TotalSeconds);
            }
        }
    }
}
