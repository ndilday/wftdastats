var app = angular.module('WftdaStatsApp', [
    'acute.select',
    'angularSpinner',
    'datatables',
    'ngRoute',
    'ngTreetable',
    'ui.bootstrap',
    'WftdaStatsApp.Controllers.TeamRating',
    'WftdaStatsApp.Controllers.TeamPlayerPerformance',
    'WftdaStatsApp.Factories'
]);
app.config(function ($routeProvider) {
    $routeProvider
        .when('/home', {
            templateUrl: '/angular/_home.html'
        })
        .when('/teamRating', {
            templateUrl: '/angular/TeamRating/_index.html'
        })
        .when('/teamRating/matchupWizard', {
            templateUrl: '/angular/TeamRating/_matchupWizard.html',
            controller: 'MatchupWizardController'
        })
        .when('/teamRating/rankingTable', {
            templateUrl: '/angular/TeamRating/_rankingTable.html',
            controller: 'TeamRankingsTableController',
            controllerAs: 'rankings'
        })
        .when('/teamRating/teamMatchup', {
            templateUrl: '/angular/TeamRating/_teamMatchup.html',
            controller: 'TeamMatchupController',
            controllerAs: 'teamMatchup'
        })
        .when('/teamRating/winMargin', {
            templateUrl: '/angular/TeamRating/_winMargin.html',
            controller: 'WinMarginController'
        })
        .when('/teamPlayerPerformance', {
            templateUrl: '/angular/TeamPlayerPerformance/_index.html'
        })
        .when('/teamPlayerPerformance/calculator/points', {
            templateUrl: '/angular/TeamPlayerPerformance/_teamPlayerPointPerformance.html',
            controller: 'PlayerPointsPerformanceController'
        })
        .when('/teamPlayerPerformance/calculator/value', {
            templateUrl: '/angular/TeamPlayerPerformance/_teamPlayerValuePerformance.html',
            controller: 'PlayerValuePerformanceController'
        })
       .when('/teamPlayerPerformance/methodology', {
           templateUrl: '/angular/TeamPlayerPerformance/_methodology.html'
        })
        .otherwise({
            redirectTo: '/home'
        });
});
app.config(['usSpinnerConfigProvider', function (usSpinnerConfigProvider) {
    usSpinnerConfigProvider.setDefaults({
        lines: 19, // The number of lines to draw
        length: 20, // The length of each line
        width: 10, // The line thickness
        radius: 30, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#FFF', // #rgb or #rrggbb or array of colors
        speed: 0.5, // Rounds per second
        trail: 60, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: false, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: '50%', // Top position relative to parent
        left: '50%' // Left position relative to parent
    });
}]);
app.run(function (acuteSelectService) {
    // Set the template path for all instances
    acuteSelectService.updateSetting("templatePath", "/Content/acute.select/template");
});