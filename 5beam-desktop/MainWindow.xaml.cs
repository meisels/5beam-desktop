using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Path = System.IO.Path;

namespace _5beam
{
	/// made by Zelo101
	/// version 4.0
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// 
	/// </summary>
	/// 

	public class Level
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Author { get; set; }
	}
	public partial class MainWindow : Window
	{
		const string database = "https://api.arimeisels.com/5beam/api/";
		const string offlinemsg = "Refresh Failed. Either you, or the server is offline.";
		static string directory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "levels");
		string[] arguments = Environment.GetCommandLineArgs();
		string selectedlevel;
		string levelBeamed;
		Level[] levellist;

		public void Refresh()
		{
			Levelslist.Items.Clear();

			if (arguments.Length == 2)
            {
				if (!(arguments[1] == "fivebeam:\\\\" || arguments[1] == "fivebeam:%5C%5C" || arguments[1] == "fivebeam:\\" || arguments[1] == "fivebeam:%5C"))
                {
					levelBeamed = arguments[1].Replace("fivebeam:\\\\", "").Replace("fivebeam:%5C%5C", "").Replace("fivebeam:\\", "").Replace("fivebeam:%5C", "");
				}
			}

			var levelRequest = WebRequest.Create(database);

			Stream levelStream;
			try
			{
				levelStream = levelRequest.GetResponse().GetResponseStream();
			}
			catch (System.Net.WebException)
			{
				MessageBox.Show(offlinemsg);
				return;
			}

			if (levelStream != null)
			{
				using (var streamReader = new StreamReader(levelStream))
				{
					while (streamReader.Peek() > -1)
					{
						ParseStream(streamReader.ReadLine());
					}
				}

				if (levelBeamed != null)
				{
					if (!(levelBeamed == ""))
					{
						System.IO.Directory.CreateDirectory(Path.Combine(directory, levelBeamed));
						Thread downloadThread = new Thread(() =>
						{
							WebClient client = new WebClient();
							client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Client_DownloadFileCompleted);
							client.DownloadFileAsync(new Uri("https://api.arimeisels.com/5beam/download/" + levelBeamed), Path.Combine(directory, levelBeamed, "levels.txt"));
							using (WebClient webClient = new WebClient())
							{
								webClient.DownloadFile("http://battlefordreamisland.com/5b/5b.swf", Path.Combine(directory, levelBeamed, "5b.swf"));
							}
							Dispatcher.BeginInvoke((Action)delegate
							{
								System.Diagnostics.Process.Start(Path.Combine(directory, levelBeamed, "5b.swf"));
							});
						});
						downloadThread.Start();
					}
					levelBeamed = null;
				}
			}
			else
			{
				MessageBox.Show(offlinemsg);
			}
		}

		public void ParseStream(string jsonlevellist)
		{
			JavaScriptSerializer js = new JavaScriptSerializer();
			levellist = js.Deserialize<Level[]>(jsonlevellist);

			for (int i = 0; i < levellist.Length; i++)
			{
				Levelslist.Items.Add(
					levellist[i].Name + " (Uploaded By: " +
					levellist[i].Author + ")"
				);
			}

		}

		public MainWindow()
		{
			InitializeComponent();
			Refresh();
		}

		private void Start5b_Click(object sender, RoutedEventArgs e)
		{

			if (selectedlevel != null)
			{
				var id = levellist[Levelslist.SelectedIndex].Id;
				System.IO.Directory.CreateDirectory(Path.Combine(directory, id));
				Thread downloadThread = new Thread(() => {
					WebClient client = new WebClient();
					client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Client_DownloadFileCompleted);
					client.DownloadFileAsync(new Uri("https://api.arimeisels.com/5beam/download/" + id), Path.Combine(directory, id, "levels.txt"));
					using (WebClient webClient = new WebClient())
					{
						webClient.DownloadFile("http://battlefordreamisland.com/5b/5b.swf", Path.Combine(directory, id, "5b.swf"));
					}
					Dispatcher.BeginInvoke((Action)delegate {
						System.Diagnostics.Process.Start(Path.Combine(directory, id, "5b.swf"));
					});
				});
				downloadThread.Start();
			}
		}

		void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
		}

		private void Levelslist_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Levelslist.Items.Count != 0)
			{
				selectedlevel = Levelslist.SelectedItem.ToString();
				int sl_int = Levelslist.SelectedIndex;
				textBlockSelection.Text = "You have selected '" + levellist[sl_int].Name + "' by " + levellist[sl_int].Author + ".";
			}
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			Refresh();
		}

		private void UploadButton_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("https://5beam.5blevels.com/upload.html");
		}

		private void uploadButton_Click_1(object sender, RoutedEventArgs e)
		{

		}
	}
}