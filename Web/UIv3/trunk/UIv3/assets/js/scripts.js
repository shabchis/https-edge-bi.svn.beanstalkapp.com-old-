var url = "";
$(function(){
	 //DD_roundies.addRule('#accounts', '5px');
	
	
	var hash = "" ;
	$('h2.trigger').addClass("active");
    
	//Switch the "Open" and "Close" state per click then slide up/down (depending on open/close state)
	$("h2.trigger").live("click",function(){
    

		$(this).toggleClass("active").next().slideToggle("slow");
	});
      
  
    

		$("#sub").delegate("a", "click", function() {
			location.hash = $(this).attr("href");
			var item = $(this).parent(".current");
			var text = item.text();
			var content =""	;
			var $parent = "";
			var $grand = "";
			if(item.parent("ul").hasClass("parent")){
				$parent = item.parent("ul.parent").attr("data-name");
				$section = item.parent().parent().parent().parent().parent().prev("h2.trigger").find("span").text();
				//console.log($section+"->"+$parent+"->"+text );
				
					
					 content =	$("<ul><li>"+$section+"</li><li>"+$parent+"</li><li>"+item.find("a:first").text()+"</li></ul>");
		
					$("#breadcrumbs").html(content);
			}	
			else if(item.parent().parent().parent("ul").hasClass("parent")){
				$parent = item.parent("ul").attr("data-name");
				$grand = item.parent().parent().parent("ul.parent").attr("data-name");
				$section = item.parent().parent().parent().parent().parent().parent().parent().prev("h2.trigger").find("span").text();
				 content =	$("<ul><li>"+$section+"</li><li>"+$grand+"</li><li>"+$parent+"</li><li>"+text+"</li></ul>");
				$("#breadcrumbs").html(content);
				 
			
			}
			else if(item.parent().hasClass("list"))
			{
			
				$section = item.parent().parent().parent().prev("h2.trigger").find("span").text();
				content = $("<ul><li>"+$section+"</li><li>"+item.find("a:first").text()+"</li></ul>");
				//console.log($section+"->"+item.find("a:first").text() );
					
						$("#breadcrumbs").html(content);
			}
			else if(window.location.hash == ""||!window.location.hash){
				
					$("#breadcrumbs ul").empty();
			}
			
		
			else
			{
				$section = item.parent().parent().parent().prev("h2.trigger").find("span").text();
				content = $("<ul><li>"+$section+"</li><li>"+text+"</li></ul>");
				$("#breadcrumbs").html(content);
			
			}
				
			
			return false;
			
			
		});

		$(window).hashchange(function(){
			// If no hash is available, use default
			if (location.hash.length < 1)
				hash = 'home';
			else
				hash = location.hash.substring(1);
          
			$.ajax({
             
				url: ''+hash+'',
				success: function(data) {
				$("#ajaxloader").fadeIn();
					$('#main').html(data);
					$("#ajaxloader").fadeOut();
				
				
			},
				complete:function(data){
				
					$("#sub li").removeClass("current");
					$("#sub li a[href="+hash+"]").parent().addClass("current");
					
					
					
			var item = $("#sub a").parent(".current");
			var text = item.text();
			var content =""	;
			var $parent = "";
			var $grand = "";
			if(item.parent("ul").hasClass("parent")){
				$parent = item.parent("ul.parent").attr("data-name");
				$section = item.parent().parent().parent().parent().parent().prev("h2.trigger").find("span").text();
				//console.log($section+"->"+$parent+"->"+text );
				
					
					 content =	$("<ul><li>"+$section+"</li><li>"+$parent+"</li><li>"+item.find("a:first").text()+"</li></ul>");
		
					$("#breadcrumbs").html(content);
			}	
			else if(item.parent().parent().parent("ul").hasClass("parent")){
				$parent = item.parent("ul").attr("data-name");
				$grand = item.parent().parent().parent("ul.parent").attr("data-name");
				$section = item.parent().parent().parent().parent().parent().parent().parent().prev("h2.trigger").find("span").text();
				 content =	$("<ul><li>"+$section+"</li><li>"+$grand+"</li><li>"+$parent+"</li><li>"+text+"</li></ul>");
				$("#breadcrumbs").html(content);
			
			}
			else if(item.parent().hasClass("list"))
			{
			
				$section = item.parent().parent().parent().prev("h2.trigger").find("span").text();
				content = $("<ul><li>"+$section+"</li><li>"+item.find("a:first").text()+"</li></ul>");
				//console.log($section+"->"+item.find("a:first").text() );
					
						$("#breadcrumbs").html(content);
			}
			else if(location.hash == ""||!location.hash){
				
				content = "<ul><li>Home</li></ul>";
					$("#breadcrumbs").html(content);
			}
			else
			{
				$section = item.parent().parent().parent().prev("h2.trigger").find("span").text();
				content = $("<ul><li>"+$section+"</li><li>"+text+"</li></ul>");
				$("#breadcrumbs").html(content);
				
			
			}
				
				}
				
			});

			});
        
		$(window).hashchange();
    
      
		$("#login select").change(function(){
		$("#login option").removeClass("selected");
		$("#login option:selected").addClass("selected");
      
     	 });
      
      
		$("#menu li a").click(function(){
			$("#menu li").removeClass("current");
			$(this).parent().addClass("current");
    
      
			});
		$("#slider span").hover(function(){
        
			$("#caption").html("Hide").show();
               
			},
			function(){
			$("#caption").hide();
         
		}
		);
		$("#slider span").click(function(){
			//$("#menu").toggleClass("hide");
      
			//$(this).toggleClass("closed");
			//$("#container").toggleClass("sidebar");
    
			$("#caption").html("Show");
          
          $("#menu").animate({width:'toggle'},1500);
			});
    /*
    account 
    */
    $("#accounts").delegate("li","click",function(){
    	
    	//console.log($(this).text());
    	
    	var $parents = "";
    	var url = $("#selected").text();
    	var $sub  = "";
    	
    	//	 $parents = $(this).parent().parent().prev(".campaign").find("span").text()
    	if($(this).hasClass("parent"))
    	{
    		
    		 $parents = $(this).siblings(".campaign").find("span").text();
    		 $sub  = $(this).find('a:first').text();
    		
    	}
    	else if($(this).hasClass("campaign")){
    		
			$parents = 	$(this).find("span").text();
			$sub  = "";
    	}
    	else
    	{
				$parents = $(this).parent().parent().siblings(".campaign").find("span").text();
				$sub = $(this).text()
    		
    	}
    	
    	//console.log($sub);
			$("#selected").html($parents);
    		$("#accounts").removeClass("unfolded");
			$("#accounts").addClass("folded");
			$("#arrow").removeClass("pressed");
			$("#arrow").addClass("regular");
			$("#Campaign").html($sub);
			$("#Campaign").show();
			return false;
			
   });
    

		$("#arrow").toggle(function(){
			$(this).removeClass("regular");
			$(this).addClass("pressed");
			$("#accounts").removeClass("folded");
			$("#accounts").addClass("unfolded");
			$("#accounts").animateToSelector({
    			selectors: ['.unfolded'],
   				 properties: [
   				 'height','-moz-box-shadow','box-shadow'
   				 ],
   				 duration:[100000],
    events: ['click']
});

			$("#Campaign").hide();
		},function(){
			$(this).removeClass("pressed");
			$(this).addClass("regular");
			$("#accounts").removeClass("unfolded");
			$("#accounts").addClass("folded");
			$("#accounts").animateToSelector({
    			selectors: ['.folded'],
   				 properties: [
   				 'height','-moz-box-shadow','box-shadow'
   				 ],
   				 duration:[100000],
    events: ['click']
});
			$("#Campaign").show();
		});
		
		/*breadcrumbs*/
		
		
		
})

