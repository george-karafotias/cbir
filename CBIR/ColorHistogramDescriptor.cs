using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;


namespace CBIR
{
    public class ColorHistogramDescriptor
    {
        public FileInfo QueryItem;
        public FileInfo[] CollectionItems;

        public void FindSimilarity(System.ComponentModel.BackgroundWorker worker, System.ComponentModel.DoWorkEventArgs e)
        {
            List<SimilarItem> similarItems = new List<SimilarItem>();

            try
            {
                int comparisonsMade = 0;
                int collectionSize = CollectionItems.Length;

                Bitmap query = new Bitmap(QueryItem.FullName);
                double[] queryHistogram = Histogram1(query);

                foreach (FileInfo collectionItem in CollectionItems)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        double[] collectionItemHistogram = Histogram1(new Bitmap(collectionItem.FullName));
                        double similarity = CompareHistograms1(queryHistogram, collectionItemHistogram);
                        similarItems.Add(new SimilarItem() { FullPath = collectionItem.FullName, Score = similarity });

                        comparisonsMade++;
                        int percentCompleted = Convert.ToInt32(Math.Floor(((double)comparisonsMade / collectionSize)* 100));
                        worker.ReportProgress(percentCompleted, null);
                    }
                }

                List<SimilarItem> sortedSimilarItems = similarItems.OrderByDescending(o => o.Score).ToList();

                e.Result = sortedSimilarItems;
                worker.ReportProgress(100, null);
            }
            catch
            {
                e.Result = null;
                worker.ReportProgress(100, "Error");
            }
        }

        private double CompareHistograms(double[][] histogram1, double[][] histogram2)
        {
            double distance = 0;

            for (int i = 0; i < histogram1.Length; i++)
            {
                for (int j = 0; j < histogram1.Length; j++)
                {
                    distance += Math.Abs(histogram1[i][j] - histogram2[i][j]);
                }
            }

            return 1-distance;
        }

        private double CompareHistograms1(double[] histogram1, double[] histogram2)
        {
            double distance = 0;

            for (int i = 0; i < histogram1.Length; i++)
            {
                distance += Math.Abs(histogram1[i] - histogram2[i]);
            }

            return 1 - (distance/2);
        }

        private double[][] Histogram(Bitmap sourceImage)
        {
            double[][] RGBColor = { new double[256], new double[256], new double[256] };
            int width = sourceImage.Width, height = sourceImage.Height;
            byte Red, Green, Blue;
            Color pixelColor;

            for (int i = 0, j; i < width; ++i)
            {
                for (j = 0; j < height; ++j)
                {
                    pixelColor = sourceImage.GetPixel(i, j);
                    Red = pixelColor.R;
                    Green = pixelColor.G;
                    Blue = pixelColor.B;
                    ++RGBColor[0][Red];
                    ++RGBColor[1][Green];
                    ++RGBColor[2][Blue];
                }
            }

            double normalizationFactor = width * height;
            for (int i = 0; i < RGBColor.Length; i++)
            {
                for (int j = 0; j < RGBColor[0].Length; j++)
                {
                    RGBColor[i][j] = RGBColor[i][j] / normalizationFactor;
                }
            }

            return RGBColor;
        }

        private double[] Histogram1(Bitmap sourceImage)
        {
            double[] RGBColor = new double[512];
            int width = sourceImage.Width, height = sourceImage.Height;
            byte Red, Green, Blue;
            Color pixelColor;

            for (int i = 0, j; i < width; ++i)
            {
                for (j = 0; j < height; ++j)
                {
                    pixelColor = sourceImage.GetPixel(i, j);
                    Red = pixelColor.R;
                    Green = pixelColor.G;
                    Blue = pixelColor.B;

                    int quantColor = ((Red / 32) * 64) + ((Green / 32) * 8) + (Blue / 32);

                    ++RGBColor[quantColor];
                }
            }

            double normalizationFactor = width * height;
            for (int i = 0; i < RGBColor.Length; i++)
            {
                RGBColor[i] = RGBColor[i] / normalizationFactor;
            }

            return RGBColor;
        }

    }
}
