<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TokenTest.aspx.cs" Inherits="TokenTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TokenMaster Test</title>
    <link rel="stylesheet" href="http://localhost/jslibs/OpenLayers/2.12/theme/default/style.css" type="text/css" />
    <script src="http://localhost/jslibs/OpenLayers/2.12/lib/OpenLayers.js"></script>
    <script type="text/javascript">
        var map;
        function init() {
            map = new OpenLayers.Map(
                'map',
                {
                    projection: new OpenLayers.Projection("EPSG:3857"),
                    units: "m",
                    maxResolution: 156543.0339,
                    maxExtent: new OpenLayers.Bounds(-20037508.34, -20037508.34, 20037508.34, 20037508.34),
                    //layer extent read from capabilities
                    restrictedExtent: new OpenLayers.Bounds(-649300.94999264134, 5982833.9021321312, 2597078.7110703941, 7558100.0422791662)
                }
            );

            //token
            var token = '';
            if (typeof (__token__) != 'undefined') {
                token = __token__;
            }

            map.addLayer(
                new OpenLayers.Layer.WMS(
                    "wig500k",
                    [
                        //"http://wms1.hgis.cartomatic.pl/topo/?source=3857",
                        //"http://wms2.hgis.cartomatic.pl/topo/?source=3857",
                        //"http://wms3.hgis.cartomatic.pl/topo/?source=3857",
                        //"http://wms4.hgis.cartomatic.pl/topo/?source=3857",
                        //"http://wms5.hgis.cartomatic.pl/topo/?source=3857",
                        //"http://wms6.hgis.cartomatic.pl/topo/?source=3857",
                        //"http://wms7.hgis.cartomatic.pl/topo/?source=3857",
                        //"http://wms8.hgis.cartomatic.pl/topo/?source=3857",
                        "http://localhost/hgis_wms/gdal.ashx"
                        //"http://geoserver.hgis.cartomatic.pl/topo3857/wms"
                    ],
                    {
                        layers: 'wig500k_google',
                        FORMAT: 'image/jpeg',
                        VERSION: '1.3.0',
                        t: token
                    },
                    {
                        attribution: 'hgis.cartomatic.pl'
                    }
                )
            );

            map.addControl(new OpenLayers.Control.Attribution());

            map.zoomToExtent(map.restrictedExtent);
        }
    </script>
</head>
<body onload="init()">
    <div style="position: absolute; width: 100%; height: 100%;" id="map">
    
    </div>
</body>
</html>
