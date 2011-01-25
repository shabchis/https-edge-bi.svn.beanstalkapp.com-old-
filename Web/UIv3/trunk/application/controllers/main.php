<?php

class Main extends Controller {
	  
	function index()
	{
		// Get user data
		$user = $this->edgeapi->Request('/users/'.get_cookie('edgebi_user'));
		
		
		
		// Get menu data
		global $MENU_JSON;
		$menu = $MENU_JSON;
	
		
		// Get account data
	$accounts = $this->edgeapi->Request('/accounts');  
		
	//	$this->firephp->log(json_decode($accounts));
		$data = array(
			"menu"=>$menu,
			"account"=>	stripslashes($accounts),
			"user"=>$user
			);
		//$this->firephp->log(json_decode($data["account"]));
		$this->load->view('includes/template',$data);	
		 
		
			
  }

}

