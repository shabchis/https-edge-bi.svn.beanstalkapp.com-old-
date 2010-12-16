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
		"session"=>$this->input->post('session')
		
		);
	
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/menu');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session:'.$data["session"].''));
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('accept: application/json'));
		
			
		$menu = curl_exec($curl_handle);  
		curl_close($curl_handle);  
   
		
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/accounts');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session:'.$data["session"].''));
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('accept: application/json'));
		
			
		$accounts = curl_exec($curl_handle);  
		curl_close($curl_handle);  
	
		$data = array(
		"menu"=>$menu,
		"account"=>$accounts
		);
	//	

//	$this->firephp->log($data); 
//$mainurl = EDGE_API_URL.'/menu';
 //	 $json =  file_get_contents($mainurl);
//$accounturl = EDGE_API_URL.'/accounts';
	//$json2 =     file_get_contents($accounturl);

//$testurl = 'http://AlonYa-PC:53448/WcfHttpLearning/menu';
//$json3 =  file_get_contents($testurl);
 //$data["json"]=$json;    
 //$data["json2"]=$json2;    
	  //$data["json3"]=$json3;
$this->firephp->log($data);
	
	$this->load->view('includes/template',$data);		
 
	
	

 
   
    
  }

}

