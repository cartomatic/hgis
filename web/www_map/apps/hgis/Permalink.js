/**
 * @author mika
 * @copyright cartomatic.pl
 * Simple permalink module; by default only handles basic map state - center and zoom
 */
Ext.define('hgis.Permalink', {
    
    config: {
        /**
         * ol3 map object
         */
        map: null,
        
        /**
         * an array of extra plink data getter functions;
         * data collected by getter functions are appended to the basic data collected by the permalink
         */
        pLinkGetters: null,
        
        /**
         * an array of xtra plink setter funtions;
         * functions are called in the order and passed the arguments in the order collected by the setter functions
         */
        pLinkSetters: null,
        
        /**
         * Last saved hash
         */
        lastHash: null,
        
        /**
         * Whether or not the module is active
         */
        active: false,
        
        /**
         * Separator for plink data
         */
        plinkSeparator: ',',
        
        /**
         * update sensitivity - how long module waits after the last change before updating the permalink hash
         * @type Number
         */
        sensitivity: 250,
        
        /**
         * How often should check if a hash has changed
         * @type Number
         */
        hashPollingFrequency: 250,
        
        /**
         * Event name to be used when receiving hash change evt
         * @type String
         */
        inEvtName: 'applypermalink',
        
        /**
         * Event name to be used when reporting hash changed
         * @type String
         */
        outEvtName: 'permalinkupdate'
    },
    
    /**
     * Creates an instance
     */
    constructor: function(config){
        
        this.initConfig(config);
        
        if(!Ext.isArray(this.getPLinkGetters())){
            this.setPLinkGetters([]);
        }
        
        if(!Ext.isArray(this.getPLinkSetters())){
            this.setPLinkSetters([]);
        }
        
    },
    
    /**
     * Activates the module
     */
    activate: function(){
        this.applyPermalink();
        this.getMap().on('moveend', this.update, this);
        this.setActive(true);
        this.startHashPolling();
    },
    
    /**
     * flags the module as not active
     */
    deactivate: function(){
        this.stopHashPolling();
        this.getMap().un('moveend', this.update, this);
        this.setActive(false);
        this.resetPermalink();
    },
    
    /**
     * Hash polling scheduler
     * @type {Ext.util.DelayedTask}
     * @private
     */
    hashPollster: null,
    
    /**
     * An object used to enable proper evt removal
     * @type {Object}
     * @private
     */
    onMessageContext:  {
        scope: null,
        handleEvent: function(e){
            //just pass the evt to the actual handler
            this.scope.onMessage(e);
        }
    },
    
    /**
     * starts hash polling
     * @private
     */
    startHashPolling: function(){
        //start the polling
        this.hashPollster = new Ext.util.DelayedTask(
            this.onHashPollRequest,
            this
        );
        this.hashPollster.delay(this.getHashPollingFrequency());
        
        //wire up postmessage listeners if running in iframe
        if(parent){
            //set the context of the onMessage obj, so it can do its work
            this.onMessageContext.scope = this;
            if(window.addEventListener){
                window.addEventListener('message',this.onMessageContext, false);
            }
            else {
                //this is ie <= 9
                window.attachEvent('onmessage', this.onMessageContext);
            }
        }
    },
    
    /**
     * Starts the hash polling / postMessage listening
     * @private
     */
    stopHashPolling: function(){
        
        if(this.hashPollster){
            this.hashPollster.cancel();
        }
        
        //unblind postMessage listeners if running in iframe
        if(parent){
            var me = this;
            if(window.removeEventListener){
               window.removeEventListener('message', this.onMessageContext, false);
            }
            else {
                //this is ie <= 9
                window.removeEvent('onmessage', this.onMessageContext);
            }
        }
    },
    
    /**
     * Poll the hash for changes
     */
    onHashPollRequest: function(){
        
        //get the current hash and see if it has changed
        if(this.getHash() != this.getLastHash()){
            this.applyPermalink();
        }
        
        //reschedule the poll
        this.hashPollster.delay(this.getHashPollingFrequency());
    },
    
    /**
     * onmessageevt callback
     * @param {} e
     */
    onMessage: function(e){
        //make sure, this is the evt i should react to...
        if(e.data && e.data.evt == this.getInEvtName()){
            this.applyPermalink(e.data.permalink);
        }
    },
    
    /**
     * plink update scheduler
     * @type 
     */
    updateScheduler: null,
    
    /**
     * initiates plink update
     */
    update: function(){
        //make sure there is something to do
        if(!this.getActive()) return;
        
        clearTimeout(this.updateScheduler);
        
        
        this.updateScheduler = Ext.defer(
            this.updateInternal,
            this.getSensitivity(),
            this
        );
    },
    
    /**
     * updates permalink
     */
    updateInternal: function(){
        
        var newH = this.getPermalink();
        this.setLastHash(newH);
            
        //update the browser's url
        window.location.hash = newH;
        
        //and if launched in an iframe report plink update to the parent
        if (parent) {
            parent.postMessage(
                {
                    evt: this.getOutEvtName(),
                    permalink: newH
                },
                '*'
            );
        }
    },
    
    /**
     * Gets the hash off the url
     */
    getHash: function(){
        return window.location.hash.replace('#', '');
    },
    
    /**
     * Gets permalink string
     */
    getPermalink: function(){
        
        var pldata = [];
        
        var vw = this.getMap().getView();
        
        //permalink is created base on the map center and zoom
        var c = vw.getCenter();
        var z = vw.getZoom();
        
        pldata = [c[0], c[1], z];
        
        //if supplied, extra functions are used to collect extra data
        var pLinkGetters = this.getPLinkGetters();
        for(var i = 0; i < pLinkGetters.length; i ++){
            var data = '';
            if(Ext.isFunction(pLinkGetters[i])){
                data = pLinkGetters[i]();
            }
            pldata.push(data);
        }
        return pldata.join(this.getPlinkSeparator());
    },
    
    /**
     * Applies permalink
     */
    applyPermalink: function(hash){
        //first get the content of permalink off the url
        var vw = this.getMap().getView();
        
        //grab the current hash if not provided
        if(!hash) hash = this.getHash();

        //and split it into parts
        var pldata = hash.split(this.getPlinkSeparator());

        //position
        if (pldata.length > 1) {
            var lon = parseFloat(pldata[0]);
            var lat = parseFloat(pldata[1]);
            if (!isNaN(lon) && !isNaN(lat)) {
                vw.setCenter([lon, lat]);
            }
        }

        //zoom
        if (pldata.length > 2) {
            var z = parseFloat(pldata[2])
            if(!isNaN(z)){
                vw.setZoom(z);
            }
        }
        
        //custom data starts at index 3
        var custIdx = 3;
        var i = 0;
        var pLinkSetters = this.getPLinkSetters();
        for(i; i < pLinkSetters.length; i++){
            var dataIdx = i + custIdx;
            if(pldata.length > dataIdx){
                if(Ext.isFunction(pLinkSetters[i])){
                    pLinkSetters[i](pldata[dataIdx]);
                }
            }
            else{
                break;
            }
        }
    },
    
    /**
     * Resets url hash
     */
    resetPermalink: function(){
        window.location.hash = null;
    }
});