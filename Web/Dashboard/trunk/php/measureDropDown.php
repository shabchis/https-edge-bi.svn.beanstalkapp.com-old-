
<?php  $account_id = $_GET['account_id'];?>

<?php

// $url = 'http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Measures?AccountID='.$account_id.'';
 $url = 'http://qa/ConsoleDataServices/service.svc/Measures?AccountID='.$_GET['account_id'].'';
 
 // echo $url;
if(!$xml=simplexml_load_file($url)){
    // trigger_error('Error reading XML file',E_USER_ERROR);
}

foreach($xml as $measure){
    echo '<option value="'.$measure->MeasureID.'"> '.$measure->DisplayName.'</option>' ;
}
/*
$data = file_get_contents("http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Measures?AccountID=106",0);
   		echo $data;
*/
/*
$ch = curl_init();

// 2. set the options, including the url
curl_setopt($ch, CURLOPT_URL, "http://qa/ConsoleDataServices/service.svc/ConsoleDataServices/Measures?AccountID=106");
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($ch, CURLOPT_HEADER, 0);

// 3. execute and fetch the resulting HTML output
$output = curl_exec($ch);

// 4. free up the curl handle
curl_close($ch);
*/
?>