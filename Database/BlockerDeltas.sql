WITH 
JamSubset(JamID) AS
(
SELECT j.ID 
FROM Jam j
JOIN Bout b ON b.ID = j.BoutID
JOIN Team t1 ON b.HomeTeamID = t1.ID
JOIN Team t2 ON b.AwayTeamID = t2.ID
WHERE b.PlayDate >= '2019-08-01' AND t1.TeamTypeID = 1 AND t2.TeamTypeID = 1 AND (t1.Name = 'Boston Massacre' OR t2.Name = 'Boston Massacre')
),
JPLeads (JamPlayerId, IsLead) AS
(
	SELECT j.Jam_PlayerID, j.Lead
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		jp.IsJammer = 1
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
JPPoints(JamPlayerId, Points) AS
(
	SELECT j.Jam_PlayerID, SUM(j.Points)
	FROM Jammer j
	JOIN Jam_Player jp ON jp.ID = j.Jam_PlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	WHERE
		jp.IsJammer = 1
	GROUP BY j.Jam_PlayerID
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
JPOppPoints(JamPlayerID, Points) AS
(
	SELECT j.Jam_PlayerID, SUM(oj.Points)
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
	GROUP BY j.Jam_PlayerID
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
JData(TeamPlayerID, DPJ, LPJ) AS
(
	SELECT tpjs.TeamPlayerID, 
	(tpp.Points - tpop.Points)/CAST(tpjs.Jams AS float),
	ISNULL(tpl.Leads, 0)/CAST(tpjs.Jams AS float)
	FROM TPJService tpjs
	LEFT JOIN TPLeads tpl ON tpl.TeamPlayerID = tpjs.TeamPlayerID
	JOIN TPPoints tpp ON tpp.TeamPlayerID = tpjs.TeamPlayerID
	JOIN TPOppPoints tpop ON tpop.TeamPlayerID = tpjs.TeamPlayerID
),
JamPartials(JamPlayerId, ExpectedLead, ExpectedDelta, Lead, Delta) AS
(
	SELECT jp.ID, jd.LPJ, jd.DPJ, jpl.IsLead, jpp.Points - jpop.Points
	FROM JamSubset js
	JOIN Jam_Player jp ON jp.JamID = js.JamID
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
	JOIN JData jd ON jd.TeamPlayerID = tp.ID
	JOIN JPPoints jpp ON jpp.JamPlayerId = jp.ID
	JOIN JPOppPoints jpop ON jpop.JamPlayerID = jp.ID
	JOIN JPLeads jpl ON jpl.JamPlayerId = jp.ID
),
BlockerPartials(JamPlayerId, DeltaDelta, LeadDelta) AS
(
	SELECT jp.ID, SUM(CAST(parts.Delta AS float) - parts.ExpectedDelta), SUM(CAST(parts.Lead AS float) - parts.ExpectedLead)
	FROM JamSubset js
	JOIN Jam_Player jjp ON jjp.JamID = js.JamId
	JOIN Team_Player jtp ON jtp.ID = jjp.Team_PlayerID
	JOIN Team_Player tp ON tp.TeamID = jtp.TeamID
	JOIN Jam_Player jp ON jp.JamID = jjp.JamID AND jp.Team_PlayerID = tp.ID
	JOIN JamPartials parts ON parts.JamPlayerId = jjp.ID
	WHERE jp.IsJammer = 0
	GROUP BY jp.ID
),
BlockerTotals(TeamPlayerId, DeltaDelta, LeadDelta) AS
(
	SELECT jp.Team_PlayerID, SUM(bp.DeltaDelta)/COUNT(bp.DeltaDelta), SUM(bp.LeadDelta)/COUNT(bp.DeltaDelta)
	FROM BlockerPartials bp
	JOIN Jam_Player jp ON jp.ID = bp.JamPlayerId
	GROUP BY jp.Team_PlayerID
)
SELECT p.Number, p.Name, bt.*
FROM BlockerTotals bt
JOIN Team_Player tp ON tp.ID = bt.TeamPlayerId
JOIN Player p ON p.ID = tp.PlayerID
JOIN Team t ON t.ID = tp.TeamID
WHERE t.Name = 'Boston Massacre'