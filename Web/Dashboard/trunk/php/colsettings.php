<?php  header ("Content-Type:text/xml");?>
<?php $colors = array('8d8d8d','90B63D','E3EDCB','BBBDBC'); ?>
<?php $startdate = $_GET['startDate'];?>
<?php $endDate = $_GET['endDate'];?>
<?php $measure = $_GET['measure'];?>
<?php  $startDateName = $_GET['startDateName'];  ?>
<?php  $endDateName = $_GET['endDateName'];  ?>
<?php 

switch ($startdate){
		case 1:
			$startdate  =  "Previous Day";
			case 2:
			$startdate  =  "7 days ago";
			}
	switch ($endDate){
		case 1: 
		$startDateName = $startdate;
		$endDateName = "Yesterday";
		break;
		case 2: 
		$startDateName = "Previous 7 days";
		$endDateName = "Last 7 days";
		break;
		case 3: 
		$startDateName = "Previous 30 days";
		$endDateName = "Last 30 days";
	break;
	case 4: 
		$startDateName = "Last Month";
		$endDateName = "This Month";
	break;
	case 5: 
		$startDateName = "Last Month";
		$endDateName = "Previous Month";
		break;
		case 6: 
		$startDateName = "This Week";
		$endDateName = "Last Week";
		break;
		case 7: 
		$startDateName = "Previous Week";
		$endDateName = "Last Week";
		break;
	}

?>
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
  <error_messages>
    <color>90B63D</color>
    <alpha>100</alpha>
  </error_messages>
  <column>
    <width>85</width>
    <balloon_text>{title}: {value} <?php echo $measure;?></balloon_text>
    <grow_time>3</grow_time>
    <grow_effect>regular</grow_effect>
  </column>
  <line>
    <width>0</width>
  </line>
  <graphs>
<graph gid="0">

</graph>
<graph gid="0">
<axis>right</axis>
<title><?php echo $startDateName ;?></title>
<color>8D8D8D</color>
</graph>
<graph gid="1">
<axis>right</axis>
<title><?php echo $endDateName;?></title>
<color>90B63D</color>
</graph>

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
