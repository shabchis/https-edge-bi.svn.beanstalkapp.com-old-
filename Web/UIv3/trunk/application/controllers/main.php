<?php

class Main extends Controller {
	   function __construct()
       {
            parent::Controller();
			//redirect(base_url().'login', 'location', 301);
       }
	function index()
	{
	//	
	$mainurl = 'http://AlonYa-PC/API/EdgeBIAPIService.svc/menu';
 	  $json =  file_get_contents($mainurl);
	$accounturl = 'http://AlonYa-PC/API/EdgeBIAPIService.svc/accounts';
		$json2 =     file_get_contents($accounturl);

     $data["json"]=$json;    
	 $data["json2"]=$json2;    
 $this->firephp->log(json_decode($data["json"]));
		
	$this->load->view('includes/template',$data);		
 
	
	

 
   
    
  }

}

