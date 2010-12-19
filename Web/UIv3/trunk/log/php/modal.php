<?php 

$myServer = "79.125.11.74";
$myUser = "edgedev";
$myPass = "Uzi!2010";
$dboption = $_GET['db'];
$startdate = $_GET['startdate'];
$endtdate = $_GET['enddate'];
$executionid = $_GET['executionid'];
$sourceid = $_GET['sourceid'];


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
$query = "
SELECT [executionid]
            ,[sourceid]
            ,[starttime]
      ,[endtime]
      ,[event]
      ,[source]
      ,[message]
  FROM [easynet_admin].[dbo].[sysssislog]
  where cast ( [starttime] as date) between '".$startdate."'  and '".$endtdate."'
  and [event] in ('PackageStart','PackageEnd','OnWarning','OnError','OnInformation') 
      and [executionid] = '".$executionid."' -- this will be the executionID parameter
      and [sourceid]  ='".$sourceid."'           -- this will be the sourceID parameter
  order by [executionid],[sourceid]
";




  


//execute the SQL query and return records
$result = mssql_query($query);

$numRows = mssql_num_rows($result);

//display the results



echo "<ul>";
while($row = mssql_fetch_array($result))
{

// $uniqueResult = array_unique($row);
echo $query;
echo "<li>".$row["message"]."</li>";
}

echo "</ul>";


//close the connection
mssql_close($dbhandle);


?>
