using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageMagick;
using iText.Kernel.Pdf;

public class MainForm : Form
{
    private Label lblDragDrop = null!;
    private ListBox listBox = null!;
    private Button btnSelectPDFs = null!, btnRemoveSelected = null!, btnClearAll = null!, btnConvert = null!;
    private TrackBar trackBarDPI = null!;
    private Label lblDPIValue = null!;
    private ProgressBar progressBar = null!;
    private Label lblProgress = null!;

    public MainForm()
    {
        this.Icon = new Icon("X:/pdf to PNG/PdfToPngConverter/Assets/grass_1497189_MMM_icon.ico");  // Adjust the path as per your project structure

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Main Form Properties
        this.Text = "DashVerify PDF to PNG";
        this.Size = new Size(600, 450);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;
        this.Font = new Font("Segoe UI", 9F);

        // Drag and Drop Label (Top)
        lblDragDrop = new Label
        {
            Text = "Drag and Drop PDFs or Select Files",
            Size = new Size(560, 60),
            Location = new Point(20, 20),
            BorderStyle = BorderStyle.FixedSingle,
            TextAlign = ContentAlignment.MiddleCenter,
            AllowDrop = true,
            BackColor = Color.LightBlue,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };
        lblDragDrop.DragEnter += Label_DragEnter;
        lblDragDrop.DragDrop += Label_DragDrop;

        // Listbox to display selected files
        listBox = new ListBox
        {
            Size = new Size(560, 120),
            Location = new Point(20, 90),
            ScrollAlwaysVisible = true,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Buttons Section (Under Listbox)
        btnSelectPDFs = CreateButton("Select PDFs", 20, 220);
        btnSelectPDFs.Click += BtnSelectPDFs_Click;

        btnRemoveSelected = CreateButton("Remove Selected", 160, 220);
        btnRemoveSelected.Click += BtnRemoveSelected_Click;

        btnClearAll = CreateButton("Clear All", 300, 220);
        btnClearAll.Click += BtnClearAll_Click;

        btnConvert = CreateButton("Convert to PNG", 440, 220);
        btnConvert.Click += BtnConvert_Click;

        // DPI Slider (Under Buttons)
        trackBarDPI = new TrackBar
        {
            Minimum = 50,
            Maximum = 300,
            Value = 300,
            TickFrequency = 72,
            Location = new Point(20, 270),
            Size = new Size(460, 45)
        };
        trackBarDPI.Scroll += TrackBarDPI_Scroll;

        lblDPIValue = new Label
        {
            Text = "DPI: 300",
            Size = new Size(80, 30),
            Location = new Point(490, 270),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Progress Bar (Bottom)
        progressBar = new ProgressBar
        {
            Size = new Size(560, 25),
            Location = new Point(20, 320),
            Style = ProgressBarStyle.Continuous
        };

        lblProgress = new Label
        {
            Size = new Size(560, 20),
            Location = new Point(20, 350),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Add controls to the form
        this.Controls.AddRange(new Control[] {
            lblDragDrop, listBox, btnSelectPDFs, btnRemoveSelected, btnClearAll, btnConvert,
            trackBarDPI, lblDPIValue, progressBar, lblProgress
        });

        // Enable drag and drop for the entire form
        this.AllowDrop = true;
        this.DragEnter += Label_DragEnter;
        this.DragDrop += Label_DragDrop;
    }

    private Button CreateButton(string text, int x, int y)
    {
        return new Button
        {
            Text = text,
            Size = new Size(130, 35),
            Location = new Point(x, y),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
    }

    private void Label_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
    }

    private void Label_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
        {
            foreach (string file in files)
            {
                if (Path.GetExtension(file).ToLower() == ".pdf")
                    listBox.Items.Add(file);
            }
        }
    }

    private void BtnSelectPDFs_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog openFileDialog = new()
        {
            Multiselect = true,
            Filter = "PDF Files (*.pdf)|*.pdf"
        };
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            listBox.Items.AddRange(openFileDialog.FileNames);
        }
    }

    private void BtnRemoveSelected_Click(object? sender, EventArgs e)
    {
        for (int i = listBox.SelectedIndices.Count - 1; i >= 0; i--)
        {
            listBox.Items.RemoveAt(listBox.SelectedIndices[i]);
        }
    }

    private void BtnClearAll_Click(object? sender, EventArgs e) => listBox.Items.Clear();

    private async void BtnConvert_Click(object? sender, EventArgs e)
    {
        if (listBox.Items.Count == 0)
        {
            MessageBox.Show("Please select at least one PDF file to convert.", "No Files Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using FolderBrowserDialog folderBrowser = new();
        folderBrowser.Description = "Select Output Folder";
        if (folderBrowser.ShowDialog() == DialogResult.OK)
        {
            string outputFolder = folderBrowser.SelectedPath;
            int dpi = trackBarDPI.Value;

            progressBar.Minimum = 0;
            progressBar.Maximum = listBox.Items.Count;
            progressBar.Value = 0;

            btnConvert.Enabled = false;

            await Task.Run(() =>
            {
                Parallel.ForEach(listBox.Items.Cast<string>(), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    (pdfPath, state, index) =>
                    {
                        ConvertPdfToPng(pdfPath, outputFolder, dpi);
                        this.Invoke(new Action(() =>
                        {
                            progressBar.Value++;
                            lblProgress.Text = $"Converting: {progressBar.Value} / {listBox.Items.Count}";
                        }));
                    });
            });

            btnConvert.Enabled = true;
            MessageBox.Show("Conversion completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void ConvertPdfToPng(string pdfPath, string outputFolder, int dpi)
    {
        string pdfName = Path.GetFileNameWithoutExtension(pdfPath);
        string pdfOutputFolder = Path.Combine(outputFolder, pdfName);
        Directory.CreateDirectory(pdfOutputFolder);

        using var pdfReader = new PdfReader(pdfPath);
        using var pdfDocument = new PdfDocument(pdfReader);
        int pageCount = pdfDocument.GetNumberOfPages();

        Parallel.For(1, pageCount + 1, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            pageNum =>
            {
                var magickReadSettings = new MagickReadSettings
                {
                    Density = new Density(dpi),
                    Format = MagickFormat.Pdf
                };

                using var images = new MagickImageCollection();
                images.Read($"{pdfPath}[{pageNum - 1}]", magickReadSettings);

                using var image = images[0];
                image.Format = MagickFormat.Png;
                image.Alpha(AlphaOption.Remove);  // Remove transparency
                image.BackgroundColor = new MagickColor("#FFFFFF");  // Set white background

                string outputPath = Path.Combine(pdfOutputFolder, $"{pdfName}_page{pageNum}.png");
                image.Write(outputPath);
            });
    }

    private void TrackBarDPI_Scroll(object? sender, EventArgs e)
    {
        lblDPIValue.Text = $"DPI: {trackBarDPI.Value}";
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
