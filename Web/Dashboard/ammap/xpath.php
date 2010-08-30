<?php  header ("Content-Type:text/xml");?>

<map map_file="maps/world.swf" tl_long="-168.49" tl_lat="83.63" br_long="190.3" br_lat="-55.58" zoom_x="0%" zoom_y="0%" zoom="100%">
  <areas>
<?php
  
 	$xml = simplexml_load_file("books.xml") 
    or die("Error: Cannot create object");

	// we want to show just <title> nodes
	foreach($xml->xpath('//book') as $data)
	{
	


 echo '<area title="'. $data->title['title'] .'" mc_name="'. $data->title['mc_name'] .'" value="'. $data->title['value'] .'"></area>';
	  
	}



?>
  </areas>
</map>
