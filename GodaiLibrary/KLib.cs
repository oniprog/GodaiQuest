using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GodaiLibrary
{
    public static class KLib
    {
        private static String gLoadedFilePath;

        public static String getFileLoadedFilePath()
        {
            return gLoadedFilePath;
        }

        public static Image loadAndResizeImage(int nResizeWidth, int nResizeHeight)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "all files|*.*";
            if (dlg.ShowDialog() != DialogResult.OK)
                return null;

            Image imgSrc;
            try
            {
                imgSrc = System.Drawing.Image.FromFile(dlg.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("ファイルが読み込めませんでした");
                return null;
            }

            Bitmap resizeBitmap = new Bitmap(nResizeWidth, nResizeHeight);
            Graphics g = Graphics.FromImage(resizeBitmap);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            int nWidth = nResizeWidth, nHeight = nResizeHeight;
            double dRatio = imgSrc.Width / (0.0 + imgSrc.Height);
            if (dRatio > 1.0)
            {
                nHeight = (int)(nResizeWidth / dRatio);
            }
            else
            {
                nWidth = (int)(nResizeHeight * dRatio);
            }
            g.Clear(Color.Gray);
            g.DrawImage(imgSrc, (nResizeWidth - nWidth) / 2, (nResizeHeight - nHeight) / 2, nWidth, nHeight);
            g.Dispose();

            // 保存しておく
            gLoadedFilePath = dlg.FileName;

            return resizeBitmap;
        }
    }
}
