<?php 

$myServer = "79.125.11.74";
$myUser = "edgedev";
$myPass = "Uzi!2010";
$dboption = $_GET['db'];
$startdate = $_GET['startdate'];
$endtdate = $_GET['enddate'];
$source = $_GET['source'];

switch($dboption){
case 1 : 
	$myDB = "Easynet_Admin";
break;
	case 2:
	$myDB = "Seperia_Admin";
	break;
}
// $myDB = "Seperia_Admin";

//connection to the database
$dbhandle = mssql_connect($myServer, $myUser, $myPass)
  or die("Couldn't connect to SQL Server on $myServer.". mssql_get_last_message());

//select a database to work with
$selected = mssql_select_db($myDB, $dbhandle)
  or die("Couldn't open database $myDB");

  
  
//declare the SQL statement that will query the database
$query =  "SELECT 
		[executionid]
      , [starttime]
      ,[endtime]
      ,[event]
      ,[source]
      ,[message]

           
  FROM ".$myDB.".[dbo].[sysssislog]
    where cast ( [starttime] as date) between  '".$startdate ."'  and '".$endtdate."'
    and [event] in ('PackageStart','PackageEnd','OnWarning','OnError','OnInformation') 
	 and [source]  ='".$source."' 
  order by [starttime] desc
";




  


//execute the SQL query and return records
$result = mssql_query($query);

$numRows = mssql_num_rows($result);

//display the results
// echo $query;

// echo '<li>'.$numRows.'</li>';

while($row = mssql_fetch_assoc($result))
{
// echo "<pre>";
// echo print_r($query);
// echo "</pre>";
// $uniqueResult = array_unique($row);

  echo "<li class='".$row["event"]."' data-exec='".$row["executionid"]."' data-source='".$row["source"]."' data-message='".$row["message"]."' data-time='".date('Y-m-d H:i:s',strtotime($row["starttime"]))."'>" .date('Y-m-d H:i:s',strtotime($row["starttime"]))."&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;".$row["event"]."</li>";
}



mssql_free_result($result);
//close the connection
mssql_close($dbhandle);


?>
