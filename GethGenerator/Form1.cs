using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace GethGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string GeneratePassword ()
        {
            string passwrod = System.Web.Security.Membership.GeneratePassword(16, 4);
            return passwrod;
        }

        private void AddItem (string item)
        {
            if (this.lbPassword.InvokeRequired)
            {
                this.lbPassword.Invoke(new Action<string>(AddItem), item);
            }
            else
            {
                this.lbPassword.Items.Add(item);
            }
        }

        private void AddText(string text)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                this.richTextBox1.Invoke(new Action<string>(AddText), text);
            }
            else
            {
                this.richTextBox1.Text+= text;
            }
        }

        private string GenKey (string password,string guidName)
        {
            string res = "";
            ThreadStart start = new ThreadStart(() =>
            {
                using (var sw = File.CreateText("temppassword.txt")) {
                    sw.WriteLine(password);
                }
                Process process = new Process();
                process.StartInfo.FileName = AppContext.BaseDirectory +"geth\\geth.exe";
                string arg = string.Format("account new --keystore {0} --password temppassword.txt", AppContext.BaseDirectory + "\\KeyStore\\" + guidName);
                process.StartInfo.Arguments = arg;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {

                    // Prepend line numbers to each line of the output.
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        if (e.Data.Contains ("Address"))
                        {
                            string add = Regex.Match(e.Data, @"\{([^)]*)\}").Groups[1].Value;
                            res = add;
                        }
                        if (txtLog.InvokeRequired)
                        {
                            txtLog.Invoke(new Action(() =>
                            {
                                txtLog.Text += e.Data + Environment.NewLine;
                            }));
                        }
                        else
                        {
                            txtLog.Text += (e.Data);

                        }
                    }
                });

                process.Start();

                // Asynchronously read the standard output of the spawned process. 
                // This raises OutputDataReceived events for each line of output.
                process.BeginOutputReadLine();
                process.WaitForExit();

                process.WaitForExit();
                process.Close();
            }
           );

            Thread n = new Thread(start);
            n.Start();
            n.Join();
            return res;
        }

        private void btnStart_Click(object sender, EventArgs eargs)
        {
            Directory.CreateDirectory("KeyStore");
            int numberToGenerate = (int)Math.Round(this.numericUpDown1.Value);

            this.lbPassword.Items.Clear();
            this.richTextBox1.Text = "Starting" + Environment.NewLine;
            this.txtLog.Text = "";
            ThreadStart th = new ThreadStart(() =>
          {
              for (int i = 0; i < numberToGenerate; i++)
              {
                  string g = Guid.NewGuid().ToString();
                  Directory.CreateDirectory("KeyStore\\"+g);
                  string password = this.GeneratePassword();
                  File.WriteAllText("KeyStore\\" + g + "\\password.txt", password);

                  AddItem(password);
                  string key = GenKey(password, g);
                  AddText("key:"+key + Environment.NewLine+"PAssword:"+password+Environment.NewLine);
                  Directory.Move("KeyStore\\" + g, "KeyStore\\" + key);
              }
          });

            new Thread(th).Start();
        }
    }
}
