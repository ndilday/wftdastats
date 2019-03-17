using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    // stating mu = 500
    // Beta = 200
    // starting sigma = 500/3
    public class PlayerTrueSkillCalculator
    {
        private string _connectionString;
        private Dictionary<int, SkillGaussian> _jammerSkillDictionary = new Dictionary<int, SkillGaussian>(500);
        private Dictionary<int, SkillGaussian> _blockerSkillDictionary = new Dictionary<int,SkillGaussian>(1000);
        private TimeSpan _baseTimeSpan = new TimeSpan(730, 0, 0, 0, 0);
        private Dictionary<int, Dictionary<int, int>> _jamTeamPointDeltaMap = new Dictionary<int,Dictionary<int,int>>(1500);
        private Dictionary<int, Dictionary<int, JamPlayerEffectiveness>> _pjeMap = null;
        private IList<Jam> _jams = null;
        private Dictionary<int, Bout> _boutMap = null;
        private IList<JamTeamData> _jamData = null;
        private Dictionary<int, int> _pointDeltaCount = new Dictionary<int,int>(81);
        private Dictionary<int, double> _pointDeltaCumulative = new Dictionary<int,double>(81);

        private const double _baseSigma = 500.0 / 3.0;
        private const double _baseVariance = (500.0 / 3.0) * (500.0 / 3.0);
        // beta is 200
        private const double _betaSquared = 40000;

        public PlayerTrueSkillCalculator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CalculateTrueSkills()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            _pjeMap = new JamPlayerEffectivenessGateway(connection, transaction).GetAllJamPlayerEffectiveness();
            _jams = new JamGateway(connection, transaction).GetAllJams();
            _boutMap = new BoutGateway(connection, transaction).GetBouts().ToDictionary(b => b.ID);
            _jamData = new JamDataGateway(connection, transaction).GetAllJamTeamData();
            CalculatePointDeltaMaps();

            var jamOrder =
                from jam in _jams
                join bout in _boutMap.Values on jam.BoutID equals bout.ID
                orderby bout.BoutDate ascending, jam.ID ascending
                select jam;


            foreach(Jam jam in jamOrder)
            {
                AnalyzeJam(_pjeMap[jam.ID].Values, _boutMap[jam.BoutID].BoutDate);
            }

            // do a second pass, see what happens
            Dictionary<int, SkillGaussian> newBlockerDictionary = new Dictionary<int, SkillGaussian>();
            Dictionary<int, SkillGaussian> newJammmerDictionary = new Dictionary<int, SkillGaussian>();
            foreach(SkillGaussian player in _blockerSkillDictionary.Values)
            {
                newBlockerDictionary[player.ID] = new SkillGaussian(player.ID, player.Mean, _betaSquared, player.IsJammer, DateTime.MinValue);
            }
            foreach (SkillGaussian player in _jammerSkillDictionary.Values)
            {
                newJammmerDictionary[player.ID] = new SkillGaussian(player.ID, player.Mean, _betaSquared, player.IsJammer, DateTime.MinValue);
            }
            _blockerSkillDictionary = newBlockerDictionary;
            _jammerSkillDictionary = newJammmerDictionary;
            foreach (Jam jam in jamOrder)
            {
                AnalyzeJam(_pjeMap[jam.ID].Values, _boutMap[jam.BoutID].BoutDate);
            }

            var ptsGateway = new PlayerTrueSkillGateway(connection, transaction);
            var insertList = _jammerSkillDictionary.Values.Select( psd =>
                new PlayerTrueSkill
                {
                    PlayerID = psd.ID,
                    IsJammer = psd.IsJammer,
                    Mean = psd.Mean,
                    StdDev = psd.Sigma,
                    LastUpdated = psd.LastUpdated
                }
            ).ToList();
            insertList.AddRange(_blockerSkillDictionary.Values.Select( psd =>
                new PlayerTrueSkill
                {
                    PlayerID = psd.ID,
                    IsJammer = psd.IsJammer,
                    Mean = psd.Mean,
                    StdDev = psd.Sigma,
                    LastUpdated = psd.LastUpdated
                }
            ));
            ptsGateway.InsertPlayerTrueSkills(insertList);
            transaction.Commit();
            connection.Close();
        }

        private void CalculatePointDeltaMaps()
        {
            // bucket each instance of a given point delta
            foreach(JamTeamData data in _jamData)
            {
                if(!_jamTeamPointDeltaMap.ContainsKey(data.JamID))
                {
                    _jamTeamPointDeltaMap[data.JamID] = new Dictionary<int, int>(2);
                }
                _jamTeamPointDeltaMap[data.JamID][data.TeamID] = data.PointDelta;

                if(!_pointDeltaCount.ContainsKey(data.PointDelta))
                {
                    _pointDeltaCount[data.PointDelta] = 1;
                }
                else
                {
                    _pointDeltaCount[data.PointDelta]++;
                }
            }

            int rollingSum = 0;
            // get the total number of results
            double total = _pointDeltaCount.Values.Sum();
            
            // make a cumulative distribution
            foreach(int delta in _pointDeltaCount.Keys.OrderBy(x => x))
            {
                int thisItem = _pointDeltaCount[delta];
                _pointDeltaCumulative[delta] = (rollingSum + (thisItem / 2.0)) / total;
                rollingSum += thisItem;
            }

        }

        private void AnalyzeJam(IEnumerable<JamPlayerEffectiveness> players, DateTime dateOfJam)
        {
            var teams = players.GroupBy(p => p.TeamID);
            var team1 = teams.First().ToList();
            var team2 = teams.Last().ToList();
            var skills1 = team1.Select(p => CreatePlayerSkill(p.PlayerID, p.IsJammer, dateOfJam));
            var skills2 = team2.Select(p => CreatePlayerSkill(p.PlayerID, p.IsJammer, dateOfJam));
            var team1Skill = CalculateTeamRating(team1, skills1);
            var team2Skill = CalculateTeamRating(team2, skills2);

            // the difference in team skills, adjusted by the beta, give us a sense of how we expected this matchup to go
            // that can be compared to the actual result
            double c = Math.Sqrt(team1Skill.Variance + team2Skill.Variance + 2 * _betaSquared);

            // in normal TrueSkill, a Beta lead in rating is interpreted as having about an 80% chance of a win.
            // in our implementation, it represents an 80% result; in the 0-penalty case, 
            // this is approximately equal to getting about a 3.5 point delta on a jam

            // determine which team exceeded expectations
            // basically, what in trueskill would be the main value becomes our "draw" value,
            // and the actual result becomes our main value
            double meanDiff = team1Skill.Mean - team2Skill.Mean;
            double scaledMeanDiff = meanDiff / c;
            double trueSkillDenominator = SkillGaussian.CumulativeTo(scaledMeanDiff, 0, 1);
            double result = _pointDeltaCumulative[_jamTeamPointDeltaMap[players.First().JamID][team1.First().TeamID]];
            // the denominator is effectively the percent result expected for team 1; 
            // if the actual result is greater, team 1 overperformed
            double team1Multiplier = 1;
            if (trueSkillDenominator > result)
            {
                meanDiff = team2Skill.Mean - team1Skill.Mean;
                scaledMeanDiff = meanDiff / c;
                trueSkillDenominator = SkillGaussian.CumulativeTo(scaledMeanDiff, 0, 1);
                result = _pointDeltaCumulative[_jamTeamPointDeltaMap[players.First().JamID][team2.First().TeamID]];
                team1Multiplier = -1;
            }
            double inverseResult = SkillGaussian.InverseCumulativeTo(result, 0, 1);
            double newDenominator = SkillGaussian.CumulativeTo(scaledMeanDiff - inverseResult, 0, 1);
            double v = (SkillGaussian.At(scaledMeanDiff - inverseResult, 0, 1)) / newDenominator;
            double w = v * (v + scaledMeanDiff - inverseResult);

            foreach(SkillGaussian player in skills1)
            {
                CalculateNewPlayerRating(c, team1Multiplier, v, w, player);
            }
            foreach(SkillGaussian player in skills2)
            {
                CalculateNewPlayerRating(c, -team1Multiplier, v, w, player);
            }
        }

        private void CalculateNewPlayerRating(double c, double team1Multiplier, double v, double w, SkillGaussian player)
        {
            double meanMultiplier = player.Variance / c;
            double stdDevMultiplier = meanMultiplier / c;

            double playerMeanDelta = (team1Multiplier * meanMultiplier * v);
            double newMean = player.Mean + playerMeanDelta;
            double newVariance = player.Variance * (1 - (w * stdDevMultiplier));
            SkillGaussian newValues = new SkillGaussian(player.ID, newMean, newVariance, player.IsJammer, player.LastUpdated);
            if(player.IsJammer)
            {
                _jammerSkillDictionary[player.ID] = newValues;
            }
            else
            {
                _blockerSkillDictionary[player.ID] = newValues;
            }
        }

        private SkillGaussian CreatePlayerSkill(int playerID, bool isJammer, DateTime dateOfGame)
        {
            // variances, not std dev, are sum-able
            // a^2 + b^2 != (a+b)^2
            var dictionary = isJammer ? _jammerSkillDictionary : _blockerSkillDictionary;

            if(!dictionary.ContainsKey(playerID))
            {
                SkillGaussian newPlayer = new SkillGaussian(playerID, 500, _baseVariance, isJammer, dateOfGame);
                dictionary[playerID] = newPlayer;
                return newPlayer;
            }
            var startingPlayer = dictionary[playerID];
            double tau = CalculateTimeEffect(startingPlayer, dateOfGame);
            startingPlayer.AddVariance(Math.Pow(tau, 2));
            startingPlayer.LastUpdated = dateOfGame;
            return startingPlayer;
        }

        private SkillGaussian CalculateTeamRating(IEnumerable<JamPlayerEffectiveness> side, IEnumerable<SkillGaussian> players)
        {
            if(side.Count() > 5)
            {
                throw new InvalidOperationException("too many players on team!");
            }
            // for each player, we need their mean and their playtime portion
            double summedQuality = 0;
            double summedVariance = 0;
            foreach(JamPlayerEffectiveness player in side)
            {
                // we scale the jammer to be half of the total importance of the team
                // we're adding in the beta variance here, rather than on the player,
                // to make future updates to the player cleaner
                SkillGaussian playerSkill = players.First(p => p.ID == player.PlayerID);
                if (player.IsJammer)
                {
                    summedQuality += player.JamPortion * playerSkill.Mean * 4;
                    summedVariance += player.JamPortion * (playerSkill.Variance + _betaSquared)* 4;
                }
                else
                {
                    summedQuality += player.JamPortion * playerSkill.Mean;
                    summedVariance += player.JamPortion * (playerSkill.Variance + _betaSquared);
                }
            }
            return new SkillGaussian(0, summedQuality, summedVariance);
        }

        // at some point, we'll need to take the current sigma of the player and increase it, probably based on time since last data
        private double CalculateTimeEffect(SkillGaussian player, DateTime newTime)
        {
            // how much do we want to scale the player's std dev?
            // it probably ought to be flat, not proportional
            // one way to think of it: how long would a well-understood player have to disappear to be treated as new again?
            // let's say two years-ish, and see what happens
            
            TimeSpan timeSpan = newTime - player.LastUpdated;
            if(timeSpan > _baseTimeSpan)
            {
                timeSpan = _baseTimeSpan;
            }
            double ratio = ((double)timeSpan.Ticks) / _baseTimeSpan.Ticks;
            double range = _baseSigma - player.Sigma;
            return range * ratio;
        }
    }
}
