

 <iframe src ="https://console.edge-bi.com/seperia/web/dashboard/dashboard.php?account_id=95"/>

<?php
$cookie = array(
'name' => 'dashboard',
'value' => 'yes',
'expire' => '86500',
'domain' => 'localhost',
'prefix' => 'manu_'

);

set_cookie($cookie);
?>
<script type="text/javascript">
/*
$(function(){

    $("#ajaxloader").fadeIn();
 $('#main').load("external/dashboard2/dashboard.php?account_id=7");
$("#ajaxloader").fadeOut();
 });
 
 
 </script>