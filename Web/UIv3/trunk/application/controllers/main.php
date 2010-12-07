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
	$url = 'http://AlonYa-PC/API/EdgeBIAPIService.svc/menu';
 	  $json =  file_get_contents($url);
	

   
     $data["json"]=$json;    
	
    //$this->firephp->log($data["json"]);

	$this->load->view('includes/template',$data);		
 
	
	

 
   
    
  }

}

