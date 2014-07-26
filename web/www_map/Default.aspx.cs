using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;

public partial class _Default : System.Web.UI.Page
{
    //where to link the jslibs from
    string jslibs = ConfigurationManager.AppSettings["jslibs"];


    protected void Page_Load(object sender, EventArgs e)
    {
        InitHgis();
    }

    
    /// <summary>
    /// Inits GeoReg3 app
    /// </summary>
    private void InitHgis()
    {

        //Inject the scripts
        IncludeLocalhost();

        //Favicon
        IncludeFavicon();

        //Splashscreen
        IncludeSplashScreen();

        
        //OpenLayers
        IncludeOpenLayers();

        //ExtJs
        IncludeExtJs();

        //token
        IncludeToken();


        //custom css
        IncludeCustomCss();


        //Always include the LoaderSettings script before the actual app script is included
        //This makes it possible to set all the references and dynamic variables properly even for compiled scripts
        //that are likely to have a bit different loading order than when using a dynamic loader
        var loader_path = "apps/hgis/LoaderSettings.js";
        if (System.IO.File.Exists(Server.MapPath(loader_path)))
        {
            //include the app script
            System.Web.UI.HtmlControls.HtmlGenericControl loader = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
            loader.Attributes["type"] = "text/javascript";
            loader.Attributes["src"] = loader_path;
            Page.Header.Controls.Add(loader);
        }


        //check if a compiled js should be used - it is always used whenever the built js script is present
        bool use_compiled_js = false;
        if (System.IO.File.Exists(Server.MapPath("apps/_build/hgis-all.js")))
        {
            use_compiled_js = true;
        }

        //if the app is meant to use compiled js, test if there is a param that revokes it - used by devs
        if (use_compiled_js)
        {
            bool devjs;
            Boolean.TryParse(Request.QueryString["devjs"], out devjs);
            use_compiled_js = !devjs;
        }

        string script_path = "apps/hgis/app.js";

        if (use_compiled_js)
        {
            bool debugjs;
            Boolean.TryParse(Request.QueryString["debugjs"], out debugjs);
            if (debugjs)
            {
                script_path = "apps/_build/hgis-all-debug.js";
            }
            else
            {
                script_path = "apps/_build/hgis-all.js";
            }
        }

        //Finally the application entry point
        System.Web.UI.HtmlControls.HtmlGenericControl js = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        js.Attributes["type"] = "text/javascript";
        js.Attributes["src"] = script_path;
        Page.Header.Controls.Add(js);
    }

    /// <summary>
    /// Includes custom css
    /// </summary>
    private void IncludeCustomCss()
    {
        //
        System.Web.UI.HtmlControls.HtmlLink hgisCss = new System.Web.UI.HtmlControls.HtmlLink();
        hgisCss.Attributes["rel"] = "stylesheet";
        hgisCss.Attributes["type"] = "text/css";
        hgisCss.Href = "resx/css/hgis.css";
        Page.Header.Controls.Add(hgisCss);

        //font awsome
        System.Web.UI.HtmlControls.HtmlLink faCss = new System.Web.UI.HtmlControls.HtmlLink();
        faCss.Attributes["rel"] = "stylesheet";
        faCss.Attributes["type"] = "text/css";
        faCss.Href = "resx/font-awesome-4.1.0/css/font-awesome.min.css";
        Page.Header.Controls.Add(faCss);
    }

    /// <summary>
    /// Takes care of generating the token used to authenticate the services acces
    /// </summary>
    private void IncludeToken()
    {
        HGIS.TokenMaster tm = HGIS.TokenMaster.FromFile(ConfigurationManager.AppSettings["token_master_settings"]);

        System.Web.UI.HtmlControls.HtmlGenericControl localhostScript = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        localhostScript.Attributes["type"] = "text/javascript";
        localhostScript.InnerHtml =
            "var __token__ = {param: '" + tm.GetTokenParam() + "', value: '" + tm.GimmeToken(Request) + "'}";
        Page.Header.Controls.Add(localhostScript);
    }
    
    //Generic script injection logic
    //------------------------------


    /// <summary>
    /// Includes splash screen
    /// </summary>
    private void IncludeSplashScreen()
    {
        //First inject splashscreen css and js
        System.Web.UI.HtmlControls.HtmlLink ssCss = new System.Web.UI.HtmlControls.HtmlLink();
        ssCss.Attributes["rel"] = "stylesheet";
        ssCss.Attributes["type"] = "text/css";
        ssCss.Href = "splash/hgis.css";
        Page.Header.Controls.Add(ssCss);

        System.Web.UI.HtmlControls.HtmlGenericControl ssJs = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        ssJs.Attributes["type"] = "text/javascript";
        ssJs.Attributes["src"] = "splash/hgis.js";
        Page.Header.Controls.Add(ssJs);
    }

    /// <summary>
    /// Injects a variable holding a jslibs location - used for dynamic script linking
    /// </summary>
    private void IncludeLocalhost()
    {
        System.Web.UI.HtmlControls.HtmlGenericControl localhostScript = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        localhostScript.Attributes["type"] = "text/javascript";
        localhostScript.InnerHtml =
            "var __jslibs__ = '" + jslibs + "';";
        Page.Header.Controls.Add(localhostScript);
    }

    /// <summary>
    /// Injects favicon
    /// </summary>
    private void IncludeFavicon()
    {
        System.Web.UI.HtmlControls.HtmlLink favicon = new System.Web.UI.HtmlControls.HtmlLink();
        favicon.Attributes["rel"] = "icon";
        favicon.Attributes["type"] = "image/png";
        favicon.Href = "resx/favicon/favicon.png";
        Page.Header.Controls.Add(favicon);
    }


    /// <summary>
    /// Injects ExtJS
    /// </summary>
    private void IncludeExtJs()
    {
        //web.config sencha_settings
        var sencha_settings = Cartomatic.Utils.Serialisation.DeserializeJsonString(ConfigurationManager.AppSettings["sencha_settings"]);

        string version = Request.QueryString["ext"];
        if (string.IsNullOrEmpty(version))
        {
            version = sencha_settings.extjs_version;
        }

        //Emit some info on picked extjs version so xtra ext goodies can be linked dynamically
        System.Web.UI.HtmlControls.HtmlGenericControl extJsVersionScript = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        extJsVersionScript.Attributes["type"] = "text/javascript";
        extJsVersionScript.InnerHtml =
            "var __extjs__ = '" + version + "';";
        Page.Header.Controls.Add(extJsVersionScript);


        string style = Request.QueryString["style"] ?? "";
        string extThemeName = "";
        switch (style.ToLower())
        {
            default:              
            case "crisp-touch":
                extThemeName = "ext-theme-crisp-touch";
                break;

            case "crisp":
                extThemeName = "ext-theme-crisp";
                break;

            case "blue":
                extThemeName = "ext-theme-classic";
                break;

            case "gray":
                extThemeName = "ext-theme-gray";
                break;

            case "neptune":
                extThemeName = "ext-theme-neptune";
                break;

            case "neptune-touch":
                extThemeName = "ext-theme-neptune-touch";
                break;
        }

        string extSkinCssPath = jslibs + "extjs/" + version + "/build/packages/" + extThemeName + "/build/resources/" + extThemeName + "-all.css";
        string extSkinJsPath = jslibs + "extjs/" + version + "/build/packages/" + extThemeName + "/build/" + extThemeName + ".js";;

        //Add extjs css
        System.Web.UI.HtmlControls.HtmlLink skinCss = new System.Web.UI.HtmlControls.HtmlLink();
        skinCss.Attributes["rel"] = "stylesheet";
        skinCss.Attributes["type"] = "text/css";
        skinCss.Href = extSkinCssPath;
        Page.Header.Controls.Add(skinCss);

               
        bool debugjs;
        Boolean.TryParse(Request.QueryString["devjs"], out debugjs);

        //Extjs script
        System.Web.UI.HtmlControls.HtmlGenericControl extJs = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        extJs.Attributes["type"] = "text/javascript";
        if (debugjs)
        {
            extJs.Attributes["src"] = jslibs + "extjs/" + version + "/build/ext-all-debug.js";
        }
        else
        {
            extJs.Attributes["src"] = jslibs + "extjs/" + version + "/build/ext-all.js";
        }
        Page.Header.Controls.Add(extJs);


        //add extjs skin js
        System.Web.UI.HtmlControls.HtmlGenericControl skinJs = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        skinJs.Attributes["type"] = "text/javascript";
        skinJs.Attributes["src"] = extSkinJsPath;
        Page.Header.Controls.Add(skinJs);
    }


    /// <summary>
    /// Injects OpenLayers CSS & JS;
    /// Injects proj4js JS
    /// </summary>
    private void IncludeOpenLayers()
    {
        //web.config ol_settings
        var ol_settings = Cartomatic.Utils.Serialisation.DeserializeJsonString(ConfigurationManager.AppSettings["ol_settings"]);

        //extract version from url
        string version = Request.QueryString["ol"];

        //if not provided auto default to whatever is set in the web.config
        if (string.IsNullOrEmpty(version))
        {
            version = ol_settings.version;
        }

        //OpenLayers css
        System.Web.UI.HtmlControls.HtmlLink olCss = new System.Web.UI.HtmlControls.HtmlLink();
        olCss.Attributes["rel"] = "stylesheet";
        olCss.Attributes["type"] = "text/css";
        olCss.Href = jslibs + "OpenLayers/" + version + "/css/ol.css";
        Page.Header.Controls.Add(olCss);


        //Proj4Js
        System.Web.UI.HtmlControls.HtmlGenericControl p4Js = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        p4Js.Attributes["type"] = "text/javascript";
        p4Js.Attributes["src"] = jslibs + "proj4js/1.5.0/dist/proj4.js";
        Page.Header.Controls.Add(p4Js);


        //OpenLayers
        System.Web.UI.HtmlControls.HtmlGenericControl olJs = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
        olJs.Attributes["type"] = "text/javascript";
        olJs.Attributes["src"] = jslibs + "OpenLayers/" + version + "/build/ol.js";
        Page.Header.Controls.Add(olJs);
    }


    /// <summary>
    /// Returns 404
    /// </summary>
    private void Fail()
    {
        Context.Response.Clear();
        Context.Response.Status = "404 Not Found";
        Context.Response.StatusCode = 404;

        Context.ApplicationInstance.CompleteRequest();
    }
}