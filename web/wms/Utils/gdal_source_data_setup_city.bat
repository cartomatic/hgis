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
rem bialystok_gsgs4435
echo creating links for bialystok_gsgs4435
mklink /H %target%2180\bialystok_gsgs4435.%ext% %source%bialystok_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\bialystok_gsgs4435.%ext% %source%bialystok_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\bialystok_gsgs4435.%ext% %source%bialystok_gsgs4435_4326_transparent.%ext%
rem breslau_gsgs4480
echo creating links for breslau_gsgs4480
mklink /H %target%2180\breslau_gsgs4480.%ext% %source%breslau_gsgs4480_2180_transparent.%ext%
mklink /H %target%3857\breslau_gsgs4480.%ext% %source%breslau_gsgs4480_3857_transparent.%ext%
mklink /H %target%4326\breslau_gsgs4480.%ext% %source%breslau_gsgs4480_4326_transparent.%ext%
rem bydgoszcz_gsgs4435
echo creating links for bydgoszcz_gsgs4435
mklink /H %target%2180\bydgoszcz_gsgs4435.%ext% %source%bydgoszcz_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\bydgoszcz_gsgs4435.%ext% %source%bydgoszcz_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\bydgoszcz_gsgs4435.%ext% %source%bydgoszcz_gsgs4435_4326_transparent.%ext%
rem czestochowa_gsgs4435
echo creating links for czestochowa_gsgs4435
mklink /H %target%2180\czestochowa_gsgs4435.%ext% %source%czestochowa_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\czestochowa_gsgs4435.%ext% %source%czestochowa_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\czestochowa_gsgs4435.%ext% %source%czestochowa_gsgs4435_4326_transparent.%ext%
rem danzig_gsgs4496
echo creating links for danzig_gsgs4496
mklink /H %target%2180\danzig_gsgs4496.%ext% %source%danzig_gsgs4496_2180_transparent.%ext%
mklink /H %target%3857\danzig_gsgs4496.%ext% %source%danzig_gsgs4496_3857_transparent.%ext%
mklink /H %target%4326\danzig_gsgs4496.%ext% %source%danzig_gsgs4496_4326_transparent.%ext%
rem grudziac_gsgs4435
echo creating links for grudziac_gsgs4435
mklink /H %target%2180\grudziac_gsgs4435.%ext% %source%grudziac_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\grudziac_gsgs4435.%ext% %source%grudziac_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\grudziac_gsgs4435.%ext% %source%grudziac_gsgs4435_4326_transparent.%ext%
rem inowroclaw_gsgs4435
echo creating links for inowroclaw_gsgs4435
mklink /H %target%2180\inowroclaw_gsgs4435.%ext% %source%inowroclaw_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\inowroclaw_gsgs4435.%ext% %source%inowroclaw_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\inowroclaw_gsgs4435.%ext% %source%inowroclaw_gsgs4435_4326_transparent.%ext%
rem jaslo_gsgs4435
echo creating links for jaslo_gsgs4435
mklink /H %target%2180\jaslo_gsgs4435.%ext% %source%jaslo_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\jaslo_gsgs4435.%ext% %source%jaslo_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\jaslo_gsgs4435.%ext% %source%jaslo_gsgs4435_4326_transparent.%ext%
rem katowice_gsgs4435
echo creating links for katowice_gsgs4435
mklink /H %target%2180\katowice_gsgs4435.%ext% %source%katowice_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\katowice_gsgs4435.%ext% %source%katowice_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\katowice_gsgs4435.%ext% %source%katowice_gsgs4435_4326_transparent.%ext%
rem kielce_gsgs4435
echo creating links for kielce_gsgs4435
mklink /H %target%2180\kielce_gsgs4435.%ext% %source%kielce_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\kielce_gsgs4435.%ext% %source%kielce_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\kielce_gsgs4435.%ext% %source%kielce_gsgs4435_4326_transparent.%ext%
rem krakow_gsgs4435
echo creating links for krakow_gsgs4435
mklink /H %target%2180\krakow_gsgs4435.%ext% %source%krakow_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\krakow_gsgs4435.%ext% %source%krakow_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\krakow_gsgs4435.%ext% %source%krakow_gsgs4435_4326_transparent.%ext%
rem lodz_gsgs4435
echo creating links for lodz_gsgs4435
mklink /H %target%2180\lodz_gsgs4435.%ext% %source%lodz_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\lodz_gsgs4435.%ext% %source%lodz_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\lodz_gsgs4435.%ext% %source%lodz_gsgs4435_4326_transparent.%ext%
rem lublin_gsgs4435
echo creating links for lublin_gsgs4435
mklink /H %target%2180\lublin_gsgs4435.%ext% %source%lublin_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\lublin_gsgs4435.%ext% %source%lublin_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\lublin_gsgs4435.%ext% %source%lublin_gsgs4435_4326_transparent.%ext%
rem poznan_gsgs4435
echo creating links for poznan_gsgs4435
mklink /H %target%2180\poznan_gsgs4435.%ext% %source%poznan_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\poznan_gsgs4435.%ext% %source%poznan_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\poznan_gsgs4435.%ext% %source%poznan_gsgs4435_4326_transparent.%ext%
rem radom_gsgs4435
echo creating links for radom_gsgs4435
mklink /H %target%2180\radom_gsgs4435.%ext% %source%radom_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\radom_gsgs4435.%ext% %source%radom_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\radom_gsgs4435.%ext% %source%radom_gsgs4435_4326_transparent.%ext%
rem rzeszow_gsgs4435
echo creating links for rzeszow_gsgs4435
mklink /H %target%2180\rzeszow_gsgs4435.%ext% %source%rzeszow_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\rzeszow_gsgs4435.%ext% %source%rzeszow_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\rzeszow_gsgs4435.%ext% %source%rzeszow_gsgs4435_4326_transparent.%ext%
rem siedlce_gsgs4435
echo creating links for siedlce_gsgs4435
mklink /H %target%2180\siedlce_gsgs4435.%ext% %source%siedlce_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\siedlce_gsgs4435.%ext% %source%siedlce_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\siedlce_gsgs4435.%ext% %source%siedlce_gsgs4435_4326_transparent.%ext%
rem tarnow_gsgs4435
echo creating links for tarnow_gsgs4435
mklink /H %target%2180\tarnow_gsgs4435.%ext% %source%tarnow_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\tarnow_gsgs4435.%ext% %source%tarnow_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\tarnow_gsgs4435.%ext% %source%tarnow_gsgs4435_4326_transparent.%ext%
rem torun_gsgs4435
echo creating links for torun_gsgs4435
mklink /H %target%2180\torun_gsgs4435.%ext% %source%torun_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\torun_gsgs4435.%ext% %source%torun_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\torun_gsgs4435.%ext% %source%torun_gsgs4435_4326_transparent.%ext%
rem warszawa_gsgs4435
echo creating links for warszawa_gsgs4435
mklink /H %target%2180\warszawa_gsgs4435.%ext% %source%warszawa_gsgs4435_2180_transparent.%ext%
mklink /H %target%3857\warszawa_gsgs4435.%ext% %source%warszawa_gsgs4435_3857_transparent.%ext%
mklink /H %target%4326\warszawa_gsgs4435.%ext% %source%warszawa_gsgs4435_4326_transparent.%ext%

pause