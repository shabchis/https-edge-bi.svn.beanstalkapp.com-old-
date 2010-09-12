<?php  header ("Content-Type:text/xml");?>

<chart>
  <series>
    <value xid="0">Google</value>
    <value xid="1">Facebook</value>
    <value xid="2">Bing</value>
  </series>
  <graphs>
    <graph gid="0">
      <value xid="0"><?php echo rand(30, 60);?></value>
      <value xid="1"><?php echo rand(10, 30);?></value>
      <value xid="2"><?php echo rand(1, 15);?></value>
    </graph>
    <graph gid="1">
      <value xid="0"><?php echo rand(30, 60);?></value>
      <value xid="1"><?php echo rand(10, 30);?></value>
      <value xid="2"><?php echo rand(1, 15);?></value>
    </graph>
	
  </graphs>
</chart>