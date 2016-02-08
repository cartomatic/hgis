<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/Custom/Themes/SimpleBlog/front.master" CodeBehind="FrontPage.aspx.cs" %>
<asp:content id="Content1" contentplaceholderid="cphBody" runat="Server">
<!--Just an iframe, so the map app can be loaded from a remote location-->
<iframe src="http://hgis.maphive.net" id="hgisiframe" style="width: 100%; height: 100%; border: none;">
</iframe>
<script type="text/javascript">
	var ifr = document.getElementById('hgisiframe');
	var lastHash = window.location.hash;
	
	ifr.src = 'http://hgis.maphive.net' + window.location.hash;
	
	var applyPermaLink = function(){
		var plink = window.location.hash.replace('#','');
		ifr.contentWindow.postMessage({evt: 'applypermalink', permalink: plink}, '*');
	}

	var onMessage = function(e){
		//assume the message is sent only from the hgis iframe
		if(e.data && e.data.evt == 'permalinkupdate'){
			window.location.hash = e.data.permalink;
			lastHash = window.location.hash;
		}
	}
	
	//setup evt listeners so can hear stuff from iframe
	if(window.addEventListener){
		window.addEventListener('message',onMessage,false);
	}
	else {
		//this is ie <= 9
		window.attachEvent('onmessage', onMessage);
	}
	
	//setup url hash polling so can update
	setInterval(
		function(){
			if(window.location.hash != lastHash){
				applyPermaLink();
			}
		},
		250
	);
</script>
</asp:content>
