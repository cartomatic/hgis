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
rem wig500k
echo creating links for wig500k
mklink /H %target%2180\wig500k.%ext% %source%wig500k\mosaick\wig500k_2180_transparent.%ext%
mklink /H %target%3857\wig500k.%ext% %source%wig500k\mosaick\wig500k_3857_transparent.%ext%
mklink /H %target%4326\wig500k.%ext% %source%wig500k\mosaick\wig500k_4326_transparent.%ext%
rem wig300k
echo creating links for wig300k
mklink /H %target%2180\wig300k.%ext% %source%wig300k\mosaick\wig300k_2180_transparent.%ext%
mklink /H %target%3857\wig300k.%ext% %source%wig300k\mosaick\wig300k_3857_transparent.%ext%
mklink /H %target%4326\wig300k.%ext% %source%wig300k\mosaick\wig300k_4326_transparent.%ext%
rem wig100k
echo creating links for wig100k
mklink /H %target%2180\wig100k.%ext% %source%wig100k\mosaick\wig100k_2180_transparent.%ext%
mklink /H %target%3857\wig100k.%ext% %source%wig100k\mosaick\wig100k_3857_transparent.%ext%
mklink /H %target%4326\wig100k.%ext% %source%wig100k\mosaick\wig100k_4326_transparent.%ext%
rem wig25k
echo creating links for wig25k
mklink /H %target%2180\wig25k.%ext% %source%wig25k\mosaick\wig25k_2180_transparent.%ext%
mklink /H %target%3857\wig25k.%ext% %source%wig25k\mosaick\wig25k_3857_transparent.%ext%
mklink /H %target%4326\wig25k.%ext% %source%wig25k\mosaick\wig25k_4326_transparent.%ext%
rem ukvme
echo creating links for ukvme
mklink /H %target%2180\ukvme.%ext% %source%ukvme\mosaick\ukvme_2180_transparent.%ext%
mklink /H %target%3857\ukvme.%ext% %source%ukvme\mosaick\ukvme_3857_transparent.%ext%
mklink /H %target%4326\ukvme.%ext% %source%ukvme\mosaick\ukvme_4326_transparent.%ext%
rem m25k
echo creating links for m25k
mklink /H %target%2180\m25k.%ext% %source%messtischblat_25k\mosaick\m25k_2180_transparent.%ext%
mklink /H %target%3857\m25k.%ext% %source%messtischblat_25k\mosaick\m25k_3857_transparent.%ext%
mklink /H %target%4326\m25k.%ext% %source%messtischblat_25k\mosaick\m25k_4326_transparent.%ext%
rem kdwr
echo creating links for kdwr
mklink /H %target%2180\kdwr.%ext% %source%kdwr\mosaick\kdwr_2180_transparent.%ext%
mklink /H %target%3857\kdwr.%ext% %source%kdwr\mosaick\kdwr_3857_transparent.%ext%
mklink /H %target%4326\kdwr.%ext% %source%kdwr\mosaick\kdwr_4326_transparent.%ext%
rem kdr_gb
echo creating links for kdr_gb
mklink /H %target%2180\kdr_gb.%ext% %source%kdr_gb\mosaick\kdr_gb_2180_transparent.%ext%
mklink /H %target%3857\kdr_gb.%ext% %source%kdr_gb\mosaick\kdr_gb_3857_transparent.%ext%
mklink /H %target%4326\kdr_gb.%ext% %source%kdr_gb\mosaick\kdr_gb_4326_transparent.%ext%
rem kdr
echo creating links for kdr
mklink /H %target%2180\kdr.%ext% %source%kdr\mosaick\kdr_2180_transparent.%ext%
mklink /H %target%3857\kdr.%ext% %source%kdr\mosaick\kdr_3857_transparent.%ext%
mklink /H %target%4326\kdr.%ext% %source%kdr\mosaick\kdr_4326_transparent.%ext%
echo done
pause