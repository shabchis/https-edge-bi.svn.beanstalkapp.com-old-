<?php
	$codebase = "https://console.edge-bi.com/Intelligence";
?>

<script type="text/javascript">
	var _codebase_ = '<?php echo $codebase ?>';
</script>
<script type="text/javascript" src="<?php echo $codebase ?>/js/novaview.js" ></script>

<div id="failMessage" class="errorMessage" style="display: none"></div>

<script type="text/javascript">

	var _startupFail;
	
	// Test for presence of panorama js
	if (typeof (BeginNovaView) == 'undefined')
		_startupFail = 'Analysis module not found.';
	
	if (_startupFail)
	{
		// Startup errors detected
		$('#failMessage').show().html(_startupFail);
	}
	else
	{
		BeginNovaView("100%", "99%", "pnbookapplet", 1);

		AttachParameter("Alias", '<?php echo $book ?>');
		AttachParameter("Slicers", '[C];[Accounts Dim].[Accounts].<?php echo $level == 2 ? '[Client]' : '[Account]' ?>.&[<?php echo $accountID ?>];[H][Accounts Dim]');
		AttachParameter("Language", "en");
		AttachParameter("banner", "true");
		AttachParameter("Flags", "1");
		AttachParameter("User", '<?php echo addslashes($role) ?>');
		AttachParameter("Buttons", "1,6760,280,420,430,460,470,480,9220,490,500,510");
		AttachParameter("Menus", "0,2370,300,3800,7310");
		//AttachParameter("Settings", "Skin=Edge.BI");
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