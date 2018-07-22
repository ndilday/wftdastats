WITH JamSubset(JamID) AS
(
SELECT j.ID 
FROM Jam j
JOIN Bout b ON b.ID = j.BoutID
WHERE b.PlayDate > '2014-12-01'
),
JamTeamLead(JamID, TeamID, IsLead) AS
(
SELECT j.ID, tp.TeamID, jammer.Lead
FROM Jam j
JOIN Jam_Player jp ON jp.JamID = j.ID
JOIN Jammer ON jammer.Jam_PlayerID = jp.ID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
WHERE jp.IsJammer = 1
),
JamPlayerPenalties(JamPlayerID, PenaltyCount) AS
(
SELECT jp.ID, COUNT(p.ID)
FROM Jam_Player jp
JOIN Penalty p ON p.JamPlayerID = jp.ID
GROUP BY jp.ID
),
JamTeamPoints(JamID, TeamID, Points) AS
(
SELECT j.ID, tp.TeamID, SUM(Jammer.Points)
FROM Jam j
JOIN Jam_Player jp ON jp.JamID = j.ID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN Jammer ON Jammer.Jam_PlayerID = jp.ID
GROUP BY j.ID, tp.TeamID
),
JamTeamDelta(JamID, TeamID, Points, Delta) AS
(
SELECT jtp.JamID, jtp.TeamID, jtp.Points, jtp.Points - jtp2.Points
FROM JamTeamPoints jtp
JOIN JamTeamPoints jtp2 ON jtp2.JamID = jtp.JamID AND jtp2.TeamID != jtp.TeamID
),
TeamPlayerData(TeamPlayerID, IsJammer, Jams, Leads, Penalties, TotalDelta) AS
(
SELECT 
	tp.ID AS Team_PlayerID, 
	jp.IsJammer, 
	COUNT(j.JamID) AS Jams, 
	SUM(CAST(jtl.IsLead AS int)) AS Leads, 
	SUM(ISNULL(jpp.PenaltyCount,0)) AS Penalties, 
	SUM(jtd.Delta) AS TotalDelta
FROM JamSubset j
JOIN Jam_Player jp ON jp.JamID = j.JamID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
JOIN JamTeamLead jtl ON jtl.JamID = j.JamID AND jtl.TeamID = tp.TeamID
JOIN JamTeamDelta jtd ON jtd.JamID = j.JamID AND jtd.TeamID = tp.TeamID
LEFT JOIN JamPlayerPenalties jpp ON jpp.JamPlayerID = jp.ID
GROUP BY tp.ID, jp.IsJammer
),
WFTDAAverages(IsJammer, Leads, Penalties) AS
(
	SELECT 
		IsJammer, CAST(SUM(Leads) AS float)/SUM(Jams), CAST(SUM(Penalties) AS float)/SUM(Jams)
	FROM TeamPlayerData tpd
	JOIN Team_Player tp ON tp.ID = tpd.TeamPlayerID
	JOIN Team t ON t.ID = tp.TeamID
	WHERE
		t.TeamTypeID = 1
	GROUP BY IsJammer
),
FinalCalculations(TeamPlayerID, IsJammer, Jams, LeadRate, PenaltyRate, AvgDelta, PenaltyCostVsAvg) AS
(
SELECT 
	tpd.TeamPlayerID, 
	tpd.IsJammer,
	tpd.Jams,
	CAST(tpd.Leads as float) / tpd.Jams,
	CAST(tpd.Penalties as float) / tpd.Jams,
	CAST(tpd.TotalDelta as float) / tpd.Jams,	
	CASE
	WHEN tpd.IsJammer = 0 THEN 
		(tpd.Penalties - (av.Penalties * tpd.Jams)) * 2.5 / tpd.Jams
	ELSE
		(tpd.Penalties - (av.Penalties * tpd.Jams)) * 10 / tpd.Jams
	END AS PenaltyCostVersusAverage
FROM TeamPlayerData tpd
JOIN WFTDAAverages av ON av.IsJammer = tpd.IsJammer
)
--SELECT * FROM WFTDAAverages
SELECT 
	l.name,
	p.Name,
	fc.IsJammer,
	fc.Jams,
	fc.LeadRate AS LeadRate,
	fc.PenaltyRate AS PenaltyRate,
	fc.AvgDelta,
	fc.AvgDelta - fc.PenaltyCostVsAvg AS PenaltyAdjustedDelta
FROM FinalCalculations fc
JOIN Team_Player tp ON tp.ID = fc.TeamPlayerID
JOIN Player p ON p.ID = tp.PlayerID
JOIN Team t ON t.ID = tp.TeamID
JOIN League l ON l.ID = t.LeagueID
WHERE
	t.TeamTypeID = 1
ORDER BY l.Name, IsJammer DESC, Jams DESC