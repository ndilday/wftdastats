-- Team stats
-- jammer penalties/game
-- blocker penalties/game
-- star passes/game

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
TeamJammerPenalties(TeamID, Penalties) AS
(
	SELECT tp.TeamID, COUNT(p.ID)
	FROM Penalty p
	JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		jp.IsJammer = 1
	GROUP BY tp.TeamID
),
TeamBlockerPenalties(TeamID, Penalties) AS
(
	SELECT tp.TeamID, COUNT(p.ID)
	FROM Penalty p
	JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		jp.IsJammer = 0
	GROUP BY tp.TeamID
),
TeamLeads(TeamID, Leads) AS
(
	SELECT tp.TeamID, COUNT(j.ID)
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		j.Lead = 1
	GROUP BY tp.TeamID
),
TeamStarPasses(TeamID, StarPasses) AS
(
	SELECT tp.TeamID, COUNT(j.ID)
	FROM JamSubset js
	JOIN Jam_Player jp ON jp.JamID = js.JamID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	LEFT JOIN Jammer j ON j.Jam_PlayerID = jp.ID
	WHERE
		j.PassedStar = 1
	GROUP BY tp.TeamID
),
CollectedStats(TeamName, Bouts, Jams, JammerPenalties, BlockerPenalties, StarPasses, Leads) AS
(
	SELECT 
		l.Name, 
		COUNT(DISTINCT b.ID), 
		COUNT(js.JamID), 
		tjp.Penalties, 
		tbp.Penalties, 
		ISNULL(tsp.StarPasses,0),
		tl.Leads
	FROM League l
	JOIN Team t ON t.LeagueID = l.ID
	JOIN TeamJammerPenalties tjp ON tjp.TeamID = t.ID
	JOIN TeamBlockerPenalties tbp ON tbp.TeamID = t.ID
	JOIN TeamLeads tl ON tl.TeamID = t.ID
	LEFT JOIN TeamStarPasses tsp ON tsp.TeamID = t.ID
	JOIN Bout b ON b.AwayTeamID = t.ID OR b.HomeTeamID = t.ID
	JOIN Jam j ON j.BoutID = b.ID
	JOIN JamSubset js ON js.JamID = j.ID
	WHERE
		t.TeamTypeID = 1
	GROUP BY l.Name, tjp.Penalties, tbp.Penalties, tsp.StarPasses, tl.Leads
)
SELECT TeamName, Bouts, Jams, JammerPenalties, BlockerPenalties, StarPasses, 
CAST(JammerPenalties AS float)/Bouts AS JPRate,
CAST(BlockerPenalties AS float)/Bouts AS BPRate,
CAST(StarPasses AS float)/Bouts AS SPRate,
CAST(Leads AS float)/Jams AS LeadRate
FROM CollectedStats
--WHERE Bouts > 3
