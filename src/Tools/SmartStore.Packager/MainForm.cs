using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;
using SmartStore.Packager.Properties;
using SmartStore.Utilities;

namespace SmartStore.Packager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            this.Load += (object sender, EventArgs e) =>
            {
                var s = Settings.Default;
                s.Reload();

                try
                {
                    if (s.RootPath.IsEmpty())
                    {
                        var rootPath = new DirectoryInfo(CommonHelper.MapPath("~/")).Parent.Parent.Parent;
                        s.RootPath = Path.Combine(rootPath.FullName, "build\\Web");
                    }

                    if (s.OutputPath.IsEmpty())
                    {
                        var rootPath = new DirectoryInfo(CommonHelper.MapPath("~/")).Parent.Parent.Parent;
                        s.OutputPath = Path.Combine(rootPath.FullName, "build\\Packages");
                    }
                }
                catch { }

                txtRootPath.Text = s.RootPath;
                txtOutputPath.Text = s.OutputPath;
            };

            this.FormClosing += (object sender, FormClosingEventArgs e) =>
            {
                Settings.Default.RootPath = txtRootPath.Text;
                Settings.Default.OutputPath = txtOutputPath.Text;

                Settings.Default.Save();
            };
        }

        private void btnCreatePackages_Click(object sender, EventArgs e)
        {
            if (!ValidatePaths())
                return;

            var rootPath = txtRootPath.Text;
            var outputPath = txtOutputPath.Text;

            var erroredIds = new List<string>();
            ExtensionInfo currentPackage = null;

            try
            {
                btnCreatePackages.Enabled = false;
                btnClose.Enabled = false;

                var creator = new PackageCreator(rootPath, outputPath);

                // create plugin packages
                foreach (var sel in lstPlugins.SelectedItems)
                {
                    currentPackage = (ExtensionInfo)sel;
                    var fi = creator.CreatePluginPackage(currentPackage.Path);
                    if (!LogMessage(fi, currentPackage.Name))
                    {
                        erroredIds.Add(currentPackage.Path);
                    }
                }

                // create theme packages
                foreach (var sel in lstThemes.SelectedItems)
                {
                    currentPackage = (ExtensionInfo)sel;
                    var fi = creator.CreateThemePackage(currentPackage.Path);
                    if (!LogMessage(fi, currentPackage.Name))
                    {
                        erroredIds.Add(currentPackage.Name);
                    }
                }

                var msg = string.Empty;

                if (erroredIds.Count == 0)
                {
                    msg = "Successfully created all {0} packages".FormatCurrent(lstPlugins.SelectedItems.Count + lstThemes.SelectedItems.Count);
                    MessageBox.Show(msg, "Packages created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    msg = "Successfully created {0} packages.\n\nUnable to create:\n\n".FormatCurrent(lstPlugins.SelectedItems.Count + lstThemes.SelectedItems.Count - erroredIds.Count);
                    erroredIds.Each(x => msg += x + "\n");
                    MessageBox.Show(msg, "Packages created with errors", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[{0}]: {1}".FormatCurrent(currentPackage, ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCreatePackages.Enabled = true;
                btnClose.Enabled = true;
            }

        }

        private bool LogMessage(FileInfo fi, string packageName)
        {
            string msg = string.Empty;
            if (fi != null)
            {
                msg = "Created package for '{0}'...".FormatCurrent(packageName);
            }
            else
            {
                msg = "Unable to create package for '{0}'...".FormatCurrent(packageName);
            }
            Debug.WriteLine(msg);

            return fi != null;
        }

        private void btnReadDescriptions_Click(object sender, EventArgs e)
        {
            if (!ValidatePaths())
                return;

            var rootPath = txtRootPath.Text;
            var vpp = new RootedVirtualPathProvider(rootPath);

            btnReadDescriptions.Enabled = false;

            ReadPackages(vpp);

            btnReadDescriptions.Enabled = true;
        }


        private void ReadPackages(IVirtualPathProvider vpp)
        {
            if (!ValidatePaths())
                return;

            lstPlugins.DisplayMember = "Name";
            lstPlugins.ValueMember = "Path";
            lstThemes.DisplayMember = "Name";
            lstThemes.ValueMember = "Path";

            lstPlugins.Items.Clear();
            lstThemes.Items.Clear();

            IEnumerable<string> dirs = Enumerable.Empty<string>();

            if (vpp.DirectoryExists("~/Plugins") || vpp.DirectoryExists("~/Themes"))
            {
                if (vpp.DirectoryExists("~/Plugins"))
                {
                    dirs = dirs.Concat(vpp.ListDirectories("~/Plugins"));
                }
                if (vpp.DirectoryExists("~/Themes"))
                {
                    dirs = dirs.Concat(vpp.ListDirectories("~/Themes"));
                }
            }
            else
            {
                dirs = vpp.ListDirectories("~/");
            }

            foreach (var dir in dirs)
            {
                bool isTheme = false;

                // is it a plugin?
                var filePath = vpp.Combine(dir, "Description.txt");
                if (!vpp.FileExists(filePath))
                {
                    // ...no! is it a theme?
                    filePath = vpp.Combine(dir, "theme.config");
                    if (!vpp.FileExists(filePath))
                        continue;

                    isTheme = true;
                }

                try
                {
                    if (isTheme)
                    {
                        var manifest = ThemeManifest.Create(vpp.MapPath(dir));
                        lstThemes.Items.Add(new ExtensionInfo(dir, manifest.ThemeName));
                    }
                    else
                    {
                        var descriptor = PluginFileParser.ParsePluginDescriptionFile(vpp.MapPath(filePath));
                        if (descriptor != null)
                        {
                            lstPlugins.Items.Add(new ExtensionInfo(dir, descriptor.FolderName));
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            if (lstPlugins.Items.Count > 0)
            {
                tabMain.SelectedIndex = 0;
            }
            else if (lstThemes.Items.Count > 0)
            {
                tabMain.SelectedIndex = 1;
            }
        }

        private bool ValidatePaths()
        {
            if (!Directory.Exists(txtRootPath.Text))
            {
                MessageBox.Show("Root path does not exist! Please specify an existing path", "Path error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtRootPath.Focus();
                return false;
            }

            return true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            btnReadDescriptions.Enabled = txtRootPath.Text.Length > 0;
            var selCount = lstPlugins.SelectedItems.Count + lstThemes.SelectedItems.Count;
            btnCreatePackages.Enabled = selCount > 0;
            if (selCount > 1)
            {
                btnCreatePackages.Text = "Create {0} packages".FormatCurrent(selCount);
            }
            else
            {
                btnCreatePackages.Text = "Create package";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnBrowseRootPath_Click(object sender, EventArgs e)
        {
            if (txtRootPath.Text.Length > 0)
                fb.SelectedPath = txtRootPath.Text;

            var result = fb.ShowDialog();
            txtRootPath.Text = fb.SelectedPath;
        }

        private void btnBrowseOutputPath_Click(object sender, EventArgs e)
        {
            if (txtOutputPath.Text.Length > 0)
                fb.SelectedPath = txtOutputPath.Text;

            var result = fb.ShowDialog();
            txtOutputPath.Text = fb.SelectedPath;
        }

    }
}
