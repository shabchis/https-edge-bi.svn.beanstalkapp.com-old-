<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="graph.aspx.cs" Inherits="Easynet.Edge.UI.WebPages.graph" %>

<%@ Register Assembly="ZedGraph.Web" Namespace="ZedGraph.Web" TagPrefix="cc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Graph</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <cc1:ZedGraphWeb ID="ZedGraphWeb1" runat="server" 
            onrendergraph="ZedGraphWeb1_RenderGraph" Height="650" Width="1150">
<XAxis Cross="0" CrossAuto="True" AxisColor="Black" Title="" MinSpace="0" IsTicsBetweenLabels="True" IsZeroLine="False" IsVisible="True" Type="Linear" IsOmitMag="False" IsShowTitle="True" IsUseTenPower="False" IsPreventLabelOverlap="True">
<FontSpec Angle="0" Size="14" Family="Arial" FontColor="Black" StringAlignment="Center" IsBold="True" IsItalic="False" IsUnderline="False">
<Border Color="Black" IsVisible="False" Width="0" InflateFactor="0"></Border>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="None" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>
</FontSpec>

<MinorGrid Color="Black" IsVisible="False" DashOn="1" DashOff="5" PenWidth="1"></MinorGrid>

<MajorGrid Color="Black" IsVisible="False" DashOn="1" DashOff="5" PenWidth="1"></MajorGrid>

<MinorTic Color="Black" Size="5" IsOutside="True" IsInside="True" IsOpposite="True" PenWidth="1"></MinorTic>

<MajorTic Color="Black" Size="5" IsOutside="True" IsInside="True" IsOpposite="True" PenWidth="1"></MajorTic>

<Scale Min="0" Max="0" MajorStepAuto="True" MajorStep="1" MajorUnit="Day" MinorStepAuto="True" MinorStep="1" MinorUnit="Day" MinAuto="True" MaxAuto="True" MinGrace="0.1" MaxGrace="0.1" IsReverse="False" FormatAuto="True" Format="g" Align="Center" Mag="0" MagAuto="True">
<FontSpec Angle="0" Size="14" Family="Arial" FontColor="Black" StringAlignment="Center" IsBold="False" IsItalic="False" IsUnderline="False">
<Border Color="Black" IsVisible="False" Width="1" InflateFactor="0"></Border>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="None" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>
</FontSpec>
</Scale>
</XAxis>

<Y2Axis Cross="0" CrossAuto="True" AxisColor="Black" Title="" MinSpace="0" IsTicsBetweenLabels="False" IsZeroLine="True" IsVisible="False" Type="Linear" IsOmitMag="False" IsShowTitle="True" IsUseTenPower="False" IsPreventLabelOverlap="True">
<FontSpec Angle="0" Size="14" Family="Arial" FontColor="Black" StringAlignment="Center" IsBold="True" IsItalic="False" IsUnderline="False">
<Border Color="Black" IsVisible="False" Width="1" InflateFactor="0"></Border>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="None" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>
</FontSpec>

<MinorGrid Color="Black" IsVisible="False" DashOn="1" DashOff="5" PenWidth="1"></MinorGrid>

<MajorGrid Color="Black" IsVisible="False" DashOn="1" DashOff="5" PenWidth="1"></MajorGrid>

<MinorTic Color="White" Size="5" IsOutside="False" IsInside="False" IsOpposite="False" PenWidth="1"></MinorTic>

<MajorTic Color="White" Size="5" IsOutside="False" IsInside="False" IsOpposite="False" PenWidth="1"></MajorTic>

<Scale Min="0" Max="0" MajorStepAuto="True" MajorStep="1" MajorUnit="Day" MinorStepAuto="True" MinorStep="1" MinorUnit="Day" MinAuto="True" MaxAuto="True" MinGrace="0.1" MaxGrace="0.1" IsReverse="False" FormatAuto="True" Format="g" Align="Center" Mag="0" MagAuto="True">
<FontSpec Angle="-90" Size="14" Family="Arial" FontColor="Black" StringAlignment="Center" IsBold="False" IsItalic="False" IsUnderline="False">
<Border Color="Black" IsVisible="False" Width="1" InflateFactor="0"></Border>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="None" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>
</FontSpec>
</Scale>
</Y2Axis>

<FontSpec Angle="0" Size="16" Family="Arial" FontColor="Black" StringAlignment="Center" IsBold="True" IsItalic="False" IsUnderline="False">
<Border Color="Black" IsVisible="False" Width="1" InflateFactor="0"></Border>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="None" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>
</FontSpec>

<MasterPaneFill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="Solid" AlignH="Center" AlignV="Center" IsScaled="True"></MasterPaneFill>

<YAxis Cross="0" CrossAuto="True" AxisColor="Black" Title="" MinSpace="0" IsTicsBetweenLabels="False" IsZeroLine="True" IsVisible="True" Type="Linear" IsOmitMag="False" IsShowTitle="True" IsUseTenPower="False" IsPreventLabelOverlap="True">
<FontSpec Angle="-180" Size="14" Family="Arial" FontColor="Black" StringAlignment="Center" IsBold="True" IsItalic="False" IsUnderline="False">
<Border Color="Black" IsVisible="False" Width="1" InflateFactor="0"></Border>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="None" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>
</FontSpec>

<MinorGrid Color="Black" IsVisible="False" DashOn="1" DashOff="5" PenWidth="1"></MinorGrid>

<MajorGrid Color="Black" IsVisible="False" DashOn="1" DashOff="5" PenWidth="1"></MajorGrid>

<MinorTic Color="White" Size="5" IsOutside="False" IsInside="False" IsOpposite="False" PenWidth="1"></MinorTic>

<MajorTic Color="White" Size="5" IsOutside="False" IsInside="False" IsOpposite="False" PenWidth="1"></MajorTic>

<Scale Min="0" Max="0" MajorStepAuto="True" MajorStep="1" MajorUnit="Day" MinorStepAuto="True" MinorStep="1" MinorUnit="Day" MinAuto="True" MaxAuto="True" MinGrace="0.1" MaxGrace="0.1" IsReverse="False" FormatAuto="True" Format="g" Align="Center" Mag="0" MagAuto="True">
<FontSpec Angle="90" Size="14" Family="Arial" FontColor="Black" StringAlignment="Center" IsBold="False" IsItalic="False" IsUnderline="False">
<Border Color="Black" IsVisible="False" Width="1" InflateFactor="0"></Border>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="None" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>
</FontSpec>
</Scale>
</YAxis>

<Legend IsVisible="True" IsHStack="True" Position="Top" IsReverse="False">
<Location Height="0" Width="0" Y="0" X="0" AlignH="Right" AlignV="Center" CoordinateFrame="ChartFraction">
<TopLeft X="0" Y="0"></TopLeft>

<BottomRight X="0" Y="0"></BottomRight>
</Location>

<FontSpec Angle="0" Size="12" Family="Arial" FontColor="Black" StringAlignment="Center" IsBold="False" IsItalic="False" IsUnderline="False">
<Border Color="Black" IsVisible="False" Width="1" InflateFactor="0"></Border>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="Solid" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>
</FontSpec>

<Fill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="Brush" AlignH="Center" AlignV="Center" IsScaled="True"></Fill>

<Border Color="Black" IsVisible="False" Width="1" InflateFactor="0"></Border>
</Legend>

<PaneFill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="Solid" AlignH="Center" AlignV="Center" IsScaled="True"></PaneFill>

<ChartFill Color="White" ColorOpacity="100" IsVisible="True" RangeMax="0" RangeMin="0" Type="Brush" AlignH="Center" AlignV="Center" IsScaled="True"></ChartFill>

<ChartBorder Color="Black" IsVisible="True" Width="1" InflateFactor="0"></ChartBorder>

<MasterPaneBorder Color="Black" IsVisible="True" Width="1" InflateFactor="0"></MasterPaneBorder>

<Margins Left="10" Top="10" Right="10" Bottom="10"></Margins>

<PaneBorder Color="Black" IsVisible="True" Width="1" InflateFactor="0"></PaneBorder>
        </cc1:ZedGraphWeb>
    </div>
    </form>
</body>
</html>
