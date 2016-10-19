(function () {
    'use strict';

    initController.$inject = ['$scope', '$timeout', 'spotsService'];

    angular
        .module('spots')
        .controller('mainController', initController);

    function initController($scope, $timeout, spotsService) {

        $scope.$timeout = $timeout;
        $scope.spots = null;
        $scope.activeId = null;


        spotsService.getData().success(function (response) {
            console.log("success", response);
            $scope.spots = response;

        }).error(function () {
            console.log("error getting spots");
        });;


        $scope.setActiveId = function(id) {

            if (id != $scope.activeId) {
                $scope.activeId = id;
            } else {
                $scope.activeId = null;
            }
        }

    }
})();
