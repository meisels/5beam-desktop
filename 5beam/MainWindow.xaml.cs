using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WK.Libraries.BetterFolderBrowserNS;
using Path = System.IO.Path;

namespace _5beam {
	/// made by Zelo101
	/// version 1.0
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		static string directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		string fivebPath = Path.Combine(directory, "5b.swf");
		string levelsPath = Path.Combine(directory, "levels.txt");
		string levelfolderPath = Path.Combine(directory, "levels");
		string configPath = Path.Combine(directory, "config.zelo");
		string selectedlevel;

		Boolean fiveb = true;

		public void Refresh() {
			foreach (var file in Directory.GetFiles(levelfolderPath, "*.txt")) {
				Levelslist.Items.Add(Path.GetFileName(file));
			}
		}

		public MainWindow() {
			InitializeComponent();
			if (!(File.Exists(fivebPath))) {
				fiveb = false;
				MessageBox.Show("5beam cannot find the 5b swf. It should be in '" + fivebPath + "' You can get one at http://battlefordreamisland.com/5b/5b.swf");
			}
			if (!(File.Exists(levelsPath))) {
			    MessageBox.Show("levels.txt doesn't seem to exist! In order for the 5b swf to function, you'll need to SELECT a txt file from the listbox.");
			}
			if (!(File.Exists(configPath))) {
				File.WriteAllText(configPath, levelfolderPath);
			} else {
				levelfolderPath = File.ReadAllText(configPath);
			}
			System.IO.Directory.CreateDirectory(levelfolderPath);

			Refresh();
		}

		private void Start5b_Click(object sender, RoutedEventArgs e) {
			if (selectedlevel != null) {
				string levelbuffer = File.ReadAllText(Path.Combine(levelfolderPath, selectedlevel));
				File.WriteAllText(levelsPath,levelbuffer);
			}

			if (fiveb) {
				System.Diagnostics.Process.Start(fivebPath);
			} else {
				MessageBox.Show("The 5b.swf file needs to be in '" + directory + "' for this to work. If it is, restart this program.");
			}
		}

		private void Looklevels_Click(object sender, RoutedEventArgs e) {
			var betterFolderBrowser = new BetterFolderBrowser {
				Title = "Please select a folder.",
				RootFolder = directory
			};

			if (betterFolderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				string selectedFolder = betterFolderBrowser.SelectedFolder;
				levelfolderPath = selectedFolder;
				File.WriteAllText(configPath, selectedFolder);
				Levelslist.Items.Clear();
				Refresh();
			}
		}

		private void Levelslist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			selectedlevel = Levelslist.SelectedItem.ToString();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e) {
			Levelslist.Items.Clear();
			Refresh();
		}
	}
}
