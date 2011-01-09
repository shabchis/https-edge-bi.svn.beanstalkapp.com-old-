<?php
class Dashboard extends Controller {
  
  function index($accountID)
  {
    
    if (strpos($this->agent->referrer(), base_url()) !== 0 )
    {
      //redirect
      redirect(base_url().'#'.$this->uri->uri_string(), 'location', 301);
    }
    else
    {
      $this->load->view('dashboard');
    }
  }

}