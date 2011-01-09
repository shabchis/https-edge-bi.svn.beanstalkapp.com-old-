<?php

class iFrame extends Controller{
	
	function index($accountID){
		global $MENU_IFRAME_URLS;
		global $MENU_IFRAME_WPF ; // not in use
	
		$path = $this->uri->assoc_to_uri($this->uri->uri_to_assoc(3)); 
		$this->firephp->log($MENU_IFRAME_URLS[$path]);
		$iframeurl  = $MENU_IFRAME_URLS[$path];
		$iframeurl = str_replace("{account}",$accountID,$iframeurl);
		$iframeurl = str_replace('{session}',get_cookie("edgebi_session"),$iframeurl);
		$iframeurl = str_replace('{path}',urlencode($path),$iframeurl);
		
	//	$IsWPFInSTR = stripos(strtolower($iframeurl), '/wpf/');
	//$this->firephp->log($IsWPFInSTR);
	/*
		if ($IsWPFInSTR !== false){
			
			if (!isset($_COOKIE['wpf'])) {
				setcookie("wpf", true, time()+86400*365, '/');
				$iframeurl = base_url().'install.html';
				$this->firephp->log('install page');
			}
			else {
					setcookie("wpf", true, time()+86400*365, '/');
					
			}
	//	$MENU_IFRAME_WPF['Wpf'] = true;
			
		}
		*/
		$data=array(
			"iframeurl"=>$iframeurl
		);		
		
		
		$this->load->view('iframe',$data);
			
	}
	
	
}
