<!DOCTYPE html>

<html lang="en">
	<head>
		<meta http-equiv="Content-Type" content="text/html; charset=utf-8"> 
		<title>Framework</title>

		<link rel="stylesheet" href="<?php base_url(); ?>assets/css/style.css" type="text/css" media="screen" />
		<link rel="stylesheet" href="<?php base_url(); ?>assets/css/960.css" type="text/css" media="screen" />
		<!--[if IE]>
		<script src="http://html5shiv.googlecode.com/svn/trunk/html5.js"></script>
		<meta http-equiv="X-UA-Compatible" content="IE=8" />
			<script src="<?php base_url();?>assets/js/modernizr-1.6.min.js"></script>
		<![endif]-->

		<script src="<?php base_url();?>assets/js/jquery-1.4.4.js"></script>  
		<script src="<?php base_url();?>assets/js/selectivizr.js"></script>  
		<script src="<?php base_url();?>assets/js/jquery.dropshadow.js"></script>
		<script src="<?php base_url();?>assets/js/jquery.tmpl.js"></script>
		<script src="<?php base_url();?>assets/js/jquery.tmplPlus.js"></script>
		<script src="<?php base_url();?>assets/js/DD_roundies_0.0.2a.js"></script>
		<script src="<?php base_url();?>assets/js/jquery.ba-hashchange.js"></script>
		<script src="<?php base_url();?>assets/js/animatetoselector.jquery.js"></script>
		<script src="<?php base_url();?>assets/js/jquery.cookie.js"></script>
		<script src="<?php base_url();?>assets/js/scripts.js"></script>
	</head>

	<body>
		<div id="container" >
		
			<script id="usertmpl"  type="text/x-jquery-tmpl">
			<span id="${UserID}">${Name}</span>
			
			</script>
		
		<script id="tmpl"  type="text/x-jquery-tmpl">
		 {{if Name != "TOPBAR"}}
		<h2 class="trigger"><span> ${Name} </span></h2>
	<div class='toggle_container'>
  		<div class='block'>
		{{if ChildItems}} 
		 <ul class="list">
		 	
	{{each ChildItems}}
	{{if MetaData}}
	
		 	<li class="menuheader" data-url="${MetaData.iFrameURL}"><a href="${Path}">${Name}</a>
		 	
		 	{{/if}}
		 	  {{if ChildItems != ""}}
		 	 <ul data-name ="${Name}" class="parent"> 
 			{{each ChildItems}} 
 		
	 	 	<li class="menuitem"  data-url="${MetaData.iFrameURL}"><a href="${Path}">${Name} </a>
	 	 	
		 	 		{{if ChildItems }}
		 	 	
		 	 		<ul data-name ="${Name}">  	
		 	 			  {{each ChildItems}}
		 	 			  
		 	 			<li class="menuitem" data-url="${MetaData.iFrameURL}"><a href="${Path}">${Name}</a> </li>
		 	 			
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
		 
		 </ul>
		   {{/if}}
		</div>
		
		</div>
  		
 		{{/if}}
	
 		
			
			
	</script>

			<script id="topmenu" type ="text/x-jquery-tmpl">
			
			 {{if Name == "TOPBAR"}}
			 	 <ul>
			 	{{each ChildItems}}
					<li>| <a href="http://${MetaData.External}">${Name}</a></li>
			  {{/each}}
          		 </ul>
			 {{/if}}
			
			</script>
		<header>
			<img src="<?php base_url();?>assets/img/logo_01.png" id="logo" />
			<div id="login">
				<div id="user">
				&nbsp; <span id="loginout"><a href="login/logout">(Log Out)</a> </span>
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
		<li id="${ID}" class="campaign"  {{if Permissions}}data-Permissions=${Permissions} {{else}} data-Permissions="none" {{/if}}><span>${ Name }</span>
		{{if ChildAccounts}}
		
		{{each ChildAccounts}}
			{{if ChildAccounts}}
		<li id="${ID}" class="parent" {{if Permissions}}data-Permissions=${Permissions} {{else}} data-Permissions="none" {{/if}}><a href="#">${ Name }</a>
			{{if ChildAccounts}}
		<ul>
		{{each ChildAccounts}}
		<li id="${ID}"{{if Permissions}}data-Permissions=${Permissions} {{else}} data-Permissions="none" {{/if}}><a href="#" >${Name}</a></li>
		{{/each}}
		</ul>
			{{/if}}
		</li>
		{{else}}
			<li id="${ID}" {{if Permissions}} data-Permissions=${Permissions} {{else}} data-Permissions="none" {{/if}}><a href="#">${Name}</a></li>
		
		{{/if}}
		{{/each}}
		</ul>
		
		{{/if}}
		</li>
		
		{{/if}}
	</ul>
	</script>
	
	<div id="slider"><span><a href="#"><img src="<?php base_url();?>assets/img/arrows_04.png" /></a></span><div id="caption">Hide</div></div>
		<div id="menu">
			<div id="accounts" class="folded">	
			<div id="head">
				<div id="favicon"><img src='<?php base_url();?>assets/img/favicon4.ico'></img></div>	
				<div  id="selected"></div>
				<div id="arrow" class="regular">&nbsp;</div>
			</div>
			<div class="clear"></div>
			<div id="Campaign"></div>	
	
			</div>
		<div id="sub"></div>
		</div>
		
 

<script type="text/javascript">




	var menudata = jQuery.parseJSON('<?php echo $menu; ?>');
	var account = 	jQuery.parseJSON('<?php echo $account; ?>');
	var user = jQuery.parseJSON('<?php echo $user; ?>');
 	if(menudata){
 		
 	

		$("#tmpl").tmpl(menudata).appendTo("#sub");
		$("#topmenu").tmpl(menudata).appendTo("#top");
		$("#usertmpl").tmpl(user).prependTo("#user");
 		}

 	if(account){
		

 	$("#accountbar").tmpl(account).appendTo("#accounts");
		
		}
		

$(function(){
$("#sub a").click(function(){
	
	if($("#Campaign").size()<1){
	
	alert("please choose an account");
	}
		
	});
var i ="";
$("#top ul li a").attr("target","_blank");
	$("#accounts li").click(function(){
		
	var permission = $(this).attr("data-Permissions");
			var permissionsplted = permission.split(",");
		$.each(permissionsplted,function(i){
				var data ={"url":permissionsplted};
			 						 
			console.log(permissionsplted[i]);
		$("#sub a[href="+permissionsplted[i]+"]").addClass('disabled');
		
		 $("a").not("[href="+permissionsplted[i]+"]").addClass('disabled');
		 
		  console.log($("a").not("[href="+permissionsplted[i]+"]").attr("href") +" ->"+$("a").not("[href="+permissionsplted[i]+"]").attr("href"));
		
	});
	
	
		
		
	
	
  
   


			
});

});
		
</script>
  <div id="main">
  main content --- 
  </div>
  <div class="clear"></div>

	<footer>
   
    </footer>

  </div>
</body>
</html>