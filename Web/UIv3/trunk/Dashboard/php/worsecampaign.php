<?php
 header('Content-type: text/xml; charset=utf-8'); 
 $startdate = $_GET['startdate']; 
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	$global = $_GET['global'];
	include ('timeParse.php');
		$before = array('(',')');

 $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
 $totalurl = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
 // $totalurl = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
 // $url = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
// echo $totalurl;
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
	
$totalstr = file_get_contents($totalurl);

$totalaxml =new SimpleXMLElement($totalstr);
	
	$namespaces = $totalaxml ->getNamespaces();
	$totalaxml ->registerXPathNamespace('a', $namespaces['']); 
	
	
	$total = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');  
	$totaldiff =  $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="Diff2"]/a:ValueData');  
	
	echo '<campaigns> ';
	

foreach($xml as  $value){
	
$diff = $value->Value->clsvalue[2]->ValueData;
$name = $value->Name;
$fullname = $value->Name;
if(strlen($diff) >= 0){
$diff = $value->Value->clsvalue[2]->ValueData;
}
else {
	$diff = "";
}


if(strlen($name) > 20){
	$name = substr($value->Name,0,18)."...";
}
else {
$name = $value->Name;
}

	echo '<campaign>';
	 echo '<fullname>'.trim($fullname).'</fullname>';
    echo '<name>'.iconv("UTF-8","UTF-8",$name).'</name>';
	echo '<value>('.str_replace($before,"",$value->Value->clsvalue[0]->ValueData).')</value>';
	echo '<diff>'.$diff.'</diff>';
	echo '</campaign>';
}
if(strlen($totaldiff[0]) > 0){
	$totaldiff[0] = $totaldiff[0];
}
if(strlen($total[0]) > 0){
	$total[0] = "(".$total[0].")";
}
	echo '<total>'.$total[0].'</total>';
		echo'<totaldiff>'.$totaldiff[0].'</totaldiff>';
echo '</campaigns>';
	?>


