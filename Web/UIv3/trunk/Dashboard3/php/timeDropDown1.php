<?php header("Content-Type:text/xml");?>
<?php
$startdate = $_GET['startdate'];
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	
	include ('timeParse.php');
libxml_use_internal_errors(true);
$url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID=95&measureID=8&ranges=20101016-20101016,20101015-20101015&diff=True&grouping=campaign&top=10&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures=14&displayMeasures=20,14,19';





$xmlstr = file_get_contents($url);


	$xml = simplexml_load_string($xmlstr);
	
	   if(!$xml){
	header('HTTP/1.1 500 Internal Server Error');
 echo '<xml>';
 echo '<error>There is an error in the web service</error>';
 echo '<address>'.$url.'</address>';
 echo '</xml>';
 
}

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

		
		if(strlen($name) > 6){
			$name = substr($name,0,6)."...";
		}
		else{
		$name = $item->xpath('//a:Name');
		}
	echo '<campaign>';
		echo '<fullname>'.$fullname[$key]. '</fullname>';
		echo '<name>'.$name[$key]. '</name>';
		echo '<cost>'.$cost[$key]. '</cost>';
		echo '<cpa>'.$cpa[$key]. '</cpa>';
		echo '<acq>'.$acq[$key]. '</acq>';
		echo '<roi>'.$roi[$key]. '%</roi>';
	echo '</campaign>';
	}
 echo '</campaigns>';
 


?>
