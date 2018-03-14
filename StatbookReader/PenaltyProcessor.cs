using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DerbyDataModels;
using StatbookReader.Models;

namespace StatbookReader
{
    class PenaltyProcessor
    {
        private IList<Jam> _jams;
        private Dictionary<int, Player> _players;
        private Jam _lastJam;
        public PenaltyProcessor(IList<Jam> jams, Dictionary<string, Player> homePlayers, Dictionary<string, Player> awayPlayers)
        {
            _jams = jams;
            _players = homePlayers.Values.ToDictionary(p => p.ID);
            foreach(Player player in awayPlayers.Values)
            {
                _players[player.ID] = player;
            }
            _lastJam = _jams.Last();
        }

        public List<PenaltyService> ProcessPenalties(Dictionary<int, Dictionary<int, IList<BoxTimeModel>>> homePlayerJamBoxTimeMap, 
                                                     Dictionary<int, PlayerPenaltiesModel> homePlayerPenaltiesModelMap,
                                                     Dictionary<int, int> homeEndJammerMap,
                                                     Dictionary<int, Dictionary<int, IList<BoxTimeModel>>> awayPlayerJamBoxTimeMap,
                                                     Dictionary<int, PlayerPenaltiesModel> awayPlayerPenaltiesModelMap,
                                                     Dictionary<int, int> awayEndJammerMap)
        {
            // create penalty and box time maps
            Dictionary<int, List<BoxTime>> allJamBoxTimeMap;
            TranslatePenaltyDictionary(homePlayerPenaltiesModelMap, out Dictionary<int, List<Penalty>> homeJamPenaltyMap, out Dictionary<int, List<Penalty>> homePlayerPenaltyMap);
            TranslatePenaltyDictionary(awayPlayerPenaltiesModelMap, out Dictionary<int, List<Penalty>> awayJamPenaltyMap, out Dictionary<int, List<Penalty>> awayPlayerPenaltyMap);
            TranslateBoxTimeDictionary(homePlayerJamBoxTimeMap, out Dictionary<int, List<BoxTime>> homeJamBoxTimeMap, out Dictionary<int, List<BoxTime>> homePlayerBoxTimeMap);
            TranslateBoxTimeDictionary(awayPlayerJamBoxTimeMap, out Dictionary<int, List<BoxTime>> awayJamBoxTimeMap, out Dictionary<int, List<BoxTime>> awayPlayerBoxTimeMap);
            var penaltyService = ProcessTeamPenalties(homePlayerBoxTimeMap, homeJamBoxTimeMap, homePlayerPenaltyMap, homeEndJammerMap);
            penaltyService.AddRange(ProcessTeamPenalties(awayPlayerBoxTimeMap, awayJamBoxTimeMap, awayPlayerPenaltyMap, awayEndJammerMap));
            
            var longServices = penaltyService.Where(ps => ps.Penalties.Count > 1).SelectMany(ps => ps.BoxTimes).ToList();
            allJamBoxTimeMap = new Dictionary<int, List<BoxTime>>();
            foreach(KeyValuePair<int, List<BoxTime>> kvp in homeJamBoxTimeMap)
            {
                if(!allJamBoxTimeMap.ContainsKey(kvp.Key))
                {
                    allJamBoxTimeMap[kvp.Key] = new List<BoxTime>(kvp.Value);
                }
                else
                {
                    allJamBoxTimeMap[kvp.Key].AddRange(kvp.Value);
                }
            }
            CheckForBoxTimeMismatch(allJamBoxTimeMap, longServices);
            return penaltyService;
        }

        private List<PenaltyService> ProcessTeamPenalties(Dictionary<int, List<BoxTime>> playerBoxTimeMap, Dictionary<int, List<BoxTime>> jamBoxTimeMap,
            Dictionary<int, List<Penalty>> playerPenaltyMap, Dictionary<int, int> endJammerMap)
        {
            SolveAmbiguousBoxTimes(playerBoxTimeMap, jamBoxTimeMap);
            
            var jamFullBoxMap = jamBoxTimeMap.ToDictionary(jbt => jbt.Key, jbt => jbt.Value.Count(bt => bt.StartedJamInBox == null || bt.StartedJamInBox == true) > 1);
            List<PenaltyService> penaltyService = new List<PenaltyService>();

            foreach(KeyValuePair<int, List<Penalty>> playerPenalties in playerPenaltyMap)
            {
                if (playerBoxTimeMap.ContainsKey(playerPenalties.Key))
                {
                    ProcessPlayerPenalties(playerPenalties.Value, playerBoxTimeMap[playerPenalties.Key], jamBoxTimeMap, jamFullBoxMap, penaltyService);
                }
            }
            // handle leftovers
            bool leftoverPenaltyMap = playerPenaltyMap.Any(kvp => kvp.Value.Any());
            bool leftoverBoxTimeMap = playerBoxTimeMap.Any(kvp => kvp.Value.Any());
            if(leftoverPenaltyMap || leftoverBoxTimeMap)
            {
                ProcessLeftoverBoxes(playerPenaltyMap, playerBoxTimeMap, jamFullBoxMap, endJammerMap, penaltyService);
                leftoverPenaltyMap = playerPenaltyMap.Any(kvp => kvp.Value.Any());
                leftoverBoxTimeMap = playerBoxTimeMap.Any(kvp => kvp.Value.Any());
                if (leftoverPenaltyMap || leftoverBoxTimeMap)
                {
                    throw new InvalidDataException("Leftovers!");
                }
            }
            return penaltyService;
        }

        private void CheckForBoxTimeMismatch(Dictionary<int, List<BoxTime>> jamBoxTimeMap, List<BoxTime> longServices)
        {
            bool exceptions = false;
            foreach(KeyValuePair<int, List<BoxTime>> kvp in jamBoxTimeMap)
            {
                // key is jamId
                // value is boxTimeList
                var boxEntireJam = kvp.Value.Where(bt => bt.StartedJamInBox != null && bt.StartedJamInBox == true && bt.EndedJamInBox);
                var fullyServed = kvp.Value.Where(bt => bt.StartedJamInBox != null && bt.StartedJamInBox == false && !bt.EndedJamInBox);
                if (fullyServed.Any() && boxEntireJam.Any())
                {
                    // if the short service box times are part of a multi-penalty service, they can be ignored
                    foreach (BoxTime fullBT in boxEntireJam)
                    {
                        if(!longServices.Contains(fullBT))
                        {
                            Jam jam = _jams.Single(j => j.ID == kvp.Key);
                            exceptions = true;
                            Console.WriteLine(jam.ToString() + ": Penalty timing problem");
                            break;
                        }
                    }
                }
            }
            if(exceptions)
            {
                throw new InvalidDataException("Penalty timing problems");
            }
        }

        private void ProcessPlayerPenalties(List<Penalty> playerPenalties, List<BoxTime> playerBoxTimes, 
                                            Dictionary<int, List<BoxTime>> jamBoxTimeMap, Dictionary<int, bool> jamFullBoxMap, 
                                            List<PenaltyService> penaltyService)
        {
            int penaltyIndex = 0;
            int serviceIndex = 0;
            List<PenaltyService> servicesBoxed = ConvertBoxTimeList(playerBoxTimes);
            PenaltyService lastPenaltyService = null;
            // iterate through each penalty for this player
            while (penaltyIndex < playerPenalties.Count)
            {
                Penalty penalty = playerPenalties[penaltyIndex];
                while(penalty.MatchingKey != null)
                {
                    penaltyIndex++;
                    if(penaltyIndex < playerPenalties.Count)
                    {
                        penalty = playerPenalties[penaltyIndex];
                    }
                    else
                    {
                        break;
                    }
                }
                if(penaltyIndex >= playerPenalties.Count)
                {
                    break;
                }
                PenaltyService currentService = null;
                BoxTime serviceStart = null;
                if (serviceIndex < servicesBoxed.Count)
                {
                    currentService = servicesBoxed[serviceIndex];
                    serviceStart = currentService.BoxTimes.First();
                    // if this service starts before this penalty, skip it
                    // if this service is a special case, skip it
                    while (CompareJams(_jams.First(j => j.ID == serviceStart.JamID), _jams.First(j => j.ID == penalty.JamID)) < 0 || currentService.ServiceKey != null)
                    {
                        serviceIndex++;
                        if (serviceIndex < servicesBoxed.Count)
                        {
                            currentService = servicesBoxed[serviceIndex];
                            serviceStart = currentService.BoxTimes.First();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if(serviceIndex < servicesBoxed.Count)
                {
                    if (serviceStart.JamID == penalty.JamID)
                    {
                        // same jam
                        if(serviceStart.StartedJamInBox == false)
                        {
                            // this penalty can be applied to this service
                            currentService.Penalties.Add(penalty);
                            playerPenalties.RemoveAt(penaltyIndex);
                            servicesBoxed.RemoveAt(serviceIndex);
                            penaltyService.Add(currentService);
                            lastPenaltyService = currentService;
                            foreach(BoxTime boxTime in currentService.BoxTimes)
                            {
                                playerBoxTimes.Remove(boxTime);
                            }
                        }
                        else if(serviceStart.StartedJamInBox == null)
                        {
                            // what do we want to do in this case?
                            Player player = _players.Values.First(p => p.ID == penalty.PlayerID);
                            Jam jam = _jams.First(j => j.ID == penalty.JamID);
                            Console.WriteLine(string.Format("{0}: {1} null box time start confusion", jam.ToString(), player.Number));
                            serviceIndex++;
                        }
                        else
                        {
                            // assume that this box time was for something else and move on
                            serviceIndex++;
                        }
                    }
                    else if (GetNextJamID(penalty.JamID) == serviceStart.JamID)
                    {
                        // next jam

                        // if they started the jam in the box, or
                        // if there were already two people who might have started in the box
                        if (serviceStart.StartedJamInBox == true || jamFullBoxMap[serviceStart.JamID])
                        {
                            // this penalty can be applied to this service
                            currentService.Penalties.Add(penalty);
                            playerPenalties.RemoveAt(penaltyIndex);
                            servicesBoxed.RemoveAt(serviceIndex);
                            penaltyService.Add(currentService);
                            lastPenaltyService = currentService;
                            foreach (BoxTime boxTime in currentService.BoxTimes)
                            {
                                playerBoxTimes.Remove(boxTime);
                            }
                        }
                        else if (serviceStart.StartedJamInBox == null)
                        {
                            // what do we want to do in this case?
                            Player player = _players.Values.First(p => p.ID == penalty.PlayerID);
                            Jam jam = _jams.First(j => j.ID == penalty.JamID);
                            throw new InvalidOperationException(string.Format("{0}: {1} null box time start confusion", jam.ToString(), player.Number));
                        }
                        else if (lastPenaltyService != null &&
                                CompareJams(_jams.First(j => j.ID == lastPenaltyService.BoxTimes.Last().JamID), _jams.First(j => j.ID == penalty.JamID)) >= 0)
                        {
                            // this penalty fits into the last service
                            lastPenaltyService.Penalties.Add(penalty);
                            playerPenalties.RemoveAt(penaltyIndex);
                        }
                        else
                        {
                            // unexpected not starting jam in box?
                            Player player = _players.Values.First(p => p.ID == penalty.PlayerID);
                            Jam jam = _jams.First(j => j.ID == penalty.JamID);

                            currentService.Penalties.Add(penalty);
                            playerPenalties.RemoveAt(penaltyIndex);
                            servicesBoxed.RemoveAt(serviceIndex);
                            penaltyService.Add(currentService);
                            lastPenaltyService = currentService;
                            foreach (BoxTime boxTime in currentService.BoxTimes)
                            {
                                playerBoxTimes.Remove(boxTime);
                            }
                            Console.WriteLine(string.Format("{0}: {1} did not start in box", jam.ToString(), player.Number));
                        }
                    }
                    else if (lastPenaltyService != null &&
                            CompareJams(_jams.First(j => j.ID == lastPenaltyService.BoxTimes.Last().JamID), _jams.First(j => j.ID == penalty.JamID)) >= 0)
                    {
                        // this penalty fits into the last service
                        lastPenaltyService.Penalties.Add(penalty);
                        playerPenalties.RemoveAt(penaltyIndex);
                    }
                    else
                    {
                        int prevJamID = GetPreviousJamID(serviceStart.JamID);
                        int nextJamID = GetNextJamID(penalty.JamID);

                        if (prevJamID != -1 && 
                            nextJamID != -1 && 
                            !serviceStart.IsJammer &&
                            serviceStart.StartedJamInBox == true && 
                            jamFullBoxMap.ContainsKey(prevJamID) &&
                            jamFullBoxMap[prevJamID] && 
                            GetNextJamID(nextJamID) == serviceStart.JamID)
                        {
                            Player player = _players.Values.First(p => p.ID == penalty.PlayerID);
                            Jam jam = _jams.First(j => j.ID == serviceStart.JamID);
                            Jam jam2 = _jams.First(j => j.ID == penalty.JamID);
                            Console.WriteLine(string.Format("{0}: {1} did not fit in the box in {2}", jam.ToString(), player.Number, jam2.ToString()));
                            // there was no room in the box during the next jam, 
                            // so see if they started in the box in the subsequent jam
                            // this penalty can be applied to this service
                            currentService.Penalties.Add(penalty);
                            playerPenalties.RemoveAt(penaltyIndex);
                            servicesBoxed.RemoveAt(serviceIndex);
                            penaltyService.Add(currentService);
                            lastPenaltyService = currentService;
                            foreach (BoxTime boxTime in currentService.BoxTimes)
                            {
                                playerBoxTimes.Remove(boxTime);
                            }
                        }
                        else if (penalty.PenaltyNumber < 7)
                        {
                            // subsequent jam
                            Player player = _players.Values.First(p => p.ID == penalty.PlayerID);
                            Jam jam = _jams.First(j => j.ID == penalty.JamID);
                            Console.WriteLine(string.Format("{0}: {1} missing penalty service", jam.ToString(), player.Number));
                            penaltyIndex++;
                        }
                        else
                        {
                            serviceIndex++;
                        }
                    }
                }
                else if (lastPenaltyService != null &&
                         CompareJams(_jams.First(j => j.ID == lastPenaltyService.BoxTimes.Last().JamID), _jams.First(j => j.ID == penalty.JamID)) >= 0 &&
                         (penalty.PenaltyNumber < 7 || lastPenaltyService.BoxTimes.Last().EndedJamInBox))
                {
                    // if this player previously served time in the box, that service may be for this penalty if:
                    // 1) this penalty occurred before that box service ended, and
                    // 2) if it was a foul out, the service did not end with the player back on the track
                    lastPenaltyService.Penalties.Add(penalty);
                    playerPenalties.RemoveAt(penaltyIndex);
                }
                else
                {
                    penaltyIndex++;
                }
            }
            // run through to see if any of the penalties could be given to 
        }

        private void ProcessLeftoverBoxes(Dictionary<int, List<Penalty>> playerPenaltyMap, Dictionary<int, List<BoxTime>> playerBoxTimeMap,
                                                          Dictionary<int, bool> jamFullBoxMap, Dictionary<int, int> endJammerMap, List<PenaltyService> services)
        {
            HandleSpecialCases(playerPenaltyMap, playerBoxTimeMap, services);
            // if there was a penalty in the last jam with no service, ignore it
            RemoveLastJamPenalties(playerPenaltyMap, playerBoxTimeMap, services);
            CheckForBadFoulOuts(services);

            // see if this is for a foul out
            
            bool leftoverBoxTimeMap = playerBoxTimeMap.Any(kvp => kvp.Value.Any());
            if (leftoverBoxTimeMap)
            {
                HandlePartialService(playerPenaltyMap, playerBoxTimeMap, services, true);
            }

            bool leftoverPenaltyMap = playerPenaltyMap.Any(kvp => kvp.Value.Any());
            if (leftoverPenaltyMap)
            {
                HandleLeftoverPenalties(playerPenaltyMap, playerBoxTimeMap, jamFullBoxMap, endJammerMap, services);
            }

            leftoverBoxTimeMap = playerBoxTimeMap.Any(kvp => kvp.Value.Any());
            if (leftoverBoxTimeMap)
            {
                HandlePartialService(playerPenaltyMap, playerBoxTimeMap, services, false);
            }

            bool throwException = false;
            foreach (KeyValuePair<int, List<Penalty>> playerPenalties in playerPenaltyMap)
            {
                if (playerPenalties.Value.Any())
                {
                    foreach (Penalty penalty in playerPenalties.Value)
                    {
                        Jam jam = _jams.First(j => j.ID == penalty.JamID);
                        Player player = _players[penalty.PlayerID];
                        Console.WriteLine(string.Format("{0}: Unhandled Penalty for {1} {2}.", jam.ToString(), player.Number, player.Name));
                        throwException = true;
                    }
                }
            }
            foreach (KeyValuePair<int, List<BoxTime>> playerBoxtimes in playerBoxTimeMap)
            {
                if (playerBoxtimes.Value.Any())
                {
                    foreach (BoxTime boxTime in playerBoxtimes.Value)
                    {
                        Jam jam = _jams.First(j => j.ID == boxTime.JamID);
                        Player player = _players[boxTime.PlayerID];
                        Console.WriteLine(string.Format("{0}: Unhandled box time for {1} {2}.", jam.ToString(), player.Number, player.Name));
                        throwException = true;
                    }
                }
            }
            if (throwException)
            {
                throw new InvalidDataException("Unhandled penalties or box times.");
            }
        }

        private void HandleSpecialCases(Dictionary<int, List<Penalty>> playerPenaltyMap, Dictionary<int, List<BoxTime>> playerBoxTimeMap, List<PenaltyService> services)
        {
            Dictionary<char?, PenaltyService> specialServiceMap = new Dictionary<char?, PenaltyService>();
            var matchedPenalties = playerPenaltyMap.Values.SelectMany(p => p).Where(p => p.MatchingKey != null).ToList();

            foreach(Penalty specialPenalty in matchedPenalties)
            {
                if (!specialServiceMap.ContainsKey(specialPenalty.MatchingKey))
                {
                    var matchingBoxes = playerBoxTimeMap.Values.SelectMany(bt => bt).Where(bt => bt.MatchingKey == specialPenalty.MatchingKey).ToList();
                    if (!matchingBoxes.Any())
                    {
                        throw new InvalidDataException("No box times match the special case character " + specialPenalty.MatchingKey);
                    }
                    PenaltyService service = new PenaltyService();
                    // find all boxes that match this special character
                    foreach (BoxTime bt in matchingBoxes)
                    {
                        service.BoxTimes.Add(bt);
                        playerBoxTimeMap[bt.PlayerID].Remove(bt);
                    }
                    specialServiceMap[specialPenalty.MatchingKey] = service;
                    services.Add(service);
                }

                specialServiceMap[specialPenalty.MatchingKey].Penalties.Add(specialPenalty);
                playerPenaltyMap[specialPenalty.PlayerID].Remove(specialPenalty);
            }
        }

        private void CheckForBadFoulOuts(List<PenaltyService> services)
        {
            var returnedSeventh = services.Where(s => s.Penalties.Count > 0 && s.BoxTimes.Count > 0 && s.Penalties.Last().PenaltyNumber > 6 && !s.BoxTimes.Last().EndedJamInBox && s.BoxTimes.Last().PlayerID == s.Penalties.Last().PlayerID);
            foreach (PenaltyService service in returnedSeventh)
            {
                Jam penaltyJam = _jams.First(j => j.ID == service.Penalties.Last().JamID);
                Player penaltyPlayer = _players[service.Penalties.Last().PlayerID];

                // there is a possibility that a player was accidentally allowed to continue to play
                // if this is a multi-jam penalty service, log a warning and do nothing
                if (service.BoxTimes.Count > 1)
                {
                    Console.WriteLine(string.Format("{0}: penalty by {1} {2} was 7th, but continued service in the next jam?", penaltyJam.ToString(), penaltyPlayer.Number, penaltyPlayer.Name));
                }
                else
                {
                    Console.WriteLine(string.Format("{0}: penalty by {1} {2} was 7th, but returned to track!", penaltyJam.ToString(), penaltyPlayer.Number, penaltyPlayer.Name));
                    service.BoxTimes.Last().EndedJamInBox = true;
                    service.BoxTimes.Last().Finished = true;
                }
            }
        }

        private void HandlePartialService(Dictionary<int, List<Penalty>> playerPenaltyMap, Dictionary<int, List<BoxTime>> playerBoxMap, List<PenaltyService> services, bool requireSingleMatch)
        {
            // if someone picked up their 7+ penalty and either:
            // 1) didn't serve any time at all, or
            // 2) was still "in the box" at the end of the jam
            var incompleteService = services.Where(s => s.BoxTimes.Count > 0 && s.BoxTimes.Last().EndedJamInBox && !s.BoxTimes.Last().Finished && s.BoxTimes.Last().JamID != _lastJam.ID);
            var leftoverBoxes = playerBoxMap.Values.SelectMany(p => p).Where(b => b.FullService != true).ToList();
            foreach (PenaltyService service in incompleteService)
            {
                Penalty lastPenalty = service.Penalties.Last();
                BoxTime lastBox = service.BoxTimes.Last();
                Jam penaltyJam = _jams.First(j => j.ID == lastPenalty.JamID);
                Player penaltyPlayer = _players[lastPenalty.PlayerID];

                // when the penalized served some time, we can't be sure it wasn't sufficient time
                var boxes = leftoverBoxes.Where(bt => bt.JamID == lastPenalty.JamID + 1 && (bt.IsJammer == lastBox.IsJammer) && (bt.IsPivot == lastBox.IsPivot) && (bt.FullService != true));
                BoxTime bestBox = boxes.Any() && (!requireSingleMatch || boxes.Count() == 1) ? boxes.First() : null;

                if (bestBox != null)
                {
                    Jam boxTimeJam = _jams.First(j => j.ID == bestBox.JamID);
                    Player boxTimePlayer = _players[bestBox.PlayerID];
                    Console.WriteLine(string.Format("{0}: penalty by {1} {2} continued service by {3} {4}", penaltyJam.ToString(), 
                                                    penaltyPlayer.Number, penaltyPlayer.Name,
                                                    boxTimePlayer.Number, boxTimePlayer.Name));
                    service.BoxTimes.Add(bestBox);
                    leftoverBoxes.Remove(bestBox);
                    playerBoxMap[bestBox.PlayerID].Remove(bestBox);
                    //if (bestBox.StartedJamInBox == null)
                    //{
                        bestBox.StartedJamInBox = true;
                    //}

                    // get the ending jam of the service
                    while (bestBox.EndedJamInBox && bestBox.JamID < _lastJam.ID)
                    {
                        BoxTime previousBox = bestBox;
                        bestBox = leftoverBoxes.Where(bt => bt.StartedJamInBox == true && bt.JamID == bestBox.JamID + 1 && bt.PlayerID == bestBox.PlayerID).FirstOrDefault();
                        if (bestBox == null)
                        {
                            boxTimeJam = _jams.First(j => j.ID == previousBox.JamID);
                            string errorString = string.Format("{0}: box time by {1} {2} does not appear correctly finished.",
                                                               boxTimeJam.ToString(), boxTimePlayer.Number, boxTimePlayer.Name);
                            Console.WriteLine(errorString);
                            throw new InvalidDataException(errorString);
                        }
                        service.BoxTimes.Add(bestBox);
                        leftoverBoxes.Remove(bestBox);
                        playerBoxMap[bestBox.PlayerID].Remove(bestBox);
                    }

                    // see if the serving player picked up any other penalties as part of this service
                    if (playerPenaltyMap.Keys.Contains(boxTimePlayer.ID))
                    {
                        var nextPlayerPenalty = playerPenaltyMap[boxTimePlayer.ID].FirstOrDefault(p => p.JamID >= service.BoxTimes.Last().JamID);
                        if (nextPlayerPenalty != null)
                        {
                            // see if there's a box time for that player that fits that penalty
                            var nextPlayerBoxTime = playerBoxMap[boxTimePlayer.ID].FirstOrDefault(bt => bt.JamID >= nextPlayerPenalty.JamID);
                            if (nextPlayerBoxTime == null || nextPlayerBoxTime.JamID > nextPlayerPenalty.JamID + 1)
                            {
                                service.Penalties.Add(nextPlayerPenalty);
                                playerPenaltyMap[nextPlayerPenalty.PlayerID].Remove(nextPlayerPenalty);
                            }
                        }
                    }
                }
            }
        }

        private void HandleLeftoverPenalties(Dictionary<int, List<Penalty>> playerPenaltyMap, Dictionary<int, List<BoxTime>> playerBoxMap, 
                                             Dictionary<int, bool> jamFullBoxMap, Dictionary<int, int> endJammerMap, List<PenaltyService> services)
        {
            var leftoverPenalties = playerPenaltyMap.Values.SelectMany(p => p).ToList();
            var leftoverBoxes = playerBoxMap.Values.SelectMany(p => p).ToList();

            // handle penalties with no time served
            while (leftoverPenalties.Count > 0)
            {
                Penalty penalty = leftoverPenalties[0];
                Jam penaltyJam = _jams.First(j => j.ID == penalty.JamID);
                Player penaltyPlayer = _players[penalty.PlayerID];
                bool wasEndJammer = endJammerMap.ContainsKey(penaltyJam.ID) && endJammerMap[penaltyJam.ID] == penaltyPlayer.ID;
                PenaltyService service = new PenaltyService();
                service.Penalties.Add(penalty);
                // if no time was served, it's conceivable the penalty didn't get served until later on
                // TODO: confirm jammer penalty+service matching
                var matchingBoxes = leftoverBoxes
                    .Where(bt => (bt.JamID > penalty.JamID && bt.IsJammer == wasEndJammer) || 
                                 (bt.JamID == penalty.JamID && bt.PlayerID == penalty.PlayerID))
                    .OrderBy(bt => bt.JamID);
                if (matchingBoxes.Any())
                {
                    BoxTime bestBox = matchingBoxes.First();
                    Jam boxTimeJam = _jams.First(j => j.ID == bestBox.JamID);
                    Player boxTimePlayer = _players[bestBox.PlayerID];
                    int nextJamID = GetNextJamID(penaltyJam.ID);
                    if (boxTimeJam.ID - penaltyJam.ID > 1 && (!jamFullBoxMap.ContainsKey(nextJamID) || !jamFullBoxMap[nextJamID]))
                    {
                        string errorString = string.Format("{0}: penalty by {1} {2} does not appear correctly served.",
                                                           penaltyJam.ToString(), penaltyPlayer.Number, penaltyPlayer.Name);
                        Console.WriteLine(errorString);
                        throw new InvalidDataException(errorString);
                    }
                    Console.WriteLine(string.Format("{0}: penalty by {1} {2} served by {3} {4}", penaltyJam.ToString(),
                                                    penaltyPlayer.Number, penaltyPlayer.Name,
                                                    boxTimePlayer.Number, boxTimePlayer.Name));
                    service.BoxTimes.Add(bestBox);
                    leftoverBoxes.Remove(bestBox);
                    playerBoxMap[bestBox.PlayerID].Remove(bestBox);
                    leftoverPenalties.Remove(penalty);
                    playerPenaltyMap[penalty.PlayerID].Remove(penalty);

                    // see if this penalty has friends
                    var hangersOn = leftoverPenalties.Where(p => p.JamID == penalty.JamID && p.PlayerID == penalty.PlayerID && p.PenaltyNumber != penalty.PenaltyNumber).ToList();
                    foreach (Penalty extra in hangersOn)
                    {
                        service.Penalties.Add(extra);
                        leftoverPenalties.Remove(extra);
                        playerPenaltyMap[extra.PlayerID].Remove(extra);
                    }

                    // get the ending jam of the service
                    while (bestBox.EndedJamInBox && bestBox.JamID < _lastJam.ID)
                    {
                        BoxTime previousBox = bestBox;
                        bestBox = leftoverBoxes.Where(bt => bt.StartedJamInBox == true && bt.JamID == bestBox.JamID + 1 && bt.PlayerID == bestBox.PlayerID).FirstOrDefault();
                        if (bestBox == null)
                        {
                            boxTimeJam = _jams.First(j => j.ID == previousBox.JamID);

                            throw new InvalidDataException(boxTimeJam.ToString() + ": box time by #" + boxTimePlayer.Number + " does not appear correctly finished.");
                        }
                        service.BoxTimes.Add(bestBox);
                        leftoverBoxes.Remove(bestBox);
                        playerBoxMap[bestBox.PlayerID].Remove(bestBox);
                    }
                    services.Add(service);
                }
                else
                {
                    Jam jam = _jams.First(j => j.ID == penalty.JamID);
                    Player player = _players[penalty.PlayerID];
                    Console.WriteLine(string.Format("{0}: Unhandled Penalty for {1} {2}.", jam.ToString(), player.Number, player.Name));
                    throw new InvalidDataException(string.Format("{0}: Unhandled Penalty for {1} {2}.", jam.ToString(), player.Number, player.Name));
                }
            }
        }

        private void RemoveLastJamPenalties(Dictionary<int, List<Penalty>> playerPenaltyMap, Dictionary<int, List<BoxTime>> playerBoxTimeMap, List<PenaltyService> services)
        {
            var endGame = playerPenaltyMap.Values.SelectMany(p => p).Where(p => p.JamID == _lastJam.ID).ToList();
            foreach (Penalty penalty in endGame)
            {
                if(playerBoxTimeMap.ContainsKey(penalty.PlayerID) && playerBoxTimeMap[penalty.PlayerID].Any() && playerBoxTimeMap[penalty.PlayerID].Last().JamID == _lastJam.ID)
                {
                    continue;
                }
                Jam penaltyJam = _jams.First(j => j.ID == penalty.JamID);
                Player penaltyPlayer = _players[penalty.PlayerID];
                Console.WriteLine(penaltyJam.ToString() + ": penalty by #" + penaltyPlayer.Number + " in last jam assumed to not have been served");
                playerPenaltyMap[penalty.PlayerID].Remove(penalty);
                PenaltyService service = new PenaltyService();
                service.Penalties.Add(penalty);
                services.Add(service);
            }
        }

        private List<PenaltyService> ConvertBoxTimeList(List<BoxTime> playerBoxTimes)
        {
            List<PenaltyService> penaltyService = new List<PenaltyService>();
            int boxTimeIndex = 0;
            while(boxTimeIndex < playerBoxTimes.Count)
            {
                PenaltyService service = new PenaltyService();
                BoxTime lastBoxTime = playerBoxTimes[boxTimeIndex];
                service.BoxTimes.Add(lastBoxTime);
                if(lastBoxTime.MatchingKey != null)
                {
                    service.ServiceKey = lastBoxTime.MatchingKey;
                }
                boxTimeIndex++;

                while (lastBoxTime.EndedJamInBox && boxTimeIndex < playerBoxTimes.Count)
                {
                    int nextJamID = GetNextJamID(lastBoxTime.JamID);
                    BoxTime nextBoxTime = playerBoxTimes[boxTimeIndex];
                    if(nextBoxTime.JamID == nextJamID && (nextBoxTime.StartedJamInBox == null || nextBoxTime.StartedJamInBox == true))
                    {
                        nextBoxTime.StartedJamInBox = true;
                        service.BoxTimes.Add(nextBoxTime);
                        lastBoxTime = nextBoxTime;
                        boxTimeIndex++;
                    }
                    else
                    {
                        Jam jam = _jams.First(j => j.ID == lastBoxTime.JamID);
                        Player player = _players[lastBoxTime.PlayerID];
                        Console.WriteLine(string.Format("{0}: No box time continuation for {1} {2}", jam.ToString(), player.Number, player.Name));
                        break;
                    }
                }
                penaltyService.Add(service);
            }
            return penaltyService;
        }

        private void TranslatePenaltyDictionary(Dictionary<int, PlayerPenaltiesModel> playerPenaltiesModelMap, 
                                                out Dictionary<int, List<Penalty>> jamPenaltyMap, out Dictionary<int, List<Penalty>> playerPenaltyMap)
        {
            jamPenaltyMap = new Dictionary<int, List<Penalty>>(50);
            playerPenaltyMap = new Dictionary<int, List<Penalty>>(28);

            foreach (KeyValuePair<int, PlayerPenaltiesModel> playerPenalty in playerPenaltiesModelMap)
            {
                int penaltyCount = 1;
                foreach (PenaltyModel penaltyModel in playerPenalty.Value.Penalties)
                {
                    Jam jam = _jams.First(j => j.IsFirstHalf == penaltyModel.IsFirstHalf && j.JamNumber == penaltyModel.JamNumber);
                    if (!jamPenaltyMap.ContainsKey(jam.ID))
                    {
                        jamPenaltyMap[jam.ID] = new List<Penalty>();
                    }
                    if (!playerPenaltyMap.ContainsKey(playerPenalty.Key))
                    {
                        playerPenaltyMap[playerPenalty.Key] = new List<Penalty>();
                    }
                    Penalty penalty = new Penalty
                    {
                        PlayerID = playerPenalty.Key,
                        PenaltyCode = penaltyModel.PenaltyCode,
                        PenaltyNumber = penaltyCount,
                        JamID = jam.ID,
                        MatchingKey = penaltyModel.SpecificKey
                    };
                    penaltyCount++;
                    jamPenaltyMap[jam.ID].Add(penalty);
                    playerPenaltyMap[playerPenalty.Key].Add(penalty);
                }
            }
        }

        private static void TranslateBoxTimeDictionary(Dictionary<int, Dictionary<int, IList<BoxTimeModel>>> playerJamBoxTimeMap, out Dictionary<int, List<BoxTime>> jamBoxTimeMap, out Dictionary<int, List<BoxTime>> playerBoxTimeMap)
        {
            jamBoxTimeMap = new Dictionary<int, List<BoxTime>>();
            playerBoxTimeMap = new Dictionary<int, List<BoxTime>>();
            foreach (KeyValuePair<int, Dictionary<int, IList<BoxTimeModel>>> pjbtMap in playerJamBoxTimeMap)
            {
                if (!playerBoxTimeMap.ContainsKey(pjbtMap.Key))
                {
                    playerBoxTimeMap[pjbtMap.Key] = new List<BoxTime>();
                }
                foreach (KeyValuePair<int, IList<BoxTimeModel>> jamBoxTimes in pjbtMap.Value)
                {
                    if (jamBoxTimes.Value.Any())
                    {
                        if (!jamBoxTimeMap.ContainsKey(jamBoxTimes.Key))
                        {
                            jamBoxTimeMap[jamBoxTimes.Key] = new List<BoxTime>();
                        }
                        foreach (BoxTimeModel model in jamBoxTimes.Value)
                        {
                            BoxTime boxTime = new BoxTime
                            {
                                JamID = jamBoxTimes.Key,
                                PlayerID = pjbtMap.Key,
                                EndedJamInBox = !model.Exited,
                                StartedJamInBox = model.Started,
                                IsJammer = model.IsJammer,
                                IsPivot = model.IsPivot,
                                Finished = false,
                                FullService = model.IsFullService,
                                MatchingKey = model.SpecialKey
                            };
                            if (boxTime.MatchingKey != null)
                            {
                                Console.WriteLine(string.Format("Special case code {0} encountered in jamId {1}", boxTime.MatchingKey, boxTime.JamID));
                            }
                            jamBoxTimeMap[jamBoxTimes.Key].Add(boxTime);
                            playerBoxTimeMap[pjbtMap.Key].Add(boxTime);
                        }
                    }
                }
            }
        }

        private void SolveAmbiguousBoxTimes(Dictionary<int, List<BoxTime>> playerBoxTimeMap, Dictionary<int, List<BoxTime>> jamBoxTimeMap)
        {
            foreach (KeyValuePair<int, List<BoxTime>> playerBoxTime in playerBoxTimeMap)
            {
                int boxTimeCount = playerBoxTime.Value.Count;
                // ignore the first boxTime; if we don't know whether the player started in the box for it,
                // it's an ambiguous case we won't solve until the end
                for(int i = 1; i < boxTimeCount; i++)
                {
                    var currentBoxTime = playerBoxTime.Value[i];
                    int previousJamID = GetPreviousJamID(currentBoxTime.JamID);
                    if(currentBoxTime.StartedJamInBox == null)
                    {
                        /*if(currentBoxTime.MatchingKey != null)
                        {
                            currentBoxTime.StartedJamInBox = false;
                            continue;
                        }*/
                        var previousBoxTime = playerBoxTime.Value[i - 1];
                        // if the player ended the previous jam in the box, this box time started in the box
                        if(previousBoxTime.EndedJamInBox && previousBoxTime.JamID == previousJamID)
                        {
                            currentBoxTime.StartedJamInBox = true;
                        }
                    }
                }
            }
            foreach(KeyValuePair<int, List<BoxTime>> jamBoxTime in jamBoxTimeMap)
            {
                int boxTimeCount = jamBoxTime.Value.Count;
                for(int i = 0; i < boxTimeCount; i++)
                {
                    var currentBoxTime = jamBoxTime.Value[i];
                    int previousJamID = GetPreviousJamID(currentBoxTime.JamID);
                    if (currentBoxTime.StartedJamInBox == null)
                    {
                        if (previousJamID == -1)
                        {
                            // if this is the first jam, the player could not have started in the box
                            currentBoxTime.StartedJamInBox = false;
                        }
                        else if(!jamBoxTimeMap.ContainsKey(previousJamID))
                        {
                            // there were no box times in the previous jam, so this can't be a case of starting in the box
                            currentBoxTime.StartedJamInBox = false;
                        }
                        else if (jamBoxTimeMap[previousJamID].Count(bt => bt.EndedJamInBox) - jamBoxTimeMap[currentBoxTime.JamID].Count(bt => bt.StartedJamInBox == true) <= 0)
                        {
                            // if there are enough people starting in the box to cover what's needed from people ending in the box, assume they did not start in the box
                            currentBoxTime.StartedJamInBox = false;
                        }
                        else if(currentBoxTime.IsJammer && !jamBoxTimeMap[previousJamID].Any(bt => bt.IsJammer && bt.EndedJamInBox))
                        {
                            currentBoxTime.StartedJamInBox = false;
                        }
                    }
                }
            }
        }

        private int GetPreviousJamID(int jamID)
        {
            Jam currentJam = _jams.First(j => j.ID == jamID);
            if(currentJam.JamNumber == 1)
            {
                if(currentJam.IsFirstHalf)
                {
                    return -1;
                }
                else
                {
                    // we need the last jam of the first half
                    int lastJamNumber = _jams.Where(j => j.IsFirstHalf).Select(j => j.JamNumber).Max();
                    return _jams.First(j => j.IsFirstHalf && j.JamNumber == lastJamNumber).ID;
                }
            }
            else
            {
                return _jams.First(j => j.IsFirstHalf == currentJam.IsFirstHalf && j.JamNumber == currentJam.JamNumber - 1).ID;
            }
        }

        private int GetNextJamID(int jamID)
        {
            Jam currentJam = _jams.First(j => j.ID == jamID);
            var nextJam = _jams.FirstOrDefault(j => j.IsFirstHalf == currentJam.IsFirstHalf && j.JamNumber == currentJam.JamNumber + 1);
            if(nextJam == null && currentJam.IsFirstHalf)
            {
                nextJam = _jams.First(j => !j.IsFirstHalf && j.JamNumber == 1);
            }
            
            return nextJam == null ? -1 : nextJam.ID;
        }

        private int CompareJams(Jam jam1, Jam jam2)
        {
            if (jam1.IsFirstHalf && !jam2.IsFirstHalf)
            {
                return -1;
            }
            else if (jam2.IsFirstHalf && !jam1.IsFirstHalf)
            {
                return 1;
            }
            else if (jam1.JamNumber < jam2.JamNumber)
            {
                return -1;
            }
            else if (jam2.JamNumber < jam1.JamNumber)
            {
                return 1;
            }
            else return 0;
        }

        private bool IsBoxTimePossibleForPenalty(Jam penaltyJam, Jam boxTimeJam)
        {
            return CompareJams(penaltyJam, boxTimeJam) >= 0;
        }
    }
}
