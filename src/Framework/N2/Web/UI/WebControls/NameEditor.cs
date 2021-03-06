using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2.Configuration;

namespace N2.Web.UI.WebControls
{
    /// <summary>A webcontrol that pulls out the page title and transforms it into a web-compatible name that can be used in url's</summary>
    [ValidationProperty("Text")]
    public class NameEditor : CompositeControl, IValidator, ITextControl
    {
        static NameEditorElement config;

        private Slug slug;
        private CheckBox keepUpdated = new CheckBox();
        private TextBox editor = new TextBox();
        private bool? showKeepUpdated;
        private bool? keepUpdatedDefaultValue;
        private char? whitespaceReplacement;
        private bool? toLower;
        private string prefix = string.Empty;
        private string suffix = string.Empty;
        private string uniqueNameErrorFormat = "Another item already has the name '{0}'.";
        private string nameTooLongErrorFormat = "Name too long, the full url cannot exceed {0} characters: {1}";
        private string invalidCharactersErrorFormat = "Invalid characters in path. Only english alphanumerical characters allowed.";

        /// <summary>Constructs the name editor.</summary>
        public NameEditor()
        {
            editor.MaxLength = 250;
            CssClass = "nameEditor";
            slug = N2.Context.Current.Resolve<Slug>();
        }

        public static NameEditorElement Config
        {
            get { return config ?? (Config = LoadConfiguration()); }
            set { config = value; }
        }

        /// <summary>Gets or sets the name of the editor containing the title to input in the name editor.</summary>
        public string TitleEditorName
        {
            get { return (string)(ViewState["TitleEditorName"] ?? "Title"); }
            set { ViewState["TitleEditorName"] = value; }
        }

        public string Prefix
        {
            get { return prefix; }
            set { prefix = value; }
        }

        public string Suffix
        {
            get { return suffix; }
            set { suffix = value; }
        }

        public char? WhitespaceReplacement
        {
            get { return whitespaceReplacement; }
            set { whitespaceReplacement = value; }
        }

        public bool? ToLower
        {
            get { return toLower; }
            set { toLower = value; }
        }

        public bool? ShowKeepUpdated
        {
            get { return showKeepUpdated; }
            set { showKeepUpdated = value; }
        }

        public bool? KeepUpdatedDefaultValue
        {
            get { return keepUpdatedDefaultValue; }
            set { keepUpdatedDefaultValue = value; }
        }

        public string UniqueNameErrorFormat
        {
            get { return uniqueNameErrorFormat; }
            set { uniqueNameErrorFormat = value; }
        }

        public string NameTooLongErrorFormat
        {
            get { return nameTooLongErrorFormat; }
            set { nameTooLongErrorFormat = value; }
        }

        public string InvalidCharactersErrorFormat
        {
            get { return invalidCharactersErrorFormat; }
            set { invalidCharactersErrorFormat = value; }
        }

        public string Text
        {
            get { return editor.Text; }
            set { editor.Text = value; }
        }

        [NotifyParentProperty(true)]
        public CheckBox KeepUpdated
        {
            get { return keepUpdated; }
        }

        [NotifyParentProperty(true)]
        public TextBox Editor
        {
            get { return editor; }
        }

        #region Methods

        private static NameEditorElement LoadConfiguration()
        {
            EditSection editSection = ConfigurationManager.GetSection("n2/edit") as EditSection;
            if (editSection != null)
                return editSection.NameEditor;

            NameEditorElement config = new NameEditorElement();
            config.WhitespaceReplacement = '-';
            config.ShowKeepUpdated = true;
            config.KeepUpdatedDefaultValue = true;
            config.ToLower = true;
            config.MaxUrlLength = 260;
            return config;
        }

        protected override void OnInit(EventArgs e)
        {
            Page.Validators.Add(this);
            base.OnInit(e);
        }

        protected override void CreateChildControls()
        {
            editor.ID = "e";
            Controls.Add(editor);

            keepUpdated.ID = "ku";
            keepUpdated.Checked = false; //Initially unchecked. It gets updated on load via jquery.n2name.js
            keepUpdated.CssClass = "keepUpdated";
            Controls.Add(keepUpdated);

            base.CreateChildControls();
        }

        /// <summary>Initializes the name editor attaching to the title control onchange event.</summary>
        /// <param name="e">ignored</param>
        protected override void OnPreRender(EventArgs e)
        {
            IItemEditor itemEditor = ItemUtility.FindInParents<IItemEditor>(Parent);

            if (itemEditor == null)
                return;

            try
            {
                if (itemEditor.AddedEditors.ContainsKey(TitleEditorName))
                {
                    Control tbTitle = itemEditor.AddedEditors[TitleEditorName];
                    if (tbTitle == null)
                        return;

                    //Setting Visible false removes it from DOM so keepUpdated is kept visible here. It is hidden via js.
                    string titleID = tbTitle.ClientID;
                    string nameID = editor.ClientID;
                    char whitespaceReplacement = (WhitespaceReplacement ?? Config.WhitespaceReplacement);
                    string toLower = (ToLower ?? Config.ToLower).ToString().ToLower();
                    string keepUpdatedDefaultValue = (KeepUpdatedDefaultValue ?? Config.KeepUpdatedDefaultValue).ToString().ToLower();
                    string replacements = GetReplacementsJson();
                    string keepUpdatedBoxID = keepUpdated.ClientID;
                    string showKeepUpdated = (ShowKeepUpdated ?? Config.ShowKeepUpdated).ToString().ToLower();
                    string s = string.Format("jQuery('#{0}').n2name({{nameId:'{0}', titleId:'{1}', whitespaceReplacement:'{2}', toLower:{3}, replacements:{4}, keepUpdatedBoxId:'{5}', keepUpdatedDefaultValue:{6}, showKeepUpdated:{7}}});",
                        nameID, titleID, whitespaceReplacement, toLower, replacements, keepUpdatedBoxID, keepUpdatedDefaultValue, showKeepUpdated);
                    Page.ClientScript.RegisterStartupScript(typeof(NameEditor), "UpdateScript", s, true);
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new N2Exception("No editor definition found for the Title property. The NameEditor copies the title and adjusts it for beeing part of the url. Either add a title editor or use another control to edit the name.", ex);
            }

            base.OnPreRender(e);
        }

        string GetReplacementsJson()
        {
            StringBuilder sb = new StringBuilder("[");
            foreach (PatternValueElement element in Config.Replacements.AllElements)
            {
                if (sb.Length > 1)
                    sb.Append(", ");
                sb.AppendFormat("{{pattern:/{0}/g, value:'{1}'}}", element.Pattern, element.Value);
            }
            sb.Append("]");
            return sb.ToString();
        }

        #endregion

        #region IValidator Members

        /// <summary>Gets or sets the error message generated when the name editor contains invalid values.</summary>
        public string ErrorMessage
        {
            get { return (string)ViewState["ErrorMessage"] ?? ""; }
            set { ViewState["ErrorMessage"] = value; }
        }

        /// <summary>Gets or sets wether the name editor's value passes validaton.</summary>
        public bool IsValid
        {
            get { return ViewState["IsValid"] != null ? (bool)ViewState["IsValid"] : true; }
            set { ViewState["IsValid"] = value; }
        }

        /// <summary>Validates the name editor's value checking uniqueness and lenght.</summary>
        public void Validate()
        {
            ContentItem currentItem = ItemUtility.FindCurrentItem(Parent);
            
            if (currentItem != null)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    // Ensure that the chosen name is locally unique
                    if (!N2.Context.IntegrityManager.IsLocallyUnique(Text, currentItem))
                    {
                        bool unique = false;
                        for (int i = 1; i < 100; i++)
                        {
                            string newText = string.Format("{0}-{1}", Text, i.ToString());
                            if (N2.Context.IntegrityManager.IsLocallyUnique(newText, currentItem))
                            {
                                this.Text = newText;
                                unique = true;
                                break;
                            }
                        }
                        if (!unique)
                        {
                            //Another item with the same parent and the same name was found 
                            ErrorMessage = string.Format(UniqueNameErrorFormat, Text);
                            IsValid = false;
                            return;
                        }
                    }   

                    // Ensure that the path isn't longer than max length
                    if (currentItem.Parent != null && currentItem.Parent.Url.Length + Text.Length > Config.MaxUrlLength)
                    {
                        ErrorMessage = string.Format(NameTooLongErrorFormat, Config.MaxUrlLength, Text);
                        IsValid = false;
                        return;
                    }

                    // Check validity of slug
                    if (!this.slug.IsValid(Text))
                    {
                        ErrorMessage = InvalidCharactersErrorFormat;
                        IsValid = false;
                        return;
                    }
                }
            }

            IsValid = true;
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (!string.IsNullOrEmpty(Prefix))
                writer.Write("<span class='prefix'>" + Prefix + "</span>");
            base.RenderBeginTag(writer);
        }

        /// <summary>Renders an additional asterix when validation didn't pass.</summary>
        /// <param name="writer">The writer to render the asterix to.</param>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);
            if (!string.IsNullOrEmpty(Suffix))
                writer.Write("<span class='suffix'>" + Suffix + "</span>");
            if (!IsValid)
                writer.Write("<span style='color:red'>*</span>");
        }

        #endregion
    }
}
