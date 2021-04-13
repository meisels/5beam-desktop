using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace _5beam_upload
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            text.Rtf = @"{\rtf1\ansi \b Rules:\b0 \par 1. Your file must be a levelpack file. Other files will be deleted. \par 2. Filesize limit: 500KB \par 3. Files must not have inappropiate content. \par 4. Please put at least 53 levels in your levelpack. Right now only 53 will display and any less will crash 5b. I am working on fixing this so bear with me while I deal with my lack of confidence. \par 5. Make sure your levelpack works without any mods. (you can DM me your mod and I might put it up) \par \b Notices: \b0 \par For now, if you want your file deleted, you can DM me on discord, imaperson#1060. Don't tell anybody this but... there may or may not be an account system in the works!}";
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            openFile.FileName = "levels.txt";
            openFile.Filter = "Text files (*.txt)|*.txt";
            openFile.Title = "Upload Process (1/6)";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var filePath = openFile.FileName;
                    using (Stream str = openFile.OpenFile())
                    {
                        string Author;
                        string Name;
                        string Description;
                        string Difficulty;
                        string Guy;

                        do
                        {
                            Author = Interaction.InputBox("Author Name:", "Upload Process (2/6)", "John Doe", 100, 100);
                        } while (Author == "");

                        do
                        {
                            Name = Interaction.InputBox("Levelpack Name:", "Upload Process (3/6)", "Untitled Levelpack", 100, 100);
                        } while (Name == "");

                        do
                        {
                            Description = Interaction.InputBox("Description:", "Upload Process (4/6)", "Description...", 100, 100);
                        } while (Description == "");

                        do
                        {
                            Difficulty = Interaction.InputBox("Difficulty Rating (check 5beam.5blevels.com for ratings):", "Upload Process (5/6)", "0-7", 100, 100);
                        } while (Difficulty != "0" && Difficulty != "1" && Difficulty != "2" && Difficulty != "3" && Difficulty != "4" && Difficulty != "5" && Difficulty != "6" && Difficulty != "7");

                        do
                        {
                            Guy = Interaction.InputBox("Uses Guy's Mod:", "Upload Process (6/6)", "Y/N", 100, 100);
                        } while (Guy != "Y" && Guy != "y" && Guy != "N" && Guy != "n");

                        if (Guy == "Y" || Guy == "y")
                        {
                            Guy = "true";
                        }
                        else if (Guy == "N" || Guy == "n")
                        {
                            Guy = "false";
                        }

                        WebRequest request = WebRequest.Create("https://5beam.5blevels.com/upload/" + Name + "/" + Author + "/" + Description + "/" + Difficulty + "/" + Guy);
                        request.Credentials = CredentialCache.DefaultCredentials;
                        request.Method = "POST";

                        string postData = "uploadfile=" + File.ReadAllText(filePath);
                        byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                        request.ContentLength = byteArray.Length;
                        request.ContentType = "application/x-www-form-urlencoded";

                        Stream dataStream = request.GetRequestStream();
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        dataStream.Close();

                        System.Threading.Thread.Sleep(200);

                        Close();
                    }
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                    Close();
                }
            }
        }
    }
}
