(function () {
    'use strict';

    angular
        .module('spots')
        .service('spotsService', initService);

    initService.$inject = ['$http'];

    function initService($http) {
        this.getData = getData;

        function getData(articleType, limit, page) {
            var article_type = articleType;
            var limit = limit;
            var page = page;
            var url = "/Umbraco/Api/Spots/GetAllSpots";
            //var url = "/Api/Stream/?article_type=" + article_type + "&limit=" + limit + "&page=" + page;

            return $http.get(url);
        }
    }
})();