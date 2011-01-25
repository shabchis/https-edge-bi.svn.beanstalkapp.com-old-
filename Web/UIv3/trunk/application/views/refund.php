<form action="#" method="post">
   
    <div id="topfield">
    <label for="channel">Channel </label>
    <select>
        <option value="1">Google</option>
    </select>
    
    </div>
    <div id="bottomfield">
    <label for="refund">Refund </label>
    <input name="refund" id="refund"  autocomplete="off" />
    <label for="startDate">Date </label>
    <input name="startDate" id="startDate" class="date-picker" autocomplete="off" />
    </div>
	<input type="submit" value="Submit" id="submit"/>
    
</form>
<div id="success"></div>
<style>

 .ui-datepicker-calendar{
 display: none;
 }
   
.ui-datepicker-title{

	font-family: verdana;
	font-size: 12px;

}

      
form{
	width:500px;
	margin:0 auto;
	font-family: verdana;
	font-size: 12px;
	color: #616161;
	padding: 10px;
        }
input[type=submit]{
		margin-top:3px; 
        }
#topfield{
    width: 50%;
     float: left;
        }
#bottomfield{
   margin-top:5px;
   width: 51%;
   float: left;
        }
        
    label{
    display: block;
    float:left;
    width: 53px;
    margin-right: 10px;
    }
    label[for=startDate],#startDate{
    margin-top: 5px;
    }
    select,input{
    display: block;
    width:150px;
    float: left;
    margin-left: 5px;
    }
    select{
    width: 154px;
    }
    #success{
     width:500px;
    color:#9BBD53;
    text-align: center;
    margin:80px auto;
    }
    </style>
    
<script type="text/javascript">
    $(function() {


        $('.date-picker').datepicker( {
            changeMonth: true,
            changeYear: true,
               showButtonPanel: true,
			dateFormat: 'mm/dd/yy',
			  onClose: function(dateText, inst) {
            var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
            var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
            $(this).datepicker('setDate', new Date(year, month, 1));
        }
                
        });
        
      $("#submit").button();

		$("#submit").click(function(){
		
			var form_data = {
				"accountID": getHashSegments().accountID,
				"refund":$("#refund").val(),
				"date": $('#startDate').val(),
				"channel": $('select option:selected').val()
			};

			try
			{
				$.ajax({
					dataType:"json",
					type: "POST",
					data:form_data,
					url:"refund/proccess",
					success: function(data) {
	    		
	    				$("#success").html("Refund data added successfully").appendTo("#main");
	    				
	    				
	    			},
	    			error:function(data){
	    			$("#success").html("Failed to add refund data").css('color','red').appendTo("#main");
	    			
	    			}
	 			 });
 			 }
			catch(ex) {
				alert(ex);
			}
			
			return false;
	
		});
	
    });
    </script>