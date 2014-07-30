(function () {
	"use strict;"

	var app = angular.module('goToWindowSiteApp', ["hc.marked"]);

	app.config(['markedProvider', function (markedProvider) {
		markedProvider.setOptions({ gfm: true });
	}]);

})();
