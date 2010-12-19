<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>DashBoard</title>

	<meta http-equiv="content-type" content="text/html; charset=utf-8"/>
	<link href="styles/main.css" rel="stylesheet" type="text/css" />

	<script type='text/javascript' src='scripts/jquery-1.4.2.min.js'></script>
	<script type="text/javascript" src="scripts/jquery.corners.js"></script>
	<script type="text/javascript" src="scripts/DD_roundies_0.0.2a-min.js"></script>
	<script type="text/javascript" src="scripts/jquery-ui-personalized-1.5.2.packed.js"></script>
	<script type="text/javascript" src="scripts/swfobject.js"></script>

</head>

<body>
	<div id="dropdown"> 
		<select name="combo" id="combo">
					<option value="1"><span class="period">Yesterday</span>	 (<span class="dates"><?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>)</option></span>
						<option value="2">Last 7 days	(<?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-7,date("Y"))));?> - <?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>)</option>
						<option value="3">Last 30 days	(<?php echo (date("d/m/y", mktime(0,0,0,date("m")-1,date("d")-1,date("Y"))));?> -  <?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>)</option>
						<option value="4">This Month(<?php echo date("M",mktime(0,0,0,date("m"),date("d"),date("Y")));?>)</option>
						<option value="5">Last Month(<?php echo date("M",strtotime('-1 month'));?>)</option>
						<option value="6">This Week	(<?php echo date("d/m/y",strtotime('last sunday'));?> - <?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>)</option>
						<option value="7">Last Week	(<?php echo date("d/m/y",strtotime('-1 week'));?>)</option>
		</select>
	</div>
	
	<div class="clear"></div>
<?php $Account_id = $_GET['Account_id'];?>
	
<?php echo $Account_id ; ?>

	<div id="layout">

		<!--  Channel Performance -->
		<div class="layoutRow">
			
			<div id="barPanel" class="widget" >
				<div class="header">
					<h3>Channel Performance</h3>
					<select id="GraphCombo">
						<?php include 'php/measureDropDown.php'; ?>
						
						
					</select>
				</div>
				
				<div class="content">
					<div class="timeframe">
						<span class="timevalue"></span> vs.
						<div class="equal">
							<select id='yesterdayCombo'class="bar">
							<option value = '1'>Previous Day</option>
							<option value ='2'>7 days ago</option>
							</select>
						</div>
					</div>
								
					<div id="weekagograph">
						<strong>You need to upgrade your Flash Player</strong>
					</div>
				</div>
			</div>
			
			
			<!-- Campaign Performance ROI -->
			
			<div id="atten" class="widget">
				<div class="header">
							<h3>Campaign Performance – ROI</h3>
							<select id="TopCombo">
						<?php include 'php/measureDropDown.php'; ?>
					</select>
				</div>
			
				<div class="content">
					<div id="innertables" class="factTableWrapper">
						<div id="table2" class="factTable">
							<h3>Top Positive</h3>
							<table id="top">
								<tr id="tableheader">
									<th class="campaign">Campaign</th>
									<th colspan=2>Net Revenue </th>
								</tr>						
							</table>
						</div>
						<div id="table1" class="factTable">
							<h3>Top Negative</h3>
							<table id="Worse">
								<tr>
									<th class="campaign">Campaign</th>
									<th colspan=2>Net Revenue </th>
								</tr>						
							</table>
						</div>
						<div class="error"><h4>There was a problem loading the data</h4></div>
					</div>
				</div>
			</div>
			
		</div>
		
		<div class="layoutRow">
			<!-- Geo -->
			
				<div id="maps" class="widget">
					<div class="header">
						<h3>Geo Distribution</h3>
						<select id="MapCombo">
							<?php include 'php/measureDropDown.php'; ?>
						</select>
					</div>
				
					<div id="map1" class="content"></div>
				</div>
			
			
			<!-- Campaign Flucatuations-->
			
				<div id="trendcahnges" class="widget">
					<div class="header">
						<h3>Fluctuating Campaigns – ROI</h3>
						<select id="flcamp">
							<?php include 'php/measureDropDown.php'; ?>
						</select>
					</div>
					
					<div class="content">
						<div class="timeframe">
							<span class="timevalue"></span> vs. 
							<div class="equal">
							<select id='yesterdayCombo'class="trend">
							<option value = '1'>Previous Day</option>
							<option value ='2'>7 days ago</option>
							</select>
							</div>
						</div>
						
						<div id="innertables2" class="factTableWrapper">
							<div id="table3" class="factTable">
								<h3>Top Positive</h3>
								<table id="upward">
									<tr id="tableheader">
										<th class="campaign">Campaign</th>
										<th colspan="2">% Change</th>
									</tr>					
								</table>
							</div>
							<div id="table4" class="factTable" >
								<h3>Top Negative</h3>
								<table id="downward">
									<tr>
										<th class="campaign">Campaign</th>
										<th colspan="2">% Change</th>
									</tr>						
								</table>
							</div>
							<div class="error1"><h4>There was a problem loading the data</h4></div>
						</div>
					</div>
				</div>
			
		</div>
	</div>
	<script type="text/javascript">
	DD_roundies.addRule('.header', '5px 5px 0px 0px');
  DD_roundies.addRule('.tabnav ui-tabs-nav>li', '10px');
var account_id = <?php echo $_GET['Account_id'];?>;
	var startDateText = $("#yesterdayCombo option:selected").text();
	var startDate = $("#yesterdayCombo option:selected").val();
	var endDate = $("#combo option:selected").val();
	var measure =  $("#GraphCombo option:selected").val();
	var measureText = $("#GraphCombo option:selected").text();
	var startTimeFrame = $("combo option:selected").text();
	var endTimeFrame = $("second option:selected").text();
	var bar = $("#weekagograph").height();
$(function(){	


console.log(account_id);
	var percent = $("span.precent").text();

	var time = 	$("#combo option:selected").val();
	var text = $("#combo option:selected").text();

	
		// var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
	
	
	$('.timevalue').empty();
	$('span.timevalue').append(text);
	 // $('.equal').empty();
	
	ChangeStatus();
	
	
		// changeCampaignPerforamceCombo();
	// LoadTopCampaigns();
	changeCampaignPerforamceCombo();
	 graphcombo();
	 // ChangeYesterdayMatch();
	changeCampaignPerforamceCombo();
		$("#combo").change(function(){
		// var endDate = $("#combo option:selected").val()
			// var text = $("#combo option:selected").text();
			// var selectValue = $("#combo option:selected").val();
			// var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
				var endDate = $("#combo option:selected").val();
				// startDate = "Previous Day";
			$('span.timevalue').empty();
			 $('.equal').empty();
			// $('.equal').hide();
				
				ChangeStatus();
	 			$('span.timevalue').append(text);
				graphcombo();
				
	 			// getMainTime();
				LoadMap(startDate,endDate);
				LoadGraph(account_id,startDate,endDate,measure,measureText);
				graphcombo();
				// ChangeYesterdayMatch();
				LoadTopCampaigns(account_id);
				LoadWorstCampaigns(account_id);
				 
	

		})

	 $("select#second").change(function(){
		 var sectionTime = $("#second option:selected").val();
		 
		// getSectionTime();
		// LoadGraph(startDate,endDate,measure);
	
		 })		
		 
 LoadGraph(account_id,startDate,endDate,measure,measureText);
		 // getMainTime();
		 // getSectionTime();
		LoadMap(startDate,endDate);
		LoadTopCampaigns();
		 LoadWorstCampaigns();
		
		 ChangeYesterdayMatch();
		
		// LoadGraph(startDate,endDate,measure,startTimeFrame,endTimeFrame);

	 

})
function changeCampaignPerforamceCombo(){
	$("#TopCombo").change(function(){
		LoadTopCampaigns();
		
	})
	
	}
	ChangeYesterdayMatch();
	$("#MapCombo").change(function(){
		LoadMap(startDate,endDate);
	})
	
	$("#trendcahnges #flcamp").change(function(){
			measure = $("#flcamp option:selected").val();
			
			LoadWorstCampaigns();
	})
	
	// $("#yesterdayCombo").change(function(){
			// startDate = $("#yesterdayCombo option:selected").val();
			
	// })
	
	function ChangeStatus(){
	
	// $('span.timevalue').empty();
			 $('.equal').empty();
			// $('.equal').show();
			var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
			 var parent = yesterday.parent().parent().parent().attr('id');
			
			// console.log(parent);
	var selectValue = $("#combo option:selected").val();
			
				
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
	
	function ChangeYesterdayMatch(){
		$("#barPanel #yesterdayCombo").change(function(){
		startDate = $("#barPanel #yesterdayCombo option:selected").val();
		LoadGraph(account_id,startDate,endDate,measure,measureText);
		
		
		
			})
	
	$("#trendcahnges #yesterdayCombo").change(function(){
		LoadWorstCampaigns();
	
	})
		}
	function fluncuatingMeasure(){
		$("#flcamp").change(function(){
		measure = $("#flcamp option:selected").val();
		LoadWorstCampaigns();
		})
	}
	
	
	function graphcombo() {
		$("#GraphCombo").change(function(){
			 measure =  $("#GraphCombo option:selected").val();
			 measureText = $("#GraphCombo option:selected").text();
			LoadGraph(account_id,startDate,endDate,measure,measureText);
			
			})
			}
    function LoadGraph(account_id,startDate,endDate,measure,measureText){
	console.log(account_id);
				 console.log('http://testing/Dashboard/php/amcolumn_data.php?account_id='+account_id+'&endDate='+endDate+'&measure='+measure+'&startdate='+startDate+'');
				 // console.log("http://testing/Dashboard/php/colsettings.php?measure="+measureText+"&endDate="+endDate+"&startDate="+startDate+"");
               var so = new SWFObject("charts/amcolumn/amcolumn.swf", "amcolumn", "100%", "300", "8", "#FFFFFF");
               so.addVariable("path", "charts/amcolumn/");
               // so.addVariable("settings_file", encodeURIComponent("amcolumn/amcolumn_settings.xml"));        // you can set two or more different settings files here (separated by commas)
               so.addVariable("settings_file", encodeURIComponent("php/colsettings.php?measure="+measureText+"&endDate="+endDate+"&startDate="+startDate+""));                          
			 so.addVariable("data_file", encodeURIComponent("php/amcolumn_data.php?account_id="+account_id+"&endDate="+endDate+"&measure="+measure+"&startdate="+startDate+""));
			  // so.addVariable("data_file", encodeURIComponent("php/amcolumn_data.php?startDate="+startdate+"&endDate="+endDate+"&measure="+measure+""));
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
			  
    }   

	function LoadMap(startdate,endDate){
		var so = new SWFObject("charts/ammap/ammap.swf", "ammap", "100%", "368px", "8", "#e8f6f7");
		so.addVariable("path", "charts/ammap/");
		so.addVariable("settings_file", escape("php/ammap_settings.xml"));                  // you can set two or more different settings files here (separated by commas)
		so.addVariable("data_file", escape("php/xpath.php"));
		//so.addVariable("data_file", escape("ammap/ammap_data.xml"));
//	  	so.addVariable("map_data", "<map ...>...</map>");                                   // you can pass map data as a string directly from this file
//  	so.addVariable("map_settings", "<settings>...</settings>");                         // you can pass map settings as a string directly from this file
//      so.addVariable("additional_map_settings", "<settings>...</settings>");              // you can append some map settings to the loaded ones
//   	so.addVariable("loading_settings", "LOADING SETTINGS");                             // you can set custom "loading settings" text here
//    	so.addVariable("loading_data", "LOADING DATA");                                     // you can set custom "loading data" text here
//   	so.addVariable("preloader_color", "#999999");	                                      // you can set preloader bar and text color here

        so.write("map1");

		}
	function LoadTopCampaigns(account_id){
	$(".loader1").remove();
	$(".error").hide();
			measure = $("#TopCombo option:selected").val();
			endDate = $("#combo option:selected").val();
			$("#table2,#table1").hide();
			 $("#innertables").append('<div class="loader1"><img src="images/loading_bar7.gif"></img></div>');
			$('tr.topcpa').remove();
		
		
			$.ajax({
	
		
		type: "GET",
		url: "php/campaignPerformance.php?endDate="+endDate+"&startdate=1&measure="+measure+"&orderby=DESC",
		dataType: "xml",
		error: function (xml) {
		$(".loader1").hide();
		$(".error").show();
			
			},
		success: function(xml) {
				
				
			$(xml).find('campaign').each(function(){
				var name = $(this).find('name').text();
				var cpa = $(this).find('value').text();
				var diff = $(this).find('diff').text();
			
			
				
					$('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'</td><td class="last"><span class="con"><span class="precent">'+diff+'</span></span></td></tr>').appendTo('#top');
				// $('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'&nbsp &nbsp('+number+') </td></tr>').appendTo('#upward');
		 // $('<tr class="worstcpa"><td>'+name+'</td><td>'+percent+'%</td><td>('+number+')</td></tr>').appendTo("#upward");
				
			
				$("span.precent:contains(-)").css('color','red').parent("td.last").css('color','red');
			
				$("span.precent").not(":contains(-)").css('color','green').parent("td.last").css('color','green');
				
			
			});
		
		}
	});
		
	$.ajax({
	
	
		type: "GET",
		url: "php/campaignPerformance.php?endDate="+endDate+"&startdate=1&measure="+measure+"&orderby=ASC",
		dataType: "xml",
		error: function (xml) {
		$(".loader2").hide();
		$(".error").show();
			
			},
		success: function(xml) {
				
			$(xml).find('campaign').each(function(){
				var name = $(this).find('name').text();
				var cpa = $(this).find('value').text();
				var diff = $(this).find('diff').text();
				
			
				
					$('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'</td><td class="last"><span class="con"><span class="precent">'+diff+'</span></span></td></tr>').appendTo('#Worse');
				// $('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'&nbsp &nbsp('+number+') </td></tr>').appendTo('#upward');
		 // $('<tr class="worstcpa"><td>'+name+'</td><td>'+percent+'%</td><td>('+number+')</td></tr>').appendTo("#upward");
				
			
				$("span.precent:contains(-)").css('color','red').parent("td.last").css('color','red');
			
				$("span.precent").not(":contains(-)").css('color','green').parent("td.last").css('color','green');
				
			
			});
					$("div.loader1").fadeOut(100);
					$("#table2,#table1").show();
		}

	});
		
	}

	function LoadWorstCampaigns(account_id){
	$(".error1").hide();

	$(".loader1").remove();
		startDate = $("#barPanel #yesterdayCombo option:selected").val();
		measure = $("#flcamp option:selected").val();
		endDate = $("#combo option:selected").val();
			$("#table3,#table4").hide();
			 $("#innertables2").append('<div class="loader1"><img src="images/loading_bar7.gif"></img></div>');
	
		$('tr.worstcpa').remove();
		$.ajax({
			type: "GET",
			url: "php/worsecampaign.php?endDate="+endDate+"&startdate="+startDate+"&measure="+measure+"&orderby=ASC",
			dataType: "xml",
			error: function (xml) {
		
		$(".loader2").remove();
			if(xml.length == 0) {
			$("#innertables2").append("<div class='error1'>There were a problem loading the data2</div>");
			}
			
			
			},
		
			success: function(xml) {
				$(xml).find('campaign').each(function(){
					var name = $(this).find('name').text();
					var cpa = $(this).find('value').text();
					var diff = $(this).find('diff').text();
					var number = $(this).find('diff_number').text();
						var	percent = $(this).find('percent').text();
					// $('<tr class="worstcpa"><td>'+name+'</td><td class="color">'+cpa+'&nbsp &nbsp(<span class="precent">'+diff+'</span>%)</td></tr>').appendTo('#Worse');
					// $('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'</td><td class ="last"><span class="con">(<span class="precent">'+diff+'</span>%)</span></td></tr>').appendTo('#Worse');
						$('<tr class="worstcpa"><td>'+name+'</td><td>'+diff+'</td><td>'+cpa+'</td></tr>').appendTo('#downward');
		 	
				$("span.precent:contains(-)").css('color','red').parent(".con").css('color','red');
			
				$("span.precent").not(":contains(-)").css('color','green').parent(".con").css('color','green');
	  					// var title = "";
					// $('#TopCombo').change(function(){
						 // title = $('#TopCombo option:selected').text();

						// $('#table2 h3 span,#table1 h3 span').text(title);
						

					// })

	
				});
			}
		});
		
$.ajax({
			type: "GET",
			url: "php/worsecampaign.php?endDate="+endDate+"&startdate="+startDate+"&measure="+measure+"&orderby=DESC",
			dataType: "xml",
			error: function (xml) {
		
		$(".loader1").remove();
			$(".error1").show();
			
			
			},
			success: function(xml) {
				$(xml).find('campaign').each(function(){
					var name = $(this).find('name').text();
					var cpa = $(this).find('value').text();
					var diff = $(this).find('diff').text();
					var number = $(this).find('diff_number').text();
						var	percent = $(this).find('percent').text();
					// $('<tr class="worstcpa"><td>'+name+'</td><td class="color">'+cpa+'&nbsp &nbsp(<span class="precent">'+diff+'</span>%)</td></tr>').appendTo('#Worse');
					// $('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'</td><td class ="last"><span class="con">(<span class="precent">'+diff+'</span>%)</span></td></tr>').appendTo('#Worse');
						$('<tr class="worstcpa"><td>'+name+'</td><td>'+diff+'</td><td>'+cpa+'</td></tr>').appendTo('#upward');
		 	
				$("span.precent:contains(-)").css('color','red').parent(".con").css('color','red');
			
				$("span.precent").not(":contains(-)").css('color','green').parent(".con").css('color','green');
	  					var title = "";
					$('#TopCombo').change(function(){
						 title = $('#TopCombo option:selected').text();

						$('#table2 h3 span,#table1 h3 span').text(title);
						

					})

	
					
				});
				$("div.loader1").hide();
				$("#table3,#table4").show();
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



