using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace PgMelaToSvgConverter
{
    public partial class Form1 : Form
    {
        StringBuilder svgOutput;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = this.textBoxFileHtml.Text;
                if (string.IsNullOrWhiteSpace(filename))
                {
                    MessageBox.Show("Nome file mancante");
                    return;
                }

                svgOutput = new StringBuilder();
                svgOutput.Append(@"<svg width='100%' height='100%' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink'>");

              
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(filename);

                var  table = doc.DocumentNode.SelectNodes("//table").Cast<HtmlNode>();
                var rows = table.First().SelectNodes("tr").Cast<HtmlNode>();

                svgOutput.Append("<g>");
                int x=0, y=0;

                int numColsFirstRow = 0;
                int numColsNRow = 0;
                for (int j = 0; j < rows.Count() ; j++)
                {
                    var row = rows.ElementAt(j);
                    x = 0;
                    var cells = row.SelectNodes("th|td").Cast<HtmlNode>();
                    for (int i = 0; i < cells.Count(); i++)
                    {
                        var cell = cells.ElementAt(i);

                        int.TryParse(cell.Attributes["colspan"].Value, out int colspan);

                        if (j == 0)
                        {
                            numColsFirstRow = numColsFirstRow + (colspan > 0 ? colspan : 1);
                        }
                        else
                        {
                            numColsNRow = numColsNRow + (colspan > 0 ? colspan : 1);
                        }

                        string style = cell.Attributes["style"].Value;
                        string color = "";
                        try
                        {
                            color = style.Substring(style.IndexOf('#'), 7);
                        }
                        catch(Exception ex)
                        {

                        }
                        Console.WriteLine("color: {0}", color);
                        // se sono su ultima colonna a dx o riga in basso, largo 10, altimenti largo 11 per andare sotto la cella successiva ed evitare aliasing dei bordi
                        int width = (i == (cells.Count() - 1)) ? 10 : 11;
                        int height = (j == (rows.Count() - 1)) ? 10 : 11;
                        if (colspan > 1)
                        {
                            for (int z = 0; z < colspan; z++)
                            {
                                width = (i + z + 1 ) == ((cells.Count() - 1) + colspan)  ? 10 : 11;
                                svgOutput.Append($"<rect x='{x}' y='{y}' width='{width}' height='{height}' fill='{color}' />");
                                x += 10;
                            }
                        }
                        else
                        {
                            svgOutput.Append($"<rect x='{x}' y='{y}' width='{width}' height='{height}' fill='{color}' />");
                            x += 10;
                        }
                    }

                    if (numColsFirstRow != numColsNRow && j != 0)
                    {
                        MessageBox.Show($"La riga {j+1} ha {numColsNRow} colonne, ma la prima riga ne ha {numColsFirstRow} !!!"  );
                    }
                    numColsNRow = 0;
                    y += 10;
                }

                svgOutput.Append("</g>");
                svgOutput.Append("</svg>");

                saveFileDialog1.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}");
            }
            
         


        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "html | *.html";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBoxFileHtml.Text = this.openFileDialog1.FileName;
            }
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Get file name.
            string name = saveFileDialog1.FileName;
            // Write to the file name selected.
            // ... You can write the text from a TextBox instead of a string literal.
            File.WriteAllText(name, svgOutput.ToString());


            MessageBox.Show($"File creato in: {name}");
        }
    }
}
