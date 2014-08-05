/**
 * Ext.Loader settings.
 * In order to make the build app work, make sure it is properly included before including the 
 * actual app scripts!!!
 * 
 * This makes it possible to set all the references and dynamic variables properly even for compiled scripts
 * that are likely to have a bit different loading order than when using a dynamic loader
 */
 
Ext.Loader.setConfig({
	enabled:true
});

//hgis
Ext.Loader.setPath('hgis', 'apps/hgis');
