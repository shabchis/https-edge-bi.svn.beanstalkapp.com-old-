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
		//curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session'));
		curl_setopt($curl_handle, CURLOPT_POSTFIELDS, json_encode($data));  
		$buffer = curl_exec($curl_handle);  
		curl_close($curl_handle);  
   
		$result = json_decode($buffer); 
		$this->firephp->log($buffer); 
		//$this->firephp->log(json_encode($data)); 
		
	
 		if(($result ==json_decode('{"LogINResult":-1}'))  )
		{
	 	
	 	
	 	echo json_decode('{"LogINResult":-1}');
		}  
  
		else  
		{
			$this->firephp->log($result); 
			echo json_encode($result);

 		}  	
		
		
		
	}	 
	
	
	function sendsession(){
		
		$data = array(
		"session"=>$this->input->post('session')
		
		);
	
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, 'http://AlonYa-PC/API/EdgeBIAPIService.svc/menu');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session:'.$data["session"].''));
			
		$menu = curl_exec($curl_handle);  
		curl_close($curl_handle);  
   
		
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, 'http://AlonYa-PC/API/EdgeBIAPIService.svc/accounts');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session:'.$data["session"].''));
			
		$accounts = curl_exec($curl_handle);  
		curl_close($curl_handle);  
	
		$data = array(
		"menu"=>$menu,
		"account"=>$accounts
		);
		$this->load->view('includes/template',$data);
	
	}
	
}