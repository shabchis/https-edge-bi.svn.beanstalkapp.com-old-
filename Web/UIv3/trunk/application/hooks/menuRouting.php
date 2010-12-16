<?php 

class menuRouting{
	
	function index(){
		
		global $MENU_ROUTES;
		
		
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/menu');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		//curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session:'.$data["session"].''));
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('accept: application/json'));
		$menujson = curl_exec($curl_handle);  
		curl_close($curl_handle);  
	
		$routesArray = array();
		$menuItems = json_decode($menujson);
		$this->addRoutesFromMenuItems($routesArray,$menuItems);
		$MENU_ROUTES = $routesArray;
		//var_dump($routesArray);
	}
	
	function addRoutesFromMenuItems($routesArray, $menuItems) {
		
		foreach($menuItems as $item) {
	//var_dump($item->MetaData);
			if (isset($item->MetaData->Controller)) 
			{
				
				$routesArray[$item->Path] = $item->MetaData->Controller;
			}
			
			if (isset($item->MetaData->ChildItems))
				 addRoutesFromMenuItems($routesArray, $item->MetaData->ChildItems);
		}
		
	}
	
	
}