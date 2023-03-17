using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PatternRecognition.FingerprintRecognition.Core;
using PatternRecognition.FingerprintRecognition.FeatureExtractors;
using PatternRecognition.FingerprintRecognition.Matchers;

using static System.Net.Mime.MediaTypeNames;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace fingerprint
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            obrazek_2.Source = obrazek.Source;
            obrazek_4.Source = obrazek_3.Source;
        }

        #region Odczyt/Zapis
        private void ZaladujZPliku(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.bmp;*.gif;*.tif;*.tiff;*.jpeg;)|*.png;*.jpg;*.bmp;*.gif;*.tif;*.tiff;*.jpeg; | All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Uri fileUri = new Uri(openFileDialog.FileName);
                    obrazek.Source = new BitmapImage(fileUri);
                    qry = openFileDialog.FileName;
                    obrazek_2.Source = obrazek.Source;
                   
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("Zły format pliku!", "Wczytywanie z pliku", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }


            left_border.BorderBrush = Brushes.Black;
            right_border.BorderBrush = Brushes.Black;
            left_border.BorderThickness = new Thickness(1);
            right_border.BorderThickness = new Thickness(1);

            rozgalezienia_przycisk.IsEnabled = false;

        }

        private void ZaladujZPliku2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.bmp;*.gif;*.tif;*.tiff;*.jpeg;)|*.png;*.jpg;*.bmp;*.gif;*.tif;*.tiff;*.jpeg; | All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Uri fileUri = new Uri(openFileDialog.FileName);
                    obrazek_3.Source = new BitmapImage(fileUri);
                    temp = openFileDialog.FileName;
                    obrazek_4.Source = obrazek_3.Source;
                }
                catch (NotSupportedException)
                {
                    MessageBox.Show("Zły format pliku!", "Wczytywanie z pliku", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }


            right_border_2.BorderBrush = Brushes.Black;
            right_border_3.BorderBrush = Brushes.Black;
            right_border_2.BorderThickness = new Thickness(1);
            right_border_3.BorderThickness = new Thickness(1);

            rozgalezienia_przycisk.IsEnabled = false;

        }


        private void ZapiszDoPliku(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                          "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                          "Portable Network Graphic (*.png)|*.png"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                FileStream saveStream = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate);
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)obrazek.Source));
                encoder.Save(saveStream);
                saveStream.Close();
            }
        }
        #endregion

        #region ZamianaBitmap
        private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using MemoryStream outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);
            Bitmap bitmap = new Bitmap(outStream);

            return new Bitmap(bitmap);
        }
        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }
        #endregion

        #region Binaryzacja

        private void ZamianaNaOdcienSzarosci(Bitmap bmp)
        {
            Color p;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    p = bmp.GetPixel(x, y);

                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;
                    int avg = (r + g + b) / 3;

                    bmp.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                }
            }
            obrazek_2.Source = BitmapToBitmapImage(bmp);

        }
        private void ZamianaNaOdcienSzarosci2(Bitmap bmp)
        {
            Color p;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    p = bmp.GetPixel(x, y);

                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;
                    int avg = (r + g + b) / 3;

                    bmp.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                }
            }
            obrazek_4.Source = BitmapToBitmapImage(bmp);

        }

        private void BinaryzacjaAutomatyczna(Bitmap b) //otsu
        {
            ZamianaNaOdcienSzarosci(b);
            if (b != null)
            {
                Color curColor;
                int kolor;
                int prog;
                prog = ObliczanieProgOtsu(b);

                for (int i = 0; i < b.Width; i++)
                {
                    for (int j = 0; j < b.Height; j++)
                    {
                        curColor = b.GetPixel(i, j);
                        kolor = curColor.R;

                        if (kolor > prog)
                        {
                            kolor = 255;
                        }
                        else
                            kolor = 0;
                        b.SetPixel(i, j, Color.FromArgb(kolor, kolor, kolor));
                    }
                }
                obrazek.Source = BitmapToBitmapImage(b);

            }
        }

        private void BinaryzacjaAutomatyczna2(Bitmap b) //otsu
        {
            ZamianaNaOdcienSzarosci2(b);
            if (b != null)
            {
                Color curColor;
                int kolor;
                int prog;
                prog = ObliczanieProgOtsu(b);

                for (int i = 0; i < b.Width; i++)
                {
                    for (int j = 0; j < b.Height; j++)
                    {
                        curColor = b.GetPixel(i, j);
                        kolor = curColor.R;

                        if (kolor > prog)
                        {
                            kolor = 255;
                        }
                        else
                            kolor = 0;
                        b.SetPixel(i, j, Color.FromArgb(kolor, kolor, kolor));
                    }
                }
                obrazek_3.Source = BitmapToBitmapImage(b);

            }
        }


        private int ObliczanieProgOtsu(Bitmap b)
        {
            int[] histogram = new int[256];
            for (int m = 0; m < b.Width; m++)
            {
                for (int n = 0; n < b.Height; n++)
                {
                    Color pixel = b.GetPixel(m, n);
                    histogram[pixel.R]++;
                }
            }
            long[] pob = new long[256];
            long[] pt = new long[256];
            for (int t = 0; t < 256; t++)
            {
                for (int t1 = 0; t1 <= t; t1++)
                    pob[t] += histogram[t1];
                for (int t1 = t + 1; t1 < 256; t1++)
                    pt[t] += histogram[t1];
            }
            double[] srOb = new double[256];
            double[] srT = new double[256];
            for (int t = 0; t < 256; t++)
            {
                for (int k = 0; k <= t; k++)
                    srOb[t] += (k * histogram[k]);
                for (int k = t + 1; k < 256; k++)
                    srT[t] += (k * histogram[k]);
            }
            for (int t = 0; t < 256; t++)
            {
                if (pob[t] != 0)
                    srOb[t] = srOb[t] / pob[t];
                if (pt[t] != 0)
                    srT[t] = srT[t] / pt[t];
            }
            double[] wariancjaMiedzy = new double[256];
            double maks = 0;
            for (int t = 0; t < 256; t++)
                wariancjaMiedzy[t] = pob[t] * pt[t] * (srOb[t] - srT[t]) * (srOb[t] - srT[t]);
            int x = 0;
            for (int w = 0; w < 256; w++)
            {
                if (wariancjaMiedzy[w] > maks)
                {
                    maks = wariancjaMiedzy[w];
                    x = w;
                }
            }
            return x;
        }
        #endregion

        #region Szkieletyzacja
        private void BinaryzacjaISzkieletyzacja(object sender, RoutedEventArgs e)
        {
            if (obrazek.Source == null)
            {
                left_border.BorderBrush = Brushes.Red;
                right_border.BorderBrush = Brushes.Red;
                left_border.BorderThickness = new Thickness(2);
                right_border.BorderThickness = new Thickness(2);
                right_border_2.BorderBrush = Brushes.Red;
                right_border_3.BorderBrush = Brushes.Red;
                right_border_2.BorderThickness = new Thickness(2);
                right_border_3.BorderThickness = new Thickness(2);

                return;
            }
            BitmapImage source = obrazek_2.Source as BitmapImage;
            BitmapImage source2 = obrazek_4.Source as BitmapImage;
            Bitmap b = BitmapImageToBitmap(source);
            Bitmap bb = BitmapImageToBitmap(source2);
            BinaryzacjaAutomatyczna(b);
            BinaryzacjaAutomatyczna2(bb);
            KMM(b);
            KMM2(bb);
        }
        private void KMM(Bitmap b)
        {
            int[] listaCzworek = { 3, 6, 7, 12, 14, 15, 24, 28, 30, 48, 56, 60, 96, 112, 120, 129, 131, 135, 192, 193, 195, 224, 225, 240 };
            int[,] maksaSprawdzajaca = { { 128, 64, 32 }, { 1, 0, 16 }, { 2, 4, 8 } };
            int[] maskaWyciec = { 3, 5, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30, 31, 48, 52, 53, 54, 55, 56, 60, 61, 62, 63, 65, 67, 69, 71, 77, 79, 80, 81, 83, 84, 85, 86, 87, 88, 89, 91, 92, 93, 94, 95, 97, 99, 101, 103, 109, 111, 112, 113, 115, 116, 117, 118, 119, 120, 121, 123, 124, 125, 126, 127, 131, 133, 135, 141, 143, 149, 151, 157, 159, 181, 183, 189, 191, 192, 193, 195, 197, 199, 205, 207, 208, 209, 211, 212, 213, 214, 215, 216, 217, 219, 220, 221, 222, 223, 224, 225, 227, 229, 231, 237, 239, 240, 241, 243, 244, 245, 246, 247, 248, 249, 251, 252, 253, 254, 255 };
            List<int> czworki = new List<int>(listaCzworek);
            List<int> wciecia = new List<int>(maskaWyciec);

            int dlugosc = 1;
            int[,] nowePixele = new int[b.Width, b.Height];
            for (int x = dlugosc; x < b.Width - dlugosc; x++)
            {
                for (int y = dlugosc; y < b.Height - dlugosc; y++)
                {
                    Color koloryOb = b.GetPixel(x, y);
                    if (koloryOb.R == 0) nowePixele[x, y] = 1;
                    else nowePixele[x, y] = 0;
                }
            }
            Boolean zmiana = false;
            do
            {
                zmiana = false;
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 1)
                        {
                            if (nowePixele[x + 1, y] == 0 || nowePixele[x, y + 1] == 0 || nowePixele[x, y - 1] == 0 || nowePixele[x - 1, y] == 0)
                                nowePixele[x, y] = 2;
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 1)
                        {
                            if (nowePixele[x + 1, y + 1] == 0 || nowePixele[x - 1, y + 1] == 0 || nowePixele[x - 1, y - 1] == 0 || nowePixele[x + 1, y - 1] == 0)
                                nowePixele[x, y] = 3;
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 2)
                        {
                            nowePixele[x, y] = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (nowePixele[x + i, y + j] == 4 || nowePixele[x + i, y + j] == 1 || nowePixele[x + i, y + j] == 2 || nowePixele[x + i, y + j] == 3)
                                        nowePixele[x, y] += maksaSprawdzajaca[1 + i, 1 + j];
                                }
                            }
                            if (czworki.Contains(nowePixele[x, y]))
                            {
                                nowePixele[x, y] = 4;
                                //zmiana = true;
                            }
                            else
                                nowePixele[x, y] = 2;
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 4)
                        {
                            nowePixele[x, y] = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (nowePixele[x + i, y + j] == 4 || nowePixele[x + i, y + j] == 1 || nowePixele[x + i, y + j] == 2 || nowePixele[x + i, y + j] == 3)
                                        nowePixele[x, y] += maksaSprawdzajaca[1 + i, 1 + j];
                                }
                            }
                            if (wciecia.Contains(nowePixele[x, y]))
                            {
                                nowePixele[x, y] = 0;
                                zmiana = true;
                            }
                            else
                            {
                                nowePixele[x, y] = 1;
                            }
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 2)
                        {
                            nowePixele[x, y] = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (nowePixele[x + i, y + j] == 1 || nowePixele[x + i, y + j] == 4 || nowePixele[x + i, y + j] == 3 || nowePixele[x + i, y + j] == 2)
                                        nowePixele[x, y] += maksaSprawdzajaca[1 + i, 1 + j];
                                }
                            }
                            if (wciecia.Contains(nowePixele[x, y]))
                            {
                                nowePixele[x, y] = 0;
                                zmiana = true;
                            }
                            else
                                nowePixele[x, y] = 1;
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 3)
                        {
                            nowePixele[x, y] = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (nowePixele[x + i, y + j] == 3 || nowePixele[x + i, y + j] == 1 || nowePixele[x + i, y + j] == 2 || nowePixele[x + i, y + j] == 4)
                                        nowePixele[x, y] += maksaSprawdzajaca[1 + i, 1 + j];
                                }
                            }
                            if (wciecia.Contains(nowePixele[x, y]))
                            {
                                nowePixele[x, y] = 0;
                                zmiana = true;
                            }
                            else
                                nowePixele[x, y] = 1;
                        }
                    }
                }
            } while (zmiana == true);

            for (int x = dlugosc; x < b.Width - dlugosc; x++)
            {
                for (int y = dlugosc; y < b.Height - dlugosc; y++)
                {
                    if (nowePixele[x, y] == 0) nowePixele[x, y] = 255;
                    if (nowePixele[x, y] == 1) nowePixele[x, y] = 0;
                    b.SetPixel(x, y, Color.FromArgb(nowePixele[x, y], nowePixele[x, y], nowePixele[x, y]));
                }
            }
            obrazek.Source = BitmapToBitmapImage(b);
            obrazekPoSzkieletyzacji = BitmapToBitmapImage(b);
            rozgalezienia_przycisk.IsEnabled = true;

        }
        BitmapImage obrazekPoSzkieletyzacji;




        private void KMM2(Bitmap b)
        {
            int[] listaCzworek = { 3, 6, 7, 12, 14, 15, 24, 28, 30, 48, 56, 60, 96, 112, 120, 129, 131, 135, 192, 193, 195, 224, 225, 240 };
            int[,] maksaSprawdzajaca = { { 128, 64, 32 }, { 1, 0, 16 }, { 2, 4, 8 } };
            int[] maskaWyciec = { 3, 5, 7, 12, 13, 14, 15, 20, 21, 22, 23, 28, 29, 30, 31, 48, 52, 53, 54, 55, 56, 60, 61, 62, 63, 65, 67, 69, 71, 77, 79, 80, 81, 83, 84, 85, 86, 87, 88, 89, 91, 92, 93, 94, 95, 97, 99, 101, 103, 109, 111, 112, 113, 115, 116, 117, 118, 119, 120, 121, 123, 124, 125, 126, 127, 131, 133, 135, 141, 143, 149, 151, 157, 159, 181, 183, 189, 191, 192, 193, 195, 197, 199, 205, 207, 208, 209, 211, 212, 213, 214, 215, 216, 217, 219, 220, 221, 222, 223, 224, 225, 227, 229, 231, 237, 239, 240, 241, 243, 244, 245, 246, 247, 248, 249, 251, 252, 253, 254, 255 };
            List<int> czworki = new List<int>(listaCzworek);
            List<int> wciecia = new List<int>(maskaWyciec);

            int dlugosc = 1;
            int[,] nowePixele = new int[b.Width, b.Height];
            for (int x = dlugosc; x < b.Width - dlugosc; x++)
            {
                for (int y = dlugosc; y < b.Height - dlugosc; y++)
                {
                    Color koloryOb = b.GetPixel(x, y);
                    if (koloryOb.R == 0) nowePixele[x, y] = 1;
                    else nowePixele[x, y] = 0;
                }
            }
            Boolean zmiana = false;
            do
            {
                zmiana = false;
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 1)
                        {
                            if (nowePixele[x + 1, y] == 0 || nowePixele[x, y + 1] == 0 || nowePixele[x, y - 1] == 0 || nowePixele[x - 1, y] == 0)
                                nowePixele[x, y] = 2;
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 1)
                        {
                            if (nowePixele[x + 1, y + 1] == 0 || nowePixele[x - 1, y + 1] == 0 || nowePixele[x - 1, y - 1] == 0 || nowePixele[x + 1, y - 1] == 0)
                                nowePixele[x, y] = 3;
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 2)
                        {
                            nowePixele[x, y] = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (nowePixele[x + i, y + j] == 4 || nowePixele[x + i, y + j] == 1 || nowePixele[x + i, y + j] == 2 || nowePixele[x + i, y + j] == 3)
                                        nowePixele[x, y] += maksaSprawdzajaca[1 + i, 1 + j];
                                }
                            }
                            if (czworki.Contains(nowePixele[x, y]))
                            {
                                nowePixele[x, y] = 4;
                                //zmiana = true;
                            }
                            else
                                nowePixele[x, y] = 2;
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 4)
                        {
                            nowePixele[x, y] = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (nowePixele[x + i, y + j] == 4 || nowePixele[x + i, y + j] == 1 || nowePixele[x + i, y + j] == 2 || nowePixele[x + i, y + j] == 3)
                                        nowePixele[x, y] += maksaSprawdzajaca[1 + i, 1 + j];
                                }
                            }
                            if (wciecia.Contains(nowePixele[x, y]))
                            {
                                nowePixele[x, y] = 0;
                                zmiana = true;
                            }
                            else
                            {
                                nowePixele[x, y] = 1;
                            }
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 2)
                        {
                            nowePixele[x, y] = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (nowePixele[x + i, y + j] == 1 || nowePixele[x + i, y + j] == 4 || nowePixele[x + i, y + j] == 3 || nowePixele[x + i, y + j] == 2)
                                        nowePixele[x, y] += maksaSprawdzajaca[1 + i, 1 + j];
                                }
                            }
                            if (wciecia.Contains(nowePixele[x, y]))
                            {
                                nowePixele[x, y] = 0;
                                zmiana = true;
                            }
                            else
                                nowePixele[x, y] = 1;
                        }
                    }
                }
                for (int x = dlugosc; x < b.Width - dlugosc; x++)
                {
                    for (int y = dlugosc; y < b.Height - dlugosc; y++)
                    {
                        if (nowePixele[x, y] == 3)
                        {
                            nowePixele[x, y] = 0;
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (nowePixele[x + i, y + j] == 3 || nowePixele[x + i, y + j] == 1 || nowePixele[x + i, y + j] == 2 || nowePixele[x + i, y + j] == 4)
                                        nowePixele[x, y] += maksaSprawdzajaca[1 + i, 1 + j];
                                }
                            }
                            if (wciecia.Contains(nowePixele[x, y]))
                            {
                                nowePixele[x, y] = 0;
                                zmiana = true;
                            }
                            else
                                nowePixele[x, y] = 1;
                        }
                    }
                }
            } while (zmiana == true);

            for (int x = dlugosc; x < b.Width - dlugosc; x++)
            {
                for (int y = dlugosc; y < b.Height - dlugosc; y++)
                {
                    if (nowePixele[x, y] == 0) nowePixele[x, y] = 255;
                    if (nowePixele[x, y] == 1) nowePixele[x, y] = 0;
                    b.SetPixel(x, y, Color.FromArgb(nowePixele[x, y], nowePixele[x, y], nowePixele[x, y]));
                }
            }
            obrazek_3.Source = BitmapToBitmapImage(b);
            obrazekPoSzkieletyzacji2 = BitmapToBitmapImage(b);
            rozgalezienia_przycisk.IsEnabled = true;

        }
        BitmapImage obrazekPoSzkieletyzacji2;
        #endregion





        #region Minucje
        private void Rozwidlenia(object sender, RoutedEventArgs e) //szukanie minucji wywolanie po szkieletyzacji i bitmap
        {
            BitmapImage source = obrazekPoSzkieletyzacji;
            BitmapImage source2 = obrazekPoSzkieletyzacji2;
            Bitmap b = BitmapImageToBitmap(source);
            Bitmap b2 = BitmapImageToBitmap(source2);

            SzukanieMinucji(b);
            SzukanieMinucji2(b2);

        }

        private List<Point> SzukanieMinucji(Bitmap b)
        {
            int wr = 0;
            int wx = 0;
            int wy = 0;
            int wd = 0;
            Bitmap bb = b;
            int[,] minutiae = new int[b.Width, b.Height];
            int dlugosc = 2;
            int[,] cn = new int[b.Width, b.Height];
            int[,] nowePixele = new int[b.Width, b.Height];
            List<Point> minutiaePoints = new List<Point>();

            for (int x = dlugosc; x < b.Width - dlugosc; x++)
            {
                for (int y = dlugosc; y < b.Height - dlugosc; y++)
                {
                    Color koloryOb = b.GetPixel(x, y);
                    if (koloryOb.R == 0) nowePixele[x, y] = 1;
                    else nowePixele[x, y] = 0;
                }
            }
            for (int x = dlugosc; x < b.Width - dlugosc; x++)
            {
                for (int y = dlugosc; y < b.Height - dlugosc; y++)
                {
                    if (nowePixele[x, y] == 1)
                    {
                        cn[x, y] = ((Math.Abs(nowePixele[x, y + 1] - nowePixele[x - 1, y + 1]) + //1-2
                                     Math.Abs(nowePixele[x - 1, y + 1] - nowePixele[x - 1, y]) + //2-3
                                     Math.Abs(nowePixele[x - 1, y] - nowePixele[x - 1, y - 1]) + //3-4
                                     Math.Abs(nowePixele[x - 1, y - 1] - nowePixele[x, y - 1]) + //4-5
                                     Math.Abs(nowePixele[x, y - 1] - nowePixele[x + 1, y - 1]) + //5-6
                                     Math.Abs(nowePixele[x + 1, y - 1] - nowePixele[x + 1, y]) + //6-7
                                     Math.Abs(nowePixele[x + 1, y] - nowePixele[x + 1, y + 1]) + //7-8
                                     Math.Abs(nowePixele[x + 1, y + 1] - nowePixele[x, y + 1])) / 2); //8-1    

                        if (cn[x, y] == 0) // pojedyńczy punkt - minucja - różowy
                        {

                            wr++;
                            minutiaePoints.Add(new Point(x, y));

                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 2, y - 1, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 2, y, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 2, y + 1, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 2, y + 2, Color.FromArgb(255, 0, 255));

                            bb.SetPixel(x - 1, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 1, y + 2, Color.FromArgb(255, 0, 255));

                            bb.SetPixel(x, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x, y + 2, Color.FromArgb(255, 0, 255));

                            bb.SetPixel(x + 1, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 1, y + 2, Color.FromArgb(255, 0, 255));

                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 2, y - 1, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 2, y, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 2, y + 1, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 2, y + 2, Color.FromArgb(255, 0, 255));




                        }

                        if (cn[x, y] == 1) //zakończenie krawędzi - minucja - pomarańczowy
                        {
                            wx++;
                            minutiaePoints.Add(new Point(x, y));
                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 2, y - 1, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 2, y, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 2, y + 1, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 2, y + 2, Color.FromArgb(255, 140, 0));

                            bb.SetPixel(x - 1, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 1, y + 2, Color.FromArgb(255, 140, 0));

                            bb.SetPixel(x, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x, y + 2, Color.FromArgb(255, 140, 0));

                            bb.SetPixel(x + 1, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 1, y + 2, Color.FromArgb(255, 140, 0));

                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 2, y - 1, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 2, y, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 2, y + 1, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 2, y + 2, Color.FromArgb(255, 140, 0));

                        }
                        if (cn[x, y] == 3) //rozwidlenie - minucja - zielony
                        {
                            wy++;
                            minutiaePoints.Add(new Point(x, y));
                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 2, y, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 2, y + 1, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 2, y + 2, Color.FromArgb(0, 130, 0));

                            bb.SetPixel(x - 1, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 1, y + 2, Color.FromArgb(0, 130, 0));

                            bb.SetPixel(x, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x, y + 2, Color.FromArgb(0, 130, 0));

                            bb.SetPixel(x + 1, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 1, y + 2, Color.FromArgb(0, 130, 0));

                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 2, y, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 2, y + 1, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 2, y + 2, Color.FromArgb(0, 130, 0));

                        }
                        if (cn[x, y] == 4) // skrzyżowanie - minucja - niebieski
                        {
                            wd++;
                            minutiaePoints.Add(new Point(x, y));
                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 2, y - 1, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 2, y, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 2, y + 1, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 2, y + 2, Color.FromArgb(0, 0, 255));

                            bb.SetPixel(x - 1, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 1, y + 2, Color.FromArgb(0, 0, 255));

                            bb.SetPixel(x, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x, y + 2, Color.FromArgb(0, 0, 255));

                            bb.SetPixel(x + 1, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 1, y + 2, Color.FromArgb(0, 0, 255));

                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 2, y - 1, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 2, y, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 2, y + 1, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 2, y + 2, Color.FromArgb(0, 0, 255));

                        }
                    }
                }
            }
            obrazek.Source = BitmapToBitmapImage(bb);
            label1.Content = "Pojedynczych punktow(różowe): " + wr;
            label2.Content = "Zakończenia krawędzi(pomarańczowy): " + wx;
            label3.Content = "Rozwidlenia(zielony): " + wy;
            label4.Content = "Skrzyżowania(niebieski): " + wd;

            return minutiaePoints;
        }



        private List<Point> SzukanieMinucji2(Bitmap b)
        {
            int wr = 0;
            int wx = 0;
            int wy = 0;
            int wd = 0;
            Bitmap bb = b;
            int[,] minutiae = new int[b.Width, b.Height];
            int dlugosc = 2;
            int[,] cn = new int[b.Width, b.Height];
            int[,] nowePixele = new int[b.Width, b.Height];
            List<Point> minutiaePoints = new List<Point>();

            for (int x = dlugosc; x < b.Width - dlugosc; x++)
            {
                for (int y = dlugosc; y < b.Height - dlugosc; y++)
                {
                    Color koloryOb = b.GetPixel(x, y);
                    if (koloryOb.R == 0) nowePixele[x, y] = 1;
                    else nowePixele[x, y] = 0;
                }
            }
            for (int x = dlugosc; x < b.Width - dlugosc; x++)
            {
                for (int y = dlugosc; y < b.Height - dlugosc; y++)
                {
                    if (nowePixele[x, y] == 1)
                    {
                        cn[x, y] = ((Math.Abs(nowePixele[x, y + 1] - nowePixele[x - 1, y + 1]) + //1-2
                                     Math.Abs(nowePixele[x - 1, y + 1] - nowePixele[x - 1, y]) + //2-3
                                     Math.Abs(nowePixele[x - 1, y] - nowePixele[x - 1, y - 1]) + //3-4
                                     Math.Abs(nowePixele[x - 1, y - 1] - nowePixele[x, y - 1]) + //4-5
                                     Math.Abs(nowePixele[x, y - 1] - nowePixele[x + 1, y - 1]) + //5-6
                                     Math.Abs(nowePixele[x + 1, y - 1] - nowePixele[x + 1, y]) + //6-7
                                     Math.Abs(nowePixele[x + 1, y] - nowePixele[x + 1, y + 1]) + //7-8
                                     Math.Abs(nowePixele[x + 1, y + 1] - nowePixele[x, y + 1])) / 2); //8-1    

                        if (cn[x, y] == 0) // pojedyńczy punkt - minucja - różowy
                        {

                            wr++;
                            minutiaePoints.Add(new Point(x, y));

                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 2, y - 1, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 2, y, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 2, y + 1, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 2, y + 2, Color.FromArgb(255, 0, 255));

                            bb.SetPixel(x - 1, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x - 1, y + 2, Color.FromArgb(255, 0, 255));

                            bb.SetPixel(x, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x, y + 2, Color.FromArgb(255, 0, 255));

                            bb.SetPixel(x + 1, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 1, y + 2, Color.FromArgb(255, 0, 255));

                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 2, y - 1, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 2, y, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 2, y + 1, Color.FromArgb(255, 0, 255));
                            bb.SetPixel(x + 2, y + 2, Color.FromArgb(255, 0, 255));




                        }

                        if (cn[x, y] == 1) //zakończenie krawędzi - minucja - pomarańczowy
                        {
                            wx++;
                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 2, y - 1, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 2, y, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 2, y + 1, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 2, y + 2, Color.FromArgb(255, 140, 0));

                            bb.SetPixel(x - 1, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x - 1, y + 2, Color.FromArgb(255, 140, 0));

                            bb.SetPixel(x, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x, y + 2, Color.FromArgb(255, 140, 0));

                            bb.SetPixel(x + 1, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 1, y + 2, Color.FromArgb(255, 140, 0));

                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 2, y - 1, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 2, y, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 2, y + 1, Color.FromArgb(255, 140, 0));
                            bb.SetPixel(x + 2, y + 2, Color.FromArgb(255, 140, 0));
                            minutiaePoints.Add(new Point(x, y));
                        }
                        if (cn[x, y] == 3) //rozwidlenie - minucja - zielony
                        {
                            wy++;
                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 2, y, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 2, y + 1, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 2, y + 2, Color.FromArgb(0, 130, 0));

                            bb.SetPixel(x - 1, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x - 1, y + 2, Color.FromArgb(0, 130, 0));

                            bb.SetPixel(x, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x, y + 2, Color.FromArgb(0, 130, 0));

                            bb.SetPixel(x + 1, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 1, y + 2, Color.FromArgb(0, 130, 0));

                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 2, y, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 2, y + 1, Color.FromArgb(0, 130, 0));
                            bb.SetPixel(x + 2, y + 2, Color.FromArgb(0, 130, 0));
                            minutiaePoints.Add(new Point(x, y));
                        }
                        if (cn[x, y] == 4) // skrzyżowanie - minucja - niebieski
                        {
                            wd++;
                            bb.SetPixel(x - 2, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 2, y - 1, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 2, y, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 2, y + 1, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 2, y + 2, Color.FromArgb(0, 0, 255));

                            bb.SetPixel(x - 1, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x - 1, y + 2, Color.FromArgb(0, 0, 255));

                            bb.SetPixel(x, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x, y + 2, Color.FromArgb(0, 0, 255));

                            bb.SetPixel(x + 1, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 1, y + 2, Color.FromArgb(0, 0, 255));

                            bb.SetPixel(x + 2, y - 2, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 2, y - 1, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 2, y, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 2, y + 1, Color.FromArgb(0, 0, 255));
                            bb.SetPixel(x + 2, y + 2, Color.FromArgb(0, 0, 255));
                            minutiaePoints.Add(new Point(x, y));
                        }
                    }
                }
            }
            obrazek_3.Source = BitmapToBitmapImage(bb);
               labell1.Content = "Pojedynczych punktow(różowe): " + wr;
                 labell2.Content = "Zakończenia krawędzi(pomarańczowy): " + wx;
               labell3.Content = "Rozwidlenia(zielony): " + wy;
                labell4.Content = "Skrzyżowania(niebieski): " + wd;
            return minutiaePoints;
        }





        #endregion


        private double CompareFingerprints(List<Point> minutiae1, List<Point> minutiae2)
        {
            double similarity = 0;
            int matchedMinutiae = 0;
            double[] distances1 = new double[minutiae1.Count];
            double[] distances2 = new double[minutiae2.Count];
            double[,] distancesBetweenMinutiae1 = new double[minutiae1.Count, minutiae1.Count];
            double[,] distancesBetweenMinutiae2 = new double[minutiae2.Count, minutiae2.Count];

            // obliczanie odleglosci pomiedzy minucjami dla pierwszego odcisku
            for (int i = 0; i < minutiae1.Count; i++)
            {
                for (int j = i + 1; j < minutiae1.Count; j++)
                {
                    distancesBetweenMinutiae1[i, j] = Math.Sqrt(Math.Pow(minutiae1[i].X - minutiae1[j].X, 2) + Math.Pow(minutiae1[i].Y - minutiae1[j].Y, 2));
                    distancesBetweenMinutiae1[j, i] = distancesBetweenMinutiae1[i, j];
                }
            }

            // obliczanie odleglosci pomiedzy minucjami dla drugiego odcisku
            for (int i = 0; i < minutiae2.Count; i++)
            {
                for (int j = i + 1; j < minutiae2.Count; j++)
                {
                    distancesBetweenMinutiae2[i, j] = Math.Sqrt(Math.Pow(minutiae2[i].X - minutiae2[j].X, 2) + Math.Pow(minutiae2[i].Y - minutiae2[j].Y, 2));
                    distancesBetweenMinutiae2[j, i] = distancesBetweenMinutiae2[i, j];
                }
            }
            List<Point> matchedMinutiae1 = new List<Point>();
            List<Point> matchedMinutiae2 = new List<Point>();
            for (int i = 0; i < minutiae1.Count; i++)
            {
                for (int j = 0; j < minutiae2.Count; j++)
                {
                    if (!matchedMinutiae1.Contains(minutiae1[i]) && !matchedMinutiae2.Contains(minutiae2[j]))
                    {
                        double diff = Math.Abs(distancesBetweenMinutiae1[i, j] - distancesBetweenMinutiae2[i, j]);
                        if (diff < 10)
                        {
                            matchedMinutiae1.Add(minutiae1[i]);
                            matchedMinutiae2.Add(minutiae2[j]);
                            matchedMinutiae++;
                            break;
                        }
                    }
                }

            }
            // obliczanie podobieństwa odcisków
            similarity = (double)matchedMinutiae / (double)minutiae1.Count;
            return similarity;
        }




        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            obrazek.Source = obrazek_2.Source;
            obrazek_3.Source = obrazek_4.Source;
            rozgalezienia_przycisk.IsEnabled = false;

            label1.Content = "Pojedynczych punktow(różowe): ";
            label2.Content = "Zakończenia krawędzi(pomarańczowy): ";
            label3.Content = "Rozwidlenia(zielony): ";
            label4.Content = "Skrzyżowania(niebieski): ";
           
            labell1.Content = "Pojedynczych punktow(różowe): ";
            labell2.Content = "Zakończenia krawędzi(pomarańczowy): ";
            labell3.Content = "Rozwidlenia(zielony): ";
            labell4.Content = "Skrzyżowania(niebieski): ";
        }

        public string score;
        public string qry;
        public string temp;
        private void match(string query, string template)
        {
          

            var fingerprintImg1 = ImageLoader.LoadImage(query);
            var fingerprintImg2 = ImageLoader.LoadImage(template);

            var featExtractor = new PNFeatureExtractor() { MtiaExtractor = new Ratha1995MinutiaeExtractor() };
            var features1 = featExtractor.ExtractFeatures(fingerprintImg1);
            var features2 = featExtractor.ExtractFeatures(fingerprintImg2);

            var matcher = new PN();
            double similarity = matcher.Match(features1, features2);
            score = similarity.ToString("0.000");
            MessageBox.Show("Podobieństwo " + similarity.ToString("0.000"), "Wynik");

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //BitmapImage source = obrazekPoSzkieletyzacji;
            //BitmapImage source2 = obrazekPoSzkieletyzacji2;
            //Bitmap b = BitmapImageToBitmap(source);
            //Bitmap b2 = BitmapImageToBitmap(source2);
            //List<Point> minutiae1 = SzukanieMinucji(b);
            //List<Point> minutiae2 = SzukanieMinucji2(b2);
            //double similarity = CompareFingerprints(minutiae1, minutiae2);
            //similarity = Math.Round(similarity, 4);
            //label5.Content = "Podobieństwo(score): " + similarity ;
            match(qry, temp);


        }


    




    }

}

    







