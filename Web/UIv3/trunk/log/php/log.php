<?php 

$myServer = "79.125.11.74";
$myUser = "edgedev";
$myPass = "Uzi!2010";
$dboption = $_GET['db'];
$startdate = $_GET['startdate'];
$endtdate = $_GET['enddate'];

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
SELECT distinct [sourceid],[source],[executionid]
           
            ,[starttime]
      ,[endtime]
      ,[event]
           
  FROM ".$db.".[dbo].[sysssislog]
    where cast ( [starttime] as date) between '".$startdate ."'  and '".$endtdate."'
      and [event] in ('PackageStart','PackageEnd','OnWarning','OnError','OnInformation') 
  order by [starttime] desc
";




  


//execute the SQL query and return records
$result = mssql_query($query);

$numRows = mssql_num_rows($result);

//display the results




while($row = mssql_fetch_array($result))
{

// $uniqueResult = array_unique($row);

  echo "<h2 class='trigger ".$row["event"]."' data-event='".$row["event"]."''><a class ='link'>".$row["source"]."</a></h2>";
  

echo  "<div class='toggle_container'>";
echo  "<div class='block'>";

 echo "<ul class='list' data-exec='".$row["executionid"]."' date-sourceid='".$row["sourceid"]."'>";


	 

echo "<li>". $row["event"] ."</li><li> ". $row["starttime"]."</li>";




echo   "</ul>";
 echo "</div>";
 echo "</div>";
}




//close the connection
mssql_close($dbhandle);


?>
