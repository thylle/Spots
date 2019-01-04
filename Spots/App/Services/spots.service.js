(function () {
    'use strict';

    angular
        .module('spots')
        .service('spotsService', initService);

    initService.$inject = ['$http'];

    function initService($http) {
        this.getData = getData;
        this.getSpotById = getSpotById;

        function getData() {
            var url = "/Umbraco/Api/Spots/GetAllSpots?lat=55.859792&lon=9.848656499999999";

            return $http.get(url);
        }

        function getSpotById(id) {
            var url = "/Umbraco/Api/Spots/GetSpotById?spotId=" + id;

            return $http.get(url);
        }
    }
})();