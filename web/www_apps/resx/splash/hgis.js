/**
 * @author mika <mika@cartomatic.pl>
 * @docauthor mika <mika@cartomatic.pl>
 * This is a simplistic splash screen.
 * 
 * The splashcreen will start automatically after it is loaded.
 * To dismiss it call splashScreen.hide()
 */
var SplashScreen = function() {
	
	this.getHtml = function(){
		var html = 
			'<div id="splash-screen-container">' +
				'<div id="splash-screen-mask"></div>' +
		        '<div id="load-info-container">' +
			        '<div id="loader-image"></div>' +
		        '</div>' +
	    	'</div>';
	    	
    	return html;
	}
	
	this.init = function(){
		//emit html to dom
		document.write(this.getHtml());
	}
	
	this.hidden = false;
	this.hide = function(){
		if(!this.hidden){
			this.hidden = true;
			clearTimeout(this.autoUpdateSchedule); 
			var ssc = Ext.get('splash-screen-container');
			if(ssc.fadeOut){
				ssc.fadeOut({duration: 1000, remove: true});
			}
			else {
				if(ssc.remove){
					ssc.remove(); //Sencha touch 1.x and 2 pre and betas
				}
				else {
					ssc.destroy(); //sencha touch 2.x
				}
			}
		}
	}
};

//init the splash screen after the script has been loaded
var splashScreen = new SplashScreen();
splashScreen.init();