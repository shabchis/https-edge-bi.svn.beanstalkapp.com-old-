<?php header("Content-Type:text/xml");
	$startdate = $_GET['startdate'];
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	$global = $_GET['global'];
	include ('timeParse.php');
 $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$measureId.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&UseSortByCalculatField=True&diffType=abs';
$totalurl = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$measureId.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&UseSortByCalculatField=True&diffType=abs';
 // $totalurl = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
 // $url = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';

    if(!$xml=simplexml_load_file($url)){
    // trigger_error('Error reading XML file', E_USER_ERROR);
	header('HTTP/1.1 500 Internal Server Error');
 echo '<xml>';
 echo '<error>There is an error in the web service</error>';
  echo '<address>'.$url.'</address>';
 echo '</xml>';
}
	
$totalstr = file_get_contents($totalurl);

$totalaxml =new SimpleXMLElement($totalstr);
	
	$namespaces = $totalaxml ->getNamespaces();
	$totalaxml ->registerXPathNamespace('a', $namespaces['']); 
	
	
	$total1 = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');  
		$total2 = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="VALUE2"]/a:ValueData');  
	$totaldiff =  $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="Diff2"]/a:ValueData');  
		$totalcombined = $total2[0]."->" .$total1[0];
	
	echo '<campaigns> ';
	

foreach($xml as  $value){
$value1 = $value->Value->clsvalue[0]->ValueData;
$value2 = $value->Value->clsvalue[1]->ValueData;
$combined = $value2."->".$value1;
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

if (strlen($combined)>0){

$combined = '('.$combined.')';
}
else{
$combined = '';
}
	echo '<campaign>';
	 echo '<fullname>'.trim($fullname).'</fullname>';
    echo '<name>'.iconv("UTF-8","UTF-8",$name).'</name>';
	echo '<value>'.str_replace($before,"",$combined).'</value>';
	echo '<diff>'.str_replace($before,"",$diff).'</diff>';
	echo '</campaign>';
}
if(strlen($totaldiff[0]) > 0){
	$totaldiff[0] = $totaldiff[0];
}

if(strlen($total1[0]) ==null ){
	$total1[0]= "0";
}
if(strlen($total2[0]) == null){
	$total2[0]= "0";
}

if(strlen($totalcombined) > 1){
	$totalcombined = '('.$totalcombined.')';
}
else
{
$totalcombined = '';
}
	echo '<total>'.$totalcombined.'</total>';
		echo'<totaldiff>'.str_replace($totalbefore,"",$totaldiff[0]).'</totaldiff>';
echo '</campaigns>';




