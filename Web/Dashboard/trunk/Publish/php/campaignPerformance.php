<?php header ("Content-Type:text/xml");
	$startdate = $_GET['startdate'];
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	$global = $_GET['global'];
	include ('timeParse.php');



 // $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=value1&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'';
 $url = 'https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=value1&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'';

 
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


	else{
 echo '<campaigns>';


foreach($xml as  $value){
	
	$diff = '('.$value->Value->clsvalue[2]->ValueData.'%)';
	$difftest = $value->Value->clsvalue[2]->ValueData['i:nil'];
	$name = $value->Name;
	$fullname = $value->Name;
	$val = $value->Value->clsvalue[0]->ValueData;
		if (empty($val)||(!isset($val))){
			$val = "";
		}
		else{
		$val = ''.$value->Value->clsvalue[0]->ValueData.'' ;
		}
		if (!isset($diff)||empty($diff)||$diff === null||$diff === $difftest) {
			$diff = "";
		}
		else{
		$diff = '('.$value->Value->clsvalue[2]->ValueData.'%)' ;
		}
		if (empty($name)){
		$name = "";
		}
		if(strlen($name) > 20){
			$name = substr($value->Name,0,18)."...";
		}

		// else {
		// $name =  substr($value->Name,0,30);
		// }


	echo '<campaign>';
	// echo '<address>'.$url.'</address>';
	 echo '<fullname>'.trim($fullname).'</fullname>';
    echo '<name>'.$name.'</name>';
	echo '<value>'.$val.'</value>';
	echo '<diff>'.str_replace("(%)","",$diff).'</diff>';
	echo '</campaign>';
}


	
	echo	'</campaigns>';
	
}

		?>
