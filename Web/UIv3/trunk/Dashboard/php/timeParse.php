<?php

switch ($startdate){
		case 1:
			$yesterdayStart  =  date("Ymd", mktime(0,0,0,date("m"),date("d")-2,date("Y")));
		break;
		case 2:
			$yesterdayStart  =  date("Ymd", mktime(0,0,0,date("m"),date("d")-8,date("Y")));
			break;
			}
	switch ($endDate){
	case 1: 
		$startDateName1 = $yesterdayStart;
		 $startDateName2 = $yesterdayStart;
		 $endDateName1 =    date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		 $endDateName2 =    date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 2: 
		$startDateName1 = date("Ymd", mktime(0,0,0,date("m"),date("d")-14,date("Y")));
		$startDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-8,date("Y")));
		$endDateName1 = date("Ymd", mktime(0,0,0,date("m"),date("d")-7,date("Y")));
		$endDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 3: 
	
		$startDateName1 =(date("Ymd", mktime(0,0,0,date("m")-2,date("d"),date("Y"))));
		$startDateName2 = date("Ymd", mktime(0,0,0,date("m")-1,date("d")-1,date("Y")));
		$endDateName1 = date("Ymd", mktime(0,0,0,date("m")-1,date("d"),date("Y")));
		$endDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		break;
	case 4: 

if(strtotime('today') ==date("Ymd",mktime(0,0,0,date("m"),date("d")-date("d")+1,date("Y"))) ){
		$startDateName1 =date("Ymd",mktime(0,0,0,date("m")-1,date("d")-date("d")+1,date("Y")));
		$startDateName2 = date("Ymd",mktime(0,0,0,date("m"),date("d")-date("d"),date("Y")));
		$endDateName1 = date("Ymd", mktime(0,0,0,date("m"),date("d")-date("d")+1,date("Y")));
		$endDateName2 =date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
		}
		
		else{
		$startDateName1 =date("Ymd",mktime(0,0,0,date("m")-2,date("d")-date("d")+1,date("Y")));
		$startDateName2 = date("Ymd",mktime(0,0,0,date("m")-1,date("d")-date("d"),date("Y")));
		$endDateName1 = date("Ymd",mktime(0,0,0,date("m")-1,date("d")-date("d")+1,date("Y")));
		$endDateName2 =date("Ymd",mktime(0,0,0,date("m"),date("d")-date("d"),date("Y")));
		
		}
		break;
	case 5: 
		$startDateName1 =date("Ymd",mktime(0,0,0,date("m")-2,date("d")-date("d")+1,date("Y")));
		$startDateName2 = date("Ymd",mktime(0,0,0,date("m")-1,date("d")-date("d"),date("Y")));
		$endDateName1 = date("Ymd",mktime(0,0,0,date("m")-1,date("d")-date("d")+1,date("Y")));
		$endDateName2 =date("Ymd",mktime(0,0,0,date("m"),date("d")-date("d"),date("Y")));
		break;
	case 6: 
	
	 
	  if(strtotime('today') == strtotime('monday'))
	  {
	    $startDateName1 =date("Ymd",strtotime('-2 weeks'));
		 $startDateName2 = date("Ymd",strtotime('-1 week last sunday'));
	  $endDateName1	= date("Ymd",strtotime('-7 days'));
	   $endDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
	  }
	  else
	  {
	    $startDateName1 =date("Ymd",strtotime('-1 week last monday'));
		 $startDateName2 = date("Ymd",strtotime('last sunday'));
		$endDateName1 = date("Ymd",strtotime('last monday'));
		$endDateName2 = date("Ymd", mktime(0,0,0,date("m"),date("d")-1,date("Y")));
	  }
	 
  break;
	 case 7: 
	  if (strtotime('today') == strtotime('monday')){
		$startDateName1 = date("Ymd",strtotime('-14 days'));
		$endDateName1 = date("Ymd",strtotime('-7 days'));
 }
 else
 {
		$startDateName1 = date("Ymd",strtotime('-14 days last monday'));
		
		$endDateName1 = date("Ymd",strtotime('-2 week monday'));
	
	}
		$startDateName2 = date("Ymd",strtotime('-7 days last sunday'));
		$endDateName2 = date("Ymd",strtotime('last sunday'));
	  break;
	}