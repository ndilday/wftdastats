WITH 
JamSubset(JamID) AS
(
SELECT j.ID 
FROM Jam j
JOIN Bout b ON b.ID = j.BoutID
JOIN Team t1 ON b.HomeTeamID = t1.ID
JOIN Team t2 ON b.AwayTeamID = t2.ID
WHERE b.PlayDate > '2017-08-31' AND t1.TeamTypeID = 1 AND t2.TeamTypeID = 1 AND b.ID != 959 AND b.ID != 1099 AND b.ID != 1223
),
TPJammerPenalties(TeamPlayerID, Penalties) AS
(
	SELECT tp.ID, COUNT(p.ID)
	FROM Penalty p
	JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		jp.IsJammer = 1
	GROUP BY tp.ID
),
TPLeads(TeamPlayerID, Leads) AS
(
	SELECT tp.ID, COUNT(j.ID)
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		j.Lead = 1
	GROUP BY tp.ID
),
TPStarPasses(TeamPlayerID, StarPasses) AS
(
	SELECT tp.ID, COUNT(j.ID)
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		j.PassedStar = 1
	GROUP BY tp.ID
),
TPPoints(TeamPlayerID, Points) AS
(
	SELECT tp.ID, SUM(j.Points)
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		jp.IsJammer = 1
	GROUP BY tp.ID
),
TPService(TeamPlayerID, Bouts, Jams) AS
(
	SELECT tp.ID, COUNT(DISTINCT b.ID), COUNT(js.JamID)
	FROM Jam_Player jp
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	JOIN Jam j ON j.ID = js.JamID
	JOIN Bout b ON b.ID = j.BoutID
	WHERE
		jp.IsJammer = 1
	GROUP BY tp.ID
)
SELECT 
	l.Name AS TeamName, 
	p.Number, 
	p.Name, 
	tps.Bouts AS Games, 
	tps.Jams AS Jams, 
	tpp.Points,
	ISNULL(tpjp.Penalties, 0) AS Penalties, 
	ISNULL(tpl.Leads, 0)/CAST(tps.Jams AS float) AS LeadRate, 
	ISNULL(tpsp.StarPasses, 0) AS StarPasses
FROM Team_Player tp
JOIN Team t ON t.ID = tp.TeamID
JOIN League l ON l.ID = t.LeagueID
JOIN Player p ON p.ID = tp.PlayerID
JOIN TPService tps ON tps.TeamPlayerID = tp.ID
JOIN TPPoints tpp ON tpp.TeamPlayerID = tp.ID
LEFT JOIN TPJammerPenalties tpjp ON tpjp.TeamPlayerID = tp.ID
LEFT JOIN TPLeads tpl ON tpl.TeamPlayerID = tp.ID
LEFT JOIN TPStarPasses tpsp ON tpsp.TeamPlayerID = tp.ID
WHERE
	t.TeamTypeID = 1
ORDER BY l.Name, tps.Jams DESC