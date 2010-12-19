
<?php  $account_id = $_GET['account_id'];?>

<?php


 $url = 'http://qa/ConsoleDataServices/service.svc/Measures?AccountID='.$_GET['account_id'].'';
 
 // echo $url;
if(!$xml=simplexml_load_file($url)){
    // trigger_error('Error reading XML file',E_USER_ERROR);
}

foreach($xml as $measure){
    echo '<option value="'.$measure->MeasureID.'"> '.$measure->DisplayName.'</option>' ;
}

?>