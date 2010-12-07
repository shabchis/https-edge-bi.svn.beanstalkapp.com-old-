<!DOCTYPE html>

<html lang="en">
	<head>
		<meta http-equiv="Content-Type" content="text/html; charset=utf-8"> 
		<title>Framework</title>

		<link rel="stylesheet" href="<?php base_url(); ?>assets/css/style.css" type="text/css" media="screen" />
		<link rel="stylesheet" href="<?php base_url(); ?>assets/css/960.css" type="text/css" media="screen" />
		<!--[if IE]>
		<script src="http://html5shiv.googlecode.com/svn/trunk/html5.js"></script>
		<![endif]-->

		<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4/jquery.min.js" type="text/javascript" charset="utf-8"></script>  
		<script src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8/jquery-ui.min.js"></script>
		<script src="<?php base_url();?>assets/js/jquery.tmpl.js"></script>
		<script src="<?php base_url();?>assets/js/scripts.js"></script>
	</head>

	<body>
		<div id="container" >

		<header>
			<img src="<?php base_url();?>assets/img/edge_bi_logo.png" id="logo" />
			<div id="login">
				Logged in as <span>Doron</span>
				<select id="account"></select>
				<div id="ajaxloader">
					<img src="<?php base_url(); ?>assets/img/loading_bar.gif" id="ajax" />
				</div>
			</div>
 		</header>
 		<div class="clear"></div>

		<script id="menuitems" type="text/x-jquery-tmpl">

    
	<h2 class="trigger"><span> ${Name} </span></h2>
	<div class='toggle_container'>
  		<div class='block'>
    		<ul class="list">

      		{{each ChildMenues}}
        		<li><a href="${Name}">${Name}</a></li>
         	<ul> 
          {{each ChildMenues}} 
          		<li><a href="${Link}">${Name}</a><ul>
          	</ul>
          		{{each ChildMenues}} 
          	<ul>
            
            	 <li><a href="${Name}">${Name}</a></li>
                  
          	</ul>
                {{/each}}
             {{/each}}
      		</ul>
        {{/each}}
      
	</div>
 		</div>
			</div>
	</script>
	<div id="slider"><span><<</span><div id="caption">Hide</div></div>
		<div id="menu">
			<h2 class="trigger"><span>Menu</span></h2>
			<div class='toggle_container'>
				<div class='block'>
				<ul class="list">
					<li><a href="home">Home</a></li>
					<li><a href="about">About</a></li>
					<li><a href="dashboard">Dashboard</a></li>
				</ul>
				</div>
			</div>
		</div>
 

<script>


      var menudata = jQuery.parseJSON('<?php echo $json; ?>');
		
		$( "#menuitems" ).tmpl(menudata).appendTo("#menu");



</script>
  <div id="main">
  main content --- 
  </div>
  <div class="clear"></div>

	<footer>
      footer
    </footer>

  </div>
</body>
</html>