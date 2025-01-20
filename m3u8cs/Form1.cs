using Python.Runtime;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
namespace m3u8cs
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "site link";
            label2.Text = "m3u8:";
            comboBox1.Items.Add("Chrome");
            comboBox1.Items.Add("FireFox");
        }

        private void RunPythonCode(string url)
        {
            try
            {
                using (Py.GIL())
                {
                    dynamic selenium = Py.Import("selenium.webdriver");
                    dynamic time = Py.Import("time");
                    dynamic bs4 = Py.Import("bs4");
                    dynamic sys = Py.Import("sys");
                    if (!string.IsNullOrEmpty(url))
                    {
                        if (comboBox1.SelectedIndex == 0)
                        {
                            dynamic driver = selenium.Chrome();
                            driver.get(url);
                            time.sleep(10);
                            Analyze(driver.page_source.ToString(), bs4);
                            driver.quit();
                        }
                        else if (comboBox1.SelectedIndex == 1)
                        {
                            dynamic driver = selenium.Firefox();
                            driver.get(url);
                            time.sleep(10);
                            Analyze(driver.page_source.ToString(), bs4);
                            driver.quit();
                        }
                        else
                        {
                            MessageBox.Show("Please select a browser.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a link.");

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Analyze(string html, dynamic bs4)
        {
            using (Py.GIL())
            {
                dynamic soup = bs4.BeautifulSoup(html, "html.parser");
                findM3u8(soup);
            }
        }

        private void findM3u8(dynamic soup)
        {
            using (Py.GIL())
            {
                dynamic re = Py.Import("re");
                foreach (var script in soup.find_all("script"))
                {
                    if (script.text.ToString().Contains("m3u8"))
                    {
                        var links = re.findall(@"https?://[^\s\""]+\/master.m3u8", script.text);
                        foreach (var link in links)
                        {
                            richTextBox1.AppendText(link + Environment.NewLine);
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RunPythonCode(textBox1.Text);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = comboBox1.SelectedIndex.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text.Length > 0)
            {
                string link = richTextBox1.Lines[0];
                SaveFileDialog save = new SaveFileDialog
                {
                    Filter = "MP4 Video (*.mp4)|*.mp4",
                    Title = "Save MP4 File"
                };
                if (save.ShowDialog() == DialogResult.OK)
                {
                    FFmpeg(link, save.FileName, Path.GetDirectoryName(save.FileName));
                }
            }
        }
        private void FFmpeg(string m3u8Link, string output, string outputDir)
        {
            ProcessStartInfo ff = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = outputDir,
                CreateNoWindow = false,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            
            Process process = new Process
            {
                StartInfo = ff
            };
            process.Start();
;
            process.StandardInput.WriteLine($"ffmpeg -i \"{m3u8Link}\" -c copy \"{output}\"");
            process.StandardInput.Close();
            process.WaitForExit();
        }
    }
}
