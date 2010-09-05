<?php  header ("Content-Type:text/xml");?>
<?php $colors = array('8d8d8d','90B63D','E3EDCB','BBBDBC'); ?>
<?php $startdate = $_GET['startDate'];?>
<?php $endDate = $_GET['endDate'];?>
<?php $measure = $_GET['measure'];?>

<settings>
  <font>Verdana</font>
  <redraw>1</redraw>
  <background>
    <alpha>100</alpha>
    <border_color>FFFFFF</border_color>
    <border_alpha>20</border_alpha>
  </background>
  <plot_area>
    <margins>
      <left>70</left>
      <right>11</right>
    </margins>
  </plot_area>
  <grid>
    <category>
      <color>FFFFFF</color>
      <dashed>1</dashed>
      <dash_length>1</dash_length>
    </category>
    <value>
      <dash_length>1</dash_length>
      <approx_count>7</approx_count>
    </value>
  </grid>
  <axes>
    <category>
      <tick_length>1</tick_length>
      <width>1</width>
      <color>E7E7E7</color>
    </category>
    <value>
      <tick_length>19</tick_length>
      <width>1</width>
      <color>E7E7E7</color>
    </value>
  </axes>
  <values>
    <value>
    <enabled>1</enabled>
      </value>
  </values>
  <balloon>
    <color>8DB53B</color>
    <border_width>1</border_width>
    <border_color>FFFFFF</border_color>
  </balloon>
  <column>
    <width>85</width>
    <balloon_text>{title}: {value} <?php echo $measure?></balloon_text>
    <grow_time>3</grow_time>
    <grow_effect>regular</grow_effect>
  </column>
  <line>
    <width>0</width>
  </line>
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
             echo  '<text><![CDATA[<b>'.$startdate." -   " .$endDate. '</b>]]></text>';
             echo   '<y>18</y>';
             echo   '<text_color>'.$colors[$key].'</text_color>';
             echo  '<text_size>13</text_size>';
             echo '<align>center</align>';
        echo   '</label>';

}
 ?>

  </labels>
</settings>
