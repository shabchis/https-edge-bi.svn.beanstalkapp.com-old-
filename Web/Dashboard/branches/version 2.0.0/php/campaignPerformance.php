<?php header ("Content-Type:text/xml");
$startdate = $_GET['startdate'];
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	$global = $_GET['global'];
	include ('timeParse.php');

 $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=value1&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'';
 $totalurl = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=value1&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'';


 // echo $totalurl;

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
	$totaldiff =  $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="Diff2"]/a:ValueData');  


   $namespaces = $xml->getNamespaces(); 
	

	 $xml->registerXPathNamespace('a', $namespaces['']); 

 echo '<campaigns>';


	foreach( $xml ->xpath('/a:ArrayOfReturnData//a:ReturnData') as $key => $item)
	{
		$item->registerXPathNamespace('a', $namespaces['']);
		$fullname = $item->xpath('//a:Name');
		$name = $item->xpath('//a:Name');
		$val =  $item->xpath('//a:Value//a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');
		$diff = $item->xpath('//a:Value//a:clsvalue[a:FieldName="Diff2"]//a:ValueData');
			
		if(strlen($name[$key]) > 20){
			$name[$key] = substr($name[$key],0,17)."...";
		}
		
		if(strlen($diff[$key]) > 0){
			$diff[$key] = '('.$diff[$key].'%)';
		}
		if(strlen($name[$key]) > 0){
			$name[$key] = $name[$key];
		}
		if(strlen($val[$key]) > 0){
			$val[$key] = $val[$key];
		}
		
	echo '<campaign>';
		echo '<fullname>'.$fullname[$key]. '</fullname>';
		echo '<name>'.$name[$key]. '</name>';
		echo '<value>'.$val[$key]. '</value>';
		echo '<diff>'.$diff[$key]. '</diff>';
		
	echo '</campaign>';
	}
	if(strlen($totaldiff[0]) > 0){
			$totaldiff[0] = '('.$totaldiff[0]."%)";
		}
	echo '<total>'.$total[0].'</total>';
	echo'<totaldiff>'.$totaldiff[0].'</totaldiff>';

	
 echo '</campaigns>';

		?>
