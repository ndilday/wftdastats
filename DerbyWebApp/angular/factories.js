var app = angular.module('WftdaStatsApp.Factories', []);
app.factory('TeamRatings', function TeamRatings($http, $q) {
    return {
        internalData: '',

        get: function () {
            // Create the deffered object
            var deferred = $q.defer();

            if (!this.internalData) {
                // Request has not been made, so make it
                $http.get('/api/teamRatings').then(function (resp) {
                    var length = resp.data.length;
                    for (i = 0; i < length; i++) {
                        var jsDate = new Date(resp.data[i].AddedDate);
                        resp.data[i].AddedDate = (jsDate.getMonth() + 1) + "/" + jsDate.getDate() + "/" + jsDate.getFullYear();
                    }
                    deferred.resolve(resp.data);
                });
                // Add the promise to myObject
                this.internalData = deferred.promise;
            }
            // Return the myObject stored on the service
            return this.internalData;

        }

    };
});
app.factory('TeamPlayerPointPerformance', function TeamPlayerPerformance($http, $q) {
    return {
        get: function (teamID) {
            // Create the deffered object
            var deferred = $q.defer();

            // Request has not been made, so make it
            $http.get('/api/teamPlayerPerformance/points/' + teamID).then(function (resp) {
                deferred.resolve(resp.data);
            });
            // Return the myObject stored on the service
            return deferred.promise;

        }

    };
});
app.factory('TeamPlayerValuePerformance', function TeamPlayerPerformance($http, $q) {
    return {
        get: function (teamID) {
            // Create the deffered object
            var deferred = $q.defer();

            // Request has not been made, so make it
            $http.get('/api/teamPlayerPerformance/value/' + teamID).then(function (resp) {
                deferred.resolve(resp.data);
            });
            // Return the myObject stored on the service
            return deferred.promise;

        }

    };
});
app.factory('TeamData', function TeamData($http, $q) {
    return {
        internalData: '',

        get: function () {
            // Create the deffered object
            var deferred = $q.defer();

            if (!this.internalData) {
                // Request has not been made, so make it
                $http.get('/api/teams').then(function (resp) {
                    deferred.resolve(resp.data);
                });
                // Add the promise to myObject
                this.internalData = deferred.promise;
            }
            // Return the myObject stored on the service
            return this.internalData;

        }

    };
});