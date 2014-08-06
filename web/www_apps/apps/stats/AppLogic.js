/**
 * extremaly simplistic front map
 */
Ext.define('stats.AppLogic', {
    
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
        
    },


    /**
     * Builds application layout
     */
    buildLayout: function(){
        
        //toolbar
        this.switcher  = Ext.create('Ext.button.Segmented', {
            defaults: {
                listeners: {
                    click: Ext.bind(this.onRangeChange, this)
                }
            },
            items: [
                {
                    text: 'Total',
                    itemId: 'total',
                    pressed: true
                },
                {
                    text: 'This year',
                    itemId: 'year'
                },
                {
                    text: 'This month',
                    itemId: 'month'
                },
                {
                    text: 'This week',
                    itemId: 'week'
                },
                {
                    text: 'Today',
                    itemId: 'day'
                }
            ]
        });
        
        //map panel
        this.mapPanel = Ext.create('Ext.panel.Panel', {
            title: 'By Location',
            glyph: 'xf041@FontAwesome',
            layout: 'fit',
            itemId: 'map',
            html: '<div id="mapcnt" style="position: absolute; width: 100%; height: 100%;"></div>'
        });
        
        this.referrerModel = Ext.define('hgis.ReferrerStats', {
            extend: 'Ext.data.Model', 
            fields: [
                {name: 'Referrer', type: 'string', defaultValue: null, useNull: true},
                {name: 'Bytes', type: 'number', defaultValue: null, useNull: true, convert: function(value, rec) {return (value ? (value / 1073741824) : 0);}},
                {name: 'Hits', type: 'int', defaultValue: 0, useNull: false}
            ]   
        })

        //grid panel
        this.gridPanel = Ext.create('Ext.grid.Panel', {
            title: 'By Referrer',
            glyph: 'xf0c1@FontAwesome',
            itemId: 'grid',
            columns: [
                {header: 'Referrer', flex: 1, dataIndex: 'Referrer'},
                {header: 'GBytes', flex: 1, dataIndex: 'Bytes'},
                {header: 'Hits', flex: 1, dataIndex: 'Hits'}
            ],
            store: Ext.create('Ext.data.Store', {
                model: this.referrerModel,
                data: []
            })
        });
        
        //Application viewport
        this.viewport = Ext.create('Ext.container.Viewport', {
            layout: 'fit',
            items: [
                Ext.create('Ext.tab.Panel', {
                    title: 'HGIS WMS Stats',
                    glyph: 'xf0ce@FontAwesome',
                    tabPosition: 'left',
                    dockedItems: [
                        {
                            xtype: 'toolbar',
                            dock: 'top',
                            height: 44,
                            items: [
                                this.switcher
                            ]
                        }
                    ],
                    items: [
                        this.mapPanel,
                        this.gridPanel
                    ],
                    listeners: {
                        tabchange: Ext.bind(this.onTabChange, this)
                    }
                })
            ]
        });
        
        //update map size on viewport resize
        this.viewport.on('resize', function(){this.map.updateSize();},this);
        
        this.mode = 'map';
        this.range = 'total';
        
        this.getData();
    },

    onTabChange: function(tabpanel, newC,oldC, eOpts){
        this.mode = newC.getItemId();
        this.getData();
    },
    
    onRangeChange: function(btn, e, eOpts){
        this.range = btn.getItemId();
        this.getData();
    },
    
    
    getData: function(){
        //show load mask
        this.viewport.mask('Loading...');
        
        //and pull the data
        Ext.Ajax.request({
            url: 'webservices/stats.asmx/GetWmsStats',
            method: 'POST',
            headers: {
               'Content-Type': 'application/json; charset=utf-8'
               },
               params: Ext.JSON.encode({
                   t: typeof(__token__) != undefined ? __token__.value : null,
                   mode: this.mode,
                   range: this.range
               }),
            success: Ext.bind(this.getDataCallbackSuccess, this),
            failure: Ext.bind(this.getDataCallbackFailure, this)
        });
    },
    
    getDataCallbackSuccess: function(response){
        
        //this is an asp.net asmx response
        response = Ext.JSON.decode(response.responseText).d;

        //check if this was an async ws
        //if so, the actual response data will be under a Result property
        if(response.Result){
            response = response.Result;
        }
        
        if(response.success){
            if(this.mode == 'map'){
                this.loadMapData(response.data);
            }
            else {
                this.loadGridData(response.data);
            }
            this.viewport.unmask();
        }
        else {
            this.viewport.unmask();
            this.giveFailureMsg(response.exception);
        }
    },
    
    getDataCallbackFailure: function(){
        this.viewport.unmask();
        this.giveFailureMsg('Could not connect to the stats server');
    },
    
    giveFailureMsg: function(msg, callback){
        Ext.Msg.show({
            title: 'Error',
            msg: msg,
            icon: Ext.Msg.WARNING,
            buttons: Ext.isFunction(callback) ? Ext.Msg.OKCANCEL : Ext.Msg.OK,
            fn: Ext.isFunction(callback) ? callback : null
        });
    },
    
    loadGridData: function(data){
        this.gridPanel.getStore().loadRawData(data);
    },
    
    loadMapData: function(data){
        
        //first check the vector layer
        if(!this.vLayer){
            this.vLayer = new ol.layer.Vector({
                source: new ol.source.Vector({
                    projection: ol.proj.get('EPSG:3857')
                })
                
                //Note: so far no dynamic styles - for each feature a separate style needs to be created
                //so styles get assigned to a feature rather than layer at this stage
//                style: function(feature, resolution) {
//				    var text = resolution < 5000 ? feature.get('name') : '';
//				    if (!styleCache[text]) {
//				      styleCache[text] = [new ol.style.Style({
//				        fill: new ol.style.Fill({
//				          color: 'rgba(255, 255, 255, 0.6)'
//				        }),
//				        stroke: new ol.style.Stroke({
//				          color: '#319FD3',
//				          width: 1
//				        }),
//				        text: new ol.style.Text({
//				          font: '12px Calibri,sans-serif',
//				          text: text,
//				          fill: new ol.style.Fill({
//				            color: '#000'
//				          }),
//				          stroke: new ol.style.Stroke({
//				            color: '#fff',
//				            width: 3
//				          })
//				        })
//				      })];
//				    }
//				    return styleCache[text];
//				  }

                
                //style the circles depending on the incoming data
//                style: (
//                    function(){
//                        
//                        var getTxt = function(f){
//                            
//                        }
//                        
//                        var getSize = function(f){
//                            
//                        }
//                        
//                        return function(feature, resolution){
//                            
//                        }
//                    }()
//                )
//                
//                
//                /*new ol.style.Style({
//                    image: new ol.style.Circle({
//			          radius: 10,
//			          stroke: new ol.style.Stroke({
//			            color: '#fff'
//			          }),
//			          fill: new ol.style.Fill({
//			            color: '#3399CC'
//			          })
//			        }),
//                    text: new ol.style.Text({
//                        text: 'dupa',
//                        stroke: new ol.style.Stroke({
//                        color: '#fff'
//                      }),
//                      fill: new ol.style.Fill({
//                        color: '#3399CC'
//                      })
//                    })
//                })*/
//               
            });
            
            this.map.addLayer(this.vLayer);
        }
        
        var src = this.vLayer.getSource(),
            tsrc = ol.proj.get('EPSG:4326'),
            tdest = ol.proj.get('EPSG:3857');
        
        //wipe out the layer
        src.clear();
        
        //
        
        
        var d = 0,
            dlen = data.length,
            features = [],
            maxSize = dlen > 0 ? data[0].Bytes : 0
            maxCircleSize = 70; //radius of 70px
        
        var getTopText = function(o){
            var txt = o.Ip;
            
            if(o.City){
                txt = o.City + '(' + txt + ')';
            }
             
            return txt;
        }
        
        var getBottomText = function(o){
            return 'Hits:' + o.Hits + '; Gb:' + (o.Bytes / 1073741824).toFixed(5);
        }
        
        var getRadius = function(o){
            return (o.Bytes / maxSize) * maxCircleSize;
        }
        
        for(d; d < dlen; d++){
            if(!data[d].Lon) continue; //ignore recs withou geo
            
            var f = new ol.Feature({
                geometry: new ol.geom.Point([data[d].Lon, data[d].Lat]).transform(tsrc, tdest)
                //assigning style here so far does not work!
                //not exported api property or something, so setting the style through setStyle
                //on a feature object
            });
            f.setStyle([
                new ol.style.Style({
					image: new ol.style.Circle({
						radius: getRadius(data[d]),
						stroke: new ol.style.Stroke({
	                        color: '#fff'
						}),
						fill: new ol.style.Fill({
						    color: 'rgba(51,152,153,0.5)'
					    })
					}),
					text: new ol.style.Text({
					   text: getTopText(data[d]),
                       font: 'bold 14px Arial',
                       offsetY: -7,
					   stroke: new ol.style.Stroke({
					       color: '#fff'
					   }),
					   fill: new ol.style.Fill({
					       color: 'rgb(153,51,102)'
					   })
					})
				}),
                new ol.style.Style({
                    text: new ol.style.Text({
                       text: getBottomText(data[d]),
                       font: 'bold 12px Arial',
                       textAlign: 'center',
                       textBaseline: 'middle',
                       offsetY: 6,
                       stroke: new ol.style.Stroke({
                           color: '#fff'
                       }),
                       fill: new ol.style.Fill({
                           color: '#000'
                       })
                    })
                })
            ]);
            features.push(f);
        }
        src.addFeatures(features);
        
        //zoom to layer data extent
        this.map.getView().fitExtent(
            ol.extent.buffer(src.getExtent(), 500),
            this.map.getSize()
        );
        
    },
    
    /**
     * creates a map
     */
    createMap: function () {

        //base layer
        var osm = new ol.layer.Tile({
            source: new ol.source.OSM()
        });

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
                zoom: 6
            }),
            logo: {
                src: 'resx/favicon/favicon.png',
                href: 'http://hgis.cartomatic.pl'
            }
        });
    }
});