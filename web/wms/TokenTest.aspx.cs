using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;

public partial class TokenTest : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["generate_token"] == "true")
        {
            HGIS.TokenMaster tm = HGIS.TokenMaster.FromFile(ConfigurationManager.AppSettings["token_master_settings"]);

            System.Web.UI.HtmlControls.HtmlGenericControl localhostScript = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
            localhostScript.Attributes["type"] = "text/javascript";
            localhostScript.InnerHtml =
                "var __token__ = '" + tm.GimmeToken() + "';";
            Page.Header.Controls.Add(localhostScript);
        }
    }
}