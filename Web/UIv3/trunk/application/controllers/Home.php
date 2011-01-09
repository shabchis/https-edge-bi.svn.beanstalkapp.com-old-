<?php

class Home extends Controller {
  
  function index($accountID = null)
  {
      $this->load->view('home'); 
  }  
  


}
