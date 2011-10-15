using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.google.zxing.qrcode;
using com.google.zxing.common;
using System.Drawing;
using com.google.zxing;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    /// <summary>
    /// Helper class for QR Barcodes
    /// </summary>
    public class QRCodeGenerator
    {
        /// <summary>
        /// Encodes the given message String as QR Barcode and returns the bitmap
        /// </summary>
        /// <param name="_message">Message that should be encoded</param>
        /// <returns>Bitmap that contains the QR Barcode</returns>
        internal static Bitmap Generate(string _message)
        {
            QRCodeWriter writer = new QRCodeWriter();
            ByteMatrix matrix = writer.encode(_message,
                BarcodeFormat.QR_CODE, 240, 240, null);
            sbyte[][] img = matrix.Array;
            Bitmap bmp = new Bitmap(matrix.Width, matrix.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            for (int i = 0; i <= img.Length - 1; i++)
            {
                for (int j = 0; j <= img[i].Length - 1; j++)
                {
                    if (img[i][j] == 0)
                    {
                        g.FillRectangle(Brushes.Black, j, i, 1, 1);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.White, j, i, 1, 1);
                    }
                }
            }
            return bmp;
        }
    }
}
