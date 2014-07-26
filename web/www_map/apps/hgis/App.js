Ext.require([
	'hgis.AppLogic'
]);

Ext.application({
	name: 'HGIS', //Note: this MUST NOT be the same as the app's namespace!
	launch: function(){
		Ext.QuickTips.init();
		 //setTimeout( //so the splash screen hangs there a bit longer
		 //    function(){
		 //   	 splashScreen.hide();
		 //   	 Ext.create('hgis.AppLogic');
		 //    },
		 //    1000
		 //);
		splashScreen.hide();
		Ext.create('hgis.AppLogic');
	}
});