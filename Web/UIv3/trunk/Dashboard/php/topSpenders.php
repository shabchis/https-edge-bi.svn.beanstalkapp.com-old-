<?php header ("Content-Type:text/xml");
	$startdate = $_GET['startdate'];
	$measureId = $_GET['measure'];
	$endDate = $_GET['endDate'];
	$orderby = $_GET['orderby'];
	$account_id = $_GET['account_id'];
	
	include ('timeParse.php');
	

$url ='http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID=8&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=5&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures='.$measureId.'&displayMeasures=20,'.$measureId.',19';
 $totalurl ='http://qa/ConsoleDataServices/service.svc/Data?accountID='.$account_id.'&measureID=8&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures='.$measureId.'&displayMeasures=20,'.$measureId.',19';
 // $totalurl ='https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID=8&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=account&top=5&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures='.$measureId.'&displayMeasures=20,'.$measureId.',19';
// $url ='https://console.edge-bi.com/Seperia/DataServices/service.svc/Data?accountID='.$account_id.'&measureID=8&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&diff=True&grouping=campaign&top=10&dataSort=value1&dataSortDir=DESC&functionDisplayMeasures='.$measureId.'&displayMeasures=20,'.$measureId.',19';
 

 // echo $url;

   if(!$xml=simplexml_load_file($url)){
    // trigger_error('Error reading XML file', E_USER_ERROR);
	 header('HTTP/1.1 200 OK');

	 header ("Content-Type:text/xml");
  // trigger_error('Error reading XML file',E_USER_ERROR);
 echo '<xml>';
 echo '<error>There is an error in the web service</error>';
  echo '<address>'.$url.'</address>';
   echo '<child>'.strlen($xml->ArrayOfReturnData->children()).'</child>';
 echo '</xml>';
break;
}

$totalstr = file_get_contents($totalurl);

$totalaxml =new SimpleXMLElement($totalstr);
	
	$namespaces = $totalaxml ->getNamespaces();
	$totalaxml ->registerXPathNamespace('a', $namespaces['']); 
	
	
	$total = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');  
	$totalcpa =  $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="DISPLAYMEASURE1"]/a:ValueData');  
	$totalacq = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="DISPLAYMEASURE2"]/a:ValueData');  
	$totalroi = $totalaxml->xpath('//a:ReturnData//a:clsvalue[a:FieldName="DISPLAYMEASURE3"]/a:ValueData');  
 // elseif (strlen($xml->children()<=1){



 // echo '<text>success</text>';
 // break;
// }

 echo '<campaigns>';


foreach($xml as  $value){
		$name = $value->Name;
		$cost = $value->Value->clsvalue[0]->ValueData;
		$cpa = $value->Value->clsvalue[2]->ValueData;
		$acq = $value->Value->clsvalue[3]->ValueData;
		$roi = $value->Value->clsvalue[4]->ValueData;
		
	// $diff = '('.$value->Value->clsvalue[2]->ValueData.'%)';
	// $difftest = $value->Value->clsvalue[2]->ValueData['i:nil'];

	$fullname = $value->Name;
	
		 if (empty($cost)||(!isset($cost))){
			$cost = "";
	 }
	 else{
		$cost = ''.$value->Value->clsvalue[0]->ValueData.'' ;
		 }
		  if (empty($cpa)||(!isset($cpa))){
			$cpa = "";
	 }
	 else{
		$cpa = ''.$value->Value->clsvalue[2]->ValueData.'' ;
		 }
		 if (empty($acq )||(!isset($acq ))){
			$acq  = "";
	 }
	 else{
		$acq  = ''.$value->Value->clsvalue[3]->ValueData.'' ;
		 }
		 
		  if (empty($roi)||(!isset($roi))||strlen($roi)==0){
			$roi  = "";
	 }
	 else{
		$roi  = ''.$value->Value->clsvalue[4]->ValueData.'%' ;
		 }
		if (!isset($diff)||empty($diff)) {
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

		else {
		$name =  substr($value->Name,0,30);
		}


	echo '<campaign>';
	 echo '<fullname>'.trim($fullname).'</fullname>';
    echo '<name>'.$name.'</name>';
	echo '<cost>'.$cost.'</cost>';
	echo '<cpa>'.$cpa.'</cpa>';
	echo '<acq>'.str_replace('$',"",$acq).'</acq>';
		echo '<roi>'.str_replace('$',"",$roi).'</roi>';
	// echo '<url>'.$url.'</url>';
	// echo '<diff>'.str_replace("(%)","",$diff).'</diff>';
	echo '</campaign>';
}

if(strlen($totalroi[0]) > 0){
			$totalroi[0] =$totalroi[0]."%";
		}
	echo '<total>'.$total[0].'</total>';
	echo'<totalcpa>'.$totalcpa[0].'</totalcpa>';
	echo'<totalacq>'.$totalacq[0].'</totalacq>';	
	echo'<totalroi>'.$totalroi[0].'</totalroi>';
	
	
	echo	'</campaigns>';
	


		?>
