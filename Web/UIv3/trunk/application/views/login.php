<head>
		<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.4/jquery.min.js" type="text/javascript" charset="utf-8"></script>  

</head>

<body>

<div id="login_form">

	<h1>Login, Fool!</h1>
    <?php 
	echo form_open('login/validate_credentials');
	echo form_input('username', 'Username','id="username"');
	echo form_password('password', 'Password','id="password"');
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
			$('body').html(msg);
		}
	});
		return false;
});


</script>