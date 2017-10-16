namespace SmartStoreNetWebApiClient
{
	partial class MainForm
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.callApi = new System.Windows.Forms.Button();
            this.cboPath = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cboMethod = new System.Windows.Forms.ComboBox();
            this.txtResponse = new System.Windows.Forms.RichTextBox();
            this.clear = new System.Windows.Forms.Button();
            this.txtSecretKey = new System.Windows.Forms.TextBox();
            this.txtPublicKey = new System.Windows.Forms.TextBox();
            this.cboContent = new System.Windows.Forms.ComboBox();
            this.btnDeletePath = new System.Windows.Forms.Button();
            this.btnDeleteContent = new System.Windows.Forms.Button();
            this.txtRequest = new System.Windows.Forms.RichTextBox();
            this.radioJson = new System.Windows.Forms.RadioButton();
            this.radioXml = new System.Windows.Forms.RadioButton();
            this.btnDeleteQuery = new System.Windows.Forms.Button();
            this.cboQuery = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.radioOdata = new System.Windows.Forms.RadioButton();
            this.radioApi = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblRequest = new System.Windows.Forms.TextBox();
            this.lblResponse = new System.Windows.Forms.TextBox();
            this.lblFile = new System.Windows.Forms.Label();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.txtVersion = new System.Windows.Forms.TextBox();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.btnFileOpen = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.txtIdentfier1 = new System.Windows.Forms.TextBox();
            this.lblIdentifier1 = new System.Windows.Forms.Label();
            this.txtIdentfier2 = new System.Windows.Forms.TextBox();
            this.lblIdentfier2 = new System.Windows.Forms.Label();
            this.txtPictureId = new System.Windows.Forms.TextBox();
            this.lblPictureId = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(516, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Public-Key";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Secret-Key";
            // 
            // callApi
            // 
            this.callApi.AutoSize = true;
            this.callApi.Location = new System.Drawing.Point(17, 784);
            this.callApi.Name = "callApi";
            this.callApi.Size = new System.Drawing.Size(75, 23);
            this.callApi.TabIndex = 8;
            this.callApi.Text = "Call API";
            this.callApi.UseVisualStyleBackColor = true;
            this.callApi.Click += new System.EventHandler(this.callApi_Click);
            // 
            // cboPath
            // 
            this.cboPath.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboPath.FormattingEnabled = true;
            this.cboPath.Location = new System.Drawing.Point(551, 9);
            this.cboPath.Name = "cboPath";
            this.cboPath.Size = new System.Drawing.Size(571, 22);
            this.cboPath.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Store URL";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 98);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "JSON Body";
            // 
            // cboMethod
            // 
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.FormattingEnabled = true;
            this.cboMethod.Items.AddRange(new object[] {
            "GET",
            "POST",
            "PUT",
            "PATCH",
            "DELETE"});
            this.cboMethod.Location = new System.Drawing.Point(370, 9);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(89, 21);
            this.cboMethod.TabIndex = 3;
            this.cboMethod.SelectionChangeCommitted += new System.EventHandler(this.cboMethod_changeCommitted);
            // 
            // txtResponse
            // 
            this.txtResponse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtResponse.DetectUrls = false;
            this.txtResponse.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtResponse.HideSelection = false;
            this.txtResponse.Location = new System.Drawing.Point(18, 376);
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.ReadOnly = true;
            this.txtResponse.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtResponse.Size = new System.Drawing.Size(1126, 405);
            this.txtResponse.TabIndex = 10;
            this.txtResponse.Text = "";
            // 
            // clear
            // 
            this.clear.AutoSize = true;
            this.clear.Location = new System.Drawing.Point(1074, 784);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(70, 23);
            this.clear.TabIndex = 9;
            this.clear.Text = "Clear";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.clear_Click);
            // 
            // txtSecretKey
            // 
            this.txtSecretKey.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSecretKey.Location = new System.Drawing.Point(84, 36);
            this.txtSecretKey.Name = "txtSecretKey";
            this.txtSecretKey.Size = new System.Drawing.Size(225, 20);
            this.txtSecretKey.TabIndex = 1;
            // 
            // txtPublicKey
            // 
            this.txtPublicKey.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPublicKey.Location = new System.Drawing.Point(84, 10);
            this.txtPublicKey.Name = "txtPublicKey";
            this.txtPublicKey.Size = new System.Drawing.Size(225, 20);
            this.txtPublicKey.TabIndex = 0;
            // 
            // cboContent
            // 
            this.cboContent.FormattingEnabled = true;
            this.cboContent.Location = new System.Drawing.Point(84, 95);
            this.cboContent.Name = "cboContent";
            this.cboContent.Size = new System.Drawing.Size(1038, 21);
            this.cboContent.TabIndex = 7;
            // 
            // btnDeletePath
            // 
            this.btnDeletePath.AutoSize = true;
            this.btnDeletePath.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeletePath.Location = new System.Drawing.Point(1124, 9);
            this.btnDeletePath.Name = "btnDeletePath";
            this.btnDeletePath.Size = new System.Drawing.Size(20, 22);
            this.btnDeletePath.TabIndex = 7;
            this.btnDeletePath.Text = "x";
            this.btnDeletePath.UseVisualStyleBackColor = true;
            this.btnDeletePath.Click += new System.EventHandler(this.btnDeletePath_Click);
            // 
            // btnDeleteContent
            // 
            this.btnDeleteContent.AutoSize = true;
            this.btnDeleteContent.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteContent.Location = new System.Drawing.Point(1124, 95);
            this.btnDeleteContent.Name = "btnDeleteContent";
            this.btnDeleteContent.Size = new System.Drawing.Size(20, 22);
            this.btnDeleteContent.TabIndex = 9;
            this.btnDeleteContent.Text = "x";
            this.btnDeleteContent.UseVisualStyleBackColor = true;
            this.btnDeleteContent.Click += new System.EventHandler(this.btnDeleteContent_Click);
            // 
            // txtRequest
            // 
            this.txtRequest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRequest.DetectUrls = false;
            this.txtRequest.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRequest.HideSelection = false;
            this.txtRequest.Location = new System.Drawing.Point(18, 165);
            this.txtRequest.Name = "txtRequest";
            this.txtRequest.ReadOnly = true;
            this.txtRequest.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtRequest.Size = new System.Drawing.Size(1126, 187);
            this.txtRequest.TabIndex = 20;
            this.txtRequest.Text = "";
            // 
            // radioJson
            // 
            this.radioJson.AutoSize = true;
            this.radioJson.Location = new System.Drawing.Point(30, 3);
            this.radioJson.Name = "radioJson";
            this.radioJson.Size = new System.Drawing.Size(51, 17);
            this.radioJson.TabIndex = 0;
            this.radioJson.TabStop = true;
            this.radioJson.Text = "JSON";
            this.radioJson.UseVisualStyleBackColor = true;
            // 
            // radioXml
            // 
            this.radioXml.AutoSize = true;
            this.radioXml.Location = new System.Drawing.Point(89, 3);
            this.radioXml.Name = "radioXml";
            this.radioXml.Size = new System.Drawing.Size(44, 17);
            this.radioXml.TabIndex = 1;
            this.radioXml.TabStop = true;
            this.radioXml.Text = "XML";
            this.radioXml.UseVisualStyleBackColor = true;
            // 
            // btnDeleteQuery
            // 
            this.btnDeleteQuery.AutoSize = true;
            this.btnDeleteQuery.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteQuery.Location = new System.Drawing.Point(1124, 35);
            this.btnDeleteQuery.Name = "btnDeleteQuery";
            this.btnDeleteQuery.Size = new System.Drawing.Size(20, 22);
            this.btnDeleteQuery.TabIndex = 8;
            this.btnDeleteQuery.Text = "x";
            this.btnDeleteQuery.UseVisualStyleBackColor = true;
            this.btnDeleteQuery.Click += new System.EventHandler(this.btnDeleteQuery_Click);
            // 
            // cboQuery
            // 
            this.cboQuery.FormattingEnabled = true;
            this.cboQuery.Location = new System.Drawing.Point(551, 35);
            this.cboQuery.Name = "cboQuery";
            this.cboQuery.Size = new System.Drawing.Size(571, 21);
            this.cboQuery.TabIndex = 6;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(510, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 13);
            this.label8.TabIndex = 24;
            this.label8.Text = "Query";
            // 
            // radioOdata
            // 
            this.radioOdata.AutoSize = true;
            this.radioOdata.Location = new System.Drawing.Point(3, 3);
            this.radioOdata.Margin = new System.Windows.Forms.Padding(0);
            this.radioOdata.Name = "radioOdata";
            this.radioOdata.Size = new System.Drawing.Size(53, 17);
            this.radioOdata.TabIndex = 0;
            this.radioOdata.TabStop = true;
            this.radioOdata.Text = "odata";
            this.radioOdata.UseVisualStyleBackColor = true;
            this.radioOdata.Click += new System.EventHandler(this.odata_Click);
            // 
            // radioApi
            // 
            this.radioApi.AutoSize = true;
            this.radioApi.Location = new System.Drawing.Point(58, 3);
            this.radioApi.Margin = new System.Windows.Forms.Padding(0);
            this.radioApi.Name = "radioApi";
            this.radioApi.Size = new System.Drawing.Size(39, 17);
            this.radioApi.TabIndex = 1;
            this.radioApi.TabStop = true;
            this.radioApi.Text = "api";
            this.radioApi.UseVisualStyleBackColor = true;
            this.radioApi.CheckedChanged += new System.EventHandler(this.radioApi_CheckedChanged);
            this.radioApi.Click += new System.EventHandler(this.api_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioApi);
            this.panel1.Controls.Add(this.radioOdata);
            this.panel1.Location = new System.Drawing.Point(367, 62);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(103, 23);
            this.panel1.TabIndex = 25;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.radioXml);
            this.panel2.Controls.Add(this.radioJson);
            this.panel2.Location = new System.Drawing.Point(1013, 353);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(135, 23);
            this.panel2.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(323, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "Method";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(323, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 13);
            this.label7.TabIndex = 29;
            this.label7.Text = "Version";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(324, 67);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(42, 13);
            this.label9.TabIndex = 30;
            this.label9.Text = "Service";
            // 
            // lblRequest
            // 
            this.lblRequest.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblRequest.Location = new System.Drawing.Point(19, 150);
            this.lblRequest.Name = "lblRequest";
            this.lblRequest.ReadOnly = true;
            this.lblRequest.Size = new System.Drawing.Size(1123, 14);
            this.lblRequest.TabIndex = 31;
            this.lblRequest.Text = "Request";
            // 
            // lblResponse
            // 
            this.lblResponse.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblResponse.Location = new System.Drawing.Point(19, 361);
            this.lblResponse.Name = "lblResponse";
            this.lblResponse.ReadOnly = true;
            this.lblResponse.Size = new System.Drawing.Size(988, 14);
            this.lblResponse.TabIndex = 32;
            this.lblResponse.Text = "Response";
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(23, 125);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(58, 13);
            this.lblFile.TabIndex = 34;
            this.lblFile.Text = "File upload";
            // 
            // txtFile
            // 
            this.txtFile.Location = new System.Drawing.Point(84, 122);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(423, 21);
            this.txtFile.TabIndex = 33;
            // 
            // txtVersion
            // 
            this.txtVersion.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::SmartStoreNetWebApiClient.Properties.Settings.Default, "ApiVersion", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtVersion.Location = new System.Drawing.Point(370, 36);
            this.txtVersion.Name = "txtVersion";
            this.txtVersion.Size = new System.Drawing.Size(89, 21);
            this.txtVersion.TabIndex = 4;
            this.txtVersion.Text = global::SmartStoreNetWebApiClient.Properties.Settings.Default.ApiVersion;
            // 
            // txtUrl
            // 
            this.txtUrl.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::SmartStoreNetWebApiClient.Properties.Settings.Default, "ApiUrl", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtUrl.Location = new System.Drawing.Point(84, 62);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(225, 21);
            this.txtUrl.TabIndex = 2;
            this.txtUrl.Text = global::SmartStoreNetWebApiClient.Properties.Settings.Default.ApiUrl;
            // 
            // btnFileOpen
            // 
            this.btnFileOpen.AutoSize = true;
            this.btnFileOpen.Location = new System.Drawing.Point(510, 120);
            this.btnFileOpen.Name = "btnFileOpen";
            this.btnFileOpen.Size = new System.Drawing.Size(43, 24);
            this.btnFileOpen.TabIndex = 35;
            this.btnFileOpen.Text = "Open";
            this.btnFileOpen.UseVisualStyleBackColor = true;
            this.btnFileOpen.Click += new System.EventHandler(this.btnFileOpen_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // txtIdentfier1
            // 
            this.txtIdentfier1.Location = new System.Drawing.Point(595, 122);
            this.txtIdentfier1.Name = "txtIdentfier1";
            this.txtIdentfier1.Size = new System.Drawing.Size(61, 21);
            this.txtIdentfier1.TabIndex = 36;
            // 
            // lblIdentifier1
            // 
            this.lblIdentifier1.AutoSize = true;
            this.lblIdentifier1.Location = new System.Drawing.Point(571, 125);
            this.lblIdentifier1.Name = "lblIdentifier1";
            this.lblIdentifier1.Size = new System.Drawing.Size(18, 13);
            this.lblIdentifier1.TabIndex = 37;
            this.lblIdentifier1.Text = "ID";
            // 
            // txtIdentfier2
            // 
            this.txtIdentfier2.Location = new System.Drawing.Point(751, 123);
            this.txtIdentfier2.Name = "txtIdentfier2";
            this.txtIdentfier2.Size = new System.Drawing.Size(200, 21);
            this.txtIdentfier2.TabIndex = 38;
            // 
            // lblIdentfier2
            // 
            this.lblIdentfier2.AutoSize = true;
            this.lblIdentfier2.Location = new System.Drawing.Point(668, 126);
            this.lblIdentfier2.Name = "lblIdentfier2";
            this.lblIdentfier2.Size = new System.Drawing.Size(82, 13);
            this.lblIdentfier2.TabIndex = 39;
            this.lblIdentfier2.Text = "SKU, Name etc.";
            // 
            // txtPictureId
            // 
            this.txtPictureId.Location = new System.Drawing.Point(1045, 123);
            this.txtPictureId.Name = "txtPictureId";
            this.txtPictureId.Size = new System.Drawing.Size(61, 21);
            this.txtPictureId.TabIndex = 40;
            // 
            // lblPictureId
            // 
            this.lblPictureId.AutoSize = true;
            this.lblPictureId.Location = new System.Drawing.Point(989, 126);
            this.lblPictureId.Name = "lblPictureId";
            this.lblPictureId.Size = new System.Drawing.Size(50, 13);
            this.lblPictureId.TabIndex = 41;
            this.lblPictureId.Text = "PictureId";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1154, 812);
            this.Controls.Add(this.txtPictureId);
            this.Controls.Add(this.lblPictureId);
            this.Controls.Add(this.txtIdentfier2);
            this.Controls.Add(this.lblIdentfier2);
            this.Controls.Add(this.txtIdentfier1);
            this.Controls.Add(this.lblIdentifier1);
            this.Controls.Add(this.btnFileOpen);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.lblResponse);
            this.Controls.Add(this.lblRequest);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtVersion);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnDeleteQuery);
            this.Controls.Add(this.cboQuery);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtRequest);
            this.Controls.Add(this.btnDeleteContent);
            this.Controls.Add(this.btnDeletePath);
            this.Controls.Add(this.cboContent);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.txtResponse);
            this.Controls.Add(this.cboMethod);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboPath);
            this.Controls.Add(this.callApi);
            this.Controls.Add(this.txtSecretKey);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPublicKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1170, 850);
            this.MinimumSize = new System.Drawing.Size(1170, 850);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtSecretKey;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button callApi;
		private System.Windows.Forms.TextBox txtPublicKey;
		private System.Windows.Forms.ComboBox cboPath;
		private System.Windows.Forms.TextBox txtUrl;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cboMethod;
		private System.Windows.Forms.RichTextBox txtResponse;
		private System.Windows.Forms.Button clear;
		private System.Windows.Forms.ComboBox cboContent;
		private System.Windows.Forms.Button btnDeletePath;
		private System.Windows.Forms.Button btnDeleteContent;
		private System.Windows.Forms.RichTextBox txtRequest;
		private System.Windows.Forms.RadioButton radioJson;
		private System.Windows.Forms.RadioButton radioXml;
		private System.Windows.Forms.Button btnDeleteQuery;
		private System.Windows.Forms.ComboBox cboQuery;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.RadioButton radioOdata;
		private System.Windows.Forms.RadioButton radioApi;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtVersion;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox lblRequest;
		private System.Windows.Forms.TextBox lblResponse;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Label lblFile;
		private System.Windows.Forms.Button btnFileOpen;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.TextBox txtIdentfier1;
		private System.Windows.Forms.Label lblIdentifier1;
		private System.Windows.Forms.TextBox txtIdentfier2;
		private System.Windows.Forms.Label lblIdentfier2;
		private System.Windows.Forms.TextBox txtPictureId;
		private System.Windows.Forms.Label lblPictureId;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

