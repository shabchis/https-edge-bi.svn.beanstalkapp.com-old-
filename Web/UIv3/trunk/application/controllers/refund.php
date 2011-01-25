<?php

class Refund extends Controller{
	
	function index(){
		
		$this->load->view('refund');		
	}
	function delete(){
		
		$this->load->view('delete_refund');		
		
	}
	function proccess(){
		
		$data=array(
			"AccountID"=>$this->input->post('accountID'),
			"Month" =>$this->input->post('date'),
			"RefundAmount" =>$this->input->post('refund'),
			"ChannelID"=>$this->input->post('channel')
		);		
		
		
		$result = $this->edgeapi->Request('/tools/refund', true, array('Content-Type: application/json'), $data, true);
		
	}	
	
	function deleteprocess(){
		
			$data=array(
			"AccountID"=>$this->input->post('accountID'),
			"Month" =>$this->input->post('date'),
			"ChannelID"=>$this->input->post('channel')
		);		
		
		
		$result = $this->edgeapi->Request('/tools/deleterefund', true, array('Content-Type: application/json'), $data, true);
		
		
	}
}