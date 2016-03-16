using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

using DerbyDataAccessLayer;
using DerbyDataModels;

namespace DerbyCalculators
{
    class ConnectedBoxTime
    {
        public BoxTime BoxTime { get; set; }
        public PenaltyGroup PenaltyGroup { get; set; }
        public int BoutID { get; set; }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ConnectedBoxTime jpp = obj as ConnectedBoxTime;
            if ((System.Object)jpp == null)
            {
                return false;
            }

            // Return true if the fields match:
            return PenaltyGroup.Equals(jpp.PenaltyGroup) &&
                   BoxTime.Equals(jpp.BoxTime) &&
                   BoutID == jpp.BoutID;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + PenaltyGroup.GetHashCode();
            hash = hash * 23 + BoxTime.GetHashCode();
            hash = hash * 23 + BoutID.GetHashCode();
            return hash;
        }
    }

    class JamPlayerPair
    {
        public int JamID { get; set; }
        public int PlayerID { get; set; }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            JamPlayerPair jpp = obj as JamPlayerPair;
            if ((System.Object)jpp == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (JamID == jpp.JamID) &&
                   (PlayerID == jpp.PlayerID);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + JamID.GetHashCode();
            hash = hash * 23 + PlayerID.GetHashCode();
            return hash;
        }
    }

    public class DurationEstimatesCalculator
    {
        private string _connectionString;
        private IList<Jam> _jams;
        private IList<PenaltyGroup> _penaltyGroups;
        private IList<Jammer> _jammers;
        private IList<ConnectedBoxTime> _connectedBoxList;
        private IList<ConnectedBoxTime> _crazyJammerBoxTimes;
        private IList<ConnectedBoxTime> _matchedStartingJammerBoxTimes;
        private IEnumerable<int> _endJams;
        private IList<IGrouping<JamPlayerPair, ConnectedBoxTime>> _singleJamMultiPenalty;

        public DurationEstimatesCalculator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CalculateDurationEstimates()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();
            if (_jams == null)
            {
                _jams = new JamGateway(connection, transaction).GetAllJams();
            }
            if (_penaltyGroups == null)
            {
                _penaltyGroups = new PenaltyGroupGateway(connection, transaction).GetAllPenaltyGroups();
            }

            var jamBoutMap = _jams.ToDictionary(j => j.ID, j => j.BoutID);
            var penaltyGroupMap = _penaltyGroups.GroupBy(pg => jamBoutMap[pg.BoxTimes.First().JamID]).ToDictionary(gp => gp.Key);
            var boutJams = _jams.GroupBy(j => j.BoutID);

            var jamEstimateMap = CalculateJamDurationLimits(connection, transaction);

            foreach (IGrouping<int, Jam> boutJamSet in boutJams)
            {
                ProcessBout(boutJamSet, penaltyGroupMap[boutJamSet.Key], jamEstimateMap);
            }
            var boxTimeEstimates = CalculateBoxTimeEstimates(jamEstimateMap);
            new JamTimeLimitGateway(connection, transaction).InsertJamTimeEstimates(jamEstimateMap.Values);
            new BoxTimeEstimateGateway(connection, transaction).InsertBoxTimeEstimates(boxTimeEstimates);
            transaction.Commit();
            connection.Close();
        }

        private Dictionary<int, JamTimeEstimate> CalculateJamDurationLimits(SqlConnection connection, SqlTransaction transaction)
        {
            if (_penaltyGroups == null)
            {
                _penaltyGroups = new PenaltyGroupGateway(connection, transaction).GetAllPenaltyGroups();
            }
            if (_jams == null)
            {
                _jams = new JamGateway(connection, transaction).GetAllJams();
            }
            if (_jammers == null)
            {
                _jammers = new JammerGateway(connection, transaction).GetAllJammers();
            }

            var jamMap = _jams.ToDictionary(j => j.ID);

            if (_connectedBoxList == null)
            {
                _connectedBoxList = GenerateConnectedBoxList(jamMap);
            }

            var jamEstimateMap = jamMap.ToDictionary(jm => jm.Key, jm => new JamTimeEstimate { JamID = jm.Key, Minimum = 5, Maximum = 120 });
            _endJams = jamMap.Values.GroupBy(j => j.BoutID).Select(g => g.Max(j => j.ID));
            if(_matchedStartingJammerBoxTimes == null)
            {
                _matchedStartingJammerBoxTimes =
                    (from cbl1 in _connectedBoxList
                     join cbl2 in _connectedBoxList on cbl1.BoxTime.JamID equals cbl2.BoxTime.JamID
                     where
                         cbl1.BoxTime.IsJammer &&
                         cbl2.BoxTime.IsJammer &&
                         cbl1.BoxTime.PlayerID != cbl2.BoxTime.PlayerID &&
                         cbl1.BoxTime.StartedJamInBox == true &&
                         cbl2.BoxTime.StartedJamInBox == true
                     select cbl1).ToList();
            }
            if (_crazyJammerBoxTimes == null)
            {
                _crazyJammerBoxTimes =
                    (from cbl1 in _matchedStartingJammerBoxTimes
                     join cbl2 in _matchedStartingJammerBoxTimes on cbl1.BoxTime.JamID equals cbl2.BoxTime.JamID
                     where
                         cbl1.PenaltyGroup.BoxTimes.Count == 1 &&
                         cbl2.PenaltyGroup.BoxTimes.Count == 1
                     select cbl1).ToList();
            }

            var fullJamService = _connectedBoxList.Where(cbl => cbl.BoxTime.EndedJamInBox && cbl.BoxTime.StartedJamInBox == true);
            foreach (ConnectedBoxTime cbt in fullJamService)
            {
                if (cbt.PenaltyGroup.BoxTimes.Count != 1)
                {
                    // the penalty was not completely served in this jam
                    int newMax = (30 * cbt.PenaltyGroup.Penalties.Count) - (3 * (cbt.PenaltyGroup.BoxTimes.Count - 1));
                    if (jamEstimateMap[cbt.BoxTime.JamID].Maximum > newMax)
                    {
                        jamEstimateMap[cbt.BoxTime.JamID].Maximum = newMax;
                    }
                }
            }

            var singleJamService = _connectedBoxList.Where(cbl => cbl.PenaltyGroup.BoxTimes.Count == 1);
            foreach (ConnectedBoxTime cbt in singleJamService)
            {
                int newMin;

                if (_matchedStartingJammerBoxTimes.Contains(cbt))
                {
                    // when both jammers start a jam in the box and neither ended the previous jam in the box
                    // then they are both released after 10 seconds, or something like that
                    newMin = 10;
                }
                else if (cbt.BoxTime.EndedJamInBox && _endJams.Contains(cbt.BoxTime.JamID))
                {
                    // if someone did not come back onto the track after a penalty 
                    // they started serving in the final jam,
                    // assume they did not serve the whole penalty
                    newMin = 10;
                }
                else
                {
                    // leave enough time for the player to have gotten into and out of the box
                    newMin = cbt.PenaltyGroup.Penalties.Count * 30 + 2;
                }

                JamTimeEstimate limit = jamEstimateMap[cbt.BoxTime.JamID];
                if (newMin > limit.Minimum)
                {
                    limit.Minimum = newMin;
                }
            }

            if (_singleJamMultiPenalty == null)
            {
                _singleJamMultiPenalty = _connectedBoxList.GroupBy(cbl => new JamPlayerPair { JamID = cbl.BoxTime.JamID, PlayerID = cbl.BoxTime.PlayerID }).Where(g => g.Count() > 1).ToList();
            }
            foreach (IGrouping<JamPlayerPair, ConnectedBoxTime> group in _singleJamMultiPenalty)
            {
                float penaltyCount = 0;
                JamTimeEstimate limit = jamEstimateMap[group.Key.JamID];
                foreach (ConnectedBoxTime boxTime in group)
                {
                    if (!boxTime.BoxTime.StartedJamInBox == true && !boxTime.BoxTime.EndedJamInBox)
                    {
                        penaltyCount += boxTime.PenaltyGroup.Penalties.Count;
                    }
                    else
                    {
                        penaltyCount += 0.5f;
                    }
                }
                int totalSeconds = (int)(penaltyCount * 32);
                if (totalSeconds < limit.Maximum && totalSeconds > limit.Minimum)
                {
                    limit.Minimum = totalSeconds;
                }
            }

            var jammers = _jammers.GroupBy(j => j.JamID);
            bool exceptions = false;
            foreach (IGrouping<int, Jammer> jamGroup in jammers)
            {
                // Jams that do not end by call or injury must have gone the full two minutes
                if (!jamGroup.Where(j => j.Called || j.Injury).Any())
                {
                    jamEstimateMap[jamGroup.Key].Minimum = 120;
                    if (jamEstimateMap[jamGroup.Key].Maximum != 120)
                    {
                       Console.WriteLine("JamID " + jamGroup.Key + ": conflicting maximums");
                        exceptions = true;
                    }
                }
                else
                {
                    // jams must have gone at least long enough for people to get around the track
                    int highestScore = jamGroup.GroupBy(jg => jg.TeamID).Max(g => g.Sum(j => j.Score));
                    int newMin = (highestScore + 4 / 5) * 6 + 5;
                    JamTimeEstimate limit = jamEstimateMap[jamGroup.Key];
                    if (newMin > limit.Minimum)
                    {
                        limit.Minimum = newMin;
                    }
                }
            }
            if(exceptions)
            {
                throw new InvalidDataException("Conflicting maximums");
            }
            return jamEstimateMap;
        }

        private List<ConnectedBoxTime> GenerateConnectedBoxList(Dictionary<int, Jam> jamMap)
        {
            List<ConnectedBoxTime> connectedBoxList = new List<ConnectedBoxTime>();
            foreach (PenaltyGroup group in _penaltyGroups)
            {
                foreach (BoxTime boxTime in group.BoxTimes)
                {
                    connectedBoxList.Add(new ConnectedBoxTime
                    {
                        BoutID = jamMap[boxTime.JamID].BoutID,
                        BoxTime = boxTime,
                        PenaltyGroup = group
                    });
                }
            }
            return connectedBoxList;
        }

        private void ProcessBout(IGrouping<int, Jam> boutJamSet, IGrouping<int, PenaltyGroup> penaltyGroups, Dictionary<int, JamTimeEstimate> estimateMap)
        {
            // first, we do the approximations
            var periods = boutJamSet.GroupBy(j => j.IsFirstHalf);
            foreach (IEnumerable<Jam> jams in periods)
            {
                var jamIDs = jams.Select(j => j.ID);
                int totalSeconds = 1800;
                // we'll assume four clock stoppages per half, then add time in as necessary
                totalSeconds -= 30 * (jams.Count() - 5);
                int minTime = 0;
                int maxTime = 0;
                foreach (int jamID in jamIDs)
                {
                    minTime += estimateMap[jamID].Minimum;
                    maxTime += estimateMap[jamID].Maximum;
                }
                totalSeconds -= minTime;
                if (totalSeconds < 0)
                {
                    // just assume the min time for everything
                    foreach (Jam jam in jams)
                    {
                        var jamLimit = estimateMap[jam.ID];
                        jamLimit.Estimate = jamLimit.Minimum;
                    }
                }
                else
                {
                    int difference = maxTime - minTime;
                    double ratio = ((double)(totalSeconds)) / difference;
                    if (ratio > 1.0)
                    {
                        Console.WriteLine("Bout: " + jams.First().BoutID + " has a period that the maximums don't fill");
                        ratio = 1.0;
                    }
                    foreach (Jam jam in jams)
                    {
                        var jamLimit = estimateMap[jam.ID];
                        jamLimit.Estimate = jamLimit.Minimum + (int)((jamLimit.Maximum - jamLimit.Minimum) * ratio);
                    }
                }
            }
        }

        private Dictionary<int, int> CalculateBoxTimeEstimates(Dictionary<int, JamTimeEstimate> estimateMap)
        {
            Dictionary<int, int> boxTimeEstimateMap = new Dictionary<int, int>();
            Dictionary<int, int> groupDurationMap = CorrectJamLengthsForBoxes(estimateMap);
            if (_singleJamMultiPenalty == null)
            {
                _singleJamMultiPenalty = _connectedBoxList.GroupBy(cbl => new JamPlayerPair { JamID = cbl.BoxTime.JamID, PlayerID = cbl.BoxTime.PlayerID }).Where(g => g.Count() > 1).ToList();
            }

            foreach(PenaltyGroup penaltyGroup in _penaltyGroups)
            {
                int penaltyDuration = groupDurationMap[penaltyGroup.GroupID];
                var fullJamBoxes = penaltyGroup.BoxTimes.Where(bt => bt.StartedJamInBox == true && bt.EndedJamInBox);
                var partialJamBoxes = penaltyGroup.BoxTimes.Except(fullJamBoxes);
                foreach (BoxTime boxTime in fullJamBoxes)
                {
                    int jamTime = estimateMap[boxTime.JamID].Estimate;
                    boxTimeEstimateMap[boxTime.BoxTimeID] = estimateMap[boxTime.JamID].Estimate;
                    penaltyDuration -= estimateMap[boxTime.JamID].Estimate;
                }

                // apply remaining penalty time proportionally among jams
                List<JamTimeEstimate> boxEstimates = new List<JamTimeEstimate>();
                int availableJamDuration = 0;
                foreach (BoxTime boxTime in partialJamBoxes)
                {
                    JamTimeEstimate estimate = estimateMap[boxTime.JamID];
                    boxEstimates.Add(estimate);
                    var others = _singleJamMultiPenalty.Where(sjmp => sjmp.Key.JamID == boxTime.JamID && sjmp.Key.PlayerID == boxTime.PlayerID);
                    if (others.Any())
                    {
                        availableJamDuration += (estimate.Estimate / 2) - 1;
                    }
                    else
                    {
                        
                        availableJamDuration += estimate.Estimate;
                    }
                }

                float ratio = ((float)penaltyDuration) / availableJamDuration;
                foreach (BoxTime boxTime in partialJamBoxes)
                {
                    JamTimeEstimate thisJam = estimateMap[boxTime.JamID];
                    int time;
                    var others = _singleJamMultiPenalty.Where(sjmp => sjmp.Key.JamID == boxTime.JamID && sjmp.Key.PlayerID == boxTime.PlayerID);
                    if (others.Any())
                    {
                        time = (int)(ratio * estimateMap[boxTime.JamID].Estimate / 2) - 1;
                    }
                    else
                    {

                        time = (int)(ratio * estimateMap[boxTime.JamID].Estimate);
                    }
                    boxTimeEstimateMap[boxTime.BoxTimeID] = time;
                    penaltyDuration -= time;
                }

                // any leftover seconds can just pack wherever is first available
                if (penaltyDuration > 0)
                {
                    foreach (BoxTime boxTime in partialJamBoxes)
                    {
                        JamTimeEstimate thisJam = estimateMap[boxTime.JamID];
                        int boxTimeEstimate = boxTimeEstimateMap[boxTime.BoxTimeID];
                        if (thisJam.Estimate > boxTimeEstimate + 1)
                        {
                            if (thisJam.Estimate - boxTimeEstimate - 1 > penaltyDuration)
                            {
                                boxTimeEstimateMap[boxTime.BoxTimeID] += penaltyDuration;
                                penaltyDuration = 0;
                                break;
                            }
                            else
                            {
                                int difference = thisJam.Estimate - boxTimeEstimate - 1;
                                boxTimeEstimateMap[boxTime.BoxTimeID] += difference;
                                penaltyDuration -= difference;
                            }
                        }
                    }
                }
                if (penaltyDuration > 0)
                {
                    throw new InvalidDataException("Penalty group " + penaltyGroup.GroupID + " has duration issues");
                }
            }
            return boxTimeEstimateMap;
        }

        private Dictionary<int, int> CorrectJamLengthsForBoxes(Dictionary<int, JamTimeEstimate> estimateMap)
        {
            Dictionary<int, int> groupDurationMap = new Dictionary<int, int>();
            Dictionary<int, bool> durationIssues = new Dictionary<int, bool>();
            var crazyJammerBoxTimes = _crazyJammerBoxTimes.Select(cj => cj.BoxTime);
            bool exceptions = false;
            foreach (PenaltyGroup penaltyGroup in _penaltyGroups)
            {
                int penaltyDuration;
                if (penaltyGroup.BoxTimes.Count == 1 && crazyJammerBoxTimes.Contains(penaltyGroup.BoxTimes.First()))
                {
                    penaltyDuration = 10;
                    groupDurationMap[penaltyGroup.GroupID] = penaltyDuration;
                }
                else if (penaltyGroup.BoxTimes.Last().EndedJamInBox && _endJams.Contains(penaltyGroup.BoxTimes.Last().JamID))
                {
                    // if this was just the end jam, assume 10 seconds; otherwise, assume 20
                    penaltyDuration = penaltyGroup.BoxTimes.Count == 1 ? 10 : 20;
                    groupDurationMap[penaltyGroup.GroupID] = penaltyDuration;
                }
                else
                {
                    penaltyDuration = penaltyGroup.Penalties.Count * 30;
                    groupDurationMap[penaltyGroup.GroupID] = penaltyDuration;
                    // we want to make sure there are a few seconds on either side of the penalty
                    penaltyDuration += 10;
                }
                List<JamTimeEstimate> groupJams = new List<JamTimeEstimate>();
                foreach (BoxTime boxTime in penaltyGroup.BoxTimes)
                {
                    groupJams.Add(estimateMap[boxTime.JamID]);
                }
                int currentDuration = groupJams.Select(gj => gj.Estimate).Sum();
                int minimumDuration = groupJams.Select(gj => gj.Minimum).Sum();
                int maximumDuration = groupJams.Select(gj => gj.Maximum).Sum();
                foreach (BoxTime boxTime in penaltyGroup.BoxTimes)
                {
                    var others = _singleJamMultiPenalty.Where(sjmp => sjmp.Key.JamID == boxTime.JamID && sjmp.Key.PlayerID == boxTime.PlayerID);
                    if (others.Any())
                    {
                        currentDuration -= estimateMap[boxTime.JamID].Estimate / 2;
                    }
                }
                if (maximumDuration < penaltyDuration)
                {
                    durationIssues[penaltyGroup.BoxTimes.First().JamID] = true;
                    exceptions = true;
                }
                else if (currentDuration < penaltyDuration)
                {
                    // determine where we can extend 
                    int timeNeeded = penaltyDuration - currentDuration;
                    int timeAvailable = maximumDuration - currentDuration;
                    int boxCount = penaltyGroup.BoxTimes.Count;
                    float ratio = ((float)timeNeeded) / timeAvailable;

                    foreach (BoxTime boxTime in penaltyGroup.BoxTimes)
                    {
                        JamTimeEstimate thisJam = estimateMap[boxTime.JamID];
                        if (thisJam.Maximum > thisJam.Estimate)
                        {
                            // this was a partial service jam, so we can potentially manipulate its length
                            int addition = (int)(ratio * (thisJam.Maximum - thisJam.Estimate));
                            thisJam.Estimate += addition;
                            timeNeeded -= addition;
                        }
                    }
                    // any leftover seconds can just pack wherever is first available
                    if (timeNeeded > 0)
                    {
                        foreach (BoxTime boxTime in penaltyGroup.BoxTimes)
                        {
                            JamTimeEstimate thisJam = estimateMap[boxTime.JamID];
                            if (thisJam.Maximum > thisJam.Estimate)
                            {
                                if (thisJam.Maximum - thisJam.Estimate > timeNeeded)
                                {
                                    thisJam.Estimate += timeNeeded;
                                    break;
                                }
                                else
                                {
                                    int difference = thisJam.Maximum - thisJam.Estimate;
                                    thisJam.Estimate = thisJam.Maximum;
                                    timeNeeded -= difference;
                                }
                            }
                        }
                    }
                }
            }
            if (exceptions)
            {
                Console.WriteLine("Duration issues in:");
                foreach(int foo in durationIssues.Keys.OrderBy(f => f))
                {
                    Console.WriteLine(foo);
                }
                throw new InvalidDataException("duration issues");
            }
            return groupDurationMap;
        }
    }
}
