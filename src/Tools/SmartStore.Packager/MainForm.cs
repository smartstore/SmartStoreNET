using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartStore.Core.Plugins;
using SmartStore.Core.IO.WebSite;
using SmartStore.Core.IO.VirtualPath;
using SmartStore.Core.Packaging;
using SmartStore.Core.Themes;
using System.Diagnostics;
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
			string currentPackage = string.Empty;

			try
			{
				btnCreatePackages.Enabled = false;
				btnClose.Enabled = false;
				
				var creator = new PackageCreator(rootPath, outputPath);

				// create plugin packages
				foreach (var sel in lstPlugins.SelectedItems)
				{
					currentPackage = (string)sel;
					var fi = creator.CreatePluginPackage((string)sel);
					if (!LogMessage(fi, (string)sel))
					{
						erroredIds.Add(currentPackage);
					}
				}

				// create theme packages
				foreach (var sel in lstThemes.SelectedItems)
				{
					currentPackage = (string)sel;
					var fi = creator.CreateThemePackage((string)sel);
					if (!LogMessage(fi, (string)sel))
					{
						erroredIds.Add(currentPackage);
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

			ReadPlugins(vpp);
			ReadThemes(vpp);

			btnReadDescriptions.Enabled = true;
		}

		private void ReadPlugins(IVirtualPathProvider vpp)
		{
			if (!ValidatePaths())
				return;

			lstPlugins.Items.Clear();
			foreach (var dir in vpp.ListDirectories("~/Plugins"))
			{
				var filePath = vpp.Combine(dir, "Description.txt");
				if (!vpp.FileExists(filePath))
					continue;

				try
				{
					var descriptor = PluginFileParser.ParsePluginDescriptionFile(vpp.MapPath(filePath));
					if (descriptor != null)
					{
						lstPlugins.Items.Add(descriptor.FolderName);
					}
				}
				catch
				{
					continue;
				}
			}
		}

		private void ReadThemes(IVirtualPathProvider vpp)
		{
			if (!ValidatePaths())
				return;

			lstThemes.Items.Clear();
			foreach (var dir in vpp.ListDirectories("~/Themes"))
			{
				var filePath = vpp.Combine(dir, "theme.config");
				if (!vpp.FileExists(filePath))
					continue;

				try
				{
					var manifest = ThemeManifest.Create(vpp.MapPath(dir));
					lstThemes.Items.Add(manifest.ThemeName);
				}
				catch
				{
					continue;
				}
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

			//if (!Directory.Exists(txtOutputPath.Text))
			//{
			//	MessageBox.Show("Output path does not exist! Please specify an existing path", "Path error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			//	txtOutputPath.Focus();
			//	return false;
			//}

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
