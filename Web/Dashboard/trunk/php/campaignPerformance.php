<?php header ("Content-Type:text/xml");
	$startdate = $_GET['startdate'];
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	$global = $_GET['global'];
	include ('timeParse.php');


 // $url = 'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID='.$account_id.'&MeasureID='.$measureId.'&DateRanges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&Diff=True&Group_By=campaign&Top=10&Data_Sort=value1&View_Sort=Diff2&Data_Sort_Dir='.$orderby.'';
 $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$global.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=value1&dataSortDir='.$orderby.'&functionMeasures='.$measureId.'';

 // $url = 'http://qa/ConsoleDataServices/service.svc/Data?AccountID='.$account_id.'&MeasureID='.$measureId.'&DateRanges='.$startDateName1.'-'.$startDateName2.','.$endDateName1.'-'.$endDateName2.'&Diff=True&Group_By=campaign&Top=10&Data_Sort=VALUE1&Data_Sort_Dir='.$orderby.'';
// $url = 'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Data?AccountID='.$account_id.'&Calc_Measures='.$measureId.'&MeasureID=17&DateRanges='.$startDateName1.'-'.$startDateName2.','.$endDateName1.'-'.$endDateName2.'&Diff=True&Group_By=campaign&Top=10&Data_Sort=VALUE1&Data_Sort_Dir='.$orderby.'&View_Sort=Diff2';
// $url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID='.$measureId.'&ranges='.$startDateName1.'-'.$startDateName2.','.$endDateName1.'-'.$endDateName2.'&grouping=campaign&diff=True&top=10&dataSort=VALUE1&viewSort=VALUE1&dataSortDir='.$orderby.'';

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
