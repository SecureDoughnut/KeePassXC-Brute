using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace KeepassBrute
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static bool status = true;
        static object access = new object();

        static List<Thread> threads = new List<Thread>();
        static List<string> passwords = new List<string>();
        static string file = "";

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.InitialDirectory = Environment.CurrentDirectory;
            op.ShowDialog();

            textBox2.Text = op.FileName;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.InitialDirectory = Environment.CurrentDirectory;
            op.ShowDialog();

            textBox3.Text = op.FileName;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            label6.Text = "RUNNING";
            status = true;

            passwords = File.ReadAllLines(textBox2.Text, Encoding.UTF8).Distinct().ToList();
            file = textBox3.Text;


            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                Thread th = new Thread(BruteThread);
                threads.Add(th);
                th.Start();
                Console.WriteLine(" [+] Thread " + i + " started!");
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            foreach (Thread t in threads)
                t.Abort();

            label6.Text = "STOPPED";

        }

        private static void BruteThread()
        {
            while(true)
            {
                if (!status)
                    break;

                string password = "";

                lock(access)
                {
                    password = passwords[0];
                    passwords.RemoveAt(0);
                }

                Console.WriteLine(" [*] Trying " + password + "...");
                Process ps = new Process();
                ps.StartInfo.FileName = @"C:\Program Files\KeePassXC\keepassxc-cli.exe";
                ps.StartInfo.UseShellExecute = false;
                ps.StartInfo.CreateNoWindow = true;
                ps.StartInfo.RedirectStandardOutput = true;
                ps.StartInfo.RedirectStandardInput = true;
                ps.StartInfo.Arguments = " db-info \"" + file + "\"";
                ps.Start();

                StreamWriter FirstProcessWriter = ps.StandardInput;
                FirstProcessWriter.WriteLine(password);

                while (!ps.StandardOutput.EndOfStream)
                {
                    string line = ps.StandardOutput.ReadLine();
                    if (line.Contains("UUID"))
                    {
                        Console.WriteLine(line);

                        Console.WriteLine(" [+] FOUND! Password is '" + password + "'");
                        status = false;
                        break;
                    }
                }
            }
        }
    }
}
