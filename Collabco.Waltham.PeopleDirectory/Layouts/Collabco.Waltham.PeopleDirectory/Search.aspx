<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Collabco.Waltham.PeopleDirectory.Layouts.Collabco.Waltham.PeopleDirectory.Search" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    
<script src="/_layouts/15/init.js" type="text/javascript"></script>
<style type="text/css">

.search_boxes
{
  -moz-border-radius: 10px;
 border-radius: 10px;
    border:solid 1px black; 
    padding:10px;
    background: url("/_layouts/15/Collabco/icons/toolbar_find.png") right no-repeat ;
    background-size: 20px 20px;
    height:15px;
    padding-right:40px;
}

 #sideNavBox {DISPLAY: none; width:0px}
    #titleAreaBox {
        display:none;
        width:0px;
        height:0px;
    }
 #contentBox {MARGIN-LEFT: 5px}
</style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
                <asp:Button Text="Reset Form" runat="server" ID="btnReset" OnClick="btnReset_Click" />
                <asp:Button Text="Print" runat="server" ID="btnPrint" OnClientClick="javascript:window.print(); return false;"/>
    <br /><br />
                <asp:Label runat="server" ID="LabelTableRowHeaderCell" Font-Bold="true"></asp:Label>
                <asp:PlaceHolder ID="PlaceHolderErrorMessage" runat="server"></asp:PlaceHolder>
                <div class="rounded_corners">
                    <asp:GridView ID="GridView1" runat="server" CellPadding="4" ForeColor="#333333" GridLines="Both"
                        ShowHeaderWhenEmpty="true"
                        OnRowDataBound="GridView1_RowDataBound"
                        OnSorting="GridView1_Sorting"
                         OnRowCreated="GridView1_RowCreated"
                         AllowSorting="true">
                    <AlternatingRowStyle BackColor="White"/>
                    <EditRowStyle BackColor="#2461BF" />
                    <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
                    <RowStyle BackColor="#99FFCC"/>
                    <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
                    </asp:GridView>
                </div>
   
        
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
People Search
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
People Search
</asp:Content>
