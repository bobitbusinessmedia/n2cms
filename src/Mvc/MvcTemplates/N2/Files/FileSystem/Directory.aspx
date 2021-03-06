﻿<%@ Page Language="C#" MasterPageFile="../../Content/Framed.Master" AutoEventWireup="true" CodeBehind="Directory.aspx.cs" Inherits="N2.Edit.FileSystem.Directory1" %>
<%@ Register TagPrefix="edit" TagName="FileUpload" Src="FileUpload.ascx" %>
<%@ Register TagPrefix="edit" Namespace="N2.Edit.Web.UI.Controls" Assembly="N2.Management" %>
<%@ Import Namespace="N2.Web" %>
<asp:Content ContentPlaceHolderID="Toolbar" runat="server">
	<edit:ButtonGroup runat="server" CssClass="btn btn-danger">
		<asp:LinkButton ID="btnDelete" runat="server" Text="Delete selected" CssClass="command primary-action" OnCommand="OnDeleteCommand" OnClientClick="return confirm('Delete selected files and folders?');" meta:resourceKey="btnDelete" />
        <asp:LinkButton ID="btnAdd" runat="server" Text="Add selected" CssClass="command primary-action" OnCommand="OnAddCommand" OnClientClick="return confirm('Add selected files?');" meta:resourceKey="btnAdd" Visible="false"/>
		<asp:HyperLink ID="hlEdit" runat="server" Text="Edit" CssClass="command edit" meta:resourceKey="hlEdit" />
        <asp:HyperLink ID="hlCancel" runat="server" Text="Close" CssClass="btn" meta:resourceKey="hlCancel" Visible="false"/>
        
	</edit:ButtonGroup>
</asp:Content>
<asp:Content ContentPlaceHolderID="Content" runat="server">	
	<h1><% foreach (N2.ContentItem node in ancestors) {
                var url = Url.Parse("Directory.aspx").AppendSelection(node);
        %>/<a href="<%= string.Format("{0}{1}{2}", url, string.IsNullOrEmpty(ParentQueryString) ? "" : url.ToString().Contains("?") ? "&" : "?", ParentQueryString) %>"><%= node.Title %></a><% } %></h1>
	<div class="tabPanel" data-flag="Unclosable">
        <div class="directory cf">
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

		<edit:PermissionPanel id="ppPermitted" RequiredPermission="Write" runat="server" meta:resourceKey="ppPermitted">
			<edit:FileUpload runat="server" />
		</edit:PermissionPanel>
    </div>
</asp:Content>
