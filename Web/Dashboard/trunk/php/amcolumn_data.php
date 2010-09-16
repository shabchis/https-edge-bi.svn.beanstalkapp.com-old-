<?php  header ("Content-Type:text/xml");
		
		 $startdate = isset($_GET['startDate'])?$_GET['startDate']:"";
		 $endDate = isset($_GET['endDate'])?$_GET['endDate']:"";
		 $measure = $_GET['measure'];

	switch (isset($endDate)){
	case 1: 
		$startDateName = $startdate;
		$endDateName =   (date("dmy", mktime(0,0,0,date("m"),date("d")-2,date("Y"))));
		break;
	case 2: 
		$startDateName = date("dmy", mktime(0,0,0,date("m"),date("d")-7,date("Y")));
		$endDateName = date("dmy", mktime(0,0,0,date("m"),date("d")-7,date("Y")));
		break;
	case 3: 
		$startDateName = date("dmy", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		$endDateName = date("dmy", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 4: 
		$startDateName = "Last Month";
		$endDateName = "This Month";
		break;
	case 5: 
		$startDateName = "Last Month";
		$endDateName = "Previous Month";
		break;
	case 6: 
		$startDateName = "This Week";
		$endDateName = "Last Week";
		break;
	case 7: 
		$startDateName = "Previous Week";
		$endDateName = "Last Week";
		break;
	}


 $url =  'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID=7&MeasureID='.$measure.'&DateRanges=20100908-20100910,20100908-20100914&Diff=True&Group_By=channel&Top=10';
 

 
 ?>

<chart>
  <series>

  <?php
  if(!$xml=simplexml_load_file($url)){
    trigger_error('Error reading XML file',E_USER_ERROR);
}

foreach($xml as $measure){
    echo '<value xid="'.$measure->ID.'"> '.$measure->Name.'</value>' ;
	
}
?>

  </series>
  <graphs>
  <?php
    echo '<graph gid="0">';
	foreach($xml as  $value){
    echo '<value xid="'.$value->ID.'"> '.$value->Value->clsvalue[0]->ValueData.'</value>' ;

}
     
	  ?>
	
    </graph>
		 
	<?php
   echo '<graph gid="1">';
      foreach($xml as  $value){
    echo '<value xid="'.$value->ID.'"> '.$value->Value->clsvalue[1]->ValueData.'</value>' ;
}
     
	  ?>
		
    </graph>
	
  </graphs>
</chart>