var app = angular.module('WftdaStatsApp.Controllers.TeamRating', [
    'WftdaStatsApp.Factories'
]);

// the acute-select doesn't handle the new controller aliasing, 
// so we'll kick it old school for now
app.controller('MatchupWizardController', function ($scope, TeamRatings, DTOptionsBuilder) {
    $scope.title = "Team Matchup Wizard";
    $scope.selectedTeam = null;
    $scope.finishedLoading = false;
    $scope.rankList = [];
    $scope.team = {};
    $scope.winWin = [];
    $scope.oneSided = [];
    $scope.dtOptions = DTOptionsBuilder.newOptions()
        .withOption('paging', false)
        .withOption('searching', false);

    TeamRatings.get().then(function (data) {
        $scope.rankList = data.sort(function (a, b) {
            if (a.LeagueName < b.LeagueName)
                return -1;
            if (a.LeagueName > b.LeagueName)
                return 1;
            return 0;
        });
        $scope.finishedLoading = true;
    }, function (error) {
        $scope.error = error;
        $scope.finishedLoading = true;
    });

    $scope.$watch('selectedTeam', function () {
        $scope.winWin = [];
        $scope.oneSided = [];
        $scope.ProcessTeam();
    });

    $scope.ProcessTeam = function () {
        var team1 = $scope.rankList.filter(function (obj) { return obj.TeamID == $scope.selectedTeam.TeamID; })[0];
        var arrayLength = $scope.rankList.length;
        for (i = 0; i < arrayLength; i++) {
            var curObj = $scope.rankList[i];
            if (curObj.TeamID != team1.TeamID) {
                $scope.ProcessMatchup(team1, curObj);
            }
        }
    }

    $scope.ProcessMatchup = function(team1, team2) {
        var team1Share = 1 / (1 + Math.pow(Math.E, ((team2.FtsScore - team1.FtsScore) / 100)));
        var team2Share = 1 / (1 + Math.pow(Math.E, ((team1.FtsScore - team2.FtsScore) / 100)));
        var team1WftdaNeed = team1.WftdaScore / (300 * team2.WftdaStrength);
        var team2WftdaNeed = team2.WftdaScore / (300 * team1.WftdaStrength);
        var team1Improvement = Math.round(((team1Share / team1WftdaNeed) - 1) * 100);
        var team2Improvement = Math.round(((team2Share / team2WftdaNeed) - 1) * 100);
        var share1 = Math.round(team1Share * 100);
        if (team1Improvement > 0 && team2Improvement > 0) {
            // add to win-win list
            var newEntry = { oppName: team2.LeagueName, share: share1, t1i: team1Improvement, t2i: team2Improvement };
            $scope.winWin.push(newEntry);
        }
        else if (team1Improvement > 0) {
            // add to one-sided list
            var newEntry = { oppName: team2.LeagueName, share: share1, t1i: team1Improvement, t2i: team2Improvement };
            $scope.oneSided.push(newEntry);
        }
    }
});

app.controller('TeamMatchupController', function (TeamRatings) {

});

app.controller('TeamRankingsTableController', function (TeamRatings, DTOptionsBuilder) {
    this.title = 'Combined WFTDA/FTS Ranking Data';
    this.finishedLoading = false;
    this.dtOptions = DTOptionsBuilder.newOptions()
        .withOption('paging', false);
    this.rankList = [];
    var vm = this;
    TeamRatings.get().then(function (data) {
        vm.rankList = data;
        vm.finishedLoading = true;
    }, function (error) {
        vm.error = error;
        vm.finishedLoading = true;
    });
});

app.controller('WinMarginController', function ($scope, TeamRatings, DTOptionsBuilder) {
    $scope.title = "Win Margin Calculator";
    $scope.selectedTeam = null;
    $scope.finishedLoading = false;
    $scope.rankList = [];
    $scope.team = {};
    $scope.matchupList = [];
    $scope.points = 100;
    $scope.dtOptions = DTOptionsBuilder.newOptions()
        .withOption('paging', false);

    TeamRatings.get().then(function (data) {
        $scope.rankList = data.sort(function (a, b) {
            if (a.LeagueName < b.LeagueName)
                return -1;
            if (a.LeagueName > b.LeagueName)
                return 1;
            return 0;
        });
        $scope.finishedLoading = true;
    }, function (error) {
        $scope.error = error;
        $scope.finishedLoading = true;
    });

    $scope.ProcessTeam = function () {
        $scope.matchupList = [];
        var wftdaPoints = $scope.points;
        var team1 = $scope.rankList.filter(function (obj) { return obj.TeamID == $scope.selectedTeam.TeamID; })[0];
        var arrayLength = $scope.rankList.length;
        for (i = 0; i < arrayLength; i++) {
            var curObj = $scope.rankList[i];
            if (curObj.TeamID != team1.TeamID) {
                $scope.ProcessMatchup(team1, curObj, wftdaPoints);
            }
        }
    };

    $scope.ProcessMatchup = function (team1, team2, rating) {
        var team1ShareNeed = rating / (300 * team2.WftdaStrength);
        var pointsNeed = Math.round(350 * team1ShareNeed);
        var otherPoints = 350 - pointsNeed;
        var share1 = Math.round(team1ShareNeed * 100);
        if (team1ShareNeed < 1) {
            // we can add this matchup to the list
            var newEntry = { oppName: team2.LeagueName, share: share1, score: pointsNeed + " - " + otherPoints };
            $scope.matchupList.push(newEntry);
        }
    };
});

