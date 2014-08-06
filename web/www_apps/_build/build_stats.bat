sencha -sdk F:\_jsLibs\ExtJs\5.0.0\ ^
compile -classpath=..\apps\stats ^
exclude -namespace Ext.* and ^
exclude -tag core and ^
exclude -file LoaderSettings and ^
concat -out ..\apps\_build\stats-all-dev.js and ^
-option debug:false ^
concat -out ..\apps\_build\stats-all-debug.js and ^
concat -yui -out ..\apps\_build\stats-all.js
pause