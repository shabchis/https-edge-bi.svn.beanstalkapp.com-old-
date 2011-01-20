<?php

require_once ('application/libraries/Errors.php');

class EdgeApi
{
	var $errors;
	function __construct(){
		$this->errors = new Errors(); 
	}
	
	function Request($url, $includeSession=true, $headers=null, $postData=null,$autoLogin=true)
	{
		// autologin only if cookie is enabled
		$autoLogin = $autoLogin && isset($_COOKIE['edgebi_remember']);
		
		global $SESSION_ID;
		$curl_handle = curl_init();
		
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
			if (isset($SESSION_ID))
				array_push($headers, 'x-edgebi-session:'.$SESSION_ID);
			else
				$this->errors->ThrowEx('Please log in.', 403, null, true);
		}
		
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,$headers);
		
		if ($postData)
		{
			curl_setopt($curl_handle, CURLOPT_POST, 1); 
			curl_setopt($curl_handle, CURLOPT_POSTFIELDS, json_encode($postData));
		}
		
		//==========================
		$result = null;
		$attempting = true;
		
		while($attempting)
		{
			$attempting = false;
			
			// exec the request
			$result = curl_exec($curl_handle);
			$error = curl_errno($curl_handle);
			
			if($error)
			{
				curl_close($curl_handle);
				$this->errors->ThrowEx('An Edge API request failed: ' . curl_error($curl_handle), 500);
			}
			else
			{
				$status = curl_getinfo($curl_handle, CURLINFO_HTTP_CODE);
								
				// we need to try autologin later on, so copy the request details
				$autoLoginNeeded = $autoLogin && $includeSession && $status == 403 && isset($SESSION_ID) && isset($_COOKIE['edgebi_user']);
				if ($status != 200 && $autoLoginNeeded)
					$temp = curl_copy_handle($curl_handle);
				
				// Close the handle
				curl_close($curl_handle);
					
				if ($status != 200 && $autoLoginNeeded)
					$curl_handle = $temp;
				
				$json = json_decode($result);
				
				if ($status != 200)
				{
					// auto-login if possible
					if ($autoLoginNeeded)
					{
						$data = array(
							"OperationType" => 'Renew',
							"UserID" => $_COOKIE['edgebi_user'],
							"Session"=> $SESSION_ID
						);
						
						// try to renew the session
						$this->Login($data, true, false);
						
						// Apply the new session and reapply the post data
						array_pop($headers);
						array_push($headers, 'x-edgebi-session:'.$SESSION_ID);
						curl_setopt($curl_handle, CURLOPT_HTTPHEADER, $headers);
						if ($postData)
							curl_setopt($curl_handle, CURLOPT_POSTFIELDS, json_encode($postData));
					
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
		// exec the request and get status
		$result = $this->Request('/sessions', false, array('Content-Type: application/json'), $data, false);
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

		if ($remember)
			setcookie("edgebi_remember", true, $expiration, '/');
		
		global $SESSION_ID; $SESSION_ID = $response->Session;
		
		return $response;
	}
}
