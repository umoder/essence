using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace Essence_graphics
{
    public partial class CModel
    {
        public class CReduce
        {
            CModel Model;
            public CReduce(object sender)
            {
                Model = (CModel)sender;
            }

            #region Params
            public int i0 = -1;
            public int j0 = -1;
            public int k1 = 0;
            public int k2 = 0;
            public double M = 0;
            public double dis = 0;
            public double freq = 0;
            public double r1 = 0;
            public double r2 = 0;
            public double r_inter = 0;
            public double angle = 0;
            public float maxcoef = -1;
            public float mincoef = -1;
            public int state = 0;
            public int prop = 0;
            public Point prevLoc = new Point(0, 0);
            #endregion
            public void Draw()
            {
                if (state > 0)
                {
                    double xc, yc, dx1, dx2, dy1, dy2;
                    dx1 = this.r1 * Math.Cos(this.angle / 180 * Math.PI);
                    dx2 = this.r2 * Math.Cos((this.angle + 90) / 180 * Math.PI);
                    dy1 = this.r1 * Math.Sin(this.angle / 180 * Math.PI);
                    dy2 = this.r2 * Math.Sin((this.angle + 90) / 180 * Math.PI);
                    double dx11 = dx1 * r_inter;
                    double dx21 = dx2 * r_inter;
                    double dy11 = dy1 * r_inter;
                    double dy21 = dy2 * r_inter;
                    xc = Model.Cell.CenterX(this.i0, this.j0);
                    yc = Model.Cell.CenterY(this.i0, this.j0);

                    GL.Color4(1.0f, 0.0f, 0.0f, 0.35f);

                    GL.Begin(PrimitiveType.Triangles);
                    GL.Vertex2(xc + dx1 + dx2, yc + dy1 + dy2);
                    GL.Vertex2(xc - dx1 + dx2, yc - dy1 + dy2);
                    GL.Vertex2(xc + dx1 - dx2, yc + dy1 - dy2);
                    GL.Vertex2(xc - dx1 + dx2, yc - dy1 + dy2);
                    GL.Vertex2(xc + dx1 - dx2, yc + dy1 - dy2);
                    GL.Vertex2(xc - dx1 - dx2, yc - dy1 - dy2);
                    GL.End();

                    GL.LineWidth(3f);
                    GL.Color4(1.0f, 0.0f, 0.0f, 0.6f);
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex2(xc + dx1 + dx2, yc + dy1 + dy2);
                    GL.Vertex2(xc - dx1 + dx2, yc - dy1 + dy2);
                    GL.Vertex2(xc - dx1 - dx2, yc - dy1 - dy2);
                    GL.Vertex2(xc + dx1 - dx2, yc + dy1 - dy2);
                    GL.Vertex2(xc + dx1 + dx2, yc + dy1 + dy2);
                    GL.End();

                    if (state > 2)
                    {
                        GL.Color4(0.0f, 0.0f, 1.0f, 0.35f);
                        GL.Begin(PrimitiveType.Triangles);
                        GL.Vertex2(xc + dx11 + dx21, yc + dy11 + dy21);
                        GL.Vertex2(xc - dx11 + dx21, yc - dy11 + dy21);
                        GL.Vertex2(xc + dx11 - dx21, yc + dy11 - dy21);
                        GL.Vertex2(xc - dx11 + dx21, yc - dy11 + dy21);
                        GL.Vertex2(xc + dx11 - dx21, yc + dy11 - dy21);
                        GL.Vertex2(xc - dx11 - dx21, yc - dy11 - dy21);
                        GL.End();

                        GL.LineWidth(3f);
                        GL.Color4(0.0f, 1.0f, 0.0f, 0.6f);
                        GL.Begin(PrimitiveType.LineStrip);
                        GL.Vertex2(xc + dx11 + dx21, yc + dy11 + dy21);
                        GL.Vertex2(xc - dx11 + dx21, yc - dy11 + dy21);
                        GL.Vertex2(xc - dx11 - dx21, yc - dy11 - dy21);
                        GL.Vertex2(xc + dx11 - dx21, yc + dy11 - dy21);
                        GL.Vertex2(xc + dx11 + dx21, yc + dy11 + dy21);
                        GL.End();
                    }
                    //GL.Disable(EnableCap.Blend);

                    GL.PointSize(15f);
                    GL.Color3(Color.Maroon);

                    GL.Begin(PrimitiveType.Points);
                    GL.Vertex2(xc, yc);
                    GL.End();
                }
            }

            public void New()
            {
                i0 = 0;
                j0 = 0;
                k1 = Model.KRange[0];
                k2 = Model.KRange[1];
                M = 0;
                dis = 0;
                freq = 0;
                r1 = 0;
                r2 = 0;
                r_inter = 0;
                angle = 0;
                maxcoef = -1;
                mincoef = -1;
                state = 0;
                prop = Model.CurrentProperty;
            }

            public void UpdateTable(DataGridView dgv)
            {
                if (dgv.Tag != "RD")
                {
                    dgv.Rows.Clear();
                    dgv.Tag = "RD";
                    dgv.Rows.Add();
                    dgv.Rows[0].Cells[0].Value = "I";
                    dgv.Rows[0].Cells[1].Value = i0 + 1;
                    dgv.Rows.Add();
                    dgv.Rows[1].Cells[0].Value = "J";
                    dgv.Rows[1].Cells[1].Value = j0 + 1;
                    dgv.Rows.Add();
                    dgv.Rows[2].Cells[0].Value = "K_top";
                    dgv.Rows[2].Cells[1].Value = k1 + 1;
                    dgv.Rows.Add();
                    dgv.Rows[3].Cells[0].Value = "K_bot";
                    dgv.Rows[3].Cells[1].Value = k2 + 1;
                    dgv.Rows.Add();
                    dgv.Rows[4].Cells[0].Value = "r1";
                    dgv.Rows[4].Cells[1].Value = Math.Round(r1, 0);
                    dgv.Rows.Add();
                    dgv.Rows[5].Cells[0].Value = "r2";
                    dgv.Rows[5].Cells[1].Value = Math.Round(r2, 0);
                    dgv.Rows.Add();
                    dgv.Rows[6].Cells[0].Value = "r_inter";
                    dgv.Rows[6].Cells[1].Value = Math.Round(r_inter, 2);
                    dgv.Rows.Add();
                    dgv.Rows[7].Cells[0].Value = "Angle";
                    dgv.Rows[7].Cells[1].Value = Math.Round(angle, 0);
                    dgv.Rows.Add();
                    dgv.Rows[8].Cells[0].Value = "M";
                    dgv.Rows[8].Cells[1].Value = M;
                    dgv.Rows.Add();
                    dgv.Rows[9].Cells[0].Value = "dis";
                    dgv.Rows[9].Cells[1].Value = dis;
                    dgv.Rows.Add();
                    dgv.Rows[10].Cells[0].Value = "freq";
                    dgv.Rows[10].Cells[1].Value = freq;
                    dgv.Rows.Add();
                    dgv.Rows[11].Cells[0].Value = "mincoef";
                    dgv.Rows[11].Cells[1].Value = mincoef;
                    dgv.Rows.Add();
                    dgv.Rows[12].Cells[0].Value = "maxcoef";
                    dgv.Rows[12].Cells[1].Value = maxcoef;
                }
                else
                {
                    dgv.Rows[0].Cells[1].Value = i0 + 1;
                    dgv.Rows[1].Cells[1].Value = j0 + 1;
                    dgv.Rows[2].Cells[1].Value = k1 + 1;
                    dgv.Rows[3].Cells[1].Value = k2 + 1;
                    dgv.Rows[4].Cells[1].Value = Math.Round(r1, 0);
                    dgv.Rows[5].Cells[1].Value = Math.Round(r2, 0);
                    dgv.Rows[6].Cells[1].Value = Math.Round(r_inter, 2);
                    dgv.Rows[7].Cells[1].Value = Math.Round(angle, 0);
                    dgv.Rows[8].Cells[1].Value = M;
                    dgv.Rows[9].Cells[1].Value = dis;
                    dgv.Rows[10].Cells[1].Value = freq;
                    dgv.Rows[11].Cells[1].Value = mincoef;
                    dgv.Rows[12].Cells[1].Value = maxcoef;
                }
            }

            public string GetName()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Reduce ");
                sb.Append(Model.Props[prop].Name + " ");
                sb.Append("I/J: " + (i0 + 1) + "/" + (j0 + 1) + " ");
                sb.Append("K-Range: " + (k1 + 1) + "-" + (k2 + 1) + " ");
                sb.Append("M:" + M + " dis:" + dis + " freq:" + freq + " ");
                sb.Append("Radius: " + Math.Round(r1, 0) + "/" + Math.Round(r2, 0) + "~" + Math.Round(r_inter, 2) + " ");
                sb.Append("Angle: " + Math.Round(angle, 0) + " Min/Max: " + maxcoef + "/" + mincoef);
                return sb.ToString();
            }

            public string GetTag()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("RD ");
                sb.Append(prop + " ");
                sb.Append(i0 + " " + j0 + " ");
                sb.Append(k1 + " " + k2 + " ");
                sb.Append(M + " " + dis + " " + freq + " ");
                sb.Append(r1 + " " + r2 + " " + r_inter + " ");
                sb.Append(angle + " " + maxcoef + " " + mincoef + " ");
                return sb.ToString();
            }

            public void Read(string str)
            {
                string[] splitted = str.Split(' ');
                prop = Convert.ToInt32(splitted[1]);
                i0 = Convert.ToInt32(splitted[2]);
                j0 = Convert.ToInt32(splitted[3]);
                k1 = Convert.ToInt32(splitted[4]);
                k2 = Convert.ToInt32(splitted[5]);
                M = Convert.ToDouble(splitted[6]);
                dis = Convert.ToDouble(splitted[7]);
                freq = Convert.ToDouble(splitted[8]);
                r1 = Convert.ToDouble(splitted[9]);
                r2 = Convert.ToDouble(splitted[10]);
                r_inter = Convert.ToDouble(splitted[11]);
                angle = Convert.ToDouble(splitted[12]);
                maxcoef = Convert.ToSingle(splitted[13]);
                mincoef = Convert.ToSingle(splitted[14]);
            }

            private class CList
            {
                public int i = 0;
                public int j = 0;
                public int k = 0;
                public bool led = false;

                public CList(int i, int j, int k)
                {
                    this.i = i;
                    this.j = j;
                    this.k = k;
                    this.led = false;
                }
            }

            public void Process()
            {
                double r1p = r1 / 0.987283876f;
                double r2p = r2 / 0.987283876f;
                double d = r1p > r2p ? r1p : r2p;
                double cosA = Math.Cos((180 - angle) / 180 * Math.PI);
                double sinA = Math.Sin((180 - angle) / 180 * Math.PI);

                int ii = Convert.ToInt32(Math.Round(d / Model.CellSizeI));
                List<CList> CL = new List<CList>();

                for (int k = k1; k <= k2; k++)
                {
                    #region Gathering all cells in a list
                    for (int i = i0 - ii; i <= i0 + ii; i++)
                        for (int j = j0 - ii; j <= j0 + ii; j++)
                            if (i >= 0 && i < Model.NI && j >= 0 && j < Model.NJ)
                            {
                                double X = (Model.Cell.CenterX(i, j) - Model.Cell.CenterX(i0, j0)) * cosA - (Model.Cell.CenterY(i, j) - Model.Cell.CenterY(i0, j0)) * sinA;
                                double Y = (Model.Cell.CenterX(i, j) - Model.Cell.CenterX(i0, j0)) * sinA + (Model.Cell.CenterY(i, j) - Model.Cell.CenterY(i0, j0)) * cosA;

                                if ((Math.Abs(Math.Pow(X / r1p, 3)) + Math.Abs(Math.Pow(Y / r2p, 3)) <= 0.987283876f) && (Math.Abs(Math.Pow(X / r1p / r_inter, 3)) + Math.Abs(Math.Pow(Y / r2p / r_inter, 3)) >= 0.987283876f))
                                    CL.Add(new CList(i, j, k));
                            }
                    #endregion

                    #region Generating cell-list for mod
                    Supports.rnd.Index = Convert.ToInt32(500 * (Math.Cos(k * k * k) + Math.Sin(k * k)) + i0 + 3 * j0);
                    float freqCount = 0;
                    int whileCount = 0;
                    if (CL.Count > 0)
                        while ((freqCount / CL.Count) < freq && (whileCount < 1000000))
                        {
                            int Index = Supports.rnd.Next(CL.Count);
                            if (Index >= 0 && Index < CL.Count)
                            {
                                if (CL[Index].led == false)
                                {
                                    CL[Index].led = true;
                                    freqCount++;
                                }
                                whileCount++;
                            }
                        }
                    #endregion
                    if (whileCount == 1000000) { MessageBox.Show("Too many cells in selection"); break; }
                    else
                    {
                        #region Performing mod
                        foreach (CList cl in CL)
                            if (cl.led)
                            {
                                float coef = Supports.Normalize(M, dis);
                                if (maxcoef != -1 && coef > maxcoef) coef = maxcoef;
                                if (mincoef != -1 && coef < mincoef) coef = mincoef;
                                Model.Props[prop].Mult[cl.i, cl.j, cl.k] = Model.Props[prop].Mult[cl.i, cl.j, cl.k] * (float)coef;
                            }
                        #endregion
                    }
                }
                CL = null;
            }
        }
    }
}
