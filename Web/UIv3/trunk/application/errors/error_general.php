<html>
<head>
<title>Error</title>
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
padding-bottom:5px;
}
</style>
	<div id="content">
	<h1>Edge.BI Error</h1>
	<h2><?php echo $heading; ?></h2>
		<?php echo $message; ?>
	</div>

<?php else: ?>		
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
padding:			25px 20px 12px 20px;
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
</head>
<body>
	<div id="content">
		<h1>Edge.BI Error</h1>
		<h2><?php echo $heading; ?></h2>
		<?php echo $message; ?>
	</div>
</body>
</html>

<?php endif; ?>