

	<h1>Add Refund</h1>
<form action="#" method="post">
    <fieldset>
    <label for="channel">Channel</label>
    <select>
        <option value="1">Google</option>
    </select>
    <label for="startDate">Date :</label>
    <input name="startDate" id="startDate" class="date-picker" />
	<input type="submit" value="Submit" id="submit"/>
      </fieldset>  
</form>

<style>
h1{
text-align:center;
color:#616161;

}
    .ui-datepicker-calendar {
        display: none;
        }
        
       select,input{
            display:block;
             margin-bottom: 5px;
        
           

        }

        label{
            display:block;
            margin-bottom: 5px;
        
            
        }

        form{
            width:500px;
            margin:0 auto;
        }
        input[type=submit]{
            margin-top:10px;
        }
        
        select,input:focus{
        border:1px solid #92B743;
        }
    </style>
    
<script type="text/javascript">
    $(function() {


        $('.date-picker').datepicker( {
            changeMonth: true,
            changeYear: true,
            showButtonPanel: true,
            dateFormat: 'mm/dd/yy',
            onClose: function(dateText, inst)
            {
                var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
                var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
                $(this).datepicker('setDate', new Date(year, month, 1));
            }
            

           
        });
        
        $.ajax({
			url: "http://localhost/projects/UIv3/login",
			type: 'POST',
			data: form_data,
			success: function(result, status, request)
			{
					$("#main").append("success");
			},
			error:function(result)
			{
			
				
				
			},
			complete:function(result)
			{
			}
		});
    });
    </script>
    
<?php
