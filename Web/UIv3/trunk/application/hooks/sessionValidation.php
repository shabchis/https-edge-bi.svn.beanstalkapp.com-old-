<?php

class sessionValidation{
	
	function index(){
		
		global $REQUEST_PATH;
		
		if($REQUEST_PATH != 'login' && !isset($_COOKIE['edgebi_session']))
		{
			header( 'Location: http://localhost/projects/login' ) ;
			exit;	
		}
			
	}
	
}
