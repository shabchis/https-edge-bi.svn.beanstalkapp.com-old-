
$(function(){
    //Hide (Collapse) the toggle containers on load

    $('h2.trigger').addClass("active");

	//Switch the "Open" and "Close" state per click then slide up/down (depending on open/close state)
	$("h2").live("click",function(){

		$(this).toggleClass("active").next().slideToggle("slow");
     
	});

})




