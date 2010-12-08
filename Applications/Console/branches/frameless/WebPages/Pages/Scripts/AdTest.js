      var winAdTresh = 0;
      var loseAdTresh = 15;
//                  Sys.WebForms.PageRequestManager.getInstance().add_endRequest(pageLoad);

//                  function pageLoad() {
//                      loseAdTresh = 15;
//                  }
      var primMeasureTresh = '';

      function OnPrimaryMeasureChanged()
      {
      	var dropdown = document.getElementById(_primaryMeasureID);
      	var text = dropdown.options[document.getElementById(_primaryMeasureID).selectedIndex].text;
      	document.getElementById('primMeasureThreshLabel').innerHTML = text + ' Threshold:';
      }

      function checkThreshold()
      {
      	var _losingAdThreshold = document.getElementById(_losingAdThresholdID);
      	var _primaryThreshold = document.getElementById(_primaryThresholdID);

      	_losingAdThreshold.value = _losingAdThreshold.value.split(" ").join("");
      	_primaryThreshold.value = _primaryThreshold.value.split(" ").join("");
      	
      	if ((_primaryThreshold.value != parseFloat(_primaryThreshold.value) && _primaryThreshold.value != "") ||
			(_losingAdThreshold.value != parseFloat(_losingAdThreshold.value) && _losingAdThreshold.value != ""))
      	{
      		return "Threshold contains non-numeric characters.";
      	}
      	if ((parseFloat(_primaryThreshold.value) < 0) || parseFloat(_losingAdThreshold.value) > 100 || parseFloat(_losingAdThreshold.value) < 0)
      	{
      		return "Illegal value.";
      	}
      	else
      	{
      		return "";
      	}
      }

      function ConfirmForm()
      {
      	var thresholdError = checkThreshold();
      	if (thresholdError != "")
      	{
      		$get('warning').innerHTML = thresholdError;
      		window.setTimeout(function() { PopupOpen('thresholdControl'); }, 0);
      		return false;
      	}
	      
      	if (document.getElementById(_campaignSelectorID).value == -1)
      	{
      		if (confirm("Retrieving data for all campaigns might take a few minutes. Proceed anyway? "))
      		{
      			return true;
      		}
      		else return false;
      	}
      	else return true;
      }