﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace png2bmp32
{
   /// <summary>
   /// Image converter class.
   /// </summary>
   class ImageConverter
   {
      public const float InchesPerMeter = 39.3700787f;

      /// <summary>
      /// …
      /// </summary>
      /// <param name="strInputPath">…</param>
      internal static void Convert(string strInputPath)
      {
         string strOutputPath = Path.GetDirectoryName(strInputPath) + "\\" + Path.GetFileNameWithoutExtension(strInputPath) + ".bmp";

         // Load source image data and check if it is a png
         byte[] inputData = File.ReadAllBytes(strInputPath);
         if (!ImageTools.IsPNG(inputData)) throw new Exception(Properties.Resource.strNoPNGData);

         // Get a Bitmap object from the source image data
         Image imgInput;
         using (var inputDataStream = new MemoryStream(inputData))
         {
            imgInput = Bitmap.FromStream(inputDataStream);
         }
         Bitmap bmpInput = new Bitmap(imgInput);

         using (var output = new MemoryStream())
         {
            // Write BMP file and info headers
            WriteBMPHeaders(output, bmpInput);

            if (imgInput.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
               // Copy image data line by line:
               Convert32bppSource(output, bmpInput);
            }
            else
            {
               // Copy image data pixel by pixel:
               ConvertNon32bppSource(output, bmpInput);
            }

            // Write generated 32bit BMP data to file:
            using (FileStream file = File.OpenWrite(strOutputPath)) output.WriteTo(file);
         }
      }

      /// <summary>
      /// …
      /// </summary>
      /// <param name="output">…</param>
      /// <param name="bmpInput">…</param>
      private static void ConvertNon32bppSource(MemoryStream output, Bitmap bmpInput)
      {
         int nWidth = bmpInput.Width;
         int nHeight = bmpInput.Height;

         for (int y = nHeight - 1; y >= 0; --y)
         {
            for (int x = 0; x < nWidth; ++x)
            {
               Color c = bmpInput.GetPixel(x, y);
               output.WriteByte(c.B);
               output.WriteByte(c.G);
               output.WriteByte(c.R);
               output.WriteByte(c.A);
            }
         }
      }

      /// <summary>
      /// …
      /// </summary>
      /// <param name="output">…</param>
      /// <param name="bmpInput">…</param>
      private static void Convert32bppSource(MemoryStream output, Bitmap bmpInput)
      {
         BitmapData bmpDataInput = null;
         int nWidth = bmpInput.Width;
         int nHeight = bmpInput.Height;

         try
         {
            bmpDataInput = bmpInput.LockBits(
               new Rectangle(0, 0, nWidth, nHeight),
               System.Drawing.Imaging.ImageLockMode.ReadOnly,
               System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int nStride = bmpDataInput.Stride;
            byte[] line = new byte[Math.Abs(nStride)];

            if (nStride > 0)     // Bottom up image
            {
               IntPtr ptr = new IntPtr((long)bmpDataInput.Scan0 + (nHeight - 1) * nStride);
               for (int i = 0; i < nHeight; ++i)
               {
                  Marshal.Copy(ptr, line, 0, line.Length);
                  output.Write(line, 0, line.Length);
                  ptr = new IntPtr((long)ptr - nStride);
               }
            }
            else                 // Top down image
            {
               IntPtr ptr = bmpDataInput.Scan0;
               for (int i = 0; i < nHeight; ++i)
               {
                  Marshal.Copy(ptr, line, 0, line.Length);
                  output.Write(line, 0, line.Length);
                  ptr = new IntPtr((long)ptr + nStride);
               }
            }
         }
         finally
         {
            if (bmpDataInput != null) bmpInput.UnlockBits(bmpDataInput);
         }
      }

      /// <summary>
      /// …
      /// </summary>
      /// <param name="output">…</param>
      /// <param name="bmpInput">…</param>
      private static void WriteBMPHeaders(MemoryStream output, Bitmap bmpInput)
      {
         int nWidth = bmpInput.Width;
         int nHeight = bmpInput.Height;

         BitmapFileHeader hdrFile = new BitmapFileHeader();
         BitmapInfoHeader hdrBmpInf = new BitmapInfoHeader();
         hdrFile.Init();
         hdrBmpInf.Init();

         // Get some values in advance:

         UInt32 uImageSize = (UInt32)(nWidth * nHeight);
         UInt32 uFileSize = (UInt32)(hdrFile.bfOffBits + uImageSize);
         UInt32 uXPelsPerMeter = (UInt32)Math.Round(bmpInput.HorizontalResolution * InchesPerMeter, 0);
         UInt32 uYPelsPerMeter = (UInt32)Math.Round(bmpInput.VerticalResolution * InchesPerMeter, 0);

         // Set file header values:

         hdrFile.bfSize = uFileSize;

         // Set image header values:

         hdrBmpInf.biWidth = nWidth;
         hdrBmpInf.biHeight = nHeight;
         hdrBmpInf.biPlanes = 1;
         hdrBmpInf.biBitCount = 32;
         hdrBmpInf.biCompression = 0;
         hdrBmpInf.biSizeImage = uImageSize;
         hdrBmpInf.biXPelsPerMeter = (int)uXPelsPerMeter;
         hdrBmpInf.biYPelsPerMeter = (int)uYPelsPerMeter;
         hdrBmpInf.biClrUsed = hdrBmpInf.biClrImportant = 0;

         // Write headers:

         byte[] dataHdrFile = StructureToByteArray(hdrFile);
         byte[] dataHdrBmpInf = StructureToByteArray(hdrBmpInf);

         output.Write(dataHdrFile, 0, dataHdrFile.Length);
         output.Write(dataHdrBmpInf, 0, dataHdrBmpInf.Length);
      }

      /// <summary>
      /// …
      /// </summary>
      /// <param name="structure">…</param>
      /// <returns>…</returns>
      internal static byte[] StructureToByteArray(object structure)
      {
         int nLen = Marshal.SizeOf(structure);
         byte[] data = new byte[nLen];

         IntPtr pData = Marshal.AllocHGlobal(nLen);

         Marshal.StructureToPtr(structure, pData, true);
         Marshal.Copy(pData, data, 0, nLen);
         Marshal.FreeHGlobal(pData);

         return data;
      }
   }
}
