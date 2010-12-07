<?php
class Mylogin{
		
		
	function Mylogin()
{
	
		 $CI = null;
		
		$this->CI =& get_instance();

		
		$this->CI->load->helper('url');
		//$this->CI->load->view('login');
		 //redirect(base_url());
	
}
	
	function userlogin(){
			//redirect('main');
		//	$this->CI->load->view('home');
			//$this->CI->load->helper('url');
			$logged = false;
			if($logged==true){
				header("Location:http://localhost/projects/UIv3/#home");
				
			}

//	exit;
	
	
		}
		
	
}
