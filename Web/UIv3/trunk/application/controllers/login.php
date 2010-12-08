<?php

class Login extends Controller {
		
		
	function index(){
		
		$this->load->view('login');
		
	}
	
	function validate_credentials()
	{		
		$data = array(
		"email" =>  $this->input->post('email'),
		"password"=>$this->input->post('password')
		
		);
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, 'http://AlonYa-PC/API/EdgeBIAPIService.svc/sessions');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_POST, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('Content-Type: application/json'));
		curl_setopt($curl_handle, CURLOPT_POSTFIELDS, json_encode($data));  
		$buffer = curl_exec($curl_handle);  
		curl_close($curl_handle);  
   
		$result = json_decode($buffer); 
		$this->firephp->log($data); 
		$this->firephp->log($buffer); 
		
		$this->firephp->log($result); 
 		if(($result ==json_decode('{"LogINResult":-1}'))  )
		{
	 	
	 	
	 	echo 'fail';
		}  
  
		else  
		{
			
		echo "success";

 		}  	
		
		
		
	}	 
}