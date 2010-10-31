<?php  header ("Content-Type:text/xml");
ini_set('display_errors', 1);
ini_set('log_errors', 1);
error_reporting(E_ALL);

			$Account_id = $_GET['account_id']; //Account id
			$startdate = $_GET['startdate'];// start dates for the graph
			$endDate = $_GET['endDate'];//end dates for the graph 
			$measureId = $_GET['measure'];//measure ID
			$global = $_GET['global'];
			//regular expression , repace all the "Before" values with the "After" , Respectively
			$before   = array('(',')','$', ',');
			$after   = array('-','""','""', '""');
	
	include ('timeParse.php');
//main url - the url contains the names,main values and the ROI
$url = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$Account_id.'&measureID='.$measureId.'&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&grouping=channel&diff=True&top=10&dataSort=value1&dataSortDir=DESC&displayMeasures=20,19&functionDisplayMeasures='.$measureId.'';
//Secondary url - contains the cpa corresponding each value
$urlcpa = 'http://qa/ConsoleDataServices/service.svc/Data?accountID='.$Account_id.'&measureID=20&ranges='.$endDateName1.'-'.$endDateName2.','.$startDateName1.'-'.$startDateName2.'&grouping=channel&top=10&dataSort=value1&dataSortDir=DESC&functionMeasures='.$measureId.'';



echo '<chart>';
echo  '<series>';
 
// echo $url;
 // echo $urlcpa;
 
 //Geting the content from the url into a variable 
$xmlstr = file_get_contents($url);
$xmlstr2 = file_get_contents($urlcpa);
$cpavar1 = array();
$cpavar2 = array();
$namearr = array();
$ids = array();
$names= array();


	
/* Create a simplexml object, it's allowing the the get access via xpath function  */
	$xml = new SimpleXMLElement($xmlstr);

	$cpaxml =new SimpleXMLElement($xmlstr2);
	
	//error handling 
	   if(!$xml){
		header('HTTP/1.1 500 Internal Server Error');
			 echo '<xml>';
			 echo '<error>There is an error in the web service</error>';
			 echo '<address>'.$url.'</address>';
			 echo '</xml>';
 
}		 
//assign a namespace for each variable
	$namespaces = $cpaxml->getNamespaces(); 
	$namespaces = $xml->getNamespaces();
	$xml->registerXPathNamespace('a', $namespaces['']); 
	$cpaxml->registerXPathNamespace('a', $namespaces['']); 
 

// print_r($namevar[$key]);
//Getting the ID from the main xml 
foreach ($xml->xpath('//a:ReturnData') as $key => $item){
	$id = $item->ID;
	

foreach($cpaxml->xpath('//a:ReturnData') as $key => $cpa){
		$cpa->registerXPathNamespace('a', $namespaces['']);
		$id = $cpa->ID;
		$cpaname = $cpa->xpath('//a:Name');
		 $cpavalue1 = $cpa->xpath('//a:Value/a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');  
		 $cpavalue2 = $cpa->xpath('//a:Value/a:clsvalue[a:FieldName="VALUE2"]/a:ValueData');  
		$namearr[$key] = $cpaname;

		
}

}
//Getting the name of the campaigns by the ID
  foreach ($xml->xpath('//a:ReturnData') as $key => $cpa){
	$cpa->registerXPathNamespace('a', $namespaces['']);
			$id = $cpa->ID;
			
			$ids[$key] = $id;
		
	foreach($xml->xpath('//a:ReturnData[a:ID="'.$id.'"]') as $item){
/* Register namespace for the main value varibales */
		$item->registerXPathNamespace('a', $namespaces['']);
/* Get the names from the main xml  */
		$name = $item->Name;
		
//
    echo '<value xid="'.$id.'"> '.str_replace($before,"",$name).'</value>' ;

}
}

  echo '</series>';
  echo  '<graphs>';

    echo '<graph gid="0">';
// print_r($ids);

//Getting the cpa from the secondary xml and the values from the main xml by the ID of the secondary xml 

/*  Looping through the cpa xml , grabbing the ID ,and the CPA ,*/
	foreach ($cpaxml->xpath('//a:ReturnData') as $key => $cpa){
		$id = $cpa->ID;
		$cpa->registerXPathNamespace('a', $namespaces['']);
		$cpavalue1 = $cpa->xpath('//a:Value//a:clsvalue[a:FieldName="VALUE1"]/a:ValueData');  
	/*  Get the main value of the channel by the ID taken from the cpa xml*/
		  $value1 = $xml->xpath('//a:ReturnData[a:ID="'.$id.'"]//a:clsvalue[a:FieldName="VALUE1"]//a:ValueData');
			/* Register namespace for the main value varibales */
			$item->registerXPathNamespace('a', $namespaces['']);
			/* Get the ROI */
			$roi1 = $xml->xpath('//a:ReturnData[a:ID="'.$id.'"]//a:clsvalue[a:FieldName="DISPLAYMEASURE2"]//a:ValueData');
		
		/* Error handling  */
		
		//If the ROI is not empty add percent sign to it
		if(strlen($roi1[0])>0){
		$roi1[0] = $roi1[0].'%';
		}
		echo '<value xid="'.$id. '" percent="'.$roi1[0].'" description="'. $cpavalue1[$key].' /' .$roi1[0].'">'.str_replace($before,"",$value1[0]).'</value>' ;

}
     
	
   echo '</graph>';
		 

	

   echo '<graph gid="1">';
//Getting the cpa from the secondary xml and the values from the main xml by the ID of the secondary xml 

/*  Looping through the cpa xml , grabbing the ID ,and the CPA ,*/
   foreach ($cpaxml->xpath('//a:ReturnData') as $key => $cpa){
   $cpa->registerXPathNamespace('a', $namespaces['']);
			$id = $cpa->ID;
			$cpavalue2 = $cpa->xpath('//a:Value//a:clsvalue[a:FieldName="VALUE2"]//a:ValueData');
				/*  Get the main value of the channel by the ID taken from the cpa xml*/
			$value2 = $xml->xpath('//a:ReturnData[a:ID="'.$id.'"]//a:clsvalue[a:FieldName="VALUE2"]//a:ValueData');
			/* Get the ROI */
			$roi2 = $xml->xpath('//a:ReturnData[a:ID="'.$id.'"]//a:clsvalue[a:FieldName="DISPLAYMEASURE2"]//a:ValueData');
			
				if(strlen($roi2[0])>0){
				$roi2[0] = $roi2[0].'%';
		}
		echo '<value xid="'.$id.'" percent="'.$roi2[0].'" description="'.$cpavalue2[$key].' / '.$roi2[0].'"> '.str_replace($before,"", $value2[0] ).'</value>' ;
	


}
 

		
echo    '</graph>';
	
 echo    ' </graphs>';
  
echo    '</chart>';
	  ?>