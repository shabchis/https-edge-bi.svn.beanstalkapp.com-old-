

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns=”http://www.w3.org/1999/xhtml”>
<head>
    <meta http-equiv="content-type" content="text/html; charset=utf-8"/>
	<link href="style.css" rel="stylesheet" type="text/css" />
      <!--[if IE]>
  <link href="ie.css" rel="stylesheet" type="text/css" />
      <![endif]-->


	<script type='text/javascript' src='http://ajax.googleapis.com/ajax/libs/jquery/1.4/jquery.min.js'></script>
	<script type="text/javascript" src="jquery.corners.js"></script>
	<script type="text/javascript" src="DD_roundies_0.0.2a-min.js"></script>
	<script type="text/javascript" src="js/jquery.fusioncharts.js"></script>
	<script type="text/javascript" src="jquery-ui-personalized-1.5.2.packed.js"></script>
	<script type="text/javascript" src="js/jquery.tmpl.js"></script>
	<script type="text/javascript" src="js/jquery.tmplPlus.js"></script>
	<script type="text/javascript" src="js/FusionCharts.js"></script>
	<script type="text/javascript" src="sprinkle.js"></script>
	<script type="text/javascript" src="visualize.jQuery.js"></script>
	<script type="text/javascript" src="http://www.google.com/jsapi"></script>
	<!--<script type="text/javascript" src="yesterday.js"></script>-->
	<!-- <script type="text/javascript" src="yesterday2.js"></script>-->
	<!--   <script type="text/javascript" src="LastWeek.js"></script>-->
	<!--<script type="text/javascript" src="LastMonth.js"></script>-->
	<!--<script type="text/javascript" src="Map.js"></script>-->
	<script type="text/javascript" src="swfobject.js"></script>
	<script language="javascript">AC_FL_RunContent = 0;</script>
	<script language="javascript"> DetectFlashVer = 0; </script>
	<script src="AC_RunActiveContent.js" language="javascript"></script>
  
	<title>
	  DashBoard
	</title>
	




 </head>
 <body>

  <div id="wrapper">
  	 <div id="dropdown">
 
  		<select name="combo" id="combo">
  			<?php include 'timeDropDown.php'?>
  		</select>
  
  		
  	</div>
	<div class="clear"></div>


	 <div id="tabvanilla" class="widget" >
 	 <div class="header">
        <h3>Channel Performance</h3>
         <select id="GraphCombo">
		<!--<?php include 'measureDropDown.php'?>-->
		<option>New Users</option>
		<option>New Active Users</option>
		<option>Cpa</option>
		<option>Cost</option>
		</select>
		
     </div>
     <div class="timeframe">
       <span class="timevalue"></span> VS 
       <select id="second"><?php include 'timeDropDown.php'?></select>
      
      
	</div>

      <div id="weekagograph">
      <strong>You need to upgrade your Flash Player</strong>
      </div>
         
               
        <!--/popular-->
          
      </div><!--/widget-->


	<div id="maps" class="widget rounded">
         <div class="header">
               <h3>Geo Distribution</h3>
               <select id="MapCombo">
				<!--<?php include 'measureDropDown.php'?>-->
		<option>New Users</option>
		<option>New Active Users</option>
		<option>Cpa</option>
		<option>Cost</option>
				</select>
       	 </div>

          <div id="map1">
		      
            </div>
              
    </div>
	<div class="clear"></div>
		<div id="oper" class="widget">
            <div class="header">
                        <h3>Performance by Targets</h3>
                        <select id="GaugeCombo">
							<?php include 'measureDropDown.php'?>
						</select>

                    </div>
            
              <div id="gouge1" class="tabdiv" >
                  <script language="JavaScript" type="text/javascript">
						<!--
						var requiredMajorVersion = 9;
						var requiredMinorVersion = 0;
						var requiredRevision = 45;
						-->
					</script>
				
					
					
					<script  type="text/javascript">
					<!--
						if (AC_FL_RunContent == 0 || DetectFlashVer == 0) {
							alert("This page requires AC_RunActiveContent.js.");
						} else {
							var hasRightVersion = DetectFlashVer(requiredMajorVersion, requiredMinorVersion, requiredRevision);
							if(hasRightVersion) { 
								AC_FL_RunContent(
								'codebase', 'http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,45,0',
								'width', '100%',
								'height', '300',
								'scale', 'noscale',
								'salign', 'TL',
								'bgcolor', '#666666',
								'wmode', 'opaque',
								'movie', 'slickboard',
								'src', 'slickboard',
								'FlashVars', 'xml_source=sample.xml', 
								'id', 'my_board',
								'name', 'my_board',
								'menu', 'true',
								'allowFullScreen', 'true',
								'allowScriptAccess','sameDomain',
								'quality', 'high',
								'align', 'middle',
								'pluginspage', 'http://www.macromedia.com/go/getflashplayer',
								'play', 'true',
								'devicefont', 'false'
								); 
							} else { 
							var alternateContent = 'This content requires the Adobe Flash Player. '
							+ '<u><a href=http://www.macromedia.com/go/getflash/>Get Flash</a></u>.';
							document.write(alternateContent); 
						}
					}
					// -->
					</script>
            </div><!--end gauge1-->
         
        </div><!--/widget-->
		
	<div id="atten" class="widget rounded">
	   <div class="header">
	            <h3>Campaign performance - Revenue</h3>
	            <select id="TopCombo">
					<?php include 'measureDropDown.php'?>
				</select>
		    </div>

		<div id="table2" >
			<h3>Top Winning Campaigns </h3>
				<table id="top">
				<tr id="tableheader">
	                    <th>Campaign</th>
	                    <th>CPA</th>
				</tr>
				
			</table>
		</div>
		<div id="table1" >
			<h3>Top Losing Campaigns</h3>
			<table id="Worse">
				<tr>
                    <th>Campaign</th>
                    <th>CPA</th>
				</tr>
				
			</table>
		</div>
			
		</div>
	
	<div class="clear"></div>
	<div id="trendcahnges" class="widget rounded">
	 <div class="header">
               <h3>Campaign performance-Trend Changes</h3>
               <select id="MapCombo">
				<?php include 'measureDropDown.php'?>
				</select>
				
       	 </div>
		 
	<div class="timeframe">
       <span class="timevalue"></span> VS <select class="innertimecombo"><?php include 'timeDropDown.php'?></select>
	</div>
	</div>
	<div id="topspenders" class="widget rounded">
	<div class="header">
               <h3>Top Spenders</h3>
               <select id="MapCombo">
				<?php include 'measureDropDown.php'?>
				</select>
				
       	 </div>
		 
</div>
	
  <script type="text/javascript">
  $(document).ready(function() {
       $("table tr td:nth-child(1)").css("color","black");
      $("table td:nth-child(2)").css("text-align","left");

      $("table#top th").eq(0).addClass("rightround");
      $("table#top th").eq(1).addClass("leftround");
      $("table#Worse th").eq(0).addClass("rightround");
      $("table#Worse th").eq(1).addClass("leftround");

      });
  

  </script>
  <script type="text/javascript">
$(document).ready( function(){

  /*$('.rounded').corners("10px 10px");*/


});</script>
 <script type="text/javascript">
 
/* DD_roundies.addRule('.rounded', '10px 10px 0px 0px');*/
  DD_roundies.addRule('.tabnav ui-tabs-nav>li', '10px');
 </script>
 <script type="text/javascript">

 var startDate = "";
	var endDate = "";
	var measure =  $("#GraphCombo option:selected").text();
	var startTimeFrame = $("combo option:selected").text();
	var endTimeFrame = $("second option:selected").text();
$(function(){

	
	var time = 	$("#combo option:selected").val();
	var text = $("#combo option:selected").text();
	var width = $("#atten").width();
	var performancewidth  = $("#tabvanilla").width();
	$("#trendcahnges,#oper").width(performancewidth);
	$("#maps,#topspenders").width(width);
	$('span.timevalue').append(text);
	
		$("#GraphCombo").change(function(){
			measure  = $("#GraphCombo option:selected").text();
			LoadGraph(startDate,endDate,measure);
			})
	
		$("#combo").change(function(){
			var text = $("#combo option:selected").text();
			$('span.timevalue').empty();
			
			
	 			$('span.timevalue').append(text);
	 			getMainTime();
				LoadMap(startDate,endDate);
				LoadTopCampaigns();
				LoadWorstCampaigns();
				

		})
		
	
		
	 $("select#second").change(function(){
		 var sectionTime = $("#second option:selected").val();
		 
		 getSectionTime();
		LoadGraph(startDate,endDate,measure);
	
		 })		

		 getMainTime();
		 getSectionTime();
		LoadMap(startDate,endDate);
		LoadTopCampaigns();
		LoadWorstCampaigns();
		LoadGraph(startDate,endDate,measure);
	
})

	
	
    function LoadGraph(startdate,endDate,measure){

               var so = new SWFObject("amcolumn/amcolumn.swf", "amcolumn", "100%", "300", "8", "#FFFFFF");
               so.addVariable("path", "amcolumn/");
               so.addVariable("settings_file", encodeURIComponent("amcolumn/colsettings.php?startDate="+startdate+"&endDate="+endDate+"&measure="+measure+""));        // you can set two or more different settings files here (separated by commas)
               so.addVariable("data_file", encodeURIComponent("amcolumn/amcolumn_data.xml"));
            //       so.addVariable("data_file", encodeURIComponent("amcolumn/coldata.php?startDate="+startdate+"&endDate="+endDate+""));                   
            //	so.addVariable("chart_data", encodeURIComponent("data in CSV or XML format"));                // you can pass chart data as a string directly from this file
            //	so.addVariable("chart_settings", encodeURIComponent("<settings>...</settings>"));             // you can pass chart settings as a string directly from this file
            //	so.addVariable("additional_chart_settings", encodeURIComponent("<settings>...</settings>"));  // you can append some chart settings to the loaded ones
            //  so.addVariable("loading_settings", "LOADING SETTINGS");                                       // you can set custom "loading settings" text here
            //  so.addVariable("loading_data", "LOADING DATA");                                               // you can set custom "loading data" text here
            //  so.addVariable("preloader_color", "#000000");
            //  so.addVariable("error_loading_file", "ERROR LOADING FILE");                                   // you can set custom "error loading file" text here
               so.write("weekagograph");
    }   

	function LoadMap(startdate,endDate){
		var so = new SWFObject("ammap/ammap.swf", "ammap", "100%", "300", "8", "#e8f6f7");
		so.addVariable("path", "ammap/");
		so.addVariable("settings_file", escape("ammap/ammap_settings.xml"));                  // you can set two or more different settings files here (separated by commas)
		so.addVariable("data_file", escape("ammap/xpath.php"));
		//so.addVariable("data_file", escape("ammap/ammap_data.xml"));
//	  	so.addVariable("map_data", "<map ...>...</map>");                                   // you can pass map data as a string directly from this file
//  	so.addVariable("map_settings", "<settings>...</settings>");                         // you can pass map settings as a string directly from this file
//      so.addVariable("additional_map_settings", "<settings>...</settings>");              // you can append some map settings to the loaded ones
//   	so.addVariable("loading_settings", "LOADING SETTINGS");                             // you can set custom "loading settings" text here
//    	so.addVariable("loading_data", "LOADING DATA");                                     // you can set custom "loading data" text here
//   	so.addVariable("preloader_color", "#999999");	                                      // you can set preloader bar and text color here

        so.write("map1");

		}
	function LoadTopCampaigns(){
		$('tr.topcpa').remove();
	$.ajax({
		type: "GET",
		url: "campaigns.xml",
		dataType: "xml",
		success: function(xml) {
			$(xml).find('campaign').each(function(){
				var name = $(this).find('name').text();
				var cpa = $(this).find('cpa').text();
				var diff = $(this).find('diff').text();
				
				$('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'&nbsp &nbsp('+diff+') </td></tr>').appendTo('#top');
				
				
				
			});
		}
	});
	}

	function LoadWorstCampaigns(){
		$('tr.worstcpa').remove();
		$.ajax({
			type: "GET",
			url: "worsecampaign.xml",
			dataType: "xml",
			success: function(xml) {
				$(xml).find('campaign').each(function(){
					var name = $(this).find('name').text();
					var cpa = $(this).find('cpa').text();
					var diff = $(this).find('diff').text();
					
					$('<tr class="worstcpa"><td>'+name+'</td><td>'+cpa+'&nbsp &nbsp('+diff+') </td></tr>').appendTo('#Worse');
					
			
	  					var title = "";
	  $('#TopCombo').change(function(){
					 title = $('#TopCombo option:selected').text();

					$('#table2 h3 span,#table1 h3 span').text(title);
					

					})

				
					
				});
			}
		});
		

		}

	function getMainTime(){
	
		var time = 	$("#combo option:selected").val();
		if (time == "1"){
			startDate = <?php echo (date("dmy", mktime(0,0,0,date("m"),date("d")-2,date("Y"))));?>;
			
			}
		
		if (time == "2"){
			startDate = <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-14,date("Y"))));?>;
			
			}

		if (time == "3"){
			startDate = <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-30,date("Y"))));?>;
		
			}


		if (time == "4"){
			startDate = <?php echo (date("Ymd", mktime(0,0,0,date("m")-1,date("d"),date("Y"))));?>;
		
			}
		if (time == "5"){
			startDate = <?php echo (date("Ymd", mktime(0,0,0,date("m")-2,date("d"),date("Y"))));?>;
		
			}
		if (time == "6"){
			startDate = <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-8,date("Y"))));?>;
			
			}
		if (time == "7"){
			startDate = <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-14,date("Y"))));?>;

			}
		LoadGraph(startDate,endDate,measure);
		}

	function getSectionTime(){

		 var sectionTime = $("#second option:selected").val();
		
		
		if (sectionTime == "1"){
			
			endDate =   <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>;
			}
		if (sectionTime == "2"){
			
			endDate =   <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-7,date("Y"))));?>;
			}
		if (sectionTime == "3"){
			
			endDate =   <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>;
			}
		if (sectionTime == "4"){
			
			endDate =   <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>;
			}
		if (sectionTime == "5"){
			
			endDate =   <?php echo (date("Ymd", mktime(0,0,0,date("m")-1,date("d"),date("Y"))));?>;
			}
		if (sectionTime == "6"){
			
			endDate =   <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>;
			}
		if (sectionTime == "7"){
			
			endDate =   <?php echo (date("Ymd", mktime(0,0,0,date("m"),date("d")-7,date("Y"))));?>;
			}

		}
	function LoadMeasures(){
			$('#GaugeCombo,#GraphCombo,#MapCombo,#TopCombo').empty();
		
		$.ajax({
			type: "GET",
			url: "http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Measures?AccountID=106",

		
			success: function(xml) {
		
				$(xml).find('Measure').each(function(){
					var name = $(this).find('DisplayName').text();
					var value = $(this).find('MeasureID').text();
				
					
					$('<option value="'+value+'">'+name+'</option>').appendTo('#TopCombo,#GraphCombo,#MapCombo,#GaugeCombo');
				
					
					
				});
			}
		});
		


		}
                    </script>
                    
                  

  </body>


</html>



