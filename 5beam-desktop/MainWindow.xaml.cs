using System;
using System.Collections.Generic; /// This and all other unecessary things are either here from previous versions of 5beam, failed feature attempts, or for future features :D
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
	public class Level
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Author { get; set; }
		public string Views { get; set; }
		public string Mod { get; set; }
	}
	
	public class CheckVersion
	{
		public string Version { get; set; }
	}

	public partial class MainWindow : Window
	{
		const string database = "https://5beam.5blevels.com/api/";
		const string offlinemsg = "Refresh Failed. Either you, or the server is offline.";
		static string directory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "levels");
		string[] arguments = Environment.GetCommandLineArgs();
		string selectedlevel;
		string levelBeamed;
		Level[] levellist;

		public void getArgs()
		{
			if (arguments.Length == 2) /// Checks if there are arguments sent from your browser (the first is always the file path on all executables)
			{
				if (!(arguments[1] == "fivebeam:\\\\" || arguments[1] == "fivebeam:%5C%5C" || arguments[1] == "fivebeam:\\" || arguments[1] == "fivebeam:%5C")) /// Makes sure the argument isn't just to start the app (fivebeam:\\)
				{
					Hide();
					levelBeamed = arguments[1].Replace("fivebeam:\\\\", "").Replace("fivebeam:%5C%5C", "").Replace("fivebeam:\\", "").Replace("fivebeam:%5C", ""); /// Removes the protocol itself so you have only the id left (if you put in any letters or symbols then the app crashes)
				}
			}
		}

		public void CheckUpdate()
        {
			var levelRequest = WebRequest.Create("https://5beam.5blevels.com/version/");
			Stream levelStream;
			try
			{
				levelStream = levelRequest.GetResponse().GetResponseStream();
			}
			catch (WebException)
			{
				MessageBox.Show(offlinemsg);
				return;
			}
			using (var streamReader = new StreamReader(levelStream))
			{
				while (streamReader.Peek() > -1)
				{
					JavaScriptSerializer js = new JavaScriptSerializer();
					var current = js.Deserialize<CheckVersion[]>(streamReader.ReadLine());
					if (!(current[0].Version == "5-alpha.3"))
					{
						if (Convert.ToString(MessageBox.Show("Please update to version " + current[0].Version + " to continue using 5beam-Desktop.", "Update Required", (MessageBoxButton)System.Windows.Forms.MessageBoxButtons.OKCancel)) == "OK")
						{
							Process.Start("https://5beam.5blevels.com/player/");
						}
						Close();
					}
				}
			}
		}

		public void Refresh()
		{
			CheckUpdate();

			Levelslist.Items.Clear();

			/// Complicated stuff I can't explain, DM me (imaperson#1060) if you want to know what it does

			var levelRequest = WebRequest.Create(database);

			Stream levelStream;
			try
			{
				levelStream = levelRequest.GetResponse().GetResponseStream();
			}
			catch (WebException)
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

				if (levelBeamed != null) /// This following if statement is all my code so expect a lot of dumb things I could've used shorter (and better) code for
				{
					if (!(levelBeamed == "") && (levellist.Length >= Convert.ToInt32(levelBeamed)) && (Convert.ToInt32(levelBeamed) > 0)) /// Makes sure the beamed id is a. not a blank string (which should be impossible), b. listed in the api, and c. greater than 0
					{
						var id = levellist[Convert.ToInt32(levelBeamed) - 1].Id; /// Gets the level id (and crashes if there are illegal characters)
						Directory.CreateDirectory(Path.Combine(directory, id)); /// Creates a new directory for the levelpack (~10mb)
						Thread downloadThread = new Thread(() => /// Downloads the levelpack
						{
							WebClient client = new WebClient();
							client.DownloadFileCompleted += new AsyncCompletedEventHandler(Client_DownloadFileCompleted);
							client.DownloadFileAsync(new Uri("https://5beam.5blevels.com/download/" + id), Path.Combine(directory, id, "levels.txt"));
							using (WebClient webClient = new WebClient())
							{
								if (!File.Exists(Path.Combine(directory, id, "5b.exe"))) /// If you've never downloaded this levelpack before, it will download the 5b executable (levelpacks are redownloaded because they can be changed and they take almost no time)
								{
									MessageBox.Show("As this is the first time you are playing this levelpack, please wait up to 10 seconds for the files to download.");
									if (levellist[Convert.ToInt32(levelBeamed) - 1].Mod == "")
									{
										webClient.DownloadFile("https://5beam.5blevels.com/5b.exe", Path.Combine(directory, id, "5b.exe"));
									}
									else
                                    {
										webClient.DownloadFile("https://5beam.5blevels.com/mods/" + levellist[Convert.ToInt32(levelBeamed) - 1].Mod + ".exe", Path.Combine(directory, id, "5b.exe"));
                                    }
								}
							}
							Dispatcher.BeginInvoke((Action)delegate /// When the download completes 5b is started and the application is paused until 5b is closed. Normally in this case I would just close the application but the levels file won't finish downloading if I do.
							{
								Process game = Process.Start(Path.Combine(directory, id, "5b.exe"));
								game.WaitForExit();
								Close();
							});
						});
						downloadThread.Start(); /// Starts that entire thread... to be honest, I'm not sure why it couldn't have just run without the thread
					}
					else
                    {
						Close();
					}
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
				Levelslist.Items.Add( /// Adds a levelpack to the list
					levellist[i].Name + " (Uploaded By: " +
					levellist[i].Author + ") - Views: " +
					levellist[i].Views
				);
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			getArgs();
			Refresh();
		}

		private void Start5b_Click(object sender, RoutedEventArgs e)
		{

			if (selectedlevel != null)
			{
				var id = levellist[Levelslist.SelectedIndex].Id; /// Gets the level id. I guess the point of this is if a file is deleted? Or I guess if there's sorting? All I know is it's unecessary.
				Directory.CreateDirectory(Path.Combine(directory, id)); /// If it doesn't already exist, a new directory for the levelpack is created
				Thread downloadThread = new Thread(() => {
					WebClient client = new WebClient();
					client.DownloadFileCompleted += new AsyncCompletedEventHandler(Client_DownloadFileCompleted);
					client.DownloadFileAsync(new Uri("https://5beam.5blevels.com/download/" + id), Path.Combine(directory, id, "levels.txt")); // Downloads the levelpack
					using (WebClient webClient = new WebClient())
					{
						if (!File.Exists(Path.Combine(directory, id, "5b.exe"))) /// If you've never downloaded this levelpack before, it will download the 5b executable (levelpacks are redownloaded because they can be changed and they take almost no time)
						{
							MessageBox.Show("As this is the first time you are playing this levelpack, please wait up to 10 seconds for the files to download.");
							if (levellist[Convert.ToInt32(id) - 1].Mod == "")
							{
								webClient.DownloadFile("https://5beam.5blevels.com/5b.exe", Path.Combine(directory, id, "5b.exe"));
							}
							else
							{
								webClient.DownloadFile("https://5beam.5blevels.com/mods/" + levellist[Convert.ToInt32(id) - 1].Mod + ".exe", Path.Combine(directory, id, "5b.exe"));
							}
						}
					}
					Dispatcher.BeginInvoke((Action)delegate {
						Hide(); /// Oh, this is new. Because the application is already showing, I can't just tell it to wait until 5b closes. I'm going to hide it first so you pesky users won't try to download a levelpack while it's being read from and break the entire file system (jk but it crashes the app).
						Process game = Process.Start(Path.Combine(directory, id, "5b.exe"));
						game.WaitForExit();
						Show(); /// Makes the application visible again so you see the orange background representing a sunset because v5 is the last version of 5beam-desktop. I bet no one noticed that by themselves.
						Refresh(); /// Refreshes so you see the new view count
					});
				});
				downloadThread.Start();
			}
		}

		void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			/// When the levelpack finishes downloading
		}

		private void Levelslist_SelectionChanged(object sender, SelectionChangedEventArgs e) /// When a new levelpack is selected
		{
			if (Levelslist.Items.Count != 0)
			{
				selectedlevel = Levelslist.SelectedItem.ToString(); /// This is the selected levelpack's name (used to get the id when you press Start 5b).
				int sl_int = Levelslist.SelectedIndex; /// The position this is from the top
				textBlockSelection.Text = "You have selected '" + levellist[sl_int].Name + "' by " + levellist[sl_int].Author + "."; /// The level you selected appears in that small slot thing above the buttons
			}
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			Refresh();
		}

		private void UploadButton_Click(object sender, EventArgs e)
		{
			Process upload = Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "upload.exe"));
			upload.WaitForExit();
			Refresh();
		}
    }
}

/* Coming soon:
 * Flash Player is not required - Done
 * Upload levels from your desktop - Done
 * Like/Dislike levelpacks
 * REPORT levelpacks
 * Accounts?
 * Mac/Linux support (probably not because that'll require a rewrite of the code)
*/