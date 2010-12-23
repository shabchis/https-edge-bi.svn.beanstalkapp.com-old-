<?php

function MY_hook()
{
    parent::Controller();
    $GLOBALS['EXT']->_call_hook('pre_system_constructor');
	
	
} 