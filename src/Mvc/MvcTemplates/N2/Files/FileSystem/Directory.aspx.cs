using N2.Collections;
using N2.Configuration;
using N2.Edit.FileSystem.Items;
using N2.Edit.Navigation;
using N2.Edit.Versioning;
using N2.Edit.Web;
using N2.Engine;
using N2.Persistence;
using N2.Resources;
using N2.Web;
using N2.Web.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace N2.Edit.FileSystem
{
    public partial class Directory1 : EditPage
	{
		protected bool IsMultiUpload, IsAllowed;
		protected string ParentQueryString = "";
		private string targetType, targetProperty, targetID, targetDomain, targetZone, selected, useDefaultUploadDirectory, versionIndex, versionKey = "";
		protected string CustomImagePath { get; set; }
		protected string CustomThumbResizePattern { get; set; }
		protected bool UseCustomResizing { get; set; }
		protected string ReturnTabId { get; set; }

		protected override void RegisterToolbarSelection()
		{
			string script = GetToolbarSelectScript("preview");
			Register.JavaScript(this, script, ScriptPosition.Bottom, ScriptOptions.ScriptTags);
		}

		protected IEnumerable<ContentItem> ancestors;

		IList<Directory> directories;
		IList<File> files;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			IsMultiUpload = !string.IsNullOrEmpty(Request.QueryString["TargetType"]);
			selected = Request.QueryString["selected"];
			useDefaultUploadDirectory = Request.QueryString["UseDefaultUploadDirectory"];

			if (!string.IsNullOrEmpty(selected) && useDefaultUploadDirectory != null && useDefaultUploadDirectory.ToLower() == "true")
			{
				//Find and/or create default upload directory for templated content.
				IFileSystem FS = Engine.Resolve<IFileSystem>();
				var host = Engine.Resolve<IHost>();
				var rootItem = Engine.Persister.Get(host.DefaultSite.RootItemID);
				var root = new HierarchyNode<ContentItem>(rootItem);
				var selectionTrail = new List<ContentItem>();

				var uploadDirectories = MediaBrowserUtils.GetAvailableUploadFoldersForAllSites(Context, root, selectionTrail, Engine, FS);

				if (uploadDirectories.Count == 1)
				{
					var dir = uploadDirectories[0].Current as Directory;

					if (!string.IsNullOrWhiteSpace(selected))
					{
						var segments = (selected.Split(new[] { '/' }, 4, StringSplitOptions.RemoveEmptyEntries));
						if (segments.Length == 4)
						{
							var siteName = segments[1];
							var uploadSiteFolder = dir.GetDirectories().FirstOrDefault(d => d.Name == siteName.ToLower());
							if (uploadSiteFolder == null)
							{
								var newDir = VirtualPathUtility.AppendTrailingSlash(Request.ApplicationPath + dir.LocalUrl) + siteName.ToLower();
								FS.CreateDirectory(newDir);
								uploadSiteFolder = dir.GetDirectories().FirstOrDefault(d => d.Name == siteName.ToLower());
							}

							var contentName = segments[2];
							var uploadSiteContentFolder = uploadSiteFolder.GetDirectories().FirstOrDefault(d => d.Name == contentName.ToLower());
							if (uploadSiteContentFolder == null)
							{
								var newDir = VirtualPathUtility.AppendTrailingSlash(Request.ApplicationPath + uploadSiteFolder.LocalUrl) + contentName.ToLower();
								FS.CreateDirectory(newDir);
								uploadSiteContentFolder = uploadSiteFolder.GetDirectories().FirstOrDefault(d => d.Name == contentName.ToLower());
							}

							var templateName = segments[3];
							var uploadSiteContentTemplateFolder = uploadSiteContentFolder.GetDirectories().FirstOrDefault(d => d.Name == templateName.ToLower());
							if (uploadSiteContentTemplateFolder == null)
							{
								var newDir = VirtualPathUtility.AppendTrailingSlash(Request.ApplicationPath + uploadSiteContentFolder.LocalUrl) + templateName.ToLower();
								FS.CreateDirectory(newDir);
								uploadSiteContentTemplateFolder = uploadSiteContentFolder.GetDirectories().FirstOrDefault(d => d.Name == templateName.ToLower());

								Selection.SelectedItem = uploadSiteContentTemplateFolder;
							}
							else
							{
								Selection.SelectedItem = uploadSiteContentTemplateFolder;
							}

                            var monthName = DateTime.Now.ToString("yyyy-MM");
                            var uploadSiteContentTemplateMonthFolder = uploadSiteContentTemplateFolder.GetDirectories().FirstOrDefault(d => d.Name == monthName.ToLower());
                            if (uploadSiteContentTemplateMonthFolder == null)
                            {
                                var newDir = VirtualPathUtility.AppendTrailingSlash(Request.ApplicationPath + uploadSiteContentTemplateFolder.LocalUrl) + monthName.ToLower();
                                FS.CreateDirectory(newDir);
                                uploadSiteContentTemplateMonthFolder = uploadSiteContentTemplateFolder.GetDirectories().FirstOrDefault(d => d.Name == monthName.ToLower());

                                Selection.SelectedItem = uploadSiteContentTemplateMonthFolder;
                            }
                            else
                            {
                                Selection.SelectedItem = uploadSiteContentTemplateMonthFolder;
                            }

                        }
					}
				}
			}


			Page.StyleSheet("{ManagementUrl}/Files/Css/Files.css");

			ancestors = Find.EnumerateParents(Selection.SelectedItem, null, true).Where(a => a is AbstractNode).Reverse();

			var config = new ConfigurationManagerWrapper();
			var authorizationToDelete = config.Sections.Management.UploadFolders.RequiredPermissionToDelete;
			var authorizationToModify = config.Sections.Management.UploadFolders.RequiredPermissionToModify; //Edit (Rename Directory), copy, paste

			CustomImagePath = config.Sections.Management.Images.CustomImagePath;
			CustomThumbResizePattern = config.Sections.Management.Images.CustomThumbResizePattern;
			UseCustomResizing = config.Sections.Management.Images.UseCustomResizing;
			IsAllowed = btnDelete.Enabled = btnDelete.Visible = Engine.SecurityManager.IsAuthorized(User, Selection.SelectedItem, authorizationToDelete);
			hlEdit.Visible = Engine.SecurityManager.IsAuthorized(User, Selection.SelectedItem, authorizationToModify);


			hlEdit.NavigateUrl = Engine.ManagementPaths.GetEditExistingItemUrl(Selection.SelectedItem);

			// EditableMultiUploadButtonAttribute
			if (IsMultiUpload)
			{
				targetType = Request.QueryString["TargetType"];
				targetProperty = Request.QueryString["TargetProperty"];
				targetID = Request.QueryString["TargetID"];
				targetZone = Request.QueryString["TargetZone"];
				targetDomain = Request.QueryString["TargetDomain"];
				versionIndex = Request.QueryString["VersionIndex"] ?? "";
				versionKey = Request.QueryString["VersionKey"] ?? "";
				ReturnTabId = Request.QueryString["ReturnTab"] ?? "";
			

				if (!string.IsNullOrEmpty(targetType) && !string.IsNullOrEmpty(targetProperty) && !string.IsNullOrEmpty(targetID))
				{
					ParentQueryString = string.Format("TargetType={0}&TargetProperty={1}&TargetID={2}{3}{4}{5}{6}{7}",
						targetType, targetProperty, targetID,
						string.IsNullOrEmpty(targetZone) ? "" : "&TargetZone=" + targetZone,
						string.IsNullOrEmpty(targetDomain) ? "" : "&TargetDomain=" + targetDomain,
						string.IsNullOrEmpty(versionIndex) ? "" : "&VersionIndex=" + versionIndex,
						string.IsNullOrEmpty(versionKey) ? "" : "&VersionKey=" + versionKey,
						string.IsNullOrEmpty(ReturnTabId) ? "" : "&ReturnTab=" + ReturnTabId
					);
				}

				//Get item by id.
					ContentItem item = Find.Items.Where.ID.Eq(int.Parse(targetID)).Select().First();

				//Get a specific version of the item if version index and key are provided.
				if (!string.IsNullOrWhiteSpace(versionIndex) && !string.IsNullOrWhiteSpace(versionKey))
				{
					var cvr = Engine.Resolve<ContentVersionRepository>();
					item = cvr.ParseVersion(versionIndex, versionKey, item);
				}

				var navigateUrl = Engine.ManagementPaths.GetEditExistingItemUrl(item);
				if (!string.IsNullOrEmpty(ReturnTabId)) navigateUrl = "#" + ReturnTabId;

				hlCancel.NavigateUrl = navigateUrl;

				btnDelete.Visible = hlEdit.Visible = false;
				btnAdd.Visible = hlCancel.Visible = true;
			}

			Reload();

			Refresh(Selection.SelectedItem, ToolbarArea.Navigation, force: false);
		}

		private void Reload()
		{
			var dir = Selection.SelectedItem as Directory;
			if (dir == null) return;

			directories = dir.GetDirectories();
			files = dir.GetFiles();

			rptDirectories.DataSource = directories;
			rptFiles.DataSource = files;
			DataBind();
		}

		public void OnDeleteCommand(object sender, CommandEventArgs args)
		{
			Delete(Request.Form["directory"], directories.Select(f => f.Url), Engine.Resolve<IFileSystem>().DeleteDirectory);
			Delete(Request.Form["file"], files.Select(f => f.Url), Engine.Resolve<IFileSystem>().DeleteFile);
		}

		public void OnAddCommand(object sender, CommandEventArgs args)
		{
			targetType = Request.QueryString["TargetType"];
			targetProperty = Request.QueryString["TargetProperty"];
			targetID = Request.QueryString["TargetID"];
			targetZone = Request.QueryString["TargetZone"] ?? "";
			targetDomain = Request.QueryString["TargetDomain"] ?? "";
			versionIndex = Request.QueryString["VersionIndex"] ?? "";
			versionKey = Request.QueryString["VersionKey"] ?? "";
			ReturnTabId = Request.QueryString["ReturnTab"] ?? "";
			if (!string.IsNullOrEmpty(ReturnTabId))
				ReturnTabId = "#" + ReturnTabId;

			//Get item by id.
			ContentItem item = Find.Items.Where.ID.Eq(int.Parse(targetID)).Select().First();

			//Get a specific version of the item if version index and key are provided.
			if (!string.IsNullOrWhiteSpace(versionIndex) && !string.IsNullOrWhiteSpace(versionKey))
			{
				var cvr = Engine.Resolve<ContentVersionRepository>();
				item = cvr.ParseVersion(versionIndex, versionKey, item);
			}

			var selectedFiles = Request.Form["file"];
			var allowed = files.Select(f => f.Url);

			if (string.IsNullOrEmpty(selectedFiles))
				return;

			var items = selectedFiles.Split(',');

			string[] partType = targetType.Split(',');

			foreach (string file in items.Select(i => i.TrimEnd('/')).Intersect(allowed.Select(a => a.TrimEnd('/'))))
			{
				var newPart = (ContentItem)Activator.CreateInstance(partType[0], partType[1]).Unwrap();

				newPart[targetProperty] = targetDomain + file;
				newPart.State = ContentState.Published;
				newPart.Title = partType[1].Split('.').Last();
				newPart.ZoneName = targetZone;
				newPart.VersionIndex = item.VersionIndex;
				newPart.Parent = item;

				newPart.AddTo(item, targetZone);

				if (item.VersionOf.HasValue)
				{
					var cvr = Engine.Resolve<ContentVersionRepository>();
					cvr.Save(item);
				}
				else
				{
					Engine.Persister.SaveRecursive(item);
				}
			}

			Response.Redirect(Engine.ManagementPaths.GetEditExistingItemUrl(item)+ ReturnTabId);
		}

		private void Delete(string itemsToDelete, IEnumerable<string> allowed, Action<string> deleteAction)
		{
			if (string.IsNullOrEmpty(itemsToDelete))
				return;

			var items = itemsToDelete.Split(',');
			foreach (string item in items.Select(i => i.TrimEnd('/')).Intersect(allowed.Select(a => a.TrimEnd('/'))))
			{
				deleteAction(item);
			}

			Reload();
		}

		protected string ImageBackgroundStyle(string url)
		{
			if (!ImagesUtility.IsImagePath(url))
				return "";

			if (UseCustomResizing && !string.IsNullOrWhiteSpace(CustomThumbResizePattern) && !string.IsNullOrWhiteSpace(CustomImagePath))
			{
				var ThumbnailImageResizeUrl = CustomImagePath + CustomThumbResizePattern;
				return string.Format("background-image:url({0})", N2.Edit.Web.UI.Controls.ResizedImage.GetCustomResizedImageUrl(ThumbnailImageResizeUrl, url));
			}
			else
			{
				return string.Format("background-image:url({0})", N2.Edit.Web.UI.Controls.ResizedImage.GetResizedImageUrl(url, 100, 100, N2.Web.Drawing.ImageResizeMode.Fit));
			}
		}

        protected string GetEditUrl()
        {
            if (Selection.SelectedItem == null) throw new ArgumentNullException("selected");

            ContentItem parent = Selection.SelectedItem;

            Url url = Url.ResolveTokens("{ManagementUrl}/Content/Edit.aspx");
            url = url.AppendQuery(SelectionUtility.SelectedQueryKey, parent.Path);
            url = url.AppendQuery("discriminator", "Directory");      
            return url.AppendQuery("returnUrl", Request["returnUrl"]);
        }

	}
}
