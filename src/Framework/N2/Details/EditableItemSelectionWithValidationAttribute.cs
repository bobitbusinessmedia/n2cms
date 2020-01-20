using N2.Definitions;
using N2.Edit;
using N2.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Parameter = N2.Persistence.Parameter;
using ParameterCollection = N2.Persistence.ParameterCollection;

namespace N2.Details
{
    /// <summary>
    /// Allows selecting zero or more items of a specific type from an exapandable check box list.
    /// </summary>
    /// <example>
    ///     [EditableItemSelection]
    ///     public virtual IEnumerable&gt;ContentItem&lt; Links { get; set; }
    /// </example>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditableItemSelectionWithValidationAttribute : EditableDropDownWithValidationAttribute
    {
        public Type LinkedType { get; set; }
        public Type ExcludedType { get; set; }
        public int SearchTreshold { get; set; }
        public bool ListItemsBelowCurrentStartPageOnly { get; set; }
        public EditableItemSelectionFilter Include { get; set; }
        public bool ShowUnpublish { get; set; }
        public bool SortListByTitle { get; set; }

        public EditableItemSelectionWithValidationAttribute()
        {
            LinkedType = typeof(ContentItem);
            ExcludedType = typeof(ISystemNode);
            SearchTreshold = 10;
            ListItemsBelowCurrentStartPageOnly = false;
            Include = EditableItemSelectionFilter.Pages;
            ShowUnpublish = false;
            SortListByTitle = false;
        }

        public EditableItemSelectionWithValidationAttribute(Type linkedType)
            : this()
        {
            LinkedType = linkedType;
        }

        public EditableItemSelectionWithValidationAttribute(Type linkedType, string title, int sortOrder)
            : this(linkedType)
        {
            Title = title;
            SortOrder = sortOrder;
        }

        protected override string GetValue(ContentItem item)
        {
            return item.ID.ToString(CultureInfo.InvariantCulture);
        }

        protected override object ConvertToValue(string value)
        {
            int id;
            if (!int.TryParse(value, out id))
            {
                throw new Exception(string.Format("Invalid id: {0}", value));
            }

            return id;
        }

        protected override ListItem[] GetListItems()
        {
            ParameterCollection query = new ParameterCollection();

            if (!ShowUnpublish)
                query &= Parameter.Equal("State", ContentState.Published);


            if (ListItemsBelowCurrentStartPageOnly)
            {
                var selection = new SelectionUtility(HttpContext.Current, Engine);
                ContentItem item = selection.SelectedItem;
                var startPage = Find.ClosestOf<IStartPage>(item);
                if (startPage == null)
                    return new ListItem[] { }; //No startpage found. Return empty list. 

                if (startPage.VersionOf.HasValue)
                    startPage = startPage.VersionOf.Value;

                query &= Parameter.Below(startPage);
            }

            if (LinkedType != null && LinkedType != typeof(ContentItem))
                query &= Parameter.TypeEqual(LinkedType);

            if (ExcludedType != null)
                query &= Parameter.TypeNotIn(Engine.Definitions.GetDefinitions().Where(d => ExcludedType.IsAssignableFrom(d.ItemType)).Select(d => d.Discriminator).ToArray());

            if (!Is(EditableItemSelectionFilter.Pages))
                query &= Parameter.IsNotNull("ZoneName");
            if (!Is(EditableItemSelectionFilter.Parts))
                query &= Parameter.IsNull("ZoneName");

            var items = Engine.Content.Search.Repository.Select(query, "ID", "Title");

            var listItems = items.Select(row => new ListItem((string)row["Title"], row["ID"].ToString())).ToList();
            
            if (SortListByTitle)
            {
                listItems.Sort((item1, item2) => item1.Text.CompareTo(item2.Text));
            }
            if (Required)
            {
                listItems.Insert(0, new ListItem("- Select -", "0"));
            }

            return listItems.ToArray();
        }

        private bool Is(EditableItemSelectionFilter filter)
        {
            return (filter & Include) == filter;
        }

        protected virtual IEnumerable<ContentItem> GetDataItemsByIds(params int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Enumerable.Empty<ContentItem>();

            var items = Engine.Content.Search.Repository.Find(Parameter.In("ID", ids.Select(id => (object)id).ToArray()));

            return items;
        }

        public override void UpdateEditor(ContentItem item, System.Web.UI.Control editor)
        {
            var linkedItems = GetStoredSelection(item);

            var checkboxList = editor as ListControl;
            if (checkboxList == null) return;

            foreach (ListItem checkboxItem in checkboxList.Items)
            {
                int checkboxValue;
                if (int.TryParse(checkboxItem.Value, out checkboxValue))
                {
                    checkboxItem.Selected = linkedItems.Contains(checkboxValue);
                }
            }
        }

        public override bool UpdateItem(ContentItem item, System.Web.UI.Control editor)
        {
            var checkboxList = editor as ListControl;
            if (checkboxList == null) return false;

            // Get a map of currently linked items
            var storedItems = GetStoredSelection(item);

            // Get a map of all selected items from UI
            var selectedLinkedItems = (from ListItem checkboxItem in checkboxList.Items
                                       where checkboxItem.Selected
                                       where !string.IsNullOrEmpty(checkboxItem.Value)
                                       select (int)ConvertToValue(checkboxItem.Value)).ToArray();

            // Check whether there were any changes
            var hasChanges = storedItems.All(selectedLinkedItems.Contains) == false
                || selectedLinkedItems.All(storedItems.Contains) == false
                || storedItems.Count != selectedLinkedItems.Length;

            // Only hook up items when there were changes
            if (hasChanges)
            {
                // Convert id array to ContentItems
                var linksToAdd = GetDataItemsByIds(selectedLinkedItems);

                ReplaceStoredValue(item, linksToAdd);
            }

            return hasChanges;
        }

        protected virtual void ReplaceStoredValue(ContentItem item, IEnumerable<ContentItem> linksToReplace)
        {
            item[Name] = linksToReplace.FirstOrDefault();
        }

        protected virtual HashSet<int> GetStoredSelection(ContentItem item)
        {
            var storedSelection = new HashSet<int>();

            var referencedItem = item[Name] as ContentItem;
            if (referencedItem != null)
                storedSelection.Add(referencedItem.ID);

            return storedSelection;
        }

        public override void Write(ContentItem item, string propertyName, System.IO.TextWriter writer)
        {
            var referencedItem = item[propertyName] as ContentItem;

            if (referencedItem != null)
            {
                DisplayableAnchorAttribute.GetLinkBuilder(item, referencedItem, propertyName, null, null).WriteTo(writer);
            }
        }

        /// <summary>Adds a required field validator.</summary>
        /// <param name="container">The container control for this validator.</param>
        /// <param name="editor">The editor control to validate.</param>
        protected override Control AddRequiredFieldValidator(Control container, Control editor)
        {
            var composite = (DropDownListWithValidation)editor;
            if (composite == null)
                return null;

            var cv = new CustomValidator()
            {
                ID = Name + "_cv",
                ControlToValidate = composite.ID,
                Display = ValidatorDisplay.Dynamic,
                Text = GetLocalizedText("RequiredText") ?? RequiredText,
                ErrorMessage = GetLocalizedText("RequiredMessage") ?? RequiredMessage
            };

            cv.ServerValidate += (object source, ServerValidateEventArgs args) =>
            {
                if (string.IsNullOrWhiteSpace(composite.SelectedValue) || composite.SelectedValue.Equals("0"))
                {
                    args.IsValid = false;
                }
            };

            editor.Controls.Add(cv);

            return cv;
        }
    }
}
