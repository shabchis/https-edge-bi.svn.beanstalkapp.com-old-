<head>
		<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4/jquery.min.js" type="text/javascript" charset="utf-8"></script>  

</head>
<style>
html{
margin:0 auto;
width: 960px;
}


#login_form{
margin: 0 auto;
border:1px solid black;
-moz-border-radius:5px;
height: 400px;

}
input{
display: block;
margin:10px auto 10px auto;
padding:5px;
-moz-border-radius:10px;
-webkit-border-radius:10px;
}

h1{
text-align: center;
}
#submit{
display: block;
background-color: white;
-moz-border-radius: 5px;

}
</style>
<body>

<div id="login_form">

	<h1>Login</h1>
    <?php 
	echo form_open('login/validate_credentials');
	echo form_input('username', 'Username','id="username"');
	echo form_input('password', 'Password','id="password"');
	echo form_submit('submit', 'Login','id="submit"');
	
	echo form_close();
	?>

</div><!-- end login_form-->

</body>
<script>


$("#submit").click(function(){
		
		var form_data = {
			"email": $('#username').val(),
			"password": $('#password').val()
			
	};
	
	$.ajax({
		url: "<?php echo site_url('login/validate_credentials'); ?>",
		type: 'POST',
		data: form_data,
		success: function(msg) {
		//console.log(msg);
	
	var result = msg;
		var json =  jQuery.parseJSON(result);
	
		//console.log(msg);
		var session = {"session":json.LogINResult};
	
		setsession(session);
		},
		error:function(msg){
			//console.log(msg);
		
		}
		
	});
	function setsession(session){
		$.post("<?php echo site_url('login/sendsession'); ?>", session);
		
		
		
	 

		  
		
			}
		return false;
});



</script>