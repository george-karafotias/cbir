using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace CBIR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.ComponentModel.BackgroundWorker BackgroundWorker1;

        private FileInfo[] directoryImages;
        public List<SimilarItem> imageList = new List<SimilarItem>();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void OpenInputImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image files (*.bmp, *.png, *.jpg)|*.bmp;*.png;*.jpg|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            bool? userClickedOK = openFileDialog1.ShowDialog();

            if (userClickedOK == true)
            {
                try
                {
                    string imageLocation = openFileDialog1.FileName;

                    BitmapImage b = new BitmapImage();
                    b.BeginInit();
                    b.UriSource = new Uri(imageLocation);
                    b.EndInit();

                    InputImage.Source = b;
                    InputImageLocation.Text = imageLocation;
                }
                catch
                {

                }
            }
        }

        private void OpenImagesDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            
            if (result.ToString() == "OK")
            {
                string selectedDirectoryPath = dialog.SelectedPath;

                directoryImages = GetDirectoryImages(selectedDirectoryPath);

                if (directoryImages != null && directoryImages.Length > 0)
                {
                    ImagesDirectory.Text = selectedDirectoryPath;
                    StatusBarText.Text = "Loaded " + directoryImages.Length + " images";
                }
                else
                {
                    StatusBarText.Text = "No images to load in the selected directory";
                }
            }
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            Button compareButton = sender as Button;
            if (compareButton.Content.ToString() == "Cancel")
            {
                this.BackgroundWorker1.CancelAsync();
                compareButton.Content = "Compare";
            }
            else
            {
                FindSimilarImages();
            }
        }

        private FileInfo[] GetDirectoryImages(string directoryFullPath)
        {
            DirectoryInfo selectedDirectoryInfo = new DirectoryInfo(directoryFullPath);
            string[] extensionArray = new string[] { ".bmp", ".png", ".jpg" };
            HashSet<string> allowedExtensions = new HashSet<string>(extensionArray, StringComparer.OrdinalIgnoreCase);
            FileInfo[] files = Array.FindAll(selectedDirectoryInfo.GetFiles(), f => allowedExtensions.Contains(f.Extension));

            return files;
        }

        private void FindSimilarImages()
        {
            if (directoryImages != null && directoryImages.Length > 0)
            {
                InitializeBackgroundWorker();

                ColorHistogramDescriptor chd = new ColorHistogramDescriptor();
                chd.QueryItem = new FileInfo(InputImageLocation.Text);
                chd.CollectionItems = directoryImages;

                BackgroundWorker1.RunWorkerAsync(chd);

                CompareButton.Content = "Cancel";
            }
        }

        private void InitializeBackgroundWorker()
        {
            BackgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            BackgroundWorker1.WorkerReportsProgress = true;
            BackgroundWorker1.WorkerSupportsCancellation = true;

            BackgroundWorker1.DoWork +=
                new System.ComponentModel.DoWorkEventHandler(BackgroundWorker1_DoWork);
            BackgroundWorker1.ProgressChanged +=
                new System.ComponentModel.ProgressChangedEventHandler(BackgroundWorker1_ProgressChanged);
            BackgroundWorker1.RunWorkerCompleted +=
                new System.ComponentModel.RunWorkerCompletedEventHandler(BackgroundWorker1_RunWorkerCompleted);
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker;
            worker = (System.ComponentModel.BackgroundWorker)sender;

            ColorHistogramDescriptor chd = (ColorHistogramDescriptor)e.Argument;
            chd.FindSimilarity(worker, e);
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error: " + e.Error.Message);
            }
            else if (e.Cancelled)
            {
                StatusBarText.Text = "Last action cancelled";
            }
            else
            {
                SimilarImagesListView.ItemsSource = e.Result as List<SimilarItem>;
            }

            CompareButton.Content = "Compare";
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusBarText.Text = "Percent completed: " + e.ProgressPercentage;
        }

    }
}
