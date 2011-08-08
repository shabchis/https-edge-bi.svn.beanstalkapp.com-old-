namespace MyNewWizard
{
	partial class CreateNewCube
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
            this.components = new System.ComponentModel.Container();
            this.tabsCreateCube = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.chkProcessCubes = new System.Windows.Forms.CheckBox();
            this.chkContent = new System.Windows.Forms.CheckBox();
            this.tabMeasures = new System.Windows.Forms.TabPage();
            this.btnClearBeData = new System.Windows.Forms.Button();
            this.btnRemoveBeData = new System.Windows.Forms.Button();
            this.listViewBeData = new System.Windows.Forms.ListView();
            this.From = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.To = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OnlyCalc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chkBeOnlyCalc = new System.Windows.Forms.CheckBox();
            this.btnAddBe = new System.Windows.Forms.Button();
            this.txtBeReplace = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.cmbBeData = new System.Windows.Forms.ComboBox();
            this.label30 = new System.Windows.Forms.Label();
            this.tabCPAS = new System.Windows.Forms.TabPage();
            this.txtTargetCPA = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAccountID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkAddMeasure = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTargetAcquisitionName = new System.Windows.Forms.TextBox();
            this.cmbCpaReplaceTo = new System.Windows.Forms.ComboBox();
            this.btnClearCPA = new System.Windows.Forms.Button();
            this.btnRemoveCpa = new System.Windows.Forms.Button();
            this.livCPA = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chkOnlyCalcCPA = new System.Windows.Forms.CheckBox();
            this.btnAddCPA = new System.Windows.Forms.Button();
            this.label34 = new System.Windows.Forms.Label();
            this.cmbCPA = new System.Windows.Forms.ComboBox();
            this.label35 = new System.Windows.Forms.Label();
            this.tabStringReplace = new System.Windows.Forms.TabPage();
            this.txtString = new System.Windows.Forms.TextBox();
            this.btnClearStrings = new System.Windows.Forms.Button();
            this.btnRemoveStringReplace = new System.Windows.Forms.Button();
            this.livStrings = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chkCalcOnlyString = new System.Windows.Forms.CheckBox();
            this.btnAddStringReplace = new System.Windows.Forms.Button();
            this.txtStringReplace = new System.Windows.Forms.TextBox();
            this.label36 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabsCreateCube.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabMeasures.SuspendLayout();
            this.tabCPAS.SuspendLayout();
            this.tabStringReplace.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabsCreateCube
            // 
            this.tabsCreateCube.Controls.Add(this.tabGeneral);
            this.tabsCreateCube.Controls.Add(this.tabMeasures);
            this.tabsCreateCube.Controls.Add(this.tabCPAS);
            this.tabsCreateCube.Controls.Add(this.tabStringReplace);
            this.tabsCreateCube.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tabsCreateCube.Location = new System.Drawing.Point(0, 87);
            this.tabsCreateCube.Name = "tabsCreateCube";
            this.tabsCreateCube.SelectedIndex = 0;
            this.tabsCreateCube.Size = new System.Drawing.Size(789, 298);
            this.tabsCreateCube.TabIndex = 1;
            this.tabsCreateCube.SelectedIndexChanged += new System.EventHandler(this.tabsCreateCube_SelectedIndexChanged);
            this.tabsCreateCube.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabsCreateCube_Selected);
            this.tabsCreateCube.TabIndexChanged += new System.EventHandler(this.tabsCreateCube_TabIndexChanged);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.linkLabel1);
            this.tabGeneral.Controls.Add(this.chkProcessCubes);
            this.tabGeneral.Controls.Add(this.chkContent);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(781, 272);
            this.tabGeneral.TabIndex = 1;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(14, 86);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(71, 13);
            this.linkLabel1.TabIndex = 35;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Flow Diagram";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // chkProcessCubes
            // 
            this.chkProcessCubes.AutoSize = true;
            this.chkProcessCubes.Location = new System.Drawing.Point(17, 55);
            this.chkProcessCubes.Name = "chkProcessCubes";
            this.chkProcessCubes.Size = new System.Drawing.Size(97, 17);
            this.chkProcessCubes.TabIndex = 34;
            this.chkProcessCubes.Text = "Process Cubes";
            this.chkProcessCubes.UseVisualStyleBackColor = true;
            // 
            // chkContent
            // 
            this.chkContent.AutoSize = true;
            this.chkContent.Location = new System.Drawing.Point(17, 23);
            this.chkContent.Name = "chkContent";
            this.chkContent.Size = new System.Drawing.Size(85, 17);
            this.chkContent.TabIndex = 4;
            this.chkContent.Text = "Has Content";
            this.chkContent.UseVisualStyleBackColor = true;
            // 
            // tabMeasures
            // 
            this.tabMeasures.Controls.Add(this.btnClearBeData);
            this.tabMeasures.Controls.Add(this.btnRemoveBeData);
            this.tabMeasures.Controls.Add(this.listViewBeData);
            this.tabMeasures.Controls.Add(this.chkBeOnlyCalc);
            this.tabMeasures.Controls.Add(this.btnAddBe);
            this.tabMeasures.Controls.Add(this.txtBeReplace);
            this.tabMeasures.Controls.Add(this.label31);
            this.tabMeasures.Controls.Add(this.cmbBeData);
            this.tabMeasures.Controls.Add(this.label30);
            this.tabMeasures.Location = new System.Drawing.Point(4, 22);
            this.tabMeasures.Name = "tabMeasures";
            this.tabMeasures.Size = new System.Drawing.Size(781, 272);
            this.tabMeasures.TabIndex = 3;
            this.tabMeasures.Text = "BE Data";
            this.tabMeasures.UseVisualStyleBackColor = true;
            // 
            // btnClearBeData
            // 
            this.btnClearBeData.Location = new System.Drawing.Point(226, 159);
            this.btnClearBeData.Name = "btnClearBeData";
            this.btnClearBeData.Size = new System.Drawing.Size(39, 23);
            this.btnClearBeData.TabIndex = 5;
            this.btnClearBeData.Text = "Clear";
            this.btnClearBeData.UseVisualStyleBackColor = true;
            this.btnClearBeData.Click += new System.EventHandler(this.btnClearBeData_Click);
            // 
            // btnRemoveBeData
            // 
            this.btnRemoveBeData.Location = new System.Drawing.Point(226, 86);
            this.btnRemoveBeData.Name = "btnRemoveBeData";
            this.btnRemoveBeData.Size = new System.Drawing.Size(39, 23);
            this.btnRemoveBeData.TabIndex = 4;
            this.btnRemoveBeData.Text = "<<";
            this.btnRemoveBeData.UseVisualStyleBackColor = true;
            this.btnRemoveBeData.Click += new System.EventHandler(this.btnRemoveBeData_Click);
            // 
            // listViewBeData
            // 
            this.listViewBeData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.From,
            this.To,
            this.OnlyCalc});
            this.listViewBeData.Location = new System.Drawing.Point(268, 46);
            this.listViewBeData.Name = "listViewBeData";
            this.listViewBeData.Size = new System.Drawing.Size(317, 136);
            this.listViewBeData.TabIndex = 7;
            this.listViewBeData.UseCompatibleStateImageBehavior = false;
            this.listViewBeData.View = System.Windows.Forms.View.Details;
            // 
            // From
            // 
            this.From.Text = "From";
            this.From.Width = 108;
            // 
            // To
            // 
            this.To.Text = "To";
            this.To.Width = 123;
            // 
            // OnlyCalc
            // 
            this.OnlyCalc.Text = "Only Calc Replace";
            this.OnlyCalc.Width = 77;
            // 
            // chkBeOnlyCalc
            // 
            this.chkBeOnlyCalc.AutoSize = true;
            this.chkBeOnlyCalc.Checked = true;
            this.chkBeOnlyCalc.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBeOnlyCalc.Location = new System.Drawing.Point(375, 13);
            this.chkBeOnlyCalc.Name = "chkBeOnlyCalc";
            this.chkBeOnlyCalc.Size = new System.Drawing.Size(117, 17);
            this.chkBeOnlyCalc.TabIndex = 5;
            this.chkBeOnlyCalc.Text = "Calc Members Only";
            this.chkBeOnlyCalc.UseVisualStyleBackColor = true;
            // 
            // btnAddBe
            // 
            this.btnAddBe.Location = new System.Drawing.Point(226, 46);
            this.btnAddBe.Name = "btnAddBe";
            this.btnAddBe.Size = new System.Drawing.Size(39, 23);
            this.btnAddBe.TabIndex = 3;
            this.btnAddBe.Text = ">>";
            this.btnAddBe.UseVisualStyleBackColor = true;
            this.btnAddBe.Click += new System.EventHandler(this.btnAddBe_Click);
            // 
            // txtBeReplace
            // 
            this.txtBeReplace.Location = new System.Drawing.Point(268, 13);
            this.txtBeReplace.Name = "txtBeReplace";
            this.txtBeReplace.Size = new System.Drawing.Size(100, 20);
            this.txtBeReplace.TabIndex = 1;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(193, 13);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(75, 13);
            this.label31.TabIndex = 2;
            this.label31.Text = "Replace With:";
            // 
            // cmbBeData
            // 
            this.cmbBeData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBeData.FormattingEnabled = true;
            this.cmbBeData.Location = new System.Drawing.Point(66, 13);
            this.cmbBeData.Name = "cmbBeData";
            this.cmbBeData.Size = new System.Drawing.Size(121, 21);
            this.cmbBeData.TabIndex = 0;
            this.cmbBeData.SelectedValueChanged += new System.EventHandler(this.cmbBeData_SelectedValueChanged);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(13, 13);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(47, 13);
            this.label30.TabIndex = 0;
            this.label30.Text = "BE Data";
            // 
            // tabCPAS
            // 
            this.tabCPAS.Controls.Add(this.txtTargetCPA);
            this.tabCPAS.Controls.Add(this.label3);
            this.tabCPAS.Controls.Add(this.txtAccountID);
            this.tabCPAS.Controls.Add(this.label2);
            this.tabCPAS.Controls.Add(this.chkAddMeasure);
            this.tabCPAS.Controls.Add(this.label1);
            this.tabCPAS.Controls.Add(this.txtTargetAcquisitionName);
            this.tabCPAS.Controls.Add(this.cmbCpaReplaceTo);
            this.tabCPAS.Controls.Add(this.btnClearCPA);
            this.tabCPAS.Controls.Add(this.btnRemoveCpa);
            this.tabCPAS.Controls.Add(this.livCPA);
            this.tabCPAS.Controls.Add(this.chkOnlyCalcCPA);
            this.tabCPAS.Controls.Add(this.btnAddCPA);
            this.tabCPAS.Controls.Add(this.label34);
            this.tabCPAS.Controls.Add(this.cmbCPA);
            this.tabCPAS.Controls.Add(this.label35);
            this.tabCPAS.Location = new System.Drawing.Point(4, 22);
            this.tabCPAS.Name = "tabCPAS";
            this.tabCPAS.Size = new System.Drawing.Size(781, 272);
            this.tabCPAS.TabIndex = 4;
            this.tabCPAS.Text = "Acquisition";
            this.tabCPAS.UseVisualStyleBackColor = true;
            // 
            // txtTargetCPA
            // 
            this.txtTargetCPA.Location = new System.Drawing.Point(605, 56);
            this.txtTargetCPA.Name = "txtTargetCPA";
            this.txtTargetCPA.Size = new System.Drawing.Size(100, 20);
            this.txtTargetCPA.TabIndex = 24;
            this.txtTargetCPA.TextChanged += new System.EventHandler(this.txtTargetCPA_TextChanged);
            this.txtTargetCPA.Validated += new System.EventHandler(this.txtTargetCPA_Validated);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(557, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "Target:";
            // 
            // txtAccountID
            // 
            this.txtAccountID.Enabled = false;
            this.txtAccountID.Location = new System.Drawing.Point(92, 52);
            this.txtAccountID.Name = "txtAccountID";
            this.txtAccountID.Size = new System.Drawing.Size(121, 20);
            this.txtAccountID.TabIndex = 22;
            this.toolTip1.SetToolTip(this.txtAccountID, "Identity of account for adding measures and target cpa.\r\nmust be set when add mea" +
                    "sure check box is checed or target \r\ncpa has been set");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Account ID:";
            // 
            // chkAddMeasure
            // 
            this.chkAddMeasure.AutoSize = true;
            this.chkAddMeasure.Enabled = false;
            this.chkAddMeasure.Location = new System.Drawing.Point(12, 94);
            this.chkAddMeasure.Name = "chkAddMeasure";
            this.chkAddMeasure.Size = new System.Drawing.Size(89, 17);
            this.chkAddMeasure.TabIndex = 20;
            this.chkAddMeasure.Text = "Add Measure";
            this.chkAddMeasure.UseVisualStyleBackColor = true;
            this.chkAddMeasure.CheckedChanged += new System.EventHandler(this.chkAddMeasure_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(232, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Target Acquisition Name:";
            // 
            // txtTargetAcquisitionName
            // 
            this.txtTargetAcquisitionName.Location = new System.Drawing.Point(358, 54);
            this.txtTargetAcquisitionName.Name = "txtTargetAcquisitionName";
            this.txtTargetAcquisitionName.Size = new System.Drawing.Size(178, 20);
            this.txtTargetAcquisitionName.TabIndex = 18;
            // 
            // cmbCpaReplaceTo
            // 
            this.cmbCpaReplaceTo.FormattingEnabled = true;
            this.cmbCpaReplaceTo.Location = new System.Drawing.Point(358, 17);
            this.cmbCpaReplaceTo.Name = "cmbCpaReplaceTo";
            this.cmbCpaReplaceTo.Size = new System.Drawing.Size(178, 21);
            this.cmbCpaReplaceTo.TabIndex = 1;
            this.cmbCpaReplaceTo.SelectedIndexChanged += new System.EventHandler(this.cmbCpaReplaceTo_SelectedIndexChanged);
            this.cmbCpaReplaceTo.Validated += new System.EventHandler(this.cmbCpaReplaceTo_Validated);
            // 
            // btnClearCPA
            // 
            this.btnClearCPA.Location = new System.Drawing.Point(221, 176);
            this.btnClearCPA.Name = "btnClearCPA";
            this.btnClearCPA.Size = new System.Drawing.Size(39, 23);
            this.btnClearCPA.TabIndex = 5;
            this.btnClearCPA.Text = "Clear";
            this.btnClearCPA.UseVisualStyleBackColor = true;
            this.btnClearCPA.Click += new System.EventHandler(this.btnClearCPA_Click);
            // 
            // btnRemoveCpa
            // 
            this.btnRemoveCpa.Location = new System.Drawing.Point(221, 123);
            this.btnRemoveCpa.Name = "btnRemoveCpa";
            this.btnRemoveCpa.Size = new System.Drawing.Size(39, 23);
            this.btnRemoveCpa.TabIndex = 4;
            this.btnRemoveCpa.Text = "<<";
            this.btnRemoveCpa.UseVisualStyleBackColor = true;
            this.btnRemoveCpa.Click += new System.EventHandler(this.btnRemoveCpa_Click);
            // 
            // livCPA
            // 
            this.livCPA.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader7,
            this.columnHeader8});
            this.livCPA.Location = new System.Drawing.Point(266, 94);
            this.livCPA.Name = "livCPA";
            this.livCPA.Size = new System.Drawing.Size(439, 105);
            this.livCPA.TabIndex = 17;
            this.livCPA.UseCompatibleStateImageBehavior = false;
            this.livCPA.View = System.Windows.Forms.View.Details;
            this.livCPA.Validating += new System.ComponentModel.CancelEventHandler(this.livCPA_Validating);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "From";
            this.columnHeader1.Width = 105;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "To";
            this.columnHeader2.Width = 83;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Only Calc Replace";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Target Acquisition Name";
            this.columnHeader7.Width = 130;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Target";
            // 
            // chkOnlyCalcCPA
            // 
            this.chkOnlyCalcCPA.AutoSize = true;
            this.chkOnlyCalcCPA.Location = new System.Drawing.Point(557, 19);
            this.chkOnlyCalcCPA.Name = "chkOnlyCalcCPA";
            this.chkOnlyCalcCPA.Size = new System.Drawing.Size(117, 17);
            this.chkOnlyCalcCPA.TabIndex = 3;
            this.chkOnlyCalcCPA.Text = "Calc Members Only";
            this.chkOnlyCalcCPA.UseVisualStyleBackColor = true;
            // 
            // btnAddCPA
            // 
            this.btnAddCPA.Location = new System.Drawing.Point(221, 94);
            this.btnAddCPA.Name = "btnAddCPA";
            this.btnAddCPA.Size = new System.Drawing.Size(39, 23);
            this.btnAddCPA.TabIndex = 14;
            this.btnAddCPA.Text = ">>";
            this.btnAddCPA.UseVisualStyleBackColor = true;
            this.btnAddCPA.Click += new System.EventHandler(this.btnAddCPA_Click);
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(234, 20);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(75, 13);
            this.label34.TabIndex = 12;
            this.label34.Text = "Replace With:";
            // 
            // cmbCPA
            // 
            this.cmbCPA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCPA.FormattingEnabled = true;
            this.cmbCPA.Location = new System.Drawing.Point(92, 17);
            this.cmbCPA.Name = "cmbCPA";
            this.cmbCPA.Size = new System.Drawing.Size(121, 21);
            this.cmbCPA.TabIndex = 0;
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(5, 15);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(87, 13);
            this.label35.TabIndex = 10;
            this.label35.Text = "Acquisition Data:";
            // 
            // tabStringReplace
            // 
            this.tabStringReplace.Controls.Add(this.txtString);
            this.tabStringReplace.Controls.Add(this.btnClearStrings);
            this.tabStringReplace.Controls.Add(this.btnRemoveStringReplace);
            this.tabStringReplace.Controls.Add(this.livStrings);
            this.tabStringReplace.Controls.Add(this.chkCalcOnlyString);
            this.tabStringReplace.Controls.Add(this.btnAddStringReplace);
            this.tabStringReplace.Controls.Add(this.txtStringReplace);
            this.tabStringReplace.Controls.Add(this.label36);
            this.tabStringReplace.Controls.Add(this.label37);
            this.tabStringReplace.Location = new System.Drawing.Point(4, 22);
            this.tabStringReplace.Name = "tabStringReplace";
            this.tabStringReplace.Size = new System.Drawing.Size(781, 272);
            this.tabStringReplace.TabIndex = 2;
            this.tabStringReplace.Text = "String Replace";
            this.tabStringReplace.UseVisualStyleBackColor = true;
            // 
            // txtString
            // 
            this.txtString.Location = new System.Drawing.Point(57, 14);
            this.txtString.Name = "txtString";
            this.txtString.Size = new System.Drawing.Size(100, 20);
            this.txtString.TabIndex = 0;
            // 
            // btnClearStrings
            // 
            this.btnClearStrings.Location = new System.Drawing.Point(185, 161);
            this.btnClearStrings.Name = "btnClearStrings";
            this.btnClearStrings.Size = new System.Drawing.Size(39, 23);
            this.btnClearStrings.TabIndex = 5;
            this.btnClearStrings.Text = "Clear";
            this.btnClearStrings.UseVisualStyleBackColor = true;
            this.btnClearStrings.Click += new System.EventHandler(this.btnClearStrings_Click);
            // 
            // btnRemoveStringReplace
            // 
            this.btnRemoveStringReplace.Location = new System.Drawing.Point(185, 105);
            this.btnRemoveStringReplace.Name = "btnRemoveStringReplace";
            this.btnRemoveStringReplace.Size = new System.Drawing.Size(39, 23);
            this.btnRemoveStringReplace.TabIndex = 4;
            this.btnRemoveStringReplace.Text = "<<";
            this.btnRemoveStringReplace.UseVisualStyleBackColor = true;
            this.btnRemoveStringReplace.Click += new System.EventHandler(this.btnRemoveStringReplace_Click);
            // 
            // livStrings
            // 
            this.livStrings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.livStrings.Location = new System.Drawing.Point(230, 49);
            this.livStrings.Name = "livStrings";
            this.livStrings.Size = new System.Drawing.Size(333, 136);
            this.livStrings.TabIndex = 27;
            this.livStrings.UseCompatibleStateImageBehavior = false;
            this.livStrings.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "From";
            this.columnHeader4.Width = 120;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "To";
            this.columnHeader5.Width = 129;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Only Calc Replace";
            // 
            // chkCalcOnlyString
            // 
            this.chkCalcOnlyString.AutoSize = true;
            this.chkCalcOnlyString.Checked = true;
            this.chkCalcOnlyString.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCalcOnlyString.Enabled = false;
            this.chkCalcOnlyString.Location = new System.Drawing.Point(370, 17);
            this.chkCalcOnlyString.Name = "chkCalcOnlyString";
            this.chkCalcOnlyString.Size = new System.Drawing.Size(117, 17);
            this.chkCalcOnlyString.TabIndex = 2;
            this.chkCalcOnlyString.Text = "Calc Members Only";
            this.chkCalcOnlyString.UseVisualStyleBackColor = true;
            // 
            // btnAddStringReplace
            // 
            this.btnAddStringReplace.Location = new System.Drawing.Point(185, 49);
            this.btnAddStringReplace.Name = "btnAddStringReplace";
            this.btnAddStringReplace.Size = new System.Drawing.Size(39, 23);
            this.btnAddStringReplace.TabIndex = 3;
            this.btnAddStringReplace.Text = ">>";
            this.btnAddStringReplace.UseVisualStyleBackColor = true;
            this.btnAddStringReplace.Click += new System.EventHandler(this.btnAddStringReplace_Click);
            // 
            // txtStringReplace
            // 
            this.txtStringReplace.Location = new System.Drawing.Point(263, 14);
            this.txtStringReplace.Name = "txtStringReplace";
            this.txtStringReplace.Size = new System.Drawing.Size(100, 20);
            this.txtStringReplace.TabIndex = 1;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(182, 14);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(75, 13);
            this.label36.TabIndex = 22;
            this.label36.Text = "Replace With:";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(10, 14);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(33, 13);
            this.label37.TabIndex = 20;
            this.label37.Text = "From:";
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Used For:";
            // 
            // CreateNewCube
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.tabsCreateCube);
            this.Name = "CreateNewCube";
            this.StepDescription = "Create new cube";
            this.StepName = "CreateNewCubeCollector";
            this.Load += new System.EventHandler(this.CreateNewCube_Load);
            this.VisibleChanged += new System.EventHandler(this.CreateNewCube_VisibleChanged);
            this.Controls.SetChildIndex(this.tabsCreateCube, 0);
            this.tabsCreateCube.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabMeasures.ResumeLayout(false);
            this.tabMeasures.PerformLayout();
            this.tabCPAS.ResumeLayout(false);
            this.tabCPAS.PerformLayout();
            this.tabStringReplace.ResumeLayout(false);
            this.tabStringReplace.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        

		#endregion

		private System.Windows.Forms.TabControl tabsCreateCube;
		private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.CheckBox chkProcessCubes;
        private System.Windows.Forms.CheckBox chkContent;
		private System.Windows.Forms.TabPage tabCPAS;
		private System.Windows.Forms.ComboBox cmbCpaReplaceTo;
		private System.Windows.Forms.Button btnClearCPA;
		private System.Windows.Forms.Button btnRemoveCpa;
		private System.Windows.Forms.ListView livCPA;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.CheckBox chkOnlyCalcCPA;
		private System.Windows.Forms.Button btnAddCPA;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.ComboBox cmbCPA;
		private System.Windows.Forms.Label label35;
		private System.Windows.Forms.TabPage tabStringReplace;
		private System.Windows.Forms.TextBox txtString;
		private System.Windows.Forms.Button btnClearStrings;
		private System.Windows.Forms.Button btnRemoveStringReplace;
		private System.Windows.Forms.ListView livStrings;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.CheckBox chkCalcOnlyString;
		private System.Windows.Forms.Button btnAddStringReplace;
		private System.Windows.Forms.TextBox txtStringReplace;
		private System.Windows.Forms.Label label36;
		private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTargetAcquisitionName;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.TabPage tabMeasures;
        private System.Windows.Forms.Button btnClearBeData;
        private System.Windows.Forms.Button btnRemoveBeData;
        private System.Windows.Forms.ListView listViewBeData;
        private System.Windows.Forms.ColumnHeader From;
        private System.Windows.Forms.ColumnHeader To;
        private System.Windows.Forms.ColumnHeader OnlyCalc;
        private System.Windows.Forms.CheckBox chkBeOnlyCalc;
        private System.Windows.Forms.Button btnAddBe;
        private System.Windows.Forms.TextBox txtBeReplace;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.ComboBox cmbBeData;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.TextBox txtAccountID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkAddMeasure;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtTargetCPA;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ColumnHeader columnHeader8;
	}
}
