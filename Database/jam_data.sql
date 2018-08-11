SELECT b.PlayDate, hl.Name, al.Name, j.IsFirstHalf, j.JamNum
FROM Jam j
JOIN Bout b ON b.ID = j.BoutID
JOIN Team ht ON ht.ID = b.HomeTeamID
JOIN League hl ON hl.ID = ht.LeagueID
JOIN Team t2 ON t2.ID = b.AwayTeamID
JOIN League al ON al.ID = t2.LeagueID
WHERE j.ID IN (55190, 61262, 61673)