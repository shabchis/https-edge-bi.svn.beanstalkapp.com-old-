<?php  header ("Content-Type:text/xml");?>
<?php $colors = array('8d8d8d','90B63D','E3EDCB','BBBDBC'); ?>
<?php $startdateName = $_GET['startDate'];?>
<?php $endDate = $_GET['endDate'];?>
<?php $measure = $_GET['measure'];?>

<?php 
switch ($startdateName){
			case 1:
			$startdate  =  "Previous Day";
			break;
			case 2:
			$startdate  =  "7 days ago";
			break;
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
		$startDateName = "Previous Month";
		$endDateName = "Last Month";
		break;
		case 6: 
		$startDateName = "Last Week";
		$endDateName = "This Week";
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
  <decimals_separator>.</decimals_separator>
  <thousands_separator>,</thousands_separator>
  <add_time_stamp>true</add_time_stamp>
  <digits_after_decimal>1</digits_after_decimal>
  <plot_area>
    <margins>
      <left>60</left>
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
		
      <strict_min_max>0</strict_min_max>
      <skip_first>0</skip_first>
      <width>1</width>
	      <color>E7E7E7</color>
    </value>
  </axes>
   <values>
  <value>
      <min>0</min>
      <strict_min_max>1</strict_min_max>
      <skip_first>0</skip_first>
    </value>
 
    <category>
	 <letter number="1000">K</letter>
      <enabled>1</enabled>
    </category>
	
	
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
    <width>50</width>
    <balloon_text>
	<![CDATA[{title}: <?php echo $measure;?> = {value}, CPA = {description}]]>
	
	</balloon_text>
    <grow_time>3</grow_time>
    <grow_effect>regular</grow_effect>
  </column>
  <line>
    <width>0</width>
  </line>
  <graphs>

<graph gid="0">
<axis>right</axis>
<title><?php echo $endDateName ;?></title>
<color>8D8D8D</color>
</graph>
<graph gid="1">
<axis>right</axis>
<title><?php echo $startDateName;?></title>

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
