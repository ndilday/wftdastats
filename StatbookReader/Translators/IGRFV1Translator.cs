using System;
using System.Collections.Generic;

using OfficeOpenXml;
using StatbookReader.Models;

namespace StatbookReader.Translators
{
    class IGRFV1Translator : BaseIGRFTranslator, ITranslator
    {
        public IGRFV1Translator()
        {
            /*
             * "B8", "B9", "H8", "H9", "B11", "H11", "B5",
                "A4", "AA4", "A50", "AA50",
                "A4:J43", "AC4:AL43", "P4:Y43", "AR4:BA43",
                "A4:Q41", "T4:AJ41", "A51:Q88", "T51:AJ88"
             */
            _cells = new StatbookCells
            {
                HomeLeagueCell = "B8",
                HomeTeamNameCell = "B9",
                HomeRosterCell = "B11",
                AwayLeagueCell = "H8",
                AwayTeamNameCell = "H9",
                AwayRosterCell = "H11",
                BoutDateCell = "B5",
                HomeLineupFirstPeriodCell = "A4",
                HomeLineupSecondPeriodCell = "A50",
                AwayLineupFirstPeriodCell = "AA4",
                AwayLineupSecondPeriodCell = "AA50",
                HomePenaltyFirstPeriodCell = "A4",
                HomePenaltySecondPeriodCell = "AC4",
                AwayPenaltyFirstPeriodCell = "P4",
                AwayPenaltySecondPeriodCell = "AR4",
                HomeScoreFirstPeriodCell = "A4",
                HomeScoreSecondPeriodCell = "A51",
                AwayScoreFirstPeriodCell = "T4",
                AwayScoreSecondPeriodCell = "T51",
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
                BoxTimeModel boxTime;
                bool isJammer = player.IsJammer;
                string foulMark = foulMarkObj.ToString().Trim();
                char? specialKey = null;
                if(foulMark.Length > 1)
                {
                    specialKey = foulMark[1];
                    foulMark = foulMark.Substring(0, 1);
                }
                switch (foulMark.ToString().Trim())
                {
                    case "x":
                    case "X":
                        boxTime = new BoxTimeModel
                        {
                            Started = false,
                            Exited = true,
                            IsJammer = player.IsJammer,
                            IsPivot = player.IsPivot,
                            IsFullService = true,
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
                            IsPivot =player.IsPivot,
                            IsFullService = false,
                            SpecialKey = specialKey
                        };
                        player.BoxTimes.Add(boxTime);
                        break;
                    case "s":
                    case "S":
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
            if (isSP)
            {
                foulCol = 2;
                if(player.IsJammer)
                {
                    foulCol += 4;
                }
                else if(player.IsPivot)
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
                            break;
                        case "$":
                            if (lastBox != null)
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
                        default:
                            throw new InvalidOperationException("Unexpected penalty character " + foulMark.ToString().Trim() + " for #" + player.PlayerNumber);
                    }
                    foulCol++;
                }
            }
        }
    }
}
