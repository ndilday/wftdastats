-- players with dupe tp entries
SELECT * FROM Player p
JOIN Team_Player tp1 ON tp1.PlayerID = p.ID
JOIN Team_Player tp2 ON tp2.PlayerID = tp1.PlayerID AND tp2.ID > tp1.ID
JOIN Team t1 ON tp1.TeamID = t1.ID
JOIN Team t2 ON tp2.TeamID = t2.ID
WHERE
	t1.TeamTypeID = 1 AND
	t2.TeamTypeID = 1 AND
	t1.LeagueID = t2.LeagueID


-- clean out unused entries
SELECT * FROM Team_Player WHERE ID NOT IN (SELECT Team_PlayerID FROM Jam_Player)
DELETE FROM Team_Player WHERE ID NOT IN (SELECT Team_PlayerID FROM Jam_Player)
SELECT * FROM Player WHERE ID NOT IN (SELECT PlayerID FROM Team_Player)
DELETE FROM Player WHERE ID NOT IN (SELECT PlayerID FROM Team_Player)
UPDATE Jam_Player SET Team_PlayerID = 2218 WHERE Team_PlayerID = 5463
DELETE FROM Team_Player WHERE ID = 5463
DELETE FROM Player WHERE ID = 4223
UPDATE Jam_Player SET Team_PlayerID = 2215 WHERE Team_PlayerID = 5765
DELETE FROM Team_Player WHERE ID = 5765
DELETE FROM Player WHERE ID = 4485

-- move players into hard knocks that were only in Houston All-Stars
INSERT INTO Team_Player
SELECT 198, p.ID From Player p
WHERE ID IN (SELECT PlayerID FROM Team_Player WHERE TeamID = 200) AND ID NOT IN (SELECT PlayerID FROM Team_Player WHERE TeamID = 198)

-- move jp rows to point at the tp we want
UPDATE jp
SET jp.Team_PlayerID = tpg.ID
FROM Player p
JOIN Team_Player tpg ON tpg.PlayerID = p.ID
JOIN Team_Player tpb ON tpb.PlayerID = tpg.PlayerID
JOIN Jam_Player jp ON jp.Team_PlayerID = tpb.ID
WHERE
	tpg.TeamID = 198 AND
	tpb.TeamID = 200

-- UPDATE Bouts to point to the desired team
UPDATE Bout
SET AwayTeamID = 198
WHERE AwayTeamID = 200
UPDATE Bout
SET HomeTeamID = 198
WHERE HomeTeamID = 200

DELETE FROM Team WHERE ID = 200