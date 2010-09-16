<?php

$xml = simplexml_load_file("options.xml")
     or die("Error: Cannot create object");
foreach($xml->xpath('//time') as $date)
	{



	  echo '<option value="'.$date['value'].'">'.$date. '</option>';

	}
?>