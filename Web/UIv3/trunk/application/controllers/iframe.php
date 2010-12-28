<?php

class iFrame extends Controller{
	
	function index(){
		global $MENU_IFRAME_URLS;
	
			$iframeurl  = 	$MENU_IFRAME_URLS[$this->uri->uri_string()];
			$iframeurl = str_replace("{account}",get_cookie('edgebi_child_account'),$iframeurl);
			$iframeurl = str_replace('{session}',get_cookie("edgebi_session"),$iframeurl);
			$iframeurl = str_replace('{path}',urlencode($this->uri->uri_string()),$iframeurl);
			$data=array(
			"iframeurl"=>$iframeurl
		);
		
		$this->load->view('iframe',$data);
			
	}
	
	
}
