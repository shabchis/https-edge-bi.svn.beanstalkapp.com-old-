<?php header("Content-Type:text/xml");?>
<?php 
// 1. initialize
$ch = curl_init();

// 2. set the options, including the url
curl_setopt($ch, CURLOPT_URL, "http://qa/ConsoleDataServices/service.svc/Data?accountID=95&measureID=8&ranges=20101012-20101012,20101011-20101011&diff=True&grouping=campaign&top=10&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures=14&displayMeasures=20,14,19");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_HEADER, 0);

// 3. execute and fetch the resulting HTML output
$output = curl_exec($ch);
$output2 = preg_replace("/<.*(xmlns *= *[\"'].[^\"']*[\"']).[^>]*>/i", "", $output); 
// 4. free up the curl handle
curl_close($ch);
	$sitemap = simplexml_load_string($output2);
echo '<campaigns>';

foreach( $sitemap->xpath('/a:ArrayOfReturnData//a:ReturnData') as $key => $item)
{

		$name = $item->Name;
		$cost =  $item->xpath('//a:Value/a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');
		$cpa = $item->xpath('//a:Value//a:clsvalue[FieldName="DISPLAYMEASURE1"]//a:ValueData');
		$acq = $item->xpath('//a:Value//xmlns:clsvalue[FieldName="DISPLAYMEASURE2"]//a:ValueData');
		$roi = $item->xpath('//a:Value//xmlns:clsvalue[FieldName="DISPLAYMEASURE3"]//a:ValueData'); 

	echo '<campaign>';
		echo '<name>'.$name. '</name>';
	
		echo '<cost>'.$cost. '</cost>';
		echo '<cpa>'.$cpa[$key]. '</cpa>';
		echo '<acq>'.$acq[$key]. '</acq>';
		echo '<roi>'.$roi[$key]. '</roi>';
	echo '</campaign>';
	}
 echo '</campaigns>';
 
 ?>