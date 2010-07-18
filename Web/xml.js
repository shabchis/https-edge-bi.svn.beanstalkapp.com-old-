/**
 * Created by IntelliJ IDEA.
 * User: Yaron
 * Date: Jul 7, 2010
 * Time: 3:23:42 PM
 * To change this template use File | Settings | File Templates.
 */

	$.ajax({
		type: "GET",
		url: "tables.xml",
		dataType: "xml",
		success: function(xml) {

			$(xml).find('Table').each(function(){
				var id = $(this).find('TableID').text();
				var title = $(this).find('TableName').text();
				var url = $(this).find('url').text();
                $('<h2 class="trigger" id="trigger_'+id+'"></h2>').html('<a class= "link" href="#">'+title+'</a>').appendTo('#content');
                                   
                    $('<div class="toggle_container" id='+id+'>').appendTo('#content');
                    $('<div class="block" id = "block_'+id+'"></div>').appendTo('.toggle_container#'+id+'') ;
                    $('<ul class="list" id=list_'+id+'></ul>').appendTo('#block_'+id+'');
                    $('<li id='+id+'></li>').html(title).appendTo('#list_'+id+'');


			});
		}
	});
