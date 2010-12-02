<?php

class Main extends Controller {
	
	function index()
	{
	  
   $json = json_decode('[{"ID":1,"Name":"MainMenu","ParentID":0,"MetaData":null,"childMenus":[{"ID":2,"Name":"home","ParentID":1,"MetaData":null,"childMenus":[{"ID":5,"Name":"home1","Link":"home1","ParentID":2,"MetaData":null,"childMenus":[{"ID":9,"Name":"home1","ParentID":5,"MetaData":null,"childMenus":[]}]},{"ID":6,"Name":"child1Child2","ParentID":2,"MetaData":null,"childMenus":[]},{"ID":7,"Name":"child1child3","ParentID":2,"MetaData":null,"childMenus":[]}]},{"ID":3,"Name":"about","ParentID":1,"MetaData":null,"childMenus":[{"ID":8,"Name":"child2Child1","ParentID":3,"MetaData":null,"childMenus":[]}]},{"ID":4,"Name":"dashboard","ParentID":1,"MetaData":null,"childMenus":[]}]}]');  
    foreach ($json as  $value) {
      $data["json"]=json_encode($value);    
	  
    }
		$this->load->view('includes/template',$data);		
 
	
	

 
   
    
  }

}

