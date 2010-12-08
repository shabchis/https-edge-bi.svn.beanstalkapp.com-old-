<%@ Page MasterPageFile="~/default.Master" Language="C#" AutoEventWireup="true" CodeBehind="AdTest.aspx.cs" Inherits="Easynet.Edge.UI.WebPages.AdTest" %>


<asp:Content ContentPlaceHolderID="HeadPlaceHolder" runat="server">

    <title><%= _dbName %> Ad Testing</title>
    
    <style type="text/css">
    	.selectorsContainer
    	{
    	    float: left;
    	}
    	
		#selectors .control select
		{
			width: 130px;
		}
		
		#adgroupCheckboxList
		{
		    height: 400px;
		    width: 250px;
		    overflow: auto;
		    border: 1px solid;
		    position: absolute;
			z-index: 999;
			background-color: #fff;
		}
		
		#thresholdControl
		{
		    height: 90px;
		    width: 320px;
		    overflow: auto;
		    border: 1px solid;
		    position: absolute;
			z-index: 999;
			background-color: #fff;
		}
		
		#thresholdControl .field
		{
		    float: left;
		    margin: 5px;
		}
			
		.goButton
		{
		    float: left;
		    font-weight: bold;
		    font-size: 14px;
		    width: 120px;
		    margin-left: 30px;
		    margin-top: 30px;
		}
		
		#warning
		{
		    color: #f00;
		    clear: both;
		}
		
		.resultsGrid .losing
		{
		    color: Red;
		}
		
		.resultsGrid .winning
		{
		    color: Green;
		}
     
     </style>
     		
    <link href="<%= ResolveUrl("~/Resources/style.css") %>" rel="stylesheet" type="text/css" />
    
</asp:Content>


<asp:Content runat="server" ContentPlaceHolderID="ScriptPlaceHolder">

	<script type="text/javascript">
		var _primaryMeasureID = '<%= _primaryMeasure.ClientID %>';
		var _losingAdThresholdID = '<%= losingAdThreshold.ClientID %>';
		var _primaryThresholdID = '<%= primaryThreshold.ClientID %>';
		var _campaignSelectorID = '<%= _campaignSelector.ClientID %>';
		
		
		//.. TODO: all the rest
	</script>
  <script type="text/javascript" src="Scripts/AdTest.js" ></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="SelectorsPlaceHolder" runat="server">
	<div class="selectorsContainer">
		<div class="selector">
			<div class="label">Channel</div>
			<div class="control">
				<asp:DropDownList ID="_channelSelector" runat="server" AutoPostBack="true" OnSelectedIndexChanged="_channelSelector_SelectedIndexChanged">
				</asp:DropDownList>
			</div>
		</div>
		<div class="selector">
			<div class="label">Campaign</div>
			<div class="control">
				<asp:UpdatePanel runat="server">
					<Triggers>
						<asp:AsyncPostBackTrigger ControlID="_channelSelector" />
					</Triggers>
					<ContentTemplate>
						<asp:DropDownList style="width: auto" ID="_campaignSelector" runat="server" AutoPostBack="true" OnSelectedIndexChanged="_campaignSelector_SelectedIndexChanged">
						</asp:DropDownList>
					</ContentTemplate>
				</asp:UpdatePanel>
			</div>
		</div>
		<div class="selector">
			<div class="label">From</div>
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
							<asp:Calendar runat="server" ID="_fromDate" OnSelectionChanged="_fromDate_SelectionChanged" />						
						</ContentTemplate>
					</asp:UpdatePanel>
				</div>
			</div>
		</div>
		<div class="selector">
			<div class="label">To</div>
			<div class="control, calendarHost">
				<asp:UpdatePanel runat="server" UpdateMode="Conditional">
					<Triggers>
						<asp:AsyncPostBackTrigger ControlID="_toDate" EventName="SelectionChanged" />
					</Triggers>
					<ContentTemplate>
						<input type="text" readonly="readonly" value="<%= _toDate.SelectedDate.ToShortDateString() %>"
							onclick="PopupOpen('<%= _toDate.ClientID %>', 2)" />
					</ContentTemplate>
				</asp:UpdatePanel>
				<div class="calendar" style="display: none" onclick="if (window.event) window.event.cancelBubble = true; else event.stopPropagation();">
					<asp:UpdatePanel runat="server" UpdateMode="Conditional">
						<ContentTemplate>
							<asp:Calendar runat="server" ID="_toDate" OnSelectionChanged="_toDate_SelectionChanged" />						
						</ContentTemplate>
					</asp:UpdatePanel>
				</div>
			</div>
		</div>
		
		<div style="clear: both" ></div>
		
		<div class="selector">
			<div class="label">Primary Measure</div>
			<div class="control">
				<asp:DropDownList ID="_primaryMeasure" runat="server"/>
			</div>
		</div>
		<div class="selector">
			<div class="label">Secondary Measure</div>
			<div class="control">
				<asp:DropDownList ID="_secondaryMeasure" runat="server" />
			</div>
		</div>
		<div class="selector">
			<div class="label">Ad types</div>
			<div class="control">
				<asp:DropDownList ID="_AdsType" runat="server">
					<asp:ListItem Selected="True">Ad-testing</asp:ListItem>
					<asp:ListItem>Non ad-testing</asp:ListItem>
					<asp:ListItem>Both</asp:ListItem>
				</asp:DropDownList>
			</div>
		</div>
		
		<div class="option">
			<div class="label">
				<asp:CheckBox ID="_toExcludeAdGroups" runat="server" Text="Exclude specific adgroups" style="vertical-align: middle"/>
				(<asp:LinkButton ID="adgroupChooseButton" runat="server" OnClientClick="PopupOpen('adgroupCheckboxList')" OnClick="adgroupChooseButton_Click" CssClass="adgroupExcludeLink">select</asp:LinkButton>)
			</div>
			<div class="control">
				
					<asp:UpdatePanel runat="server" UpdateMode="Conditional">
						<Triggers>
							<asp:AsyncPostBackTrigger ControlID="adgroupChooseButton" EventName="Click"/>
						</Triggers>
						<ContentTemplate>
							<div id="adgroupCheckboxList" style="display: none" onclick="if (window.event) window.event.cancelBubble = true; else event.stopPropagation();">
								<asp:CheckBoxList ID="_adgroupList" runat="server" AutoPostBack="true"/>
							</div> 
						</ContentTemplate>
					</asp:UpdatePanel>		
			
			</div>
		</div>
		
		<div class="option">
			<div class="label">
				<asp:CheckBox ID="_showOnlyLosingAds" runat="server" Text="Show Only Losing Ads" style="vertical-align: middle"/>
				(<a href="javascript:OnPrimaryMeasureChanged(); PopupOpen('thresholdControl')">threshold</a>)
			</div>
			<div class="control">
				<div id="thresholdControl" style="display: none" onclick="if (window.event) window.event.cancelBubble = true; else event.stopPropagation();">
					<div class="field">
						<div>Chance-to-Beat Threshold:</div>
						<asp:TextBox ID="losingAdThreshold" runat="server" Width="40px" onkeypress="checkThreshold()"></asp:TextBox> %
					</div>
					<div class="field">
						<div id="primMeasureThreshLabel">Primary Measure Threshold:</div>
						<asp:TextBox ID="primaryThreshold" runat="server" Width="40px" onkeypress="checkThreshold()"></asp:TextBox>
					</div>
					<div id="warning"></div>
				</div> 
			</div>
		</div>
    </div>
	
	<asp:Button runat="server" ID="_submit" class="goButton" Text="Go" OnClientClick="return ConfirmForm()" onclick="_submit_Click" />
</asp:Content>


<asp:Content ContentPlaceHolderID="ResultsPlaceHolder" runat="server">
	
	<asp:DataGrid ID="_DataGrid" runat="server" Width="<%# Unit.Percentage(100) %>" OnSortCommand="_DataGrid_SortCommand" CssClass="resultsGrid" />

</asp:Content>