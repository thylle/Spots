(function () {
    'use strict';

    initController.$inject = ['$scope', '$timeout', 'spotsService', 'cfpLoadingBar'];

    angular
        .module('spots')
        .controller('mainController', initController);

    function initController($scope, $timeout, spotsService, cfpLoadingBar) {

        $scope.$timeout = $timeout;
        $scope.spots = null;
        $scope.activeId = null;

        //fake the loading time - to make time to show animation and show elements more fluid
        cfpLoadingBar.start();
        //cfpLoadingBar.complete();

        spotsService.getData().success(function (response) {
            console.log("success", response);
            $scope.spots = response;

        }).error(function () {
            console.log("error", response);
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
