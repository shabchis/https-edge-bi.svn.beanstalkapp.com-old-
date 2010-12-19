<?php header('Content-type: text/html; charset=utf-8');

$myServer = "79.125.11.74";
$myUser = "edgedev";
$myPass = "Uzi!2010";
$dboption = $_GET['db'];

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
  
  ?>