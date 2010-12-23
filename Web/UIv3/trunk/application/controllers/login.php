<?php

class Login extends Controller {
		
		
	function index()
	{
		
				
		// Get login parameters from form
		$data = array(
			"operationType" => 'New',
			"email" =>  $this->input->post('email'),
			"password"=>$this->input->post('password')		
		);
		
		//$this->firephp->log($data);
		
		// Check if valid parameters
		if (!isset($data["email"]) || $data["email"] == '' ||
			!isset($data["password"]) || $data["password"] == ''
			)
		{
			
		//	header('HTTP/1.0 400 Bad Request');
		//	$this->output->set_header("HTTP/1.0 400 Bad Request");
			//$this->output->set_header("Status: 400 Bad Request");
			
			echo "Invalid login details.";
		}
			
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/sessions');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_POST, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('Content-Type: application/json','accept: application/json'));
		curl_setopt($curl_handle, CURLOPT_POSTFIELDS, json_encode($data));  
		$info = curl_getinfo($curl_handle);
		$result = curl_exec($curl_handle);  
		
		curl_close($curl_handle);  
		$this->firephp->log($result);
 	//	$this->output->set_header("Status: ".$info['http_code']);
		if(IS_AJAX)
		{
			echo $result;
		}
		else
		{
			//header('HTTP/1.0 400 Bad Request');
			//$this->output->set_header("HTTP/1.0 400 Bad Request");
			//$this->output->set_header("Status: 400 Bad Request");
			
 			echo 'Please enable JavaScript.';
 		}
	}	 
	
	function logout(){
		
		delete_cookie("edgebi_session");
		delete_cookie("edgebi_user");
		delete_cookie("edgebi_child_account");
		delete_cookie("edgebi_parent_account");
		
		
		redirect("http://localhost/projects/login/");
	}

}