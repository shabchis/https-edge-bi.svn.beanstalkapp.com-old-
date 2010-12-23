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


global $REQUEST_PATH;

// Find the path after the application root (in relation to index.php of CodeIgniter)
$applicationRoot = str_replace('index.php', '', $_SERVER['SCRIPT_NAME']);
$REQUEST_PATH = str_replace($applicationRoot, '', $_SERVER['REQUEST_URI']);


$hook['pre_system'][] = array(
                                'class'    => 'sessionValidation',
                                'function' => 'index',
                                'filename' => 'sessionValidation.php',
                                'filepath' => 'hooks'
                                );

$hook['pre_system'][] = array(
                                'class'    => 'menuRouting',
                                'function' => 'index',
                                'filename' => 'menuRouting.php',
                                'filepath' => 'hooks'
                                );

/* End of file hooks.php */
/* Location: ./system/application/config/hooks.php */