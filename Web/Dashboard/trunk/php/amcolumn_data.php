<?php  header ("Content-Type:text/xml");
		
			$startdate = $_GET['startdate'];
			$endDate = $_GET['endDate'];
			$measureId = $_GET['measure'];
	switch ($startdate){
		case 1:
			$yesterdayStart  =  date("Ymd", mktime(0,0,0,date("m"),date("d")-2,date("Y")));
	
		case 2:
			$yesterdayStart  =  date("Ymd", mktime(0,0,0,date("m"),date("d")-8,date("Y")));
			}
	switch ($endDate){
	case 1: 
		 $startDateName1 = $yesterdayStart;
		 $startDateName2 = $yesterdayStart;
		 $endDateName1 =    date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		 $endDateName2 =    date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 2: 
		$startDateName1 = date("Ymd", mktime(0,0,0,date("m"),date("d")-15,date("Y")));
		$startDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-8,date("Y")));
		$endDateName1 = date("Ymd", mktime(0,0,0,date("m"),date("d")-8,date("Y")));
		$endDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 3: 
		$startDateName1 =(date("Ymd", mktime(0,0,0,date("m")-2,date("d")-1,date("Y"))));
		$startDateName2 = date("Ymd", mktime(0,0,0,date("m")-1,date("d")-1,date("Y")));
		$endDateName1 = date("Ymd", mktime(0,0,0,date("m")-1,date("d")-1,date("Y")));
		$endDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 4: 
		$startDateName1 =date("Ymd",strtotime('-2 month'));
		$startDateName2 = date("Ymd",strtotime('-1 month'));
		$endDateName1 = date("Ymd",strtotime('-1 month'));
		$endDateName2 =date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 5: 
		$startDateName1 =date("Ymd",strtotime('-3 month'));
		$startDateName2 = date("Ymd",strtotime('-2 month'));
		$endDateName1 = date("Ymd",strtotime('-2 month'));
		$endDateName2 =date("Ymd",strtotime('-1 month'));
		break;
	case 6: 
		$startDateName1 =date("Ymd",strtotime('-2 week'));
		$startDateName2 = date("Ymd",strtotime('last sunday'));
		$endDateName1 = date("Ymd",strtotime('last sunday'));
		$endDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 7: 
		$startDateName1 =date("Ymd",strtotime('-3 week'));
		$startDateName2 = date("Ymd",strtotime('-2 week'));
		$endDateName1 = date("Ymd",strtotime('-2 week'));
		$endDateName2 = date("Ymd",strtotime('last sunday'));
		break;
	}

// $url ='http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID=7&MeasureID=1&DateRanges=20100917-20100917,20100811-20100917&Diff=True&Group_By=channel&Top=10';
 $url = 'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID=95&MeasureID='.$measureId.'&DateRanges='.$startDateName1.'-'.$startDateName2.','.$endDateName1.'-'.$endDateName2.'&Diff=True&Group_By=channel&Top=10';
 

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

    echo '<value xid="'.$value->ID.'">'.$value->Value->clsvalue[0]->ValueData.'</value>' ;

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