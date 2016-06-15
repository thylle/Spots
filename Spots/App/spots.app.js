(function () {
    'use strict';

    angular.module('spots', ['angular-loading-bar', 'iso.directives'])
    .config(['cfpLoadingBarProvider', function (cfpLoadingBarProvider) {
        cfpLoadingBarProvider.includeSpinner = false;
    }]);
})();