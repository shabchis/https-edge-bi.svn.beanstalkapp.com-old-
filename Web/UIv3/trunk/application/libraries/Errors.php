<?php

class Errors
{
	function ThrowEx($msg, $statusCode, $data = null, $redirect = false )
	{
		if (IS_AJAX)
		{
			if (!$data)
				$data = array();
			if (!$msg)
				$data['message'] = $msg;
			if ($redirect)
				$data['redirect'] = LOGIN_PAGE;
			
			header('HTTP/1.1 '. $statusCode);
			header('Content-Type: application/json');
			echo json_encode($data);
			exit();
		}
		else
		{
			if ($redirect)
				header( 'Location: '.LOGIN_PAGE ) ;
			else
				show_error($msg, $statusCode);
		}
	}
}
