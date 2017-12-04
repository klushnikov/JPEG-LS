using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_LS
{
    class ReadImageFile
    {
        public struct BITMAPFILEHEADER
        {
            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        };

        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        };

        public readonly BITMAPFILEHEADER bmpheader = new BITMAPFILEHEADER();
        public readonly BITMAPINFOHEADER bmpinfo = new BITMAPINFOHEADER();
        public int count = 0;
        public readonly byte[] data;

        public ReadImageFile(FileStream fileIn)
        {
            BinaryReader strIn = new BinaryReader(fileIn);

            data = strIn.ReadBytes(Convert.ToInt32(fileIn.Length));

            strIn.Close();

            bmpheader.bfType = ReadInt16(data[count], data[++count]);
            bmpheader.bfSize = ReadInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpheader.bfReserved1 = ReadInt16(data[++count], data[++count]);
            bmpheader.bfReserved2 = ReadInt16(data[++count], data[++count]);
            bmpheader.bfOffBits = ReadInt32(data[++count], data[++count], data[++count], data[++count]);

            bmpinfo.biSize = ReadUInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpinfo.biWidth = ReadInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpinfo.biHeight = ReadInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpinfo.biPlanes = ReadUInt16(data[++count], data[++count]);
            bmpinfo.biBitCount = ReadUInt16(data[++count], data[++count]);
            bmpinfo.biCompression = ReadUInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpinfo.biSizeImage = ReadUInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpinfo.biXPelsPerMeter = ReadInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpinfo.biYPelsPerMeter = ReadInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpinfo.biClrUsed = ReadUInt32(data[++count], data[++count], data[++count], data[++count]);
            bmpinfo.biClrImportant = ReadUInt32(data[++count], data[++count], data[++count], data[++count]);


        }

        private UInt16 ReadUInt16(byte a, byte b)
        {
            return Convert.ToUInt16((b << 8) | a);
        }

        private Int16 ReadInt16(byte a, byte b)
        {
            return Convert.ToInt16((b << 8) | a);
        }

        private UInt32 ReadUInt32(byte a, byte b, byte c, byte d)
        {
            return Convert.ToUInt32((((((d << 8) | c) << 8) | b) << 8) | a);
        }

        private Int32 ReadInt32(byte a, byte b, byte c, byte d)
        {
            return Convert.ToInt32((((((d << 8) | c) << 8) | b) << 8) | a);
        }
    }
}
