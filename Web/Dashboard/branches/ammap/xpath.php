<?php  header ("Content-Type:text/xml");?>

<map map_file="maps/world.swf" tl_long="-168.49" tl_lat="83.63" br_long="190.3" br_lat="-55.58" zoom_x="0%" zoom_y="0%" zoom="100%">
  <areas>
   <area title="LATVIA" mc_name="LV" value="2366515"></area>
      <area title="LIECHTENSTEIN" mc_name="LI" value="32842"></area>
      <area title="LITHUANIA" mc_name="LT" value="3601138"></area>
      <area title="LUXEMBOURG" mc_name="LU" value="448569"></area>
      <area title="MACEDONIA" mc_name="MK" value="2054800"></area>
      <area title="MALTA" mc_name="MT" value="397499"></area>
      <area title="MOLDOVA" mc_name="MD" value="4434547"></area>
      <area title="MONTENEGRO" mc_name="ME" value="500000"></area>
      <area title="NETHERLANDS" mc_name="NL" value="16491461"></area>
      <area title="NORWAY" mc_name="NO" value="4525116"></area>
      <area title="POLAND" mc_name="PL" value="38625478"></area>
      <area title="PORTUGAL" mc_name="PT" value="10084245"></area>
      <area title="ROMANIA" mc_name="RO" value="22303552"></area>
      <area title="SERBIA" mc_name="RS" value="9780000"></area>
      <area title="SLOVAKIA" mc_name="SK" value="5422366"></area>
      <area title="SLOVENIA" mc_name="SI" value="1932917"></area>
      <area title="SPAIN" mc_name="ES" value="40077100"></area>
      <area title="RUSSIA" mc_name="RU" value="40077200"></area>
      <area title="SVALBARD AND JAN MAYEN" mc_name="SJ" value="2868"></area>
      <area title="SWEDEN" mc_name="SE" value="8876744"></area>
      <area title="SWITZERLAND" mc_name="CH" value="7301994"></area>
      <area title="UKRAINE" mc_name="UA" value="48396470"></area>
      <area title="UNITED KINGDOM" mc_name="GB" value="59778002"></area>
      <area title="JAPAN" mc_name="JP"  value="10084245"></area>
      <area title="JORDAN" mc_name="JO"  value="10084245"></area>
      <area title="KAZAKHSTAN" mc_name="KZ"  value="10084245"></area>
      <area title="KENYA" mc_name="KE"  value="10084245"></area>
      <area title="NAURU" mc_name="NR" value="10084245"></area>
      <area title="NORTH KOREA" mc_name="KP" value="10084245"></area>
      <area title="SOUTH KOREA" mc_name="KR" value="10084245"></area>
      <area title="KOSOVO" mc_name="KV" value="10084245"></area>
      <area title="KUWAIT" mc_name="KW" value="10084245"></area>
      <area title="KYRGYZSTAN" mc_name="KG" value="10084245"></area>
      <area title="LAO PEOPLE'S DEMOCRATIC REPUBLIC" mc_name="LA"></area>
      <area title="LATVIA" mc_name="LV" value="353545"></area>
      <area title="LEBANON" mc_name="LB" value="64654"></area>
      <area title="LESOTHO" mc_name="LS" value="343435"></area>
      <area title="LIBERIA" mc_name="LR" value="435423"></area>
      <area title="LIBYA" mc_name="LY" value="23542354"></area>
      <area title="LIECHTENSTEIN" mc_name="LI" value="32523"></area>
      <area title="LITHUANIA" mc_name="LT" value="3535345"></area>
      <area title="LUXEMBOURG" mc_name="LU" value="3523535"></area>
      <area title="MACEDONIA" mc_name="MK"></area>
      <area title="MADAGASCAR" mc_name="MG"></area>
      <area title="MALAWI" mc_name="MW"></area>
      <area title="MALAYSIA" mc_name="MY"></area>
      <area title="MALI" mc_name="ML"></area>
      <area title="MALTA" mc_name="MT"></area>
      <area title="MARTINIQUE" mc_name="MQ"></area>
      <area title="MAURITANIA" mc_name="MR"></area>
      <area title="MAURITIUS" mc_name="MU"></area>
      <area title="MEXICO" mc_name="MX"></area>
      <area title="MOLDOVA" mc_name="MD"></area>
      <area title="MONGOLIA" mc_name="MN"></area>
      <area title="MONTENEGRO" mc_name="ME"></area>
      <area title="MONTSERRAT" mc_name="MS"></area>
      <area title="MOROCCO" mc_name="MA"></area>
      <area title="MOZAMBIQUE" mc_name="MZ"></area>
      <area title="MYANMAR" mc_name="MM"></area>
      <area title="NAMIBIA" mc_name="NA"></area>
      <area title="NAURU" mc_name="NR"></area>
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
