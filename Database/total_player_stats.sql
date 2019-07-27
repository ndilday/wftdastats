WITH 
JamSubset(JamID) AS
(
SELECT j.ID 
FROM Jam j
JOIN Bout b ON b.ID = j.BoutID
JOIN Team t1 ON b.HomeTeamID = t1.ID
JOIN Team t2 ON b.AwayTeamID = t2.ID
WHERE b.PlayDate >= '2019-01-01' AND t1.TeamTypeID = 1 AND t2.TeamTypeID = 1
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
TPOppPoints(TeamPlayerID, Points) AS
(
	SELECT tp.ID, SUM(oj.Points)
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	JOIN Jam_Player ojp ON ojp.JamID = js.JamID
	JOIN Team_Player otp ON otp.ID = ojp.Team_PlayerID
	JOIN Jammer oj ON oj.Jam_PlayerID = ojp.ID
	WHERE
		jp.IsJammer = 1 AND
		otp.TeamID != tp.TeamID
	GROUP BY tp.ID
),
TPJService(TeamPlayerID, Bouts, Jams) AS
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
TPBService(TeamPlayerID, Bouts, Jams) AS
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
),
BData(TeamPlayerID, BGames, BJams, BPen, BPenRate, PJams, SPRec) AS
(
	SELECT 
		tps.TeamPlayerID,
		tps.Bouts AS Games, 
		tps.Jams AS Jams,
		ISNULL(tpbp.Penalties, 0) AS Penalties, 
		CAST(ISNULL(tpbp.Penalties, 0) AS float)/tps.Jams AS PenRate,
		ISNULL(tpps.Jams, 0) AS JamsAsPivot,
		ISNULL(tprs.StarPasses, 0) AS StarPasses
	FROM TPBService tps
	LEFT JOIN TPBlockerPenalties tpbp ON tpbp.TeamPlayerID = tps.TeamPlayerID
	LEFT JOIN TPPivotService tpps ON tpps.TeamPlayerID = tps.TeamPlayerID
	LEFT JOIN TPReceivedStar tprs ON tprs.TeamPlayerID = tps.TeamPlayerID
),
JData(TeamPlayerID, JGames, JJams, Points, PPJ, Delta, DPJ, LeadRate, JPen, JPenRate, StarPasses) AS
(
	SELECT 
		tps.TeamPlayerID,
		tps.Bouts, 
		tps.Jams, 
		tpp.Points,
		tpp.Points/CAST(tps.Jams AS float),
		tpp.Points - tpop.Points,
		(tpp.Points - tpop.Points)/CAST(tps.Jams AS float),
		ISNULL(tpl.Leads, 0)/CAST(tps.Jams AS float), 
		ISNULL(tpjp.Penalties, 0), 
		ISNULL(tpjp.Penalties, 0)/CAST(tps.Jams AS float),
		ISNULL(tpsp.StarPasses, 0)
	FROM TPJService tps
	JOIN TPPoints tpp ON tpp.TeamPlayerID = tps.TeamPlayerID
	JOIN TPOppPoints tpop ON tpop.TeamPlayerID = tps.TeamPlayerID
	LEFT JOIN TPJammerPenalties tpjp ON tpjp.TeamPlayerID = tps.TeamPlayerID
	LEFT JOIN TPLeads tpl ON tpl.TeamPlayerID = tps.TeamPlayerID
	LEFT JOIN TPStarPasses tpsp ON tpsp.TeamPlayerID = tps.TeamPlayerID
)
SELECT 
	--l.Name AS TeamName, 
	p.Number, 
	p.Name, 
	bd.BGames,
	bd.BJams,
	bd.BPen,
	FORMAT(bd.BPenRate, 'P0') AS BPenRate,
	bd.PJams,
	bd.SPRec,
	jd.JGames,
	jd.JJams,
	jd.Points,
	FORMAT(jd.PPJ, 'F2') AS PPJ,
	jd.Delta,
	FORMAT(jd.DPJ, 'F2') AS DPJ,
	FORMAT(jd.LeadRate, 'P0') AS LeadRate,
	jd.JPen,
	FORMAT(jd.JPenRate, 'P0') AS JPenRate,
	jd.StarPasses
FROM Team_Player tp
JOIN Team t ON t.ID = tp.TeamID
JOIN League l ON l.ID = t.LeagueID
JOIN Player p ON p.ID = tp.PlayerID
LEFT JOIN BData bd ON bd.TeamPlayerID = tp.ID
LEFT JOIN JData jd ON jd.TeamPlayerID = tp.ID
WHERE
	t.TeamTypeID = 1 AND l.Name='E-Ville Roller Derby' AND
	(bd.TeamPlayerID IS NOT NULL OR jd.TeamPlayerID IS NOT NULL)
ORDER BY l.Name, jd.JJams DESC, bd.BJams DESC
FOR JSON PATH