<style type="text/css">

body {
background-color:	#fff;

font-family:		Verdana;
font-size:			14px;
color:				#000;
}

#content  {
border:				#999 1px solid;
background-color:	#fff;
padding:			20px 20px 12px 20px;
}

h1 {
font-weight:		bold;
font-size:			18px;
color:				red;
margin: 			0 0 4px 0;
border-bottom:1px solid #B1B1B1;
padding-bottom:5px;
}
</style>

<div >
	<h1>Edge.BI Error</h1>
	<h4>A PHP Error was encountered</h4>
	
	<p>Severity: <?php echo $severity; ?></p>
	<p>Message:  <?php echo $message; ?></p>
	<p>Filename: <?php echo $filepath; ?></p>
	<p>Line Number: <?php echo $line; ?></p>

</div>