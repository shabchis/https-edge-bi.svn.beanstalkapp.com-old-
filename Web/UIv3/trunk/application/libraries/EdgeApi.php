<?php

require_once ('application/libraries/Errors.php');

class EdgeApi
{
	var $errors;
	function __construct(){
		$this->errors = new Errors(); 
	}
	
	function Request($url, $curl_handle=null, $includeSession=true, $headers=null)
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
			array_push($headers, 'x-edgebi-session:'.$_COOKIE['edgebi_session']);
		
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,$headers);
		
		// exec the request
		$result = curl_exec($curl_handle);
		$error = curl_errno($curl_handle);
		
		if($error)
		{
			curl_close($curl_handle);
			$this->errors->ThrowEx(curl_error($curl_handle), 500);
		}
		else
		{
			$status = curl_getinfo($curl_handle, CURLINFO_HTTP_CODE);
			if ($shouldClose)
				curl_close($curl_handle);
			
			$json = json_decode($result);
			
			if ($status != 200)
			{ 
				$this->errors->ThrowEx(null, $status, $json, $status == 403);
			}
			
			
			return $result;
		}
	}
	
	
	function Login($data, $setCookies)
	{
		$curl_handle = curl_init();    
		curl_setopt($curl_handle, CURLOPT_POST, 1); 
		curl_setopt($curl_handle, CURLOPT_POSTFIELDS, json_encode($data));  
		
		// exec the request and get status
		$result = $this->Request('/sessions', $curl_handle, false, array('Content-Type: application/json'));
		curl_close($curl_handle);
		
		$response = json_decode($result);
		if ($setCookies)
		{
			setcookie("edgebi_session", $response->Session, 0, '/');
			setcookie("edgebi_user", $response->UserID, time()+60*60*24*14, '/');
		}
	}
}
