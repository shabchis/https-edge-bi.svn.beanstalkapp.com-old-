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

		<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.4.4/jquery.min.js"></script>  
		<script src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8/jquery-ui.min.js"></script>
	
		<script src="http://ajax.microsoft.com/ajax/jquery.templates/beta1/jquery.tmpl.min.js"></script>
		
	</head>

	<body>
	<script id="tmpl"  type="text/x-jquery-tmpl">
		 {{if Name != "TOPBAR"}}
		<h2 class="trigger"><span> ${Name} </span></h2>
	<div class='toggle_container'>
  		<div class='block'>
		{{if ChildItems}} 
		 <ul class="list">
		 	
	{{each ChildItems}}
		 	<li class="menuheader"><a href="${Path}">${Name}</a>
		 	  {{if ChildItems != ""}}
		 	 <ul data-name ="${Name}" class="parent"> 
 {{each ChildItems}} 
	 	 	<li class="menuitem"><a href="${Path}">${Name}</a>
		 	 		{{if ChildItems }}
		 	 	
		 	 		<ul data-name ="${Name}">  	
		 	 			  {{each ChildItems}}
		 	 			<li class="menuitem"><a href="${Path}">${Name}</a></li>
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
	
	
	
		<div id="sub"></div>
		
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
	
		<div id="accounts"></div>
<script type="text/javascript">




	var menudata = jQuery.parseJSON('[{"ID":5,"Name":"Management","Path":"management","Ordinal":0,"MetaData":{"iFrameURL":"edge"},"ChildItems":[{"ID":6,"Name":"Grouping & Targets","Path":"management/campaigns","Ordinal":0,"MetaData":{"iFrameURL":"edge"},"ChildItems":[]},{"ID":7,"Name":"Segments","Path":"management/segments","Ordinal":10,"MetaData":{"WPF-class":"Easynet.Edge.UI.Client.Pages, Edge.UI.Client.Pages"},"ChildItems":[]}]},{"ID":1,"Name":"Intelligence","Path":"Intelligence","Ordinal":10,"MetaData":{"Selectable":"false"},"ChildItems":[{"ID":3,"Name":"Analysis","Path":"Intelligence/Analysis","Ordinal":0,"MetaData":{"iFrameURL":"edge"},"ChildItems":[{"ID":12,"Name":"Test10","Path":"Intelligence/Analysis/Test10","Ordinal":0,"MetaData":{"iFrameURL":"yaron"},"ChildItems":[]}]},{"ID":4,"Name":"BU Analysis","Path":"Intelligence/buanalysis","Ordinal":10,"MetaData":{"iFrameURL":"edge"},"ChildItems":[]},{"ID":2,"Name":"Dashboard","Path":"Intelligence/Dashboard","Ordinal":20,"MetaData":{"iFrameURL":"edge"},"ChildItems":[{"ID":10,"Name":"Test1","Path":"Intelligence/Dashboard/Test1","Ordinal":0,"MetaData":{"Controller":"iframe_controller","iFrame-URL":"http"},"ChildItems":[{"ID":18,"Name":"TEST4","Path":"Intelligence/Dashboard/Test1/1","Ordinal":0,"MetaData":{"iFrameURL":"www.edge"},"ChildItems":[]}]},{"ID":11,"Name":"Test2","Path":"Intelligence/Dashboard/Test2","Ordinal":10,"MetaData":{"iFrameURL":"yaron"},"ChildItems":[]}]}]},{"ID":15,"Name":"TOPBAR","Path":"topbar","Ordinal":20,"MetaData":{"Location":"top"},"ChildItems":[{"ID":13,"Name":"Settings","Path":"topbar/settings","Ordinal":0,"MetaData":{"iFrameURL":"edge"},"ChildItems":[]},{"ID":14,"Name":"Help","Path":"topbar/help","Ordinal":10,"MetaData":{"iFrameURL":"edge"},"ChildItems":[]}]}]');
	var account = 	jQuery.parseJSON('[{"ID":1,"Name":"888       ","ChildAccounts":[{"ID":100000,"Name":"Casino","ChildAccounts":[{"ID":10001,"Name":"888.com - Casino  UK ","ChildAccounts":[]},{"ID":10005,"Name":"Live Casino - UK","ChildAccounts":[]},{"ID":10006,"Name":"888Casino - UK","ChildAccounts":[]},{"ID":10007,"Name":"Reefclubcasino - UK","ChildAccounts":[]},{"ID":10014,"Name":"Casino - AT","ChildAccounts":[]},{"ID":10015,"Name":"Live Casino - AT","ChildAccounts":[]},{"ID":10016,"Name":"ReefClub Casino - AT","ChildAccounts":[]},{"ID":10023,"Name":"Euro City","ChildAccounts":[]}]},{"ID":100001,"Name":"Bingo","ChildAccounts":[{"ID":10002,"Name":"888ladies - UK+IE","ChildAccounts":[]},{"ID":10009,"Name":"888Bingo - UK","ChildAccounts":[]}]},{"ID":100002,"Name":"Games","ChildAccounts":[{"ID":10008,"Name":"888games - UK","ChildAccounts":[]}]},{"ID":100003,"Name":"Poker","ChildAccounts":[{"ID":10000,"Name":"Poker 888.com - UK+IE","ChildAccounts":[]},{"ID":10004,"Name":"Poker - 888Poker - UK","ChildAccounts":[]},{"ID":10010,"Name":"Poker PCP - UK","ChildAccounts":[]},{"ID":10011,"Name":"Poker PCP - AT","ChildAccounts":[]},{"ID":10012,"Name":"Poker 888.com - AT","ChildAccounts":[]}]},{"ID":100004,"Name":"Sport","ChildAccounts":[{"ID":10003,"Name":"888Sport - UK","ChildAccounts":[]},{"ID":10013,"Name":"888Sport - AT","ChildAccounts":[]},{"ID":10018,"Name":"888.com Sport - UK","ChildAccounts":[]},{"ID":10019,"Name":"888Sport Comp","ChildAccounts":[]}]}]},{"ID":3,"Name":"WSOP","ChildAccounts":[{"ID":100005,"Name":"DragonFish","ChildAccounts":[{"ID":10024,"Name":"WSOP","ChildAccounts":[]}]}]},{"ID":95,"Name":"Demo","ChildAccounts":[{"ID":-9999,"Name":"Demo","ChildAccounts":[]}]},{"ID":10032,"Name":"TraderXP","ChildAccounts":[]},{"ID":10033,"Name":"GoPro","ChildAccounts":[]},{"ID":10034,"Name":"Bbinary","ChildAccounts":[]}]');
 	if(menudata){
 		

 			$("#tmpl").tmpl(menudata).appendTo("#sub");
		
			
 		}

 		
	//	$("#accountbar").tmpl(account).appendTo("#accounts");



	




		
</script>
	</body>
	</html>