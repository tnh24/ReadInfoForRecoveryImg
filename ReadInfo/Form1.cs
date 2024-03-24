using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReadInfo
{
    public partial class Form1 : Form
    {
        //private RichTextBox richTextBox1;
        public Form1()
        {
            InitializeComponent();
           
        }

        public static readonly string Temp = Path.Combine(Path.GetTempPath(), "L4AT_adv");

        public string selectedFilePath;

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Directory.Exists(Temp))
                {
                    Directory.Delete(Temp, true);
                    //MessageBox.Show("Directory and its contents deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the file dialog properties
            openFileDialog.Title = "Open File";
            openFileDialog.Filter = "Bin files (*.bin)|*.bin|Img Files (*.img)|*.img|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            // Show the file dialog
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file name and display it in the TextBox
                string selectedFilePath = openFileDialog.FileName;               
                textBox1.Text = selectedFilePath;                              

            }

            backgroundWorker1.RunWorkerAsync();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker2.RunWorkerAsync();
            
            

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string sourceFilePath = textBox1.Text;

            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                MessageBox.Show("Please select a file first.");
                return;
            }

            if (!File.Exists(sourceFilePath))
            {
                MessageBox.Show("The selected file does not exist.");
                return;
            }

            // Create the destination path in the temporary folder
            string destinationPath = Path.Combine(Temp, "boot.img");

            // Ensure the destination directory exists
            if (!Directory.Exists(Temp))]
            {
                Directory.CreateDirectory(Temp);
            }

            // Copy the selected file to the temporary folder
            try
            {
                File.Copy(sourceFilePath, destinationPath, true);           
                //MessageBox.Show("File copied successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while copying the file: {ex.Message}");
            }
            
        }

        private static void ExtractFile()
        {
            byte[] bootimg = Properties.Resources.bootimg;
            if (!Directory.Exists(Temp)) Directory.CreateDirectory(Temp);
            if (!File.Exists(Path.Combine(Temp, "bootimg.exe")))
                File.WriteAllBytes(Path.Combine(Temp, "bootimg.exe"), bootimg);
        }
        public static string runInDirectory(string exe, string cmd)
        {
            string Temp = Path.GetDirectoryName(exe);
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = exe,
                    Arguments = cmd,
                    WorkingDirectory = Temp,
                    RedirectStandardOutput = true
                }
            };
            proc.Start();
            return proc.StandardOutput.ReadToEnd();
        }

        private static string PropReader(string filter, string prop)
        {
            string text = File.ReadAllText(prop);
            string result = "";
            using (StringReader r = new StringReader(text))
            {
                string line;
                while (r.Peek() != -1)
                {
                    line = r.ReadLine();
                    if (line.Contains(filter)) result = line.Substring(line.IndexOf("=") + 1);
                }
            }
            return result;
        }
        public void LoadInfo()
        {
            string defaultProp = "";
            ExtractFile();
            if (File.Exists(Temp + "\\boot.img"))
            {
                runInDirectory(Temp + "\\bootimg.exe", "--unpack-bootimg");
                if (Directory.Exists(Temp + "\\initrd"))
                {
                    if (File.Exists(Temp + "\\initrd\\prop.default")) defaultProp = Temp + "\\initrd\\prop.default";
                    if (File.Exists(Temp + "\\initrd\\default.prop")) defaultProp = Temp + "\\initrd\\default.prop";
                    if (!string.IsNullOrEmpty(defaultProp))
                    {
                        // Clear the existing text in the RichTextBox
                        ClearRichTextBox();

                        using (StreamReader reader = new StreamReader(defaultProp))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {                                
                                if (line.Contains("ro.product.manufacturer"))
                                {                                    
                                    AppendToRichTextBox("Model Name\t=\t" + line.Substring(line.IndexOf("=") + 1));
                                }

                                if (line.Contains("ro.product.mod_device"))
                                {
                                    AppendToRichTextBox("Device Name\t=\t" + line.Substring(line.IndexOf("=") + 1));
                                }
                                if (line.Contains("ro.build.version.incremental"))
                                {
                                    AppendToRichTextBox("Miui Version\t=\t" + line.Substring(line.IndexOf("=") + 1));    
                                }
                                if (line.Contains("ro.build.version.release"))
                                {
                                    AppendToRichTextBox("Android Version\t=\t" + line.Substring(line.IndexOf("=") + 1));

                                }

                            }
                        }
                    }
                }
            }
        }

        private void ClearRichTextBox()
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() => richTextBox1.Clear()));
            }
            else
            {
                richTextBox1.Clear();
            }
        }

        private void AppendToRichTextBox(string text)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(text + Environment.NewLine)));
            }
            else
            {
                richTextBox1.AppendText(text + Environment.NewLine);
            }
        }


        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadInfo();

        }
    }
}
