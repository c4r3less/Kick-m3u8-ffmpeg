using IronPython.Runtime;
using Python.Runtime;
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
            label1.Text = "Link";
            label2.Text = "m3u8 linki";
        }

        private void RunPythonCode()
        {
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
            }

            runChrome(textBox1.Text);
        }

        private void runChrome(string url)
        {
            using (Py.GIL())
            {
                dynamic selenium = Py.Import("selenium.webdriver");
                dynamic bs4 = Py.Import("bs4");
                dynamic driver = selenium.Chrome();
                driver.get(url);
                Analyze(driver.page_source.ToString(), bs4);
                driver.quit();
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
                        var links = re.findall(@"https?://stream\.kick[^\s]+\.m3u8", script.text);
                        foreach (var link in links)
                        {
                            textBox2.AppendText(link + Environment.NewLine);
                        }
                    }
                }
            }
        }

        private void twitchTs()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "M3U8 Files (*.m3u8)|*.m3u8|All Files (*.*)|*.*",
                Title = "M3U8 Dosyasýný Seçin"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string path = ofd.FileName;
                string content = File.ReadAllText(path);
                Regex tsFind = new Regex(@"(\d+\.ts)", RegexOptions.Multiline);
                MatchCollection matches = tsFind.Matches(content);
                listBox1.Items.Clear();
                foreach (Match match in matches)
                {
                    listBox1.Items.Add(match.Value);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            RunPythonCode();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            twitchTs();
        }
    }
}
