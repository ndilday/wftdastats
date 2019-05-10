UPDATE League SET Name = 'Naptown Roller Derby' WHERE ID = 70
UPDATE MetaLeague SET Name = 'Naptown Roller Derby' WHERE Name = 'Naptown Roller Girls'

UPDATE League SET Name = 'Detroit Roller Derby' WHERE ID = 68
UPDATE League SET Name = 'Detroit Roller Derby' WHERE ID = 36

INSERT INTO Skater VALUES ('Green Lantern', '1940')
INSERT INTO Player VALUES (4987, 'Green Lantern', '1940')
INSERT INTO Skater_Player VALUES (5998, 2403)
INSERT INTO Team_Player VALUES (275, 2403)
UPDATE Player SET Number = '9' WHERE ID = 5093

UPDATE Player SET Number = '89' WHERE ID = 5924
INSERT INTO League VALUES (195, 'Middlesbrough Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Middlesbrough Roller Derby', '2017-01-01')
GO
INSERT INTO MetaLeague_League VALUES (139, 195)
INSERT INTO Team VALUES (NULL, 'Middlesbrough Milk Rollers', 195, 1)
INSERT INTO MetaTeam VALUES ('Middlesbrough Milk Rollers', 139, 1)
GO
INSERT INTO MetaTeam_Team VALUES (160, 346)

INSERT INTO League VALUES (247, 'Paris Rollergirls', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Paris Rollergirls', '2017-01-01')
GO
INSERT INTO MetaLeague_League VALUES(140, 247)
INSERT INTO Team VALUES (NULL, 'Paris Rollergirls All Stars', 247, 1)
INSERT INTO MetaTeam VALUES ('Paris Rollergirls All Stars', 140, 1)
GO
INSERT INTO MetaTeam_Team VALUES (161, 347)

INSERT INTO League VALUES (248, 'Leeds Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Leeds Roller Derby', '2017-01-01')
GO
INSERT INTO MetaLeague_League VALUES(141, 248)
INSERT INTO Team VALUES (NULL, 'A Team', 248, 1)
INSERT INTO MetaTeam VALUES ('A Team', 141, 1)
GO
INSERT INTO MetaTeam_Team VALUES (162, 348)
-----


----- 3-28-18 -----
INSERT INTO Team VALUES (NULL, 'Subzero Sirens', 25, 2)
INSERT INTO MetaTeam VALUES('Subzero Sirens', 87, 2)
GO
INSERT INTO MetaTeam_Team VALUES(170, 356)
UPDATE Player SET Number = '77' WHERE ID = 5965
INSERT INTO Team VALUES(NULL, 'Arbor Bruising Company', 199, 2)
INSERT INTO MetaTeam VALUES('Arbor Bruising Company', 3,2)
GO
INSERT INTO MetaTeam_Team VALUES(171, 357)
-----


----- 7-17-18 -----
DBCC CHECKIDENT ( Team, RESEED, 357 )
DBCC CHECKIDENT (MetaTeam, RESEED, 171)
INSERT INTO FoulType VALUES ('D', 'Direction'), ('U', 'Unknown')
GO
INSERT INTO League VALUES (256, 'Orangeville Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Orangeville Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(149, 256)
GO
INSERT INTO Team VALUES (NULL, 'Misfit Militia', 256, 1)
INSERT INTO MetaTeam VALUES('Misfit Militia', 149, 1)
GO
INSERT INTO MetaTeam_Team VALUES(172, 358)
GO
UPDATE League SET Name = 'Sacramento Roller Derby' WHERE ID = 50
INSERT INTO League VALUES (257, 'Gem City Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Gem City Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(150, 257)
GO
INSERT INTO Team VALUES (NULL, 'Purple Reign', 257, 1)
INSERT INTO MetaTeam VALUES('Purple Reign', 150, 1)
GO
INSERT INTO MetaTeam_Team VALUES(173, 359)
GO
INSERT INTO League VALUES (258, 'West Virginia Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('West Virginia Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(151, 258)
GO
INSERT INTO Team VALUES (NULL, 'All-Stars', 258, 1)
INSERT INTO MetaTeam VALUES('All-Stars', 151, 1)
GO
INSERT INTO MetaTeam_Team VALUES(174, 360)
GO
INSERT INTO League VALUES (259, 'Birmingham Blitz Dames', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Birmingham Blitz Dames', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(152, 259)
GO
INSERT INTO Team VALUES (NULL, 'All-Stars', 259, 1)
INSERT INTO MetaTeam VALUES('All-Stars', 152, 1)
GO
INSERT INTO MetaTeam_Team VALUES(175, 361)
GO
INSERT INTO League VALUES (260, 'Downriver Roller Dolls', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Downriver Roller Dolls', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(153, 260)
GO
INSERT INTO Team VALUES (NULL, 'Doll Stars', 260, 1)
INSERT INTO MetaTeam VALUES('Doll Stars', 153, 1)
GO
INSERT INTO MetaTeam_Team VALUES(176, 362)
GO
INSERT INTO League VALUES (261, 'VTown Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('VTown Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(154, 261)
GO
INSERT INTO Team VALUES (NULL, 'Dames', 261, 1)
INSERT INTO MetaTeam VALUES('Dames', 154, 1)
GO
INSERT INTO MetaTeam_Team VALUES(177, 363)
GO
INSERT INTO League VALUES (262, 'Winnipeg Roller Derby League', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Winnipeg Roller Derby League', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(155, 262)
GO
INSERT INTO Team VALUES (NULL, 'All-Stars', 262, 1)
INSERT INTO MetaTeam VALUES('All-Stars', 155, 1)
GO
INSERT INTO MetaTeam_Team VALUES(178, 364)
GO
INSERT INTO League VALUES (263, 'Sailor City Rollers', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Sailor City Rollers', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(156, 263)
GO
INSERT INTO Team VALUES (NULL, 'All-Stars', 263, 1)
INSERT INTO MetaTeam VALUES('All-Stars', 156, 1)
GO
INSERT INTO MetaTeam_Team VALUES(179, 365)
GO
INSERT INTO League VALUES (264, 'Gothenburg Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Gothenburg Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(157, 264)
GO
INSERT INTO Team VALUES (NULL, 'A Team', 264, 1)
INSERT INTO MetaTeam VALUES('A Team', 157, 1)
GO
INSERT INTO MetaTeam_Team VALUES(180, 366)
GO
INSERT INTO League VALUES (265, 'Canberra Roller Derby League', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Canberra Roller Derby League', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(158, 265)
GO
INSERT INTO Team VALUES (NULL, 'Vice City Rollers', 265, 1)
INSERT INTO MetaTeam VALUES('Vice City Rollers', 158, 1)
GO
INSERT INTO MetaTeam_Team VALUES(181, 367)
GO
INSERT INTO League VALUES (266, 'FoCO Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('FoCo Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(159, 266)
GO
INSERT INTO Team VALUES (NULL, 'Micro Bruisers', 266, 1)
INSERT INTO MetaTeam VALUES('Micro Bruisers', 159, 1)
GO
INSERT INTO MetaTeam_Team VALUES(182, 368)
GO
INSERT INTO League VALUES (267, 'Perth Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Perth Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(160, 267)
GO
INSERT INTO Team VALUES (NULL, 'West Coast Devils', 267, 1)
INSERT INTO MetaTeam VALUES('West Coast Devils', 160, 1)
GO
INSERT INTO MetaTeam_Team VALUES(183, 369)
GO
INSERT INTO League VALUES (268, 'Western Australia Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Western Australia Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(161, 268)
GO
INSERT INTO Team VALUES (NULL, 'Wards of the Skate', 268, 1)
INSERT INTO MetaTeam VALUES('Wards of the Skate', 161, 1)
GO
INSERT INTO MetaTeam_Team VALUES(184, 370)
GO
INSERT INTO League VALUES (269, 'Appalachian Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Appalachian Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(162, 269)
GO
INSERT INTO Team VALUES (NULL, 'All-Stars', 269, 1)
INSERT INTO MetaTeam VALUES('All-Stars', 162, 1)
GO
INSERT INTO MetaTeam_Team VALUES(185, 371)
GO
INSERT INTO League VALUES (270, '10th Mountain Roller Dolls', '2017-01-01')
INSERT INTO MetaLeague VALUES ('10th Mountain Roller Dolls', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(163, 270)
GO
INSERT INTO Team VALUES (NULL, 'Mountaineers', 270, 1)
INSERT INTO MetaTeam VALUES('Mountaineers', 163, 1)
GO
INSERT INTO MetaTeam_Team VALUES(186, 372)
GO
INSERT INTO League VALUES (271, 'Ottawa Valley Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Ottawa Valley Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(164, 271)
GO
INSERT INTO Team VALUES (NULL, 'All Stars', 271, 1)
INSERT INTO MetaTeam VALUES('All Stars', 164, 1)
GO
INSERT INTO MetaTeam_Team VALUES(187, 373)
GO
UPDATE League SET name = 'Jet City Roller Derby' WHERE ID = 44
UPDATE League SET Name = 'North Star Roller Derby' WHERE ID = 86
UPDATE Bout SET PlayDate = '2017-06-15' WHERE ID = 1146
INSERT INTO League VALUES (272, 'Adelaide Roller Derby', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Adelaide Roller Derby', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(165, 272)
GO
INSERT INTO Team VALUES (NULL, 'Adelaidies', 272, 1)
INSERT INTO MetaTeam VALUES('Adelaidies', 165, 1)
GO
INSERT INTO MetaTeam_Team VALUES(188, 374)
GO
-----


--09-19-2018
-----
INSERT INTO League VALUES (273, 'Lille Roller Girls', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Lille Roller Girls', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(166, 273)
GO
INSERT INTO Team VALUES (NULL, 'Bad Bunnies', 273, 1)
INSERT INTO MetaTeam VALUES('Bad Bunnies', 166, 1)
GO
INSERT INTO MetaTeam_Team VALUES(189, 375)
GO

INSERT INTO League VALUES (274, 'Nottingham Hellfire Harlots', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Nottingham Hellfire Harlots', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(167, 274)
GO
INSERT INTO Team VALUES (NULL, 'Harlots', 274, 1)
INSERT INTO MetaTeam VALUES('Harlots', 167, 1)
GO
INSERT INTO MetaTeam_Team VALUES(190, 376)
GO
-----


--03-16-2019
-----
DROP TABLE SituationalScore
CREATE TABLE [dbo].[SituationalScore](
	[Year] [int] NOT NULL,
	[JammerBoxComparison] [float] NOT NULL,
	[BlockerBoxComparison] [float] NOT NULL,
	[PointDelta] [int] NOT NULL,
	[Percentile] [float] NULL,
 CONSTRAINT [PK_SituationalScore] PRIMARY KEY CLUSTERED 
(
	[Year] ASC,
	[JammerBoxComparison] ASC,
	[BlockerBoxComparison] ASC,
	[PointDelta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]
GO
UPDATE Player SET Number='1845' WHERE ID = 308
-----

--03-23-2019
-----
INSERT INTO League VALUES (275, 'Kalamazoo Derby Darlins', '2011-01-01')
INSERT INTO MetaLeague VALUES ('Kalamazoo Derby Darlins', '2011-01-01')
INSERT INTO MetaLeague_League VALUES(168, 275)
GO
INSERT INTO Team VALUES (NULL, 'All-Stars', 275, 1)
INSERT INTO MetaTeam VALUES('All-Stars', 168, 1)
GO
INSERT INTO MetaTeam_Team VALUES(191, 377)
GO

INSERT INTO League VALUES (276, 'Dark River Derby Coalition', '2015-01-01')
INSERT INTO MetaLeague VALUES ('Dark River Derby Coalition', '2015-01-01')
INSERT INTO MetaLeague_League VALUES(169, 276)
GO
INSERT INTO Team VALUES (NULL, 'All-Stars', 276, 1)
INSERT INTO MetaTeam VALUES('All-Stars', 169, 1)
GO
INSERT INTO MetaTeam_Team VALUES(192, 378)
GO

INSERT INTO League VALUES (277, 'Muddy River Rollers', '2011-01-01')
INSERT INTO MetaLeague VALUES ('Muddy River Rollers', '2011-01-01')
INSERT INTO MetaLeague_League VALUES(170, 277)
GO
INSERT INTO Team VALUES (NULL, 'Lumbersmacks', 277, 1)
INSERT INTO MetaTeam VALUES('Lumbersmacks', 170, 1)
GO
INSERT INTO MetaTeam_Team VALUES(193, 379)
GO

UPDATE Player
SET Number = LEFT(Number, LEN(Number) - 1)
WHERE Number LIKE '%*'
-----

--04-01-2019
-----
INSERT INTO League VALUES (278, 'Humboldt Roller Derby', '2012-01-01')
INSERT INTO MetaLeague VALUES ('Humboldt Roller Derby', '2012-01-01')
INSERT INTO MetaLeague_League VALUES(171, 278)
GO
INSERT INTO Team VALUES (NULL, 'Redwood Rollers', 278, 1)
INSERT INTO MetaTeam VALUES('Redwood Rollers', 171, 1)
GO
INSERT INTO MetaTeam_Team VALUES(194, 380)
GO

INSERT INTO League VALUES (279, 'Lava City Roller Dolls', '2009-08-01')
INSERT INTO MetaLeague VALUES ('Lava City Roller Dolls', '2009-08-01')
INSERT INTO MetaLeague_League VALUES(172, 279)
GO
INSERT INTO Team VALUES (NULL, 'Smokin'' Ashes', 279, 1)
INSERT INTO MetaTeam VALUES('Smokin'' Ashes', 172, 1)
GO
INSERT INTO MetaTeam_Team VALUES(195, 381)
GO

INSERT INTO League VALUES (280, 'Greater Vancouver Roller Derby', '2016-01-01')
INSERT INTO MetaLeague VALUES ('Greater Vancouver Roller Derby', '2016-01-01')
INSERT INTO MetaLeague_League VALUES(173, 280)
GO
INSERT INTO Team VALUES (NULL, 'Anarchy Angels', 280, 1)
INSERT INTO MetaTeam VALUES('Anarchy Angels', 173, 1)
GO
INSERT INTO MetaTeam_Team VALUES(196, 382)
GO

INSERT INTO League VALUES (281, 'Sitka Sound Slayers', '2017-01-01')
INSERT INTO MetaLeague VALUES ('Sitka Sound Slayers', '2017-01-01')
INSERT INTO MetaLeague_League VALUES(174, 281)
GO
INSERT INTO Team VALUES (NULL, 'All-Stars', 281, 1)
INSERT INTO MetaTeam VALUES('All-Stars', 174, 1)
GO
INSERT INTO MetaTeam_Team VALUES(197, 383)
GO

INSERT INTO League VALUES (282, 'Wilkes-Barre Scranton Roller Derby', '2015-01-01')
INSERT INTO MetaLeague VALUES ('Wilkes-Barre Scranton Roller Derby', '2015-01-01')
INSERT INTO MetaLeague_League VALUES(175, 282)
GO
INSERT INTO Team VALUES (NULL, 'Roller Radicals', 282, 1)
INSERT INTO MetaTeam VALUES('Roller Radicals', 175, 1)
GO
INSERT INTO MetaTeam_Team VALUES(198, 384)
GO

INSERT INTO League VALUES (283, 'Sonoma County Roller Derby', '2011-01-01')
INSERT INTO MetaLeague VALUES ('Sonoma County Roller Derby', '2011-01-01')
INSERT INTO MetaLeague_League VALUES(176, 283)
GO
INSERT INTO Team VALUES (NULL, 'Hella Organic Rollers', 283, 1)
INSERT INTO MetaTeam VALUES('Hella Organic Rollers', 176, 1)
GO
INSERT INTO MetaTeam_Team VALUES(199, 385)
GO

INSERT INTO League VALUES (284, 'Eves of Destruction', '2018-01-01')
INSERT INTO MetaLeague VALUES ('Eves of Destruction', '2018-01-01')
INSERT INTO MetaLeague_League VALUES(177, 284)
GO
INSERT INTO Team VALUES (NULL, 'The A Team', 284, 1)
INSERT INTO MetaTeam VALUES('The A Team', 177, 1)
GO
INSERT INTO MetaTeam_Team VALUES(200, 386)
GO

INSERT INTO League VALUES (285, 'Gorge Roller Derby', '2018-01-01')
INSERT INTO MetaLeague VALUES ('Gorge Roller Derby', '2018-01-01')
INSERT INTO MetaLeague_League VALUES(178, 285)
GO
INSERT INTO Team VALUES (NULL, 'All Stars', 285, 1)
INSERT INTO MetaTeam VALUES('All Stars', 178, 1)
GO
INSERT INTO MetaTeam_Team VALUES(201, 387)
GO

UPDATE League SET Name = 'Emerald City Roller Derby' WHERE ID = 173
-----

--04-20-2019
-----
UPDATE League SET Name = 'Sun State Roller Derby' WHERE ID = 160
-----


--2019-05-07
-----
UPDATE League SET Name = 'Dub City Roller Derby' WHERE Id = 179
UPDATE League SET Name = 'Royal City Roller Derby' WHERE Id = 250
UPDATE League SET Name = 'Atlanta Roller Derby' WHERE Id = 58

INSERT INTO League VALUES (286, 'Antwerp Roller Derby', '2014-10-01')
INSERT INTO MetaLeague VALUES ('Antwerp Roller Derby', '2014-10-01')
INSERT INTO MetaLeague_League VALUES(179, 286)
GO
INSERT INTO Team VALUES (NULL, 'All Stars', 286, 1)
INSERT INTO MetaTeam VALUES('All Stars', 179, 1)
GO
INSERT INTO MetaTeam_Team VALUES(202, 388)
GO