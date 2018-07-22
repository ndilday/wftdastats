SELECT b.id, hl.Name as League1, al.Name as League2, b.PlayDate
FROM Bout b
JOIN Team ht ON ht.ID = b.HomeTeamID
JOIN League hl ON hl.ID = ht.LeagueID
JOIN Team at ON at.ID = b.AwayTeamID
JOIN League al ON al.ID = at.LeagueID
WHERE b.PlayDate > '01-01-2017' AND ht.TeamTypeID = 1 AND at.TeamTypeID = 1
UNION
SELECT b.ID, al.Name as League1, hl.Name as League2, b.PlayDate
FROM Bout b
JOIN Team ht ON ht.ID = b.HomeTeamID
JOIN League hl ON hl.ID = ht.LeagueID
JOIN Team at ON at.ID = b.AwayTeamID
JOIN League al ON al.ID = at.LeagueID
WHERE b.PlayDate > '01-01-2017' AND ht.TeamTypeID = 1 AND at.TeamTypeID = 1
ORDER BY League1 ASC, PlayDate ASC
