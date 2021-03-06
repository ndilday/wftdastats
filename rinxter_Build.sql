USE [master]
GO
/****** Object:  Database [rinxter]    Script Date: 9/29/2014 4:14:18 PM ******/
CREATE DATABASE [rinxter]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'derby', FILENAME = N'C:\Code\rinxter.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'derby_log', FILENAME = N'C:\Code\rinxter_log.ldf' , SIZE = 5696KB , MAXSIZE = 16777216KB , FILEGROWTH = 10%)
GO
ALTER DATABASE [rinxter] SET COMPATIBILITY_LEVEL = 110
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [rinxter].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [rinxter] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [rinxter] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [rinxter] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [rinxter] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [rinxter] SET ARITHABORT OFF 
GO
ALTER DATABASE [rinxter] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [rinxter] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [rinxter] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [rinxter] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [rinxter] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [rinxter] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [rinxter] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [rinxter] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [rinxter] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [rinxter] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [rinxter] SET  DISABLE_BROKER 
GO
ALTER DATABASE [rinxter] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [rinxter] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [rinxter] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [rinxter] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [rinxter] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [rinxter] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [rinxter] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [rinxter] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [rinxter] SET  MULTI_USER 
GO
ALTER DATABASE [rinxter] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [rinxter] SET DB_CHAINING OFF 
GO
ALTER DATABASE [rinxter] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [rinxter] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
USE [rinxter]
GO
/****** Object:  Table [dbo].[Bout]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bout](
	[ID] [int] NOT NULL,
	[HomeTeamID] [int] NOT NULL,
	[AwayTeamID] [int] NOT NULL,
	[PlayDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BoxTime]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BoxTime](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StartJamID] [int] NOT NULL,
	[EndJamID] [int] NOT NULL,
	[IsJammer] [bit] NOT NULL,
 CONSTRAINT [PK_BoxTime] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Foul]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Foul](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoulTypeID] [int] NOT NULL,
	[BoxTimeID] [int] NOT NULL,
 CONSTRAINT [PK_Foul] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FoulType]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FoulType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](2) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_FoulType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jam]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jam](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsFirstHalf] [bit] NOT NULL,
	[JamNum] [int] NOT NULL,
	[BoutID] [int] NOT NULL,
 CONSTRAINT [PK_Jam] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jam_Player]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jam_Player](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JamID] [int] NOT NULL,
	[Team_PlayerID] [int] NOT NULL,
	[IsJammer] [bit] NOT NULL,
	[IsPivot] [bit] NOT NULL,
 CONSTRAINT [PK_Jam_Player] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jammer]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jammer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Jam_PlayerID] [int] NOT NULL,
	[Points] [int] NOT NULL,
	[Lost] [bit] NOT NULL,
	[Lead] [bit] NOT NULL,
	[Called] [bit] NOT NULL,
	[NoPass] [bit] NOT NULL,
	[PassedStar] [bit] NOT NULL,
	[ReceivedStar] [bit] NOT NULL,
 CONSTRAINT [PK_Jammer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[League]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[League](
	[ID] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[JoinDate] [datetime] NOT NULL,
 CONSTRAINT [PK_League] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Player]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Player](
	[ID] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Number] [nvarchar](10) NOT NULL,
 CONSTRAINT [Player_pk] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Team]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Team](
	[ID] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[LeagueID] [int] NOT NULL,
	[TeamTypeID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Team_Player]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Team_Player](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TeamID] [int] NOT NULL,
	[PlayerID] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NULL,
 CONSTRAINT [PK_Team_Player] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TeamType]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeamType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  View [dbo].[Blocker_Stats_View]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/* jammer stats*/
CREATE VIEW [dbo].[Blocker_Stats_View]
AS
select tp.ID, tp.TeamID, COUNT(f.ID) AS Fouls
from Team_Player tp
join Jam_Player jp on jp.Team_PlayerID = tp.ID
LEFT JOIN BoxTime bt on bt.StartJamID = jp.ID
LEFT JOIN Foul f on f.BoxTimeID = bt.ID
WHERE
	jp.IsJammer = 0
GROUP BY tp.ID, tp.TeamID



GO
/****** Object:  View [dbo].[Jam_Team_Fouls_View]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* Jam Team Foul*/
CREATE VIEW [dbo].[Jam_Team_Fouls_View]
AS
WITH JamTeamFoul(JamID, BoxTimeID, IsJammer, TeamID, FoulAmount) AS (SELECT        j3.ID, bt.ID AS Expr1, bt.IsJammer, tp.TeamID, CASE WHEN j3.ID = j1.ID AND 
                                                                                                                                                                                                 j3.ID = j2.ID THEN 1.0 WHEN j3.ID = j1.ID AND 
                                                                                                                                                                                                 j3.ID != j2.ID THEN 0.5 WHEN j3.ID != j1.ID AND 
                                                                                                                                                                                                 j3.ID = j2.ID THEN 0.5 ELSE 1.0 END AS Expr2
                                                                                                                                                                       FROM            dbo.BoxTime AS bt INNER JOIN
                                                                                                                                                                                                 dbo.Jam_Player AS jp1 ON jp1.ID = bt.StartJamID INNER JOIN
                                                                                                                                                                                                 dbo.Jam AS j1 ON j1.ID = jp1.JamID INNER JOIN
                                                                                                                                                                                                 dbo.Jam_Player AS jp2 ON jp2.ID = bt.EndJamID INNER JOIN
                                                                                                                                                                                                 dbo.Jam AS j2 ON j2.ID = jp2.JamID INNER JOIN
                                                                                                                                                                                                 dbo.Jam AS j3 ON j3.BoutID = j1.BoutID AND j1.ID <= j3.ID AND 
                                                                                                                                                                                                 j2.ID >= j3.ID INNER JOIN
                                                                                                                                                                                                 dbo.Team_Player AS tp ON tp.ID = jp1.Team_PlayerID), BoutTeams(BoutID, TeamID) 
AS
    (SELECT        ID, HomeTeamID
      FROM            dbo.Bout
      UNION
      SELECT        ID, AwayTeamID
      FROM            dbo.Bout AS Bout_1), SummedJammerFouls(JamID, TeamID, JammerFouls) AS
    (SELECT        j.ID, bt.TeamID, ISNULL(SUM(jtf1.FoulAmount), 0.0)
      FROM            dbo.Jam AS j INNER JOIN
                                BoutTeams AS bt ON bt.BoutID = j.BoutID LEFT OUTER JOIN
                                JamTeamFoul AS jtf1 ON jtf1.JamID = j.ID AND jtf1.TeamID = bt.TeamID AND jtf1.IsJammer = 1
      GROUP BY j.ID, bt.TeamID), 
SummedBlockerFouls(JamID, TeamID, BlockerFouls) AS
    (SELECT        j.ID, bt.TeamID, ISNULL(SUM(jtf1.FoulAmount), 0.0)
      FROM            dbo.Jam AS j INNER JOIN
                                BoutTeams AS bt ON bt.BoutID = j.BoutID LEFT OUTER JOIN
                                JamTeamFoul AS jtf1 ON jtf1.JamID = j.ID AND jtf1.TeamID = bt.TeamID AND jtf1.IsJammer = 0
      GROUP BY j.ID, bt.TeamID), JamScore(JamID, TeamID, GotLead, Points) AS
    (SELECT        dbo.Jam.ID, htp.TeamID, hj.Lead, hj.Points
      FROM            dbo.Jam INNER JOIN
                                dbo.Jam_Player AS hjp ON hjp.JamID = dbo.Jam.ID INNER JOIN
                                dbo.Team_Player AS htp ON htp.ID = hjp.Team_PlayerID INNER JOIN
                                dbo.Jammer AS hj ON hj.Jam_PlayerID = hjp.ID INNER JOIN
                                dbo.Player AS hp ON hp.ID = htp.PlayerID
      WHERE        (hj.ReceivedStar = 0) AND (hj.PassedStar = 0)), Combined(JamID, TeamID, Points, OppJamFouls, OppBlockFouls) AS
    (SELECT        js.JamID, js.TeamID, js.Points, sjf.JammerFouls AS OppJamFouls, sbf.BlockerFouls AS OppBlockFouls
      FROM            SummedJammerFouls AS sjf INNER JOIN
                                SummedBlockerFouls AS sbf ON sbf.JamID = sjf.JamID AND sbf.TeamID = sjf.TeamID INNER JOIN
                                JamScore AS js ON js.JamID = sjf.JamID AND js.TeamID <> sjf.TeamID)
    SELECT        JamID, TeamID, Points, OppJamFouls, OppBlockFouls
     FROM            Combined AS Combined_1


GO
/****** Object:  View [dbo].[Jammer_Stats_View]    Script Date: 9/29/2014 4:14:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/* jammer stats*/
CREATE VIEW [dbo].[Jammer_Stats_View]
AS
SELECT        tp.PlayerID, tp.TeamID, COUNT(j.Jam_PlayerID) AS Jams, SUM(j.Points) AS Points, SUM(CASE WHEN j.Lead = 1 THEN 1 ELSE 0 END) AS Lead, 
                         SUM(CASE WHEN j.Called = 1 THEN 1 ELSE 0 END) AS Called, COUNT(f.ID) AS Fouls, SUM(CASE WHEN j.PassedStar = 1 THEN 1 ELSE 0 END) AS Passed
FROM            dbo.Team_Player AS tp INNER JOIN
                         dbo.Jam_Player AS jp ON jp.Team_PlayerID = tp.ID INNER JOIN
                         dbo.Jammer AS j ON j.Jam_PlayerID = jp.ID LEFT OUTER JOIN
                         dbo.BoxTime AS bt ON bt.StartJamID = jp.ID LEFT OUTER JOIN
                         dbo.Foul AS f ON f.BoxTimeID = bt.ID
WHERE        (j.ReceivedStar = 0)
GROUP BY tp.PlayerID, tp.TeamID

GO
/****** Object:  Index [IDX_Jam_ID_BoutID]    Script Date: 9/29/2014 4:14:18 PM ******/
CREATE NONCLUSTERED INDEX [IDX_Jam_ID_BoutID] ON [dbo].[Jam]
(
	[ID] ASC,
	[BoutID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ID_JamID]    Script Date: 9/29/2014 4:14:18 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ID_JamID] ON [dbo].[Jam_Player]
(
	[ID] ASC,
	[JamID] ASC
)
INCLUDE ( 	[Team_PlayerID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ID_TPID]    Script Date: 9/29/2014 4:14:18 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ID_TPID] ON [dbo].[Jam_Player]
(
	[ID] ASC,
	[Team_PlayerID] ASC
)
INCLUDE ( 	[JamID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ID_PlayerID]    Script Date: 9/29/2014 4:14:18 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ID_PlayerID] ON [dbo].[Team_Player]
(
	[ID] ASC,
	[PlayerID] ASC
)
INCLUDE ( 	[TeamID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ID_TeamID]    Script Date: 9/29/2014 4:14:18 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ID_TeamID] ON [dbo].[Team_Player]
(
	[ID] ASC,
	[TeamID] ASC
)
INCLUDE ( 	[PlayerID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[BoxTime] ADD  CONSTRAINT [DF_BoxTime_IsJammer]  DEFAULT ((0)) FOR [IsJammer]
GO
ALTER TABLE [dbo].[Jam] ADD  DEFAULT ((0)) FOR [BoutID]
GO
ALTER TABLE [dbo].[Jam_Player] ADD  DEFAULT ((0)) FOR [IsJammer]
GO
ALTER TABLE [dbo].[Jam_Player] ADD  DEFAULT ((0)) FOR [IsPivot]
GO
ALTER TABLE [dbo].[Jammer] ADD  DEFAULT ((0)) FOR [Lost]
GO
ALTER TABLE [dbo].[Jammer] ADD  DEFAULT ((0)) FOR [Lead]
GO
ALTER TABLE [dbo].[Jammer] ADD  DEFAULT ((0)) FOR [Called]
GO
ALTER TABLE [dbo].[Jammer] ADD  DEFAULT ((0)) FOR [NoPass]
GO
ALTER TABLE [dbo].[Jammer] ADD  DEFAULT ((0)) FOR [PassedStar]
GO
ALTER TABLE [dbo].[Jammer] ADD  DEFAULT ((0)) FOR [ReceivedStar]
GO
ALTER TABLE [dbo].[Bout]  WITH NOCHECK ADD  CONSTRAINT [FK_Bout_AwayTeam] FOREIGN KEY([AwayTeamID])
REFERENCES [dbo].[Team] ([ID])
GO
ALTER TABLE [dbo].[Bout] CHECK CONSTRAINT [FK_Bout_AwayTeam]
GO
ALTER TABLE [dbo].[Bout]  WITH NOCHECK ADD  CONSTRAINT [FK_Bout_HomeTeam] FOREIGN KEY([HomeTeamID])
REFERENCES [dbo].[Team] ([ID])
GO
ALTER TABLE [dbo].[Bout] CHECK CONSTRAINT [FK_Bout_HomeTeam]
GO
ALTER TABLE [dbo].[BoxTime]  WITH NOCHECK ADD  CONSTRAINT [FK_BoxTime_Jam_Player] FOREIGN KEY([StartJamID])
REFERENCES [dbo].[Jam_Player] ([ID])
GO
ALTER TABLE [dbo].[BoxTime] CHECK CONSTRAINT [FK_BoxTime_Jam_Player]
GO
ALTER TABLE [dbo].[BoxTime]  WITH NOCHECK ADD  CONSTRAINT [FK_BoxTime_Jam_Player1] FOREIGN KEY([EndJamID])
REFERENCES [dbo].[Jam_Player] ([ID])
GO
ALTER TABLE [dbo].[BoxTime] CHECK CONSTRAINT [FK_BoxTime_Jam_Player1]
GO
ALTER TABLE [dbo].[Foul]  WITH NOCHECK ADD  CONSTRAINT [FK_Foul_BoxTime] FOREIGN KEY([BoxTimeID])
REFERENCES [dbo].[BoxTime] ([ID])
GO
ALTER TABLE [dbo].[Foul] CHECK CONSTRAINT [FK_Foul_BoxTime]
GO
ALTER TABLE [dbo].[Foul]  WITH NOCHECK ADD  CONSTRAINT [FK_Foul_FoulType] FOREIGN KEY([FoulTypeID])
REFERENCES [dbo].[FoulType] ([ID])
GO
ALTER TABLE [dbo].[Foul] CHECK CONSTRAINT [FK_Foul_FoulType]
GO
ALTER TABLE [dbo].[Jam]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Bout] FOREIGN KEY([BoutID])
REFERENCES [dbo].[Bout] ([ID])
GO
ALTER TABLE [dbo].[Jam] CHECK CONSTRAINT [FK_Jam_Bout]
GO
ALTER TABLE [dbo].[Jam_Player]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Player_Team_Player] FOREIGN KEY([Team_PlayerID])
REFERENCES [dbo].[Team_Player] ([ID])
GO
ALTER TABLE [dbo].[Jam_Player] CHECK CONSTRAINT [FK_Jam_Player_Team_Player]
GO
ALTER TABLE [dbo].[Jammer]  WITH NOCHECK ADD  CONSTRAINT [FK_Jammer_Jam_Player] FOREIGN KEY([Jam_PlayerID])
REFERENCES [dbo].[Jam_Player] ([ID])
GO
ALTER TABLE [dbo].[Jammer] CHECK CONSTRAINT [FK_Jammer_Jam_Player]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_League] FOREIGN KEY([LeagueID])
REFERENCES [dbo].[League] ([ID])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_League]
GO
ALTER TABLE [dbo].[Team]  WITH CHECK ADD  CONSTRAINT [FK_Team_TeamType] FOREIGN KEY([TeamTypeID])
REFERENCES [dbo].[TeamType] ([ID])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_TeamType]
GO
ALTER TABLE [dbo].[Team_Player]  WITH NOCHECK ADD  CONSTRAINT [FK_Team_Player_Player] FOREIGN KEY([PlayerID])
REFERENCES [dbo].[Player] ([ID])
GO
ALTER TABLE [dbo].[Team_Player] CHECK CONSTRAINT [FK_Team_Player_Player]
GO
ALTER TABLE [dbo].[Team_Player]  WITH NOCHECK ADD  CONSTRAINT [FK_Team_Player_Team] FOREIGN KEY([TeamID])
REFERENCES [dbo].[Team] ([ID])
GO
ALTER TABLE [dbo].[Team_Player] CHECK CONSTRAINT [FK_Team_Player_Team]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Combined_1"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 160
               Right = 276
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Jam_Team_Fouls_View'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Jam_Team_Fouls_View'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tp"
            Begin Extent = 
               Top = 6
               Left = 262
               Bottom = 135
               Right = 448
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "jp"
            Begin Extent = 
               Top = 6
               Left = 486
               Bottom = 135
               Right = 672
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "j"
            Begin Extent = 
               Top = 6
               Left = 710
               Bottom = 135
               Right = 896
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "bt"
            Begin Extent = 
               Top = 6
               Left = 934
               Bottom = 135
               Right = 1120
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "f"
            Begin Extent = 
               Top = 6
               Left = 1158
               Bottom = 118
               Right = 1344
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Jammer_Stats_View'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Jammer_Stats_View'
GO
USE [master]
GO
ALTER DATABASE [rinxter] SET  READ_WRITE 
GO
