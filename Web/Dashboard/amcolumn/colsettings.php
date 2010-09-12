<?php  header ("Content-Type:text/xml");?>
<?php $colors = array('8d8d8d','90B63D','E3EDCB','BBBDBC'); ?>
<?php $startdate = $_GET['startDate'];?>
<?php $endDate = $_GET['endDate'];?>
<?php $measure = $_GET['measure'];?>
<?php $startDateName = $_GET['startDateName']; ?>
<?php $endDateName = $_GET['endDateName']; ?>

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
      <left>50</left>
      <right>0</right>
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
      <tick_length>13</tick_length>
      <width>1</width>
      <color>E7E7E7</color>
    </category>
    <value>
      <width>1</width>
      <color>E7E7E7</color>
    </value>
  </axes>
   <values>
    <category>
      <enabled>1</enabled>
    </category>
    <value>
      <min>0</min>
    </value>
  </values>
  <balloon>
   
	<corner_radius>5</corner_radius>
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



	        echo '<graph gid="'.$key.'">';
	        echo '<axis>right</axis>';
            echo  '<title>'.$value->date.'</title>';
            echo   '<color>'.$colors[$key].'</color>';
            echo '</graph>';

	}


?>
 </graphs>

<labels>



       <label lid="0">
            <text><![CDATA[<b></b>]]></text>
           <y>18</y>
            <text_color></text_color>
            <text_size>13</text_size>
           <align>center</align>
        </label>




  </labels>
</settings>
