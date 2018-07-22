WITH 
JamPoints(JamID, TeamID, TeamPoints) AS
(SELECT j.ID, tp.TeamID, SUM(Jammer.Points)
FROM Jam j
JOIN Jam_Player jp ON jp.JamID = j.ID
JOIN Jammer ON Jammer.Jam_PlayerID = jp.ID
JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
GROUP BY j.ID, tp.TeamID),
JamRatios(JamID, HighScore, LowScore) AS
(
SELECT
	jp1.JamID,
	CASE WHEN jp1.TeamPoints >= jp2.TeamPoints THEN jp1.TeamPoints ELSE jp2.TeamPoints END,
	CASE WHEN jp1.TeamPoints <  jp2.TeamPoints THEN jp1.TeamPoints ELSE jp2.TeamPoints END
FROM JamPoints jp1
JOIN JamPoints jp2 ON jp2.JamID = jp1.JamID AND jp2.TeamID != jp1.TeamID
),
RatioCounts(HighScore, LowScore, Frequency) AS
(
SELECT HighScore, LowScore, COUNT(JamID)
FROM JamRatios
GROUP BY HighScore, LowScore
),
TotalRows(Total) AS (SELECT SUM(Frequency) FROM RatioCounts)
SELECT
	HighScore, LowScore, FORMAT(CAST(Frequency AS FLOAT)/CAST(Total AS FLOAT), 'P')
FROM RatioCounts, TotalRows
ORDER BY HighScore, LowScore

