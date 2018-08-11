SELECT l.Name, b.PlayDate, b.ID, ROW_NUMBER() OVER(PARTITION BY l.Name ORDER BY b.PlayDate)
FROM League l
JOIN Team t ON t.LeagueID = l.ID
JOIN Bout b ON b.HomeTeamID = t.ID OR b.AwayTeamID = t.ID
WHERE
	t.TeamTypeID = 1 AND
	b.PlayDate > '2014-12-01'
ORDER BY l.Name, b.PlayDate

SELECT l.Name, t.Name, COUNT(b.Id)
FROM League l
JOIN Team t ON t.LeagueID = l.ID
JOIN Bout b ON b.HomeTeamID = t.ID OR b.AwayTeamID = t.ID
WHERE
	t.TeamTypeID = 1 AND
	b.PlayDate > '2014-12-01'
GROUP BY l.Name, t.Name
ORDER BY l.Name

SELECT l.id, l.Name, t.Name
FROM League l
JOIN Team t ON t.LeagueID = l.ID

WHERE
	t.TeamTypeID = 1
ORDER BY l.Name


SELECT * FROM Player where name like 'Boom%'

SELECT * FROM Team_Player WHERE PlayerID = 5042 OR PlayerID = 966

UPDATE Jam_Player SET Team_PlayerID = 1148 WHERE Team_PlayerID = 6403
DELETE FROM Team_Player WHERE PlayerID = 5042
DELETE FROM Player WHERE ID = 5042

SELECT * FROM Team WHERE ID = 72
SELECT * FROM League WHERE ID = 155