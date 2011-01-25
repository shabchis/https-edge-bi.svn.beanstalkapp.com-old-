<?php

class iFrame extends Controller{
	
	function index($accountID){
		global $MENU_IFRAME_URLS;
		global $MENU_IFRAME_WPF ; // not in use
		global $SESSION_ID;
	
		$path = $this->uri->assoc_to_uri($this->uri->uri_to_assoc(3)); 
		$this->firephp->log($MENU_IFRAME_URLS[$path]);
		$iframeurl  = $MENU_IFRAME_URLS[$path];
		$iframeurl = str_replace("{account}",$accountID,$iframeurl);
		$iframeurl = str_replace('{session}',$SESSION_ID,$iframeurl);
		$iframeurl = str_replace('{path}',urlencode($path),$iframeurl);
		
	$IsWPFInSTR = stripos(strtolower($iframeurl), '.xbap');
	//$this->firephp->log($IsWPFInSTR);
	/*
		if ($IsWPFInSTR !== false){
			
			if (!isset($_COOKIE['edgebi_wpf'])) {
				$this->load->view('install');
			}
			else
			 {
				$data=array(
					"iframeurl"=>$iframeurl
					);		
		
		
					$this->load->view('iframe',$data);
					
			}
	
			
		}
		 */
		
		$data=array(
					"iframeurl"=>$iframeurl
					);		
		
		
					$this->load->view('iframe',$data);	
	}
	 

	
	
}
