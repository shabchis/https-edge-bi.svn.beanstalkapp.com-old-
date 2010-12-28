<?php

require_once ('application/libraries/Errors.php');

class sessionValidation{

	var $errors;
	function __construct(){
		$this->errors = new Errors(); 
	}


	function index(){
		
		global $REQUEST_PATH;
		
		if($REQUEST_PATH != 'login' && !isset($_COOKIE['edgebi_session']))
		{
			$this->errors->ThrowEx('Please log in.', 403, null, true);	
		}
			
	}
	
}
