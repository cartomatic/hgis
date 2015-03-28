/**
 * extremaly simplistic front map
 */
Ext.define('hgis.AppLogic', {
    
    requires:[
       'hgis.Permalink'
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

        var me = this;
        
        //instantiate and activate permalink module
        this.permalink = Ext.create('hgis.Permalink',{
            map: this.map,
            pLinkGetters: [
                //active layer
                function(){
                    return me.getActiveLayerName();
                },
                //swipe
                function(){
                    return me.swipe.getValue();
                },
                //opacity
                function(){
                    return me.opacity.getValue();
                }
            ],
            pLinkSetters: [
                function(v){
                    me.activateLayer(v);
                },
                function(v){
                    v = parseFloat(v);
                    if (!isNaN(v)) {
                        me.swipe.setValue(v);
                    }
                },
                function(v){
                    v = parseFloat(v);
                    if (!isNaN(v)) {
                        me.opacity.setValue(v);
                    }
                }
            ]
        });
        this.permalink.activate();
    },

    /**
     * triggers permalink regeneration
     */
    updatePermalink: function(){
        if(this.permalink) this.permalink.update();
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
            margin: '5 0 5 5', //t r b l // 5px margins
            dockedItems: [
	            //{
	            //    xtype: 'toolbar',
                //    dock: 'bottom',
                //    items: [
                //        {
                //            glyph: 'xf019@FontAwesome',
                //            text: 'Export PNG',
                //            listeners: {
                //               click: Ext.bind(this.exportMap, this)
                //            }
                //        }
                //    ]
	            //}
            ]
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

    exportMap: function () {

        var a = document.getElementById('link_downloader');
        var that = this;

        if (!a.evtWiredUp) {
            console.warn('wiring up click evt');
            a.addEventListener(
                'click',
                function (e) {
                    //register a postcompose evt listener that will be executed once only
                    that.map.once(
                        'postcompose',
                        function (event) {
                            var canvas = event.context.canvas;
                            var dataUrl = canvas.toDataURL('image/png');

                            a.href = dataUrl;
                        }
                    );
                    that.map.renderSync();
                },
                false
            );

            a.evtWiredUp = true;
        }


        //trigger a click on a link element
        a.click();

        //Note:
        //It seems to not have an impact on the behavior to set the a href in a click callback or just prior to triggering a click
        //
        //the problem though is that when using larger displays (say full hd or larger) chrome continously crashes
        //
        //ie does not seem to like click()
        //
        //safari also ignores click()
        //
        //opera crashes
        //
        //So in general a link styled like a button will be a better option as it will work everywhere in terms of receiving clicks
        //chrome will still go down, opera not tested.
    },

    /**
     * creates a simple layer manager
     */
    createLayerManager: function(){

        this.panelTopo = Ext.create('Ext.form.Panel', {
            title: 'Mapy topograficzne',
            autoScroll: true
        });
        this.panelCity = Ext.create('Ext.form.Panel', {
            title: 'Plany miast',
            autoScroll: true
        });
        this.panelAhp = Ext.create('Ext.form.Panel', {
            title: 'Atlas Historyczny Polski',
            autoScroll: true
        });

        //create a container for the legend entries
        this.legend = Ext.create('Ext.form.Panel', {
            layout: {
                // layout-specific configs go here
                type: 'accordion',
                titleCollapse: true,
                animate: true,
                activeOnTop: true
            },
            items: [
                this.panelTopo,
                this.panelCity,
                this.panelAhp
            ]
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

        this.legend.addDocked(
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


        var legendEntriesTopo = [];
        var legendEntriesCity = [];
        var legendEntriesAhp = [];

        for (var l = 0; l < layers.getLength(); l++) {

            //get layer & properties
            var lay = layers.item(l);
            var lProps = lay.getProperties();

            //ignore layers flagged as base layers
            if (lProps.is_base_layer) continue;

            var le = {
                name: 'le',
                inputValue: lay,
                boxLabel: lProps.ldef.name,
                lname: lProps.ldef.lname
            };

            switch(lProps.ldef.type){
                case 'topo':
                    legendEntriesTopo.push(le);
                    break;

                case 'city':
                    legendEntriesCity.push(le);
                    break;

                case 'ahp':
                    legendEntriesAhp.push(le);
                    break;
            }
        }

        //radio groups
        this.rgTopo = Ext.create('Ext.form.RadioGroup', {
            columns: 1,
            vertical: false,
            items: legendEntriesTopo,
            listeners: {
                change: Ext.bind(this.onLayerChange, this)
            }
        });
        this.rgCity = Ext.create('Ext.form.RadioGroup', {
            columns: 1,
            vertical: false,
            items: legendEntriesCity,
            listeners: {
                change: Ext.bind(this.onLayerChange, this)
            }
        });
        this.rgAhp = Ext.create('Ext.form.RadioGroup', {
            columns: 1,
            vertical: false,
            items: legendEntriesAhp,
            listeners: {
                change: Ext.bind(this.onLayerChange, this)
            }
        });


        //add the legend to the layout
        this.panelTopo.add(this.rgTopo);
        this.panelCity.add(this.rgCity);
        this.panelAhp.add(this.rgAhp);

        this.westPanel.add(this.legend);
    },

    /**
     * Activates a layer by name if it finds it
     */
    activateLayer: function(l){
        if (l) {
            var radiosTopo = this.rgTopo.items.items,
                radiosCity = this.rgCity.items.items,
                radiosAhp = this.rgAhp.items.items,
                activeItem;

            for (var r = 0; r < radiosTopo.length; r++) {
                radiosTopo[r].setValue(radiosTopo[r].lname == l);
                if(radiosTopo[r].lname == l){
                    activeItem = this.panelTopo;
                }
            }

            for (var r = 0; r < radiosCity.length; r++) {
                radiosCity[r].setValue(radiosCity[r].lname == l);
                if(radiosCity[r].lname == l){
                    activeItem = this.panelCity;
                }
            }

            for (var r = 0; r < radiosAhp.length; r++) {
                radiosAhp[r].setValue(radiosAhp[r].lname == l);
                if(radiosAhp[r].lname == l){
                    activeItem = this.panelAhp;
                }
            }

            activeItem.expand();
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
     * opacity slider change callback
     */
    onOpacityChange: function(){
        if (this.activeLayer) {
            this.activeLayer.setOpacity(this.opacity.getValue() / 100);
            this.updatePermalink();
        }
    },

    /**
     * active overlay layer
     */
    activeLayer: null,

    activeRg: null,

    /**
     * layer manager change callback
     */
    onLayerChange: function(rg, newV, oldV, eOpts){

        if (this.activeLayer) {
            this.activeLayer.setVisible(false);
            this.activeLayer.setOpacity(1);

            //deactivate previously active rg if different than the incoming
            if(activeRg !== rg){
                for(var r = 0; r < activeRg.items.items.length; r++){
                    activeRg.items.items[r].setValue(false);
                }
            }
        }

        if (newV && newV.le) {
            this.activeLayer = newV.le;
            this.activeLayer.setVisible(true);
            this.activeLayer.setOpacity(this.opacity.getValue() / 100);

            activeRg = rg;

            //trigger adjustment of the layer panel
            //this.activateLayer(this.activeLayer.getProperties().ldef.lname);
        }

        this.updatePermalink();

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
            //Topo
            { name: 'WIG Mapa Polski i Krajów Ościennych 1:500 000', lname: 'wig500k', type: 'topo' },
            { name: 'WIG Mapa operacyjna 1:300 000', lname: 'wig300k', type: 'topo' },
            { name: 'WIG Polska mapa taktyczna 1:100 000', lname: 'wig100k', type: 'topo' },
            { name: 'WIG Mapa szczegółowa 1:25 000', lname: 'wig25k', type: 'topo' },
            { name: 'Meßtischblätter 1:25 000', lname: 'm25k' },
            { name: 'Übersichtskarte von Mitteleuropa 1:300 000', lname: 'ukvme', type: 'topo' },
            { name: 'Karte des Deutschen Reiches 1:100 000', lname: 'kdr', type: 'topo' },
            { name: 'Karte des westlichen Rußlands 1:100 000', lname: 'kdwr', type: 'topo' },
            { name: 'Grossblatt 1:100 000: Karte des Deutschen Reiches, Karte des westlichen Rußlands', lname: 'kdr_gb', type: 'topo' },
            { name: 'Administrativ karte von den königreichen galizien und lodomerien', lname: 'kummersberg', type: 'topo' },

            //City
            { name: 'Białystok (G.S.G.S. 4435)', lname: 'bialystok_gsgs4435', type: 'city' },
            { name: 'Bydgoszcz (G.S.G.S. 4435)', lname: 'bydgoszcz_gsgs4435', type: 'city' },
            { name: 'Breslau (G.S.G.S. 4480)', lname: 'breslau_gsgs4480', type: 'city' },
            { name: 'Częstochowa (G.S.G.S. 4435)', lname: 'czestochowa_gsgs4435', type: 'city' },
            { name: 'Danzig (G.S.G.S. 4496) - DANZIG (NORTH), DANZIG (SOUTH)', lname: 'danzig_gsgs4496', type: 'city' },
            { name: 'Grudziądz (G.S.G.S. 4435)', lname: 'grudziac_gsgs4435', type: 'city' },
            { name: 'Inowrocław (G.S.G.S. 4435)', lname: 'inowroclaw_gsgs4435', type: 'city' },
            { name: 'Jasło (G.S.G.S. 4435)', lname: 'jaslo_gsgs4435', type: 'city' },
            { name: 'Katowice (G.S.G.S. 4435)', lname: 'katowice_gsgs4435', type: 'city' },
            { name: 'Kielce (G.S.G.S. 4435)', lname: 'kielce_gsgs4435', type: 'city' },
            { name: 'Kraków (G.S.G.S. 4435)', lname: 'krakow_gsgs4435', type: 'city' },
            { name: 'Łódź (G.S.G.S. 4435)', lname: 'lodz_gsgs4435', type: 'city' },
            { name: 'Lublin (G.S.G.S. 4435)', lname: 'lublin_gsgs4435', type: 'city' },
            { name: 'Poznań (G.S.G.S. 4435)', lname: 'poznan_gsgs4435', type: 'city' },
            { name: 'Radom (G.S.G.S. 4435)', lname: 'radom_gsgs4435', type: 'city' },
            { name: 'Rzeszów (G.S.G.S. 4435)', lname: 'rzeszow_gsgs4435', type: 'city' },
            { name: 'Siedlce (G.S.G.S. 4435)', lname: 'siedlce_gsgs4435', type: 'city' },
            { name: 'Tarnów (G.S.G.S. 4435)', lname: 'tarnow_gsgs4435', type: 'city' },
            { name: 'Toruń (G.S.G.S. 4435)', lname: 'torun_gsgs4435', type: 'city' },
            { name: 'Warszawa (G.S.G.S. 4435)', lname: 'warszawa_gsgs4435', type: 'city' },

            //Ahp
            { name: 'Atlas historyczny Polski - Mapy szczegółowe XVI wieku', lname: 'ahp_xvi', type: 'ahp' }
        ];

        //create hgis layers
        var hgisLayers = [];
        for (var ln = 0; ln < ldefs.length; ln++) {
            
            //create a layer
            var hgisl = new ol.layer.Tile({
                ldef: ldefs[ln],
                extent: prjext,
                source: new ol.source.TileWMS({
                    crossOrigin: 'anonymous', //enable cors, so can dump canvas data later
                    urls: this.getLayerUrls(ldefs[ln].type, ldefs[ln].lname),
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
                this.updatePermalink();
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


    getLayerUrls: function (type, source) {
        return [
            'http://wms.hgis.cartomatic.pl/' + type +'/3857/' + source,
            'http://wms1.hgis.cartomatic.pl/' + type +'/3857/' + source,
            'http://wms2.hgis.cartomatic.pl/' + type +'/3857/' + source,
            'http://wms3.hgis.cartomatic.pl/' + type +'/3857/' + source,
            'http://wms4.hgis.cartomatic.pl/' + type +'/3857/' + source
        ];
    }
});