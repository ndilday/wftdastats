# WftdaStats

A statistics tracking and analysis system for [WFTDA](https://wftda.com/) (Women's Flat Track Derby Association) roller derby. The system ingests jam-level bout data into a SQL Server database and exposes calculated player and team metrics through a .NET Web API, backed by an AngularJS front end.

---

## Project Structure

```
wftdastats/
├── Database/                        # SQL scripts for schema, data generation, and ad-hoc queries
├── DerbyDataModels/                 # C# model classes (POCOs)
├── DerbyDataAccessLayer/            # Gateway classes for SQL Server data access
├── DerbyCalculators/                # Business logic and statistical calculators
├── StatbookReader/                  # Parses WFTDA IGRF statbook Excel files and imports to DB
├── FTSReader/                       # Scrapes team and ranking data from FlatTrackStats
├── StatsScraper/                    # Scrapes live ranking data from stats.wftda.com
├── QuickTester/                     # Console app: the main data import and calculation runner
├── EPPlus/                          # Vendored EPPlus library for Excel file parsing
└── DerbyWebApp/                     # ASP.NET Web API + AngularJS front end
```

### DerbyDataModels

Plain C# model classes representing the core domain: `Bout`, `Jam`, `JamPlayer`, `Jammer`, `Player`, `Team`, `League`, `Penalty`, `PenaltyGroup`, and various derived statistics types like `PlayerPerformance`, `JamPlayerEffectiveness`, and `TeamRating`.

### DerbyDataAccessLayer

Gateway classes that handle all SQL Server interaction using `SqlConnection` and `SqlTransaction`. Each gateway corresponds to a domain entity or derived table (e.g., `BoutGateway`, `JamGateway`, `PenaltyGateway`, `PlayerGateway`, `SituationalScoreGateway`, `TeamRatingGateway`). All gateways inherit from `DerbyGatewayBase`.

### DerbyCalculators

The core statistical engine. Key calculators include:

- **`BoutDataCalculator`** — Orchestrates calculation of secondary tables: jam team effectiveness, player effectiveness, and average penalty costs.
- **`SituationalScoreCalculator`** — Computes situational scoring distributions based on foul comparison states (power jams, even strength, etc.) across all jams for a given year.
- **`PenaltyCostCalculator`** — Estimates the point cost of penalties using box time estimates and situational scores.
- **`PlayerPerformanceCalculator`** — Calculates per-player performance metrics (points contributed, penalty costs, net value) broken down by bout and jam.
- **`PlayerTrueSkillCalculator`** — Implements a TrueSkill-style Gaussian skill rating system (μ=500, β=200, σ=500/3) for jammers and blockers, updated chronologically across bouts.
- **`TeamRankingsCalculator`** — Scrapes and reconciles team rankings from WFTDA and FlatTrackStats, caching results in the database.
- **`DurationEstimatesCalculator`** — Estimates penalty box durations from bout video/timing data.
- **`TeamDataCalculator`** / **`TeamPlayerPerformanceCalculator`** — Aggregates team-level stats and per-team player performance summaries.

### StatbookReader

Parses WFTDA official game stat sheets (IGRFs) in Excel `.xlsx` format and imports the data into the database. It auto-detects the IGRF version (V1 through V4, spanning roughly 2014–present) and routes to the appropriate translator. Contains two importers:

- **`DerbyDataImporter`** — Full import: reads a parsed `StatbookModel` and upserts leagues, teams, players, jams, jam players, jammers, and penalties into the database.
- **`RinxterDataImporter`** — Imports bouts from the WFTDA Rinxter stats repository (`stats-repo.wftda.com`), downloading statbooks and supplementing them with authoritative jam-by-jam scoring data from the Rinxter API.
- **`BasicDataImporter`** — Lightweight import that stores only the minimal bout/score data without full player-level detail.

EPPlus (vendored, see below) is used for Excel parsing.

### FTSReader

Scrapes team metadata and per-game rating history from FlatTrackStats (`flattrackstats.com`). The `FTSScraper` class builds a map of all WFTDA and B-team entries, retrieves North American rankings, and can export results to CSV. Used by `TeamRankingsCalculator` to reconcile FTS rankings with WFTDA rankings.

### StatsScraper / StatsSiteReader

Scrapes live playoff ranking data directly from `stats.wftda.com`, applying time-based weighting to recent results to build a standings picture for playoff seeding purposes.

### QuickTester

A .NET console application (`Program.cs`) that serves as the primary data ingestion and calculation runner. It is not a test harness despite the name — it is the main operational entry point for populating and refreshing the database. Given a directory path as a command-line argument, it:

1. Imports any pending bouts from Rinxter
2. Walks the directory for `.xlsx` statbook files and imports each one via `DerbyDataImporter`
3. Runs the full calculation pipeline for each affected year: duration estimates → situational scores → secondary tables (jam team effectiveness, player effectiveness, average penalty costs)

### EPPlus

A vendored copy of the EPPlus library (an open-source Excel read/write library for .NET), included directly in the repository rather than via NuGet. Used by `StatbookReader` to open and parse IGRF `.xlsx` files.

### DerbyWebApp

An ASP.NET Web API (v5.2) application with an AngularJS (v1.3) single-page front end. API endpoints:

| Route | Description |
|---|---|
| `GET /api/teams` | List all teams with summary data |
| `GET /api/teams/{id}` | Get a specific team |
| `GET /api/players` | List all players |
| `GET /api/teamRatings` | Get current team ratings (WFTDA + FTS) |
| `GET /api/teamRatings/{id}` | Get rating for a specific team |

### Database

SQL Server scripts covering:

- **`derby_generate.sql`** — Primary data generation/import script
- **`derby_Rebuild.sql`** / **`derby_building_blocks.sql`** — Schema rebuild utilities
- **`updates.sql`** — Incremental data updates
- **`blocker_stats.sql`** / **`jammer_stats.sql`** — Ad-hoc stat queries for blockers and jammers (penalties, lead rate, star passes, points/delta)
- **`team_stats.sql`** — Team-level aggregates (penalty rates, lead rate, star passes per game)
- **`total_player_stats.sql`** / **`rap_sheet.sql`** — Player penalty analysis queries
- **`BlockerDeltas.sql`** / **`jam_point_frequency.sql`** / **`team_history.sql`** — Supplemental analysis queries
- **`schema.PNG`** — Database schema diagram

---

## Prerequisites

- .NET Framework 4.5
- SQL Server (connection string named `derby` in `Web.config`)
- NuGet packages (restored automatically):
  - `HtmlAgilityPack` (HTML scraping for rankings)
  - `Microsoft.AspNet.WebApi` 5.2
  - `AngularJS` 1.3, `Bootstrap` 3.3, `jQuery` 2.1, `jQuery.DataTables` 1.10
  - `Newtonsoft.Json` 6.0

---

## Setup

1. **Database** — Run the scripts in `Database/` against a SQL Server instance to create the schema and seed data. Start with `derby_building_blocks.sql`, then `derby_generate.sql`.
2. **Connection string** — Update the `derby` connection string in both `DerbyWebApp/Web.config` and `QuickTester/App.config` to point to your SQL Server instance.
3. **Build** — Open the solution in Visual Studio and restore NuGet packages, then build.
4. **Import data** — Run `QuickTester` with a path to a directory of IGRF `.xlsx` statbook files as the first argument. It will import all bouts and run the full calculation pipeline automatically.
5. **Run the web app** — Launch `DerbyWebApp`. The AngularJS front end is served from `index.html`.

---

## Key Concepts

**Situational Scoring (SSS):** Points scored per jam are normalized against the expected scoring rate for the current foul state (e.g., power jam, even strength). This produces a situational score that controls for opponent strength and game state.

**Penalty Cost:** Each penalty is assigned a point cost by estimating how many points the opposing jammer was expected to score during the time the penalized player spent in the box, using empirical box time estimates and situational scores.

**Player Effectiveness:** Net contribution of a player across jams they participated in, accounting for points scored (if jammer) or scoring facilitated/prevented (if blocker), minus penalty costs.

**TrueSkill Rating:** Players receive separate jammer and blocker skill ratings modeled as Gaussians, updated after each jam in chronological order. Ratings decay over time to reflect roster changes and skill development.

---

## Notes

- Stats filters generally apply to WFTDA-sanctioned bouts between `TeamTypeID = 1` teams from 2019 onward; earlier data exists in the database but is excluded from most calculations.
- The `FTSScraper` integration (for correlating internal team IDs with FlatTrackStats IDs) is partially implemented.
- `PlayerTrueSkillCalculator` is present but currently commented out of the main `BoutDataCalculator` orchestration flow.
