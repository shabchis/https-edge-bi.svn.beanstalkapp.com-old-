<%@ Page Title="" Language="C#" MasterPageFile="~/default.Master" AutoEventWireup="true" %>

<asp:Content ContentPlaceHolderID="HeadPlaceHolder" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptPlaceHolder" runat="server">
 
	<%
		string _codebase = 
			String.Format("{0}://{1}/Intelligence", Request.Url.Scheme, Request.Url.Host);
	%>
	
	<script type="text/javascript">
		var _codebase_ = '<%= _codebase %>';
	</script>
	<script type="text/javascript" src="<%= _codebase %>/js/novaview.js" ></script>

	<%
		// Get the encrypted request parameter and parse into settings collection
		Easynet.Edge.Core.SettingsCollection settings = Request[null] == null ? new Easynet.Edge.Core.SettingsCollection() :
			new Easynet.Edge.Core.SettingsCollection(Easynet.Edge.Core.Utilities.Encryptor.Decrypt(Request[null]));

		// Aggregate parameter errors here seperated by |
		string _startupFail = null;

		string accountID = null;
		if (settings.ContainsKey("accountID"))
			accountID = settings["accountID"].Replace(@"\", @"\\");
		else
			_startupFail += "Account ID not defined.|";
		
		
		string role = null;
		if (settings.ContainsKey("role"))
			role = settings["role"].Replace(@"\", @"\\");
		else
			_startupFail += "Role not defined.|";

		string book = null;
		if (settings.ContainsKey("book"))
			book = settings["book"].Replace(@"\", @"\\");
		else
			_startupFail += "Book not defined.|";
	%>
	
	<div id="failMessage" class="errorMessage" style="display: none"></div>

	<script type="text/javascript">

		var _startupFail = '<%= _startupFail %>';

		// Test for presence of panorama js
		if (typeof (BeginNovaView) == 'undefined')
			_startupFail = _startupFail + 'Analysis module not found.|';
		
		if (_startupFail != '')
		{
			// Startup errors detected
			var text  = _startupFail.replace(/\|/gi, '<br/>');
			with($get('failMessage'))
			{
				style.display = '';
				innerHTML = text;
			}
		}
		else
		{
			BeginNovaView("100%", "99%", "pnbookapplet", 1);

			AttachParameter("Alias", '<%= book %>');
			AttachParameter("Slicers", '[C];[Accounts Dim].[Accounts].[Account].&[<%= accountID %>];[H][Accounts Dim]');
			AttachParameter("Language", "en");
			AttachParameter("banner", "true");
			AttachParameter("Flags", "1");
			AttachParameter("User", '<%= role %>');
			AttachParameter("Buttons", "1,6760,280,420,430,470,480,9220,490,500,510");
			AttachParameter("Menus", "0,2370,300,3800,7310");
			//AttachParameter(“Settings”, ”Skin=EdgeBI");
			//AttachParameter("Show","Toolbar;Header;Slicers;grid;PrBrBook;BrBook;")

			EndNovaView();
		}
		

		function OnLoadEvent(appletName)
		{
			//alert('The applet ' + appletName + ' has finished loading.')
			//document.all.pnbookapplet.CallSetSlicerPlacement(0)

			//Toggle legend book  to closed


			//Toggle slicer  to open
			document.all.pnbookapplet.CallSetViewLayout(64)
			document.all.pnbookapplet.CallSetViewLayout(64)

			//Toggle private briefing book  to open
			//document.all.pnbookapplet.CallSetViewLayout(32)
			//document.all.pnbookapplet.CallSetViewLayout(32)

			//Toggle briefing book to open
			//document.all.pnbookapplet.CallSetViewLayout(16)
			//document.all.pnbookapplet.CallSetViewLayout(16)
		}

	</script>

</asp:Content>

<asp:Content ContentPlaceHolderID="SelectorsPlaceHolder" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="ResultsPlaceHolder" runat="server">
</asp:Content>
