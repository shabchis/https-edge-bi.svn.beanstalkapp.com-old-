<?php  header ("Content-Type:text/xml");?>
<?php $colors = array('8d8d8d','90B63D','E3EDCB','BBBDBC'); ?>
<?php $startdate = $_GET['startDate'];?>
<?php $endDate = $_GET['endDate'];?>

<settings>
  <colors>8d8d8d,90B63D,E3EDCB</colors>
  <background>
    <alpha>100</alpha>
    <border_alpha>20</border_alpha>
  </background>
  <grid>
    <category>
      <dashed>1</dashed>

    </category>
    <value>
      <dashed>1</dashed>
    </value>
  </grid>
  <axes>
   <y_left>
<alpha>15</alpha>
</y_left>
<y_right>
<alpha>10</alpha>
<dashed>true</dashed>
</y_right>
  </axes>
  <values>
    <value>
      <min>0</min>
    </value>
  </values>
  <depth>0</depth>
  <column>
    <width>53</width>
    <balloon_text>{title}: {value} CPA</balloon_text>
    <grow_time>3</grow_time>
    <grow_effect>regular</grow_effect>
      <spacing>0</spacing>
  </column>
  <graphs>


<?php
$xml = simplexml_load_file("books.xml")
    or die("Error: Cannot create object");

	// we want to show just <title> nodes

foreach($xml->xpath('//book') as $key=>$value)
	{



	        echo '<graph gid="'.$value->id.'">';
	        echo '<axis>right</axis>';
            echo  '<title>'.$value->author.'</title>';
            echo   '<color>'.$colors[$key].'</color>';
            echo '</graph>';

	}


?>
 </graphs>

<labels>
<?php
foreach($xml->xpath('//book') as $key=>$value)
	{
        echo '<label lid="'.$value->id.'">';
             echo  '<text><![CDATA[<b>'.$startdate." -  " .$endDate. '</b>]]></text>';
             echo   '<y>18</y>';
             echo   '<text_color>'.$colors[$key].'</text_color>';
             echo  '<text_size>13</text_size>';
             echo '<align>center</align>';
        echo   '</label>';

}
 ?>

  </labels>
</settings>
