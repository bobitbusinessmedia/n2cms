<%@ Page Language="C#" MasterPageFile="../../Content/Framed.Master" AutoEventWireup="true" CodeBehind="Directory.aspx.cs" Inherits="N2.Edit.FileSystem.Directory1" %>
<%@ Register TagPrefix="edit" TagName="FileUpload" Src="FileUpload.ascx" %>
<%@ Register TagPrefix="edit" Namespace="N2.Edit.Web.UI.Controls" Assembly="N2.Management" %>
<%@ Import Namespace="N2.Web" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
	<%--Upgrading default version of bootstrap breaks the cms layout. Newer version added as needed.--%>
	<link href="//maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css" type="text/css" rel="stylesheet" />
</asp:Content>

<asp:Content ContentPlaceHolderID="Toolbar" runat="server">
	<edit:ButtonGroup runat="server" CssClass="btn btn-danger">
		<asp:LinkButton ID="btnDelete" runat="server" Text="Delete selected" CssClass="command primary-action" OnCommand="OnDeleteCommand" OnClientClick="return confirm('Delete selected files and folders?');" meta:resourceKey="btnDelete" />
        <asp:LinkButton ID="btnAdd" runat="server" Text="Add selected" CssClass="command primary-action" OnCommand="OnAddCommand" OnClientClick="return confirm('Add selected files?');" meta:resourceKey="btnAdd" Visible="false"/>
		<asp:HyperLink ID="hlEdit" runat="server" Text="Edit" CssClass="command edit" meta:resourceKey="hlEdit" />
        <asp:HyperLink ID="hlCancel" runat="server" Text="Close" CssClass="btn" meta:resourceKey="hlCancel" Visible="false"/>
	</edit:ButtonGroup>
</asp:Content>
<asp:Content ContentPlaceHolderID="Content" runat="server">
	<h1 style="float:left;margin-top:0;"><% foreach (N2.ContentItem node in ancestors) {
                var url = Url.Parse("Directory.aspx").AppendSelection(node);
        %>/<a href="<%= string.Format("{0}{1}{2}", url, string.IsNullOrEmpty(ParentQueryString) ? "" : url.ToString().Contains("?") ? "&" : "?", ParentQueryString) %>"><%= node.Title %></a><% } %></h1>
	<span class="input-group-btn" style="text-align:right;padding:0 10px 10px 0;">
        <button id="btn-reload" class="btn btn-default" type="button" title="Reload" style="margin-right:25px;"><span class="glyphicon glyphicon-repeat" style="margin-right:5px;"></span> Reload</button>
        <button id="btn-view-grid" class="btn btn-default" type="button" title="Grid View"><span class="glyphicon glyphicon-th"></span></button>
        <button id="btn-view-list" class="btn btn-default" type="button" title="List View"><span class="glyphicon glyphicon-list"></span></button>
		<script>
			$(function () {
				$("#btn-view-grid").click(function (e) {
					e.preventDefault();
					$("#directory-container").removeClass('upload-folder');
				});
				$("#btn-view-list").click(function (e) {
					e.preventDefault();
                    $("#directory-container").addClass('upload-folder');
                });
                $("#btn-reload").click(function (e) {
                    e.preventDefault();
                    $(this).prop("disabled", true);//prevent multiple clicks
                    $("#btn-reload .glyphicon-repeat").addClass('spinning');//start spinning animation
                    $.post('/filesystemreload.n2.ashx', { action: 'filesystemreload', selected: '<%=(Selection.SelectedItem as N2.Edit.FileSystem.Items.Directory)?.LocalUrl%>' }, function () {
                        location.reload();//reload the page after success refresh
                    }).always(function () {
                        //do the following after reload ajax call finish regardless of success or fail.
                        $("#btn-reload .glyphicon-repeat").removeClass('spinning');//stop spinning animation
                        $(this).prop("disabled", false);//re-enable reload button
                    });
                });
			});
        </script>
    </span>
	<div style="clear:both;"></div>
	<div class="tabPanel" data-flag="Unclosable">
        <edit:PermissionPanel id="ppPermitted" RequiredPermission="Write" runat="server" meta:resourceKey="ppPermitted">
			<edit:FileUpload runat="server" />
		</edit:PermissionPanel>

        <div id="directory-container" class="directory cf">
            <a href ="<%= GetEditUrl() %>">
            <div data-i="0" class="file create-new-folder">
                <span class="file-ic glyphicon glyphicon-folder-plus"></span>
                <label>Create a New Folder</label>
            </div>
            </a>
		    <asp:Repeater ID="rptDirectories" runat="server">
			    <ItemTemplate>
				    <div class="file">
					    <label>
                            <%if (!IsMultiUpload && IsAllowed) { %>
						        <input name="directory" value="<%# Eval("Path") %>" type="checkbox" />
                            <%} %>
						    <asp:Image ImageUrl='<%# Eval("IconUrl") %>' runat="server" />
					    </label>
						<edit:ItemLink DataSource="<%# Container.DataItem %>" InterfaceUrl="Directory.aspx" ParentQueryString='<%# ParentQueryString %>' runat="server" />
				    </div>
			    </ItemTemplate>
		    </asp:Repeater>
		
		    <asp:Repeater ID="rptFiles" runat="server">
			    <ItemTemplate>
				    <div class="file">
					    <label style='<%# ImageBackgroundStyle((string)Eval("LocalUrl")) %>'>
                            <%if (IsMultiUpload || IsAllowed) { %>
                                <input name="file" value="<%# Eval("LocalUrl") %>" type="checkbox" />
                            <% } %>
					    </label>
						<edit:ItemLink DataSource="<%# Container.DataItem %>" InterfaceUrl="File.aspx" runat="server" />
				    </div>
			    </ItemTemplate>
		    </asp:Repeater>
	    </div>
    </div>
</asp:Content>
