<?php  header ("Content-Type:text/xml");?>

<?php $startdate = $_GET['startDate'];?>
<?php $endDate = $_GET['endDate'];?>
<?php 
	switch ($endDate){
		case 1: 
		$startDateName = $startdate;
		$endDateName =  '20100912';
		break;
		case 2: 
		$startDateName = "Previous 7 days";
		$endDateName = "Last 7 days";
		break;
		case 3: 
		$startDateName = "Previous 30 days";
		$endDateName = "Last 30 days";
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
?>
<?php $me = $_GET['measure']; ?>
<?php $url =  'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID=7&MeasureID='.$me.'&DateRanges=20100910-20100912,20100990-20100912&Diff=True&Group_By=channel&Top=10';?>

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