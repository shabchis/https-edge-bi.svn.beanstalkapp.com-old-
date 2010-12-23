<?php

class Main extends Controller {
	   function __construct()
       {
            parent::Controller();
			//redirect(base_url().'login', 'location', 301);
       }
	function index()
	{
		
		$data = array(
		"session"=>get_cookie('edgebi_session'),
		"id"=>get_cookie('edgebi_user')
		
		);
		   $this->firephp->log($data);
	
	$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/users/'.$data["id"]);  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array(
			'accept: application/json',
			'x-edgebi-session:'.$_COOKIE['edgebi_session']
		));
		
			
		$user = curl_exec($curl_handle);  
		curl_close($curl_handle);  
		

		global $MENU_JSON;
		$menu = $MENU_JSON;
		
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/accounts');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array(
			'accept: application/json',
			'x-edgebi-session:'.$_COOKIE['edgebi_session']
		));		
			
		$accounts = curl_exec($curl_handle);  
		curl_close($curl_handle);  
	
		$data = array(
		"menu"=>$menu,
		"account"=>$accounts,
		"user"=>$user
		);
	//	


 	    $this->firephp->log(json_decode($data["user"]));
		$this->load->view('includes/template',$data);		
	


  }

}

