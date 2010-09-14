

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns=”http://www.w3.org/1999/xhtml”>
<head>
    <meta http-equiv="content-type" content="text/html; charset=utf-8"/>
	<link href="style1.css" rel="stylesheet" type="text/css" />
      <!--[if IE]>
  <link href="ie.css" rel="stylesheet" type="text/css" />
      <![endif]-->


	<script type='text/javascript' src='jquery-1.4.2.min.js'></script>
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
  		<?php  include 'timeDropDown.php' ?>   
  		</select>
  
  		
  	</div>
	<div class="clear"></div>


	 <div id="barPanel" class="widget" >
 	 <div class="header">
        <h3>Channel Performance</h3>
         <select id="GraphCombo">
	
		<?php include 'measureDropDown.php'; ?>
		</select>
		
     </div>
     <div class="timeframe">
       <span class="timevalue"></span> vs.
	   <div class="equal"><select id="second"><?php include 'timeDropDown.php'?></select></div>
       
      
      
	</div>

      <div id="weekagograph">
      <strong>You need to upgrade your Flash Player</strong>
      </div>
         
               
        <!--/popular-->
          
      </div><!--/widget-->


	<div id="atten" class="widget rounded">
	   <div class="header">
	            <h3>Campaign Performance â€“ ROI</h3>
	            <select id="TopCombo">
					<?php include 'measureDropDown.php'; ?>

				</select>
			
				
		
			
		    </div>

		<div id="innertables">
		<div id="table2" >
					<h3>Top Positive</h3>
						<table id="top">
						<tr id="tableheader">
					                    <th class="campaign">Campaign</th>
					                    <th colspan=2>Net Revenue </th>
						</tr>
						
					</table>
				</div>
				<div id="table1" >
					<h3>Top Negative</h3>
					<table id="Worse">
						<tr>
				                    <th class="campaign">Campaign</th>
				                    <th colspan=2>Net Revenue </th>
						</tr>
						
					</table>
				</div>
				</div>
			
		</div>
	<div class="clear"></div>
		<div id="maps" class="widget rounded">
         <div class="header">
               <h3>Geo Distribution</h3>
               <select id="MapCombo">
					<option>New Users</option>
					<option>New Active Users</option>
					<option>CPA</option>
					<option>Cost</option>
				</select>
       	 </div>

          <div id="map1">
		      
            </div>
		
	
	
	
	
	
		 
</div>
	<div id="trendcahnges" class="widget rounded">
	 <div class="header">
               <h3>Fluctuating Campaigns â€“ ROI</h3>
               <select id="flcamp">
					 <option>New Users</option>
					<option>New Active Users</option>
					<option>CPA</option>
					 <option>Cost</option>  
				</select>
				
       	 </div>
		 
	<div class="timeframe">
       <span class="timevalue"></span> vs.  <div class="equal"></div>
	</div>
	
	<div id="innertables2">
	
	<div id="table3" >
				<h3>Top Positive</h3>
					<table id="upward">
					<tr id="tableheader">
		                    <th class="campaign">Campaign</th>
		                    <th colspan=2>% Change</th>
					</tr>
					
				</table>
			</div>
			<div id="table4" >
				<h3>Top Negative</h3>
				<table id="downward">
					<tr>
		                    <th class="campaign">Campaign</th>
		                    <th colspan=2>% Change</th>
					</tr>
					
				</table>
			</div>
		</div>
	</div>
	</div>
  <script type="text/javascript">
  $(document).ready(function() {
       // $("table tr td:nth-child(1)").css("color","black");
      // $("table td:nth-child(2)").css("text-align","left");

      $("table#top th").eq(0).addClass("rightround");
      $("table#top th").eq(1).addClass("leftround");
      $("table#Worse th").eq(0).addClass("rightround");
      $("table#Worse th").eq(1).addClass("leftround");
	   $("table#upward th").eq(0).addClass("rightround");
      $("table#upward th").eq(1).addClass("leftround");
	  $("table#downward th").eq(0).addClass("rightround");
      $("table#downward th").eq(1).addClass("leftround");

      });
  

  </script>
  <script type="text/javascript">
$(document).ready( function(){

 // $('.header').corners("5px");


});</script>
 <script type="text/javascript">
 DD_roundies.addRule('.header', '5px 5px 0px 0px');
  DD_roundies.addRule('.tabnav ui-tabs-nav>li', '10px');
 </script>
 <script type="text/javascript">

	var startDate = $("#yesterdayCombo option:selected").text();
	var endDate = $("#combo option:selected").val();
	var measure =  $("#GraphCombo option:selected").val();
	var startTimeFrame = $("combo option:selected").text();
	var endTimeFrame = $("second option:selected").text();
		var bar = $("#weekagograph").height();
$(function(){	
	startDate = "Previous Day";
	var percent = $("span.precent").text();

	var time = 	$("#combo option:selected").val();
	var text = $("#combo option:selected").text();
	var width = $("#atten").width();

	
		var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
	var performancewidth  = $("#barPanel").width();
	$("#trendcahnges").width(width);
	
	$("#maps").width(performancewidth);
	$('span.timevalue').append(text);
	$('.equal').empty();
	ChangeStatus();
			
	

		$("#GraphCombo").change(function(){
			 measure =  $("#GraphCombo option:selected").val();
			LoadGraph(startDate,endDate,measure);
			
			})
	
		$("#combo").change(function(){
		// var endDate = $("#combo option:selected").val()
			var text = $("#combo option:selected").text();
			var selectValue = $("#combo option:selected").val();
			var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
				var endDate = $("#combo option:selected").val();
				startDate = "Previous Day";
			$('span.timevalue').empty();
			$('.equal').empty();
				
				ChangeStatus();
	 			$('span.timevalue').append(text);
				
	 			// getMainTime();
				LoadMap(startDate,endDate);
				LoadTopCampaigns();
				LoadWorstCampaigns();
			LoadGraph(startDate,endDate,measure);
				
				

		})
		
	
		$("#yesterdayCombo").change(function(){
		startDate = $("#yesterdayCombo option:selected").text();
		LoadGraph(startDate,endDate,measure);
	})
	 $("select#second").change(function(){
		 var sectionTime = $("#second option:selected").val();
		 
		// getSectionTime();
		// LoadGraph(startDate,endDate,measure);
	
		 })		

		 // getMainTime();
		 // getSectionTime();
		LoadMap(startDate,endDate);
		LoadTopCampaigns();
		LoadWorstCampaigns();
		LoadGraph(startDate,endDate,measure);
		// LoadGraph(startDate,endDate,measure,startTimeFrame,endTimeFrame);
	
	
})
	$("#TopCombo").change(function(){
				LoadTopCampaigns();
					LoadWorstCampaigns();

	
	})
	
	$("#MapCombo").change(function(){
	LoadMap(startDate,endDate);
	})
	
	$("#flcamp").change(function(){
	LoadWorstCampaigns();
	LoadTopCampaigns();
	})
	
	function ChangeStatus(){
	var selectValue = $("#combo option:selected").val();
			var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
	if(selectValue == 1){
						$(".equal").append(yesterday);
				}
				if(selectValue == 2){
						$(".equal").append("Previous 7 days");
				}
			
				if(selectValue == 3){
						$(".equal").append("Previous 30 days");
				}
				if(selectValue == 4){
						$(".equal").append("Last Month");
				}
			
				if(selectValue == 5){
						$(".equal").append("Previous Month");
				}
				if(selectValue == 6){
						$(".equal").append("Last Week");
				}
				if(selectValue == 7){
						$(".equal").append("Previous Week");
				}
	}
	
    function LoadGraph(startdate,endDate,measure){
			
               var so = new SWFObject("amcolumn/amcolumn.swf", "amcolumn", "100%", "300", "8", "#FFFFFF");
               so.addVariable("path", "amcolumn/");
               // so.addVariable("settings_file", encodeURIComponent("amcolumn/amcolumn_settings.xml"));        // you can set two or more different settings files here (separated by commas)
               so.addVariable("settings_file", encodeURIComponent("amcolumn/colsettings.php?measure="+measure+"&endDate="+endDate+"&startDate="+startdate+""));                          
			 so.addVariable("data_file", encodeURIComponent("amcolumn/amcolumn_data.php?startDate="+startdate+"&endDate="+endDate+"&measure="+measure+""));
            //       so.addVariable("data_file", encodeURIComponent("amcolumn/coldata.php?startDate="+startdate+"&endDate="+endDate+""));                  
 // so.addVariable("settings_file", encodeURIComponent("amcolumn/colsettings.php?startDate="+startdate+"&startDateName="+startTimeFrame+"&endDate="+endDate+"&endDateName="+endTimeFrame+"&measure="+measure+""));     			
            //	so.addVariable("chart_data", encodeURIComponent("data in CSV or XML format"));                // you can pass chart data as a string directly from this file
            //	so.addVariable("chart_settings", encodeURIComponent("<settings>...</settings>"));             // you can pass chart settings as a string directly from this file
            //	so.addVariable("additional_chart_settings", encodeURIComponent("<settings>...</settings>"));  // you can append some chart settings to the loaded ones
            //  so.addVariable("loading_settings", "LOADING SETTINGS");                                       // you can set custom "loading settings" text here
            //  so.addVariable("loading_data", "LOADING DATA");                                               // you can set custom "loading data" text here
            //  so.addVariable("preloader_color", "#000000");
            //  so.addVariable("error_loading_file", "ERROR LOADING FILE");                                   // you can set custom "error loading file" text here
               so.write("weekagograph");
			   console.log(measure);
    }   

	function LoadMap(startdate,endDate){
		var so = new SWFObject("ammap/ammap.swf", "ammap", "100%", "368px", "8", "#e8f6f7");
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
		$('tr.worstcpa').remove();
		
		
		
	$.ajax({
		type: "GET",
		url: "campaigns.php",
		dataType: "xml",
		success: function(xml) {
				
			$(xml).find('campaign').each(function(){
				var name = $(this).find('name').text();
				var cpa = $(this).find('cpa').text();
				var diff = $(this).find('diff').text();
					var number = $(this).find('diff_number').text();
				var	percent = $(this).find('percent').text();
			
				
					$('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'</td><td class="last"><span class="con">(<span class="precent">'+diff+'</span>%)</span></td></tr>').appendTo('#top');
				// $('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'&nbsp &nbsp('+number+') </td></tr>').appendTo('#upward');
		$('<tr class="worstcpa"><td>'+name+'</td><td>'+percent+'%</td><td>('+number+')</td></tr>').appendTo("#upward");
				
			
				$("span.precent:contains(-)").css('color','red').parent("td.last").css('color','red');
			
				$("span.precent").not(":contains(-)").css('color','green').parent("td.last").css('color','green');
				
			
			});
		}
	});
	}

	function LoadWorstCampaigns(){
		$('tr.topcpa').remove();
		$('tr.worstcpa').remove();
		$.ajax({
			type: "GET",
			url: "worsecampaign.php",
			dataType: "xml",
			success: function(xml) {
				$(xml).find('campaign').each(function(){
					var name = $(this).find('name').text();
					var cpa = $(this).find('cpa').text();
					var diff = $(this).find('diff').text();
					var number = $(this).find('diff_number').text();
						var	percent = $(this).find('percent').text();
					// $('<tr class="worstcpa"><td>'+name+'</td><td class="color">'+cpa+'&nbsp &nbsp(<span class="precent">'+diff+'</span>%)</td></tr>').appendTo('#Worse');
					$('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'</td><td class ="last"><span class="con">(<span class="precent">'+diff+'</span>%)</span></td></tr>').appendTo('#Worse');
						$('<tr class="worstcpa"><td>'+name+'</td><td>'+percent+'%</td><td>('+number+')</td></tr>').appendTo('#downward');
		 	
				$("span.precent:contains(-)").css('color','red').parent(".con").css('color','red');
			
				$("span.precent").not(":contains(-)").css('color','green').parent(".con").css('color','green');
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
		LoadGraph(startDate,endDate,measure,startTimeFrame,endTimeFrame);
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



