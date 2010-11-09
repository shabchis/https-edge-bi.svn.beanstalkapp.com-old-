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
	<!-- <script type="text/javascript" src="scripts/jquery.qtip-1.0.0.min.js"></script> -->

</head>

<body>

	<div id="dropdown"> 
		<select name="combo" id="combo">
					<option value="1">Yesterday &nbsp (<?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>)</option>
						<option value="2">Last 7 days&nbsp(<?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-7,date("Y"))));?> - <?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>)</option>
						<option value="3">Last 30 days&nbsp(<?php echo (date("d/m/y", mktime(0,0,0,date("m")-1,date("d"),date("Y"))));?> -  <?php echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));?>)</option>
						<option value="4">This Month&nbsp(<?php echo date("M",mktime(0,0,0,date("m"),date("d"),date("Y")));?>)</option>
						<option value="5">Last Month&nbsp(<?php echo date("M",strtotime('-1 month'));?>)</option>
						<option value="6">This Week&nbsp(<?php if(strtotime('today') == strtotime('monday')){echo date("d/m/y",strtotime('today'));}else {echo date("d/m/y",strtotime('last monday'));}?> - <?php if (strtotime('today') <= strtotime('monday')){echo (date("d/m/y",strtotime('today')));}else{	echo (date("d/m/y", mktime(0,0,0,date("m"),date("d")-1,date("Y"))));}	?>)	</option>
						<option value="7">Last Week&nbsp(<?php 	if (strtotime('today') == strtotime('monday')){	echo date("d/m/y",strtotime('last monday'));}else{echo date("d/m/y",strtotime('-2 week monday'));}?> - <?php if(strtotime('today') == strtotime('sunday')){	echo date("d/m/y",strtotime('last sunday'));}else{echo date("d/m/y",strtotime('last sunday'));}	?>)	</option>
		</select>
	</div>
	
	<div class="clear"></div>
<?php $account_id = $_GET['account_id'];?>
<?php $measure = '17';?>

	<div id="layout">

		<!--  Channel Performance -->
		<div class="layoutRow">
			<div id="atten" class="widget">
				<div class="header">
							<h3 title="Campaign Performance – ROI">Campaign Performance – ROI</h3>
							<select id="TopCombo">
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
								<tr id="tableheader2">
									<th class="campaign">Campaign</th>
									<th colspan=2>Net Revenue </th>
								</tr>						
							</table>
						</div>
						<div class="error"><h4>No data found for the selection criteria.</h4></div>
					</div>
				</div>
			</div>
			
			
			
			<!-- Campaign Performance ROI -->
			
			<div id="barPanel" class="widget" >
				<div class="header">
					<h3 title="Channel Performance">Channel Performance</h3>
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
			
		</div>
		
		<div class="layoutRow">
			<!-- Geo -->
			
					<div id="trendcahnges" class="widget">
					<div class="header">
						<h3 title="Fluctuating Campaigns – ROI">Fluctuating Campaigns – ROI</h3>
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
									<tr id="tableheader3">
										<th class="campaign">Campaign</th>
										<th colspan="2">Change</th>
									</tr>					
								</table>
							</div>
							<div id="table4" class="factTable" >
								<h3>Top Negative</h3>
								<table id="downward">
									<tr id="tableheader4">
										<th class="campaign">Campaign</th>
										<th colspan="2">Change</th>
									</tr>						
								</table>
							</div>
							<div class="error1"><h4>No data found for the selection criteria.</h4></div>
						</div>
						
					</div>
				</div>
			
			
			<!-- Campaign Flucatuations-->
			
			<div id="TopSpenders" class="widget">
					<div class="header">
						<h3 title="Top Spending campaigns">Top Spending campaigns</h3>
						<select id="TopSpendCombo">
								<?php include 'php/measureDropDown.php'; ?>
									
						</select>
					</div>
					
					<div class="content">
						<div class="timeframe">
							<span class="timevalue">
							</div>
					
						
						<div id="innertables3" class="factTableWrapper">
							<div id="table5" class="factTable">
								<h3>&nbsp</h3>
								<table id="spending">
									<tr id="tableheader5">
										<th class="campaign">Campaign</th>
										<th class="spendcost">Cost</th>
										<th class = "spendcpa">CPA</th>
										<th class="title">Acquisitions</th>
										<th>ROI</th>
									</tr>					
								</table>
							</div>
							
							<div class="error3"><h4>No data found for the selection criteria.</h4></div>
						</div>
							</div>
					</div>
				</div>
			
		</div>
	</div>
	<!--- <div class="layoutRow">
						
			
			
				<div id="gauge" class="widget">
					<div class="header">
						<h3>Overall account performance</h3>
						<select id="gaugeCombo">
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
						
						<div id="innertables3" class="factTableWrapper">
							
							
							<div class="error3"><h4>There was a problem loading the data</h4></div>
						</div>
					</div>
				</div>
				<div id="maps" class="widget">
					<div class="header">
						<h3>Geo Distribution</h3>
						<select id="MapCombo">
						
						</select>
					</div>
				
					<div id="map1" class="content"></div>
				</div>
			
		</div> -->
	</div>
	<script type="text/javascript">
	
	DD_roundies.addRule('.header', '5px 5px 0px 0px');
  DD_roundies.addRule('.tabnav ui-tabs-nav>li', '10px');
	// var account_id = <?php echo $_GET['account_id'];?>;
		var account_id = '<?php echo $_REQUEST['account_id'];?>';
	var globalMeasure = '17';
	// var globalMeasure2 = '1';
	


$(function(){	

//Disable the main time combobox  when loading new content
$('#combo').ajaxStart(function(){
$(this).attr('disabled',true);


})
$('#combo').ajaxStop(function(){
$(this).removeAttr('disabled');


})

var title = $('#TopSpendCombo option:selected').text();
	 $('th.title').text(title);
	


	var percent = $("span.precent").text();

	var time = 	$("#combo option:selected").val();
	var text = $("#combo option:selected").text();

	
		// var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
	
	
	$('.timevalue').empty();
	// $('span.timevalue').append(text);
	 // $('.equal').empty();
	
	ChangeStatus();
	LoadTopCampaigns();
	LoadWorstCampaigns();
	topSpenders();
	LoadGraph();
	ChangeYesterdayMatch();
	loadByMeasures();
	 LoadGraph();

	 // ChangeYesterdayMatch();
	// changeCampaignPerforamceCombo();
		$("#combo").change(function(){
				var endDate = $("#combo option:selected").val();
				// startDate = "Previous Day";
			$('span.timevalue').empty();
			 $('.equal').empty();

				
				ChangeStatus();
	 			ChangeYesterdayMatch();
				graphcombo();
				

				// LoadMap(startDate,endDate);
				LoadGraph();
			
				// ChangeYesterdayMatch();
				LoadTopCampaigns();
				LoadWorstCampaigns();
				 topSpenders();


		})

	 	
		 
 
		

})
// the function responsable to change the content by changing the measure combo

function loadByMeasures(){
	$("#TopCombo").change(function(){
		LoadTopCampaigns();
		
	})
		$("#GraphCombo").change(function(){
			 measure =  $("#GraphCombo option:selected").val();
			 measureText = $("#GraphCombo option:selected").text();
			LoadGraph();
			
			})
			
	$("#flcamp").change(function(){
		measure = $("#flcamp option:selected").val();
		LoadWorstCampaigns();
		})
		
		$("#MapCombo").change(function(){
		LoadMap(startDate,endDate);
	})
	
	$("#TopSpendCombo").change(function(){
		measure = $("#TopSpendCombo option:selected").val();
			 topSpenders();
		})
	

	}
	

	
	
	

	

	//change the titles of the dates when changing the time combo 
	function ChangeStatus(){
	
	  $('span.timevalue').empty();
			 $('.equal').empty();
			
			var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
			 var parent = yesterday.parent().parent().parent().attr('id');
			

	var selectValue = $("#combo option:selected").val();
			
				
				if(selectValue == 1){
				  $('span.timevalue').empty();
					 $(".equal").append(yesterday);
					$('span.timevalue').append("Yesterday");
				}
				if(selectValue == 2){
				  $('span.timevalue').empty();
						$(".equal").append("Previous 7 days");
						$('span.timevalue').append("Last 7 days");
				}	
			
				if(selectValue == 3){
				 $('span.timevalue').empty();
						$(".equal").append("Previous 30 days");
						$('span.timevalue').append("Last 30 days");
				}
				if(selectValue == 4){
				$('span.timevalue').empty();
						$(".equal").append("Last Month");
						$('span.timevalue').append("This Month");
				}
			
				if(selectValue == 5){
				$('span.timevalue').empty();
				$('span.timevalue').append("Last Month");
						$(".equal").append("Previous Month");
				}
				if(selectValue == 6){
				$('span.timevalue').empty();
				$('span.timevalue').append("This Week");
						$(".equal").append("Last Week");
						
				}
				if(selectValue == 7){
				$('span.timevalue').empty();
				$('span.timevalue').append("Last Week");
						$(".equal").append("Previous Week");
				}
	}
	
	
	//load the content by changing the yesterday combo 
	function ChangeYesterdayMatch(){
		$("#barPanel #yesterdayCombo").change(function(){
		startDate = $("#barPanel #yesterdayCombo option:selected").val();
		LoadGraph();
		
		
		
			})
	
	$("#trendcahnges #yesterdayCombo").change(function(){
	startDate = $("#trendcahnges #yesterdayCombo option:selected").val();
	
		LoadWorstCampaigns(startDate);
	
	})
	
	$("#atten #yesterdayCombo").change(function(){
	startDate = $("#atten #yesterdayCombo option:selected").val();
	
		LoadTopCampaigns();
	
	})
	
	
		}

	
	
			/* Loads the Channel Performance graph */
    function LoadGraph(){
		
		var startDate = $("#yesterdayCombo option:selected").val();
		var endDate = $("#combo option:selected").val();
		var measure =  $("#GraphCombo option:selected").val();
		var measureText = $("#GraphCombo option:selected").text();
		var startTimeFrame = $("combo option:selected").text();
		var endTimeFrame = $("second option:selected").text();
	
	// console.log(account_id);
				 // console.log('http://testing/Dashboard/php/amcolumn_data.php?account_id='+account_id+'&endDate='+endDate+'&measure='+measure+'&startdate='+startDate+'&global='+globalMeasure+'');
			 //console.log("http://testing/Dashboard/php/colsettings.php?measure="+measureText+"&endDate="+endDate+"&startDate="+startDate+"");
               var so = new SWFObject("charts/amcolumn/amcolumn.swf", "amcolumn", "100%", "300", "8", "#FFFFFF");
               so.addVariable("path", "charts/amcolumn/");

               // so.addVariable("settings_file", encodeURIComponent("amcolumn/amcolumn_settings.xml"));        // you can set two or more different settings files here (separated by commas)
               so.addVariable("settings_file", encodeURIComponent("php/colsettings.php?measure="+measureText+"&endDate="+endDate+"&startDate="+startDate+""));                          
			 so.addVariable("data_file", encodeURIComponent("php/amcolumn_data.php?account_id="+account_id+"&endDate="+endDate+"&measure="+measure+"&startdate="+startDate+"&global="+globalMeasure+""));
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
/* Loads the map  */
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
		
		/* Load the Campaign Performance tables  */
	function LoadTopCampaigns(){
	$("#table2,#table1").hide();
	$(".loader1").remove();
	$(".error").hide();
	$('tr.topcpa').remove();
	$('tr#total1').remove();
	$('tr#total2').remove();
	
	// startDate = $("#trendcahnges #yesterdayCombo option:selected").val();
	//console.log(startDate);
			measure = $("#TopCombo option:selected").val();
			endDate = $("#combo option:selected").val();
			startDate = $("#atten #yesterdayCombo option:selected").val();
			$("#table2,#table1").hide();
			 $("#innertables").append('<div class="loader1"><img src="images/loading_bar7.gif"></img></div>');
			$.ajax({
				type: "GET",
				url: "php/campaignPerformance.php?account_id="+account_id+"&endDate="+endDate+"&startdate="+startDate+"&measure="+measure+"&orderby=DESC&global="+globalMeasure+"",
				dataType: "xml",
				error: function (xml) {
		
					$(".loader1").hide();
					$(".error").show();
					$("#table2,#table1").hide();
					
				if (xml.status == 500){
			
				$(".error h4").html('Data failed to load');
				}
				
				},
				success: function(xml) {
							$(xml).find('campaigns').each(function(){
								var total = $(this).find('total').text();
								var totaldiff = $(this).find('totaldiff').text();
								$('#tableheader').after('<tr id="total1"><td>Account Total</td><td>'+total+'</td><td class="precent">'+totaldiff+'</td></tr>');
							});
				
							$(xml).find('campaign').each(function(){
								var name = $(this).find('name').text();
								var cpa = $(this).find('value').text();
								var diff = $(this).find('diff').text();
							var fullname = $(this).find('fullname').text();	
						
							
				
					$('<tr class="topcpa"><td class="name" title="'+fullname+'">'+name+'</td><td class="middle">'+cpa+'</td><td class="last"><span class="con"><span class="precent">'+diff+'</span></span></td></tr>').appendTo('#top');
				// $('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'&nbsp &nbsp('+number+') </td></tr>').appendTo('#upward');
		 // $('<tr class="worstcpa"><td>'+name+'</td><td>'+percent+'%</td><td>('+number+')</td></tr>').appendTo("#upward");
				
			
				$(".precent:contains(-)").css('color','red').parent("td.last").css('color','red');
			
				$(".precent").not(":contains(-)").css('color','green').parent("td.last").css('color','green');
				$("#top .topcpa .name").attr("'title','tesy'");
	

	
			});
				
		$("div.loader1").fadeOut(400);
		$("#table2,#table1").show();
		}
	});
		
	$.ajax({
	
	
		type: "GET",
		url: "php/campaignPerformance.php?account_id="+account_id+"&endDate="+endDate+"&startdate="+startDate+"&measure="+measure+"&orderby=ASC&global="+globalMeasure+"",
		dataType: "xml",
		error: function (xml) {
			$(".loader1").hide();
			$(".error").show();
			$("#table2,#table1").hide();
					if (xml.status == 500){
		
			$(".error h4").html('Data failed to load');
			}

			},
		success: function(xml) {
					$(xml).find('campaigns').each(function(){
							var total = $(this).find('total').text();
								var totaldiff = $(this).find('totaldiff').text();
							$('#tableheader2').after('<tr id="total2"><td>Account Total</td><td>'+total+'</td><td class="precent">'+totaldiff+'</td></tr>');
						});
				$(xml).find('campaign').each(function(){
					var name = $(this).find('name').text();
					var cpa = $(this).find('value').text();
					var diff = $(this).find('diff').text();
					var fullname = $(this).find('fullname').text();
				
				
					$('<tr class="topcpa"><td title="'+fullname+'">'+name+'</td><td class="middle">'+cpa+'</td><td class="last"><span class="con"><span class="precent">'+diff+'</span></span></td></tr>').appendTo('#Worse');
					// $('<div class="htmltooltip">RSS stands for Really Simple Syndication, and is a type of XML file format.</div>').appendTo("#table1");
				// $('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'&nbsp &nbsp('+number+') </td></tr>').appendTo('#upward');
		 // $('<tr class="worstcpa"><td>'+name+'</td><td>'+percent+'%</td><td>('+number+')</td></tr>').appendTo("#upward");
				
			
				$(".precent:contains(-)").css('color','red').parent("td.last").css('color','red');
			
				$(".precent").not(":contains(-)").css('color','green').parent("td.last").css('color','green');
				
			
			});
					$("div.loader1").fadeOut(400);
					$("#table2,#table1").show();
		}

	});
		
	}
		/* Loads the Fluctuating Campaigns tables */
	function LoadWorstCampaigns(){
	$(".error1").hide();
		$('tr#total3').remove();
	$('tr#total4').remove();
	// $(".loader1").remove();
		startDate = $("#trendcahnges #yesterdayCombo option:selected").val();
		measure = $("#flcamp option:selected").val();
		endDate = $("#combo option:selected").val();
			$("#table3,#table4").hide();
			 $("#innertables2").append('<div class="loader1"><img src="images/loading_bar7.gif"></img></div>');
	
		$('tr.worstcpa').remove();
		$.ajax({
			type: "GET",
			url: "php/worsecampaign.php?account_id="+account_id+"&endDate="+endDate+"&startdate="+startDate+"&measure="+measure+"&orderby=ASC&global="+globalMeasure+"",
			dataType: "xml",
			error: function (xml) {
		
		$(".loader1").remove();
			$(".error1").show();
			$("#table3,#table4").hide();
			if (xml.status == 500){
		
			$(".error1 h4").html('Data failed to load');
			}
			
			},
		
			success: function(xml) {
			$(xml).find('campaigns').each(function(){
							var total = $(this).find('total').text();
								var totaldiff = $(this).find('totaldiff').text();
							$('#tableheader3').after('<tr id="total3"><td>Account Total</td><td >'+total+'</td><td >'+totaldiff+'</td></tr>');
						});
				$(xml).find('campaign').each(function(){
					var name = $(this).find('name').text();
					var cpa = $(this).find('value').text();
					var diff = $(this).find('diff').text();
					var number = $(this).find('diff_number').text();
						var	percent = $(this).find('percent').text();
					var fullname = $(this).find('fullname').text();
					
					// $('<tr class="worstcpa"><td>'+name+'</td><td class="color">'+cpa+'&nbsp &nbsp(<span class="precent">'+diff+'</span>%)</td></tr>').appendTo('#Worse');
					// $('<tr class="topcpa"><td>'+name+'</td><td>'+cpa+'</td><td class ="last"><span class="con">(<span class="precent">'+diff+'</span>%)</span></td></tr>').appendTo('#Worse');
						$('<tr class="worstcpa"><td title="'+fullname+'">'+name+'</td><td class="middle">'+diff+'</td><td>'+cpa+'</td></tr>').appendTo('#downward');
		 	
				$(".precent:contains(-)").css('color','red').parent(".con").css('color','red');
			
				$(".precent").not(":contains(-)").css('color','green').parent(".con").css('color','green');
	  					// var title = "";
					// $('#TopCombo').change(function(){
						 // title = $('#TopCombo option:selected').text();

						// $('#table2 h3 span,#table1 h3 span').text(title);
						

					// })

	
				});
			
			},
			complete:function(xml) {
				$("div.loader1").fadeOut(400);
				
			}
		});
		
		$.ajax({
			type: "GET",
			url: "php/worsecampaign.php?account_id="+account_id+"&endDate="+endDate+"&startdate="+startDate+"&measure="+measure+"&orderby=DESC&global="+globalMeasure+"",
			dataType: "xml",
			error: function (xml) {
		
		$(".loader1").remove();
			$(".error1").show();
			$("#table3,#table4").hide();
						if (xml.status == 500){
		
			$(".error1 h4").html('Data failed to load');
			}
			
			},
			success: function(xml) {
			$(xml).find('campaigns').each(function(){
					var total = $(this).find('total').text();
						var totaldiff = $(this).find('totaldiff').text();
							$('#tableheader4').after('<tr id="total4"><td>Account Total</td><td>'+total+'</td><td>'+totaldiff+'</td></tr>');
						});
				$(xml).find('campaign').each(function(){
					var name = $(this).find('name').text();
					var cpa = $(this).find('value').text();
					var diff = $(this).find('diff').text();
					var number = $(this).find('diff_number').text();
						var	percent = $(this).find('percent').text();
						var fullname = $(this).find('fullname').text();
					
						$('<tr class="worstcpa"><td class="name" title="'+fullname+'">'+name+'</td><td class="middle">'+diff+'</td><td>'+cpa+'</td></tr>').appendTo('#upward');
		 	
				$("span.precent:contains(-)").css('color','red').parent(".con").css('color','red');
			
				$("span.precent").not(":contains(-)").css('color','green').parent(".con").css('color','green');
	  					var title = "";
					$('#TopCombo').change(function(){
						 title = $('#TopCombo option:selected').text();

						$('#table2 h3 span,#table1 h3 span').text(title);
						

					})

				});
						$("#table3,#table4").show();
					
			}
			
				
			
			
		});
		
		
		}
		
		/* Loads the top spenders tables */
	function topSpenders(){
		startDate = $("#TopSpenders #yesterdayCombo option:selected").val();
		measure = $("#TopSpendCombo option:selected").val();
		endDate = $("#combo option:selected").val();
		$("#table5").hide();
		$('.error3').hide();
			 $("#innertables3").append('<div class="loader3"><img src="images/loading_bar7.gif"></img></div>');
	$('tr.topspend').remove();
	$('#total5').remove();

		$.ajax({
			type: "GET",
			url: "php/topSpenders.php?account_id="+account_id+"&endDate="+endDate+"&startdate=1&measure="+measure+"&orderby=DESC",
			dataType: "xml",
			 error: function (xml) {
	
		
		
		$(".loader3").remove();
			$(".error3").show();
			$("#table5").hide();
					
					 if (xml.status == 500) {
					$(".error3 h4").html('Data failed to load');
					}
				},
			success: function(xml) {
			$(xml).find('campaigns').each(function(){
							var total = $(this).find('total').text();
								var totalcpa = $(this).find('totalcpa').text();
								var totalacq= $(this).find('totalacq').text();
								var totalroi = $(this).find('totalroi').text();
							$('#tableheader5').after('<tr id="total5"><td>Account Total</td><td>'+total+'</td><td>'+totalcpa+'</td><td>'+totalacq+'</td><td class="roi">'+totalroi+'</td></tr>');
						});
			
				$(xml).find('campaign').each(function(){
					var name = $(this).find('name').text();
					var cpa = $(this).find('cpa').text();
					var cost = $(this).find('cost').text();
					var roi = $(this).find('roi').text();
					var acq = $(this).find('acq').text();
					var number = $(this).find('diff_number').text();
					var fullname = $(this).find('fullname').text();
						
					var fullname = $(this).find('fullname').text();
				
				
						$('<tr class="topspend"><td title="'+fullname+'">'+name+'</td><td class="middle">'+cost+'</td><td>'+cpa+'</td><td>'+acq+'</td><td class="roi">'+roi+'</td></td></tr>').appendTo('#spending');
		 	
			
		
				$("span.precent:contains(-)").css('color','red').parent(".con").css('color','red');
			
				$("span.precent").not(":contains(-)").css('color','green').parent(".con").css('color','green');
	  					 var title = "";
					 $('#TopSpendCombo').change(function(){
						 title = $('#TopSpendCombo option:selected').text();

					 $('th.title').text(title);
						

					})

	$("#table5").show();
	$('.error3').hide();
				});
			
			
			},
			
			
			complete:function(xml) {
				$("div.loader3").fadeOut(400); //fade out the loading gif
				//colors the ROI by number
				$('.roi').each(function(){
				var text =  $(this).text();
				var num = parseFloat(text);

				if (num >= 130){
				$(this).css('color','green');
					}
				if(num <= 70){
				$(this).css('color','red');
				}	
					
				})
				
			}
		});
	}
	
	
		
	</script>

  
</body>
</html>



