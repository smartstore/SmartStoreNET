using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using SmartStore.Net.WebApi;
using SmartStoreNetWebApiClient.Properties;

namespace SmartStoreNetWebApiClient
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			this.Text = Program.AppName;

			this.Load += (object sender, EventArgs e) =>
			{
				var s = Settings.Default;
				s.Reload();

				cboMethod.SelectedIndex = 0;
				radioJson.Checked = true;
				radioOdata.Checked = true;
				txtPublicKey.Text = s.ApiPublicKey;
				txtSecretKey.Text = s.ApiSecretKey;
				txtUrl.Text = s.ApiUrl;
				txtVersion.Text = s.ApiVersion;
				cboPath.Items.FromString(s.ApiPaths);
				cboQuery.Items.FromString(s.ApiQuery);
				cboContent.Items.FromString(s.ApiContent);

				if (cboPath.Items.Count <= 0)
					cboPath.Items.Add("/Customers");

				cboMethod_changeCommitted(null, null);
				radioApi_CheckedChanged(null, null);

				openFileDialog1.Filter = "Supported files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.csv, *.xlsx, *.txt, *.tab, *.zip) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.csv; *.xlsx; *.txt; *.tab; *.zip";
				openFileDialog1.DefaultExt = ".jpg";
				openFileDialog1.FileName = "";
				openFileDialog1.Title = "Please select files to upload";
				openFileDialog1.Multiselect = true;
			};

			this.FormClosing += (object sender, FormClosingEventArgs e) =>
			{
				var s = Settings.Default;

				s.ApiPublicKey = txtPublicKey.Text;
				s.ApiSecretKey = txtSecretKey.Text;
				s.ApiUrl = txtUrl.Text;
				s.ApiVersion = txtVersion.Text;
				Settings.Default[radioOdata.Checked ? "ApiPaths" : "ApiPaths2"] = cboPath.Items.IntoString();
				s.ApiQuery = cboQuery.Items.IntoString();
				s.ApiContent = cboContent.Items.IntoString();

				s.Save();
			};
		}

		private void CallTheApi()
		{
			if (txtUrl.Text.HasValue() && !txtUrl.Text.EndsWith("/"))
				txtUrl.Text = txtUrl.Text + "/";

			if (cboPath.Text.HasValue() && !cboPath.Text.StartsWith("/"))
				cboPath.Text = "/" + cboPath.Text;

			var context = new WebApiRequestContext
			{
				PublicKey = txtPublicKey.Text,
				SecretKey = txtSecretKey.Text,
				Url = txtUrl.Text + (radioOdata.Checked ? "odata/" : "api/") + txtVersion.Text + cboPath.Text,
				HttpMethod = cboMethod.Text,
				HttpAcceptType = (radioJson.Checked ? ApiConsumer.JsonAcceptType : ApiConsumer.XmlAcceptType)
			};

			if (cboQuery.Text.HasValue())
				context.Url = string.Format("{0}?{1}", context.Url, cboQuery.Text);

			if (!context.IsValid)
			{
				"Please enter Public-Key, Secret-Key, URL and method.".Box(MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				Debug.WriteLine(context.ToString());
				return;
			}

			var apiConsumer = new ApiConsumer();
			var response = new WebApiConsumerResponse();
			var sb = new StringBuilder();
			StringBuilder requestContent = null;
			Dictionary<string, object> multiPartData = null;

			lblRequest.Text = "Request: " + context.HttpMethod + " " + context.Url;
			lblRequest.Refresh();

			if (radioApi.Checked && txtFile.Text.HasValue())
			{
				if (string.Compare(context.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) != 0)
				{
					"Please select POST method for image upload.".Box(MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				var id1 = txtIdentfier1.Text.ToInt();
				var id2 = txtIdentfier2.Text;
				var pictureId = txtPictureId.Text.ToInt();
				var keyForId1 = "Id";
				var keyForId2 = "";

				multiPartData = new Dictionary<string, object>();

				if (cboPath.Text.StartsWith("/Uploads/ProductImages"))
				{
					// only one identifier required: product id, sku or gtin
					keyForId2 = "Sku";
				}
				else if (cboPath.Text.StartsWith("/Uploads/ImportFiles"))
				{
					// only one identifier required: import profile id or profile name
					keyForId2 = "Name";

					// to delete existing import files:
					//multiPartData.Add("deleteExisting", true);
				}

				if (id1 != 0)
					multiPartData.Add(keyForId1, id1);

				if (id2.HasValue())
					multiPartData.Add(keyForId2, id2);

				apiConsumer.AddApiFileParameter(multiPartData, txtFile.Text, pictureId);
			}

			var webRequest = apiConsumer.StartRequest(context, cboContent.Text, multiPartData, out requestContent);
			txtRequest.Text = requestContent.ToString();

			var result = apiConsumer.ProcessResponse(webRequest, response, folderBrowserDialog1);

			lblResponse.Text = "Response: " + response.Status;

			sb.Append(response.Headers);

			if (result && response.Content.HasValue())
			{
                if (radioJson.Checked && radioOdata.Checked)
                {
                    var customers = response.TryParseCustomers();

                    if (customers != null)
                    {
                        sb.AppendLine("Parsed {0} customer(s):".FormatInvariant(customers.Count));

                        customers.ForEach(x => sb.AppendLine(x.ToString()));

                        sb.Append("\r\n");
                    }
                }
			}

			sb.Append(response.Content);
			txtResponse.Text = sb.ToString();

			cboPath.InsertRolled(cboPath.Text, 64);
			cboQuery.InsertRolled(cboQuery.Text, 64);
			cboContent.InsertRolled(cboContent.Text, 64);
		}
		
		private void SavePathItems(bool odata)
		{
			Settings.Default[odata ? "ApiPaths2" : "ApiPaths"] = cboPath.Items.IntoString();
			Settings.Default.Save();

			cboPath.Text = "";
			cboPath.Items.Clear();
			cboPath.Items.FromString(odata ? Settings.Default.ApiPaths : Settings.Default.ApiPaths2);
		}
		
		private void MainForm_Shown(object sender, EventArgs e)
		{
			if (txtVersion.Text.Length == 0)
				txtVersion.Text = "v1";

			if (txtUrl.Text.Length == 0)
				txtUrl.Text = "http://www.my-store.com/";

			cboPath.Focus();
		}
		
		private void callApi_Click(object sender, EventArgs e)
		{
			clear_Click(null, null);

			using (new HourGlass())
			{
				CallTheApi();
			}
		}
		
		private void cboMethod_changeCommitted(object sender, EventArgs e)
		{
			bool enable = ApiConsumer.BodySupported(cboMethod.Text);
			cboContent.Enabled = enable;
		}
		
		private void btnDeletePath_Click(object sender, EventArgs e)
		{
			cboPath.RemoveCurrent();
		}
		
		private void btnDeleteQuery_Click(object sender, EventArgs e)
		{
			cboQuery.RemoveCurrent();
		}
		
		private void btnDeleteContent_Click(object sender, EventArgs e)
		{
			cboContent.RemoveCurrent();
		}
		
		private void clear_Click(object sender, EventArgs e)
		{
			txtRequest.Clear();
			lblRequest.Text = "Request";
			txtResponse.Clear();
			lblResponse.Text = "Response";
			
			txtRequest.Refresh();
			lblRequest.Refresh();
			txtResponse.Refresh();
			lblResponse.Refresh();
		}
		
		private void odata_Click(object sender, EventArgs e)
		{
			SavePathItems(true);
		}
		
		private void api_Click(object sender, EventArgs e)
		{
			SavePathItems(false);
		}

		private void radioApi_CheckedChanged(object sender, EventArgs e)
		{
			var show = radioApi.Checked;

			lblFile.Visible = show;
			txtFile.Visible = show;
			btnFileOpen.Visible = show;
			lblIdentifier1.Visible = show;
			txtIdentfier1.Visible = show;
			lblIdentfier2.Visible = show;
			txtIdentfier2.Visible = show;
			txtPictureId.Visible = show;
			lblPictureId.Visible = show;
		}

		private void btnFileOpen_Click(object sender, EventArgs e)
		{
			var result = openFileDialog1.ShowDialog();
			if (result == DialogResult.OK)
			{
				txtFile.Text = string.Join(";", openFileDialog1.FileNames);
			}
		}
	}
}
