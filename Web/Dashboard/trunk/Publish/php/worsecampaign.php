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
 // $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
 $url = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';

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
$diff = $value->Value->clsvalue[2]->ValueData ;
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


