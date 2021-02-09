using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace ImageColor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((VisualBrush)rct.Fill).Visual = Img;
        }
        private static unsafe Bitmap ReplaceColor(Bitmap source, Color toReplace, Color replacement, int threshold)
        {
            const int pixelSize = 4; // 32 bits per pixel

            Bitmap target = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

            BitmapData sourceData = null, targetData = null;

            try
            {
                sourceData = source.LockBits(
                  new Rectangle(0, 0, source.Width, source.Height),
                  ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                targetData = target.LockBits(
                  new Rectangle(0, 0, target.Width, target.Height),
                  ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                for (int y = 0; y < source.Height; ++y)
                {
                    byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                    byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

                    for (int x = 0; x < source.Width; ++x)
                    {
                        byte b = sourceRow[(x * pixelSize) + 0];
                        byte g = sourceRow[(x * pixelSize) + 1];
                        byte r = sourceRow[(x * pixelSize) + 2];
                        byte a = sourceRow[(x * pixelSize) + 3];

                        if (toReplace.R - threshold <= r && toReplace.G - threshold <= g && toReplace.B - threshold <= b)
                        {
                            r = replacement.R;
                            g = replacement.G;
                            b = replacement.B;
                        }

                        targetRow[(x * pixelSize) + 0] = b;
                        targetRow[(x * pixelSize) + 1] = g;
                        targetRow[(x * pixelSize) + 2] = r;
                        targetRow[(x * pixelSize) + 3] = a;
                    }
                }
            }
            finally
            {
                if (sourceData != null)
                {
                    source.UnlockBits(sourceData);
                }

                if (targetData != null)
                {
                    target.UnlockBits(targetData);
                }
            }

            return target;
        }
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);
        public static BitmapSource GetImageStream(System.Drawing.Image img)
        {
            Bitmap bitmap = new Bitmap(img);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }
        private OpenFileDialog openFileDialog;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog { Multiselect = false, Filter = "Image Files(*.PNG; *.JPG; *.GIF)| *.PNG; *.JPG; *.GIF | All files(*.*) | *.*"};
            if (openFileDialog.ShowDialog() == true)
            {
                using (Bitmap bitmap = new Bitmap(openFileDialog.FileName))
                {
                    Img.Source = GetImageStream(bitmap);
                }
            }
        }
        private System.Windows.Media.Color GeçiciRenk { get; set; }

        private System.Windows.Media.Color Renk { get; set; }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            using (Bitmap bitmap = ReplaceColor(new Bitmap(openFileDialog.FileName), Color.FromArgb(Renk.R, Renk.G, Renk.B), System.Drawing.Color.White, (int)Sld.Value))
            {
                Img2.Source = GetImageStream(bitmap);
            }
        }
        private byte[] pixels;

        private void Img_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                CroppedBitmap cb = new CroppedBitmap(Img.Source as BitmapSource, new Int32Rect((int)((int)e.GetPosition(Img).X * ((BitmapSource)Img.Source).PixelWidth / Img.ActualWidth), (int)((int)e.GetPosition(Img).Y * ((BitmapSource)Img.Source).PixelWidth / Img.ActualWidth), 1, 1));
                pixels = new byte[4];
                try
                {
                    cb.CopyPixels(pixels, 4, 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                GeçiciRenk = System.Windows.Media.Color.FromRgb(pixels[2], pixels[1], pixels[0]);
            }
            catch (Exception)
            {
            }
        }

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Renk = GeçiciRenk;
        }

        private void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            VisualBrush b = (VisualBrush)rct.Fill;
            System.Windows.Point pos = e.MouseDevice.GetPosition(this);
            Rect viewBox = b.Viewbox;
            double xoffset = viewBox.Width / 2.0;
            double yoffset = viewBox.Height / 2.0;
            viewBox.X = pos.X - xoffset;
            viewBox.Y = pos.Y - yoffset;
            b.Viewbox = viewBox;
            Canvas.SetLeft(cnv, pos.X - (rct.Width / 2));
            Canvas.SetTop(cnv, pos.Y - (rct.Height / 2));
        }
    }
}
