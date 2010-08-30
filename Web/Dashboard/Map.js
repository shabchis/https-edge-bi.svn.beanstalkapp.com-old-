google.load('visualization', '1', {'packages': ['geomap']});
        google.setOnLoadCallback(drawMap);

         function drawMap() {
           var data = new google.visualization.DataTable();
           data.addRows(20);
           data.addColumn('string', 'Country');
           data.addColumn('number', 'Bo New Users');
           data.setValue(0, 0, 'Germany');
           data.setValue(0, 1, 4579);
           data.setValue(1, 0, 'United States');
           data.setValue(1, 1, 35742);
           data.setValue(2, 0, 'Italy');
           data.setValue(2, 1, 8413);
           data.setValue(3, 0, 'India');
           data.setValue(3, 1, 14592);
           data.setValue(4, 0, 'United Kingdom');
           data.setValue(4, 1, 9617);
           data.setValue(5, 0, 'RU');
           data.setValue(5, 1, 16782);
		   data.setValue(6, 0, 'France');
           data.setValue(6, 1, 5391);
		   data.setValue(7, 0, 'Spain');
           data.setValue(7, 1, 2696);
		   data.setValue(8, 0, 'Romania');
           data.setValue(8, 1, 4925);
		   data.setValue(9, 0, 'Turkey');
           data.setValue(9, 1, 2377);
		   data.setValue(10, 0, 'Australia');
           data.setValue(10, 1, 1600);
		   data.setValue(11, 0, 'Netherlands');
           data.setValue(11, 1, 457);
		   data.setValue(12, 0, 'Portugal');
           data.setValue(12, 1, 1620);
		   data.setValue(13, 0, 'Canada');
           data.setValue(13, 1, 19);
           data.setValue(14, 0, 'india');
           data.setValue(14, 1, 1);
		    data.setValue(15, 0, 'china');
           data.setValue(15, 1, 1);
           data.setValue(16, 0, 'brazil');
           data.setValue(16, 1, 1);
          data.setValue(17, 0, 'peru');
           data.setValue(17, 1, 1);
            data.setValue(18, 0, 'argentina');
           data.setValue(18, 1, 5000);
             data.setValue(19, 0, 'greenland');
           data.setValue(19, 1, 5000);

           var options = {};
           options['dataMode'] = 'Regions';

            options['colors'] = [0xE3EDCB, 0x90B63D]; //orange colors
            options['width'] = '100%';
            options['height'] = '300';

           var container = document.getElementById('map1');
           var geomap = new google.visualization.GeoMap(container);
           geomap.draw(data, options);
       };
