<?php 

class menuRouting{
	
	function index(){
		//$CI =& get_instance();
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
		
		//var_dump($routesArray);
		$MENU_ROUTES = $routesArray;
		//$MENU_ROUTES = $routes;
		//var_dump($routes);
	}
	
	function addRoutesFromMenuItems(&$routesArray, &$menuItems) {
		
		foreach($menuItems as $item) {
			//	print_r($item->ChildItems);
			if (isset($item->MetaData->Controller)) 
			{
				
				$routesArray[$item->Path] = $item->MetaData->Controller;
			
			}
			
			if (isset($item->ChildItems)){
				$this->addRoutesFromMenuItems($routesArray, $item->ChildItems);
				
			}
				
		
	//	if isset(($item->MetaData->Controller == "iframe_controller")){
			 //var_dump($routesArray[$item->Path]);
			//iFrameURL =$routesArray[$item->Path];
			
		//}
			
			
			
		
		
	}
	
		
	}
	
}