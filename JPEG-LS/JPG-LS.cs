using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JPEG_LS
{
    class JPG_LS
    {
        static void Main(string[] args)
        {
            for (;;)
            {
                Console.WriteLine("1 - BMP to JLS, 2 - JLS to BMP, 3 - PSNR, - - Exit");

                int.TryParse(Console.ReadLine(), out int point);

                switch (point)
                {
                    case 1:
                        BmpToJls();
                        break;

                    case 2:
                        JlsToBmp();
                        break;

                    case 3:
                        PSNR();
                        break;

                    default:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        static void BmpToJls()
        {
            Console.WriteLine("The name of the source file bmp");

            FileStream fileIn = new FileStream(Console.ReadLine() + ".bmp", FileMode.Open);
            ReadImageFile bmp = new ReadImageFile(fileIn);
            fileIn.Close();

            Console.WriteLine("The name of the output file jls");
            FileStream fileOut = new FileStream(Console.ReadLine() + ".jls", FileMode.Create);
            BinaryWriter strOut = new BinaryWriter(fileOut);

            strOut.Write(bmp.bmpheader.bfType);
            strOut.Write(bmp.bmpheader.bfSize);
            strOut.Write(bmp.bmpheader.bfReserved1);
            strOut.Write(bmp.bmpheader.bfReserved2);
            strOut.Write(bmp.bmpheader.bfOffBits);

            strOut.Write(bmp.bmpinfo.biSize);
            strOut.Write(bmp.bmpinfo.biWidth);
            strOut.Write(bmp.bmpinfo.biHeight);
            strOut.Write(bmp.bmpinfo.biPlanes);
            strOut.Write(bmp.bmpinfo.biBitCount);
            strOut.Write(bmp.bmpinfo.biCompression);
            strOut.Write(bmp.bmpinfo.biSizeImage);
            strOut.Write(bmp.bmpinfo.biXPelsPerMeter);
            strOut.Write(bmp.bmpinfo.biYPelsPerMeter);
            strOut.Write(bmp.bmpinfo.biClrUsed);
            strOut.Write(bmp.bmpinfo.biClrImportant);

            Compress compress = new Compress();

            compress.Compressing(bmp.data, ++(bmp.count), strOut, bmp.bmpinfo.biHeight, bmp.bmpinfo.biWidth);
            compress.Compressing(bmp.data, ++(bmp.count), strOut, bmp.bmpinfo.biHeight, bmp.bmpinfo.biWidth);
            compress.Compressing(bmp.data, ++(bmp.count), strOut, bmp.bmpinfo.biHeight, bmp.bmpinfo.biWidth);
            compress.Write(strOut);

            strOut.Close();
            fileOut.Close();
        }

        static void JlsToBmp()
        {
            Console.WriteLine("The name of the source file jls");

            FileStream fileIn = new FileStream(Console.ReadLine() + ".jls", FileMode.Open);
            ReadImageFile jls = new ReadImageFile(fileIn);
            fileIn.Close();

            byte[] data_Byte = new byte[jls.data.Length - jls.count - 1];

            Array.Copy(jls.data, ++(jls.count), data_Byte, 0, jls.data.Length - jls.count);

            List<bool> data = new List<bool> { };

            for (int i = 0; i < data_Byte.Length; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    data.Add(Convert.ToBoolean((data_Byte[i] >> j) & 1));
                }
            }
            int index = 0;

            Decompress decompress = new Decompress();

            double[,] blue = decompress.Decompressing(data, ref index, jls.bmpinfo.biHeight, jls.bmpinfo.biWidth);
            double[,] green = decompress.Decompressing(data, ref index, jls.bmpinfo.biHeight, jls.bmpinfo.biWidth);
            double[,] red = decompress.Decompressing(data, ref index, jls.bmpinfo.biHeight, jls.bmpinfo.biWidth);

            Console.WriteLine("The name of the output file bmp");
            FileStream fileOut = new FileStream(Console.ReadLine() + ".bmp", FileMode.Create);
            BinaryWriter strOut = new BinaryWriter(fileOut);

            strOut.Write(jls.bmpheader.bfType);
            strOut.Write(jls.bmpheader.bfSize);
            strOut.Write(jls.bmpheader.bfReserved1);
            strOut.Write(jls.bmpheader.bfReserved2);
            strOut.Write(jls.bmpheader.bfOffBits);

            strOut.Write(jls.bmpinfo.biSize);
            strOut.Write(jls.bmpinfo.biWidth);
            strOut.Write(jls.bmpinfo.biHeight);
            strOut.Write(jls.bmpinfo.biPlanes);
            strOut.Write(jls.bmpinfo.biBitCount);
            strOut.Write(jls.bmpinfo.biCompression);
            strOut.Write(jls.bmpinfo.biSizeImage);
            strOut.Write(jls.bmpinfo.biXPelsPerMeter);
            strOut.Write(jls.bmpinfo.biYPelsPerMeter);
            strOut.Write(jls.bmpinfo.biClrUsed);
            strOut.Write(jls.bmpinfo.biClrImportant);

            for (int i = 0; i < jls.bmpinfo.biHeight; i++)
            {
                for (int j = 0; j < jls.bmpinfo.biWidth; j++)
                {
                    strOut.Write(Convert.ToByte(blue[i, j]));
                    strOut.Write(Convert.ToByte(green[i, j]));
                    strOut.Write(Convert.ToByte(red[i, j]));
                }
            }

            strOut.Close();
            fileOut.Close();
        }



        static void PSNR()
        {
            Console.WriteLine("PSNR for two bmp");

            Console.WriteLine("Source bmp");
            FileStream fileSource = new FileStream(Console.ReadLine() + ".bmp", FileMode.Open);
            ReadImageFile bmpSource = new ReadImageFile(fileSource);
            fileSource.Close();

            Console.WriteLine("Restored bmp");
            FileStream fileRestored = new FileStream(Console.ReadLine() + ".bmp", FileMode.Open);
            ReadImageFile bmpRestored = new ReadImageFile(fileRestored);
            fileRestored.Close();

            double psnrBlue = 0, psnrGreen = 0, psnrRead = 0;

            try
            {
                for (int i = 0; i < bmpSource.bmpinfo.biHeight * bmpSource.bmpinfo.biWidth; i++)
                {
                    psnrBlue += Math.Pow((bmpSource.data[++bmpSource.count] - bmpRestored.data[++bmpRestored.count]), 2);
                    psnrGreen += Math.Pow((bmpSource.data[++bmpSource.count] - bmpRestored.data[++bmpRestored.count]), 2);
                    psnrRead += Math.Pow((bmpSource.data[++bmpSource.count] - bmpRestored.data[++bmpRestored.count]), 2);
                }

                Console.WriteLine();
                Console.WriteLine("PSNR Blue    - " + 10 * Math.Log10((bmpSource.bmpinfo.biHeight * bmpSource.bmpinfo.biWidth * Math.Pow((Math.Pow(2, 8) - 1), 2)) / psnrBlue));
                Console.WriteLine("PSNR Green   - " + 10 * Math.Log10((bmpSource.bmpinfo.biHeight * bmpSource.bmpinfo.biWidth * Math.Pow((Math.Pow(2, 8) - 1), 2)) / psnrGreen));
                Console.WriteLine("PSNR Red     - " + 10 * Math.Log10((bmpSource.bmpinfo.biHeight * bmpSource.bmpinfo.biWidth * Math.Pow((Math.Pow(2, 8) - 1), 2)) / psnrRead));
                Console.WriteLine();
            }
            catch(Exception e)
            {
                StackTrace st = new StackTrace(e, true);

                Console.WriteLine(DateTime.Now + " \nException: " + e + "\n" + st);
            }
        }
    }
}
