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
	
?>

<?php

 $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
 $totalurl = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
// $url = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';
 // $totalurl = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=Diff2&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'&UseSortByCalculatField=True&diffType=abs';

 // echo $url;
 
 //store the url content into a vaiable
    $xmlstr = file_get_contents($url);

//load the variable using simple xml
	$xml = simplexml_load_string($xmlstr);
	//error if xml not right 
	   if(!$xml){
	header('HTTP/1.1 500 Internal Server Error');
 echo '<xml>';
 echo '<error>There is an error in the web service</error>';
 echo '<address>'.$url.'</address>';
 echo '</xml>';
 
}

//register namespace 
$totalstr = file_get_contents($totalurl);

$totalaxml =new SimpleXMLElement($totalstr);
	
	$namespaces = $totalaxml ->getNamespaces();
	$totalaxml ->registerXPathNamespace('a', $namespaces['']); 
	
	
	$total = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');  
	$totaldiff =  $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="Diff2"]/a:ValueData');  
	
   $namespaces = $xml->getNamespaces(); 
	

 $xml->registerXPathNamespace('a', $namespaces['']); 
	 
	

 echo '<campaigns>';

//loop trough the returndata array 
foreach( $xml ->xpath('/a:ArrayOfReturnData//a:ReturnData') as $key => $item)
{	
		$item->registerXPathNamespace('a', $namespaces['']);
		$fullname = $item->xpath('//a:Name');
		$name = $item->xpath('//a:Name');
		$val =  $item->xpath('//a:Value//a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');
		$diff = $item->xpath('//a:Value//a:clsvalue[a:FieldName="Diff2"]//a:ValueData');
			
		if(strlen($name[$key]) >= 20){
			$name[$key] = substr($name[$key],0,17).'...';
		}
		if (strlen($diff[$key])>0){
			$diff[$key] = $diff[$key];
		}
		if (strlen($val[$key])>0){
			$val[$key] = '('.$val[$key].')';
		}
		
	echo '<campaign>';
		echo '<fullname>'.$fullname[$key]. '</fullname>';
		echo '<name>'.iconv("UTF-8","UTF-8",$name[$key]). '</name>';
		echo '<value>'.$val[$key]. '</value>';
		echo '<diff>'.$diff[$key]. '</diff>';
		
	echo '</campaign>';
	}
	
	if(strlen($totaldiff[0]) > 0){
	$totaldiff[0] = "(".$totaldiff[0].")";
}
		echo '<total>'.$total[0].'</total>';
		echo'<totaldiff>'.$totaldiff[0].'</totaldiff>';
 echo '</campaigns>';

	?>

