
      google.load('visualization', '1', {packages: ['corechart']});

 
      function drawVisualization() {
        // Create and populate the data table.
        var data = new google.visualization.DataTable();
        var raw_data = [['Google', 300.83043280182231, 309.8804556],
                        ['Bing', 160.190526315789, 169.190526315789],
                        ['Facebook', 129.190526315789, 161.721]];

        var CPA = ['A Week ago', 'Yesterday'];

        data.addColumn('string', 'CPA');
        for (var i = 0; i  < raw_data.length; ++i) {
          data.addColumn('number', raw_data[i][0]);
        }

        data.addRows(CPA.length);

        for (var j = 0; j < CPA.length; ++j) {
          data.setValue(j, 0, CPA[j].toString());
        }
        for (var i = 0; i  < raw_data.length; ++i) {
          for (var j = 1; j  < raw_data[i].length; ++j) {
            data.setValue(j-1, i+1, raw_data[i][j]);
          }
        }
        


        // Create and draw the visualization.
           new google.visualization.ColumnChart(document.getElementById('weekago')).
            draw(data,
                 {title:"Yesterday vs a week ago",
                  width:390, height:250,
                  colors:['#90B63D','#8d8d8d','#E3EDCB'],
                  hAxis: {title: "Year"}}
            );
      
      }


      google.setOnLoadCallback(drawVisualization);
