
google.load('visualization', '1', {packages: ['corechart']});


function drawVisualization() {
  // Create and populate the data table.
  var data = new google.visualization.DataTable();
  var raw_data = [['Google', 400],
                  ['Bing', 381],
                  ['Facebook', 157]];

  var CPA = ['Yesterday'];

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
     new google.visualization.ColumnChart(document.getElementById('yesterday')).
      draw(data,
           {title:"Yesterday",
            width:390, height:300,
                colors:['#90B63D','#8d8d8d','#E3EDCB']
            }
      );

}


google.setOnLoadCallback(drawVisualization);
