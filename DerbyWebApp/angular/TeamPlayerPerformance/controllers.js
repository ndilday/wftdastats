var app = angular.module('WftdaStatsApp.Controllers.TeamPlayerPerformance', [
    'WftdaStatsApp.Factories'
]);

// the acute-select doesn't handle the new controller aliasing, 
// so we'll kick it old school for now
app.controller('PlayerPointsPerformanceController', function ($scope, $filter, $q, TeamData, TeamPlayerPointPerformance, ngTreetableParams) {
    $scope.title = "Team Player Performance Calculator";
    $scope.selectedTeam = null;
    $scope.finishedLoading = false;

    $scope.teamList = [];
    $scope.blockers = [];
    $scope.jammers = [];
    
    $scope.blockerParams = new ngTreetableParams({
        getNodes: function (parent) {
            // return array of children
            return parent ? parent.children : $scope.blockers;
        },
        getTemplate: function (node) {
            return 'tree_node';
        },
        options: {
            onNodeExpand: function () {
                console.log('A node was expanded!');
            }
        }
    });

    $scope.jammerParams = new ngTreetableParams({
        getNodes: function (parent) {
            // return array of children
            return parent ? parent.children : $scope.jammers;
        },
        getTemplate: function (node) {
            return 'tree_node';
        },
        options: {
            onNodeExpand: function () {
                console.log('A node was expanded!');
            }
        }
    });

    TeamData.get().then(function (data) {
        var length = data.length;
        for (i = 0; i < length; i++) {
            data[i].longName = data[i].LeagueName + ": " + data[i].TeamName;
        }
        $scope.teamList = data.sort(function (a, b) {
            if (a.longName < b.longName)
                return -1;
            if (a.longName > b.longName)
                return 1;
            return 0;
        });
        $scope.finishedLoading = true;
    }, function (error) {
        $scope.error = error;
        $scope.finishedLoading = true;
    });

    $scope.$watch('selectedTeam', function () {
        if ($scope.selectedTeam != null) {
            $scope.finishedLoading = false;
            $scope.blockers = [];
            $scope.jammers = [];
            TeamPlayerPointPerformance.get($scope.selectedTeam.TeamID).then(function (data) {
                var results = CalculateTeamData(data);
                $scope.jammers = results.jammers.sort(ComparePlayers);
                $scope.jammerParams.refresh();
                $scope.blockers = results.blockers.sort(ComparePlayers);
                $scope.blockerParams.refresh();
                $scope.finishedLoading = true;
            }, function (error) {
                $scope.error = error;
                $scope.finishedLoading = true;
            });
        }
    });

    function CalculateTeamData(data){
        var playerCount = data.length;
        var result = { blockers: [], jammers: [] };
        for (p = 0; p < playerCount; p++) {
            var blocker = null;
            var jammer = null;
            if(data[p].BlockerPerformance.TotalJamPortions > 0){
                var bp = data[p].BlockerPerformance;
                blocker = {
                    playerName: data[p].Player.Name,
                    playerNumber: data[p].Player.Number,
                    boutDate: '',
                    homeTeam: '',
                    awayTeam: '',
                    period: '',
                    jam: $filter('number')(bp.TotalJamPortions, 2),
                    penaltyCount: bp.TotalPenalties,
                    penaltyCost: '-' + $filter('number')(bp.TotalPenaltyCost, 2),
                    averageDelta: '',
                    actualDelta: '',
                    totalPVM: $filter('number')(bp.TotalPointsVersusMedian, 2),
                    averagePVM: $filter('number')(bp.TotalPointsVersusMedian / bp.TotalJamPortions, 2),
                    averageValue: $filter('number')(bp.PlayerValueVersusTeamAverage / bp.TotalJamPortions, 2),
                    children: []
                };
                result.blockers.push(blocker);
            }
            if(data[p].JammerPerformance.TotalJamPortions > 0){
                var jp = data[p].JammerPerformance;
                jammer = {
                    playerName: data[p].Player.Name,
                    playerNumber: data[p].Player.Number,
                    boutDate: '',
                    homeTeam: '',
                    awayTeam: '',
                    period: '',
                    jam: $filter('number')(jp.TotalJamPortions, 2),
                    penaltyCount: jp.TotalPenalties,
                    penaltyCost: '-' + $filter('number')(jp.TotalPenaltyCost, 2),
                    averageDelta: '',
                    actualDelta: '',
                    totalPVM: $filter('number')(jp.TotalPointsVersusMedian, 2),
                    averagePVM: $filter('number')(jp.TotalPointsVersusMedian / jp.TotalJamPortions, 2),
                    averageValue: $filter('number')(jp.PlayerValueVersusTeamAverage / jp.TotalJamPortions, 2),
                    children: []
                };
                result.jammers.push(jammer);
            }
            ProcessBouts(blocker, jammer, data[p].Bouts);            
        }
        return result;
    }

    function ProcessBouts(blocker, jammer, boutList) {
        var boutCount = boutList.length;
        var bBout = null;
        var jBout = null;
        for (b = 0; b < boutCount; b++) {
            if(boutList[b].BlockerPerformance.TotalJamPortions > 0) {
                var bb = boutList[b].BlockerPerformance;
                bBout = {
                    playerName: '',
                    playerNumber: '',
                    boutDate: boutList[b].BoutDate.split("T")[0],
                    homeTeam: boutList[b].HomeTeamName,
                    awayTeam: boutList[b].AwayTeamName,
                    period: '',
                    jam: $filter('number')(bb.TotalJamPortions, 2),
                    penaltyCount: bb.TotalPenalties,
                    penaltyCost: '-' + $filter('number')(bb.TotalPenaltyCost, 2),
                    averageDelta: '',
                    actualDelta: '',
                    totalPVM: $filter('number')(bb.TotalPointsVersusMedian, 2),
                    averagePVM: $filter('number')(bb.TotalPointsVersusMedian / bb.TotalJamPortions, 2),
                    averageValue: $filter('number')(bb.PlayerValueVersusTeamAverage / bb.TotalJamPortions, 2),
                    children: []
                };
                blocker.children.push(bBout);
            }
            if(boutList[b].JammerPerformance.TotalJamPortions > 0) {
                var jb = boutList[b].JammerPerformance;
                jBout = {
                    playerName: '',
                    playerNumber: '',
                    boutDate: boutList[b].BoutDate.split("T")[0],
                    homeTeam: boutList[b].HomeTeamName,
                    awayTeam: boutList[b].AwayTeamName,
                    period: '',
                    jam: $filter('number')(jb.TotalJamPortions, 2),
                    penaltyCount: jb.TotalPenalties,
                    penaltyCost: '-' + $filter('number')(jb.TotalPenaltyCost, 2),
                    averageDelta: '',
                    actualDelta: '',
                    totalPVM: $filter('number')(jb.TotalPointsVersusMedian, 2),
                    averagePVM: $filter('number')(jb.TotalPointsVersusMedian / jb.TotalJamPortions, 2),
                    averageValue: $filter('number')(jb.PlayerValueVersusTeamAverage / jb.TotalJamPortions, 2),
                    children: []
                };
                jammer.children.push(jBout);
            }
            ProcessJams(bBout, jBout, boutList[b].Jams);
            if (blocker && blocker.children && blocker.children.length > 1) {
                blocker.children = blocker.children.sort(CompareBouts);
            }
            if (jammer && jammer.children && jammer.children.length > 1) {
                jammer.children = jammer.children.sort(CompareBouts);
            }
        }
    }

    function ProcessJams(bBout, jBout, jamList) {
        var jamCount = jamList.length;
        for (j = 0; j < jamCount; j++) {
            var newEntry = {
                playerName: bBout ? bBout.playerName : jBout.playerName,
                playerNumber: bBout ? bBout.playerNumber: jBout.playerNumber,
                boutDate: '',
                homeTeam: '',
                awayTeam: '',
                period: jamList[j].IsFirstHalf == true ? 1 : 2,
                jam: jamList[j].JamNumber,
                penaltyCount: jamList[j].JamPenalties,
                penaltyCost: '-' + $filter('number')(jamList[j].PenaltyCost, 2),
                averageDelta: $filter('number')(jamList[j].MedianDelta, 2),
                actualDelta: jamList[j].PointDelta,
                totalPVM: '',
                averagePVM: '',
                averageValue: '',
            };
            if (jamList[j].JammerJamPercentage > 0 || (jBout != null && bBout == null)) {
                jBout.children.push(newEntry);
            }
            else if(bBout != null) {
                bBout.children.push(newEntry);
            }
        }
        if (jBout && jBout.children && jBout.children.length > 1) {
            jBout.children = jBout.children.sort(CompareJams);
        }
        if (bBout && bBout.children && bBout.children.length > 1) {
            bBout.children = bBout.children.sort(CompareJams);
        }
    }

    function CompareJams(a, b) {
        if (a.period == 1 && b.period == 2)
            return -1;
        if (a.period == 2 && b.period == 1)
            return 1;
        if (Number(a.jam) < Number(b.jam))
            return -1;
        if (Number(a.jam) > Number(b.jam))
            return 1;
        return 0;
    }

    function CompareBouts(a, b) {
        if (a.boutDate > b.boutDate)
            return 1;
        return -1;

    }

    function ComparePlayers(a, b) {
        if (Number(a.jam) > Number(b.jam)) {
            return -1;
        }
        return 1;
    }
});

app.controller('PlayerValuePerformanceController', function ($scope, $filter, $q, TeamData, TeamPlayerValuePerformance, ngTreetableParams) {
    $scope.title = "Team Player Performance Calculator";
    $scope.selectedTeam = null;
    $scope.finishedLoading = false;

    $scope.teamList = [];
    $scope.blockers = [];
    $scope.jammers = [];

    $scope.blockerParams = new ngTreetableParams({
        getNodes: function (parent) {
            // return array of children
            return parent ? parent.children : $scope.blockers;
        },
        getTemplate: function (node) {
            return 'tree_node';
        },
        options: {
            onNodeExpand: function () {
                console.log('A node was expanded!');
            }
        }
    });

    $scope.jammerParams = new ngTreetableParams({
        getNodes: function (parent) {
            // return array of children
            return parent ? parent.children : $scope.jammers;
        },
        getTemplate: function (node) {
            return 'tree_node';
        },
        options: {
            onNodeExpand: function () {
                console.log('A node was expanded!');
            }
        }
    });

    TeamData.get().then(function (data) {
        var length = data.length;
        for (i = 0; i < length; i++) {
            data[i].longName = data[i].LeagueName + ": " + data[i].TeamName;
        }
        $scope.teamList = data.sort(function (a, b) {
            if (a.longName < b.longName)
                return -1;
            if (a.longName > b.longName)
                return 1;
            return 0;
        });
        $scope.finishedLoading = true;
    }, function (error) {
        $scope.error = error;
        $scope.finishedLoading = true;
    });

    $scope.$watch('selectedTeam', function () {
        if ($scope.selectedTeam != null) {
            $scope.finishedLoading = false;
            $scope.blockers = [];
            $scope.jammers = [];
            TeamPlayerValuePerformance.get($scope.selectedTeam.TeamID).then(function (data) {
                var results = CalculateTeamData(data);
                $scope.jammers = results.jammers.sort(ComparePlayers);
                $scope.jammerParams.refresh();
                $scope.blockers = results.blockers.sort(ComparePlayers);
                $scope.blockerParams.refresh();
                $scope.finishedLoading = true;
            }, function (error) {
                $scope.error = error;
                $scope.finishedLoading = true;
            });
        }
    });

    function CalculateTeamData(data) {
        var playerCount = data.length;
        var result = { blockers: [], jammers: [] };
        for (p = 0; p < playerCount; p++) {
            var blocker = null;
            var jammer = null;
            if (data[p].BlockerPerformance.TotalJamPortions > 0) {
                var bp = data[p].BlockerPerformance;
                blocker = {
                    playerName: data[p].Player.Name,
                    playerNumber: data[p].Player.Number,
                    boutDate: '',
                    homeTeam: '',
                    awayTeam: '',
                    period: '',
                    jam: $filter('number')(bp.TotalJamPortions, 2),
                    penaltyCount: bp.TotalPenalties,
                    penaltyCost: $filter('number')(bp.TotalPenaltyCost, 2),
                    averageDelta: '',
                    actualDelta: '',
                    totalPVM: $filter('number')(bp.TotalPointsVersusMedian * 100, 2),
                    averagePVM: $filter('number')(bp.TotalPointsVersusMedian * 100 / bp.TotalJamPortions, 2),
                    averageValue: $filter('number')(bp.PlayerValueVersusTeamAverage * 100 / bp.TotalJamPortions, 2),
                    children: []
                };
                result.blockers.push(blocker);
            }
            if (data[p].JammerPerformance.TotalJamPortions > 0) {
                var jp = data[p].JammerPerformance;
                jammer = {
                    playerName: data[p].Player.Name,
                    playerNumber: data[p].Player.Number,
                    boutDate: '',
                    homeTeam: '',
                    awayTeam: '',
                    period: '',
                    jam: $filter('number')(jp.TotalJamPortions, 2),
                    penaltyCount: jp.TotalPenalties,
                    penaltyCost: $filter('number')(jp.TotalPenaltyCost, 2),
                    averageDelta: '',
                    actualDelta: '',
                    totalPVM: $filter('number')(jp.TotalPointsVersusMedian * 100, 2),
                    averagePVM: $filter('number')(jp.TotalPointsVersusMedian * 100 / jp.TotalJamPortions, 2),
                    averageValue: $filter('number')(jp.PlayerValueVersusTeamAverage * 100 / jp.TotalJamPortions, 2),
                    children: []
                };
                result.jammers.push(jammer);
            }
            ProcessBouts(blocker, jammer, data[p].Bouts);
        }
        return result;
    }

    function ProcessBouts(blocker, jammer, boutList) {
        var boutCount = boutList.length;
        var bBout = null;
        var jBout = null;
        for (b = 0; b < boutCount; b++) {
            if (boutList[b].BlockerPerformance.TotalJamPortions > 0) {
                var bb = boutList[b].BlockerPerformance;
                bBout = {
                    playerName: '',
                    playerNumber: '',
                    boutDate: boutList[b].BoutDate.split("T")[0],
                    homeTeam: boutList[b].HomeTeamName,
                    awayTeam: boutList[b].AwayTeamName,
                    period: '',
                    jam: $filter('number')(bb.TotalJamPortions, 2),
                    penaltyCount: bb.TotalPenalties,
                    penaltyCost: $filter('number')(bb.TotalPenaltyCost, 2),
                    averageDelta: '',
                    actualDelta: '',
                    totalPVM: $filter('number')(bb.TotalPointsVersusMedian * 100, 2),
                    averagePVM: $filter('number')(bb.TotalPointsVersusMedian * 100 / bb.TotalJamPortions, 2),
                    averageValue: $filter('number')(bb.PlayerValueVersusTeamAverage * 100 / bb.TotalJamPortions, 2),
                    children: []
                };
                blocker.children.push(bBout);
            }
            if (boutList[b].JammerPerformance.TotalJamPortions > 0) {
                var jb = boutList[b].JammerPerformance;
                jBout = {
                    playerName: '',
                    playerNumber: '',
                    boutDate: boutList[b].BoutDate.split("T")[0],
                    homeTeam: boutList[b].HomeTeamName,
                    awayTeam: boutList[b].AwayTeamName,
                    period: '',
                    jam: $filter('number')(jb.TotalJamPortions, 2),
                    penaltyCount: jb.TotalPenalties,
                    penaltyCost: $filter('number')(jb.TotalPenaltyCost * 100, 2),
                    averageDelta: '',
                    actualDelta: '',
                    totalPVM: $filter('number')(jb.TotalPointsVersusMedian * 100, 2),
                    averagePVM: $filter('number')(jb.TotalPointsVersusMedian * 100 / jb.TotalJamPortions, 2),
                    averageValue: $filter('number')(jb.PlayerValueVersusTeamAverage * 100 / jb.TotalJamPortions, 2),
                    children: []
                };
                jammer.children.push(jBout);
            }
            ProcessJams(bBout, jBout, boutList[b].Jams);
            if (blocker && blocker.children && blocker.children.length > 1) {
                blocker.children = blocker.children.sort(CompareBouts);
            }
            if (jammer && jammer.children && jammer.children.length > 1) {
                jammer.children = jammer.children.sort(CompareBouts);
            }
        }
    }

    function ProcessJams(bBout, jBout, jamList) {
        var jamCount = jamList.length;
        for (j = 0; j < jamCount; j++) {
            var newEntry = {
                playerName: bBout ? bBout.playerName : jBout.playerName,
                playerNumber: bBout ? bBout.playerNumber : jBout.playerNumber,
                boutDate: '',
                homeTeam: '',
                awayTeam: '',
                period: jamList[j].IsFirstHalf == true ? 1 : 2,
                jam: jamList[j].JamNumber,
                penaltyCount: jamList[j].JamPenalties,
                penaltyCost: $filter('number')(jamList[j].PenaltyCost * 100, 2),
                averageDelta: $filter('number')(jamList[j].MedianDelta, 2),
                actualDelta: jamList[j].PointDelta,
                totalPVM: '',
                averagePVM: '',
                averageValue: '',
            };
            if (jamList[j].JammerJamPercentage > 0) {
                jBout.children.push(newEntry);
            }
            else {
                bBout.children.push(newEntry);
            }
        }
        if (jBout && jBout.children && jBout.children.length > 1) {
            jBout.children = jBout.children.sort(CompareJams);
        }
        if (bBout && bBout.children && bBout.children.length > 1) {
            bBout.children = bBout.children.sort(CompareJams);
        }
    }

    function CompareJams(a, b) {
        if (a.period == 1 && b.period == 2)
            return -1;
        if (a.period == 2 && b.period == 1)
            return 1;
        if (Number(a.jam) < Number(b.jam))
            return -1;
        if (Number(a.jam) > Number(b.jam))
            return 1;
        return 0;
    }

    function CompareBouts(a, b) {
        if (a.boutDate > b.boutDate)
            return 1;
        return -1;

    }

    function ComparePlayers(a, b) {
        if (Number(a.jam) > Number(b.jam)) {
            return -1;
        }
        return 1;
    }
});