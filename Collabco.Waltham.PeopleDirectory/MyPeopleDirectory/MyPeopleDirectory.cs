using System;
using System.Text;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Collabco.Saturn.SharePoint;
using Collabco.Saturn;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace Collabco.Waltham.PeopleDirectory.MyPeopleDirectory
{
    [ToolboxItemAttribute(false)]
    public class MyPeopleDirectory : WebPart
    {
        public const string TILE_NAME = "pds";
        #region properties
        string _IDENTITY = Guid.NewGuid().ToString().Replace("-", string.Empty);

        private string _linkJSON = "[]";

        [WebBrowsable(false),
        Personalizable(PersonalizationScope.Shared, true)]
        public string linkJSON
        {
            get { return _linkJSON; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _linkJSON = "[]";
                }
                else
                {
                    _linkJSON = value;
                }
            }
        }


        #endregion

        protected override void Render(HtmlTextWriter writer)
        {
            
            using (SPMonitoredScope monitoredScope = new SPMonitoredScope("Mars " + this.Title + " Render"))
            {
                writer.WriteLine("<div class=\"tileModalParent\">");
                writer.WriteLine(Util.GenerateTile(TILE_NAME, this.Title, _IDENTITY));

                //StringBuilder _modalContent = new StringBuilder();

                //_modalContent.Append(Util.GenerateIconHeading(null, "openc", "openc-" + _IDENTITY, false, "openc-head-" + _IDENTITY));

                //writer.WriteLine(Util.GenerateModal(TILE_NAME, this.Title, _IDENTITY, _modalContent.ToString()));

                writer.WriteLine("</div>");
            }

            base.Render(writer);
        }
        protected override void CreateChildControls()
        {
            this.PreRender += MyPeopleDirectory_PreRender;
        }

        void MyPeopleDirectory_PreRender(object sender, EventArgs e)
        {
            using (SPMonitoredScope monitoredScope = new SPMonitoredScope("Mars " + this.Title + " Pre-Render (" + _IDENTITY + ")"))
            {
                EnsureScript.Include(this.Page);

                string scriptURL = "tile." + TILE_NAME + ".debug.js";
                string cssURL = "tile." + TILE_NAME + ".debug.css";
                string scriptTag = "mars-" + TILE_NAME + "-debug";

                if (!this.Page.ClientScript.IsClientScriptBlockRegistered(scriptTag))
                {
                    StringBuilder script = new StringBuilder();

                    script.AppendLine("<script type='text/javascript' src='/_layouts/15/collabco/js/" + scriptURL + "'></script>");
                    script.AppendLine("<link rel='stylesheet' type='text/css' href='/_layouts/15/collabco/css/" + cssURL + "'>");


                    this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), scriptTag, script.ToString(), false);
                }

                if (!this.Page.ClientScript.IsClientScriptBlockRegistered("mars-" + TILE_NAME + "-trigger-" + _IDENTITY))
                {
                    StringBuilder s = new StringBuilder();

                    s.AppendLine("_spBodyOnLoadFunctionNames.push(\"mars_" + TILE_NAME + "_trigger_" + _IDENTITY + "\")");

                    s.AppendLine("function mars_" + TILE_NAME + "_trigger_" + _IDENTITY + " () {");
                    s.AppendLine("try {");
                    s.AppendLine("  if (!jQuery(\"#hubtile-" + TILE_NAME + "-" + _IDENTITY + "\").hasClass(\"renderComplete\")) {");

                    if (!string.IsNullOrEmpty(_linkJSON) && _linkJSON.Length > 2)
                    {
                        if (Util.ContainsTokens(_linkJSON))
                        {

                            var saturnConnector = new SharePointSaturnConnector();
                            var userContext = saturnConnector.GetUserContext();
                            _linkJSON = Util.ReplaceTokensInString(_linkJSON, userContext);
                        }

                        s.AppendLine(String.Format("    addModalButtons(\"{0}\", \"{1}\", \"\", {2});", TILE_NAME, _IDENTITY, _linkJSON));
                    }

                    s.AppendLine(string.Format("    setTimeout(function(){{spinPeopleDirectoryTile(\"{0}\", \"{1}\")}},Math.floor((Math.random()*2500)+1));", _IDENTITY, this.Title)); //Example call to javascript function to render tile data passing example setting 1 through.
                    s.AppendLine(String.Format("    jQuery(\"#draggable-{0}-{1}\").appendTo(jQuery(\"#s4-workspace\"));", TILE_NAME, _IDENTITY));
                    s.AppendLine("    jQuery(\"#hubtile-" + TILE_NAME + "-" + _IDENTITY + "\").addClass(\"renderComplete\");");
                    s.AppendLine("  }");
                    s.AppendLine("} catch (e) {console.log('tile." + TILE_NAME + " > trigger() :' + e)}");
                    s.AppendLine("};");

                    this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "mars-" + TILE_NAME + "-trigger-" + _IDENTITY, s.ToString(), true);
                }
            }
        }
    }
}
