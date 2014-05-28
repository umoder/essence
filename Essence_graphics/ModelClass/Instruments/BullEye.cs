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
        /// <summary>
        /// Объект для работы с BullEye
        /// </summary>
        public class CBulleye
        {
            CModel Model;
            public CBulleye(object sender)
            {
                Model = (CModel)sender;
                prop = Model.CurrentProperty;
            }

            #region Параметры
            /// <summary>
            /// Хранит предыдущее положение мыши
            /// </summary>
            public Point prevLoc = new Point();
            /// <summary>
            /// Хранит текущее положение мыши
            /// </summary>
            public Point curLoc = new Point();
            public double a = 1;
            public double c = 0;
            public int prop = 0;
            public int i0 = 0;
            public int j0 = 0;
            public int k1 = 0;
            public int k2 = 0;
            public double r1 = 0;
            public double r2 = 0;
            public double angle = 0;
            public int mode = 1;
            public int _state = 0;
            /// <summary>
            /// Хранит текущее состояние создания
            /// </summary>
            public int state
            {
                get { return _state; }
                set { /*if (value > 2) _state = 0; else*/ _state = value; }
            }
            #endregion

            /// <summary>
            /// Скидывает установленные параметры на умолчания
            /// </summary>
            public void New()
            {
                prevLoc = new Point();
                a = 1;
                c = 0;
                prop = Model.CurrentProperty;
                i0 = 0;
                j0 = 0;
                k1 = Model.KRange[0];
                k2 = Model.KRange[1];
                r1 = 0;
                r2 = 0;
                angle = 0;
                mode = 1;
                state = 0;
            }

            /// <summary>
            /// Выполняет отрисовку BullEye на окне карты
            /// </summary>
            public void Draw()
            {
                if (state > 0)
                {
                    double xc, yc, dx1, dx2, dy1, dy2;
                    dx1 = this.r1 * Math.Cos(this.angle / 180 * Math.PI);
                    dx2 = this.r2 * Math.Cos((this.angle + 90) / 180 * Math.PI);
                    dy1 = this.r1 * Math.Sin(this.angle / 180 * Math.PI);
                    dy2 = this.r2 * Math.Sin((this.angle + 90) / 180 * Math.PI);
                    xc = (Model.coord[i0, j0, 0].X + Model.coord[i0 + 1, j0, 0].X + Model.coord[i0, j0 + 1, 0].X + Model.coord[i0 + 1, j0 + 1, 0].X) * 0.25;
                    yc = (Model.coord[i0, j0, 0].Y + Model.coord[i0 + 1, j0, 0].Y + Model.coord[i0, j0 + 1, 0].Y + Model.coord[i0 + 1, j0 + 1, 0].Y) * 0.25;

                    GL.Color4(0.0f, 0.0f, 1.0f, 0.35f);

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

                    //GL.Disable(EnableCap.Blend);

                    GL.PointSize(15f);
                    GL.Color3(Color.Maroon);

                    GL.Begin(PrimitiveType.Points);
                    GL.Vertex2(xc, yc);
                    GL.End();

                    if (state > 2)
                    {
                        // TODO paint points for editing be-zone
                    }
                }
            }

            /// <summary>
            /// Обновляет поле таблицы в соответствии с параметрами BullEye
            /// </summary>
            /// <param name="dgv"></param>
            public void UpdateTable(DataGridView dgv)
            {
                if (dgv.Tag != "BE")
                {
                    dgv.Rows.Clear();
                    dgv.Tag = "BE";
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[0].Cells[0].Value = "Multiplier";
                    dgv.Rows[0].Cells[1].Value = a;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[1].Cells[0].Value = "Addendum";
                    dgv.Rows[1].Cells[1].Value = c;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[2].Cells[0].Value = "Center cell I";
                    dgv.Rows[2].Cells[1].Value = i0 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[3].Cells[0].Value = "Center cell J";
                    dgv.Rows[3].Cells[1].Value = j0 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[4].Cells[0].Value = "Top K";
                    dgv.Rows[4].Cells[1].Value = k1 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[5].Cells[0].Value = "Bottom K";
                    dgv.Rows[5].Cells[1].Value = k2 + 1;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[6].Cells[0].Value = "Rotation Angle";
                    dgv.Rows[6].Cells[1].Value = Math.Round(angle, 1);
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[7].Cells[0].Value = "Radius Main";
                    dgv.Rows[7].Cells[1].Value = Math.Round(r1, 1);
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[8].Cells[0].Value = "Radius Additional";
                    dgv.Rows[8].Cells[1].Value = Math.Round(r2, 1);
                    DataGridViewRow row = new DataGridViewRow();
                    DataGridViewComboBoxCell cell_CB = new DataGridViewComboBoxCell();
                    row.CreateCells(dgv);
                    row.Cells[0].Value = "Adding Mode";
                    cell_CB.Items.AddRange(new string[] { "Set", "Merge", "Min", "Max" });
                    //cell_CB.Selected = 1;
                    row.Cells[1] = cell_CB;
                    row.Cells[1].Value = (row.Cells[1] as DataGridViewComboBoxCell).Items[1];
                    dgv.Rows.Add(row);
                    /*dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[9].Cells[0].Value = "Adding Mode";
                    dgv.Rows[9].Cells[1].Value = Model.Bulleye.mode;*/
                }
                else
                {
                    dgv.Rows[0].Cells[1].Value = a;
                    dgv.Rows[1].Cells[1].Value = c;
                    dgv.Rows[2].Cells[1].Value = i0 + 1;
                    dgv.Rows[3].Cells[1].Value = j0 + 1;
                    dgv.Rows[4].Cells[1].Value = k1 + 1;
                    dgv.Rows[5].Cells[1].Value = k2 + 1;
                    dgv.Rows[6].Cells[1].Value = angle;
                    dgv.Rows[7].Cells[1].Value = Math.Round(r1, 1);
                    dgv.Rows[8].Cells[1].Value = Math.Round(r2, 1);
                    (dgv.Rows[9].Cells[1] as DataGridViewComboBoxCell).Value = (dgv.Rows[9].Cells[1] as DataGridViewComboBoxCell).Items[mode];
                }
            }

            /// <summary>
            /// Возвращает строку с полным описанием BE
            /// </summary>
            /// <returns></returns>
            public string GetTag()
            {
                StringBuilder str = new StringBuilder();
                str.Clear();
                str.Append("BE ");
                str.Append(a + " ");
                str.Append(c + " ");
                str.Append(prop + " ");
                str.Append(i0 + " " + j0 + " ");
                str.Append(k1 + " " + k2 + " ");
                str.Append(r1 + " " + r2 + " ");
                str.Append(angle + " ");
                str.Append(mode + " ");

                return str.ToString();
            }

            /// <summary>
            /// Возвращает строку с удобочитаемым описанием BE
            /// </summary>
            /// <returns></returns>
            public string GetName()
            {
                StringBuilder str = new StringBuilder();
                str.Clear();
                str.Append("BullEye ");
                str.Append(Model.Props[prop].Name);
                str.Append("*" + a);
                str.Append("+" + c + " Center:");
                str.Append((i0 + 1) + "/" + (j0 + 1) + "; ");
                str.Append(" K:" + (k1 + 1) + "/" + (k2 + 1) + "; ");
                str.Append(" range:" + Math.Round(r1) + "/" + Math.Round(r2) + "; ");
                str.Append("angle:" + Math.Round(angle) + "; ");
                str.Append("mode:" + mode);

                return str.ToString();
            }

            /// <summary>
            /// Воспроизводит объект из строки с описанием BE
            /// </summary>
            /// <param name="str"></param>
            public void Read(string str)
            {
                string[] split = str.Split(' ');
                a = Convert.ToDouble(split[1]);
                c = Convert.ToDouble(split[2]);
                prop = Convert.ToInt32(split[3]);
                i0 = Convert.ToInt32(split[4]);
                j0 = Convert.ToInt32(split[5]);
                k1 = Convert.ToInt32(split[6]);
                k2 = Convert.ToInt32(split[7]);
                r1 = Convert.ToDouble(split[8]);
                r2 = Convert.ToDouble(split[9]);
                angle = Convert.ToDouble(split[10]);
                mode = Convert.ToInt32(split[11]);
            }

            /// <summary>
            /// Выполняет модификацию солгасно заданных в объекте параметров
            /// </summary>
            public void Process()
            {
                double r1p = r1 / 1.5; // r1/0.987283876f
                double r2p = r2 / 1.5;
                double d = r1p > r2p ? r1p : r2p;
                double angleSin = Math.Sin((180 - angle) / 180 * Math.PI);
                double angleCos = Math.Cos((180 - angle) / 180 * Math.PI);
                double cX = Model.Cell.CenterX(i0, j0);
                double cY = Model.Cell.CenterY(i0, j0);
                double distM;
                double X, Y;
                int i, j, k;

                if (a != 1.0d && a != 0.0d)
                    distM = d * Math.Exp(1.0d / 3.0d) * Math.Log(Math.Log(Math.Abs((a - 1.0f) / a * 100f)));
                else
                    distM = d;

                int ii = Convert.ToInt32(Math.Round(distM / Model.CellSizeI)) + 1;// +10
                int jj = Convert.ToInt32(Math.Round(distM / Model.CellSizeJ)) + 1;// +10

                if (a != 1 || c != 0)
                    for (i = i0 - ii; i <= i0 + ii; i++)
                        for (j = j0 - jj; j <= j0 + jj; j++)
                            if (i >= 0 && i < Model.NI && j >= 0 && j < Model.NJ)
                            {
                                X = (Model.Cell.CenterX(i, j) - cX) * angleCos - (Model.Cell.CenterY(i, j) - cY) * angleSin;
                                Y = (Model.Cell.CenterX(i, j) - cX) * angleSin + (Model.Cell.CenterY(i, j) - cY) * angleCos;

                                #region Mult
                                if (a == 0)
                                    for (k = k1; k <= k2; k++)
                                        Model.Props[prop].Mult[i, j, k] = 0;
                                else
                                    if (a != 1)
                                    {
                                        d = (a - 1) * Math.Exp(-Math.Pow(Math.Abs(X / r1p), 3) - Math.Pow(Math.Abs(Y / r2p), 3)) + 1;

                                        if (Math.Abs(d - 1) >= 0.05d * Math.Abs(a - 1))
                                            for (k = k1; k <= k2; k++)
                                                switch (mode)
                                                {
                                                    case 0: // equals
                                                        Model.Props[prop].Mult[i, j, k] = Convert.ToSingle(d);
                                                        break;
                                                    case 1: // merge
                                                        Model.Props[prop].Mult[i, j, k] = Model.Props[prop].Mult[i, j, k] * Convert.ToSingle(d);
                                                        break;
                                                    case 3: // max
                                                        Model.Props[prop].Mult[i, j, k] = Model.Props[prop].Mult[i, j, k] > Convert.ToSingle(d) ? Model.Props[prop].Mult[i, j, k] : Convert.ToSingle(d);
                                                        break;
                                                    case 2: // min
                                                        Model.Props[prop].Mult[i, j, k] = Model.Props[prop].Mult[i, j, k] < Convert.ToSingle(d) ? Model.Props[prop].Mult[i, j, k] : Convert.ToSingle(d);
                                                        break;
                                                }
                                    }
                                #endregion

                                #region Add
                                if ((Math.Abs(Math.Pow(X / r1, 3)) + Math.Abs(Math.Pow(Y / r2, 3))) <= 1)// add 0.987283876
                                    for (k = k1; k <= k2; k++)
                                        switch (mode)
                                        {
                                            case 0:
                                                Model.Props[prop].Add[i, j, k] = Convert.ToSingle(c);
                                                break;
                                            case 1:
                                                Model.Props[prop].Add[i, j, k] = Model.Props[prop].Add[i, j, k] + Convert.ToSingle(c);
                                                break;
                                            case 3:
                                                Model.Props[prop].Add[i, j, k] = Model.Props[prop].Add[i, j, k] > Convert.ToSingle(c) ? Model.Props[prop].Add[i, j, k] : Convert.ToSingle(c);
                                                break;
                                            case 2:
                                                Model.Props[prop].Add[i, j, k] = Model.Props[prop].Add[i, j, k] < Convert.ToSingle(c) ? Model.Props[prop].Add[i, j, k] : Convert.ToSingle(c);
                                                break;
                                        }
                                #endregion
                            }

                #region old
                /*for (k = k1; k <= k2; k++)
                    for (i = i0 - ii; i <= i0 + ii; i++)
                        for (j = j0 - jj; j <= j0 + jj; j++)
                            if (i >= 0 && i < NI && j >= 0 && j < NJ && k >= 0 && k < NK)
                            {
                                X = (Cell.CenterX(i, j) - cX) * angleCos - (Cell.CenterY(i, j) - cY) * angleSin;
                                Y = (Cell.CenterX(i, j) - cX) * angleSin + (Cell.CenterY(i, j) - cY) * angleCos;

                                #region Mult
                                if (a==0)
                                    Props[CurrentProperty].Mult[i, j, k] = 0;
                                else
                                if (a != 1) 
                                {
                                    d = (a - 1) * Math.Exp(-Math.Pow(Math.Abs(X / r1p), 3) - Math.Pow(Math.Abs(Y / r2p), 3)) + 1;
                                    
                                    if (Math.Abs(d - 1) >= 0.05d * Math.Abs(a - 1))
                                        switch (mode)
                                        {
                                            case 0: // equals
                                                Props[CurrentProperty].Mult[i, j, k] = Convert.ToSingle(d);
                                                break;
                                            case 1: // merge
                                                Props[CurrentProperty].Mult[i, j, k] = Props[CurrentProperty].Mult[i, j, k] * Convert.ToSingle(d);
                                                break;
                                            case 2: // max
                                                Props[CurrentProperty].Mult[i, j, k] = Props[CurrentProperty].Mult[i, j, k] > Convert.ToSingle(d) ? Props[CurrentProperty].Mult[i, j, k] : Convert.ToSingle(d);
                                                break;
                                            case 3: // min
                                                Props[CurrentProperty].Mult[i, j, k] = Props[CurrentProperty].Mult[i, j, k] < Convert.ToSingle(d) ? Props[CurrentProperty].Mult[i, j, k] : Convert.ToSingle(d);
                                                break;
                                        }
                                }
                                #endregion

                                #region Add
                                if ((Math.Abs(Math.Pow(X / r1p, 3)) + Math.Abs(Math.Pow(Y / r2p, 3))) <= 0.987283876)// add
                                    switch (mode)
                                    {
                                        case 0:
                                            Props[CurrentProperty].Add[i, j, k] = Convert.ToSingle(c);
                                            break;
                                        case 1:
                                            Props[CurrentProperty].Add[i, j, k] = Props[CurrentProperty].Add[i, j, k] + Convert.ToSingle(c);
                                            break;
                                        case 2:
                                            Props[CurrentProperty].Add[i, j, k] = Props[CurrentProperty].Add[i, j, k] > Convert.ToSingle(c) ? Props[CurrentProperty].Add[i, j, k] : Convert.ToSingle(c);
                                            break;
                                        case 3:
                                            Props[CurrentProperty].Add[i, j, k] = Props[CurrentProperty].Add[i, j, k] < Convert.ToSingle(c) ? Props[CurrentProperty].Add[i, j, k] : Convert.ToSingle(c);
                                            break;
                                    }
                                #endregion
                            }
                 */
                #endregion
                //RecalculateValues();
            }
        }
    }
}
