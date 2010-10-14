<?php  header("Content-Type:text/xml"); 
 $startdate = $_GET['startdate']; 
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	
	$global = $_GET['global'];
	
$before = array('(',')');
	include ('timeParse.php');
?>

<?php
// $url = 'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID='.$account_id.'&MeasureID='.$measureId.'&DateRanges='.$startDateName1.'-'.$startDateName2.','.$endDateName1.'-'.$endDateName2.'&Diff=True&Group_By=campaign&Top=10&Data_Sort=VALUE1&Data_Sort_Dir='.$orderby.'&View_Sort=Diff2';
// $url = 'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID='.$account_id.'&Calc_Measures='.$measureId.'&MeasureID=17&DateRanges='.$startDateName1.'-'.$startDateName2.','.$endDateName1.'-'.$endDateName2.'&Diff=True&Group_By=campaign&Top=10&Data_Sort=VALUE1&Data_Sort_Dir='.$orderby.'&View_Sort=Diff2';
// $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$measureId.'&ranges='.$startDateName1.'-'.$startDateName2.','.$endDateName1.'-'.$endDateName2.'&grouping=campaign&diff=True&top=10&dataSort=VALUE1&viewSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'';//old
 $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=value1&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&viewSort=Diff2 '.$orderby.'';

 // echo $url;
    if(!$xml=simplexml_load_file($url)){
    // trigger_error('Error reading XML file', E_USER_ERROR);
	header('HTTP/1.1 500 Internal Server Error');
  // trigger_error('Error reading XML file',E_USER_ERROR);
 echo '<xml>';
 echo '<error>There is an error in the web service</error>';
  echo '<address>'.$url.'</address>';
 echo '</xml>';
}
	// if (strlen($xml) == 0){
		// header('HTTP/1.1 500 Internal Server Error');
		// echo '<error>No data found for the selection criteria.</error>';
	  // echo '<address>'.$url.'</address>';
	// }
	else{
	
	echo '<campaigns> ';
	

foreach($xml as  $value){
	
$diff = $value->Value->clsvalue[2]->ValueData;
$name = $value->Name;
$fullname = $value->Name;
if(strlen($diff) >= 0){
$diff = ''.$value->Value->clsvalue[2]->ValueData.'%' ;
}
else {
	$diff = "";
}

if (empty($name)){
$name = "";
}
if(strlen($name) > 20){
	$name = substr($value->Name,0,17)."...";
}
else {
$name = $name = $value->Name;
}

	echo '<campaign>';
	 echo '<fullname>'.trim($fullname).'</fullname>';
    echo '<name>'.$name.'</name>';
	echo '<value>('.str_replace($before,"",$value->Value->clsvalue[0]->ValueData).')</value>';
	echo '<diff>'.$diff.'</diff>';
	echo '</campaign>';
}
echo '</campaigns>';
}
	?>


