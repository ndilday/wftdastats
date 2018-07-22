WITH 
JamSubset(JamID) AS
(
SELECT j.ID 
FROM Jam j
JOIN Bout b ON b.ID = j.BoutID
WHERE b.PlayDate > '2014-12-01'
),
JamCounts(IsJammer, JamCount) AS
(
SELECT jp.IsJammer, COUNT(jp.ID)
FROM Jam_Player jp
JOIN JamSubset js ON js.JamID = jp.JamID
GROUP BY jp.IsJammer
),
PenAverage(ID, IsJammer, Rate) AS
(
	SELECT ft.ID, jp.IsJammer, COUNT(p.ID)/CAST(jc.JamCount AS float)
	FROM FoulType ft
	JOIN Penalty p ON p.FoulTypeID = ft.ID
	JOIN Jam_Player jp ON jp.ID = p.JamPlayerID
	JOIN JamSubset js ON js.JamID = jp.JamID
	JOIN JamCounts jc ON jc.IsJammer = jp.IsJammer
	GROUP BY ft.ID, jp.IsJammer, jc.JamCount
),
PlayerPenaltyCounts(TeamPlayerID, IsJammer, FoulTypeID, Pens) AS
(
	SELECT jp.Team_PlayerID, jp.IsJammer, p.FoulTypeID, COUNT(p.ID)
	FROM JamSubset js
	JOIN Jam_Player jp ON jp.JamID = js.JamID
	JOIN Penalty p ON p.JamPlayerID = jp.ID
	GROUP BY jp.Team_PlayerID, jp.IsJammer, p.FoulTypeID
),
PlayerJamCounts(TeamPlayerID, IsJammer, Jams) AS
(
	SELECT jp.Team_PlayerID, jp.IsJammer, COUNT(jp.ID)
	FROM JamSubset js
	JOIN Jam_Player jp ON jp.JamID = js.JamID
	GROUP BY jp.Team_PlayerID, jp.IsJammer
)
SELECT p.Name, pjc.IsJammer, ft.Code, pa.Rate * pjc.Jams AS Average, ISNULL(ppc.Pens, 0) AS Actual
FROM PlayerJamCounts pjc
JOIN PenAverage pa ON pa.IsJammer = pjc.IsJammer
JOIN FoulType ft ON ft.ID = pa.ID
JOIN Team_Player tp ON tp.ID = pjc.TeamPlayerID
JOIN Player p ON p.ID = tp.PlayerID
LEFT JOIN PlayerPenaltyCounts ppc ON ppc.TeamPlayerID = pjc.TeamPlayerID AND ppc.FoulTypeID = pa.ID AND ppc.IsJammer = pjc.IsJammer
WHERE tp.TeamID = 172
ORDER BY IsJammer DESC, p.Name DESC, ft.ID