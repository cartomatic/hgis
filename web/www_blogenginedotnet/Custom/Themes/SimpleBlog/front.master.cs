using System;
using System.Web.UI;
using BlogEngine.Core;
using System.Text.RegularExpressions;

public partial class SimpleBlogHGIS_Front : System.Web.UI.MasterPage
{

    protected void Page_Load(object sender, EventArgs e)
    {
        // needed to make <%# %> binding work in the page header
        Page.Header.DataBind();
    }

    protected override void Render(HtmlTextWriter writer)
    {
        using (HtmlTextWriter htmlwriter = new HtmlTextWriter(new System.IO.StringWriter()))
        {
            base.Render(htmlwriter);
            writer.Write(htmlwriter.InnerWriter.ToString());
        }
    }

}
