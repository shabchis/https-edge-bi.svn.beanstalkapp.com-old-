<?php  if ( ! defined('BASEPATH')) exit('No direct script access allowed');
/*
| -------------------------------------------------------------------------
| URI ROUTING
| -------------------------------------------------------------------------
| This file lets you re-map URI requests to specific controller functions.
|
| Typically there is a one-to-one relationship between a URL string
| and its corresponding controller class/method. The segments in a
| URL normally follow this pattern:
|
| 	example.com/class/method/id/
|
| In some instances, however, you may want to remap this relationship
| so that a different class/function is called than the one
| corresponding to the URL.
|
| Please see the user guide for complete details:
|
|	http://codeigniter.com/user_guide/general/routing.html
|
| -------------------------------------------------------------------------
| RESERVED ROUTES
| -------------------------------------------------------------------------
|
| There are two reserved routes:
|
|	$route['default_controller'] = 'welcome';
|
| This route indicates which controller class should be loaded if the
| URI contains no data. In the above example, the "welcome" class
| would be loaded.
|
|	$route['scaffolding_trigger'] = 'scaffolding';
|
| This route lets you set a "secret" word that will trigger the
| scaffolding feature for added security. Note: Scaffolding must be
| enabled in the controller in which you intend to use it.   The reserved 
| routes must come before any wildcard or regular expression routes.
|
*/
$route['default_controller'] = "main";
$route['scaffolding_trigger'] = "";

// Merge controller names from menu
global $MENU_ROUTES;
if(!empty($MENU_ROUTES)) $route = array_merge($route,$MENU_ROUTES);

$route['Intelligence/Dashboard']= "iframe_controller";
$route[':any']= "Home";
/*
$route['Intelligence/:any']= "Home";
$route['Intelligence/Dashboard/Test1']= "dashboard";
$route['Intelligence/Dashboard/Test2']="dashboard";
$route['Intelligence/Dashboard/Test1/1']="dashboard";
$route['Intelligence/Dashboard/:any/:any']= "dashboard";
$route['Intelligence/Dashboard/:any']= "dashboard";
*/



/* End of file routes.php */
/* Location: ./system/application/config/routes.php */