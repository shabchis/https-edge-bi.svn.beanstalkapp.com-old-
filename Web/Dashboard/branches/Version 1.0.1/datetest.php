

<?php
$startdata = mktime(0,0,0,date("m"),date("d")-1,date("Y"));
$tomorrow  = date("Ymd", $startdata);

echo $tomorrow;
?> 


