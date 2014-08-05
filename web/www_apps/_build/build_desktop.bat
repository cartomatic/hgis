sencha -sdk F:\_jsLibs\ExtJs\5.0.0\ ^
compile -classpath=..\apps\hgis ^
exclude -namespace Ext.* and ^
exclude -tag core and ^
exclude -file LoaderSettings and ^
concat -out ..\apps\_build\hgis-all-dev.js and ^
-option debug:false ^
concat -out ..\apps\_build\hgis-all-debug.js and ^
concat -yui -out ..\apps\_build\hgis-all.js
pause