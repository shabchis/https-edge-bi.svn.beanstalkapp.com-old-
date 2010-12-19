<?php header("Content-Type:text/xml");?>
<?php
// $url = 'options.xml';
$url ='http://qa/ConsoleDataServices/service.svc/Data?accountID=95&measureID=8&ranges=20101012-20101012,20101011-20101011&diff=True&grouping=campaign&top=10&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures=14&displayMeasures=20,14,19';

// $xml = new SimpleXMLElement($url);
 $xml = simplexml_load_file($url) ;
// $xml = file_get_contents($url);
// var_dump($xml);

 $returnValue = $xml->xpath('//ArrayOfReturnData/ReturnData');

 echo '<campaigns>';

foreach( $returnValue  as $key=>$item)
{
		$name = $item ->xpath('//Name');
		$cost =  $item->xpath('//Value/clsvalue[FieldName="VALUE1"]/ValueData');
		$cpa = $item->xpath('//Value//clsvalue[FieldName="DISPLAYMEASURE1"]/ValueData');
		$acq = $item->xpath('//Value//clsvalue[FieldName="DISPLAYMEASURE2"]/ValueData');
		$roi = $item->xpath('//Value//clsvalue[FieldName="DISPLAYMEASURE3"]/ValueData'); 
	
	echo '<campaign>';
		echo '<name>'.$name[$key]. '</name>';
		echo '<cost>'.$cost[$key]. '</cost>';
		echo '<cpa>'.$cpa[$key]. '</cpa>';
		echo '<acq>'.$acq[$key]. '</acq>';
		echo '<roi>'.$roi[$key]. '</roi>';
	
	echo '</campaign>';
	}
 echo '</campaigns>';
 
 

?>
