<?php header("HTTP/1.1 404 Not Found"); ?>

<?php if (IS_AJAX): ?>

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
padding-bottom:10px;
}
</style>

	<div id="content">
		<h1>Edge.BI Error</h1>
		<h2><?php echo $heading; ?></h2>
		<?php echo $message; ?>


<?php else: ?>

	<style type="text/css">
	
	body {
	background-color:	#fff;
	font-family:		Lucida Grande, Verdana, Sans-serif;
	font-size:			14px;
	color:				#000;
	}
	
	#content  {
	border:				#999 1px solid;
	background-color:	#fff;
	padding:			20px 20px 12px 20px;
	}
	
	h1 {
	font-weight:		normal;
	font-size:			18px;
	color:				red;
	margin: 			0 0 4px 0;
	border-bottom:1px solid #B1B1B1;
	}
	</style>
	
	<h1>Edge.BI Error</h1>
<h2><?php echo $heading; ?></h2>
	<?php echo $message; ?>
		
	<?php endif; ?>	
