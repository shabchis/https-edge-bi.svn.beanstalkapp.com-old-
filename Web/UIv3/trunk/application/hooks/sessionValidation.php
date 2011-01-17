<?php
require_once ('application/libraries/EdgeApi.php');
require_once ('application/libraries/Errors.php');

class sessionValidation{

	var $errors;
	var $edgeapi;
	function __construct(){
		$this->errors = new Errors(); 
		$this->edgeapi = new EdgeApi(); 
	}

	function index(){
			
		global $APPLICATION_ROOT;
		global $REQUEST_PATH;
		
		// Check if there is a valid session
		if($REQUEST_PATH != 'login' && !isset($_COOKIE['edgebi_session']))
		{
			$this->errors->ThrowEx('Please log in.', 403, null, true);	
		}
		
		// Check that request path is allowed for this user
		$segments = array();
		if (preg_match('/accounts\/([0-9]+)(?:\/(.*))*/i', $REQUEST_PATH, $segments))
		{
			$accountID = $segments[1];
			$path = count($segments) == 3 ? $segments[2] : '';
			
			if (!IS_AJAX)
			{
				// Don't allow non-ajax access to pages
				$this->errors->Redirect(301, $APPLICATION_ROOT."#".$REQUEST_PATH);
			}
			
			$data =array(
				"AccountID"=>$accountID,
				"Path"=>$path
			);
			
			$curl_handle = curl_init();    
			curl_setopt($curl_handle, CURLOPT_POST, 1); 
			curl_setopt($curl_handle, CURLOPT_POSTFIELDS, json_encode($data));  
			
			// exec the request and get status
			$result = $this->edgeapi->Request('/permissions', $curl_handle, true, array('Content-Type: application/json'));

			curl_close($curl_handle);
			
			if ($result != 'true')
			{
				show_error('The account you requested was not found.', 404);
			}
		}
	}
	
}
