using System;
using System.Diagnostics;
using System.Windows.Forms;
using SmartStoreNetWebApiClient.Properties;
using SmartStore.Net.WebApi;
using SmartStoreNetWebApiClient.Misc;
using System.Web;
using System.Globalization;
using System.Text;

namespace SmartStoreNetWebApiClient
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();

			this.FormClosing += MainForm_Closing;

			this.Text = Program.AppName;

			cboMethod.SelectedIndex = 0;
			radioJson.Checked = true;
			radioOdata.Checked = true;
			txtPublicKey.Text = Settings.Default.ApiPublicKey;
			txtSecretKey.Text = Settings.Default.ApiSecretKey;
			cboPath.Items.FromString(Settings.Default.ApiPaths);
			cboQuery.Items.FromString(Settings.Default.ApiQuery);
			cboContent.Items.FromString(Settings.Default.ApiContent);

			if (cboPath.Items.Count <= 0)
				cboPath.Items.Add("/Customers");

			cboMethod_changeCommitted(null, null);
		}

		private void CallTheApi()
		{
			if (!string.IsNullOrWhiteSpace(txtUrl.Text) && !txtUrl.Text.EndsWith("/"))
				txtUrl.Text = txtUrl.Text + "/";

			if (!string.IsNullOrWhiteSpace(cboPath.Text) && !cboPath.Text.StartsWith("/"))
				cboPath.Text = "/" + cboPath.Text;

			var context = new WebApiRequestContext()
			{
				PublicKey = txtPublicKey.Text,
				SecretKey = txtSecretKey.Text,
				Url = txtUrl.Text + (radioOdata.Checked ? "odata/" : "api/") + txtVersion.Text + cboPath.Text,
				HttpMethod = cboMethod.Text,
				HttpAcceptType = (radioJson.Checked ? ApiConsumer.JsonAcceptType : ApiConsumer.XmlAcceptType)
			};

			if (!string.IsNullOrWhiteSpace(cboQuery.Text))
				context.Url = string.Format("{0}?{1}", context.Url, cboQuery.Text);

			if (!context.IsValid)
			{
				"Please enter Public-Key, Secret-Key, URL and method.".Box(MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				Debug.WriteLine(context.ToString());
				return;
			}

			var apiConsumer = new ApiConsumer();
			var requestContent = new StringBuilder();
			var response = new WebApiConsumerResponse();
			var sb = new StringBuilder();

			lblRequest.Text = "Request: " + context.HttpMethod + " " + context.Url;
			lblRequest.Refresh();

			var webRequest = apiConsumer.StartRequest(context, cboContent.Text, requestContent);

			txtRequest.Text = requestContent.ToString();

			bool result = apiConsumer.ProcessResponse(webRequest, response);

			lblResponse.Text = "Response: " + response.Status;

			sb.Append(response.Headers);

			if (result && radioJson.Checked && radioOdata.Checked)
			{
				var customers = apiConsumer.TryParseCustomers(response);

				if (customers != null)
				{
					sb.AppendLine(string.Format("Parsed {0} customer(s):", customers.Count));

					foreach (var customer in customers)
						sb.AppendLine(customer.ToString());

					sb.Append("\r\n");
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

		private void MainForm_Closing(object sender, FormClosingEventArgs e)
		{
			Settings.Default["ApiPublicKey"] = txtPublicKey.Text;
			Settings.Default["ApiSecretKey"] = txtSecretKey.Text;
			Settings.Default[radioOdata.Checked ? "ApiPaths" : "ApiPaths2"] = cboPath.Items.IntoString();
			Settings.Default["ApiQuery"] = cboQuery.Items.IntoString();
			Settings.Default["ApiContent"] = cboContent.Items.IntoString();
			Settings.Default.Save();
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
	}
}
