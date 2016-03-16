EXEC sp_MSForEachTable 'DISABLE TRIGGER ALL ON ?'
GO
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO
EXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON;DELETE FROM ?'
GO
EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
GO
EXEC sp_MSForEachTable 'ENABLE TRIGGER ALL ON ?'
GO

DBCC CHECKIDENT (Bout, RESEED, 0)
DBCC CHECKIDENT (BoxTime, RESEED, 0)
DBCC CHECKIDENT (Penalty, RESEED, 0)
DBCC CHECKIDENT (FoulType, RESEED, 0)
DBCC CHECKIDENT (Jam, RESEED, 0)
DBCC CHECKIDENT (Jam_Player, RESEED, 0)
DBCC CHECKIDENT (Jammer, RESEED, 0)
DBCC CHECKIDENT (Player, RESEED, 0)
DBCC CHECKIDENT (Team, RESEED, 0)
DBCC CHECKIDENT (Team_Player, RESEED, 0)
DBCC CHECKIDENT (TeamType, RESEED, 0)

INSERT INTO FoulType VALUES
('B', 'Back Block'),
('A', 'High Block'),
('L', 'Low Block'),
('E', 'Elbow'),
('F', 'Forearm'),
('H', 'Block with Head'),
('M', 'Multi-player'),
('O', 'Out of Bounds Block'),
('C', 'Direction of Play'),
('P', 'Out of Play'),
('X', 'Cutting'),
('S', 'Skate Out of Bounds'),
('I', 'Illegal Procedure/Failure to Yield'),
('N', 'Insubordination'),
('Z', 'Delay of Game'),
('G', 'Misconduct')

INSERT INTO TeamType VALUES
('Travel (A)'),
('Travel (B)'),
('Travel (C)'),
('Home'),
('Junior')

INSERT INTO tmpVar VALUES (0,0,0)