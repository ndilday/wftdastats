-- bout count for all teams
SELECT l.Name, COUNT(b.Id)
FROM Bout b
JOIN Team t ON b.HomeTeamId = t.Id OR b.AwayTeamId = t.Id
JOIN League l ON t.LeagueId = l.ID
WHERE
	b.PlayDate > '2016-01-01' AND
	t.TeamTypeId = 1
GROUP BY l.Name
ORDER BY l.Name

-- Team specific bout breakdown
SELECT hl.Name, al.Name, b.PlayDate
FROM Bout b
JOIN Team ht ON b.HomeTeamId = ht.Id
JOIN League hl ON ht.LeagueId = hl.ID
JOIN Team at ON b.AwayTeamID = at.Id
JOIN League al ON at.LeagueID = al.Id
WHERE
	b.PlayDate > '2016-01-01' AND
	ht.TeamTypeId = 1 AND
	at.TeamTypeId = 1 AND
	(hl.Name LIKE 'Brandywine %' OR al.Name LIKE 'Brandywine %')
ORDER BY b.PlayDate

-- jam data
SELECT b.PlayDate, hl.Name, al.Name, j.IsFirstHalf, j.JamNum
FROM Jam j
JOIN Bout b ON b.Id = j.BoutID
JOIN Team ht ON b.HomeTeamId = ht.Id
JOIN League hl ON ht.LeagueId = hl.ID
JOIN Team at ON b.AwayTeamID = at.Id
JOIN League al ON at.LeagueID = al.Id
WHERE j.ID IN (40728, 40939, 41102)

select * FROM JamTimeEstimate where jamid = 41102