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
        public class CRestore
        {
            CModel Model;
            public CRestore(object sender)
            {
                Model = (CModel)sender;
            }

            #region Params
            public int i1 = -1;
            public int i2 = -1;
            public int j1 = -1;
            public int j2 = -1;
            public int k1 = 0;
            public int k2 = 0;
            public int width = 1;
            public int state = 0;
            #endregion

            public void Draw()
            {
                if (state > 0)
                {
                    double x1, y1, x2, y2, x3, y3;
                    x1 = Model.Cell.CenterX(i1, j1);
                    y1 = Model.Cell.CenterY(i1, j1);
                    if (i2 == -1)
                    {
                        x2 = x1;
                        y2 = y1;
                    }
                    else
                    {
                        x2 = Model.Cell.CenterX(i2, j2);
                        y2 = Model.Cell.CenterY(i2, j2);
                    }

                    GL.LineWidth(3f);
                    GL.Color4(1.0f, 0.0f, 0.0f, 0.6f);
                    GL.Begin(PrimitiveType.LineStrip);
                    GL.Vertex2(x1, y1);
                    GL.Vertex2(x2, y2);
                    GL.End();

                    if (state == 2)
                    {
                        x3 = Model.Cell.CenterX(i2, j2);
                        y3 = Model.Cell.CenterY(i2 + width, j2);
                        GL.Color4(0f, 0f, 1f, 0.6f);
                        GL.Begin(PrimitiveType.LineStrip);
                        GL.Vertex2(x2, y2);
                        GL.Vertex2(x2 + width, y2);
                        GL.End();
                    }

                    GL.PointSize(15f);
                    GL.Color3(Color.Maroon);
                    GL.Begin(PrimitiveType.Points);
                    GL.Vertex2(x1, y1);
                    GL.Vertex2(x2, y2);
                    GL.End();
                }
            }

            public void New()
            {
                i1 = -1;
                i2 = -1;
                j1 = -1;
                j2 = -1;
                k1 = Model.KRange[0];
                k2 = Model.KRange[1];
                width = 1;
                state = 0;
            }

            public void UpdateTable(DataGridView dgv)
            {
                if (dgv.Tag != "RS")
                {
                    dgv.Rows.Clear();
                    dgv.Tag = "RS";
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[0].Cells[0].Value = "I1";
                    dgv.Rows[0].Cells[1].Value = i1 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[1].Cells[0].Value = "J1";
                    dgv.Rows[1].Cells[1].Value = j1 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[2].Cells[0].Value = "I2";
                    dgv.Rows[2].Cells[1].Value = i2 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[3].Cells[0].Value = "J2";
                    dgv.Rows[3].Cells[1].Value = j2 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[4].Cells[0].Value = "K1";
                    dgv.Rows[4].Cells[1].Value = k1 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[5].Cells[0].Value = "K2";
                    dgv.Rows[5].Cells[1].Value = k2 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[6].Cells[0].Value = "Width";
                    dgv.Rows[6].Cells[1].Value = width;
                }
                else
                {
                    dgv.Rows[0].Cells[1].Value = i1 + 1;
                    dgv.Rows[1].Cells[1].Value = j1 + 1;
                    dgv.Rows[2].Cells[1].Value = i2 + 1;
                    dgv.Rows[3].Cells[1].Value = j2 + 1;
                    dgv.Rows[4].Cells[1].Value = k1 + 1;
                    dgv.Rows[5].Cells[1].Value = k2 + 1;
                    dgv.Rows[6].Cells[1].Value = width;
                }
            }

            public string GetTag()
            {
                StringBuilder str = new StringBuilder();
                str.Append("RS ");
                str.Append(i1 + " " + j1 + " ");
                str.Append(i2 + " " + j2 + " ");
                str.Append(k1 + " " + k2 + " ");
                str.Append(width);
                return str.ToString();
            }

            public string GetName()
            {
                StringBuilder str = new StringBuilder();
                str.Append("Restore ");
                str.Append((i1 + 1) + "/" + (j1 + 1) + " ");
                str.Append((i2 + 1) + "/" + (j2 + 1) + " ");
                str.Append((k1 + 1) + "-" + (k2 + 1) + " ");
                str.Append("Width: " + width);
                return str.ToString();
            }

            public void Read(string str)
            {
                string[] split = str.Split(' ');
                i1 = Convert.ToInt32(split[1]);
                j1 = Convert.ToInt32(split[2]);
                i2 = Convert.ToInt32(split[3]);
                j2 = Convert.ToInt32(split[4]);
                k1 = Convert.ToInt32(split[5]);
                k2 = Convert.ToInt32(split[6]);
                width = Convert.ToInt32(split[7]);
            }

            public void Process()
            {
                // x % y возвращает остаток
                // x / y при целочисленных переменных возвращает частное
                int Lmin, Lmax, Msta, Mfin;
                int m;
                int divi, modi;
                int d1, d2;
                int ni = Math.Abs(i1 - i2) + 1;
                int nj = Math.Abs(j1 - j2) + 1;
                int n1, n2;
                bool dirI = ni >= nj ? true : false;

                bool prevEd = Model.Edited;
                Model.Edited = true;
                #region old
                if (dirI)
                {
                    divi = ni / nj;
                    modi = ni % nj;
                    if (i1 < i2)
                    {
                        Lmin = i1; Lmax = i2; Msta = j1; Mfin = j2;
                    }
                    else
                    {
                        Lmin = i2; Lmax = i1; Msta = j2; Mfin = j1;
                    }
                }
                else
                {
                    divi = nj / ni;
                    modi = nj % ni;
                    if (j1 < j2)
                    {
                        Lmin = j1; Lmax = j2; Msta = i1; Mfin = i2;
                    }
                    else
                    {
                        Lmin = j2; Lmax = j1; Msta = i2; Mfin = i1;
                    }
                }
                int coef = Msta > Mfin ? -1 : 1;


                for (int l = Lmin; l <= Lmax; l++)
                {
                    if ((l - Lmin + 1) <= (modi * (divi + 1)))
                        m = Msta + coef * (((l - Lmin - 1) / (divi + 1)) + 1);
                    else
                        m = Mfin - coef * ((Lmax - l) / (divi));
                    if ((l - Lmin + 1) == 1) m = Msta;
                    if (dirI)
                    {
                        n1 = l; n2 = m;
                    }
                    else
                    {
                        n1 = m; n2 = l;
                    }

                    for (int i = n1 - width; i <= n1 + width; i++)
                        for (int j = n2 - width; j <= n2 + width; j++)
                            if (i >= 0 && i < Model.NI && j >= 0 && j < Model.NJ)
                                if ((Math.Abs(i - n1) + Math.Abs(j - n2)) <= width)
                                {
                                    d1 = Math.Abs(i - i1) + Math.Abs(j - j1);
                                    d2 = Math.Abs(i - i2) + Math.Abs(j - j2);
                                    for (int k = k1; k <= k2; k++)
                                        if (Model.actnum[i, j, k] == 0)
                                        {
                                            foreach (DProperty prop in Model.Props)
                                            {
                                                prop.Mult[i, j, k] = 0;
                                                prop.Add[i, j, k] = (float)(prop.Value[i2, j2, k] * d1 + prop.Value[i1, j1, k] * d2) / (d1 + d2);
                                            }
                                            Model.actnum[i, j, k] = 1;
                                        }
                                }
                }
                #endregion
                Model.Edited = prevEd;
            }
        }
    }
}
