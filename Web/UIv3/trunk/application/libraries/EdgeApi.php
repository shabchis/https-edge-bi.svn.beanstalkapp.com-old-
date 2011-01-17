<?php

require_once ('application/libraries/Errors.php');

class EdgeApi
{
	var $errors;
	function __construct(){
		$this->errors = new Errors(); 
	}
	
	function Request($url, $curl_handle=null, $includeSession=true, $headers=null, $autoLogin=true)
	{
		$shouldClose = false;
		if (!$curl_handle)
		{
			$shouldClose = true;
			$curl_handle = curl_init();
		}
		
		// Set URL
		if ($url)
			curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.$url);
			
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);
		
		// Set headers
		if (!$headers)
			$headers = array();
		array_push($headers, 'Accept: application/json');
		if ($includeSession)
		{
			if (isset($_COOKIE['edgebi_session']))
				array_push($headers, 'x-edgebi-session:'.$_COOKIE['edgebi_session']);
			else
				$this->errors->ThrowEx('Please log in.', 403, null, true);
		}
		
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,$headers);
		
		
		//==========================
		$result = null;
		$attempting = true;
		
		while($attempting)
		{
			
			// exec the request
			$result = curl_exec($curl_handle);
			$error = curl_errno($curl_handle);
			
			if($error)
			{
				curl_close($curl_handle);
				//$this->errors->ThrowEx(curl_error($curl_handle), 500);
				$this->errors->ThrowEx('An Edge API request failed.', 500);
			}
			else
			{
				$status = curl_getinfo($curl_handle, CURLINFO_HTTP_CODE);
				$autoLoginNeeded = $autoLogin && $status == 403 && isset($_COOKIE['edgebi_session']) && isset($_COOKIE['edgebi_user']);
				
				// close the curl handle if we're about to throw an exception
				if ($shouldClose || $status != 200)
				{
					// we need to try autologin later on, so copy the request details
					if($autoLoginNeeded)
						$temp = curl_copy_handle($curl_handle);
					
					curl_close($curl_handle);
					
					if($autoLoginNeeded)
						$curl_handle = $temp;
				}
				
				$json = json_decode($result);
				$attempting = false;
				
				if ($status != 200)
				{
					// auto-login if possible
					if ($autoLoginNeeded)
					{
						$data = array(
							"OperationType" => 'Renew',
							"UserID" => $_COOKIE['edgebi_user'],
							"Session"=> $_COOKIE['edgebi_session']
						);
						
						// try to renew the session
						$loginDetails = $this->Login($data, true, false);
						
						// apply the new session id
						curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session:'.$loginDetails->Session));
						
						// we need to attempt the operation again, but without autologin
						$attempting = true;
						$autoLogin = false;
					}
					// else throw an error and redirect to login page if it is forbidden
					else
					{
						$this->errors->ThrowEx(null, $status, $json, $status == 403);
					}
				}
			}
		}

		return $result;		
	}
	
	
	function Login($data, $remember = false, $clearCookies = true)
	{
		$curl_handle = curl_init();    
		curl_setopt($curl_handle, CURLOPT_POST, 1); 
		curl_setopt($curl_handle, CURLOPT_POSTFIELDS, json_encode($data));  
		
		// exec the request and get status
		$result = $this->Request('/sessions', $curl_handle, false, array('Content-Type: application/json'), false);
		curl_close($curl_handle);
		
		$response = json_decode($result);
		if ($clearCookies)
		{
			delete_cookie("edgebi_child_account");
			delete_cookie("edgebi_parent_account");
		}
			
		// Determines if cookies are persisted for later
		$expiration = $remember ? time()+60*60*24*14 : 0;
			
		setcookie("edgebi_session", $response->Session, $expiration, '/');
		setcookie("edgebi_user", $response->UserID, $expiration, '/');
		
		return $response;
	}
}
