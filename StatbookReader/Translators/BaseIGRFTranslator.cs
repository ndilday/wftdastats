using System;
using System.Collections.Generic;

using OfficeOpenXml;

using StatbookReader.Models;

namespace StatbookReader.Translators
{
    internal class StatbookCells
    {
        public string HomeLeagueCell { get; set; }
        public string HomeTeamNameCell { get; set; }
        public string HomeRosterCell { get; set; }
        public string AwayLeagueCell { get; set; }
        public string AwayTeamNameCell { get; set; }
        public string AwayRosterCell { get; set; }
        public string BoutDateCell { get; set; }
        public string HomeLineupFirstPeriodCell { get; set; }
        public string HomeLineupSecondPeriodCell { get; set; }
        public string AwayLineupFirstPeriodCell { get; set; }
        public string AwayLineupSecondPeriodCell { get; set; }
        public string HomePenaltyFirstPeriodCell { get; set; }
        public string HomePenaltySecondPeriodCell { get; set; }
        public string AwayPenaltyFirstPeriodCell { get; set; }
        public string AwayPenaltySecondPeriodCell { get; set; }
        public string HomeScoreFirstPeriodCell { get; set; }
        public string HomeScoreSecondPeriodCell { get; set; }
        public string AwayScoreFirstPeriodCell { get; set; }
        public string AwayScoreSecondPeriodCell { get; set; }
    }

    internal abstract class BaseIGRFTranslator
    {
        protected StatbookCells _cells;
        protected HashSet<string> _dupePlayerCheck;
        protected StatbookModel CreateStatbook(ExcelWorkbook workbook)
        {
            ExcelWorksheet irgf = workbook.Worksheets["IGRF"];
            ExcelWorksheet score = workbook.Worksheets["Score"];
            ExcelWorksheet lineups = workbook.Worksheets["Lineups"];
            ExcelWorksheet fouls = workbook.Worksheets["Penalties"];

            StatbookModel model = ProcessIrgf(irgf);
            model.Lineups = ProcessLineups(lineups);
            model.Penalties = ProcessPenalties(fouls);
            model.Scores = ProcessScores(score);
            return model;
        }

        protected StatbookModel ProcessIrgf(ExcelWorksheet irgf)
        {
            StatbookModel statbook = new StatbookModel();
            statbook.HomeTeam = new TeamModel();
            statbook.AwayTeam = new TeamModel();

            statbook.HomeTeam.LeagueName = irgf.Cells[_cells.HomeLeagueCell].Value.ToString().Trim();
            statbook.HomeTeam.Name = irgf.Cells[_cells.HomeTeamNameCell].Value.ToString().Trim();

            statbook.AwayTeam.LeagueName = irgf.Cells[_cells.AwayLeagueCell].Value.ToString().Trim();
            statbook.AwayTeam.Name = irgf.Cells[_cells.AwayTeamNameCell].Value.ToString().Trim();

            string dateString = irgf.Cells[_cells.BoutDateCell].Value.ToString().Trim();// +" " + irgf.Cells["H5"].Value.ToString();
            if (dateString.Contains("/") || dateString.Contains("-"))
            {
                statbook.Date = Convert.ToDateTime(dateString);
            }
            else
            {
                statbook.Date = DateTime.FromOADate(Convert.ToDouble(dateString));
            }

            ExcelRange homeStart = irgf.Cells[_cells.HomeRosterCell];
            ExcelRange awayStart = irgf.Cells[_cells.AwayRosterCell];
            statbook.HomeTeam.Players = new List<PlayerModel>();
            statbook.AwayTeam.Players = new List<PlayerModel>();

            for (int i = 0; i < 20; i++)
            {
                object numberCell = homeStart.Offset(i, 0).Value;
                if (numberCell != null && !string.IsNullOrWhiteSpace(numberCell.ToString()))
                {
                    PlayerModel player = new PlayerModel();

                    player.Number = numberCell.ToString().Trim();
                    player.Name = homeStart.Offset(i, 1).Value.ToString();
                    statbook.HomeTeam.Players.Add(player);
                }

                numberCell = awayStart.Offset(i, 0).Value;
                if (numberCell != null && !string.IsNullOrWhiteSpace(numberCell.ToString()))
                {
                    PlayerModel player = new PlayerModel();
                    player.Number = numberCell.ToString().Trim();
                    player.Name = awayStart.Offset(i, 1).Value.ToString();
                    statbook.AwayTeam.Players.Add(player);
                }
            }
            return statbook;
        }

        protected IList<JamLineupModel> ProcessLineups(ExcelWorksheet lineups)
        {
            List<JamLineupModel> list = new List<JamLineupModel>();
            // handle first half
            ExcelRange homeStart = lineups.Cells[_cells.HomeLineupFirstPeriodCell];
            ExcelRange awayStart = lineups.Cells[_cells.AwayLineupFirstPeriodCell];
            list.AddRange(ProcessLineupRows(homeStart, awayStart, true));
            // handle second half
            homeStart = lineups.Cells[_cells.HomeLineupSecondPeriodCell];
            awayStart = lineups.Cells[_cells.AwayLineupSecondPeriodCell];
            list.AddRange(ProcessLineupRows(homeStart, awayStart, false));

            return list;
        }

        protected IList<JamLineupModel> ProcessLineupRows(ExcelRange homeLineupStart, ExcelRange awayLineupStart, bool isFirstHalf)
        {
            List<JamLineupModel> list = new List<JamLineupModel>();
            int currentRow = 1;
            while (currentRow < 39)
            {
                string jamNumber = homeLineupStart.SubRange(currentRow, 1).Value == null ? null : homeLineupStart.SubRange(currentRow, 1).Value.ToString();
                string nextJamNumber = null;
                if (!string.IsNullOrEmpty(jamNumber))
                {
                    // add jam
                    int number = Convert.ToInt32(jamNumber);
                    JamLineupModel jamLineup = new JamLineupModel();
                    jamLineup.IsFirstHalf = isFirstHalf;
                    jamLineup.JamNumber = number;

                    nextJamNumber = homeLineupStart.SubRange(currentRow + 1, 1).Value == null ? null : homeLineupStart.SubRange(currentRow + 1, 1).Value.ToString();
                    bool isSP = nextJamNumber == null ? false : nextJamNumber.Trim().ToLowerInvariant() == "sp";
                    jamLineup.HomeLineup = new List<PlayerLineupModel>();
                    bool hasPivot = homeLineupStart.SubRange(currentRow, 2).Value == null;
                    _dupePlayerCheck = new HashSet<string>();
                    // TODO: do error checking here
                    jamLineup.HomeLineup.Add(CreateJamPlayer(homeLineupStart.SubRange(currentRow, 3), true, false, isSP));
                    jamLineup.HomeLineup.Add(CreateJamPlayer(homeLineupStart.SubRange(currentRow, 7), false, hasPivot, isSP));
                    jamLineup.HomeLineup.Add(CreateJamPlayer(homeLineupStart.SubRange(currentRow, 11), false, false, isSP));
                    jamLineup.HomeLineup.Add(CreateJamPlayer(homeLineupStart.SubRange(currentRow, 15), false, false, isSP));
                    jamLineup.HomeLineup.Add(CreateJamPlayer(homeLineupStart.SubRange(currentRow, 19), false, false, isSP));

                    nextJamNumber = awayLineupStart.SubRange(currentRow + 1, 1).Value == null ? null : awayLineupStart.SubRange(currentRow + 1, 1).Value.ToString();
                    isSP = nextJamNumber == null ? false : nextJamNumber.Trim().ToLowerInvariant() == "sp";
                    jamLineup.AwayLineup = new List<PlayerLineupModel>();
                    hasPivot = awayLineupStart.SubRange(currentRow, 2).Value == null;
                    _dupePlayerCheck = new HashSet<string>();
                    // TODO: do error checking here
                    jamLineup.AwayLineup.Add(CreateJamPlayer(awayLineupStart.SubRange(currentRow, 3), true, false, isSP));
                    jamLineup.AwayLineup.Add(CreateJamPlayer(awayLineupStart.SubRange(currentRow, 7), false, hasPivot, isSP));
                    jamLineup.AwayLineup.Add(CreateJamPlayer(awayLineupStart.SubRange(currentRow, 11), false, false, isSP));
                    jamLineup.AwayLineup.Add(CreateJamPlayer(awayLineupStart.SubRange(currentRow, 15), false, false, isSP));
                    jamLineup.AwayLineup.Add(CreateJamPlayer(awayLineupStart.SubRange(currentRow, 19), false, false, isSP));

                    list.Add(jamLineup);
                }
                currentRow++;
                while (nextJamNumber != null && (nextJamNumber.ToLower().Trim() == "sp" || nextJamNumber.ToLower().Trim() == "sp*"))
                {
                    currentRow++;
                    var nextJam = homeLineupStart.SubRange(currentRow, 1).Value;
                    nextJamNumber = nextJam == null ? null : nextJam.ToString();
                }
            }

            return list;
        }

        private PlayerLineupModel CreateJamPlayer(ExcelRange playerRange, bool isJammer, bool isPivot, bool isSP)
        {
            PlayerLineupModel model = new PlayerLineupModel();
            object value = playerRange.Value;
            if(value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return null;
            }
            model.PlayerNumber = value.ToString().Trim();
            if(_dupePlayerCheck.Contains(model.PlayerNumber))
            {
                return null;
            }
            model.IsJammer = isJammer;
            model.IsPivot = isPivot;
            model.WasInjured = false;
            model.BoxTimes = new List<BoxTimeModel>();

            ProcessPlayerBoxes(playerRange, model, isSP);

            return model;
        }

        protected abstract void ProcessPlayerBoxes(ExcelRange playerRange, PlayerLineupModel player, bool isSP);

        protected IList<JamScoreModel> ProcessScores(ExcelWorksheet scores)
        {
            List<JamScoreModel> list = new List<JamScoreModel>();
            ExcelRange homeRange = scores.Cells[_cells.HomeScoreFirstPeriodCell];
            ExcelRange awayRange = scores.Cells[_cells.AwayScoreFirstPeriodCell];
            list.AddRange(ProcessScoreRows(homeRange, awayRange, true));
            homeRange = scores.Cells[_cells.HomeScoreSecondPeriodCell];
            awayRange = scores.Cells[_cells.AwayScoreSecondPeriodCell];
            list.AddRange(ProcessScoreRows(homeRange, awayRange, false));

            return list;
        }

        private IList<JamScoreModel> ProcessScoreRows(ExcelRange homeScores, ExcelRange awayScores, bool isFirstHalf)
        {
            List<JamScoreModel> list = new List<JamScoreModel>();
            int currentRow = 1;
            while (currentRow < 39)
            {
                object jamNumber = homeScores.SubRange(currentRow, 1).Value;
                if (jamNumber != null && !string.IsNullOrEmpty(jamNumber.ToString()))
                {
                    if (jamNumber.ToString().Trim().ToLower() == "sp*")
                    {
                        currentRow++;
                        continue;
                    }
                    // add jam
                    int number = Convert.ToInt32(jamNumber);

                    JamScoreModel model = new JamScoreModel();
                    model.JamNumber = number;
                    model.IsFirstHalf = isFirstHalf;
                    model.HomeJammer = CreateScoreModel(homeScores, currentRow);
                    model.AwayJammer = CreateScoreModel(awayScores, currentRow);
                    model.HomeStarPass = model.AwayStarPass = null;

                    currentRow++;

                    // see if the next home row is a star pass
                    object nextJamNumber = homeScores.SubRange(currentRow, 1).Value;
                    if (nextJamNumber != null && 
                        nextJamNumber.ToString().Trim().ToLower() == "sp" &&
                        homeScores.SubRange(currentRow, 2).Value != null &&
                        !string.IsNullOrWhiteSpace(homeScores.SubRange(currentRow, 2).Value.ToString()))
                    {
                        model.HomeStarPass = CreateScoreModel(homeScores, currentRow);
                    }
                    nextJamNumber = awayScores.SubRange(currentRow, 1).Value;
                    if (nextJamNumber != null && 
                        nextJamNumber.ToString().Trim().ToLower() == "sp" &&
                        awayScores.SubRange(currentRow, 2).Value != null &&
                        !string.IsNullOrWhiteSpace(awayScores.SubRange(currentRow, 2).Value.ToString()))
                    {
                        model.AwayStarPass = CreateScoreModel(awayScores, currentRow);
                    }

                    if(model.AwayStarPass != null || model.HomeStarPass != null)
                    {
                        currentRow++;
                    }
                    list.Add(model);
                }
                else
                {
                    break;
                }
            }
            return list;
        }

        private ScoreModel CreateScoreModel(ExcelRange scores, int currentRow)
        {
            var scorer = scores.SubRange(currentRow, 2).Value;
            if(scorer == null || string.IsNullOrEmpty(scorer.ToString()))
            {
                return null;
            }

            ScoreModel model = new ScoreModel();
            model.PlayerNumber = scorer.ToString();
            model.JamTotal = Convert.ToInt32(scores.SubRange(currentRow, 17).Value);
            model.Lost = !string.IsNullOrWhiteSpace((string)scores.SubRange(currentRow, 3).Value);
            model.Lead = !string.IsNullOrWhiteSpace((string)scores.SubRange(currentRow, 4).Value);
            model.Called = !string.IsNullOrWhiteSpace((string)scores.SubRange(currentRow, 5).Value);
            model.NoPass = !string.IsNullOrWhiteSpace((string)scores.SubRange(currentRow, 7).Value);
            model.Injury = !string.IsNullOrWhiteSpace((string)scores.SubRange(currentRow, 6).Value);

            return model;
        }

        protected PenaltiesModel ProcessPenalties(ExcelWorksheet fouls)
        {
            PenaltiesModel penalties = new PenaltiesModel();
            ExcelRange firstHalfFoulRange = fouls.Cells[_cells.HomePenaltyFirstPeriodCell];
            ExcelRange secondHalfFoulRange = fouls.Cells[_cells.HomePenaltySecondPeriodCell];
            penalties.HomePlayerPenalties = ProcessPenaltySheet(firstHalfFoulRange, secondHalfFoulRange);
            firstHalfFoulRange = fouls.Cells[_cells.AwayPenaltyFirstPeriodCell];
            secondHalfFoulRange = fouls.Cells[_cells.AwayPenaltySecondPeriodCell];
            penalties.AwayPlayerPenalties = ProcessPenaltySheet(firstHalfFoulRange, secondHalfFoulRange);

            return penalties;
        }

        private IList<PlayerPenaltiesModel> ProcessPenaltySheet(ExcelRange firstHalfFouls, ExcelRange secondHalfFouls)
        {
            List<PlayerPenaltiesModel> penalties = new List<PlayerPenaltiesModel>();
            int rowOffset = 1;
            while (rowOffset < 40)
            {
                object playerObj = firstHalfFouls.SubRange(rowOffset, 1).Value;
                if (playerObj == null || string.IsNullOrWhiteSpace(playerObj.ToString()))
                {
                    rowOffset += 2;
                    continue;
                }
                PlayerPenaltiesModel model = new PlayerPenaltiesModel();
                model.PlayerNumber = playerObj.ToString().Trim();
                List<PenaltyModel> list = new List<PenaltyModel>();

                list.AddRange(ProcessPenaltySheetPlayer(firstHalfFouls, rowOffset, true));
                object playerObj2 = secondHalfFouls.SubRange(rowOffset, 1).Value;
                if (!playerObj2.ToString().Trim().Equals(model.PlayerNumber, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException("Huh?");
                }
                list.AddRange(ProcessPenaltySheetPlayer(secondHalfFouls, rowOffset, false));
                model.Penalties = list;
                penalties.Add(model);
                rowOffset += 2;
            }
            return penalties;
        }

        private IList<PenaltyModel> ProcessPenaltySheetPlayer(ExcelRange penalties, int rowOffset, bool isFirstHalf)
        {
            List<PenaltyModel> list = new List<PenaltyModel>();
            int columnOffset = 2;
            while (columnOffset < 10)
            {
                // go through each foul
                object foulTypeObj = penalties.SubRange(rowOffset, columnOffset).Value;
                if (foulTypeObj == null || string.IsNullOrWhiteSpace(foulTypeObj.ToString()))
                {
                    columnOffset++;
                    continue;
                }
                string foulType = foulTypeObj.ToString().Trim();
                char? specialKey = null;
                if(foulType.Length > 1)
                {
                    specialKey = foulType[1];
                    foulType = foulType.Substring(0, 1);
                }
                int jamNumber = Convert.ToInt32(penalties.SubRange(rowOffset + 1, columnOffset).Value);
                PenaltyModel penalty = new PenaltyModel
                {
                    IsFirstHalf = isFirstHalf,
                    JamNumber = jamNumber,
                    PenaltyCode = foulType,
                    SpecificKey = specialKey
                };
                list.Add(penalty);
                columnOffset++;
            }
            return list;
        }
    }
}
