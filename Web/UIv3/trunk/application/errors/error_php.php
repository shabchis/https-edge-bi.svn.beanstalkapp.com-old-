<style type="text/css">

body {
background-color:	#fff;

font-family:		Verdana;
font-size:			14px;
color:				#000;
}

#content  {
color:				#333;
background-color:	#fff;
padding:			17px 380px 0px 90px;
}
h1 {
font-weight:		bold;
font-size:			14px;
color:				red !important;
margin: 			0 0 4px 0;
border-bottom:1px solid #B1B1B1;
padding-bottom:5px;
}

h4{
font-size:12px;
}
</style>

<div id="content">
	<h1>Edge.BI Error</h1>
	<h4>A PHP Error was encountered</h4>
	
	<p>Severity: <?php echo $severity; ?></p>
	<p>Message:  <?php echo $message; ?></p>
	<p>Filename: <?php echo $filepath; ?></p>
	<p>Line Number: <?php echo $line; ?></p>

</div>