using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using DerbyDataAccessLayer;
using DerbyDataModels;

using StatbookReader.Models;

namespace StatbookReader
{
    public class BasicDataImporter
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        public void Import(string connectionString, StatbookModel statbook)
        {
            _connection = new SqlConnection(connectionString);
            try
            {
                _connection.Open();
                _transaction = _connection.BeginTransaction();
                
                // insert leagues
                LeagueGateway leagueGateway = new LeagueGateway(_connection, _transaction);
                var leagues = leagueGateway.GetAllLeagues();
                League homeLeague = leagues.FirstOrDefault(l => l.Name.ToLower() == statbook.HomeTeam.LeagueName.ToLower());
                League awayLeague = leagues.FirstOrDefault(l => l.Name.ToLower() == statbook.AwayTeam.LeagueName.ToLower());
                int maxID = leagues.Select(l => l.ID).Max();
                if(homeLeague == null || awayLeague == null)
                {
                    throw new InvalidOperationException("Bad league name");
                }

                // for the basic importer, we'll just take whatever A team this league has, rather than worrying about validating the team name
                TeamGateway teamGateway = new TeamGateway(_connection, _transaction);
                Team homeTeam = teamGateway.GetATeam(homeLeague.ID);
                Team awayTeam = teamGateway.GetATeam(awayLeague.ID);

                // insert bout
                BoutGateway boutGateway = new BoutGateway(_connection, _transaction);
                if(!boutGateway.DoesBoutExist(homeTeam.ID, awayTeam.ID, statbook.Date))
                {
                    Bout bout = boutGateway.GetBout(homeTeam.ID, awayTeam.ID, statbook.Date);
                    BoutDataImport(statbook, bout, homeTeam, awayTeam);
                }
                else
                {
                    // bout already exists
                    Console.WriteLine(string.Format("Bout between {0} and {1} on {2} already exists.", homeTeam.Name, awayTeam.Name, statbook.Date));
                }

                _transaction.Commit();
            }
            finally
            {
                _connection.Close();
            }
        }

        private void BoutDataImport(StatbookModel statbook, Bout bout, Team homeTeam, Team awayTeam)
        {
            // import players
            Dictionary<string, Player> homePlayerMap = CreatePlayerMap(homeTeam, statbook.HomeTeam.Players);
            Dictionary<string, Player> awayPlayerMap = CreatePlayerMap(awayTeam, statbook.AwayTeam.Players);

            // import jams
            List<Jam> jamList = CreateJamList(bout, statbook.Lineups);

            // import player jams
            Dictionary<int, List<JamPlayer>> jamPlayerMap = CreateJamPlayerMap(homePlayerMap, awayPlayerMap, jamList, statbook.Lineups);

            // import scores
            AddScores(homePlayerMap, awayPlayerMap, jamList, statbook.Scores);

            // import penalties/box times
            AddPenaltyServices(homePlayerMap, awayPlayerMap, jamList, statbook.Lineups, statbook.Penalties);
        }

        private Dictionary<string, Player> CreatePlayerMap(Team team, IList<PlayerModel> list)
        {
            Dictionary<string, Player> playerMap = new Dictionary<string,Player>();
            // TODO: handle player name changes
            PlayerGateway playerGateway = new PlayerGateway(_connection, _transaction);
            foreach(PlayerModel player in list)
            {
                playerMap[player.Number] = playerGateway.GetPlayer(player.Number, player.Name, team.ID);
            }
            return playerMap;
        }

        private List<Jam> CreateJamList(Bout bout, IList<JamLineupModel> lineups)
        {
            JamGateway jamGateway = new JamGateway(_connection, _transaction);
            List<Jam> jams = new List<Jam>();
            foreach(JamLineupModel jamLineup in lineups)
            {
                jams.Add(jamGateway.GetJam(bout.ID, jamLineup.IsFirstHalf, jamLineup.JamNumber));
            }
            return jams;
        }

        private Dictionary<int, List<JamPlayer>> CreateJamPlayerMap(Dictionary<string, Player> homePlayerMap, Dictionary<string, Player> awayPlayerMap, 
                                                                    IList<Jam> jamList, IList<JamLineupModel> lineups)
        {
            Dictionary<int, List<JamPlayer>> map = new Dictionary<int, List<JamPlayer>>();
            JamPlayerGateway jamPlayerGateway = new JamPlayerGateway(_connection, _transaction);

            foreach(JamLineupModel jamLineup in lineups)
            {
                Jam jam = jamList.First(j => j.IsFirstHalf == jamLineup.IsFirstHalf && j.JamNumber == jamLineup.JamNumber);
                List<JamPlayer> list = AddJamPlayers(homePlayerMap, jam, jamLineup.HomeLineup, jamPlayerGateway);
                list.AddRange(AddJamPlayers(awayPlayerMap, jam, jamLineup.AwayLineup, jamPlayerGateway));
                map[jam.ID] = list;
            }
            return map;
        }

        private List<JamPlayer> AddJamPlayers(Dictionary<string, Player> playerMap, Jam jam, IList<PlayerLineupModel> lineups, JamPlayerGateway gateway)
        {
            List<JamPlayer> list = new List<JamPlayer>();
            List<string> duplicateCheckList = new List<string>();
            foreach (PlayerLineupModel lineup in lineups)
            {
                if(lineup == null)
                {
                    // TODO: handle null players
                    Console.WriteLine(jam.ToString() + ": empty player spot");
                    continue;
                }
                if (duplicateCheckList.Contains(lineup.PlayerNumber))
                {
                    throw new InvalidOperationException(string.Format("{0}: #{1} in lineup multiple times.", jam, lineup.PlayerNumber));
                }

                duplicateCheckList.Add(lineup.PlayerNumber);
                Player player = playerMap[lineup.PlayerNumber];
                list.Add(gateway.AddJamPlayer(jam.ID, player.ID, player.TeamID, lineup.IsJammer, lineup.IsPivot));
            }
            return list;
        }
    
        private void AddScores(Dictionary<string, Player> homePlayerMap, Dictionary<string, Player> awayPlayerMap, IList<Jam> jams, IList<JamScoreModel> scores)
        {
            JammerGateway jammerGateway = new JammerGateway(_connection, _transaction);
            foreach(JamScoreModel jamScoreModel in scores)
            {
                Jam jam = jams.First(j => j.IsFirstHalf == jamScoreModel.IsFirstHalf && j.JamNumber == jamScoreModel.JamNumber);
                if(jamScoreModel.HomeStarPass == null)
                {
                    AddJammer(jammerGateway, homePlayerMap, jam, jamScoreModel.HomeJammer, false, false);
                }
                else
                {
                    AddJammer(jammerGateway, homePlayerMap, jam, jamScoreModel.HomeJammer, true, false);
                    AddJammer(jammerGateway, homePlayerMap, jam, jamScoreModel.HomeStarPass, false, true);
                }

                if (jamScoreModel.AwayStarPass == null)
                {
                    AddJammer(jammerGateway, awayPlayerMap, jam, jamScoreModel.AwayJammer, false, false);
                }
                else
                {
                    AddJammer(jammerGateway, awayPlayerMap, jam, jamScoreModel.AwayJammer, true, false);
                    AddJammer(jammerGateway, awayPlayerMap, jam, jamScoreModel.AwayStarPass, false, true);
                }
            }
        }

        private void AddJammer(JammerGateway jammerGateway, Dictionary<string, Player> playerMap, Jam jam, ScoreModel scoreModel, bool passedStar, bool receivedStar)
        {
            if (scoreModel != null && !string.IsNullOrWhiteSpace(scoreModel.PlayerNumber))
            {
                jammerGateway.AddJammer(jam.ID, playerMap[scoreModel.PlayerNumber].ID, scoreModel.JamTotal,
                    scoreModel.Lost, scoreModel.Lead, scoreModel.Called, scoreModel.Injury, scoreModel.NoPass, passedStar, receivedStar);
            }
        }
    
        private void AddPenaltyServices(Dictionary<string, Player> homePlayerMap, Dictionary<string, Player> awayPlayerMap, 
                                        IList<Jam> jams, IList<JamLineupModel> lineups, PenaltiesModel penalties)
        {
            Dictionary<int, PlayerPenaltiesModel> homePlayerPenalties = penalties.HomePlayerPenalties.ToDictionary(pp => homePlayerMap[pp.PlayerNumber].ID);
            Dictionary<int, PlayerPenaltiesModel> awayPlayerPenalties = penalties.AwayPlayerPenalties.ToDictionary(pp => awayPlayerMap[pp.PlayerNumber].ID);
            
            AddTeamPenalties(jams, homePlayerMap, homePlayerPenalties);
            AddTeamPenalties(jams, awayPlayerMap, awayPlayerPenalties);
        }

        private void AddTeamPenalties(IList<Jam> jams, Dictionary<string, Player> players, Dictionary<int, PlayerPenaltiesModel> playerPenalties)
        {
            //PenaltyProcessor processor = new PenaltyProcessor(jams, players);
            PenaltyGateway penaltyGateway = new PenaltyGateway(_connection, _transaction);
            bool isSuccess = true;
            foreach(KeyValuePair<int, PlayerPenaltiesModel> kvp in playerPenalties)
            {
                int penaltyCount = 0;
                foreach(PenaltyModel penalty in kvp.Value.Penalties)
                {
                    Jam jam = jams.FirstOrDefault(j => j.IsFirstHalf == penalty.IsFirstHalf && j.JamNumber == penalty.JamNumber);
                    if(jam == null)
                    {
                        Console.WriteLine(string.Format("Unknown jam number {0} on penalty for player {1}.", penalty.JamNumber, kvp.Value.PlayerNumber));
                        isSuccess = false;
                    }
                    penaltyCount++;
                    Penalty pen = new Penalty
                    {
                        JamID = jam.ID,
                        PlayerID = kvp.Key,
                        PenaltyCode = penalty.PenaltyCode,
                        PenaltyNumber = penaltyCount,
                        MatchingKey = penalty.SpecificKey
                    };
                    if(pen.MatchingKey != null)
                    {
                        Console.WriteLine(string.Format("Special case penalty {0}{1} encountered in {2}", penalty.PenaltyCode, penalty.SpecificKey, penalty.JamNumber));
                    }
                    penaltyGateway.AddBasicPenalty(pen);
                }
            }
            if(!isSuccess)
            {
                throw new InvalidOperationException("Bad penalty data");
            }
        }
    }
}
