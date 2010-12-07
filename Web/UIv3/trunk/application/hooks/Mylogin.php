<?php
class Mylogin{
		
		
	function Mylogin()
{
	
		 $CI = null;

       $this->CI =& get_instance();

		
		$this->CI->load->helper('url');
		//$this->CI->load->view('login');
		
	
}
	
	function login(){
			redirect('login');
			
			//$this->CI->load->helper('url');
//header("Location:http://localhost/projects/UIv3");
//	exit;
	
	
		}
		
	
}
