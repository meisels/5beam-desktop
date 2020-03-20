using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using WK.Libraries.BetterFolderBrowserNS;
using Path = System.IO.Path;

namespace _5beam {
	/// made by Zelo101
	/// version 1.0
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// 
	/// </summary>
	/// 

	public class Level {
		public int Id { get; set; }
		public string Name { get; set; }
		public string Username { get; set; }
		public string Description { get; set; }
		public int Size { get; set; }
		public int Views { get; set; }
	}
	public partial class MainWindow : Window {
		const string database = "http://5beam.zapto.org/api/levellist";
		static string directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		string fivebPath = Path.Combine(directory, "5b.swf");
		string levelsPath = Path.Combine(directory, "levels.txt");
		string levelfolderPath = Path.Combine(directory, "levels");
		string configPath = Path.Combine(directory, "config.zelo");
		string selectedlevel;

		Boolean fiveb = true;

		public void Refresh() {
			Levelslist.Items.Clear();

			var levelRequest = WebRequest.Create(database);

			var levelStream = levelRequest.GetResponse().GetResponseStream();

			if (levelStream != null) {
				using (var streamReader = new StreamReader(levelStream)) {
					// Return next avaliable character or -1 if there are no
					// characters to be read.
					while (streamReader.Peek() > -1) {
						ParseStream(streamReader.ReadLine());
					}
				}
			} else {
				Levelslist.Items.Add("Refresh Failed. Check if you & the server is online.");
			}
			//Levelslist.Items.Add(levelStream);
			//Console.WriteLine(levelStream);

			//foreach (var file in Directory.GetFiles(levelfolderPath, "*.txt")) {
			//	Levelslist.Items.Add(Path.GetFileName(file));
			//}
		}

		public void ParseStream(string jsonlevellist) {
			JavaScriptSerializer js = new JavaScriptSerializer();
			Level[] levellist = js.Deserialize<Level[]>(jsonlevellist);

			for (int i = 0; i < levellist.Length; i++) {
				Levelslist.Items.Add(
					levellist[i].Name + " (Uploaded By: " +
					levellist[i].Username + ", Views: " +
					levellist[i].Views + ")"
				);
			}

		}

		public MainWindow() {
			InitializeComponent();
			if (!(File.Exists(fivebPath))) {
				fiveb = false;
				MessageBox.Show("5beam cannot find the 5b swf. It should be in '" + fivebPath + "' You can get one at http://battlefordreamisland.com/5b/5b.swf");
			}
			//if (!(File.Exists(levelsPath))) {
			//    MessageBox.Show("levels.txt doesn't seem to exist! In order for the 5b swf to function, you'll need to SELECT a txt file from the listbox.");
			//}
			if (!(File.Exists(configPath))) {
				File.WriteAllText(configPath, levelfolderPath);
			} else {
				levelfolderPath = File.ReadAllText(configPath);
			}
			System.IO.Directory.CreateDirectory(levelfolderPath);

			Refresh();
		}

		private void Start5b_Click(object sender, RoutedEventArgs e) {
			if (fiveb) {
				//System.Diagnostics.Process.Start(fivebPath);
			}
			else {
				MessageBox.Show("The 5b.swf file needs to be in '" + directory + "' for this to work. If it is, restart this program.");
				return;
			}

			if (selectedlevel != null) {
				Thread downloadThread = new Thread(() => {
					WebClient client = new WebClient();
					client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(client_DownloadFileCompleted);
					client.DownloadFileAsync(new Uri("http://5beam.zapto.org/dl/0"), levelsPath);
				});
				downloadThread.Start();
				//string levelbuffer = File.ReadAllText(Path.Combine(levelfolderPath, selectedlevel));
				//File.WriteAllText(levelsPath,levelbuffer);
			}

		}

		void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
			Dispatcher.BeginInvoke((Action) delegate {
				System.Diagnostics.Process.Start(fivebPath);
			});
		}

		/*private void Looklevels_Click(object sender, RoutedEventArgs e) {
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
		}*/

		private void Levelslist_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			selectedlevel = Levelslist.SelectedItem.ToString();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e) {
			Levelslist.Items.Clear();
			Refresh();
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

		}
	}
}
