<%@ Page MasterPageFile="~/default.Master" Language="C#" Inherits="Easynet.Edge.UI.WebPages.OrganicPage" Codebehind="Organic.aspx.cs" EnableEventValidation="false" %>

<%@ Register Assembly="ZedGraph.Web" Namespace="ZedGraph.Web" TagPrefix="zed" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Easynet.Edge.Core.Configuration" %>
<%@ Import Namespace="Easynet.Edge.Core.Data" %>

<asp:Content ContentPlaceHolderID="headPlaceHolder" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptPlaceHolder" runat="server">
	<script type="text/javascript">
		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(pageLoad);

		function pageLoad()
		{
			$get('<%= _dateSelector.ClientID %>').onchange = UpdateCompareDate;
		}

		function UpdateCompareDate()
		{
			var dateSelector = $get('<%= _dateSelector.ClientID %>');
			var compareSelector = $get('<%= _compareSelector.ClientID %>');
			var targetSelection = dateSelector.length - 1;
			if (dateSelector.selectedIndex < dateSelector.length - 1)
				targetSelection = dateSelector.selectedIndex + 1;
			compareSelector.selectedIndex = targetSelection;
		}

		function $get(id)
		{
			return document.getElementById(id);
		}
		function confirmKwByTime()
		{
			if (confirm("This operation might take a few minutes. Proceed anyway? "))
			{
				document.getElementById('_processing').innerHTML = "processing...";
				return true;
			}
			else
			{
				document.getElementById('_processing').innerHTML = "";
				return false;
			}
		}
					
	</script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="SelectorsPlaceHolder">
	<div class="selector">
		<div class="label">Profile</div>
		<div class="control">
			<asp:DropDownList runat="server" id="_profileSelector"  OnSelectedIndexChanged="_profileSelector_SelectedIndexChanged" AutoPostBack="true" />
		</div>
	</div>
	
	<asp:UpdatePanel ChildrenAsTriggers="true" runat="server">
		<Triggers>
			<asp:AsyncPostBackTrigger ControlID="_profileSelector" EventName="SelectedIndexChanged"/>
		</Triggers>
		
		<ContentTemplate>
			<div class="selector">
				<div class="label">Sample date</div>
				<div class="control">
					<asp:DropDownList runat="server" id="_dateSelector" />	
				</div>
			</div>
			
			<div class="selector">
				<div class="label">Compare with</div>
				<div class="control">
					<asp:DropDownList runat="server" id="_compareSelector" />	
				</div>
			</div>					
		</ContentTemplate>
	</asp:UpdatePanel>
	
	<asp:Button runat="server" ID="_submit" Text="Go"/>
						
		<asp:Button runat="server" ID="_export" Text="Export" />
		<asp:Button runat="server" ID="_exportAll" Text="Export all"/>
		<asp:Button runat="server" ID="_exportAllByTime" Text="Export Kw By Time" OnClientClick="return confirmKwByTime()"/>
        
		<asp:Label runat="server" ID="_failureMsg" Visible="false"/>
        <asp:Label ID="_warning" runat="server" />
	    <asp:Label ID="_processing" runat="server" />
	
</asp:Content>


<asp:Content ContentPlaceHolderID="ResultsPlaceHolder" runat="server">

	<asp:Repeater runat="server" ID="_profileSearchEngineRepeater">
		<HeaderTemplate>
			<h1><%# _profileName %></h1>
		</HeaderTemplate>
		<ItemTemplate>
			<h2><%# RankingTable(Container).SearchEngineName  %></h2>
			<table>
				<thead>
					<tr>
						<th colspan="2">Keyword</th>
						<asp:Repeater runat="server" DataSource="<%# RankingTable(Container).Columns %>">
							<ItemTemplate>
								<th><%# Container.DataItem as string %></th>
							</ItemTemplate>
						</asp:Repeater>
					</tr>
				</thead>
				<tbody>
					<asp:Repeater runat="server" DataSource="<%# RankingTable(Container).Rows %>">
						<ItemTemplate>
							<tr>
								<!-- Keyword -->
								<td>
									<asp:PlaceHolder runat="server" Visible="<%# RankingTable(Container.Parent.Parent).SearchEngineUrl != null %>">
										<a href='<%# Server.HtmlEncode(RankingTable(Container.Parent.Parent).SearchEngineUrl) %>' target='_blank'><i><%# Server.HtmlEncode(RankingRow(Container).Keyword)%></i></a>
									</asp:PlaceHolder>
									<asp:PlaceHolder runat="server" Visible="<%# RankingTable(Container.Parent.Parent).SearchEngineUrl == null %>">
										<i><%# Server.HtmlEncode(RankingRow(Container).Keyword) %></i>
									</asp:PlaceHolder>
								</td>
								<td>
									<asp:PlaceHolder runat="server" Visible="<%# RankingRow(Container).ClientItem != null %>">
										<zed:ZedGraphWeb runat="server" Width="105" Height="40"/>
									</asp:PlaceHolder>
								</td>
								
								<!-- Client rankings -->
								<asp:Repeater DataSource="<%# RankingRow(Container).ClientItem.Rankings %>" runat="server">
									<HeaderTemplate>
										<td>
											<table cellpadding='1'>
									</HeaderTemplate>
									<ItemTemplate>
										<tr>
											<td><b><%# Ranking(Container).Rank %></b></td>
											<td style='width: 30px'>
												<asp:PlaceHolder runat="server" Visible="<%# Ranking(Container).RankDiff == 0 %>">
													&nbsp;
												</asp:PlaceHolder>
												<asp:PlaceHolder runat="server" Visible="<%# Ranking(Container).RankDiff > 0 %>">
													<span style='color: green'>(+<%# Ranking(Container).RankDiff%>)</span>
												</asp:PlaceHolder>
												<asp:PlaceHolder runat="server" Visible="<%# Ranking(Container).RankDiff < 0 %>">
													<span style='color: red'>(<%# Ranking(Container).RankDiff%>)</span>
												</asp:PlaceHolder>
											</td>
											<td>
												<a href='<%# Server.HtmlEncode(Ranking(Container).Url) %>' target='_blank'><%#
													Server.HtmlEncode(Ranking(Container).Url.Length <= 40 ? Ranking(Container).Url : Ranking(Container).Url.Substring(0, 40) + "...")%></a>
											</td>
										</tr>
									</ItemTemplate>
									<FooterTemplate>
											</table>
										</td>
									</FooterTemplate>
								</asp:Repeater>
								
								<!-- Competitor rankings -->
								<asp:Repeater runat="server" DataSource="<%# RankingTable(Container.Parent.Parent).Columns %>">
									
									<ItemTemplate>
										<td>
											<asp:Repeater runat="server" DataSource="<%# RankingItem(Container).Rankings %>">
												<ItemTemplate>
													<a href='<%# Server.HtmlEncode(Ranking(Container).Url) %>' target='_blank'>Ranking(Container).Rank</a>
													<asp:PlaceHolder runat="server" Visible="<%# Ranking(Container).RankDiff != 0 %>">
														<span style='color: #999; font-size: 8px'>(<%# (Ranking(Container).RankDiff > 0 ? "+" : string.Empty) + Ranking(Container).RankDiff.ToString()%>)</span>
													</asp:PlaceHolder>
													<%# Container.ItemIndex < (Container.Parent as Repeater).Items.Count-1 ? ", " : string.Empty %>
												</ItemTemplate>
											</asp:Repeater>
										</td>
									</ItemTemplate>
								</asp:Repeater>
							</tr>
						</ItemTemplate>
					</asp:Repeater>
				</tbody>
			</table>
		</ItemTemplate>
	</asp:Repeater>

	<asp:PlaceHolder ID="GraphsHolder" runat="server" Visible="false"></asp:PlaceHolder>
	
</asp:Content>