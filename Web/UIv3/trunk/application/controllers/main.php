<?php

class Main extends Controller {
	   function __construct()
       {
            parent::Controller();
			//redirect(base_url().'login', 'location', 301);
       }
	function index()
	{
		
		$data = array(
		"session"=>$this->input->post('session')
		
		);
	
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/menu');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session:'.$data["session"].''));
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('accept: application/json'));
		
			
		$menu = curl_exec($curl_handle);  
		curl_close($curl_handle);  
   
		
		$curl_handle = curl_init();  
		curl_setopt($curl_handle, CURLOPT_URL, EDGE_API_URL.'/accounts');  
		curl_setopt($curl_handle, CURLOPT_RETURNTRANSFER, 1);  
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('x-edgebi-session:'.$data["session"].''));
		curl_setopt($curl_handle, CURLOPT_HTTPHEADER,array('accept: application/json'));
		
			
		$accounts = curl_exec($curl_handle);  
		curl_close($curl_handle);  
	
		$data = array(
		"menu"=>$menu,
		"account"=>$accounts
		);
	//	

/*
$mainurl = 'http://alonya-pc/API/EdgeBIAPIService.svc/menu';
$json =  file_get_contents($mainurl);
$accounturl = 'http://alonya-pc/API/EdgeBIAPIService.svc/accounts';
$json2 =     file_get_contents($accounturl);
//$json = '';
//$testurl = 'http://AlonYa-PC:53448/WcfHttpLearning/menu';
//$json2 ='';
//$json3 =  file_get_contents($testurl);
$data["json"]=$json;    
$data["json2"]=$json2;    
//$data["json3"]=$json3;

//echo $json;
	//echo $json2
 * 
 /*
 */
	$this->load->view('includes/template',$data);		
//$this->load->view('includes/tmpl');		

	
	   // $this->firephp->log(json_decode($data["account"]));

$cookie = array(
'name' => 'Uiv3',
'value' => 'yes',
'expire' => '86500',
'domain' => 'localhost',
'prefix' => 'manu_'

);

set_cookie($cookie);
   
	 
	 get_cookie();
  }

function getmenus(){
	$mainurl = 'http://alonya-pc/API/EdgeBIAPIService.svc/menu';
$json =  file_get_contents($mainurl);
$accounturl = 'http://alonya-pc/API/EdgeBIAPIService.svc/accounts';
$json2 =     file_get_contents($accounturl);
//$json = '';
//$testurl = 'http://AlonYa-PC:53448/WcfHttpLearning/menu';
//$json2 ='';
//$json3 =  file_get_contents($testurl);
$data["json"]=$json;    
$data["json2"]=$json2;    
//$data["json3"]=$json3;
	
	
	echo $data["json"];
	
	
	
}

function getFavicon(){

$linkurl = $_GET['url'];

$linkurl = str_replace("http://",'',$linkurl); // remove protocol from the domain

$imgurl = "http://www.google.com/s2/favicons?domain=" . $linkurl;

echo '<img src="' . $imgurl . '" width="16" height="16" />';

}

}

