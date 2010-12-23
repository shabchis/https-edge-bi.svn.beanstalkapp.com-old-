<?php 

class menuRouting{
	
	function index(){
		
		// If not session, we can't load the menu yet
		global $REQUEST_PATH;
		if($REQUEST_PATH == 'login')
			return;
		
		global $MENU_ROUTES;
		global $MENU_IFRAME_URLS;
		
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/menu');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('accept: application/json','x-edgebi-session:'.$_COOKIE['edgebi_session']));
		$menujson = curl_exec($curl_handle);  
		curl_close($curl_handle);  

		// Save menu for later
		global $MENU_JSON;
		$MENU_JSON = $menujson;
	
		$menuItems = json_decode($menujson);
		
		$routesArray = array();
		$MENU_IFRAME_URLS = array();
			
		$this->addRoutesFromMenuItems($routesArray,$menuItems);
		
		$MENU_ROUTES = $routesArray;
	}
	
	function addRoutesFromMenuItems(&$routesArray, &$menuItems)
	{
			global $MENU_IFRAME_URLS;
		foreach($menuItems as $item)
		{
			if (isset($item->MetaData->Controller)) 
			{
				$routesArray[$item->Path] = $item->MetaData->Controller;			

				if(isset($item->MetaData->iFrameURL)){
					$MENU_IFRAME_URLS[$item->Path] = $item->MetaData->iFrameURL;
					
			
					
				}
					
			}
			
			if (isset($item->ChildItems))
				$this->addRoutesFromMenuItems($routesArray, $item->ChildItems);
		}
	}
	
}