<?php  header ("Content-Type:text/xml");?>
<chart>
 <series>
<?php
$data = mktime(0,0,0,date("m"),date("d")-1,date("Y"));
$tomorrow  = date("Ymd", $data);

echo $tomorrow;


$startdate = $_GET['startDate'];

$enddate = $_GET['enddate'];




$xml = simplexml_load_file("books.xml")
    or die("Error: Cannot create object");

	// we want to show just <title> nodes


foreach($xml->xpath('//book') as $data)
	{



	  echo '<value xid="'.$data->id.'">'.$data->author.'</value>';

	}

?>
<startdate><?php echo $startdate; ?></startdate>
<enddate><?php echo $enddate; ?></enddate>
 </series>

 <graphs>
 <?php
 foreach($xml->xpath('//book') as $data)
	{
        echo '<graph gid="'.$data->id.'">';
        echo  ' <value xid="'.$data->id.'">'.$data->title.'</value>';
        echo '</graph>';

   
    }
 ?>
  
 </graphs>
</chart>