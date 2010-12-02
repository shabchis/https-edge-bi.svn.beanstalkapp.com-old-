<?php header("Content-Type:text/xml");?>
<?php
$startdate = $_GET['startdate'];
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	
	include ('timeParse.php');
libxml_use_internal_errors(true);
$url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID=8&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures='.$measureId.'&displayMeasures=20,'.$measureId.',19';
$totalurl = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID=8&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures='.$measureId.'&displayMeasures=20,'.$measureId.',19';
// $url = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID=8&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures='.$measureId.'&displayMeasures=20,'.$measureId.',19';
// $totalurl = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID=8&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures='.$measureId.'&displayMeasures=20,'.$measureId.',19';





$xmlstr = file_get_contents($url);


	$xml = simplexml_load_string($xmlstr);
	
	   if(!$xml){
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
	
	
	$total = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');  
	$totalcpa =  $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="DISPLAYMEASURE1"]/a:ValueData');  
	$totalacq = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="DISPLAYMEASURE2"]/a:ValueData');  
	$totalroi = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="DISPLAYMEASURE3"]/a:ValueData');  
    $namespaces = $xml->getNamespaces(); 
	

	 $xml->registerXPathNamespace('a', $namespaces['']); 

 echo '<campaigns>';


foreach( $xml ->xpath('/a:ArrayOfReturnData//a:ReturnData') as $key => $item)
{
		$item->registerXPathNamespace('a', $namespaces['']);
		$fullname = $item->xpath('//a:Name');
		$name = $item->xpath('//a:Name');
		$cost =  $item->xpath('//a:Value/a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');
		$cpa = $item->xpath('//a:Value//a:clsvalue[a:FieldName="DISPLAYMEASURE1"]//a:ValueData');
		$acq = $item->xpath('//a:Value//a:clsvalue[a:FieldName="DISPLAYMEASURE2"]//a:ValueData');
		$roi = $item->xpath('//a:Value//a:clsvalue[a:FieldName="DISPLAYMEASURE3"]//a:ValueData'); 

		
		if(strlen($name[$key]) > 20){
			$name[$key] = substr($name[$key],0,17)."...";
		}
		if(strlen($name[$key])>0){
		$name[$key] = $name[$key];
		
		}
		if(strlen($roi[$key])>0){
		$roi[$key] = $roi[$key].'%';
		}
		
	echo '<campaign>';
		echo '<fullname>'.$fullname[$key]. '</fullname>';
		echo '<name>'.$name[$key]. '</name>';
		echo '<cost>'.$cost[$key]. '</cost>';
		echo '<cpa>'.$cpa[$key]. '</cpa>';
		echo '<acq>'.$acq[$key]. '</acq>';
		echo '<roi>'.$roi[$key]. '</roi>';
	echo '</campaign>';
	}
	
	if(strlen($totalroi[0]) > 0){
			$totalroi[0] =$totalroi[0]."%";
		}
	echo '<total>'.$total[0].'</total>';
	echo'<totalcpa>'.$totalcpa[0].'</totalcpa>';
	echo'<totalacq>'.$totalacq[0].'</totalacq>';	
	echo'<totalroi>'.$totalroi[0].'</totalroi>';
 echo '</campaigns>';
 


?>
