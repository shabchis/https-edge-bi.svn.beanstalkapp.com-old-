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

  DD_roundies.addRule('.header', '5px 5px 0px 0px');
  DD_roundies.addRule('.tabnav ui-tabs-nav>li', '10px');

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

	
		var yesterday = $("<select id='yesterdayCombo'><option value = '1'>Previous Day</option><option value ='2'>7 days ago</option></select>");
	
	
	
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
			
               var so = new SWFObject("charts/amcolumn/amcolumn.swf", "amcolumn", "100%", "300", "8", "#FFFFFF");
               so.addVariable("path", "charts/amcolumn/");
               // so.addVariable("settings_file", encodeURIComponent("amcolumn/amcolumn_settings.xml"));        // you can set two or more different settings files here (separated by commas)
               so.addVariable("settings_file", encodeURIComponent("php/colsettings.php?measure="+measure+"&endDate="+endDate+"&startDate="+startdate+""));                          
			 so.addVariable("data_file", encodeURIComponent("php/amcolumn_data.php?startDate="+startdate+"&endDate="+endDate+"&measure="+measure+""));
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
	function LoadTopCampaigns(){
			$('tr.topcpa').remove();
		$('tr.worstcpa').remove();
		
		
		
	$.ajax({
		type: "GET",
		url: "php/campaigns.php",
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
			url: "php/worsecampaign.php",
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
