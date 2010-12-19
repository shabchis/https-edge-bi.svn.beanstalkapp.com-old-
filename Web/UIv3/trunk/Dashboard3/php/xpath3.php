<?php header("Content-Type:text/xml");?>
<?php
$before = array('http://schemas.datacontract.org/2004/07/Dwh.Data.Service','xmlns=','=','xmlns:i");
$xmlstr = file_get_contents($source);

// $sitemap = new SimpleXMLElement($xmlstr);
// $namespaces = $sitemap->getDocNamespaces();
// foreach($namespaces as $key => $value) {
  // echo "{$key} => {$value}
// ";
// }
$sitemap = new SimpleXMLElement($source,null,true);
 $sitemap->registerXPathNamespace('a', 'http://schemas.datacontract.org/2004/07/Dwh.Data.Service'); 
 // $sitemap->registerXPathNamespace('i', 'http://www.w3.org/2001/XMLSchema-instance');



 echo '<campaigns>';

foreach( $sitemap ->xpath('//a:ArrayOfReturnData/a:ReturnData') as $key => $item)
{
// var_dump($item);
		$name = $item ->xpath('//a:ReturnData//a:Name');
		$cost =  $item->xpath('//xmlns:Value/clsvalue[FieldName="VALUE1"]/ValueData');
		$cpa = $item->xpath('//xmlns:Value//xmlns:clsvalue[FieldName="DISPLAYMEASURE1"]//xmlns:ValueData');
		$acq = $item->xpath('//xmlns:Value//xmlns:clsvalue[FieldName="DISPLAYMEASURE2"]//xmlns:ValueData');
		$roi = $item->xpath('//xmlns:Value//xmlns:clsvalue[FieldName="DISPLAYMEASURE3"]//xmlns:ValueData'); 

	echo '<campaign>';
		echo '<name>'.$name. '</name>';
	
		echo '<cost>'.$cost[$key]. '</cost>';
		echo '<cpa>'.$cpa[$key]. '</cpa>';
		echo '<acq>'.$acq[$key]. '</acq>';
		echo '<roi>'.$roi[$key]. '</roi>';
	echo '</campaign>';
	}
 echo '</campaigns>';
 



?>
