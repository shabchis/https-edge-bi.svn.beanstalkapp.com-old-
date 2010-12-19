<?php

function getrows(){
//declare the SQL statement that will query the database
$query = "
SELECT distinct sourceid,[source],[executionid]
           
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

  echo "<h2 class='trigger ".$row["event"]."' data-event='".$row["event"]."' date-sourceid='".$row["sourceid"]."'><a class ='link'>".$row["source"]."</a></h2>";
  

echo  "<div class='toggle_container'>";
echo  "<div class='block'>";

 echo "<ul class='list'>";


	 

echo "<li data-exec='".$row["executionid"]."'>". $row["event"] ."</li><li> ". $row["starttime"]."</li>";




echo   "</ul>";
 echo "</div>";
 echo "</div>";
}
}

?>