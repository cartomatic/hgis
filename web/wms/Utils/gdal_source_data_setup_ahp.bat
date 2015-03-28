echo off
rem ********************
rem a simple batch script that creates a proper data structure for the hgis wms
rem it assumes the source ant target files being ordered as below
rem Settings.WmsDataFolder
rem		\type
rem			\epsg
rem				\source
rem ********************
rem set variables
set source={h:\ere\goes\a\source\dir\}
set target={h:\ere\goes\a\target\dir\}
set ext={either ecw or jp2}
rem setup destination dirs
mkdir %target%2180
mkdir %target%3857
mkdir %target%4326
rem ahp_xvi
echo creating links for ahp_xvi
mklink /H %target%2180\ahp_xvi.%ext% %source%xvi\ahp_xvi_2180_transparent.%ext%
mklink /H %target%3857\ahp_xvi.%ext% %source%xvi\ahp_xvi_3857_transparent.%ext%
mklink /H %target%4326\ahp_xvi.%ext% %source%xvi\ahp_xvi_4326_transparent.%ext%


pause