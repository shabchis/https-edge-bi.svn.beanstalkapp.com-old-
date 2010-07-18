
$(document).ready(function()
      {
        $.get('tables.xml', function(d){
        $('body').append('<h1> Recommended Web Development Books </h1>');
        $('body').append('<dl>');

        $(d).find('Table').each(function(){

            var $Table = $(this);
            var description = $book.find('TableName').text();


            var html = '<dt> <img class="bookImage" alt="" src="' + description + '" /> </dt>';
            html += '<dd> <span class="loadingPic" alt="Loading" />';
            html += '<p class="title">' + description + '</p>';
            html += '<p> ' + description + '</p>' ;
            html += '</dd>';

            $('dl').append($(html));


    });
});
</dl>