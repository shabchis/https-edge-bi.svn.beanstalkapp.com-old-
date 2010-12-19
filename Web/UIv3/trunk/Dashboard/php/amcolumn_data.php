<?php  header ("Content-Type:text/xml");
			$Account_id = $_GET['account_id'];
			$startdate = $_GET['startdate'];
			$endDate = $_GET['endDate'];
			$measureId = $_GET['measure'];
				$global = $_GET['global'];
			$before   = array('(',')','$', ',');
			$after   = array('-','""','""', '""');
	
	include ('timeParse.php');

$url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$Account_id.'&measureID='.$measureId.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&grouping=channel&top=10&dataSort=value1&dataSortDir=DESC&displayMeasures=20,19&functionDisplayMeasures='.$measureId.'';
// $url = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$Account_id.'&measureID='.$measureId.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&grouping=channel&diff=True&top=10&dataSort=value1&dataSortDir=DESC&displayMeasures=20&functionDisplayMeasures='.$measureId.'';
// echo $url;
 ?>

<chart>
  <series>

  <?php
  if(!$xml=simplexml_load_file($url)){
  
  // header('HTTP/1.1 500 Internal Server Error');
    // trigger_error('Error reading XML file',E_USER_ERROR);
	
	  // echo $url;
}

	
foreach($xml as $measure){

    echo '<value xid="'.$measure->ID.'"> '.str_replace($before,"",$measure->Name).'</value>' ;
	
}
?>

  </series>
  <graphs>
  <?php
    echo '<graph gid="0">';
	foreach($xml as  $value){
    
    echo '<value xid="'.$value->ID.'" description="'.trim($value->Value->clsvalue[2]->ValueData.' / '.$value->Value->clsvalue[3]->ValueData).'%">'.trim(str_replace($before,"",$value->Value->clsvalue[0]->ValueData)).'</value>' ;

}
     
	  ?>
	
    </graph>
		 
	<?php
   echo '<graph gid="1">';
      foreach($xml as  $value){
	
    echo '<value xid="'.$value->ID.'" description="'.trim($value->Value->clsvalue[2]->ValueData.'/'.$value->Value->clsvalue[3]->ValueData).'%"> '.trim(str_replace($before,"",$value->Value->clsvalue[1]->ValueData)).'</value>' ;
}
     
	  ?>
		
    </graph>
	
  </graphs>
  
</chart>