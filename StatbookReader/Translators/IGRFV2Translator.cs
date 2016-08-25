using System;
using System.Collections.Generic;

using OfficeOpenXml;
using StatbookReader.Models;

namespace StatbookReader.Translators
{
    class IGRFV2Translator : BaseIGRFTranslator, ITranslator
    {
        public IGRFV2Translator()
        {
            /*
             * "B10", "B11", "H10", "H11", "B14", "H14", "B7",
                "A4", "AA4", "A46", "AA46",
                "A4:J43", "AC4:AL43", "P4:Y43", "AR4:BA43",
                "A4:Q41", "T4:AJ41", "A46:Q83", "T46:AJ83"
             */
            _cells = new StatbookCells
            {
                HomeLeagueCell = "B10",
                HomeTeamNameCell = "B11",
                HomeRosterCell = "B14",
                AwayLeagueCell = "H10",
                AwayTeamNameCell = "H11",
                AwayRosterCell = "H14",
                BoutDateCell = "B7",
                HomeLineupFirstPeriodCell = "A4",
                HomeLineupSecondPeriodCell = "A46",
                AwayLineupFirstPeriodCell = "AA4",
                AwayLineupSecondPeriodCell = "AA46",
                HomePenaltyFirstPeriodCell = "A4",
                HomePenaltySecondPeriodCell = "AC4",
                AwayPenaltyFirstPeriodCell = "P4",
                AwayPenaltySecondPeriodCell = "AR4",
                HomeScoreFirstPeriodCell = "A4",
                HomeScoreSecondPeriodCell = "A46",
                AwayScoreFirstPeriodCell = "T4",
                AwayScoreSecondPeriodCell = "T46",
            };
        }

        public StatbookModel Translate(ExcelWorkbook workbook)
        {
            return base.CreateStatbook(workbook);
        }

        protected override void ProcessPlayerBoxes(ExcelRange playerRange, PlayerLineupModel player, bool isSP)
        {
            int foulCol = 2;
            // check each foul box for this player in this jam
            while (foulCol < 5)
            {
                object foulMarkObj = playerRange.SubRange(1, foulCol).Value;
                if (foulMarkObj == null)
                {
                    break;
                }
                string foulMark = foulMarkObj.ToString().Trim();
                char? specialKey = null;
                if (foulMark.Length > 1)
                {
                    specialKey = foulMark[1];
                    foulMark = foulMark.Substring(0, 1);
                }
                BoxTimeModel boxTime;
                switch (foulMark.ToString().Trim())
                {
                    case "x":
                    case "X":
                        boxTime = new BoxTimeModel
                        {
                            Started = foulCol == 2 ? (bool?) null : false,
                            Exited = true,
                            IsJammer = player.IsJammer,
                            IsPivot = player.IsPivot,
                            IsFullService = foulCol == 2 ? (bool?)null : true,
                            SpecialKey = specialKey
                        };
                        player.BoxTimes.Add(boxTime);
                        break;
                    case "/":
                    case "\\":
                        boxTime = new BoxTimeModel
                        {
                            Started = false,
                            Exited = false,
                            IsJammer = player.IsJammer,
                            IsPivot = player.IsPivot,
                            IsFullService = false,
                            SpecialKey = specialKey
                        };
                        player.BoxTimes.Add(boxTime);
                        break;
                    case "s":
                    case "S":
                    case "i":
                    case "I":
                    case "|":
                    case "l":
                        boxTime = new BoxTimeModel
                        {
                            Started = true,
                            Exited = false,
                            IsJammer = player.IsJammer,
                            IsPivot = player.IsPivot,
                            IsFullService = false,
                            SpecialKey = specialKey
                        };
                        player.BoxTimes.Add(boxTime);
                        break;
                    case "$":
                        boxTime = new BoxTimeModel
                        {
                            Started = true,
                            Exited = true,
                            IsJammer = player.IsJammer,
                            IsPivot = player.IsPivot,
                            IsFullService = true,
                            SpecialKey = specialKey
                        };
                        player.BoxTimes.Add(boxTime);
                        break;
                    case "3":
                        player.WasInjured = true;
                        break;
                }
                foulCol++;
            }
            if(isSP)
            {
                foulCol = 2;
                if (player.IsJammer)
                {
                    foulCol += 4;
                }
                else if (player.IsPivot)
                {
                    foulCol -= 4;
                }
                int initialFoulCol = foulCol;
                int stop = foulCol + 3;
                BoxTimeModel lastBox = (player.BoxTimes.Count > 0) ? player.BoxTimes[player.BoxTimes.Count - 1] : null; 
                // check each foul box for this player in this jam
                while (foulCol < stop)
                {
                    object foulMarkObj = playerRange.SubRange(2, foulCol).Value;
                    if (foulMarkObj == null)
                    {
                        break;
                    }
                    BoxTimeModel boxTime;
                    string foulMark = foulMarkObj.ToString().Trim();
                    char? specialKey = null;
                    if (foulMark.Length > 1)
                    {
                        specialKey = foulMark[1];
                        foulMark = foulMark.Substring(0, 1);
                    }
                    switch (foulMark.ToString().Trim())
                    {
                        case "x":
                        case "X":
                            if (foulCol == initialFoulCol && lastBox != null && !lastBox.Exited)
                            {
                                lastBox.Exited = true;
                                if(lastBox.Started == false)
                                {
                                    lastBox.IsFullService = true;
                                }
                            }
                            else
                            {
                                boxTime = new BoxTimeModel
                                {
                                    Started = false,
                                    Exited = true,
                                    IsJammer = player.IsPivot,
                                    IsPivot = false,
                                    IsFullService = true,
                                    SpecialKey = specialKey
                                };
                                player.BoxTimes.Add(boxTime);
                            }
                            break;
                        case "/":
                        case "\\":
                            boxTime = new BoxTimeModel
                            {
                                Started = false,
                                Exited = false,
                                IsJammer = player.IsPivot,
                                IsPivot = false,
                                IsFullService = false,
                                SpecialKey = specialKey
                            };
                            player.BoxTimes.Add(boxTime);
                            break;
                        case "s":
                        case "S":
                            Console.WriteLine("s in SP box time");
                            break;
                        case "$":
                            if(lastBox != null)
                            {
                                lastBox.Exited = true;
                                if(lastBox.Started == false)
                                {
                                    lastBox.IsFullService = true;
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException("started in box during star pass?");
                            }
                            break;
                        case "3":
                            player.WasInjured = true;
                            break;
                    }
                    foulCol++;
                }
            }
        }
    }
}
