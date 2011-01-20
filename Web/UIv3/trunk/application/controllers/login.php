<?php

class Login extends Controller {
		
		
	function index()
	{
		// don't allow non-ajax requests
		if(!IS_AJAX)
		{
 			$this->errors->ThrowEx('Please enable JavaScript.', 400);
 		}
		
		// Get login parameters from form
		$data = array(
			"operationType" => 'New',
			"email" =>  $this->input->post('email'),
			"password"=>$this->input->post('password')
		);
	
		// Check if valid parameters
		if (!isset($data["email"]) || $data["email"] == '' ||
			!isset($data["password"]) || $data["password"] == '')
		{
			$this->errors->ThrowEx("Invalid login details.", 400);
		}
		
		// Determines whether user/session cookies are reused later
		$remember =  $this->input->post('remember');
		
		
		// execute the login
		$this->edgeapi->Login($data, $remember, true);
	}	 
	
	function logout(){
		
		delete_cookie("edgebi_session");
		delete_cookie("edgebi_user");
		delete_cookie("edgebi_child_account");
		delete_cookie("edgebi_parent_account");
		delete_cookie("edgebi_remember");
		
		redirect(LOGIN_PAGE);
	}

}