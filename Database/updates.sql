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
UPDATE League SET Name = 'Rat City Roller Derby' WHERE Id = 46

INSERT INTO League VALUES (286, 'Antwerp Roller Derby', '2014-10-01')
INSERT INTO MetaLeague VALUES ('Antwerp Roller Derby', '2014-10-01')
INSERT INTO MetaLeague_League VALUES(179, 286)
GO
INSERT INTO Team VALUES (NULL, 'All Stars', 286, 1)
INSERT INTO MetaTeam VALUES('All Stars', 179, 1)
GO
INSERT INTO MetaTeam_Team VALUES(202, 388)
GO

INSERT INTO League VALUES (287, 'Lomme Roller Girls', '2016-04-01')
INSERT INTO MetaLeague VALUES ('Lomme Roller Girls', '2016-04-01')
INSERT INTO MetaLeague_League VALUES(180, 287)
GO
INSERT INTO Team VALUES (NULL, 'Bad Bunnies', 287, 1)
INSERT INTO MetaTeam VALUES('Bad Bunnies', 180, 1)
GO
INSERT INTO MetaTeam_Team VALUES(203, 389)
GO

INSERT INTO League VALUES (288, 'Northside Rollers', '2014-12-01')
INSERT INTO MetaLeague VALUES ('Northside Rollers', '2014-12-01')
INSERT INTO MetaLeague_League VALUES(181, 288)
GO
INSERT INTO Team VALUES (NULL, 'Death Stars', 288, 1)
INSERT INTO MetaTeam VALUES('Death Stars', 181, 1)
GO
INSERT INTO MetaTeam_Team VALUES(204, 390)
GO

--2019-08-10
-- Blue Ridge
UPDATE Player SET Number = '00' WHERE Id = 416
UPDATE Player SET Name = 'Drunken Sayler' WHERE Id = 6232
UPDATE Player SET Name = 'Rider' WHERE ID = 406
UPDATE Player SET Name = 'Smash P''Tater' WHERE ID = 405
UPDATE Player SET Name = 'Aurora Thunder' WHERE ID = 419
UPDATE Player SET Name = 'R.P.Em.' WHERE ID = 4480
-- Boston
UPDATE Player SET Number = '2' WHERE Id = 317
UPDATE Player SET Name = 'Caitlin Monaghan' WHERE Id = 298
-- Detroit
UPDATE Player SET Name = 'Finn-Purcella' WHERE Id = 1158
UPDATE Player SET Name = 'Genei' WHERE Id = 1168
UPDATE Player SET Name = 'Price' WHERE Id = 5940
UPDATE Player SET Name = 'Ryder' WHERE Id = 5761
UPDATE Player SET Name = 'Oi!' WHERE Id = 5049
UPDATE Player SET Name = 'Storm CrasHer' WHERE Id = 1181
-- Gem City
UPDATE Player SET Name = 'BULLISTIC' WHERE Id = 7678
UPDATE Player SET Name = 'Emily Suter' WHERE Id = 7681
UPDATE Player SET Name = 'Florida Man' WHERE Id = 7682
-- Madison
UPDATE Jam_Player SET Team_PlayerID = 2695 WHERE Team_PlayerID = 9921
UPDATE Jam_Player_Effectiveness SET PlayerID = 2111 WHERE PlayerId = 8246
UPDATE Player SET Number = '9' WHERE Id = 2111
DELETE FROM Team_Player WHERE ID = 9921
DELETE FROM Player WHERE ID = 8246
UPDATE Player SET Name = 'One Hit Wanda' WHERE Id = 5999
UPDATE Jam_Player SET Team_PlayerID = 2700 WHERE Team_PlayerID = 10518
UPDATE Jam_Player_Effectiveness SET PlayerId = 2116 WHERE PlayerId = 8793
UPDATE Player SET Number = '52' WHERE Id = 2116
DELETE FROM Team_Player WHERE ID = 10518
DELETE FROM Player WHERE ID = 8793
-- Ohio
UPDATE Jam_Player SET Team_PlayerID = 3403 WHERE Team_PlayerID = 10828
UPDATE Jam_Player_Effectiveness SET PlayerId = 2645 WHERE PlayerId = 9059
UPDATE Player SET Number = '18' WHERE Id = 2645
DELETE FROM Team_Player WHERE ID = 10828
DELETE FROM Player WHERE ID = 9059
UPDATE Jam_Player SET Team_PlayerID = 7297 WHERE Team_PlayerID = 9941
UPDATE Jam_Player_Effectiveness SET PlayerId = 5775 WHERE PlayerId = 7297
UPDATE Player SET Name = 'BrussKnuckles' WHERE Id = 5775
DELETE FROM Team_Player WHERE ID = 9941
DELETE FROM Player WHERE ID = 7297
UPDATE Player SET Name = 'Jane, Literally' WHERE Id = 2649
UPDATE Player SET Name = 'Smash Panda' WHERE Id = 8264
UPDATE Player SET Name = 'Betty (for this jelly)' WHERE Id = 5292
UPDATE Player SET Name = 'Chainsaw' WHERE Id = 2628
-- Naptown
UPDATE Player SET Name = 'Willsmith' WHERE Id = 2473
UPDATE Player SET Number = '5' WHERE Id = 2483
UPDATE Player SET Name = 'Britt Mika' WHERE Id = 5815
UPDATE Player SET Name = 'Iggy' WHERE Id = 6517
UPDATE Player SET Name = 'Langer' WHERE Id = 5130
UPDATE Player SET Name = 'Emily Udell' WHERE Id = 6181
-- Sailor City
UPDATE Player SET Name = 'Tamara Tancredi' WHERE Id = 8846
UPDATE Player SET Name = 'Jules' WHERE Id = 8847
UPDATE Player SET Name = 'Romo' WHERE Id = 8848
UPDATE Player SET Name = 'Amanita' WHERE Id = 8256
UPDATE Player SET Name = 'Finishit' WHERE Id = 8257
UPDATE Player SET Name = 'Roja' WHERE Id = 8850
UPDATE Player SET Name = 'Carrumán' WHERE Id = 8851
--Tampa
UPDATE Player SET Name = 'Rojo Grande' WHERE Id = 5730
UPDATE Player SET Name = 'Wasylkiw' WHERE Id = 3608
UPDATE Player SET Name = 'Rude' WHERE Id = 8955
-- Toronto
UPDATE Player SET Name = 'Red Zeppelin' WHERE Id = 7920
UPDATE Player SET Name = 'Jamie''s Got a Gun' WHERE Id = 7919
UPDATE Jam_Player SET Team_PlayerID = 6574 WHERE Team_PlayerID = 10200
UPDATE Jam_Player_Effectiveness SET PlayerId = 3757 WHERE PlayerId = 8502
UPDATE Player SET Number = '01' WHERE Id = 3757
DELETE FROM Team_Player WHERE ID = 10200
DELETE FROM Player WHERE ID = 8502
UPDATE Player SET Name = 'Annguard' WHERE Id = 7923
UPDATE Player SET Name = 'Scrappy' WHERE Id = 7921
UPDATE Player SET Name = 'THAT Meg Fenway' WHERE Id = 5107
-- Tri-City
UPDATE Player SET Name = 'Anna Maul' WHERE Id = 4368
UPDATE Player SET Number = '20' WHERE Id = 5018
UPDATE Player SET Name = 'Christie Henderson' WHERE Id = 6255
UPDATE Player SET Name = 'Kristy Skelton' WHERE Id = 3759
UPDATE Player SET Name = 'Superstack' WHERE Id = 3747
UPDATE Player SET Number = '2042' WHERE Id = 3756

--2019-08-19
INSERT INTO League VALUES (289, 'Metropolitan Roller Derby', '2011-12-01')
INSERT INTO MetaLeague VALUES ('Metropolitan Roller Derby', '2011-12-01')
INSERT INTO MetaLeague_League VALUES(182, 289)
GO
INSERT INTO Team VALUES (NULL, 'All Stars', 289, 1)
INSERT INTO MetaTeam VALUES('All Stars', 182, 1)
GO
INSERT INTO MetaTeam_Team VALUES(205, 391)
GO

-- 2019-09-01
-- Angel City
UPDATE Jam_Player SET Team_PlayerID = 10059 WHERE Team_PlayerID = 10107
UPDATE Jam_Player_Effectiveness SET PlayerId = 8378 WHERE PlayerId = 8419
DELETE FROM Team_Player WHERE ID = 10107
DELETE FROM Player WHERE ID = 8419
UPDATE Jam_Player SET Team_PlayerID = 6538 WHERE Team_PlayerID = 9832
UPDATE Jam_Player_Effectiveness SET PlayerId = 5088 WHERE PlayerId = 8161
DELETE FROM Team_Player WHERE ID = 9832
DELETE FROM Player WHERE ID = 8161
UPDATE Player SET Name = 'BonnieNicole Thunderstormz' WHERE ID = 8418
UPDATE Player SET Name = 'Lo Betancourt' WHERE ID = 5088
UPDATE Player SET Name = 'Smash' WHERE ID = 7973
-- Atlanta
UPDATE Player SET Number = '76' WHERE ID = 195
UPDATE Player SET Number = '12' WHERE ID = 193
UPDATE Player SET Name = 'Baller Shot Caller' WHERE ID = 1396
UPDATE Player SET Name = 'Erykah Ba-Doozie' WHERE ID = 4150
-- Bay Area
UPDATE Player SET Name = 'Knockout' WHERE ID = 277
UPDATE Player SET Name = 'Legs//Cité' WHERE ID = 8436
UPDATE Player SET Name = 'Dirtt' WHERE ID = 5073
UPDATE Player SET Name = 'Kate Silver' WHERE ID = 7922
UPDATE Player SET Name = 'Blood, Sweat, and Ears' WHERE ID = 269
-- Bear City
UPDATE Player SET Name = 'Bambickel' WHERE ID = 4530
UPDATE Player SET Number = '666' WHERE ID = 5602
-- Helsinki
UPDATE Player SET Name = 'MIA' WHERE ID = 1712
UPDATE Player SET Name = 'Malou' WHERE ID = 4068
UPDATE Player SET Name = 'Kujala' WHERE ID = 6018
UPDATE Player SET Name = 'Majiu Rinne' WHERE ID = 1718
UPDATE Player SET Name = 'Sara Ekholm' WHERE ID = 1720
UPDATE Player SET Name = 'Ruotalainen' WHERE ID = 5178
UPDATE Player SET Name = 'Varpu Knuuttila' WHERE ID = 1723
-- Lomme
UPDATE Player SET Number = '712' WHERE ID = 8916
UPDATE Player SET Number = '86' WHERE ID = 8918
-- Paris
UPDATE Jam_Player SET Team_PlayerID = 9717 WHERE Team_PlayerID = 10076
UPDATE Jam_Player_Effectiveness SET PlayerId = 8055 WHERE PlayerId = 8395
DELETE FROM Team_Player WHERE ID = 10076
DELETE FROM Player WHERE ID = 8395
-- Rainy
UPDATE Player SET Name = 'Ducky' WHERE ID = 6200
UPDATE Player SET Name = 'Blondie' WHERE ID = 5220
UPDATE Player SET Name = 'Lauren Swaffield' WHERE ID = 5222
UPDATE Player SET Name = 'Rascal' WHERE ID = 5223
UPDATE Player SET Name = 'Elizabeth Yeatman' WHERE ID = 5227
UPDATE Player SET Name = 'Banshee' WHERE ID = 5229
-- Santa Cruz
UPDATE Player SET Number = '3' WHERE ID = 3406
UPDATE Player SET Name = 'E-Wrecks' WHERE ID = 8399
UPDATE Player SET Name = 'Mad 4 Gravy' WHERE ID = 3407
UPDATE Player SET Name = 'CassieBeck' WHERE ID = 5650
UPDATE Player SET Name = 'Killer Vee' WHERE ID = 8018
UPDATE Player SET Name = 'High Roller' WHERE ID = 8807
UPDATE Player SET Name = 'Jurassic Snark' WHERE ID = 8400
-- Texas
UPDATE Player SET Name = 'Kris Rage-o' WHERE ID = 8358
UPDATE Player SET Name = 'Kaptain sassy' WHERE ID = 7977
UPDATE Player SET Name = 'Gravy, Baby!' WHERE ID = 3658
UPDATE Player SET Name = 'Nine Lives' WHERE ID = 5072
UPDATE Jam_Player SET Team_PlayerID = 9635 WHERE Team_PlayerID = 10131
UPDATE Jam_Player_Effectiveness SET PlayerId = 7977 WHERE PlayerId = 8441
DELETE FROM Team_Player WHERE ID = 10131
DELETE FROM Player WHERE ID = 8441
-- Windy
UPDATE Player SET Name = 'Kickin'' McChugget' WHERE ID = 6518
UPDATE Player SET Name = 'Tricky Pixie' WHERE ID = 8345
UPDATE Player SET Name = 'Regina S.P.E.C.T.R.E.' WHERE ID = 3948
UPDATE Player SET Name = 'Killanois' WHERE ID = 3947
UPDATE Player SET Name = 'Maulicious' WHERE ID = 5011
UPDATE Player SET Name = 'Vazquez' WHERE ID = 6270
UPDATE Player SET Name = 'Rita Hateworthy' WHERE ID = 4837

-- 2019-09-08
-- Ann Arbor
UPDATE Player SET Name = 'Goddamn Goddamn' WHERE ID = 8779
UPDATE Player SET Name = 'Mel Havelka' WHERE ID = 6002
UPDATE Player SET Name = 'Hand Over Fist' WHERE ID = 8781
-- Arizona
UPDATE Player SET Name = 'Don''t Blink' WHERE ID = 5248
UPDATE Player SET Name = 'Sure, man' WHERE ID = 6161
UPDATE Player SET Name = 'Jess West' WHERE ID = 158
UPDATE Player SET Name = 'LUZER' WHERE ID = 156
-- Crime
UPDATE Player SET Number = '0347' WHERE ID = 935
UPDATE Player SET Name = 'YAS' WHERE ID = 7613
UPDATE Player SET Name = 'Kix' WHERE ID = 920
-- Denver
UPDATE Player SET Number = '20' WHERE ID = 1134
UPDATE Player SET Name = 'Leah' WHERE ID = 8131
UPDATE Player SET Name = 'Gobrecht' WHERE ID = 4655
UPDATE Player SET Name = 'CopperTop Crush' WHERE ID = 5080
UPDATE Player SET Name = 'Chelsea Garton' WHERE ID = 4626
UPDATE Player SET Name = 'Andee' WHERE ID = 1128
-- Jacksonville
UPDATE Player SET Name = 'Lily the Kid' WHERE ID = 8407
UPDATE Player SET Name = 'Lo Blow' WHERE ID = 1863
UPDATE Player SET Name = 'Lil'' Lass Kicker' WHERE ID = 1851
UPDATE Player SET Name = 'Stephanie Gentz' WHERE ID = 1853
-- London
UPDATE Player SET Number = '129' WHERE ID = 4877
UPDATE Player SET Name = 'Drac' WHERE ID = 5180
UPDATE Player SET Name = 'Gaz' WHERE ID = 2036
UPDATE Player SET Name = 'Katie Hellvetica Black' WHERE ID = 4795
UPDATE Player SET Number = '18' WHERE ID = 618
-- Minnesota
UPDATE Player SET Name = 'Switch Please' WHERE ID = 2339
UPDATE Player SET Name = 'Chu' WHERE ID = 980
UPDATE Player SET Name = 'Moose Def-initely' WHERE ID = 9089
UPDATE Player SET Name = 'Lola Frequency' WHERE ID = 9090
UPDATE Player SET Name = 'Barbell Fett' WHERE ID = 9091
UPDATE Player SET Name = 'Gay of Reckoning' WHERE ID = 2321
UPDATE Player SET Name = 'Thimbleberry Slam' WHERE ID = 9092
UPDATE Player SET Name = 'anne t. fascism' WHERE ID = 7841
-- Montreal
UPDATE Player SET Name = 'FR E SH A VOCA DO' WHERE ID = 6068
UPDATE Player SET Number = '9999' WHERE ID = 2402
UPDATE Jam_Player SET Team_PlayerID = 8294 WHERE Team_PlayerID = 10124
UPDATE Jam_Player_Effectiveness SET PlayerId = 6632 WHERE PlayerId = 8434
DELETE FROM Team_Player WHERE ID = 10124
DELETE FROM Player WHERE ID = 8434
UPDATE Player SET Name = 'Aïe' WHERE ID = 6632
UPDATE Player SET Name = 'Why So Sirius?' WHERE ID = 5100
-- Philly
UPDATE Player SET Name = 'Persephone' WHERE ID = 2942
UPDATE Player SET Name = 'Herrmann' WHERE ID = 2931
UPDATE Player SET Name = 'Crank Dat' WHERE ID = 8417
-- Queen City
UPDATE Jam_Player SET Team_PlayerID = 8198 WHERE Team_PlayerID = 9625
UPDATE Jam_Player_Effectiveness SET PlayerId = 6559 WHERE PlayerId = 7969
DELETE FROM Team_Player WHERE ID = 9625
DELETE FROM Player WHERE ID = 7969
UPDATE Jam_Player SET Team_PlayerID = 6659 WHERE Team_PlayerID = 8200
UPDATE Jam_Player_Effectiveness SET PlayerId = 5185 WHERE PlayerId = 6561
DELETE FROM Team_Player WHERE ID = 8200
DELETE FROM Player WHERE ID = 6561
UPDATE Player SET Name = 'Rosi' WHERE ID = 5185
UPDATE Player SET Name = 'Squirrel' WHERE ID = 4367
-- Rat City
UPDATE Player SET Name = 'Mountain D''Ranged' WHERE ID = 9106
UPDATE Player SET Name = 'Thumper Skull' WHERE ID = 5633
UPDATE Player SET Name = 'Habeas Corpses' WHERE ID = 4251
UPDATE Player SET Name = 'Lilly Lightning' WHERE ID = 3179
-- Stockholm
UPDATE Player SET Name = 'Thunder' WHERE ID = 4353
UPDATE Player SET Name = 'Foxen' WHERE ID = 6207
UPDATE Player SET Name = 'Fury' WHERE ID = 5192
UPDATE Player SET Name = 'Party-O' WHERE ID = 7895
UPDATE Player SET Name = 'Slinky' WHERE ID = 4076

-- 2021-06-27
UPDATE Player SET Number = '09' WHERE ID = 5093
UPDATE League SET Name = 'Bay Area Derby' WHERE Name = 'Bay Area Derby Girls'
UPDATE League SET Name = 'Central City Roller Derby' WHERE Name = 'Central City Rollergirls'