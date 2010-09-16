<?php header('content-type: text/xml'); ?>
<?php 
 echo '<?xml version="1.0" encoding="UTF-8" ?>';
echo  '<Measure>';
$xml = simplexml_load_file('http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Measures?AccountID=106')
     or die("Error: Cannot create object");
foreach($xml->xpath('//Measure') as $measure)
	{

	
	echo '<DisplayName>"'.$measure->FieldName.'</DisplayName>';
    echo '<MeasureID>'.$measure->MeasureID.'</MeasureID>';
	}


echo '</Measure>';




?>