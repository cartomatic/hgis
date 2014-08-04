/**
 * extremaly simplistic front map
 */
Ext.define('hgis.AppLogic', {
	
	requires:[
	
	],
	
	constructor: function(){
		
		Ext.getBody().on("contextmenu", Ext.emptyFn, null, {preventDefault: true});
		
		this.launchApp();
	},
	
	
	launchApp: function(){
	    
        //create the app layout
	    this.buildLayout();

	    this.createMap();
		
	    this.createLayerManager();

	    this.activateLayer('wig25k');

	    this.initPermalink();

	},



	/**
	 * Builds application layout
	 */
	buildLayout: function(){
		
	    this.swipe = Ext.create('Ext.slider.Single', {
	        anchor: '100%',
	        value: 50,
	        minValue: 0,
	        maxValue: 100,
            useTips: false
	    });

	    this.mapToolBar = Ext.create('Ext.toolbar.Toolbar', {
	        dock: 'top',
            layout: 'fit',
	        items: [
                this.swipe
	        ],
            height: 44
	    });

	    //map panel
	    this.mapContainer = Ext.create('Ext.panel.Panel', {
	        region: 'center',
	        layout: 'fit',
	        border: true,
	        margin: '5 5 5 0', //t r b l //5 px margins
	        dockedItems: [
				this.mapToolBar
	        ],
            html: '<div id="mapcnt" style="position: absolute; width: 100%; height: 100%;"></div>'
	    });
	    

	    //west panel - simple layer manager
	    this.westPanel = Ext.create('Ext.panel.Panel', {
	        glyph: 'xf03a@FontAwesome',
	        region: 'west',
	        width: 275,
	        layout: 'fit',
	        border: true,
	        split: true,
	        collapsible: true,
            titleCollapse: true,
	        margin: '5 0 5 5' //t r b l // 5px margins
	    });
        
	    //Application viewport
	    Ext.create('Ext.container.Viewport', {
	        layout: 'border',
	        items: [
        		this.mapContainer,
        		this.westPanel
	        ]
	    });
	},


    /**
     * creates a simple layer manager
     */
	createLayerManager: function(){

	    //create a container for the legend entries
	    var legend = Ext.create('Ext.form.Panel', {
            autoScroll: true
	    });


	    //create a opacity slider
	    this.opacity = Ext.create('Ext.slider.Single', {
	        anchor: '100%',
	        value: 100,
	        minValue: 0,
	        maxValue: 100,
	        useTips: false
	    });

	    this.opacity.on('change', this.onOpacityChange, this);

	    legend.addDocked(
            Ext.create('Ext.toolbar.Toolbar', {
                dock: 'top',
                layout: 'fit',
                items: [
                    this.opacity
                ]
            })
        );


	    //get the map layers
	    var layers = this.map.getLayers();


	    var legendEntries = [];
	    for (var l = 0; l < layers.getLength(); l++) {

            //get layer & properties
	        var lay = layers.item(l);
	        var lProps = lay.getProperties();

	        //ignore layers flagged as base layers
	        if (lProps.is_base_layer) continue;

	        legendEntries.push({
	            name: 'le',
	            inputValue: lay,
	            boxLabel: lProps.ldef.name,
	            lname: lProps.ldef.lname
	        });
	    }

	    //radio group
	    this.rg = Ext.create('Ext.form.RadioGroup', {
	        columns: 1,
	        vertical: false,
	        items: legendEntries
	    });


        //wire up some evt listeners
	    this.rg.on('change', this.onLayerChange, this);


        //add the legend to the layout
	    legend.add(this.rg);

	    this.westPanel.add(legend);

	},

    /**
     * Activates a layer by name if it finds it
     */
	activateLayer: function(l){
	    if (l) {
	        var radios = this.rg.items.items;
	        for (var r = 0; r < radios.length; r++) {
	            if (radios[r].lname == l) {
	                radios[r].setValue(true);
	                break;
	            }
	        }
	    }
	},

	getActiveLayerName: function(){
	    var n = '';
	    if (this.activeLayer) {
	        n = this.activeLayer.getProperties().ldef.lname;
	    }
	    return n;
	},

    /**
     * inits map changes watcher to generate permalink on change
     */
    plinkActive: false,
	initPermalink: function () {

        //apply permalink if any
	    this.applyPermalink();
	    this.map.on('moveend', this.doPermalink, this);
	    this.plinkActive = true;
	},

    /**
     * creates a permalink
     */
	getPermalink: function(){
	    //permalink is created off the zoom, center and layer info as well as the both sliders (swipe and opacity) settings
	    var c = this.map.getView().getCenter();
	    var z = this.map.getView().getZoom();   

	    return [c[0], c[1], z, this.getActiveLayerName(), this.swipe.getValue(), this.opacity.getValue()].join(',');
	},

    /**
     * modifies the url of the client to include permalink
     */
	doPermalink: function () {
	    if (this.plinkActive) {
	        var plink = this.getPermalink();
	        window.location.hash = plink;
	        if (parent) {
	            parent.postMessage(plink, '*');
	        }
	    }
	},

    /**
     * reads the info supplied in the permalink
     */
	applyPermalink: function(){
	    //first get the content of permalink off the url

	    //grab the current hash
	    var hash = window.location.hash.replace('#', '');;

	    //and split it into parts
	    var pldata = hash.split(',');

	    //position
	    if (pldata.length > 1) {
	        var lon = parseFloat(pldata[0]);
	        var lat = parseFloat(pldata[1]);
	        if (!isNaN(lon) && !isNaN(lat)) {
	            this.map.getView().setCenter([lon, lat]);
	        }
	    }

        //zoom
	    if (pldata.length > 2) {
	        var z = parseFloat(pldata[2])
	        if(!isNaN(z)){
	            this.map.getView().setZoom(z);
	        }
	    }

        //active layer
	    if (pldata.length > 3) {
	        this.activateLayer(pldata[3])
	    }

        //swipe
	    if (pldata.length > 4) {
	        var s = parseFloat(pldata[4]);
	        if (!isNaN(s)) {
	            this.swipe.setValue(s)
	        }
	    }

        //opacity
	    if (pldata.length > 5) {
	        var o = parseFloat(pldata[5]);
	        if (!isNaN(o)) {
	            this.opacity.setValue(o)
	        }
	    }
	},

  
    /**
     * opacity slider change callback
     */
	onOpacityChange: function(){
	    if (this.activeLayer) {
	        this.activeLayer.setOpacity(this.opacity.getValue() / 100);
	        this.doPermalink();
	    }
	},

    /**
     * active overlay layer
     */
    activeLayer: null,

    /**
     * layer manager change callback
     */
	onLayerChange: function(rg, newV, oldV, eOpts){
	    
	    if (oldV && oldV.le) {
	        oldV.le.setVisible(false);
	        oldV.le.setOpacity(1);
	    }

	    if (newV && newV.le) {
	        this.activeLayer = newV.le;
	        this.activeLayer.setVisible(true);
	        this.activeLayer.setOpacity(this.opacity.getValue() / 100);
	    }

	    this.doPermalink();

	},

    /**
     * creates a map
     */
	createMap: function () {


	    //spherial mercator
	    var prj = ol.proj.get('EPSG:3857');


	    var prjext = prj.getExtent();

	    var size = ol.extent.getWidth(prjext) / 256;
	    var resolutions = [];
	    for (var z = 0; z < 18; ++z) {
	        // generate resolutions array for this cached WMS
	        resolutions.push(size / Math.pow(2, z));
	    }


        //base layers
	    var osm = new ol.layer.Tile({
	        //just mark the layer as the base layer, so it is possible to distinguish between hgis layers and the layers that are meant to act as base layers
            //ol3 does not really need a base layer
	        is_base_layer: true,
	        source: new ol.source.OSM()
	    });


	    //hgis overlays
	    var ldefs = [
            { name: 'WIG Mapa Polski i Krajów Ościennych 1:500 000', lname: 'wig500k' },
            { name: 'WIG Mapa operacyjna 1:300 000', lname: 'wig300k' },
            { name: 'WIG Polska mapa taktyczna 1:100 000', lname: 'wig100k' },
            { name: 'WIG Mapa szczegółowa 1:25 000', lname: 'wig25k' },
            { name: 'Meßtischblätter 1:25 000', lname: 'm25k' },
            { name: 'Übersichtskarte von Mitteleuropa 1:300 000', lname: 'ukvme' },
            { name: 'Karte des Deutschen Reiches 1:100 000', lname: 'kdr' },
            { name: 'Karte des westlichen Rußlands 1:100 000', lname: 'kdwr' },
            { name: 'Grossblatt 1:100 000: Karte des Deutschen Reiches, Karte des westlichen Rußlands', lname: 'kdr_gb' }
	    ];

	    //create hgis layers
	    var hgisLayers = [];
	    for (var ln = 0; ln < ldefs.length; ln++) {
	        
            //create a layer
	        var hgisl = new ol.layer.Tile({
	            ldef: ldefs[ln],
	            extent: prjext,
	            source: new ol.source.TileWMS({
	                urls: this.getLayerUrls(ldefs[ln].lname),
	                params: {
	                    'LAYERS': ldefs[ln].lname
                        //'FORMAT': 'image/jpeg'
	                },
	                projection: prj,
	                tileGrid: new ol.tilegrid.TileGrid({
	                    origin: ol.extent.getTopLeft(prjext),
	                    resolutions: resolutions
	                }),
	                attributions: [
                        new ol.Attribution({
                            html: 'HGIS Tiles <a href="http://hgis.cartomatic.pl/">hgis.cartomatic.pl</a>'
                        })
	                ]
	            }),
	            visible: false //initially make all the layers invisible
	        });
	        
            //wire evt listenters, so it is possible to use a 'swipe'
	        var me = this;

	        hgisl.on('precompose', function (event) {
	            var ctx = event.context;
	            var width = ctx.canvas.width * (me.swipe.getValue() / 100);

	            ctx.save();
	            ctx.beginPath();
	            ctx.rect(width, 0, ctx.canvas.width - width, ctx.canvas.height);
	            ctx.clip();
	        });

	        hgisl.on('postcompose', function (event) {
	            var ctx = event.context;
	            ctx.restore();
	        });

            //add the created layer to the collection
	        hgisLayers.push(
                hgisl
            )
	    }

	    //glue in token so the watermark is not applied
	    if (typeof (__token__) != 'undefined') {
	        for (var l = 0; l < hgisLayers.length; l++) {
	            hgisLayers[l].getSource().getParams()[__token__.param] = __token__.value;
	        }
	    }

        //start the map with the osm base layer
	    this.map = new ol.Map({
	        layers: [osm],
	        target: 'mapcnt',
	        controls: ol.control.defaults({
	            attributionOptions: {
	                collapsible: false
	            }
	        }).extend([
                new ol.control.ScaleLine(),
                new ol.control.MousePosition({
                    projection: ol.proj.get('EPSG:4326'),
                    coordinateFormat: function (coords) {
                        var output = '';
                        if (coords) {
                            output = coords[0].toFixed(5) + ' : ' + coords[1].toFixed(5);
                        }
                        return output;
                    }
                })
	        ]),
	        view: new ol.View({
	            center: [2340195, 6837328],
	            zoom: 15
	        }),
	        logo: {
	            src: 'resx/favicon/favicon.png',
                href: 'http://hgis.cartomatic.pl'
	        }
	    });

	    //add the overlays
	    for (var l = 0; l < hgisLayers.length; l++) {
	        this.map.addLayer(hgisLayers[l]);
	    }


        //wire up the swipe on change
	    this.swipe.on(
            'change',
            function () {
                this.map.render();
                this.doPermalink();
            },
            this
        );

	    this.mapContainer.on(
            'resize',
            function () {
                this.map.updateSize();
            },
            this
        );
	},

	getLayerUrls: function (type) {
	    return [
            'http://wms.hgis.cartomatic.pl/topo/3857/' + type,
            'http://wms1.hgis.cartomatic.pl/topo/3857/' + type,
            'http://wms2.hgis.cartomatic.pl/topo/3857/' + type,
            'http://wms3.hgis.cartomatic.pl/topo/3857/' + type,
            'http://wms4.hgis.cartomatic.pl/topo/3857/' + type
	    ];
	}
});