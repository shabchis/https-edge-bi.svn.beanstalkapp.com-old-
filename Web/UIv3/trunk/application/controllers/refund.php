<?php

class Refund extends Controller{
	
	function index($accountID){
		
			$data=array(
			"accountID"=>$accountID
		);		
		$this->load->view('refund',$data);
	}
	
}