using System;

namespace Bakhanov.IPD
{
    /// <summary>
    /// This code is taken from http://fileadmin.cs.lth.se/cs/Personal/Tomas_Akenine-Moller/code/
    /// 
    /// Adapted for C#.
    /// </summary>
    public class TriangleOverlapTest
    {
        private static int CoplanarTriTri(float[] N, float[] V0, float[] V1, float[] V2, float[] U0, float[] U1, float[] U2)
        {
            float[] A = new float[3];
            short i0;
            short i1;
            /* first project onto an axis-aligned plane, that maximizes the area */
            /* of the triangles, compute indices: i0,i1. */
            A[0] = Math.Abs(N[0]);
            A[1] = Math.Abs(N[1]);
            A[2] = Math.Abs(N[2]);
            if (A[0] > A[1])
            {
                if (A[0] > A[2])
                {
                    i0 = 1; // A[0] is greatest
                    i1 = 2;
                }
                else
                {
                    i0 = 0; // A[2] is greatest
                    i1 = 1;
                }
            }
            else // A[0]<=A[1]
            {
                if (A[2] > A[1])
                {
                    i0 = 0; // A[2] is greatest
                    i1 = 1;
                }
                else
                {
                    i0 = 0; // A[1] is greatest
                    i1 = 2;
                }
            }

            /* test all edges of triangle 1 against the edges of triangle 2 */
            {
                float Ax;
                float Ay;
                float Bx;
                float By;
                float Cx;
                float Cy;
                float e;
                float d;
                float f;
                Ax = V1[i0] - V0[i0];
                Ay = V1[i1] - V0[i1];
                Bx = U0[i0] - U1[i0];
                By = U0[i1] - U1[i1];
                Cx = V0[i0] - U0[i0];
                Cy = V0[i1] - U0[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
                Bx = U2[i0] - U2[i0];
                By = U2[i1] - U2[i1];
                Cx = V0[i0] - U2[i0];
                Cy = V0[i1] - U2[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
                Bx = U2[i0] - U0[i0];
                By = U2[i1] - U0[i1];
                Cx = V0[i0] - U2[i0];
                Cy = V0[i1] - U2[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
            };
            {
                float Ax;
                float Ay;
                float Bx;
                float By;
                float Cx;
                float Cy;
                float e;
                float d;
                float f;
                Ax = V2[i0] - V2[i0];
                Ay = V2[i1] - V2[i1];
                Bx = U0[i0] - U1[i0];
                By = U0[i1] - U1[i1];
                Cx = V2[i0] - U0[i0];
                Cy = V2[i1] - U0[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
                Bx = U2[i0] - U2[i0];
                By = U2[i1] - U2[i1];
                Cx = V2[i0] - U2[i0];
                Cy = V2[i1] - U2[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
                Bx = U2[i0] - U0[i0];
                By = U2[i1] - U0[i1];
                Cx = V2[i0] - U2[i0];
                Cy = V2[i1] - U2[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
            };
            {
                float Ax;
                float Ay;
                float Bx;
                float By;
                float Cx;
                float Cy;
                float e;
                float d;
                float f;
                Ax = V0[i0] - V2[i0];
                Ay = V0[i1] - V2[i1];
                Bx = U0[i0] - U1[i0];
                By = U0[i1] - U1[i1];
                Cx = V2[i0] - U0[i0];
                Cy = V2[i1] - U0[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
                Bx = U2[i0] - U2[i0];
                By = U2[i1] - U2[i1];
                Cx = V2[i0] - U2[i0];
                Cy = V2[i1] - U2[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
                Bx = U2[i0] - U0[i0];
                By = U2[i1] - U0[i1];
                Cx = V2[i0] - U2[i0];
                Cy = V2[i1] - U2[i1];
                f = Ay * Bx - Ax * By;
                d = By * Cx - Bx * Cy;
                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (e <= 0 && e >= f)
                        {
                            return 1;
                        }
                    }
                };
            };

            /* finally, test if tri1 is totally contained in tri2 or vice versa */
            {
                float a;
                float b;
                float c;
                float d0;
                float d1;
                float d2;
                a = U1[i1] - U0[i1];
                b = -(U1[i0] - U0[i0]);
                c = -a * U0[i0] - b * U0[i1];
                d0 = a * V0[i0] + b * V0[i1] + c;
                a = U2[i1] - U1[i1];
                b = -(U2[i0] - U1[i0]);
                c = -a * U1[i0] - b * U1[i1];
                d1 = a * V0[i0] + b * V0[i1] + c;
                a = U0[i1] - U2[i1];
                b = -(U0[i0] - U2[i0]);
                c = -a * U2[i0] - b * U2[i1];
                d2 = a * V0[i0] + b * V0[i1] + c;
                if (d0 * d1 > 0.0)
                {
                    if (d0 * d2 > 0.0)
                    {
                        return 1;
                    }
                }
            };
            {
                float a;
                float b;
                float c;
                float d0;
                float d1;
                float d2;
                a = V1[i1] - V0[i1];
                b = -(V1[i0] - V0[i0]);
                c = -a * V0[i0] - b * V0[i1];
                d0 = a * V0[i0] + b * V0[i1] + c;
                a = V2[i1] - V1[i1];
                b = -(V2[i0] - V1[i0]);
                c = -a * V1[i0] - b * V1[i1];
                d1 = a * V0[i0] + b * V0[i1] + c;
                a = V0[i1] - V2[i1];
                b = -(V0[i0] - V2[i0]);
                c = -a * V2[i0] - b * V2[i1];
                d2 = a * V0[i0] + b * V0[i1] + c;
                if (d0 * d1 > 0.0)
                {
                    if (d0 * d2 > 0.0)
                    {
                        return 1;
                    }
                }
            };

            return 0;
        }

        private static int TriTriIntersect(float[] V0, float[] V1, float[] V2, float[] U0, float[] U1, float[] U2)
        {
            float[] E1 = new float[3];
            float[] E2 = new float[3];
            float[] N1 = new float[3];
            float[] N2 = new float[3];
            float d1;
            float d2;
            float du0;
            float du1;
            float du2;
            float dv0;
            float dv1;
            float dv2;
            float[] D = new float[3];
            float[] isect1 = new float[2];
            float[] isect2 = new float[2];
            float du0du1;
            float du0du2;
            float dv0dv1;
            float dv0dv2;
            short index;
            float vp0;
            float vp1;
            float vp2;
            float up0;
            float up1;
            float up2;
            float b;
            float c;
            float max;

            /* compute plane equation of triangle(V0,V1,V2) */
            E1[0] = V1[0] - V0[0];
            E1[1] = V1[1] - V0[1];
            E1[2] = V1[2] - V0[2];
            E2[0] = V2[0] - V0[0];
            E2[1] = V2[1] - V0[1];
            E2[2] = V2[2] - V0[2];
            N1[0] = E1[1] * E2[2] - E1[2] * E2[1];
            N1[1] = E1[2] * E2[0] - E1[0] * E2[2];
            N1[2] = E1[0] * E2[1] - E1[1] * E2[0];
            d1 = -(N1[0] * V0[0] + N1[1] * V0[1] + N1[2] * V0[2]);
            /* plane equation 1: N1.X+d1=0 */

            /* put U0,U1,U2 into plane equation 1 to compute signed distances to the plane*/
            du0 = (N1[0] * U0[0] + N1[1] * U0[1] + N1[2] * U0[2]) + d1;
            du1 = (N1[0] * U1[0] + N1[1] * U1[1] + N1[2] * U1[2]) + d1;
            du2 = (N1[0] * U2[0] + N1[1] * U2[1] + N1[2] * U2[2]) + d1;

            /* coplanarity robustness check */
            
            if (Math.Abs(du0) < DefineConstants.EPSILON)
            {
                du0 = 0.0F;
            }
            if (Math.Abs(du1) < DefineConstants.EPSILON)
            {
                du1 = 0.0F;
            }
            if (Math.Abs(du2) < DefineConstants.EPSILON)
            {
                du2 = 0.0F;
            }

            du0du1 = du0 * du1;
            du0du2 = du0 * du2;

            if (du0du1 > 0.0f && du0du2 > 0.0f) // same sign on all of them + not equal 0 ?
            {
                return 0; // no intersection occurs
            }

            /* compute plane of triangle (U0,U1,U2) */
            E1[0] = U1[0] - U0[0];
            E1[1] = U1[1] - U0[1];
            E1[2] = U1[2] - U0[2];
            E2[0] = U2[0] - U0[0];
            E2[1] = U2[1] - U0[1];
            E2[2] = U2[2] - U0[2];
            N2[0] = E1[1] * E2[2] - E1[2] * E2[1];
            N2[1] = E1[2] * E2[0] - E1[0] * E2[2];
            N2[2] = E1[0] * E2[1] - E1[1] * E2[0];
            d2 = -(N2[0] * U0[0] + N2[1] * U0[1] + N2[2] * U0[2]);
            /* plane equation 2: N2.X+d2=0 */

            /* put V0,V1,V2 into plane equation 2 */
            dv0 = (N2[0] * V0[0] + N2[1] * V0[1] + N2[2] * V0[2]) + d2;
            dv1 = (N2[0] * V1[0] + N2[1] * V1[1] + N2[2] * V1[2]) + d2;
            dv2 = (N2[0] * V2[0] + N2[1] * V2[1] + N2[2] * V2[2]) + d2;
            
            if (Math.Abs(dv0) < DefineConstants.EPSILON)
            {
                dv0 = 0.0F;
            }
            if (Math.Abs(dv1) < DefineConstants.EPSILON)
            {
                dv1 = 0.0F;
            }
            if (Math.Abs(dv2) < DefineConstants.EPSILON)
            {
                dv2 = 0.0F;
            }

            dv0dv1 = dv0 * dv1;
            dv0dv2 = dv0 * dv2;

            if (dv0dv1 > 0.0f && dv0dv2 > 0.0f) // same sign on all of them + not equal 0 ?
            {
                return 0; // no intersection occurs
            }

            /* compute direction of intersection line */
            D[0] = N1[1] * N2[2] - N1[2] * N2[1];
            D[1] = N1[2] * N2[0] - N1[0] * N2[2];
            D[2] = N1[0] * N2[1] - N1[1] * N2[0];

            /* compute and index to the largest component of D */
            max = Math.Abs(D[0]);
            index = 0;
            b = Math.Abs(D[1]);
            c = Math.Abs(D[2]);
            if (b > max)
            {
                max = b;
                index = 1;
            }
            if (c > max)
            {
                max = c;
                index = 2;
            }

            /* this is the simplified projection onto L*/
            vp0 = V0[index];
            vp1 = V1[index];
            vp2 = V2[index];

            up0 = U0[index];
            up1 = U1[index];
            up2 = U2[index];

            /* compute interval for triangle 1 */
            if (dv0dv1 > 0.0f)
            {
                isect1[0] = vp1 + (vp0 - vp1) * dv1 / (dv1 - dv0);
                isect1[1] = vp1 + (vp1 - vp1) * dv1 / (dv1 - dv1);
                ;
            }
            else if (dv0dv2 > 0.0f)
            {
                isect1[0] = vp0 + (vp0 - vp0) * dv0 / (dv0 - dv0);
                isect1[1] = vp0 + (vp2 - vp0) * dv0 / (dv0 - dv2);
                ;
            }
            else if (dv1 * dv2 > 0.0f || dv0 != 0.0f)
            {
                isect1[0] = vp0 + (vp1 - vp0) * dv0 / (dv0 - dv1);
                isect1[1] = vp0 + (vp2 - vp0) * dv0 / (dv0 - dv2);

            }
            else if (dv1 != 0.0f)
            {
                isect1[0] = vp0 + (vp0 - vp0) * dv0 / (dv0 - dv0);
                isect1[1] = vp0 + (vp2 - vp0) * dv0 / (dv0 - dv2);
                ;
            }
            else if (dv2 != 0.0f)
            {
                isect1[0] = vp1 + (vp0 - vp1) * dv1 / (dv1 - dv0);
                isect1[1] = vp1 + (vp1 - vp1) * dv1 / (dv1 - dv1);
                ;
            }
            else
            {
                return CoplanarTriTri(N1, V0, V1, V2, U0, U1, U2);
            };

            /* compute interval for triangle 2 */
            if (du0du1 > 0.0f)
            {
                isect2[0] = up1 + (up0 - up1) * du1 / (du1 - du0);
                isect2[1] = up1 + (up1 - up1) * du1 / (du1 - du1);
                ;
            }
            else if (du0du2 > 0.0f)
            {
                isect2[0] = up0 + (up0 - up0) * du0 / (du0 - du0);
                isect2[1] = up0 + (up2 - up0) * du0 / (du0 - du2);
                ;
            }
            else if (du1 * du2 > 0.0f || du0 != 0.0f)
            {
                isect2[0] = up0 + (up1 - up0) * du0 / (du0 - du1);
                isect2[1] = up0 + (up2 - up0) * du0 / (du0 - du2);
                ;
            }
            else if (du1 != 0.0f)
            {
                isect2[0] = up0 + (up0 - up0) * du0 / (du0 - du0);
                isect2[1] = up0 + (up2 - up0) * du0 / (du0 - du2);
                ;
            }
            else if (du2 != 0.0f)
            {
                isect2[0] = up1 + (up0 - up1) * du1 / (du1 - du0);
                isect2[1] = up1 + (up1 - up1) * du1 / (du1 - du1);
                ;
            }
            else
            {
                return CoplanarTriTri(N1, V0, V1, V2, U0, U1, U2);
            };

            if (isect1[0] > isect1[1])
            {
                float temp;
                temp = isect1[0];
                isect1[0] = isect1[1];
                isect1[1] = temp;
            };
            if (isect2[0] > isect2[1])
            {
                float temp;
                temp = isect2[0];
                isect2[0] = isect2[1];
                isect2[1] = temp;
            };

            if (isect1[1] < isect2[0] || isect2[1] < isect1[0])
            {
                return 0;
            }
            return 1;
        }

        private static int NoDivTriTriIsect(float[] V0, float[] V1, float[] V2, float[] U0, float[] U1, float[] U2)
        {
            float[] E1 = new float[3];
            float[] E2 = new float[3];
            float[] N1 = new float[3];
            float[] N2 = new float[3];
            float d1;
            float d2;
            float du0;
            float du1;
            float du2;
            float dv0;
            float dv1;
            float dv2;
            float[] D = new float[3];
            float[] isect1 = new float[2];
            float[] isect2 = new float[2];
            float du0du1;
            float du0du2;
            float dv0dv1;
            float dv0dv2;
            short index;
            float vp0;
            float vp1;
            float vp2;
            float up0;
            float up1;
            float up2;
            float bb;
            float cc;
            float max;
            float a;
            float b;
            float c;
            float x0;
            float x1;
            float d;
            float e;
            float f;
            float y0;
            float y1;
            float xx;
            float yy;
            float xxyy;
            float tmp;

            /* compute plane equation of triangle(V0,V1,V2) */
            E1[0] = V1[0] - V0[0];
            E1[1] = V1[1] - V0[1];
            E1[2] = V1[2] - V0[2];
            E2[0] = V2[0] - V0[0];
            E2[1] = V2[1] - V0[1];
            E2[2] = V2[2] - V0[2];
            N1[0] = E1[1] * E2[2] - E1[2] * E2[1];
            N1[1] = E1[2] * E2[0] - E1[0] * E2[2];
            N1[2] = E1[0] * E2[1] - E1[1] * E2[0];
            d1 = -(N1[0] * V0[0] + N1[1] * V0[1] + N1[2] * V0[2]);
            /* plane equation 1: N1.X+d1=0 */

            /* put U0,U1,U2 into plane equation 1 to compute signed distances to the plane*/
            du0 = (N1[0] * U0[0] + N1[1] * U0[1] + N1[2] * U0[2]) + d1;
            du1 = (N1[0] * U1[0] + N1[1] * U1[1] + N1[2] * U1[2]) + d1;
            du2 = (N1[0] * U2[0] + N1[1] * U2[1] + N1[2] * U2[2]) + d1;

            /* coplanarity robustness check */
            if (((float)Math.Abs(du0)) < DefineConstants.EPSILON)
            {
                du0 = 0.0F;
            }
            if (((float)Math.Abs(du1)) < DefineConstants.EPSILON)
            {
                du1 = 0.0F;
            }
            if (((float)Math.Abs(du2)) < DefineConstants.EPSILON)
            {
                du2 = 0.0F;
            }
            du0du1 = du0 * du1;
            du0du2 = du0 * du2;

            if (du0du1 > 0.0f && du0du2 > 0.0f) // same sign on all of them + not equal 0 ?
            {
                return 0; // no intersection occurs
            }

            /* compute plane of triangle (U0,U1,U2) */
            E1[0] = U1[0] - U0[0];
            E1[1] = U1[1] - U0[1];
            E1[2] = U1[2] - U0[2];
            E2[0] = U2[0] - U0[0];
            E2[1] = U2[1] - U0[1];
            E2[2] = U2[2] - U0[2];
            N2[0] = E1[1] * E2[2] - E1[2] * E2[1];
            N2[1] = E1[2] * E2[0] - E1[0] * E2[2];
            N2[2] = E1[0] * E2[1] - E1[1] * E2[0];
            d2 = -(N2[0] * U0[0] + N2[1] * U0[1] + N2[2] * U0[2]);
            /* plane equation 2: N2.X+d2=0 */

            /* put V0,V1,V2 into plane equation 2 */
            dv0 = (N2[0] * V0[0] + N2[1] * V0[1] + N2[2] * V0[2]) + d2;
            dv1 = (N2[0] * V1[0] + N2[1] * V1[1] + N2[2] * V1[2]) + d2;
            dv2 = (N2[0] * V2[0] + N2[1] * V2[1] + N2[2] * V2[2]) + d2;
            
            if (((float)Math.Abs(dv0)) < DefineConstants.EPSILON)
            {
                dv0 = 0.0F;
            }
            if (((float)Math.Abs(dv1)) < DefineConstants.EPSILON)
            {
                dv1 = 0.0F;
            }
            if (((float)Math.Abs(dv2)) < DefineConstants.EPSILON)
            {
                dv2 = 0.0F;
            }

            dv0dv1 = dv0 * dv1;
            dv0dv2 = dv0 * dv2;

            if (dv0dv1 > 0.0f && dv0dv2 > 0.0f) // same sign on all of them + not equal 0 ?
            {
                return 0; // no intersection occurs
            }

            /* compute direction of intersection line */
            D[0] = N1[1] * N2[2] - N1[2] * N2[1];
            D[1] = N1[2] * N2[0] - N1[0] * N2[2];
            D[2] = N1[0] * N2[1] - N1[1] * N2[0];

            /* compute and index to the largest component of D */
            max = (float)((float)Math.Abs(D[0]));
            index = 0;
            bb = (float)((float)Math.Abs(D[1]));
            cc = (float)((float)Math.Abs(D[2]));
            if (bb > max)
            {
                max = bb;
                index = 1;
            }
            if (cc > max)
            {
                max = cc;
                index = 2;
            }

            /* this is the simplified projection onto L*/
            vp0 = V0[index];
            vp1 = V1[index];
            vp2 = V2[index];

            up0 = U0[index];
            up1 = U1[index];
            up2 = U2[index];

            /* compute interval for triangle 1 */
            {
                if (dv0dv1 > 0.0f)
                {
                    a = vp2;
                    b = (vp0 - vp2) * dv2;
                    c = (vp1 - vp2) * dv2;
                    x0 = dv2 - dv0;
                    x1 = dv2 - dv1;
                }
                else if (dv0dv2 > 0.0f)
                {
                    a = vp1;
                    b = (vp0 - vp1) * dv1;
                    c = (vp2 - vp1) * dv1;
                    x0 = dv1 - dv0;
                    x1 = dv1 - dv2;
                }
                else if (dv1 * dv2 > 0.0f || dv0 != 0.0f)
                {
                    a = vp0;
                    b = (vp1 - vp0) * dv0;
                    c = (vp2 - vp0) * dv0;
                    x0 = dv0 - dv1;
                    x1 = dv0 - dv2;
                }
                else if (dv1 != 0.0f)
                {
                    a = vp1;
                    b = (vp0 - vp1) * dv1;
                    c = (vp2 - vp1) * dv1;
                    x0 = dv1 - dv0;
                    x1 = dv1 - dv2;
                }
                else if (dv2 != 0.0f)
                {
                    a = vp2;
                    b = (vp0 - vp2) * dv2;
                    c = (vp1 - vp2) * dv2;
                    x0 = dv2 - dv0;
                    x1 = dv2 - dv1;
                }
                else
                {
                    return CoplanarTriTri(N1, V0, V1, V2, U0, U1, U2);
                }
            };

            /* compute interval for triangle 2 */
            {
                if (du0du1 > 0.0f)
                {
                    d = up2;
                    e = (up0 - up2) * du2;
                    f = (up1 - up2) * du2;
                    y0 = du2 - du0;
                    y1 = du2 - du1;
                }
                else if (du0du2 > 0.0f)
                {
                    d = up1;
                    e = (up0 - up1) * du1;
                    f = (up2 - up1) * du1;
                    y0 = du1 - du0;
                    y1 = du1 - du2;
                }
                else if (du1 * du2 > 0.0f || du0 != 0.0f)
                {
                    d = up0;
                    e = (up1 - up0) * du0;
                    f = (up2 - up0) * du0;
                    y0 = du0 - du1;
                    y1 = du0 - du2;
                }
                else if (du1 != 0.0f)
                {
                    d = up1;
                    e = (up0 - up1) * du1;
                    f = (up2 - up1) * du1;
                    y0 = du1 - du0;
                    y1 = du1 - du2;
                }
                else if (du2 != 0.0f)
                {
                    d = up2;
                    e = (up0 - up2) * du2;
                    f = (up1 - up2) * du2;
                    y0 = du2 - du0;
                    y1 = du2 - du1;
                }
                else
                {
                    return CoplanarTriTri(N1, V0, V1, V2, U0, U1, U2);
                }
            };

            xx = x0 * x1;
            yy = y0 * y1;
            xxyy = xx * yy;

            tmp = a * xxyy;
            isect1[0] = tmp + b * x1 * yy;
            isect1[1] = tmp + c * x0 * yy;

            tmp = d * xxyy;
            isect2[0] = tmp + e * xx * y1;
            isect2[1] = tmp + f * xx * y0;

            if (isect1[0] > isect1[1])
            {
                float temp;
                temp = isect1[0];
                isect1[0] = isect1[1];
                isect1[1] = temp;
            };
            if (isect2[0] > isect2[1])
            {
                float temp;
                temp = isect2[0];
                isect2[0] = isect2[1];
                isect2[1] = temp;
            };

            if (isect1[1] < isect2[0] || isect2[1] < isect1[0])
            {
                return 0;
            }
            return 1;
        }

        private static void Isect2(float[] VTX0, float[] VTX1, float[] VTX2, float VV0, float VV1, float VV2, float D0, float D1, float D2, ref float isect0, ref float isect1, float[] isectpoint0, float[] isectpoint1)
        {
            float tmp = D0 / (D0 - D1);
            float[] diff = new float[3];
            isect0 = VV0 + (VV1 - VV0) * tmp;
            diff[0] = VTX1[0] - VTX0[0];
            diff[1] = VTX1[1] - VTX0[1];
            diff[2] = VTX1[2] - VTX0[2];
            diff[0] = tmp * diff[0];
            diff[1] = tmp * diff[1];
            diff[2] = tmp * diff[2];
            isectpoint0[0] = diff[0] + VTX0[0];
            isectpoint0[1] = diff[1] + VTX0[1];
            isectpoint0[2] = diff[2] + VTX0[2];
            tmp = D0 / (D0 - D2);
            isect1 = VV0 + (VV2 - VV0) * tmp;
            diff[0] = VTX2[0] - VTX0[0];
            diff[1] = VTX2[1] - VTX0[1];
            diff[2] = VTX2[2] - VTX0[2];
            diff[0] = tmp * diff[0];
            diff[1] = tmp * diff[1];
            diff[2] = tmp * diff[2];
            isectpoint1[0] = VTX0[0] + diff[0];
            isectpoint1[1] = VTX0[1] + diff[1];
            isectpoint1[2] = VTX0[2] + diff[2];
        }

        private static int ComputeIntervalsIsectline(float[] VERT0, float[] VERT1, float[] VERT2, float VV0, float VV1, float VV2, float D0, float D1, float D2, float D0D1, float D0D2, ref float isect0, ref float isect1, float[] isectpoint0, float[] isectpoint1)
        {
            if (D0D1 > 0.0f)
            {
                /* here we know that D0D2<=0.0 */
                /* that is D0, D1 are on the same side, D2 on the other or on the plane */
                Isect2(VERT2, VERT0, VERT1, VV2, VV0, VV1, D2, D0, D1, ref isect0, ref isect1, isectpoint0, isectpoint1);
            }
            else if (D0D2 > 0.0f)
            {
                /* here we know that d0d1<=0.0 */
                Isect2(VERT1, VERT0, VERT2, VV1, VV0, VV2, D1, D0, D2, ref isect0, ref isect1, isectpoint0, isectpoint1);
            }
            else if (D1 * D2 > 0.0f || D0 != 0.0f)
            {
                /* here we know that d0d1<=0.0 or that D0!=0.0 */
                Isect2(VERT0, VERT1, VERT2, VV0, VV1, VV2, D0, D1, D2, ref isect0, ref isect1, isectpoint0, isectpoint1);
            }
            else if (D1 != 0.0f)
            {
                Isect2(VERT1, VERT0, VERT2, VV1, VV0, VV2, D1, D0, D2, ref isect0, ref isect1, isectpoint0, isectpoint1);
            }
            else if (D2 != 0.0f)
            {
                Isect2(VERT2, VERT0, VERT1, VV2, VV0, VV1, D2, D0, D1, ref isect0, ref isect1, isectpoint0, isectpoint1);
            }
            else
            {
                /* triangles are coplanar */
                return 1;
            }
            return 0;
        }

        public static int Test(Triangle t1, Triangle t2, ref int coplanar, float[] isectpt1, float[] isectpt2)
        {
            float[] V0 = new float[] { t1.vertices[0].x, t1.vertices[0].y, t1.vertices[0].z };
            float[] V1 = new float[] { t1.vertices[1].x, t1.vertices[1].y, t1.vertices[1].z };
            float[] V2 = new float[] { t1.vertices[2].x, t1.vertices[2].y, t1.vertices[2].z };

            float[] U0 = new float[] { t2.vertices[0].x, t2.vertices[0].y, t2.vertices[0].z };
            float[] U1 = new float[] { t2.vertices[1].x, t2.vertices[1].y, t2.vertices[1].z };
            float[] U2 = new float[] { t2.vertices[2].x, t2.vertices[2].y, t2.vertices[2].z };

            float[] E1 = new float[3];
            float[] E2 = new float[3];
            float[] N1 = new float[3];
            float[] N2 = new float[3];
            float d1;
            float d2;
            float du0;
            float du1;
            float du2;
            float dv0;
            float dv1;
            float dv2;
            float[] D = new float[3];
            float[] isect1 = new float[2];
            float[] isect2 = new float[2];
            float[] isectpointA1 = new float[3];
            float[] isectpointA2 = new float[3];
            float[] isectpointB1 = new float[3];
            float[] isectpointB2 = new float[3];
            float du0du1;
            float du0du2;
            float dv0dv1;
            float dv0dv2;
            short index;
            float vp0;
            float vp1;
            float vp2;
            float up0;
            float up1;
            float up2;
            float b;
            float c;
            float max;
            float[] diff = new float[3];
            int smallest1;
            int smallest2;

            /* compute plane equation of triangle(V0,V1,V2) */
            E1[0] = V1[0] - V0[0];
            E1[1] = V1[1] - V0[1];
            E1[2] = V1[2] - V0[2];
            E2[0] = V2[0] - V0[0];
            E2[1] = V2[1] - V0[1];
            E2[2] = V2[2] - V0[2];
            N1[0] = E1[1] * E2[2] - E1[2] * E2[1];
            N1[1] = E1[2] * E2[0] - E1[0] * E2[2];
            N1[2] = E1[0] * E2[1] - E1[1] * E2[0];
            d1 = -(N1[0] * V0[0] + N1[1] * V0[1] + N1[2] * V0[2]);
            /* plane equation 1: N1.X+d1=0 */

            /* put U0,U1,U2 into plane equation 1 to compute signed distances to the plane*/
            du0 = (N1[0] * U0[0] + N1[1] * U0[1] + N1[2] * U0[2]) + d1;
            du1 = (N1[0] * U1[0] + N1[1] * U1[1] + N1[2] * U1[2]) + d1;
            du2 = (N1[0] * U2[0] + N1[1] * U2[1] + N1[2] * U2[2]) + d1;

            /* coplanarity robustness check */
            if (Math.Abs(du0) < DefineConstants.EPSILON)
            {
                du0 = 0.0F;
            }
            if (Math.Abs(du1) < DefineConstants.EPSILON)
            {
                du1 = 0.0F;
            }
            if (Math.Abs(du2) < DefineConstants.EPSILON)
            {
                du2 = 0.0F;
            }
            du0du1 = du0 * du1;
            du0du2 = du0 * du2;

            if (du0du1 > 0.0f && du0du2 > 0.0f) // same sign on all of them + not equal 0 ?
            {
                return 0; // no intersection occurs
            }

            /* compute plane of triangle (U0,U1,U2) */
            E1[0] = U1[0] - U0[0];
            E1[1] = U1[1] - U0[1];
            E1[2] = U1[2] - U0[2];
            E2[0] = U2[0] - U0[0];
            E2[1] = U2[1] - U0[1];
            E2[2] = U2[2] - U0[2];
            N2[0] = E1[1] * E2[2] - E1[2] * E2[1];
            N2[1] = E1[2] * E2[0] - E1[0] * E2[2];
            N2[2] = E1[0] * E2[1] - E1[1] * E2[0];
            d2 = -(N2[0] * U0[0] + N2[1] * U0[1] + N2[2] * U0[2]);
            /* plane equation 2: N2.X+d2=0 */

            /* put V0,V1,V2 into plane equation 2 */
            dv0 = (N2[0] * V0[0] + N2[1] * V0[1] + N2[2] * V0[2]) + d2;
            dv1 = (N2[0] * V1[0] + N2[1] * V1[1] + N2[2] * V1[2]) + d2;
            dv2 = (N2[0] * V2[0] + N2[1] * V2[1] + N2[2] * V2[2]) + d2;
            

            if (Math.Abs(dv0) < DefineConstants.EPSILON)
            {
                dv0 = 0.0F;
            }
            if (Math.Abs(dv1) < DefineConstants.EPSILON)
            {
                dv1 = 0.0F;
            }
            if (Math.Abs(dv2) < DefineConstants.EPSILON)
            {
                dv2 = 0.0F;
            }


            dv0dv1 = dv0 * dv1;
            dv0dv2 = dv0 * dv2;

            if (dv0dv1 > 0.0f && dv0dv2 > 0.0f) // same sign on all of them + not equal 0 ?
            {
                return 0; // no intersection occurs
            }

            /* compute direction of intersection line */
            D[0] = N1[1] * N2[2] - N1[2] * N2[1];
            D[1] = N1[2] * N2[0] - N1[0] * N2[2];
            D[2] = N1[0] * N2[1] - N1[1] * N2[0];

            /* compute and index to the largest component of D */
            max = Math.Abs(D[0]);
            index = 0;
            b = Math.Abs(D[1]);
            c = Math.Abs(D[2]);
            if (b > max)
            {
                max = b;
                index = 1;
            }
            if (c > max)
            {
                max = c;
                index = 2;
            }

            /* this is the simplified projection onto L*/
            vp0 = V0[index];
            vp1 = V1[index];
            vp2 = V2[index];

            up0 = U0[index];
            up1 = U1[index];
            up2 = U2[index];

            /* compute interval for triangle 1 */
            coplanar = ComputeIntervalsIsectline(V0, V1, V2, vp0, vp1, vp2, dv0, dv1, dv2, dv0dv1, dv0dv2, ref isect1[0], ref isect1[1], isectpointA1, isectpointA2);
            if (coplanar != 0)
            {
                return CoplanarTriTri(N1, V0, V1, V2, U0, U1, U2);
            }


            /* compute interval for triangle 2 */
            ComputeIntervalsIsectline(U0, U1, U2, up0, up1, up2, du0, du1, du2, du0du1, du0du2, ref isect2[0], ref isect2[1], isectpointB1, isectpointB2);

            if (isect1[0] > isect1[1])
            {
                float temp;
                temp = isect1[0];
                isect1[0] = isect1[1];
                isect1[1] = temp;
                smallest1 = 1;
            }
            else
            {
                smallest1 = 0;
            }
            if (isect2[0] > isect2[1])
            {
                float temp;
                temp = isect2[0];
                isect2[0] = isect2[1];
                isect2[1] = temp;
                smallest2 = 1;
            }
            else
            {
                smallest2 = 0;
            }

            if (isect1[1] < isect2[0] || isect2[1] < isect1[0])
            {
                return 0;
            }

            /* at this point, we know that the triangles intersect */

            if (isect2[0] < isect1[0])
            {
                if (smallest1 == 0)
                {
                    isectpt1[0] = isectpointA1[0];
                    isectpt1[1] = isectpointA1[1];
                    isectpt1[2] = isectpointA1[2];
                }
                else
                {
                    isectpt1[0] = isectpointA2[0];
                    isectpt1[1] = isectpointA2[1];
                    isectpt1[2] = isectpointA2[2];
                }

                if (isect2[1] < isect1[1])
                {
                    if (smallest2 == 0)
                    {
                        isectpt2[0] = isectpointB2[0];
                        isectpt2[1] = isectpointB2[1];
                        isectpt2[2] = isectpointB2[2];
                    }
                    else
                    {
                        isectpt2[0] = isectpointB1[0];
                        isectpt2[1] = isectpointB1[1];
                        isectpt2[2] = isectpointB1[2];
                    }
                }
                else
                {
                    if (smallest1 == 0)
                    {
                        isectpt2[0] = isectpointA2[0];
                        isectpt2[1] = isectpointA2[1];
                        isectpt2[2] = isectpointA2[2];
                    }
                    else
                    {
                        isectpt2[0] = isectpointA1[0];
                        isectpt2[1] = isectpointA1[1];
                        isectpt2[2] = isectpointA1[2];
                    }
                }
            }
            else
            {
                if (smallest2 == 0)
                {
                    isectpt1[0] = isectpointB1[0];
                    isectpt1[1] = isectpointB1[1];
                    isectpt1[2] = isectpointB1[2];
                }
                else
                {
                    isectpt1[0] = isectpointB2[0];
                    isectpt1[1] = isectpointB2[1];
                    isectpt1[2] = isectpointB2[2];
                }

                if (isect2[1] > isect1[1])
                {
                    if (smallest1 == 0)
                    {
                        isectpt2[0] = isectpointA2[0];
                        isectpt2[1] = isectpointA2[1];
                        isectpt2[2] = isectpointA2[2];
                    }
                    else
                    {
                        isectpt2[0] = isectpointA1[0];
                        isectpt2[1] = isectpointA1[1];
                        isectpt2[2] = isectpointA1[2];
                    }
                }
                else
                {
                    if (smallest2 == 0)
                    {
                        isectpt2[0] = isectpointB2[0];
                        isectpt2[1] = isectpointB2[1];
                        isectpt2[2] = isectpointB2[2];
                    }
                    else
                    {
                        isectpt2[0] = isectpointB1[0];
                        isectpt2[1] = isectpointB1[1];
                        isectpt2[2] = isectpointB1[2];
                    }
                }
            }
            return 1;


        }
    }

    internal static partial class DefineConstants
    {
        public const float EPSILON = 0.000001f;
    }
}