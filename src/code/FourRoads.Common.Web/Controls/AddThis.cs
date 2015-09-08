using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using FourRoads.Common.Attributes;

namespace FourRoads.Common.Web.UI
{
    public enum AddThisButtonImage
    {
        [ImageUrl("http://s7.addthis.com/button0-bm.gif"), Height(16), Width(83)]
        Bookmark,
        [ImageUrl("http://s7.addthis.com/button1-bm.gif"), Height(16), Width(125)]
        BookmarkExpanded,
        [ImageUrl("http://s7.addthis.com/button0-share.gif"), Height(16), Width(83)]
        Share,
        [ImageUrl("http://s7.addthis.com/button1-share.gif"), Height(16), Width(125)]
        ShareExpanded,
        [ImageUrl("http://s7.addthis.com/button1-addthis.gif"), Height(16), Width(125)]
        AddThis,
        [ImageUrl("http://s7.addthis.com/addthis16.gif"), Height(16), Width(16)]
        Plus,        
        Custom
    }

    public enum AddThisButtonBehavior
    {
        Button,
        DropDown
    }

    public class AddThisLink : HyperLink
    {
        readonly string _dropDownCode = "<!-- AddThis Button BEGIN -->\n<script type=\"text/javascript\">addthis_pub  = \"{0}\";</script><a href=\"http://www.addthis.com/bookmark.php\" onmouseover=\"return addthis_open(this, '', '{1}', '{2}')\" onmouseout=\"addthis_close()\" onclick=\"return addthis_sendto()\"><img src=\"{3}\" width=\"{4}\" height=\"{5}\" border=\"0\" alt=\"\" /></a><script type=\"text/javascript\" src=\"http://s7.addthis.com/js/152/addthis_widget.js\"></script>\n<!-- AddThis Button END -->";
        readonly string _buttonCode = "<!-- AddThis Button BEGIN --><a href=\"http://www.addthis.com/bookmark.php\" onclick=\"addthis_url = '{1}'; addthis_title = '{2}'; return addthis_click(this);\" target=\"_blank\"><img src=\"{3}\" width=\"{4}\" height=\"{5}\" border=\"0\" alt=\"Bookmark and Share\" /></a> <script type=\"text/javascript\">var addthis_pub = '{0}';</script><script type=\"text/javascript\" src=\"http://s7.addthis.com/js/widget.php?v=10\"></script><!-- AddThis Button END -->";

        public string Publisher
        {
            get { return (string)(ViewState["Publisher"] ?? GetConfigValue("AddThis.Publisher", null));}
            set { ViewState["Publisher"] = value; }
        }
        
        public AddThisButtonBehavior ButtonBehavior
        {
            get { return (AddThisButtonBehavior)(ViewState["ButtonBehavior"] ?? Enum.Parse(typeof(AddThisButtonBehavior), GetConfigValue("AddThis.ButtonBehavior", AddThisButtonBehavior.Button.ToString()))); }
            set { ViewState["ButtonBehavior"] = value; }
        }
        
        public AddThisButtonImage ButtonImage
        {
            get { return (AddThisButtonImage)(ViewState["ButtonImage"] ?? Enum.Parse(typeof(AddThisButtonImage), GetConfigValue("AddThis.ButtonImage", AddThisButtonImage.AddThis.ToString()))); }
            set { ViewState["ButtonImage"] = value; }
        }

        public virtual string TitleDataField
        {
            get { return (string)(ViewState["TitleDataField"] ?? string.Empty); }
            set { ViewState["TitleDataField"] = value; }
        }

        public virtual string UrlDataField
        {
            get { return (string)(ViewState["UrlDataField"] ?? string.Empty); }
            set { ViewState["UrlDataField"] = value; }
        }

        public virtual object DataSource {get; set; }

        protected bool IsDataBound = false;
        public override void DataBind()
        {
            base.DataBind();
            if (DataSource != null)
            {
                if (!string.IsNullOrEmpty(TitleDataField))
                    Text = DataBinder.Eval(DataSource, TitleDataField) as string;

                if (!string.IsNullOrEmpty(UrlDataField))
                    NavigateUrl = DataBinder.Eval(DataSource, UrlDataField) as string;
            }
            IsDataBound = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!IsDataBound)
                DataBind();
            base.OnPreRender(e);
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            
            if (string.IsNullOrEmpty(Publisher))
            {
                Visible = false;
                return;
            }

            if (string.IsNullOrEmpty(NavigateUrl) || string.IsNullOrEmpty(Text))
            {
                Visible = false;
                return;
            }

            string code = null;
            switch (ButtonBehavior)
            {
                case AddThisButtonBehavior.Button:
                    code = _buttonCode;
                    break;
                case AddThisButtonBehavior.DropDown:
                    code = _dropDownCode;
                    break;
            }

            switch (ButtonImage)
            {                
                case AddThisButtonImage.Custom:
                    code = string.Format(code, Publisher, NavigateUrl, Text, ImageUrl, Width, Height);
                    break;
                default:
                    code = string.Format(code, Publisher, NavigateUrl, Text,
                        AttributeUtility.GetImageUrl(ButtonImage), 
                        AttributeUtility.GetWidth(ButtonImage), 
                        AttributeUtility.GetHeight(ButtonImage));
                    break;
            }
            writer.Write(code);
            //writer.Write("<!-- AddThis Button BEGIN -->\n<script type=\"text/javascript\">addthis_pub  = \"{0}\";</script><a href=\"http://www.addthis.com/bookmark.php\" onmouseover=\"return addthis_open(this, '', '{1}', '{2}')\" onmouseout=\"addthis_close()\" onclick=\"return addthis_sendto()\"><img src=\"http://s7.addthis.com/button1-share.gif\" width=\"125\" height=\"16\" border=\"0\" alt=\"\" /></a><script type=\"text/javascript\" src=\"http://s7.addthis.com/js/152/addthis_widget.js\"></script>\n<!-- AddThis Button END -->", this.Publisher, this.NavigateUrl, this.Text);
        }

        protected virtual string GetConfigValue(string name, string defaultValue)
        {
            string value = System.Configuration.ConfigurationManager.AppSettings[name];
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return value;
        }

    }
}

