<%@ Page MasterPageFile="~/default.Master" Language="C#" AutoEventWireup="true" CodeBehind="RunConvertor.aspx.cs" Inherits="Easynet.Edge.UI.WebPages.WebImporter" %>
 
<asp:Content ID="Content1" ContentPlaceHolderID="HeadPlaceHolder" runat="server">
    <title>Run Convertor</title>
    <style type="text/css">
		.formField
		{
			margin-bottom: 8px;
		}
			.formField .label
			{
				width: 120px;
				float: left;
			}
			
			.formField .control
			{
			}
			
		.fileUpload
		{
			font-size: 12px;
			font-family: Verdana;
			float: left;
		}
		
		.linkButton
		{
			margin: 0px 5px 0px 5px;
		}	
		
		.filesList
		{
			margin-top: 3px;
			height: 120px;
			width: 100%;
			clear: both;
		}
		

    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ResultsPlaceHolder" runat="server">
	<div class="errorMessage" ><%= ErrorMessage %></div>
	<div class="successMessage" ><%= SuccessMessage %></div>

	<div class="formField">
	    <div class="label">Source:</div>
	    <div class="control">
		    <asp:DropDownList runat="server" AutoPostBack="false" id="_sourceSelector"
				OnSelectedIndexChanged="_sourceSelector_SelectedIndexChanged"
			>
				<asp:ListItem Text="Back Office" Value="BackOffice"/>
				<asp:ListItem Text="Bing" Value="Bing"/>
				<asp:ListItem Text="Yahoo" Value="Yahoo"/>
				<asp:ListItem Text="Creative .txt" Value="CreativeTXTfile"/>
				<asp:ListItem Text="Back Office .txt" Value="BoTXTfile"/>
		    </asp:DropDownList>
	    </div>
    </div>
    
    <div class="formField">
		<div class="label">Report file: </div>
		<div class="control" style="width: 400px">
			<asp:FileUpload runat="server" ID="_fileUpload" CssClass="fileUpload" />
			<div style="float:right">
				<asp:LinkButton runat="server" ID="_addFile" Text="add" CssClass="linkButton" OnClick="_addFile_Click"/>
				<asp:LinkButton runat="server" ID="_removeFile" Text="remove" CssClass="linkButton" OnClick="_removeFile_Click"/>
			</div>
			<asp:ListBox runat="server" ID="_listboxFiles" CssClass="filesList"/>  
		</div>
    </div>
    
 
	
	<%--<div class="formField">
		<div class="label">Save as:</div>
		<div class="control">
			<asp:TextBox  runat="server" ID="saveFilePathTextBox" Width="250" ></asp:TextBox>
		</div>
	</div>--%>
	
	<div class="formField">
		<div class="label"></div>
		<div class="control">
			<asp:Button runat="server" ID="_submit" Text="Go" Enabled="false" Font-Bold="true" Width="80" Height="25" OnClick="_submit_Click"/>
		</div>
	</div>
	
	
</asp:Content>
