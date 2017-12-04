using System;
using System.Collections.Generic;
using System.IO;

namespace JPEG_LS
{
    public class Compress : Other
    {
        public void Compressing(byte[] data, int index, BinaryWriter stream, int height, int width)
        {
            // A.2 Initialisations and conventions
            // A.2.1 Initialisations
            // 1)
            double RANGE = Math.Ceiling((MAXVAL + 2 * NEAR) / (2 * NEAR + 1)) + 1;
            double qbpp = Math.Floor(Math.Log(RANGE, 2));
            double bpp = Math.Max(2, Math.Floor(Math.Log(MAXVAL + 1, 2)));
            double LIMIT = 2 * (bpp + Math.Max(8, bpp));

            // 2)
            double[] N = new double[nContexts + 2];
            double[] A = new double[nContexts + 2];
            double[] B = new double[nContexts];
            double[] C = new double[nContexts];

            for (int i = 0; i < nContexts + 2; i++)
            {
                N[i] = 1;
                A[i] = Math.Max(2, Math.Ceiling((RANGE + Math.Pow(2, 5)) / Math.Pow(2, 6)));
            }

            // 3)
            int[] J = { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            int RUNIndex = 0;

            // 4)
            double[] Nn = new double[2];

            double[] arr_Errval = new double[512];
            double[] arr_MErrval = new double[512];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // A.3 Context determination
                    double Ra, Rb, Rc, Rd, Ix = data[index];

                    Ra = (j == 0) ? 0 : data[index - 3];
                    Rb = (i == 0 || j == 0) ? 0 : data[index - (3 * width) - 3];
                    Rc = (i == 0) ? 0 : data[index - (3 * width)];
                    Rd = (i == 0 || j >= width - 1) ? 0 : data[index - (3 * width) + 3];

                    index += 3;

                    // A.3.1 Local gradient computation
                    // Code segment A.1 – Local gradient computation for context determination
                    double D1 = Rd - Rb,
                            D2 = Rb - Rc,
                            D3 = Rc - Ra;

                    // A.3.2 Mode selection
                    // Code segment A.2 – Mode selection procedure
                    if (Math.Abs(D1) <= NEAR && Math.Abs(D2) <= NEAR && Math.Abs(D3) <= NEAR)
                    {
                        #region goto RunModeProcessing

                        // A.7 Run mode
                        // A.7.1 Run scanning and run-length coding
                        // A.7.1.1 Run scanning

                        //Code segment A.14 – Run - length determination for run mode
                        double RUNval = Ra,
                                RUNcnt = 0,
                                Rx = 0,
                                EOLine = 0;

                        while (Math.Abs(Ix - RUNval) <= NEAR)
                        {
                            ++RUNcnt;
                            Rx = RUNval;

                            if (j >= width - 2)
                            {
                                EOLine = 1;
                                break;
                            }
                            else
                            {
                                // GetNextSample () 
                                j++;
                                Ra = (j == 0) ? 0 : data[index - 3];
                                Rb = (i == 0 || j == 0) ? 0 : data[index - (3 * width) - 3];
                                Rc = (i == 0) ? 0 : data[index - (3 * width)];
                                Rd = (i == 0 || j >= width - 1) ? 0 : data[index - (3 * width) + 3];
                                Ix = data[index];
                                index += 3;
                            }
                        }

                        // A.7.1.2 Run-length coding
                        //Code segment A.15 – Encoding of run segments of length rg

                        int rm;

                        while (RUNcnt >= (rm = J[RUNIndex]))
                        {
                            Write(stream, true);
                            RUNcnt = RUNcnt - rm;
                            if (RUNIndex < 31)
                            {
                                ++RUNIndex;
                            }
                        }

                        // Code segment A.16 – Encoding of run segments of length less than rg
                        if (Math.Abs(Ix - RUNval) > NEAR)
                        {
                            Write(stream, false);

                            int bits = J[RUNIndex],
                                    value = (int)RUNcnt;

                            while (Convert.ToBoolean(bits--))
                            {
                                bool bit = Convert.ToBoolean((value >> bits) & 1);

                                Write(stream, bit);
                            }

                            if (RUNIndex > 0)
                            {
                                --RUNIndex;
                            }
                        }
                        else if (RUNcnt > 0)
                        {
                            Write(stream, true);
                        }

                        // A.7.2 Run interruption sample encoding
                        if (EOLine != 1)
                        {
                            // Code segment A.17 – Index computation
                            double RItype;

                            if (Math.Abs(Ra - Rb) <= NEAR)
                            {
                                RItype = 1;
                            }
                            else
                            {
                                RItype = 0;
                            }

                            // Code segment A.18 – Prediction error for a run interruption sample
                            double Px;

                            if (RItype == 1)
                            {
                                Px = Ra;
                            }
                            else
                            {
                                Px = Rb;
                            }

                            double Errval = Ix - Px, SIGN;

                            // Code segment A.19 – Error computation for a run interruption sample
                            if ((RItype == 0) && (Ra > Rb))
                            {
                                Errval = -Errval;
                                SIGN = -1;
                            }
                            else
                            {
                                SIGN = 1;
                            }

                            if (NEAR > 0)
                            {
                                if (Errval > 0)
                                {
                                    Errval = (Errval + NEAR) / (2 * NEAR + 1);
                                }
                                else
                                {
                                    Errval = -((NEAR - Errval) / (2 * NEAR + 1));
                                }

                                Rx = Px + SIGN * Errval * (2 * NEAR + 1);
                            }
                            else
                            {
                                Rx = Ix;
                            }

                            if (Errval < 0)
                            {
                                Errval = Errval + RANGE;
                            }

                            if (Errval >= (RANGE + 1) / 2)
                            {
                                Errval = Errval - RANGE;
                            }

                            // Code segment A.20 – Computation of the auxiliary variable TEMP
                            int Q = (int)(RItype + 365);

                            double TEMP;

                            if (RItype == 0)
                            {
                                TEMP = A[365];
                            }
                            else
                            {
                                TEMP = A[366] + ((int)N[366] >> 1);
                            }

                            double k = DetermineGolombParameter(N[Q], TEMP), EMErrval, map;

                            // Code segment A.21 – Computation of map for Errval mapping
                            if ((k == 0) && (Errval > 0) && (2 * Nn[Q - 365] < N[Q]))
                            {
                                map = 1;
                            }
                            else if ((Errval < 0) && (2 * Nn[Q - 365] >= N[Q]))
                            {
                                map = 1;
                            }
                            else if ((Errval < 0) && (k != 0))
                            {
                                map = 1;
                            }
                            else
                            {
                                map = 0;
                            }

                            // Code segment A.22 – Errval mapping for run interruption sample
                            EMErrval = 2 * Math.Abs(Errval) - RItype - map;

                            EncodeGolomb(k, LIMIT - J[RUNIndex] - 1, qbpp, EMErrval, ref stream);

                            // Code segment A.23 – Update of variables for run interruption sample
                            if (Errval < 0)
                            {
                                ++Nn[Q - 365];
                            }

                            A[Q] += ((int)(EMErrval + 1 - RItype) >> 1);

                            if (N[Q] == RESET)
                            {
                                A[Q] = (int)A[Q] >> 1;
                                N[Q] = (int)N[Q] >> 1;
                                Nn[Q - 365] = (int)Nn[Q - 365] >> 1;
                            }

                            ++N[Q];
                        }

                        #endregion
                    }
                    else
                    {
                        #region goto RegularModeProcessing

                        // A.3.3 Local gradient quantization
                        // Code segment A.4 – Quantization of the gradients
                        int Q1 = LocalGradientQuantization(D1),
                            Q2 = LocalGradientQuantization(D2),
                            Q3 = LocalGradientQuantization(D3);

                        int SIGN;

                        // A.3.4 Quantized gradient merging
                        if (Q1 < 0 || (Q1 == 0 && Q2 < 0) || (Q1 == 0 && Q2 == 0 && Q3 < 0))
                        {
                            Q1 = -Q1;
                            Q2 = -Q2;
                            Q3 = -Q3;
                            SIGN = -1;
                        }
                        else
                        {
                            SIGN = 1;
                        }

                        int Q;

                        if (Q1 == 0)
                        {
                            if (Q2 == 0)
                            {
                                Q = 360 + Q3;       // fills 360..364
                            }
                            else
                            {   // Q2 is 1 to 4
                                Q = 324 + (Q2 - 1) * 9 + (Q3 + 4);  // fills 324..359
                            }
                        }
                        else
                        {       // Q1 is 1 to 4
                            Q = (Q1 - 1) * 81 + (Q2 + 4) * 9 + (Q3 + 4);    // fills 0..323
                        }


                        // A.4 Prediction
                        double Px;

                        // A.4.1 Edge - detecting predictor
                        // Code segment A.5 – Edge-detecting predictor
                        if (Rc >= Math.Max(Ra, Rb))
                        {
                            Px = Math.Min(Ra, Rb);
                        }
                        else if (Rc <= Math.Min(Ra, Rb))
                        {
                            Px = Math.Max(Ra, Rb);
                        }
                        else
                        {
                            Px = Ra + Rb - Rc;
                        }

                        // A.4.2 Prediction correction
                        // Code segment A.6 – Prediction correction from the bias
                        if (SIGN == 1)
                        {
                            Px = Px + C[Q];
                        }
                        else
                        {
                            Px = Px - C[Q];
                        }

                        Clipping(ref Px, MAXVAL);

                        // A.4.3 Computation of prediction error
                        // Code segment A.7 – Computation of prediction error
                        double MErrval,
                                Errval = Ix - Px;

                        arr_Errval[(int)Errval + 255]++;

                        if (SIGN == -1)
                        {
                            Errval = -Errval;
                        }

                        // A.4.4 Error quantization for near-lossless coding, and reconstructed value computation
                        // Code segment A.8 – Error quantization and computation of the reconstructed value in near - lossless coding
                        if (Errval > 0)
                        {
                            Errval = (Errval + NEAR) / (2 * NEAR + 1);
                        }
                        else
                        {
                            Errval = -(NEAR - Errval) / (2 * NEAR + 1);
                        }

                        double Rx = Px + SIGN * Errval * (2 * NEAR + 1);

                        Clipping(ref Rx, MAXVAL);

                        // A.4.5 Modulo reduction of the prediction error
                        // Code segment A.9 – Modulo reduction of the error
                        if (Errval < 0)
                        {
                            Errval = Errval + RANGE;
                        }

                        if (Errval >= ((RANGE + 1) / 2))
                        {
                            Errval = Errval - RANGE;
                        }

                        // A.5 Prediction error encoding
                        // A.5.1 Golomb coding variable computation
                        // Code segment A.10 – Computation of the Golomg coding variable k
                        double k = DetermineGolombParameter(N[Q], A[Q]);

                        // A.5.2 Error mapping
                        // Code segment A.11 – Error mapping to non-negative values
                        if ((NEAR == 0) && (k == 0) && (2 * B[Q] <= -N[Q]))
                        {
                            if (Errval >= 0)
                            {
                                MErrval = 2 * Errval + 1;
                            }
                            else
                            {
                                MErrval = -2 * (Errval + 1);
                            }
                        }
                        else
                        {
                            if (Errval >= 0)
                            {
                                MErrval = 2 * Errval;
                            }
                            else
                            {
                                MErrval = -2 * Errval - 1;
                            }
                        }

                        arr_MErrval[(int)MErrval + 255]++;

                        // A.5.3 Mapped-error encoding
                        EncodeGolomb(k, LIMIT, qbpp, MErrval, ref stream);

                        // A.6 Update variables
                        // A.6.1 Update
                        // Code segment A.12 – Variables update
                        B[Q] += Errval * (2 * NEAR + 1);
                        A[Q] += Math.Abs(Errval);

                        if (N[Q] == RESET)
                        {
                            A[Q] = (int)A[Q] >> 1;

                            if (B[Q] >= 0)
                            {
                                B[Q] = ((int)B[Q] >> 1);
                            }
                            else
                            {
                                B[Q] = -((1 - (int)B[Q]) >> 1);
                            }

                            N[Q] = (int)N[Q] >> 1;
                        }

                        ++N[Q];

                        // A.6.2 Bias computation
                        // Code segment A.13 – Update of bias-related variables B[Q] and C[Q]
                        if (B[Q] <= -N[Q])
                        {
                            B[Q] += N[Q];

                            if (C[Q] > MIN_C)
                            {
                                --C[Q];
                            }

                            if (B[Q] <= -N[Q])
                            {
                                B[Q] = -N[Q] + 1;
                            }
                        }
                        else if (B[Q] > 0)
                        {
                            B[Q] -= N[Q];

                            if (C[Q] < MAX_C)
                            {
                                ++C[Q];
                            }

                            if (B[Q] > 0)
                            {
                                B[Q] = 0;
                            }
                        }
                        #endregion
                    }
                }
            }
        }

        public void Write(BinaryWriter stream)
        {
            stream.Write(buffer);
            counter = 8;
        }
    }
}