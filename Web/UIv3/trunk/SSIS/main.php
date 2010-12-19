
<!DOCTYPE html>
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <script  src="js/jquery-1.4.2.min.js"></script>
    <script src="js/effects.js"></script>
    <script src="js/jquery-ui-1.8.6.custom.min.js"></script>
    <link rel="stylesheet" href="css/smoothness/jquery-ui-1.8.6.custom.css">
    <link rel="stylesheet" href="style.css"/>
    <title>DB </title>
</head>
<body>

<div id="container">

    <nav>
	<h1>SSIS Logs</h1>
	
	<img src="images/logo.png" alt="logo" />
			<select id="db">
            <option value="1">Easynet</option>
            <option value="2">Seperia</option>

        </select>

        <p class="calendar">From:<input id="startdate" type="text" value="<?php echo trim(date("Y-m-d", mktime(0,0,0,date("m"),date("d"),date("Y")))) ;?>">
             to:<input id="enddate" type="text" value="<?php echo trim(date("Y-m-d", mktime(0,0,0,date("m"),date("d"),date("Y"))));?>" >
</p>

<div id="options">
        <ul>
            <li><input id="PackageStart" type="checkbox" name="option1" value="PackageStart"> Package Start</li>
            <li><input  id="PackageEnd" type="checkbox" name="option3" value="PackageEnd"> Package End</li>
            <li><input id="OnError" type="checkbox" name="option3" value="Error"> Error</li>
            <li> <input id="OnWarning" type="checkbox" name="option3" value="Warning"> Warning</li>
		    <li> <input id="OnInformation" type="checkbox" name="option3" value="information"> Information</li>

        </ul>







</div>

<a class="button" id="upperSubmit">GO</a>
    </nav>

<div class="clear"></div>


   <div id="header">&nbsp</div>
    <div id="content">

<!--
        <h2 class="trigger"><a class ="link">DWH_Fact_PPC</a></h2>
               <div class="toggle_container">
                   <div class="block">
                       <ul id="list">
                           <li>DWH_Dim_accounts.Account_id= DWH_Fact_PPC.Account_gk</li>
                           <li>DWH_Dim_accounts.Client_id= DWH_Fact_PPC.Client_gk</li>

                       </ul>-->



    




</div>
<div id="modal"></div>
</body>
</html>
<script type="text/javascript">
        $(function(){
		var	 db = $("#db option:selected").val();
		var startdate = $('input#startdate').val();		
		var endtdate = $('input#enddate').val();
		var content = $('#content');
	
		$('input#PackageEnd').attr('checked',true);
		$('input#OnError').attr('checked',true);
		
			$('#content').ajaxStop(function(){
				if ($('#content').text()== "") { 
			$('#modal').html('No result for selected criteria');
			$('#modal').dialog({title:'No result'});
				} 
			
			
			})
			// $('#options input').change(function(){
				
					// hidetitles();
					// $('#modal').html('changes');
					// $('#modal').dialog();
			// })
		
		$("#upperSubmit").click(function(){
			
			db = $("#db option:selected").val();
			startdate = $('input#startdate').val();		
			endtdate = $('input#enddate').val();
			 var rawstartdate = startdate.split("-")[0]+startdate.split("-")[1]+startdate.split("-")[2];
			 var rawenddate = endtdate.split("-")[0]+endtdate.split("-")[1]+endtdate.split("-")[2];
	
			 if(startdate == ""||endtdate==""){
				$("#modal").html("Please Choose a Date");
				$("#modal").dialog({width:'auto'},{title:'What about the date?'});
			 }
			 else if(rawenddate < rawstartdate){
			 $("#modal").html("The End Date is sooner the the start date");
				$("#modal").dialog({width:'auto'},{title:'What about the date?'});
			 }
			 else{
			 getdata(db,startdate,endtdate);
			
		}
				// $("#modal").html(checked);
				// $("#modal").dialog();
		 
		})
		
		
		  
            $("input#startdate,input#enddate").datepicker({ dateFormat: 'yy-mm-dd' });
			var db = $("#db option:selected").val();
			
			// $("#db").change(function(){
				
				
	getdata(db,startdate,endtdate);

		
		})
		
	
				 $("ul.list li").live("click",function(){
				 
				 var modal = $(this).attr("data-message");
				 var time = $(this).attr("data-time");
				 var title =$(this).attr("data-source");
				 $("#modal").html(modal);
			
				$("#modal").dialog({title:''+title+" - "+time+''},{width:'960px'});
				 // startdate = $('#startdate').val();		
					// endtdate = $('#enddate').val();
				// var	starttime = $(this).attr("data-time");
				 // db = $("#db option:selected").val();
				 // var event = $(this).attr("class");
			 // var executionid = $(this).attr("data-exec");
			 // var source = $(this).parent().attr("date-source");
			 // console.log(source);
		// getmodal(db,startdate,endtdate,starttime,source,event);
		
		})
		
		function getdata(db,startdate,endtdate){
		       $.ajax({
		  url: 'php/log.php?db='+db+'&startdate='+startdate+'&enddate='+endtdate+'',
		  success: function(data) {
			$("#content").html(data);
			// $("h2.PackageStart").hide().next('.toggle_container').hide();
			// $("h2.OnError").hide().next('.toggle_container').hide();
			// $("h2.PackageEnd").hide().next('.toggle_container').hide();
			// $("h2.OnWarning").hide().next('.toggle_container').hide();
			 // $("h2.OnInformation").hide().next('.toggle_container').hide();
getEvents(db,startdate,endtdate);
	  }
});
		
		}
		
		function getEvents(db,startdate,endtdate){
		$("#bullet").remove();
		$("ul.list").empty();
		$('a.link').each(function(){
		var source = $(this).text();
		var link = $(this);
	
		  $.ajax({
		   url: 'php/events.php?db='+db+'&startdate='+startdate+'&enddate='+endtdate+'&source='+source+'',
		   
		  success: function(data) {
		   // checkbox();
		    // $("ul.list#"+source+"").empty();
		  $("ul.list#"+source+"").append(data);
		
		    $('ul.list#'+source+' li').before('<span id="bullet"><img src="images/bullet.png" alt="bullet"/></span>');
					  
				var checked =  $('nav input[type=checkbox]:checked').attr("id");
			 $('li.'+checked+'').show().prev('#bullet').css('display','none');
			 
	
					$('.list li').hide().prev('#bullet').css('display','none').parent().parent().parent().prev('h2.trigger').css('display','none');
					$('.list li.PackageEnd').show().prev('#bullet').css('display','block').parent().parent().parent().prev('h2.trigger').css('display','block');
					$('.list li.OnError').show().prev('#bullet').css('display','block').parent().parent().parent().prev('h2.trigger').css('display','block');
					$('input#PackageEnd').attr('checked',true);
					$('input#OnError').attr('checked',true);
				
					
		  },
		  complete:function(data){
		   $("ul.list").each(function() {
		   // console.log($(this).attr('id'));
         if ($(this).text() == "") {
			// console.log($(this).parent().parent().prev().text());
			$(this).parent().parent().prev().hide();
			// $(this).parent().parent().prev('h2').css('color','red');
                // alert($(this).attr('class'));
         }
		 
})

			
		  
		  }
		
});

		  
		  })
		
		 
		  }
	
			
			
		
		
		
		  function getmodal(db,startdate,endtdate,starttime,source,event){
		  db = $("#db option:selected").val();
		  
			   $.ajax({
		  url: 'php/modal.php?db='+db+'&startdate='+startdate+'&enddate='+endtdate+'&starttime='+starttime+'&source='+source+'&event='+event+'',
		  success: function(data) {
			$("#modal").html(data);
			
			$("#modal").dialog({width:'auto'});
			 
			 
	  }
	  
	

		
		});
		  }
		  function hide(){
		  $("ul li").each(function(){
			  var value = $.trim($(this).text())
			
			  if(value == ""){
				$(this).prev("h2.trigger").hide();
			  }
})

		  
		  
		  }
		  $("input[type='checkbox']").each(function(){
		  
		  
		  var id = $(this).attr('id');
		  console.log(id);
		  $('input#'+id+'').change(function(){
	
			if($(this).attr('checked')){
				 
			 $('li.'+id+'').show().prev('#bullet').css('display','block');

		}
		
		else
		
		{
		 $('li.'+id+'').hide().prev('#bullet').css('display','none');
		
		}
		})
				


});
		  // $('#options input').change(function(){
				// $('ul.list').each(function(){
				
				// if($(this).find("li:visible").length ==0) {
				// $(this).hide().parent().parent().prev('h2.trigger').hide();
				
				// }
				
				// else if($(this).find("li:visible").length >0){
					// $(this).show().parent().parent().prev('h2.trigger').show();
				// }
				// })
				
		  
		  // })
		
		  
		  function hidetitles(){
				var $ul = $('ul');
				if($ul.find("li:visible").length ==0) {
				$ul.parent().parent().prev('h2.trigger').hide();
}
		}
		
</script>