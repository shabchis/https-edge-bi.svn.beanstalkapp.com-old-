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
			<script id="topmenu" type ="text/x-jquery-tmpl">
			 {{if Name == "TOPBAR"}}
			 	 <ul>
			 	{{each ChildMenues}}
					<li><a href="#">${Name}</a></li>
			  {{/each}}
          		 </ul>
			 {{/if}}
			</script>
		<header>
			<img src="<?php base_url();?>assets/img/edge_bi_logo.png" id="logo" />
			<div id="login">
				<div id="user">
				Logged in as <span>Doron</span>
				</div>
				
				<div id="top"></div>
				
				
				<div id="ajaxloader">
					<img src="<?php base_url(); ?>assets/img/loading_bar.gif" id="ajax" />
				</div>
			</div>
			<div id="breadcrumbs">
			
			</div>
			
 		</header>
 		<div class="clear"></div>

		<script id="menuitems" type="text/x-jquery-tmpl">

    {{if Name != "TOPBAR"}}
	<h2 class="trigger"><span> ${Name} </span></h2>
	<div class='toggle_container'>
  		<div class='block'>
  		{{if ChildMenues }}
  		    <ul class="list">
			
		   	{{each ChildMenues}}
      		<li class="menuheader"><a href="${Name}">${Name}</a>
        {{if ChildMenues }}
			 <ul data-name ="${Name}" class="parent"> 
		 
		  {{each ChildMenues}} 
        	<li class="menuitem"><a href="${Name}">${Name}</a></li>
         {{/each}}
          		
          	</ul>
			{{/if}}
			  
			</li>
			{{/each}}
        	
			</ul>
      		
            {{/if}}
	</div>
 		</div>
			</div>
			 {{/if}}
	</script>
	
	<script id="accountbar"  type="text/x-jquery-tmpl">
		<div id="accounts">
		<div id="selected">${Name}</div>
		
		<ul>
		{{each ChildAccounts}}
			<li>${Name}</li>
		{{/each}}
		</ul>
		</div>
	
	</script>
	<div id="slider"><span><<</span><div id="caption">Hide</div></div>
		<div id="menu">
		
		<div id="sub"></div>
		</div>
		
 

<script>


	var menudata = jQuery.parseJSON('<?php echo $json; ?>');
		
		$( "#menuitems" ).tmpl(menudata).appendTo("#sub");
		$("#topmenu").tmpl(menudata).appendTo("#top");
	var account = 	jQuery.parseJSON('<?php echo $json2; ?>');
		$("#accountbar").tmpl(account).prependTo("#menu");

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