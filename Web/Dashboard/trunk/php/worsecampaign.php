<?php  header ("Content-Type:text/xml");

	$startdate = $_GET['startdate'];
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	
	switch ($startdate){
		case 1:
			$yesterdayStart  =  date("Ymd", mktime(0,0,0,date("m"),date("d")-2,date("Y")));
		break;
		case 2:
			$yesterdayStart  =  date("Ymd", mktime(0,0,0,date("m"),date("d")-8,date("Y")));
			break;
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
?>
<campaigns>
<?php

 // $url = 'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID=95&MeasureID='.$measureId.'&DateRanges='.$startDateName1.'-'.$startDateName2.','.$endDateName1.'-'.$endDateName2.'&Diff=True&Group_By=campaign&Top=10&dataSort=Diff2&viewSort=value1&dataSortDir='.$orderby.'';
$url = 'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID=61&MeasureID=1&DateRanges=20100801-20100901,20100820-20100831&Diff=True&Group_By=campaign&Top=10&dataSort=Diff2&viewSort=value1&dataSortDir=ASC';

    if(!$xml=simplexml_load_file($url)){
    trigger_error('Error reading XML file', E_USER_ERROR);
}

foreach($xml as  $value){
	echo '<campaign>';
    echo '<name>'.$value->Name.'</name>';
	echo '<value>'.$value->Value->clsvalue[0]->ValueData.'</value>';
	echo '<diff>'.$value->Value->clsvalue[2]->ValueData.'</diff>';
	echo '</campaign>';
}

	?>

	</campaigns>
