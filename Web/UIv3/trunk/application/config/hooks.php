<?php  if ( ! defined('BASEPATH')) exit('No direct script access allowed');
/*
| -------------------------------------------------------------------------
| Hooks
| -------------------------------------------------------------------------
| This file lets you define "hooks" to extend CI without hacking the core
| files.  Please see the user guide for info:
|
|	http://codeigniter.com/user_guide/general/hooks.html
|
*/


$hook['post_controller_constructor'] = array(
                                'class'    => 'MYlogin',
                                'function' => 'userlogin',
                                'filename' => 'Mylogin.php',
                                'filepath' => 'hooks'
                                
                                );
$hook['pre_system'] = array(
//$hook['post_system']=array(
                                'class'    => 'menuRouting',
                                'function' => 'index',
                                'filename' => 'menuRouting.php',
                                'filepath' => 'hooks'
                                
                                );

/* End of file hooks.php */
/* Location: ./system/application/config/hooks.php */