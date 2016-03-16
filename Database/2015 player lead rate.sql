
WITH Jam_Team_Lead(TeamID, JamID, Lead) AS
(
	SELECT tp.TeamID, jp.JamID, Jammer.Lead
	FROM Team_Player tp
	JOIN Jam_Player jp ON jp.Team_PlayerID = tp.ID
	JOIN Jammer ON jammer.Jam_PlayerID = jp.ID
	WHERE
		jp.IsJammer = 1
),
team_player_lead_rate(TeamPlayerID, IsJammer, JamCount, LeadCount) AS
(
SELECT tp.ID AS TeamPlayerID, jp.IsJammer, COUNT(jp.ID) AS JamCount, SUM(CAST(jtl.Lead AS int)) AS LeadCount
FROM Team_Player tp
JOIN Jam_Player jp ON jp.Team_PlayerID = tp.ID
JOIN Jam_Team_Lead jtl ON jtl.TeamID = tp.TeamID AND jtl.JamID = jp.JamID
JOIN Jam j ON j.ID = jp.JamID
JOIN Bout b ON b.ID = j.BoutID
JOIN Player p ON p.ID = tp.PlayerID
WHERE 
	jp.ID NOT IN (SELECT JamPlayerID FROM BoxTime WHERE StartedInBox = 1) AND
	b.PlayDate > '2014-01-01'

GROUP BY tp.Id, jp.IsJammer
)

select p.Name, tplr.IsJammer, tplr.JamCount, CAST(tplr.LeadCount AS float)/tplr.JamCount
from team_player_lead_rate tplr
join team_player tp ON tp.ID = tplr.TeamPlayerID
join team t on t.id = tp.TeamID
join player p on p.id = tp.PlayerID
WHERE t.Name = 'Wicked Pissahs'
ORDER BY tplr.IsJammer DESC, tplr.JamCount desc