<!DOCTYPE html>

<html lang="en">
	<head>
		<meta http-equiv="Content-Type" content="text/html; charset=utf-8"> 
		<title>Framework</title>

		<link rel="stylesheet" href="<?php base_url(); ?>assets/css/style.css" type="text/css" media="screen" />
		<link rel="stylesheet" href="<?php base_url(); ?>assets/css/960.css" type="text/css" media="screen" />
		<!--[if IE]>
		<script src="http://html5shiv.googlecode.com/svn/trunk/html5.js"></script>
		<script type="text/javascript" src="http://fbug.googlecode.com/svn/lite/branches/firebug1.4/content/firebug-lite-dev.js"></script>

		<![endif]-->

		<script src="<?php base_url();?>assets/js/jquery-1.4.4.js"></script>  
		<script src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8/jquery-ui.min.js"></script>
		<script src="<?php base_url();?>assets/js/jquery.dropshadow.js"></script>
		<script src="<?php base_url();?>assets/js/jquery.tmpl.js"></script>
			<script src="<?php base_url();?>assets/js/jquery.tmplPlus.js"></script>
		<script src="<?php base_url();?>assets/js/DD_roundies_0.0.2a.js"></script>
		<script src="<?php base_url();?>assets/js/jquery.ba-hashchange.js"></script>
		<script src="<?php base_url();?>assets/js/animatetoselector.jquery.js"></script>
		<script src="<?php base_url();?>assets/js/scripts.js"></script>
	</head>

	<body>
		<div id="container" >
		<script id="menuitems" type="text/x-jquery-tmpl">

    {{if Name != "TOPBAR"}}
	<h2 class="trigger"><span> ${Name} </span></h2>
	<div class='toggle_container'>
  		<div class='block'>
 	{{if ChildMenues}} 
  		    <ul class="list">
			
		 		{{each ChildMenues}}
      		<li class="menuheader"><a href="${Path}">${Name}</a>
        {{if ChildMenues != ""}}
			 <ul data-name ="${Name}" class="parent"> 
		 
		  {{each ChildMenues}} 
        	<li class="menuitem"><a href="${Path}">${Name}</a>
        	{{if ChildMenues }}
      			<ul data-name ="${Name}">  	
           	  {{each ChildMenues}}
           	  
           	  	<li class="menuitem"><a href="${Path}">${Name}</a>
        	  {{/each}}
        	  </ul>
        	</li>
        	{{/if}}
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
			<script id="topmenu" type ="text/x-jquery-tmpl">
			
			 {{if Name == "TOPBAR"}}
			 	 <ul>
			 	{{each ChildMenues}}
					<li>| <a href="#">${Name}</a></li>
			  {{/each}}
          		 </ul>
			 {{/if}}
			
			</script>
		<header>
			<img src="<?php base_url();?>assets/img/logo_01.png" id="logo" />
			<div id="login">
				<div id="user">
				<span>Doron</span> <span id="loginout"><a href="login">(Log Out)</a> </span>
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

		
	
	<script id="accountbar"  type="text/x-jquery-tmpl">
		
		{{if Name }}
	<ul>
		<li class="campaign"><span>${ Name }</span>
		{{if ChildAccounts}}
		
		{{each ChildAccounts}}
			{{if ChildAccounts}}
		<li class="parent"><a href="#">${ Name }</a>
			{{if ChildAccounts}}
		<ul>
		{{each ChildAccounts}}
		<li><a href="#">${Name}</a></li>
		{{/each}}
		</ul>
			{{/if}}
		</li>
		{{else}}
			<li><a href="#">${Name}</a></li>
		
		{{/if}}
		{{/each}}
		</ul>
		
		{{/if}}
		</li>
		
		{{/if}}
	</ul>
	</script>
	<script type="text/javascript">
	$(function(){
	$("#accounts").delegate("li","click",function(){
  
  var url = "ynet";
  //console.log(url);
  
  });
	});
	  
	</script>
	<div id="slider"><span><img src="<?php base_url();?>assets/img/arrows_04.png" /></span><div id="caption">Hide</div></div>
		<div id="menu">
			<div id="accounts" class="folded">	
			<div id="head">
				<div id="favicon"><img src='<?php base_url();?>assets/img/favicon4.ico'></img></div>	
				<div id="selected"></div>
				<div id="arrow" class="regular">&nbsp;</div>
			</div>
			<div class="clear"></div>
			<div id="Campaign"></div>	
	
			</div>
		<div id="sub"></div>
		</div>
		
 

<script type="text/javascript">

$(function(){


	var menudata = jQuery.parseJSON('<?php echo $json; ?>');
	var account = 	jQuery.parseJSON('<?php echo $json2; ?>');
 	if(menudata){
 		
 	
 	//console.log(menudata);
 		
 			$("#menuitems").tmpl(menudata).appendTo("#sub");
		$("#topmenu").tmpl(menudata).appendTo("#top");
			
 		}
			


		$("#accountbar").tmpl(account).appendTo("#accounts");
	
//	$.ajax({
  //url: "",
  //data
  //success: function(data) {
   			
    
  //}
//});
});



		
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