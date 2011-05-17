namespace NewRestApiTester__
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.localCheckBox = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.QueryStringstextBox = new System.Windows.Forms.TextBox();
			this.ServiceAddressComboBox = new System.Windows.Forms.ComboBox();
			this.SetButton = new System.Windows.Forms.Button();
			this.GetButton = new System.Windows.Forms.Button();
			this.HeaderslistView = new System.Windows.Forms.ListView();
			this.Key = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.Value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.HeaderKeytextBox = new System.Windows.Forms.TextBox();
			this.HeaderValuetextBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.AddUpdateButton = new System.Windows.Forms.Button();
			this.ClearButton = new System.Windows.Forms.Button();
			this.removeButton = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.ResponseBodyRichTextBox = new System.Windows.Forms.RichTextBox();
			this.FormatComboBox = new System.Windows.Forms.ComboBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.ResponseHeaderTextBox = new System.Windows.Forms.TextBox();
			this.DeleteButton = new System.Windows.Forms.Button();
			this.btnPut = new System.Windows.Forms.Button();
			this.BodyTextBox = new System.Windows.Forms.RichTextBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.findButton = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(74, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Service Address";
			// 
			// localCheckBox
			// 
			this.localCheckBox.AutoSize = true;
			this.localCheckBox.Checked = true;
			this.localCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.localCheckBox.Location = new System.Drawing.Point(12, 10);
			this.localCheckBox.Name = "localCheckBox";
			this.localCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.localCheckBox.Size = new System.Drawing.Size(52, 17);
			this.localCheckBox.TabIndex = 2;
			this.localCheckBox.Text = "Local";
			this.localCheckBox.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(418, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(122, 34);
			this.label2.TabIndex = 4;
			this.label2.Text = "Query strings(seperated with \",\" respectively)";
			// 
			// QueryStringstextBox
			// 
			this.QueryStringstextBox.Location = new System.Drawing.Point(548, 11);
			this.QueryStringstextBox.Name = "QueryStringstextBox";
			this.QueryStringstextBox.Size = new System.Drawing.Size(178, 20);
			this.QueryStringstextBox.TabIndex = 3;
			// 
			// ServiceAddressComboBox
			// 
			this.ServiceAddressComboBox.FormattingEnabled = true;
			this.ServiceAddressComboBox.Location = new System.Drawing.Point(162, 10);
			this.ServiceAddressComboBox.Name = "ServiceAddressComboBox";
			this.ServiceAddressComboBox.Size = new System.Drawing.Size(250, 21);
			this.ServiceAddressComboBox.TabIndex = 5;
			this.ServiceAddressComboBox.SelectedIndexChanged += new System.EventHandler(this.ServiceAddressComboBox_SelectedIndexChanged);
			this.ServiceAddressComboBox.Validated += new System.EventHandler(this.ServiceAddressComboBox_Validated);
			// 
			// SetButton
			// 
			this.SetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SetButton.Location = new System.Drawing.Point(660, 560);
			this.SetButton.Name = "SetButton";
			this.SetButton.Size = new System.Drawing.Size(75, 23);
			this.SetButton.TabIndex = 6;
			this.SetButton.Text = "POST";
			this.SetButton.UseVisualStyleBackColor = true;
			this.SetButton.Click += new System.EventHandler(this.SetButton_Click);
			// 
			// GetButton
			// 
			this.GetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GetButton.Location = new System.Drawing.Point(578, 560);
			this.GetButton.Name = "GetButton";
			this.GetButton.Size = new System.Drawing.Size(75, 23);
			this.GetButton.TabIndex = 7;
			this.GetButton.Text = "GET";
			this.GetButton.UseVisualStyleBackColor = true;
			this.GetButton.Click += new System.EventHandler(this.GetButton_Click);
			// 
			// HeaderslistView
			// 
			this.HeaderslistView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Key,
            this.Value});
			this.HeaderslistView.FullRowSelect = true;
			this.HeaderslistView.GridLines = true;
			this.HeaderslistView.Location = new System.Drawing.Point(12, 63);
			this.HeaderslistView.MultiSelect = false;
			this.HeaderslistView.Name = "HeaderslistView";
			this.HeaderslistView.Size = new System.Drawing.Size(306, 132);
			this.HeaderslistView.TabIndex = 8;
			this.HeaderslistView.UseCompatibleStateImageBehavior = false;
			this.HeaderslistView.View = System.Windows.Forms.View.Details;
			// 
			// Key
			// 
			this.Key.Text = "Key";
			this.Key.Width = 150;
			// 
			// Value
			// 
			this.Value.Text = "Value";
			this.Value.Width = 200;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 47);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(50, 13);
			this.label3.TabIndex = 9;
			this.label3.Text = "Headers:";
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(14, 209);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(34, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Body:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(324, 69);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(28, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "Key:";
			// 
			// HeaderKeytextBox
			// 
			this.HeaderKeytextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderKeytextBox.Location = new System.Drawing.Point(375, 62);
			this.HeaderKeytextBox.Name = "HeaderKeytextBox";
			this.HeaderKeytextBox.Size = new System.Drawing.Size(100, 20);
			this.HeaderKeytextBox.TabIndex = 13;
			// 
			// HeaderValuetextBox
			// 
			this.HeaderValuetextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderValuetextBox.Location = new System.Drawing.Point(544, 62);
			this.HeaderValuetextBox.Name = "HeaderValuetextBox";
			this.HeaderValuetextBox.Size = new System.Drawing.Size(100, 20);
			this.HeaderValuetextBox.TabIndex = 15;
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(493, 69);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(37, 13);
			this.label6.TabIndex = 14;
			this.label6.Text = "Value:";
			// 
			// AddUpdateButton
			// 
			this.AddUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddUpdateButton.Location = new System.Drawing.Point(661, 60);
			this.AddUpdateButton.Name = "AddUpdateButton";
			this.AddUpdateButton.Size = new System.Drawing.Size(75, 23);
			this.AddUpdateButton.TabIndex = 16;
			this.AddUpdateButton.Text = "Add/Update";
			this.AddUpdateButton.UseVisualStyleBackColor = true;
			this.AddUpdateButton.Click += new System.EventHandler(this.AddUpdateButton_Click);
			// 
			// ClearButton
			// 
			this.ClearButton.Location = new System.Drawing.Point(327, 117);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(75, 23);
			this.ClearButton.TabIndex = 17;
			this.ClearButton.Text = "Clear";
			this.ClearButton.UseVisualStyleBackColor = true;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// removeButton
			// 
			this.removeButton.Location = new System.Drawing.Point(327, 88);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = new System.Drawing.Size(75, 23);
			this.removeButton.TabIndex = 18;
			this.removeButton.Text = "Remove";
			this.removeButton.UseVisualStyleBackColor = true;
			this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.ResponseBodyRichTextBox);
			this.panel1.Controls.Add(this.FormatComboBox);
			this.panel1.Controls.Add(this.label10);
			this.panel1.Controls.Add(this.label9);
			this.panel1.Controls.Add(this.label7);
			this.panel1.Controls.Add(this.ResponseHeaderTextBox);
			this.panel1.Location = new System.Drawing.Point(327, 147);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(408, 375);
			this.panel1.TabIndex = 19;
			// 
			// ResponseBodyRichTextBox
			// 
			this.ResponseBodyRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ResponseBodyRichTextBox.Location = new System.Drawing.Point(25, 133);
			this.ResponseBodyRichTextBox.Name = "ResponseBodyRichTextBox";
			this.ResponseBodyRichTextBox.ReadOnly = true;
			this.ResponseBodyRichTextBox.Size = new System.Drawing.Size(309, 231);
			this.ResponseBodyRichTextBox.TabIndex = 6;
			this.ResponseBodyRichTextBox.Text = "";
			// 
			// FormatComboBox
			// 
			this.FormatComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FormatComboBox.FormattingEnabled = true;
			this.FormatComboBox.Items.AddRange(new object[] {
            "XML",
            "JSON"});
			this.FormatComboBox.Location = new System.Drawing.Point(169, 96);
			this.FormatComboBox.Name = "FormatComboBox";
			this.FormatComboBox.Size = new System.Drawing.Size(121, 21);
			this.FormatComboBox.TabIndex = 5;
			this.FormatComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(121, 103);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(42, 13);
			this.label10.TabIndex = 4;
			this.label10.Text = "Format:";
			// 
			// label9
			// 
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(22, 104);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(79, 13);
			this.label9.TabIndex = 2;
			this.label9.Text = "ResponseText:";
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(24, 10);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(96, 13);
			this.label7.TabIndex = 1;
			this.label7.Text = "Response Header:";
			// 
			// ResponseHeaderTextBox
			// 
			this.ResponseHeaderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ResponseHeaderTextBox.Location = new System.Drawing.Point(21, 33);
			this.ResponseHeaderTextBox.Multiline = true;
			this.ResponseHeaderTextBox.Name = "ResponseHeaderTextBox";
			this.ResponseHeaderTextBox.ReadOnly = true;
			this.ResponseHeaderTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ResponseHeaderTextBox.Size = new System.Drawing.Size(313, 57);
			this.ResponseHeaderTextBox.TabIndex = 0;
			// 
			// DeleteButton
			// 
			this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteButton.Location = new System.Drawing.Point(496, 560);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = new System.Drawing.Size(75, 23);
			this.DeleteButton.TabIndex = 20;
			this.DeleteButton.Text = "DELETE";
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// btnPut
			// 
			this.btnPut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPut.Location = new System.Drawing.Point(415, 560);
			this.btnPut.Name = "btnPut";
			this.btnPut.Size = new System.Drawing.Size(75, 23);
			this.btnPut.TabIndex = 21;
			this.btnPut.Text = "PUT";
			this.btnPut.UseVisualStyleBackColor = true;
			this.btnPut.Click += new System.EventHandler(this.btnPut_Click);
			// 
			// BodyTextBox
			// 
			this.BodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BodyTextBox.Location = new System.Drawing.Point(12, 226);
			this.BodyTextBox.Name = "BodyTextBox";
			this.BodyTextBox.Size = new System.Drawing.Size(306, 296);
			this.BodyTextBox.TabIndex = 22;
			this.BodyTextBox.Text = "";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(48, 528);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(197, 53);
			this.textBox1.TabIndex = 23;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 528);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(30, 13);
			this.label8.TabIndex = 24;
			this.label8.Text = "Find:";
			// 
			// findButton
			// 
			this.findButton.Location = new System.Drawing.Point(252, 528);
			this.findButton.Name = "findButton";
			this.findButton.Size = new System.Drawing.Size(75, 23);
			this.findButton.TabIndex = 25;
			this.findButton.Text = "Find";
			this.findButton.UseVisualStyleBackColor = true;
			this.findButton.Click += new System.EventHandler(this.findButton_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(327, 558);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 26;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(748, 595);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.findButton);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.BodyTextBox);
			this.Controls.Add(this.btnPut);
			this.Controls.Add(this.DeleteButton);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.removeButton);
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.AddUpdateButton);
			this.Controls.Add(this.HeaderValuetextBox);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.HeaderKeytextBox);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.HeaderslistView);
			this.Controls.Add(this.GetButton);
			this.Controls.Add(this.SetButton);
			this.Controls.Add(this.ServiceAddressComboBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.QueryStringstextBox);
			this.Controls.Add(this.localCheckBox);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox localCheckBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox QueryStringstextBox;
		private System.Windows.Forms.ComboBox ServiceAddressComboBox;
		private System.Windows.Forms.Button SetButton;
		private System.Windows.Forms.Button GetButton;
		private System.Windows.Forms.ListView HeaderslistView;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox HeaderKeytextBox;
		private System.Windows.Forms.TextBox HeaderValuetextBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button AddUpdateButton;
		private System.Windows.Forms.Button ClearButton;
		private System.Windows.Forms.Button removeButton;
		private System.Windows.Forms.ColumnHeader Key;
		private System.Windows.Forms.ColumnHeader Value;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ComboBox FormatComboBox;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox ResponseHeaderTextBox;
		private System.Windows.Forms.RichTextBox ResponseBodyRichTextBox;
		private System.Windows.Forms.Button DeleteButton;
		private System.Windows.Forms.Button btnPut;
		private System.Windows.Forms.RichTextBox BodyTextBox;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button findButton;
		private System.Windows.Forms.Button button1;
	}
}

