using N2.Edit.Versioning;
using N2.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace N2.Edit.Web.UI.Controls
{
    public class ItemLink : HyperLink
    {
        public ItemLink()
        {
            ShowIcon = true;
        }

        public object DataSource { get; set; }

        public string InterfaceUrl { get; set; }

        public string ParentQueryString { get; set; }

        public bool ShowIcon { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        
            var item = DataSource as ContentItem;
			if (item == null)
			{
				var version = DataSource as ContentVersion;
				if (version != null)
				{
					item = version.GetVersionInfo(N2.Context.Current.Resolve<IContentVersionRepository>()).Content;
				}

				return;
			}

            if (string.IsNullOrEmpty(InterfaceUrl))
                this.NavigateUrl = N2.Context.Current.GetContentAdapter<NodeAdapter>(item).GetPreviewUrl(item);
            else
                this.NavigateUrl = InterfaceUrl.ResolveUrlTokens().ToUrl().AppendSelection(item);
            
            if (!string.IsNullOrEmpty(ParentQueryString))
                this.NavigateUrl = string.Format("{0}{1}{2}", this.NavigateUrl, this.NavigateUrl.Contains("?") ? "&" : "?", ParentQueryString);
            
            var title = string.IsNullOrEmpty(item.Title) ? N2.Context.Current.Definitions.GetDefinition(item).Title : item.Title;

            if (!ShowIcon)
            {
                Text = title;
            }
            else if (string.IsNullOrEmpty(item.IconUrl))
            {
                Text = string.Format("<b class='{0}'></b> {1}", item.IconClass, title);
            }
            else
            {
                Text = string.Format("<img src='{0}' alt='{1}' /> {2}", item.IconUrl.ResolveUrlTokens(), item.GetContentType().Name, title);
            }

            Attributes.Add("title", title);
        }
    }
}
