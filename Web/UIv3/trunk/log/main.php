
<!DOCTYPE html>
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <script  src="js/jquery-1.4.2.min.js"></script>
    <script src="js/effects.js"></script>
    <script src="js/jquery-ui-1.8.6.custom.min.js"></script>
    <link rel="stylesheet" href="css/ui-lightness/jquery-ui-1.8.6.custom.css">
    <link rel="stylesheet" href="style.css"/>
    <title>DB </title>
</head>
<body>

<div id="container">

    <nav>
	
			<select id="db">
            <option value="1">Easynet</option>
            <option value="2">Seperia</option>

        </select>

        <p>From:<input id="startdate" type="text">
             to:<input id="enddate" type="text">
</p>

<div id="options">
        <ul>
            <li><input id="PackageStart" type="checkbox" name="option1" value="PackageStart"> Package Start</li>
            <li><input  id="PackageEnd" type="checkbox" name="option3" value="PackageEnd"> Package End</li>
            <li><input id="Error" type="checkbox" name="option3" value="Error"> Error</li>
            <li> <input id="Warning" type="checkbox" name="option3" value="Warning"> Warning</li>
		    <li> <input id="information" type="checkbox" name="option3" value="information"> Information</li>

        </ul>







</div>


    </nav>

<div class="clear"></div>


   <div id="header">title</div>
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
	
		$('input#PackageStart').attr('checked',false);
		$('input#Error').attr('checked',false);
		
		 $('input#PackageStart').change(function(){
	
	if($(this).attr('checked')){
				 $("h2.PackageStart").show().next('.toggle_container').show();
		

		}
		
		else
		
		{
		 $("h2.PackageStart").hide().next('.toggle_container').hide();
		}
		
		


		
		
		  });
		
		$('input#Error').change(function(){
	
	if($(this).attr('checked')){
			
			 $("h2.OnError").show().next('.toggle_container').show();

		}
		
		else
		
		{
		 $("h2.OnError").hide().next('.toggle_container').hide();
		
		}
		
		


		
		
		  });
		  
		  $('input#PackageEnd').change(function(){
	
	if($(this).attr('checked')){
			
			
			  $("h2.PackageEnd").show().next('.toggle_container').show();

		}
		
		else
		
		{
		 $("h2.PackageEnd").hide().next('.toggle_container').hide();
		}
		
		


		
		
		  });
		   $('input#Warning').change(function(){
	
	if($(this).attr('checked')){
			
			 $("h2.OnWarning").show().next('.toggle_container').show();

		}
		
		else
		
		{
		$("h2.OnWarning").hide().next('.toggle_container').hide();
		 
		}
		
		


		
		
		  });
		   $('input#information').change(function(){
	
	if($(this).attr('checked')){
			 $("h2.OnInformation").show().next('.toggle_container').show();
			

		}
		
		else
		
		{
		 $("h2.OnInformation").hide().next('.toggle_container').hide();
		
		}
		
		


		
		
		  });
		  
		  
            $("input#startdate,input#enddate").datepicker({ dateFormat: 'yy-mm-dd' });
			var db = $("#db option:selected").val();
			
			$("#db").change(function(){
				
				
	getdata(db,startdate,endtdate);
			})
		
			
            // $("#content").load('php/log.php?db='+db+'');
		getdata(db,startdate,endtdate);


		})
		
		$('input#startdate,input#enddate').change(function(){
		db = $("#db option:selected").val();
		startdate = $('#startdate').val();		
		endtdate = $('#enddate').val();
		console.log(startdate);
			getdata(db,startdate,endtdate);
		
		})
		
		
		function getdata(db,startdate,endtdate){
		       $.ajax({
		  url: 'php/log.php?db='+db+'&startdate='+startdate+'&enddate='+endtdate+'',
		  success: function(data) {
			$("#content").html(data);
			$("h2.PackageStart").hide().next('.toggle_container').hide();
			$("h2.OnError").hide().next('.toggle_container').hide();
			$("h2.PackageEnd").hide().next('.toggle_container').hide();
			$("h2.OnWarning").hide().next('.toggle_container').hide();
			 $("h2.OnInformation").hide().next('.toggle_container').hide();
			 $("ul.list").live("click",function(){
			 var executionid = $(this).attr("data-exec");
			 var sourceid = $(this).attr("date-sourceid");
			 console.log(sourceid);
		getmodal(db,startdate,endtdate,executionid,sourceid);
		
		})
	  }
});
		
		}
		
		  function getmodal(db,startdate,endtdate,executionid,sourceid){
		  db = $("#db option:selected").val();
		  
			   $.ajax({
		  url: 'php/modal.php?db='+db+'&startdate='+startdate+'&enddate='+endtdate+'&executionid='+executionid+'&sourceid='+sourceid+'',
		  success: function(data) {
			$("#modal").html(data);
			
			$("#modal").dialog();
			 
			 
	  }
	  
	

		
		});
		  }
</script>