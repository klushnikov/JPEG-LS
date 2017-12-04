using System;
using System.Collections.Generic;
using System.IO;

namespace JPEG_LS
{
    public class Other
    {  
        public double NEAR = 0;
        public double MAXVAL = 255;
        public double RESET = 64;

        public int nContexts = 365;
        public int MIN_C = -128;
        public int MAX_C = 127;

        public int[] K = new int[10];
        public int[] bit = new int[35];
        public int[,] RUNIndex = new int[512, 512];

        public byte counter = 8, buffer;

        public void Write(BinaryWriter stream, bool count)
        {
            buffer += Convert.ToByte(Convert.ToInt32(count) << --counter);

            if (counter == 0)
            {
                stream.Write(buffer);

                counter = 8;
                buffer = 0;
            }
        }

        public void Clipping(ref double a)
        {
            if (a < 0)
            {
                a = 0;
            }
            else if (a > 255)
            {
                a = 255;
            }

            a = Math.Round(a);
        }

        public void Clipping(ref double a, double MAX)
        {
            if (a > MAX)
            {
                a = MAX;
            }
            else if (a < 0)
            {
                a = 0;
            }
        }

        public double Clamp(double i, double j)
        {
            // Figure C.3 – Clamping functions for default thresholds
            if (i > MAXVAL || i < j)
            {
                return j;
            }
            else
            {
                return i;
            }
        }

        public double DetermineGolombParameter(double n, double a)
        {
            int k;

            // Code segment A.10 – Computation of the Golomg coding variable k
            for (k = 0; ((int)n << k) < a; k++)
            {

            }

            return k;
        }

        public void EncodeGolomb(double k, double glimit, double qbpp_g, double value, ref BinaryWriter stream)
        {
            K[(int)k]++;

            double limit = glimit - qbpp_g - 1,
                    unarycode = (int)value >> (int)k;

            if (unarycode < limit)
            {

                bit[(int)(unarycode + 1 + k)]++;

                while (Convert.ToBoolean(unarycode--))
                {
                    Write(stream, false);
                }
                Write(stream, true);

                while (Convert.ToBoolean(k--))
                {
                    Write(stream, Convert.ToBoolean(((int)value >> (int)k) & 1));
                }
            }
            else
            {
                bit[(int)(limit + 1 + qbpp_g)]++;

                while (Convert.ToBoolean(limit--))
                {
                    Write(stream, false);
                }
                Write(stream, true);

                while (Convert.ToBoolean(qbpp_g--))
                {
                    Write(stream, Convert.ToBoolean(((int)value >> (int)qbpp_g) & 1));
                }
            }
        }

        public void DecodeGolomb(double k, double glimit, double qbpp_g, ref double value, List<bool> data, ref int count)
        {
            double limit = glimit - qbpp_g - 1,
                    unarycode = 0,
                    bitstoread;

            while (data[count] == false)
            {
                unarycode++;
                count++;
            }
            count++;

            if (unarycode < limit)
            {
                value = unarycode;
                bitstoread = k;
            }
            else
            {
                value = 0;
                bitstoread = qbpp_g;
            }

            while (Convert.ToBoolean(bitstoread--))
            {
                value = ((int)value << 1) + Convert.ToInt32(data[count]);
                count++;
            }
        }

        public int LocalGradientQuantization(double D)
        {
            // Table C.3 – Default threshold values in case MAXVAL = 255, NEAR = 0
            double BASIC_T1 = 3, BASIC_T2 = 7, BASIC_T3 = 21, T1, T2, T3;

            if (MAXVAL >= 128)
            {
                double FACTOR = Math.Ceiling(Convert.ToDouble((Math.Min(MAXVAL, 4095) + 128) / 256));

                // Figure C.4 – Default values in case MAXVAL ≥ 128
                T1 = Clamp(FACTOR * (BASIC_T1 - 2) + 2 + 3 * NEAR, NEAR + 1);
                T2 = Clamp(FACTOR * (BASIC_T2 - 2) + 2 + 5 * NEAR, BASIC_T1);
                T3 = Clamp(FACTOR * (BASIC_T3 - 2) + 2 + 7 * NEAR, BASIC_T2);
            }
            else
            {
                double FACTOR = Math.Ceiling(Convert.ToDouble(256 / (MAXVAL + 1)));

                // Figure C.5 – Default values in case MAXVAL < 128
                T1 = Clamp(Math.Max(2, Math.Floor(BASIC_T1 / FACTOR)) + 3 * NEAR, NEAR + 1);
                T2 = Clamp(Math.Max(3, Math.Floor(BASIC_T2 / FACTOR)) + 5 * NEAR, BASIC_T1);
                T3 = Clamp(Math.Max(4, Math.Floor(BASIC_T3 / FACTOR)) + 7 * NEAR, BASIC_T2);
            }

            // Code segment A.4 – Quantization of the gradients
            if (D <= -T3) return -4;
            else if (D <= -T2) return -3;
            else if (D <= -T1) return -2;
            else if (D < -NEAR) return -1;
            else if (D <= NEAR) return 0;
            else if (D < T1) return 1;
            else if (D < T2) return 2;
            else if (D < T3) return 3;
            else return 4;
        }
    }
}