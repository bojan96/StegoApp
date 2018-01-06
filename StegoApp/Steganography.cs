﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoApp
{
    public static class Steganography
    {

        public static void Embed(string srcFilename, string dstFilename, byte[] data)
        {

            if (!ValidImageFormat(srcFilename))
                throw new FileFormatException("Unsupported image format");

            using (Bitmap srcBmp = new Bitmap(srcFilename))
            {

                // Mul 2, because 2 nibbles per byte
                if ((data.Length + sizeof(int)) * 2 > srcBmp.Width * srcBmp.Height)
                    throw new FileFormatException("Image not large enough to store data");

                using (var dstBmp = new Bitmap
                    (srcBmp.Width, srcBmp.Height, PixelFormat.Format32bppArgb))
                {

                    EmbedLength(data.Length, dstBmp, srcBmp);

                    // Offset is 8 because we already processed 8 pixels
                    EmbedData(data, dstBmp, srcBmp, sizeof(int) * 2);

                    int processedNibbles = (data.Length + sizeof(int)) * 2;
                    int yI = processedNibbles / srcBmp.Width;
                    int xI = processedNibbles % srcBmp.Width;

                    // Copy rest of the pixels
                    for (; yI < srcBmp.Height; ++yI)
                    {

                        for (; xI < srcBmp.Width; ++xI)
                            dstBmp.SetPixel(xI, yI, srcBmp.GetPixel(xI, yI));

                        xI = 0;

                    }

                    dstBmp.Save(dstFilename, srcBmp.RawFormat);

                }

            }

        }

        static void EmbedLength(int length, Bitmap dstBmp, Bitmap srcBmp)
        {

            var bytes = BitConverter.GetBytes(length);
            EmbedData(bytes, dstBmp, srcBmp);
            
        }

        // Offset in pixels
        static void EmbedData(byte[] data, Bitmap dstBmp, Bitmap srcBmp, int offset = 0)
        {

            // Mul len by 2, because we operate on nibbles
            for (int i = 0; i < data.Length * 2; ++i)
            {

                int oneByte = data[i / 2];
                int y = (i + offset) / srcBmp.Width;
                int x = (i + offset) % srcBmp.Width;

                // Pixel which contains encoded nibble
                Color nibblePixel;

                var srcPixel = srcBmp.GetPixel(x, y);

                if (i % 2 == 0)
                    oneByte >>= 4;

                nibblePixel = EmbedNibble(oneByte, srcPixel);

                dstBmp.SetPixel(x, y, nibblePixel);

            }

        }

        // Encodes first four nibbles
        static Color EmbedNibble(int nibble, Color pixel)
        {

            int alpha = 0;
            int red = 0;
            int green = 0;
            int blue = 0;

            alpha = (pixel.A & ~1) | ((nibble & 0x08) >> 3);
            red = (pixel.R & ~1) | ((nibble & 0x04) >> 2);
            green = (pixel.G & ~1) | ((nibble & 0x02) >> 1);
            blue = (pixel.B & ~1) | nibble & 0x01;

            return Color.FromArgb(alpha, red, green, blue);

        }

        public static bool ValidImageFormat(string path)
        {

            using (var image = new Bitmap(path))
            {

                var format = image.RawFormat;
                return format.Equals(ImageFormat.Bmp) || format.Equals(ImageFormat.Png);

            }

        }

        public static byte[] Extract(string filename)
        {

            if (!ValidImageFormat(filename))
                throw new FileFormatException("Unsupported image format");

            using (var srcBmp = new Bitmap(filename))
            {
                
                //Minimum 4 bytes ( 8 nibbles or 8 pixels)
                if (srcBmp.Width * srcBmp.Height < sizeof(int) * 2)
                    throw new FileFormatException
                        ("Could not extract data from image, image is too short");

                int length = ExtractLength(srcBmp);

                if ( length <= 0 || ((length * 2) > (srcBmp.Width * srcBmp.Height - 8)))
                    throw new FileFormatException("Could not extract data from image");

                // Offset is 8 because we already processed 8 pixels
                var data = ExtractData(srcBmp, length, sizeof(int) * 2);

                return data;

            }

        }

        // Returns embeded data len
        static int ExtractLength(Bitmap srcBmp)
        {

            var data = ExtractData(srcBmp, sizeof(int));
            return BitConverter.ToInt32(data,0);

        }

        // offset is in pixels
        static byte[] ExtractData(Bitmap srcBmp, int length, int offset = 0)
        {

            // Array is zero initialized by default
            var data = new byte[length];

            // Mul 2, cuz 2 nibbles per byte
            for(int i = 0; i < data.Length * 2; ++i)
            {

                int y = (i + offset) / srcBmp.Width;
                int x = (i + offset) % srcBmp.Width;

                var nibblePixel = srcBmp.GetPixel(x, y);

                int nibble = ExtractNibble(nibblePixel);

                if (i % 2 == 0)
                    nibble <<= 4;

                data[i / 2] |= (byte)nibble;

            }

            return data;

        }
        static int ExtractNibble(Color pixel)
        {

            int nibble = 0;

            nibble |= (pixel.A & 0x01) << 3;
            nibble |= (pixel.R & 0x01) << 2;
            nibble |= (pixel.G & 0x01) << 1;
            nibble |= (pixel.B & 0x01);

            return nibble;

        }

    }
}
