<%@ Page MasterPageFile="~/default.Master" Language="C#" EnableEventValidation="false"  enableSessionState="true" AutoEventWireup="true" CodeBehind="Alerts.aspx.cs" Inherits="Easynet.Edge.UI.WebPages.AlertsPage" %>
 
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Easynet.Edge.Core.Configuration" %>
<%@ Import Namespace="Easynet.Edge.Core.Data" %>
 
<asp:Content ContentPlaceHolderID="headPlaceHolder" runat="server">
	<style type="text/css">
		html
		{
			width: 100%;
			height: 100%;
		}
		
		body
		{
			width: 100%;
			height: 100%;
		}
		
	</style>

   <link href="<%= ResolveUrl("~/Resources/style.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>


<asp:Content ContentPlaceHolderID="SelectorsPlaceHolder" runat="server"> 
	
	<div class="selector" align="left" >
	    <div class="label" >Period</div>
	    <div class="control">
		    <asp:DropDownList runat="server" id="_flowSelector">
				<asp:ListItem Text="Last day" Value="0"/>
				<asp:ListItem Text="Last 7 days" Value="1" />
				<asp:ListItem Text="Custom" Value="2"/>
		    </asp:DropDownList>
	    </div>
    </div>
	    
    <div class="selector">
		<div class="label">Date</div>
		<div class="control, calendarHost">
			<asp:UpdatePanel runat="server" UpdateMode="Conditional">
				<Triggers>
					<asp:AsyncPostBackTrigger ControlID="_fromDate" EventName="SelectionChanged" />
				</Triggers>
				<ContentTemplate>
					<input type="text" readonly="readonly" value="<%= _fromDate.SelectedDate.ToShortDateString() %>"
						onclick="PopupOpen('<%= _fromDate.ClientID %>', 2)" />
				</ContentTemplate>
			</asp:UpdatePanel>
			<div class="calendar" style="display: none" onclick="if (window.event) window.event.cancelBubble = true; else event.stopPropagation();">
				<asp:UpdatePanel runat="server" UpdateMode="Conditional">
					<ContentTemplate>
						<asp:Calendar runat="server" ID="_fromDate" EnableViewState="true" />						
					</ContentTemplate>
				</asp:UpdatePanel>
			</div>
		</div>
	</div>
        
     <div class="selector">
		<div class="label">Alert level</div>
        <div class="control"> 
            <asp:DropDownList ID="_alertTypeSelector" runat="server">
                <asp:ListItem Selected="True" Value="1">Campaigns</asp:ListItem>
                <asp:ListItem Value="2">Adgroups</asp:ListItem>
                <asp:ListItem Value="3">Keywords</asp:ListItem>
                <asp:ListItem Value="4">Texts</asp:ListItem>
                <asp:ListItem Value="5">Gateways</asp:ListItem>           
            </asp:DropDownList>
        </div>
    </div>
        
	<asp:Button runat="server" ID="_submit" class="goButton" Text="Go" OnClick="_submit_Click"/>
	     
    <asp:RequiredFieldValidator runat="server" ControlToValidate="_alertTypeSelector" ErrorMessage="Please select an Item" /> 
	<asp:Label runat="server" ID="_currentDateMessage" Visible="false"/> 		 
	<asp:Label runat="server" ID="_baseDateMessage" Visible="false"/>
 
</asp:Content>
				
				
<asp:Content ContentPlaceHolderID="ResultsPlaceHolder" runat="server">
	<asp:Label runat="server" ID="_reportTitle" Visible="true" />
		
	<asp:DataGrid runat="server" ID="_summaryGrid" CssClass="resultsGrid" AllowSorting="true"  EnableViewState="false"
		OnSortCommand="_summaryGrid_SortCommand"
		OnItemDataBound="_summaryGrid_ItemDataBound"
		/>
		
	<asp:DataGrid runat="server" ID="_dataGrid" CssClass="resultsGrid" AllowSorting="true"  EnableViewState="false"
		OnSortCommand="_dataGrid_SortCommand"
		OnItemDataBound="_dataGrid_ItemDataBound"
	/>
</asp:Content>