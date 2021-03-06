USE [master]
GO
/****** Object:  Database [rinxter]    Script Date: 1/22/2015 9:33:30 PM ******/
CREATE DATABASE [rinxter]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'derby', FILENAME = N'C:\Code\rinxter.mdf' , SIZE = 24576KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'derby_log', FILENAME = N'C:\Code\rinxter_log.ldf' , SIZE = 136064KB , MAXSIZE = 16777216KB , FILEGROWTH = 10%)
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
ALTER DATABASE [rinxter] SET DELAYED_DURABILITY = DISABLED 
GO
USE [rinxter]
GO
/****** Object:  Table [dbo].[Bout]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bout](
	[RinxterID] [int] NULL,
	[HomeTeamID] [int] NOT NULL,
	[AwayTeamID] [int] NOT NULL,
	[PlayDate] [datetime] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BoxTime]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BoxTime](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JamPlayerID] [int] NOT NULL,
	[IsJammer] [bit] NOT NULL CONSTRAINT [DF_BoxTime_IsJammer]  DEFAULT ((0)),
	[StartedInBox] [bit] NOT NULL,
	[EndedInBox] [bit] NOT NULL,
 CONSTRAINT [PK_BoxTime] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BoxTimeEstimate]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BoxTimeEstimate](
	[BoxTimeID] [int] NOT NULL,
	[Estimate] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BoxTimeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FoulType]    Script Date: 1/22/2015 9:33:31 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jam]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jam](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IsFirstHalf] [bit] NOT NULL,
	[JamNum] [int] NOT NULL,
	[BoutID] [int] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_Jam] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jam_Player]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jam_Player](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[JamID] [int] NOT NULL,
	[Team_PlayerID] [int] NOT NULL,
	[IsJammer] [bit] NOT NULL DEFAULT ((0)),
	[IsPivot] [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_Jam_Player] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jam_Player_Effectiveness]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jam_Player_Effectiveness](
	[JamID] [int] NOT NULL,
	[PlayerID] [int] NOT NULL,
	[JamPortion] [float] NOT NULL,
	[PenaltyCost] [float] NOT NULL,
	[BaseEffectiveness] [float] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jam_Team_Effectiveness]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jam_Team_Effectiveness](
	[JamID] [int] NOT NULL,
	[TeamID] [int] NOT NULL,
	[Percentile] [float] NOT NULL,
 CONSTRAINT [PK_Jam_Team_Effectiveness] PRIMARY KEY CLUSTERED 
(
	[JamID] ASC,
	[TeamID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jammer]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jammer](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Jam_PlayerID] [int] NOT NULL,
	[Points] [int] NOT NULL,
	[Lost] [bit] NOT NULL DEFAULT ((0)),
	[Lead] [bit] NOT NULL DEFAULT ((0)),
	[Called] [bit] NOT NULL DEFAULT ((0)),
	[Injury] [bit] NOT NULL DEFAULT ((0)),
	[NoPass] [bit] NOT NULL DEFAULT ((0)),
	[PassedStar] [bit] NOT NULL DEFAULT ((0)),
	[ReceivedStar] [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_Jammer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[JamTimeEstimate]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JamTimeEstimate](
	[JamID] [int] NOT NULL,
	[Seconds] [int] NOT NULL,
	[Minimum] [int] NOT NULL,
	[Maximum] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[JamID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[League]    Script Date: 1/22/2015 9:33:31 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Penalty]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Penalty](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoulTypeID] [int] NOT NULL,
	[JamPlayerID] [int] NULL,
 CONSTRAINT [PK_Foul] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Penalty_BoxTime]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Penalty_BoxTime](
	[GroupID] [int] NOT NULL,
	[PenaltyID] [int] NOT NULL,
	[BoxTimeID] [int] NOT NULL,
 CONSTRAINT [pk_Penalty_BoxTime] PRIMARY KEY CLUSTERED 
(
	[PenaltyID] ASC,
	[BoxTimeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Player]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Player](
	[RinxterID] [int] NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Number] [nvarchar](10) NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SituationalScore]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SituationalScore](
	[Team1JammerBoxPortion] [float] NOT NULL,
	[Team1BlockerBoxPortion] [float] NOT NULL,
	[Team2JammerBoxPortion] [float] NOT NULL,
	[Team2BlockerBoxPortion] [float] NOT NULL,
	[PointDelta] [int] NOT NULL,
	[Percentile] [float] NULL,
 CONSTRAINT [PK_SituationalScore] PRIMARY KEY CLUSTERED 
(
	[Team1JammerBoxPortion] ASC,
	[Team1BlockerBoxPortion] ASC,
	[Team2JammerBoxPortion] ASC,
	[Team2BlockerBoxPortion] ASC,
	[PointDelta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Team]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Team](
	[RinxterID] [int] NULL,
	[Name] [nvarchar](max) NOT NULL,
	[LeagueID] [int] NOT NULL,
	[TeamTypeID] [int] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Team_Player]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Team_Player](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TeamID] [int] NOT NULL,
	[PlayerID] [int] NOT NULL,
 CONSTRAINT [PK_Team_Player] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TeamNameMapper]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeamNameMapper](
	[TeamID] [int] NULL,
	[NameSpelling] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TeamRating]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeamRating](
	[TeamID] [int] NOT NULL,
	[WftdaRank] [int] NOT NULL,
	[WftdaScore] [float] NOT NULL,
	[WftdaStrength] [float] NOT NULL,
	[FtsRank] [int] NOT NULL,
	[FtsScore] [float] NOT NULL,
	[AddedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TeamID] ASC,
	[AddedDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TeamType]    Script Date: 1/22/2015 9:33:31 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tmpVar]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tmpVar](
	[ID] [int] NOT NULL,
	[bt] [int] NOT NULL,
	[p] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetJamTeamBoxTimeSeconds]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[GetJamTeamBoxTimeSeconds]
(	
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT jp.JamID, 
		tp.TeamID,
		bt.ID AS BoxTimeID, 
		bt.IsJammer,  
		btde.BoxSeconds
	FROM    dbo.BoxTime bt
	JOIN	dbo.BoxTimeDurationEstimate_View btde ON btde.BoxTimeID = bt.ID
	JOIN	dbo.Jam_Player jp ON bt.JamPlayerID = jp.ID
	JOIN	dbo.Team_Player tp ON tp.ID = jp.Team_PlayerID
)






GO
/****** Object:  View [dbo].[Dual_Jammer_Penalty_Jam_View]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Dual_Jammer_Penalty_Jam_View] AS
SELECT jp.JamID, b.ID AS BoxTimeID
FROM BoxTime b
JOIN Jam_Player jp ON jp.ID = b.JamPlayerID
JOIN Jam_Player jp2 ON jp2.JamID = jp.JamID AND jp2.ID != jp.ID
JOIN BoxTime b2 ON b2.JamPlayerID = jp2.ID
WHERE
	jp.IsJammer = 1 AND
	jp2.IsJammer = 1
GO
/****** Object:  View [dbo].[Jam_Player_Penalty_Count_View]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





/* jammer stats*/
CREATE VIEW [dbo].[Jam_Player_Penalty_Count_View]
AS
SELECT jp.ID AS Jam_PlayerID, COUNT(p.ID) AS PenaltyCount
FROM Jam_Player jp
JOIN Penalty p ON p.JamPlayerID = jp.ID
GROUP BY jp.ID






GO
/****** Object:  View [dbo].[Jam_Team_Data_View]    Script Date: 1/22/2015 9:33:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/* Jam Team Foul*/
CREATE VIEW [dbo].[Jam_Team_Data_View]
AS
WITH JamPlayerMap(JamID, JamPlayerID, TeamID, IsJammer) AS
(
	SELECT jp.JamID, jp.ID, tp.TeamID, jp.IsJammer
	FROM Jam_Player jp
	JOIN Team_Player tp ON tp.ID = jp.Team_PlayerID
),
JamPoints(JamID, TeamID, Points) AS
(
	SELECT jpm.JamID, jpm.TeamID, SUM(j.Points)
	FROM JamPlayerMap jpm
	JOIN Jammer j ON j.Jam_PlayerID = jpm.JamPlayerID
	GROUP BY jpm.JamID, jpm.TeamID
),
JamJammerBoxSeconds(JamID, TeamID, BoxSeconds) AS
(
	SELECT jpm.JamID, jpm.TeamID, SUM(bte.Estimate)
	FROM JamPlayerMap jpm
	JOIN BoxTime bt ON bt.JamPlayerID = jpm.JamPlayerID
	JOIN BoxTimeEstimate bte ON bte.BoxTimeID = bt.ID
	WHERE
		bt.IsJammer = 1
	GROUP BY jpm.JamID, jpm.TeamID
),
JamBlockerBoxSeconds(JamID, TeamID, BoxSeconds) AS
(
	SELECT jpm.JamID, jpm.TeamID, SUM(bte.Estimate)
	FROM JamPlayerMap jpm
	JOIN BoxTime bt ON bt.JamPlayerID = jpm.JamPlayerID
	JOIN BoxTimeEstimate bte ON bte.BoxTimeID = bt.ID
	WHERE
		bt.IsJammer = 0
	GROUP BY jpm.JamID, jpm.TeamID
),
GroupedData(JamID, TeamID, JammerBoxSeconds, BlockerBoxSeconds, OppJammerBoxSeconds, OppBlockerBoxSeconds, PointDelta) AS
(
	SELECT jp1.JamID, jp1.TeamID, ISNULL(jjs1.BoxSeconds,0), ISNULL(jbs1.BoxSeconds,0), ISNULL(jjs2.BoxSeconds,0), ISNULL(jbs2.BoxSeconds,0), jp1.Points - jp2.Points
	FROM JamPoints jp1
	JOIN JamPoints jp2 ON jp2.JamID = jp1.JamID AND jp2.TeamID != jp1.TeamID
	LEFT JOIN JamJammerBoxSeconds jjs1 ON jjs1.JamID = jp1.JamID AND jjs1.TeamID = jp1.TeamID
	LEFT JOIN JamJammerBoxSeconds jjs2 ON jjs2.JamID = jp1.JamID AND jjs2.TeamID != jp1.TeamID
	LEFT JOIN JamBlockerBoxSeconds jbs1 ON jbs1.JamID = jp1.JamID AND jbs1.TeamID = jp1.TeamID
	LEFT JOIN JamBlockerBoxSeconds jbs2 ON jbs2.JamID = jp1.JamID AND jbs2.TeamID != jp1.TeamID
)
SELECT * FROM GroupedData




GO
/****** Object:  Index [idx_Bout_RinxterID_notnull]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [idx_Bout_RinxterID_notnull] ON [dbo].[Bout]
(
	[RinxterID] ASC
)
WHERE ([RinxterID] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [IDX_Jam_ID_BoutID]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE NONCLUSTERED INDEX [IDX_Jam_ID_BoutID] ON [dbo].[Jam]
(
	[ID] ASC,
	[BoutID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ID_JamID]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ID_JamID] ON [dbo].[Jam_Player]
(
	[ID] ASC,
	[JamID] ASC
)
INCLUDE ( 	[Team_PlayerID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ID_TPID]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ID_TPID] ON [dbo].[Jam_Player]
(
	[ID] ASC,
	[Team_PlayerID] ASC
)
INCLUDE ( 	[JamID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [IDX_Jam_Player_JamID_TP_IsJammer]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE NONCLUSTERED INDEX [IDX_Jam_Player_JamID_TP_IsJammer] ON [dbo].[Jam_Player]
(
	[JamID] ASC
)
INCLUDE ( 	[Team_PlayerID],
	[IsJammer]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [IDX_Jam_Player_TPID_2]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE NONCLUSTERED INDEX [IDX_Jam_Player_TPID_2] ON [dbo].[Jam_Player]
(
	[Team_PlayerID] ASC
)
INCLUDE ( 	[JamID],
	[IsJammer]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [idx_Player_RinxterID_notnull]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [idx_Player_RinxterID_notnull] ON [dbo].[Player]
(
	[RinxterID] ASC
)
WHERE ([RinxterID] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [idx_Team_RinxterID_notnull]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [idx_Team_RinxterID_notnull] ON [dbo].[Team]
(
	[RinxterID] ASC
)
WHERE ([RinxterID] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ID_PlayerID]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ID_PlayerID] ON [dbo].[Team_Player]
(
	[ID] ASC,
	[PlayerID] ASC
)
INCLUDE ( 	[TeamID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO
/****** Object:  Index [IDX_ID_TeamID]    Script Date: 1/22/2015 9:33:31 PM ******/
CREATE NONCLUSTERED INDEX [IDX_ID_TeamID] ON [dbo].[Team_Player]
(
	[ID] ASC,
	[TeamID] ASC
)
INCLUDE ( 	[PlayerID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
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
ALTER TABLE [dbo].[BoxTime]  WITH NOCHECK ADD  CONSTRAINT [FK_BoxTime_Jam_Player] FOREIGN KEY([JamPlayerID])
REFERENCES [dbo].[Jam_Player] ([ID])
GO
ALTER TABLE [dbo].[BoxTime] CHECK CONSTRAINT [FK_BoxTime_Jam_Player]
GO
ALTER TABLE [dbo].[BoxTimeEstimate]  WITH NOCHECK ADD  CONSTRAINT [fk_BoxTimeEstimate_BoxTime] FOREIGN KEY([BoxTimeID])
REFERENCES [dbo].[BoxTime] ([ID])
GO
ALTER TABLE [dbo].[BoxTimeEstimate] CHECK CONSTRAINT [fk_BoxTimeEstimate_BoxTime]
GO
ALTER TABLE [dbo].[Jam]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Bout] FOREIGN KEY([BoutID])
REFERENCES [dbo].[Bout] ([ID])
GO
ALTER TABLE [dbo].[Jam] CHECK CONSTRAINT [FK_Jam_Bout]
GO
ALTER TABLE [dbo].[Jam_Player]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Player_Jam] FOREIGN KEY([JamID])
REFERENCES [dbo].[Jam] ([ID])
GO
ALTER TABLE [dbo].[Jam_Player] CHECK CONSTRAINT [FK_Jam_Player_Jam]
GO
ALTER TABLE [dbo].[Jam_Player]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Player_Team_Player] FOREIGN KEY([Team_PlayerID])
REFERENCES [dbo].[Team_Player] ([ID])
GO
ALTER TABLE [dbo].[Jam_Player] CHECK CONSTRAINT [FK_Jam_Player_Team_Player]
GO
ALTER TABLE [dbo].[Jam_Player_Effectiveness]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Player_Effectiveness_Jam] FOREIGN KEY([JamID])
REFERENCES [dbo].[Jam] ([ID])
GO
ALTER TABLE [dbo].[Jam_Player_Effectiveness] CHECK CONSTRAINT [FK_Jam_Player_Effectiveness_Jam]
GO
ALTER TABLE [dbo].[Jam_Player_Effectiveness]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Player_Effectiveness_Player] FOREIGN KEY([PlayerID])
REFERENCES [dbo].[Player] ([ID])
GO
ALTER TABLE [dbo].[Jam_Player_Effectiveness] CHECK CONSTRAINT [FK_Jam_Player_Effectiveness_Player]
GO
ALTER TABLE [dbo].[Jam_Team_Effectiveness]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Team_Effectiveness_Jam] FOREIGN KEY([JamID])
REFERENCES [dbo].[Jam] ([ID])
GO
ALTER TABLE [dbo].[Jam_Team_Effectiveness] CHECK CONSTRAINT [FK_Jam_Team_Effectiveness_Jam]
GO
ALTER TABLE [dbo].[Jam_Team_Effectiveness]  WITH NOCHECK ADD  CONSTRAINT [FK_Jam_Team_Effectiveness_Team] FOREIGN KEY([TeamID])
REFERENCES [dbo].[Team] ([ID])
GO
ALTER TABLE [dbo].[Jam_Team_Effectiveness] CHECK CONSTRAINT [FK_Jam_Team_Effectiveness_Team]
GO
ALTER TABLE [dbo].[Jammer]  WITH NOCHECK ADD  CONSTRAINT [FK_Jammer_Jam_Player] FOREIGN KEY([Jam_PlayerID])
REFERENCES [dbo].[Jam_Player] ([ID])
GO
ALTER TABLE [dbo].[Jammer] CHECK CONSTRAINT [FK_Jammer_Jam_Player]
GO
ALTER TABLE [dbo].[JamTimeEstimate]  WITH NOCHECK ADD  CONSTRAINT [FK_JamTimeEstimate_Jam] FOREIGN KEY([JamID])
REFERENCES [dbo].[Jam] ([ID])
GO
ALTER TABLE [dbo].[JamTimeEstimate] CHECK CONSTRAINT [FK_JamTimeEstimate_Jam]
GO
ALTER TABLE [dbo].[Penalty]  WITH NOCHECK ADD FOREIGN KEY([JamPlayerID])
REFERENCES [dbo].[Jam_Player] ([ID])
GO
ALTER TABLE [dbo].[Penalty]  WITH NOCHECK ADD  CONSTRAINT [FK_Foul_FoulType] FOREIGN KEY([FoulTypeID])
REFERENCES [dbo].[FoulType] ([ID])
GO
ALTER TABLE [dbo].[Penalty] CHECK CONSTRAINT [FK_Foul_FoulType]
GO
ALTER TABLE [dbo].[Penalty_BoxTime]  WITH NOCHECK ADD  CONSTRAINT [fk_PenaltyBoxTime_BoxTime] FOREIGN KEY([BoxTimeID])
REFERENCES [dbo].[BoxTime] ([ID])
GO
ALTER TABLE [dbo].[Penalty_BoxTime] CHECK CONSTRAINT [fk_PenaltyBoxTime_BoxTime]
GO
ALTER TABLE [dbo].[Penalty_BoxTime]  WITH NOCHECK ADD  CONSTRAINT [fk_PenaltyBoxTime_Penalty] FOREIGN KEY([PenaltyID])
REFERENCES [dbo].[Penalty] ([ID])
GO
ALTER TABLE [dbo].[Penalty_BoxTime] CHECK CONSTRAINT [fk_PenaltyBoxTime_Penalty]
GO
ALTER TABLE [dbo].[Team]  WITH NOCHECK ADD  CONSTRAINT [FK_Team_League] FOREIGN KEY([LeagueID])
REFERENCES [dbo].[League] ([ID])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_Team_League]
GO
ALTER TABLE [dbo].[Team]  WITH NOCHECK ADD  CONSTRAINT [FK_Team_TeamType] FOREIGN KEY([TeamTypeID])
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
ALTER TABLE [dbo].[TeamNameMapper]  WITH NOCHECK ADD  CONSTRAINT [FK_TeamNameMapper_Team] FOREIGN KEY([TeamID])
REFERENCES [dbo].[Team] ([ID])
GO
ALTER TABLE [dbo].[TeamNameMapper] CHECK CONSTRAINT [FK_TeamNameMapper_Team]
GO
ALTER TABLE [dbo].[TeamRating]  WITH NOCHECK ADD  CONSTRAINT [FK_TeamRating_Team] FOREIGN KEY([TeamID])
REFERENCES [dbo].[Team] ([ID])
GO
ALTER TABLE [dbo].[TeamRating] CHECK CONSTRAINT [FK_TeamRating_Team]
GO
USE [master]
GO
ALTER DATABASE [rinxter] SET  READ_WRITE 
GO
