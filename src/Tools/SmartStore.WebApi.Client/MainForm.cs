using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using SmartStore.WebApi.Client.Models;
using SmartStore.WebApi.Client.Properties;

namespace SmartStore.WebApi.Client
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
                txtProxyPort.Text = s.ApiProxyPort;
                txtVersion.Text = s.ApiVersion;
                cboPath.Items.FromString(s.ApiPaths);
                cboQuery.Items.FromString(s.ApiQuery);
                cboContent.Items.FromString(s.ApiContent);
                cboHeaders.Items.FromString(s.ApiHeaders);
                cboFileUpload.Items.FromString(s.FileUpload);

                if (cboPath.Items.Count <= 0)
                {
                    cboPath.Items.Add("/Customers");
                }

                if (cboHeaders.Items.Count <= 0)
                {
                    cboHeaders.Items.Add("{\"Prefer\":\"return=representation\"}");
                }

                if (cboFileUpload.Items.Count <= 0)
                {
                    var model = new FileUploadModel
                    {
                        Files = new List<FileUploadModel.FileModel>
                        {
                            new FileUploadModel.FileModel { LocalPath = @"C:\my-upload-picture.jpg" }
                        }
                    };
                    var serializedModel = JsonConvert.SerializeObject(model);
                    cboFileUpload.Items.Add(serializedModel);
                }

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
                s.ApiProxyPort = txtProxyPort.Text;
                s.ApiVersion = txtVersion.Text;
                Settings.Default[radioOdata.Checked ? "ApiPaths" : "ApiPaths2"] = cboPath.Items.IntoString();
                s.ApiQuery = cboQuery.Items.IntoString();
                s.ApiContent = cboContent.Items.IntoString();
                s.ApiHeaders = cboHeaders.Items.IntoString();
                s.FileUpload = cboFileUpload.Items.IntoString();

                s.Save();
            };
        }

        private void CallTheApi()
        {
            if (txtUrl.Text.HasValue() && !txtUrl.Text.EndsWith("/"))
            {
                txtUrl.Text += "/";
            }

            if (cboPath.Text.HasValue() && !cboPath.Text.StartsWith("/"))
            {
                cboPath.Text = "/" + cboPath.Text;
            }

            int.TryParse(txtProxyPort.Text, out var proxyPort);

            var context = new WebApiRequestContext
            {
                PublicKey = txtPublicKey.Text,
                SecretKey = txtSecretKey.Text,
                Url = txtUrl.Text + (radioOdata.Checked ? "odata/" : "api/") + txtVersion.Text + cboPath.Text,
                ProxyPort = proxyPort,
                HttpMethod = cboMethod.Text,
                HttpAcceptType = radioJson.Checked ? ApiConsumer.JsonAcceptType : ApiConsumer.XmlAcceptType,
                AdditionalHeaders = cboHeaders.Text
            };

            if (chkIEEE754Compatible.Checked)
            {
                context.HttpAcceptType += ";IEEE754Compatible=true";
            }

            if (cboQuery.Text.HasValue())
            {
                context.Url = string.Format("{0}?{1}", context.Url, cboQuery.Text);
            }

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

            // Create multipart form data.
            if (cboFileUpload.Text.HasValue())
            {
                try
                {
                    var fileUploadModel = JsonConvert.DeserializeObject(cboFileUpload.Text, typeof(FileUploadModel)) as FileUploadModel;
                    multiPartData = apiConsumer.CreateMultipartData(fileUploadModel);
                }
                catch
                {
                    cboFileUpload.RemoveCurrent();
                    cboFileUpload.Text = string.Empty;
                    "File upload data is invalid.".Box(MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
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
            cboHeaders.InsertRolled(cboHeaders.Text, 64);
            cboFileUpload.InsertRolled(cboFileUpload.Text, 64);
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
            var isBodySupported = ApiConsumer.BodySupported(cboMethod.Text);
            var isMultipartSupported = ApiConsumer.MultipartSupported(cboMethod.Text);

            cboContent.Enabled = isBodySupported;
            btnDeleteContent.Enabled = isBodySupported;

            cboFileUpload.Enabled = isMultipartSupported;
            btnDeleteFileUpload.Enabled = isMultipartSupported;
            btnFileOpen.Enabled = isMultipartSupported;
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

        private void btnDeleteHeaders_Click(object sender, EventArgs e)
        {
            cboHeaders.RemoveCurrent();
        }

        private void btnDeleteFileUpload_Click(object sender, EventArgs e)
        {
            cboFileUpload.RemoveCurrent();
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
            //var show = radioApi.Checked;
        }

        private void btnFileOpen_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK && openFileDialog1.FileNames.Any())
            {
                FileUploadModel model = null;

                // Deserialize current model.
                if (cboFileUpload.Text.HasValue())
                {
                    try
                    {
                        model = JsonConvert.DeserializeObject(cboFileUpload.Text, typeof(FileUploadModel)) as FileUploadModel;
                    }
                    catch
                    {
                        cboFileUpload.RemoveCurrent();
                        cboFileUpload.Text = string.Empty;
                    }
                }

                if (model == null)
                {
                    model = new FileUploadModel();
                }

                // Remove files that no longer exist.
                for (var i = model.Files.Count - 1; i >= 0; --i)
                {
                    if (!File.Exists(model.Files[i].LocalPath))
                    {
                        model.Files.RemoveAt(i);
                    }
                }

                // Add new selected files.
                foreach (var fileName in openFileDialog1.FileNames)
                {
                    if (!model.Files.Any(x => x.LocalPath != null && x.LocalPath == fileName))
                    {
                        model.Files.Add(new FileUploadModel.FileModel
                        {
                            LocalPath = fileName
                        });
                    }
                }

                cboFileUpload.Text = JsonConvert.SerializeObject(model);
            }
        }
    }
}
