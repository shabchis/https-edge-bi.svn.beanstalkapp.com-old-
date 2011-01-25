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
		global $SESSION_ID;
		global $ACCOUNT_ID;
		
		$SESSION_ID = isset($_COOKIE['edgebi_session']) ? $_COOKIE['edgebi_session'] : null;
		
		// Check if there is a valid session
		if($REQUEST_PATH != 'login' && !isset($SESSION_ID))
		{
			$this->errors->ThrowEx('Please log in.', 403, null, true);	
		}
		
		// Check that request path is allowed for this user
		$segments = array();
		if (preg_match('/accounts\/([0-9]+)(?:\/(.*))*/i', $REQUEST_PATH, $segments))
		{
			$ACCOUNT_ID =  $segments[1];
			$path = count($segments) == 3 ? $segments[2] : '';
			
			if (!IS_AJAX)
			{
				// Don't allow non-ajax access to pages
				$this->errors->Redirect(301, $APPLICATION_ROOT."#".$REQUEST_PATH);
			}
			
			$data =array(
				"AccountID"=>$ACCOUNT_ID,
				"Path"=>$path
			);
			
			// exec the request and get status
			$result = $this->edgeapi->Request('/permissions', true, array('Content-Type: application/json'), $data);
			
			if ($result != 'true')
			{
				show_error('The account you requested was not found.', 404);
			}
		}
	}
	
}
