
$(function(){

	var hash = "" ;
	$('h2.trigger').addClass("active");
    
	//Switch the "Open" and "Close" state per click then slide up/down (depending on open/close state)
	$("h2.trigger").live("click",function(){
    
        $(this).toggleClass("active").next().slideToggle("slow");
         
      });
      
    
     
        $("#menu").delegate("a", "click", function() {
           window.location.hash = $(this).attr("href");
            return false;
        });
       
        $(window).bind('hashchange', function(){
          // If no hash is available, use default
          if (window.location.hash.length < 1)
            hash = 'home';
          else
            hash = window.location.hash.substring(1);
          
          $.ajax({
             
              url: ''+hash+'',
              success: function(data) {
            
               
                 $("#ajaxloader").fadeIn();
                $('#main').html(data);
                 $("#ajaxloader").fadeOut();
                  $("#menu li").removeClass("current");
                   $("#menu li a[href="+hash+"]").parent().addClass("current");
                
              }
              
            });

        });
        
        $(window).trigger('hashchange');
    
      
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
        $("#menu").toggleClass("hide");
        // $("#menu").animate({width:'toggle'},1500);
            $(this).toggleClass("closed");
            $("#container").toggleClass("sidebar");
    
       $("#caption").html("Show");
          
        
        });
    

})

