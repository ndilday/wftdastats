WITH 
JamSubset(JamID) AS
(
SELECT j.ID 
FROM Jam j
JOIN Bout b ON b.ID = j.BoutID
JOIN Team t1 ON b.HomeTeamID = t1.ID
JOIN Team t2 ON b.AwayTeamID = t2.ID
WHERE b.PlayDate > '2018-01-01' AND t1.TeamTypeID = 1 AND t2.TeamTypeID = 1 AND b.ID != 959 AND b.ID != 1099 AND b.ID != 1223
),
TPBlockerPenalties(TeamPlayerID, Penalties) AS
(
	SELECT tp.ID, COUNT(p.ID)
	FROM Penalty p
	JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		jp.IsJammer = 0
	GROUP BY tp.ID
),
JamTeamLead(JamID, TeamID, JamPlayerID) AS
(
	SELECT jp.JamID, tp.TeamID, jp.ID
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		j.Lead = 1
),
TPReceivedStar(TeamPlayerID, StarPasses) AS
(
	SELECT tp.ID, COUNT(j.ID)
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		j.ReceivedStar = 1
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
		jp.IsJammer = 0
	GROUP BY tp.ID
),
TPPivotService(TeamPlayerID, Jams) AS
(
	SELECT tp.ID, COUNT(js.JamID)
	FROM Jam_Player jp
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	JOIN Jam j ON j.ID = js.JamID
	WHERE
		jp.IsPivot = 1
	GROUP BY tp.ID
)
SELECT 
	l.Name AS TeamName, 
	p.Number, 
	p.Name, 
	tps.Bouts AS Games, 
	tps.Jams AS Jams, 
	ISNULL(tpbp.Penalties, 0) AS Penalties, 
	ISNULL(tpps.Jams, 0) AS JamsAsPivot,
	ISNULL(tprs.StarPasses, 0) AS StarPasses
FROM Team_Player tp
JOIN Team t ON t.ID = tp.TeamID
JOIN League l ON l.ID = t.LeagueID
JOIN Player p ON p.ID = tp.PlayerID
JOIN TPService tps ON tps.TeamPlayerID = tp.ID
LEFT JOIN TPBlockerPenalties tpbp ON tpbp.TeamPlayerID = tp.ID
LEFT JOIN TPPivotService tpps ON tpps.TeamPlayerID = tp.ID
LEFT JOIN TPReceivedStar tprs ON tprs.TeamPlayerID = tp.ID
WHERE
	t.TeamTypeID = 1
ORDER BY l.Name, tps.Jams DESC