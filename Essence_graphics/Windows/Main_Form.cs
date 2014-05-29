using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.IO;

namespace Essence_graphics
{
    public partial class Main_Form : Form
    {
        #region Переменные
        public CModel Model;

        public BackgroundWorker BW_ProcessNodes = new BackgroundWorker();

        Font font = new Font(FontFamily.GenericSansSerif, 10);

        FileStream EssFile;
        StreamWriter EssFileWrite;
        StreamReader EssFileRead;

        /// <summary>
        /// Флаг инициализации окна разреза I
        /// </summary>
        bool _loaded_intI = false;
        /// <summary>
        /// Флаг инициализации окна разреза J
        /// </summary>
        bool _loaded_intJ = false;
        /// <summary>
        /// Флаг инициализации окна карты
        /// </summary>
        bool _loaded_map = false;
        double ratio;
        /// <summary>
        /// Последнее выбранное значение индексов карты
        /// </summary>
        int[] IJ = new int[2];
        /// <summary>
        /// Флаг обработки смещения карты
        /// </summary>
        private bool moving;
        /// <summary>
        /// Предыдущее положение мыши
        /// </summary>
        private Point prevLoc;
        /// <summary>
        /// Хэндл для DrawList'a окна Map
        /// </summary>
        int hDLMap;
        /// <summary>
        /// Хэндл для DrawList'a окна Intersection-I
        /// </summary>
        int hDLInteI;
        /// <summary>
        /// Хэндл для DrawList'a окна Intersection-J
        /// </summary>
        int hDLInteJ;
        /// <summary>
        /// Хэндл для DrawList'a пересечения текущего выбора IJ
        /// </summary>
        int hDLCross;
        /// <summary>
        /// Соотношение размеров окон до схлопывания.
        /// </summary>
        float int_h_prev;
        /// <summary>
        /// Текущий инструмент
        /// </summary>
        int mode = 0;
        /// <summary>
        /// Ссылка на редактируемый нод
        /// </summary>
        static class editingNode
        {
            public static TreeNode node;
            public static bool renameable = false;
            public static byte type = 0;
            public static void Clear()
            {
                node = null;
                renameable = false;
                type = 0;
            }
        }
        #endregion

        #region Hot-Keys processing
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    Button_Cancel_Click(null, null);
                    return true;
                case Keys.F2:
                    renameToolStripMenuItem_Click(TV_boxes, new EventArgs());
                    return true;
                case Keys.G | Keys.Control:
                    groupToolStripMenuItem_Click(TV_boxes, new EventArgs());
                    return true;
                case Keys.Enter:
                    if (ActiveControl == TB_Krange.Control)
                        B_KRangeApply_Click(TB_Krange, new EventArgs());
                    return true;
            }

            // base implementation
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        public void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            StatusPBar.Value = e.ProgressPercentage;
            if (e.UserState != null) State.Text = (string)e.UserState;
        }

        #region OnLoad

        public Main_Form()
        {
            Model = new CModel(this);

            InitializeComponent();

            glc_intersectionI.Enabled = false;
            glc_intersectionJ.Enabled = false;
            glc_map.Enabled = false;

            glc_intersectionI.Visible = false;
            glc_intersectionJ.Visible = false;
            glc_map.Visible = false;

            BW_ProcessNodes.WorkerSupportsCancellation = true;
            BW_ProcessNodes.DoWork += BW_ProcessNodesEntryPoint;
            BW_ProcessNodes.RunWorkerCompleted += BW_ProcessNodesComplited;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //base.OnLoad(e);
        }

        /*protected override void OnUnload(EventArgs e)
        {
            if (Model.Wells != null)
            {
                foreach (CModel.Well well in Model.Wells)
                    GL.DeleteTexture(well.texHandle);
                GL.DeleteLists(hDLMap, 1);
                GL.DeleteLists(hDLInteI, 1);
                GL.DeleteLists(hDLInteJ, 1);
            }
        }*/

        /// <summary>
        /// Событие закгрузки окна карты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glc_map_Load(object sender, EventArgs e)
        {
            _loaded_map = true;
            hDLMap = GL.GenLists(1);
            hDLCross = GL.GenLists(1);
            glc_map.MakeCurrent();
            GL.ClearColor(Color.WhiteSmoke);
            //GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            //GL.Enable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Equal);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            //GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ShadeModel(ShadingModel.Smooth);
            SetupViewport(ref glc_map);
        }

        /// <summary>
        /// Событие загрузки окна разреза
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glc_intersection_Load(object sender, EventArgs e)
        {
            _loaded_intI = true;
            hDLInteI = GL.GenLists(1);
            glc_intersectionI.MakeCurrent();
            GL.ClearColor(Color.WhiteSmoke);
            //GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            //GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            SetupViewport(ref glc_intersectionI);
        }

        private void glc_intersectionJ_Load(object sender, EventArgs e)
        {
            _loaded_intJ = true;
            hDLInteJ = GL.GenLists(1);
            glc_intersectionJ.MakeCurrent();
            GL.ClearColor(Color.WhiteSmoke);
            //GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            SetupViewport(ref glc_intersectionJ);
        }
        #endregion

        #region OnPaint
        /// <summary>
        /// Событие отрисовки окна разреза
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glc_intersection_Paint(object sender, PaintEventArgs e)
        {
            if (!Model.IsInitialized || !_loaded_intI) return;

            glc_intersectionI.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.CallList(hDLInteI);

            glc_intersectionI.SwapBuffers();
        }

        /// <summary>
        /// Событие отрисовки окна разреза
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glc_intersectionJ_Paint(object sender, PaintEventArgs e)
        {
            if (!Model.IsInitialized || !_loaded_intJ) return;

            glc_intersectionJ.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.CallList(hDLInteJ);

            glc_intersectionJ.SwapBuffers();
        }

        /// <summary>
        /// Событие отрисовки окна карты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glc_map_Paint(object sender, PaintEventArgs e)
        {
            if (!Model.IsInitialized || !_loaded_map) return;
            glc_map.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.CallList(hDLMap); // draw map, wells

            GL.CallList(hDLCross); // draw IJ cross

            if (Model.Bulleye.state > 0) Model.Bulleye.Draw();
            if (Model.Reduce.state > 0) Model.Reduce.Draw();
            if (Model.Restore.state > 0) Model.Restore.Draw();
            
            UpdateTextMap(); // draw text-textures
            
            glc_map.SwapBuffers();
        }
        #endregion

        #region Supports
        /// <summary>
        /// Настройка области отображения
        /// </summary>
        /// <param name="GLC">ссылка на GL-control объект</param>
        public void SetupViewport(ref CustomGLControl GLC)
        {
            if (!Model.IsInitialized) return;

            GLC.MakeCurrent();
            int w = GLC.Width;
            int h = GLC.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            double ratio = GLC.AspectRatio;
            double x0 = 0, x1 = 0, y0 = 0, y1 = 0;
            double zmin = double.MaxValue, zmax = double.MinValue;
            switch (GLC.Name)
            {
                #region Map
                case "glc_map":
                    double dx = (Model.xmax - Model.xmin) / 2;
                    double dy = (Model.ymax - Model.ymin) / 2;
                    if (w / dx > h / dy)
                    {
                        x0 = Model.ViewCenter.X + Model.ViewOffset.X - dy * ratio * Model.ZoomMap;
                        x1 = Model.ViewCenter.X + Model.ViewOffset.X + dy * ratio * Model.ZoomMap;
                        y0 = Model.ViewCenter.Y + Model.ViewOffset.Y - dy * Model.ZoomMap;
                        y1 = Model.ViewCenter.Y + Model.ViewOffset.Y + dy * Model.ZoomMap;
                        GL.Ortho(Model.InvertX ? x1 : x0, Model.InvertX ? x0 : x1, Model.InvertY ? y1 : y0, Model.InvertY ? y0 : y1, -1, 1);
                    }
                    else
                    {
                        x0 = Model.ViewCenter.X + Model.ViewOffset.X - dx * Model.ZoomMap;
                        x1 = Model.ViewCenter.X + Model.ViewOffset.X + dx * Model.ZoomMap;
                        y0 = Model.ViewCenter.Y + Model.ViewOffset.Y - dx / ratio * Model.ZoomMap;
                        y1 = Model.ViewCenter.Y + Model.ViewOffset.Y + dx / ratio * Model.ZoomMap;
                        GL.Ortho(Model.InvertX ? x1 : x0, Model.InvertX ? x0 : x1, Model.InvertY ? y1 : y0, Model.InvertY ? y0 : y1, -1, 1);
                    }
                    break;
                #endregion
                #region Intersections
                case "glc_intersectionI":
                    for (int x = Model.SelectedI - Model.IJRange; x <= Model.SelectedI + Model.IJRange; x++)
                        if (x >= 0 && x < Model.NI)
                        {
                            if (Model.zcorn[x * 2, Model.SelectedJ * 2, 0] < zmin) zmin = Model.zcorn[x * 2, Model.SelectedJ * 2, 0];
                            if (Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, 0] < zmin) zmin = Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, 0];
                            if (Model.zcorn[x * 2, Model.SelectedJ * 2, Model.NK * 2 - 1] > zmax) zmax = Model.zcorn[x * 2, Model.SelectedJ * 2, Model.NK * 2 - 1];
                            if (Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, Model.NK * 2 - 1] > zmax) zmax = Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, Model.NK * 2 - 1];
                        }
                    for (int x = Model.SelectedJ - Model.IJRange; x <= Model.SelectedJ + Model.IJRange; x++)
                        if (x >= 0 && x < Model.NJ)
                        {
                            if (Model.zcorn[Model.SelectedI * 2, x * 2, 0] < zmin) zmin = Model.zcorn[Model.SelectedI * 2, x * 2, 0];
                            if (Model.zcorn[Model.SelectedI * 2, x * 2 + 1, 0] < zmin) zmin = Model.zcorn[Model.SelectedI * 2, x * 2 + 1, 0];
                            if (Model.zcorn[Model.SelectedI * 2, x * 2, Model.NK * 2 - 1] > zmax) zmax = Model.zcorn[Model.SelectedI * 2, x * 2, Model.NK * 2 - 1];
                            if (Model.zcorn[Model.SelectedI * 2, x * 2 + 1, Model.NK * 2 - 1] > zmax) zmax = Model.zcorn[Model.SelectedI * 2, x * 2 + 1, Model.NK * 2 - 1];
                        }

                    y0 = zmax;
                    y1 = zmin;

                    x0 = -Model.IJRange * Model.CellSizeI;
                    x1 = (Model.IJRange + 1) * Model.CellSizeI;

                    /*y0 = Model.ViewCenter.Z + Model.ViewOffset.Z - (Model.zmin - Model.zmax) / 2 * Model.ZoomIntersection;
                    y1 = Model.ViewCenter.Z + Model.ViewOffset.Z + (Model.zmin - Model.zmax) / 2 * Model.ZoomIntersection;*/

                    GL.Ortho(x0, x1, y0, y1, -1, 1);
                    break;
                case "glc_intersectionJ":
                    for (int x = Model.SelectedI - Model.IJRange; x <= Model.SelectedI + Model.IJRange; x++)
                        if (x >= 0 && x < Model.NI)
                        {
                            if (Model.zcorn[x * 2, Model.SelectedJ * 2, 0] < zmin) zmin = Model.zcorn[x * 2, Model.SelectedJ * 2, 0];
                            if (Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, 0] < zmin) zmin = Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, 0];
                            if (Model.zcorn[x * 2, Model.SelectedJ * 2, Model.NK * 2 - 1] > zmax) zmax = Model.zcorn[x * 2, Model.SelectedJ * 2, Model.NK * 2 - 1];
                            if (Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, Model.NK * 2 - 1] > zmax) zmax = Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, Model.NK * 2 - 1];
                        }
                    for (int x = Model.SelectedJ - Model.IJRange; x <= Model.SelectedJ + Model.IJRange; x++)
                        if (x >= 0 && x < Model.NJ)
                        {
                            if (Model.zcorn[Model.SelectedI * 2, x * 2, 0] < zmin) zmin = Model.zcorn[Model.SelectedI * 2, x * 2, 0];
                            if (Model.zcorn[Model.SelectedI * 2, x * 2 + 1, 0] < zmin) zmin = Model.zcorn[Model.SelectedI * 2, x * 2 + 1, 0];
                            if (Model.zcorn[Model.SelectedI * 2, x * 2, Model.NK * 2 - 1] > zmax) zmax = Model.zcorn[Model.SelectedI * 2, x * 2, Model.NK * 2 - 1];
                            if (Model.zcorn[Model.SelectedI * 2, x * 2 + 1, Model.NK * 2 - 1] > zmax) zmax = Model.zcorn[Model.SelectedI * 2, x * 2 + 1, Model.NK * 2 - 1];
                        }

                    y0 = zmax;
                    y1 = zmin;

                    x0 = -Model.IJRange * Model.CellSizeJ;
                    x1 = (Model.IJRange + 1) * Model.CellSizeJ;
                    //y0 = Model.ViewCenter.Z + Model.ViewOffset.Z - (Model.zmin - Model.zmax) / 2 * Model.ZoomIntersection;
                    //y1 = Model.ViewCenter.Z + Model.ViewOffset.Z + (Model.zmin - Model.zmax) / 2 * Model.ZoomIntersection;
                    GL.Ortho(x0, x1, y0, y1, -1, 1);
                    break;
                #endregion
            }
            GL.Viewport(0, 0, w, h);
        }

        /// <summary>
        /// Событие изменения размеров окна (переразметка GL-окон)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Form_Resize(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            if (!_loaded_intI || !_loaded_intJ || !_loaded_map) return;

            SetupViewport(ref glc_map);
            Inter_Container.SplitterDistance = (Inter_Container.Width - 3) / 2;
            SetupViewport(ref glc_intersectionI);
            SetupViewport(ref glc_intersectionJ);
        }
        #endregion
        
        #region Mouse Events
        double glX, glY;
        double glXold, glYold;
        /// <summary>
        /// Обработка событий мыши.
        /// Определение смещения центра отображения при флаге moving.
        /// Определение текущих координат.
        /// Обновление строки статуса.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glc_map_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Model.IsInitialized) return;
            ratio = glc_map.AspectRatio;
            double dx = (Model.xmax - Model.xmin) / 2;
            double dy = (Model.ymax - Model.ymin) / 2;
            #region Map moving calcs
            if (moving)
            {
                if (glc_map.Width / dx > glc_map.Height / dy)
                {
                    Model.ViewOffset.X = Model.ViewOffset.X - ((double)(e.X - prevLoc.X)) / glc_map.Width * dy * 2 * ratio * Model.ZoomMap * (Model.InvertX ? -1 : 1);
                    Model.ViewOffset.Y = Model.ViewOffset.Y + ((double)(e.Y - prevLoc.Y)) / glc_map.Height * dy * 2 * Model.ZoomMap * (Model.InvertY ? -1 : 1);
                }
                else
                {
                    Model.ViewOffset.X = Model.ViewOffset.X - ((double)(e.X - prevLoc.X)) / glc_map.Width * dx * 2 * Model.ZoomMap * (Model.InvertX ? -1 : 1);
                    Model.ViewOffset.Y = Model.ViewOffset.Y + ((double)(e.Y - prevLoc.Y)) / glc_map.Height * dx * 2 / ratio * Model.ZoomMap * (Model.InvertY ? -1 : 1);
                }
                prevLoc = e.Location;
                SetupViewport(ref glc_map);
                glc_map.Invalidate();
                return;
            }
            #endregion

            #region Calculating geo-coord of mouse pointer
            if (glc_map.Width / dx > glc_map.Height / dy)
            {
                glX = Model.ViewCenter.X + Model.ViewOffset.X - dy * ratio * Model.ZoomMap + ((Model.InvertX) ? (double)(glc_map.Width - e.X) : (double)e.X) / (glc_map.Width) * dy * 2 * ratio * Model.ZoomMap;
                glY = Model.ViewCenter.Y + Model.ViewOffset.Y - dy * Model.ZoomMap + (Model.InvertY ? (double)(e.Y) : (double)(glc_map.Height - e.Y)) / (glc_map.Height) * dy * 2 * Model.ZoomMap;
            }
            else
            {
                glX = Model.ViewCenter.X + Model.ViewOffset.X - dx * Model.ZoomMap + (Model.InvertX ? (double)(glc_map.Width - e.X) : (double)e.X) / (glc_map.Width ) * dx * 2 * Model.ZoomMap;
                glY = Model.ViewCenter.Y + Model.ViewOffset.Y - dx / ratio * Model.ZoomMap + (Model.InvertY ? (double)(e.Y) : (double)(glc_map.Height - e.Y)) / (glc_map.Height) * dx * 2 / ratio * Model.ZoomMap;
            }

            IJ = Model.FindIJ_Map(glX, glY);
            #endregion

            #region BullEye processing
            if (mode == 1 && Model.Bulleye.state > 0 && e.Location != Model.Bulleye.prevLoc)
            {
                double glXc = (Model.coord[Model.Bulleye.i0, Model.Bulleye.j0, 0].X + Model.coord[Model.Bulleye.i0 + 1, Model.Bulleye.j0, 0].X + Model.coord[Model.Bulleye.i0, Model.Bulleye.j0 + 1, 0].X + Model.coord[Model.Bulleye.i0 + 1, Model.Bulleye.j0 + 1, 0].X) / 4;
                double glYc = (Model.coord[Model.Bulleye.i0, Model.Bulleye.j0, 0].Y + Model.coord[Model.Bulleye.i0 + 1, Model.Bulleye.j0, 0].Y + Model.coord[Model.Bulleye.i0, Model.Bulleye.j0 + 1, 0].Y + Model.coord[Model.Bulleye.i0 + 1, Model.Bulleye.j0 + 1, 0].Y) / 4;

                if (Model.Bulleye.state == 1)
                {
                    Model.Bulleye.r1 = Math.Pow(Math.Pow(glX - glXc, 2d) + Math.Pow(glY - glYc, 2d), 0.5d);
                    Model.Bulleye.angle = Math.Asin((glY - glYc) / Model.Bulleye.r1) / Math.PI * 180d;
                    if (glX - glXc < 0) Model.Bulleye.angle = 180 - Model.Bulleye.angle;
                    Model.Bulleye.UpdateTable(dataGridProps);
                    glXold = glX; glYold = glY;
                }
                if (Model.Bulleye.state == 2)
                {
                    //Model.Bulleye.r2 = Math.Pow(Math.Pow(glX - glXc, 2d) + Math.Pow(glY - glYc, 2d), 0.5d);
                    Model.Bulleye.r2 = Math.Abs((glYold - glYc) * (glX - glXc) + (glXc - glXold) * (glY - glYc)) / Model.Bulleye.r1;
                    Model.Bulleye.UpdateTable(dataGridProps);
                }

                Model.Bulleye.curLoc = e.Location;
                glc_map.Invalidate();
            }
            #endregion

            #region Reduce processing
            if (mode == 2 && Model.Reduce.state > 0 && e.Location != Model.Reduce.prevLoc)
            {
                double glXc = Model.Cell.CenterX(Model.Reduce.i0, Model.Reduce.j0);
                double glYc = Model.Cell.CenterY(Model.Reduce.i0, Model.Reduce.j0); ;

                if (Model.Reduce.state == 1)
                {
                    Model.Reduce.r1 = Math.Pow(Math.Pow(glX - glXc, 2d) + Math.Pow(glY - glYc, 2d), 0.5d);
                    Model.Reduce.angle = Math.Asin((glY - glYc) / Model.Reduce.r1) / Math.PI * 180d;
                    if (glX - glXc < 0) Model.Reduce.angle = 180 - Model.Reduce.angle;
                    Model.Reduce.UpdateTable(dataGridProps);
                    glXold = glX; glYold = glY;
                }
                if (Model.Reduce.state == 2)
                {
                    //Model.Reduce.r2 = Math.Pow(Math.Pow(glX - glXc, 2d) + Math.Pow(glY - glYc, 2d), 0.5d);
                    Model.Reduce.r2 = Math.Abs((glYold - glYc) * (glX - glXc) + (glXc - glXold) * (glY - glYc)) / Model.Reduce.r1;
                    Model.Reduce.UpdateTable(dataGridProps);
                }
                if (Model.Reduce.state == 3)
                {
                    double r = Math.Pow(Math.Pow(glX - glXc, 2d) + Math.Pow(glY - glYc, 2d), 0.5d);
                    double ang = Math.Asin((glY - glYc) / r) / Math.PI * 180d;
                    if (glX - glXc < 0) ang = 180 - ang;
                    ang = ang - Model.Reduce.angle;
                    r = r * ((Math.Abs(Math.Cos(ang / 180 * Math.PI) / Model.Reduce.r1) > Math.Abs(Math.Sin(ang / 180 * Math.PI) / Model.Reduce.r2)) ? Math.Abs(Math.Cos(ang / 180 * Math.PI) / Model.Reduce.r1) : Math.Abs(Math.Sin(ang / 180 * Math.PI) / Model.Reduce.r2));
                    Model.Reduce.r_inter = r > 1 ? 1 : r;
                    Model.Reduce.UpdateTable(dataGridProps);
                }
                //Model.Reduce.curLoc = e.Location;
                glc_map.Invalidate();
            }
            #endregion

            #region Restore processing
            if (mode == 3 && Model.Restore.state > 0 && (IJ[0] != Model.Restore.i1 || IJ[1] != Model.Restore.j1))
            {
                if (Model.Restore.state == 1)
                {
                    Model.Restore.i2 = IJ[0];
                    Model.Restore.j2 = IJ[1];
                    Model.Restore.UpdateTable(dataGridProps);
                }
                if (Model.Restore.state == 2)
                {
                    //Model.Restore.width = Math.Abs(Model.Restore.i2 - IJ[0]) + Math.Abs(Model.Restore.j2 - IJ[1]);
                    //Model.Restore.UpdateTable(dataGridProps);
                }
                glc_map.Invalidate();
            }
            #endregion

            #region Update state-string
            if (IJ[0] != -1)
                State.Text = "I:" + (IJ[0] + 1) + ", J:" + (IJ[1] + 1) + "; X:" + Math.Round(glX, 0) + ", Y:" + Math.Round(glY, 0) + "; Value = " + Math.Round(Model.Props[Model.CurrentProperty].Maps[Model.CurrentMapType].Value[IJ[0], IJ[1]], 3);
            #endregion
        }

        private void glc_map_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Model.IsInitialized) return;

            #region Right-Button Moving flag
            if (e.Button == MouseButtons.Right)
            {
                moving = true;
                prevLoc = e.Location;
            }
            #endregion

            #region Left button processing
            if (e.Button == MouseButtons.Left)
                switch (mode)
                {
                    #region case Picker
                    case 0:
                        if (IJ[0] != -1)
                        {
                            Model.SelectedI = IJ[0];
                            Model.SelectedJ = IJ[1];
                            UpdateDL_Cross();
                            UpdateDL_InterI();
                            UpdateDL_InterJ();
                            SetupViewport(ref glc_intersectionI);
                            SetupViewport(ref glc_intersectionJ);
                            glc_map.Invalidate();
                            glc_intersectionI.Invalidate();
                            glc_intersectionJ.Invalidate();
                        }
                        break;
                    #endregion
                    #region case BullEye
                    case 1:
                        switch (Model.Bulleye.state)
                        {
                            case 0:
                                if (IJ[0] != -1)
                                {
                                    Model.Bulleye.New();
                                    Model.Bulleye.i0 = IJ[0];
                                    Model.Bulleye.j0 = IJ[1];
                                    Model.Bulleye.UpdateTable(dataGridProps);
                                    Model.Bulleye.prevLoc = e.Location;
                                    Button_Cancel.Visible = true;
                                    Model.Bulleye.state++;
                                }
                                break;
                            case 1:
                                Model.Bulleye.prevLoc = e.Location;
                                Model.Bulleye.state++;
                                break;
                            case 2:
                                Windows.BullEye_Request dialog = new Windows.BullEye_Request();
                                dialog.ShowDialog();
                                while (dialog.DialogResult == DialogResult.None)
                                    dialog.ShowDialog();
                                if (dialog.DialogResult == DialogResult.OK)
                                {
                                    Model.Bulleye.a = Convert.ToDouble(dialog.Mult.Text);
                                    Model.Bulleye.c = Convert.ToDouble(dialog.Add.Text);
                                    Model.Bulleye.state++;
                                    Model.Bulleye.UpdateTable(dataGridProps);
                                    Button_OK.Visible = true;
                                }
                                else
                                {
                                    Button_Cancel.Visible = false;
                                    Button_Cancel_Click(sender, e);
                                }
                                dialog.Dispose();

                                break;
                            case 3:
                                // edition case
                                // make logic for editing center, angle and r1/r2
                                break;
                            case 10:
                                Model.Bulleye.New();
                                Model.Bulleye.i0 = IJ[0];
                                Model.Bulleye.j0 = IJ[1];
                                Model.Bulleye.UpdateTable(dataGridProps);
                                Model.Bulleye.prevLoc = e.Location;
                                Button_Cancel.Visible = true;
                                Model.Bulleye.state = 1;
                                break;
                        }
                        break;
                    #endregion
                    #region case Reduce
                    case 2:
                        switch (Model.Reduce.state)
                        {
                            case 0:
                                //center
                                if (IJ[0] != -1)
                                {
                                    Model.Reduce.New();
                                    Model.Reduce.i0 = IJ[0];
                                    Model.Reduce.j0 = IJ[1];
                                    Model.Reduce.UpdateTable(dataGridProps);
                                    Model.Reduce.prevLoc = e.Location;
                                    Button_Cancel.Visible = true;
                                    Model.Reduce.state++;
                                }
                                break;
                            case 1:
                                // r1 and angle
                                Model.Reduce.prevLoc = e.Location;
                                Model.Reduce.state++;
                                break;
                            case 2:
                                // r2
                                Model.Reduce.prevLoc = e.Location;
                                Model.Reduce.state++;
                                break;
                            case 3:
                                Windows.Reduce_Request dialog = new Windows.Reduce_Request();
                                while (dialog.DialogResult == DialogResult.None)
                                    dialog.ShowDialog();
                                if (dialog.DialogResult == DialogResult.OK)
                                {
                                    Model.Reduce.M = Convert.ToDouble(dialog.M.Text);
                                    Model.Reduce.dis = Convert.ToDouble(dialog.dis.Text);
                                    Model.Reduce.freq = Convert.ToDouble(dialog.freq.Text);
                                    Model.Reduce.mincoef = Convert.ToSingle(dialog.mincoef.Text);
                                    Model.Reduce.maxcoef = Convert.ToSingle(dialog.maxcoef.Text);
                                    Model.Reduce.state++;
                                    Model.Reduce.UpdateTable(dataGridProps);
                                    //Model.Reduce.state++;
                                    Button_OK.Visible = true;
                                }
                                else
                                {
                                    Button_Cancel.Visible = false;
                                    Button_Cancel_Click(sender, e);
                                }
                                dialog.Dispose();
                                break;
                            case 4:
                                // edition case 
                                break;
                            case 10:
                                Model.Reduce.New();
                                Model.Reduce.i0 = IJ[0];
                                Model.Reduce.j0 = IJ[1];
                                Model.Reduce.UpdateTable(dataGridProps);
                                Model.Reduce.prevLoc = e.Location;
                                Button_Cancel.Visible = true;
                                Model.Reduce.state = 1;
                                break;
                        }
                        break;
                    #endregion
                    #region case Restore
                    case 3:
                        switch (Model.Restore.state)
                        {
                            case 0:
                                if (IJ[0] != -1)
                                {
                                    Model.Restore.New();
                                    Model.Restore.i1 = IJ[0];
                                    Model.Restore.j1 = IJ[1];
                                    Model.Restore.UpdateTable(dataGridProps);
                                    Button_Cancel.Visible = true;
                                    Model.Restore.state++;
                                }
                                break;
                            case 1:
                                if (IJ[0] != -1)
                                {
                                    Model.Restore.i2 = IJ[0];
                                    Model.Restore.j2 = IJ[1];
                                    Model.Restore.state++;
                                    Windows.Restore_Request dialog = new Windows.Restore_Request();
                                    while (dialog.DialogResult == DialogResult.None)
                                        dialog.ShowDialog();
                                    if (dialog.DialogResult == DialogResult.OK)
                                    {
                                        Model.Restore.width = Convert.ToInt32(dialog.textBox1.Text);
                                        Model.Restore.state++;
                                        Model.Restore.UpdateTable(dataGridProps); 
                                        Button_OK.Visible = true;
                                    }
                                    else
                                    {
                                        Button_Cancel.Visible = false;
                                        Button_Cancel_Click(sender, e);
                                    }
                                    dialog.Dispose();
                                }
                                break;
                            case 2:
                                break;
                            case 10:
                                Model.Restore.New();
                                Model.Restore.i1 = IJ[0];
                                Model.Restore.j1 = IJ[1];
                                Model.Restore.UpdateTable(dataGridProps);
                                Button_Cancel.Visible = true;
                                Model.Restore.state = 1;
                                break;
                        }
                        break;
                    #endregion
                }
            #endregion
        }

        private void glc_map_MouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
        }

        private void glc_intersection_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Model.IsInitialized) return;

            #region Right button Moving flag
            if (e.Button == MouseButtons.Right)
            {
                moving = true;
                prevLoc = e.Location;
            }
            #endregion

            #region Left button processing
            if (e.Button == MouseButtons.Left)
            {
                prevLoc = e.Location;
            }
            #endregion
        }

        private void glc_intersection_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Model.IsInitialized) return;
            ratio = glc_intersectionI.AspectRatio;
            if (moving)
            {
                Model.ViewOffset.Z = Model.ViewOffset.Z - (double)(e.Y - prevLoc.Y) / glc_intersectionI.Height * (Model.zmax - Model.zmin) * Model.ZoomIntersection / (ratio < 1 ? (ratio) : 1);
                //Model.ViewOffset.Y = Model.ViewOffset.Y + (double)(e.Y - prevLoc.Y) / glc_map.Height * (Model.ymax - Model.ymin) * Model.ZoomMap / (ratio < 1 ? (ratio) : 1);
                prevLoc = e.Location;
                SetupViewport(ref glc_intersectionI);
                SetupViewport(ref glc_intersectionJ);
                glc_intersectionI.Invalidate();
                glc_intersectionJ.Invalidate();
                return;
            }
        }

        private void glc_intersection_MouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
        }

        #region useless shit :D
        private void glc_map_MouseEnter(object sender, EventArgs e)
        {
            ActiveControl = glc_map;
        }

        private void glc_intersection_MouseEnter(object sender, EventArgs e)
        {
            ActiveControl = glc_intersectionI;
        }

        private void glc_intersectionJ_MouseEnter(object sender, EventArgs e)
        {
            ActiveControl = glc_intersectionJ;
        }

        private void TV_boxes_MouseEnter(object sender, EventArgs e)
        {
            //ActiveControl = TV_boxes;
        }

        private void TV_Props_MouseEnter(object sender, EventArgs e)
        {
            //ActiveControl = TV_Props;
        }

        private void MapType_MouseEnter(object sender, EventArgs e)
        {
            ActiveControl = MapType.Control;
        }
        #endregion

        #region Zoom
        /// <summary>
        /// Обработка колесика мыши на окне карты (зум)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glc_map_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!Model.IsInitialized) return;
            if (e.Delta > 0) { Model.ZoomMap = Model.ZoomMap / 1.1; } else { Model.ZoomMap = Model.ZoomMap * 1.1; };
            SetupViewport(ref glc_map);
            glc_map.Invalidate();
        }

        /// <summary>
        /// Обработка колесика мыши на окне разреза (зум)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glc_intersection_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) { Model.IJRange = Model.IJRange - 1; } else { Model.IJRange = Model.IJRange + 1; };
            UpdateDL_Cross();
            UpdateDL_InterI();
            UpdateDL_InterJ();
            SetupViewport(ref glc_intersectionI);
            SetupViewport(ref glc_intersectionJ);
            glc_map.Invalidate();
            glc_intersectionI.Invalidate();
            glc_intersectionJ.Invalidate();
        }
        #endregion

        #endregion

        #region Form Controls
        public void UpdatePropsList()
        {
            TV_Props.BeginUpdate();
            TV_Props.Nodes.Clear();

            TV_Props.Nodes.Insert(0, "INPUT");
            TV_Props.Nodes.Insert(1, "OUTPUT");

            for (int i = 0; i < Model.Props.Count; i++)
            {
                TreeNode tn = new TreeNode();
                tn.Tag = "i" + i;
                tn.Text = Model.Props[i].Name;
                tn.Name = "i" + Model.Props[i].Name;
                TV_Props.Nodes[0].Nodes.Add(tn);
                tn = new TreeNode();
                tn.Tag = "o" + i;
                tn.Text = Model.Props[i].Name;
                tn.Name = "o" + Model.Props[i].Name;
                TV_Props.Nodes[1].Nodes.Add(tn);
            }
            TV_Props.SelectedNode = TV_Props.Nodes[0].Nodes[0];
            TV_Props.EndUpdate();

            //TS_KRange.Text = "1-" + Model.NZ;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // TODO model.dispose?
                TV_boxes.SelectedNodes = null;
                TV_boxes.Nodes.Clear();
                TV_Props.Nodes.Clear();

                glc_intersectionI.Enabled = false;
                glc_intersectionJ.Enabled = false;
                glc_map.Enabled = false;

                glc_intersectionI.Visible = false;
                glc_intersectionJ.Visible = false;
                glc_map.Visible = false;

                Model = new CModel(this);
                Model.BW_Reader.RunWorkerAsync(openFileDialog1.FileName);
                EssFile = new FileStream(openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.Length - 5) + ".ess", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                EssFileRead = new StreamReader(EssFile);
                ReadEssFile();
                EssFileRead.Dispose();
            }
        }

        /// <summary>
        /// Обработка изменения комбобокса с типом карты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            Model.CurrentMapType = MapType.SelectedIndex;
            UpdateDL_Map();
            glc_map.Invalidate();
            glc_intersectionI.Invalidate();
        }

        private void ButtonSwitchIJ_Click(object sender, EventArgs e)
        {
            Model.PaintI = !Model.PaintI;
            UpdateDL_InterI();
            SetupViewport(ref glc_intersectionI);
            glc_intersectionI.Invalidate();
            //glc_map.Invalidate();
        }

        private void ButtonShowGrid_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            Model.IsBordered = !Model.IsBordered;
            UpdateDL_Map();
            UpdateDL_InterI();
            UpdateDL_InterJ();
            glc_intersectionI.Invalidate();
            glc_intersectionJ.Invalidate();
            glc_map.Invalidate();
        }

        private void ButtonSwitchBack_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            Model.BlackBack = !Model.BlackBack;

            UpdateDL_Map();
            UpdateDL_InterI();
            UpdateDL_InterJ();

            glc_intersectionI.MakeCurrent();
            GL.ClearColor(Model.BlackBack ? Color.Black : Color.WhiteSmoke);
            glc_intersectionJ.MakeCurrent();
            GL.ClearColor(Model.BlackBack ? Color.Black : Color.WhiteSmoke);
            glc_map.MakeCurrent();
            GL.ClearColor(Model.BlackBack ? Color.Black : Color.WhiteSmoke);

            glc_intersectionI.Invalidate();
            glc_intersectionJ.Invalidate();
            glc_map.Invalidate();
        }

        private void InvertX_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            Model.InvertX = !Model.InvertX;
            SetupViewport(ref glc_map);
            glc_map.Invalidate();
        }

        private void InvertY_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            Model.InvertY = !Model.InvertY;
            SetupViewport(ref glc_map);
            glc_map.Invalidate();
        }

        private void TV_Props_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!Model.IsInitialized) return;
            if (e.Node.Text == "INPUT" || e.Node.Text == "OUTPUT") return;
            Model.CurrentProperty = Convert.ToInt32(e.Node.Tag.ToString().Substring(1, e.Node.Tag.ToString().Length - 1));
            if (e.Node.Tag.ToString().Substring(0, 1) == "o") Model.Edited = true; else Model.Edited = false;
            Model.RecalculateValues();
        }

        private void HideIntersection_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            if (glc_intersectionI.Visible == true)
            {
                int_h_prev = (float)DrawContainer.SplitterDistance / DrawContainer.Height;
                DrawContainer.SplitterDistance = DrawContainer.Height;
                glc_intersectionI.Visible = false;
                DrawContainer.IsSplitterFixed = true;
            }
            else
            {
                DrawContainer.SplitterDistance = Convert.ToInt32(int_h_prev * DrawContainer.Height);
                glc_intersectionI.Visible = true;
                DrawContainer.IsSplitterFixed = false;
            }
        }

        private void TSButtonResetView_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            Model.ViewOffset.X = 0;
            Model.ViewOffset.Y = 0;
            Model.ViewOffset.Z = 0;
            Model.ZoomIntersection = 1.05d;
            Model.ZoomMap = 1.05d;
            SetupViewport(ref glc_map);
            SetupViewport(ref glc_intersectionI);
            SetupViewport(ref glc_intersectionJ);
            glc_intersectionI.Invalidate();
            glc_intersectionJ.Invalidate();
            glc_map.Invalidate();
        }

        private void TSButtonPicker_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            TSButtonPicker.Checked = true;
            TSButtonReduce.Checked = false;
            TSButtonRestore.Checked = false;
            TSButtonBullEye.Checked = false;
            mode = 0;
            Model.Picker.UpdateTable(dataGridProps);
        }

        private void TSButtonBullEye_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            if (TSButtonBullEye.Checked == true)
            {
                TSButtonPicker.Checked = true;
                TSButtonBullEye.Checked = false;
                mode = 0;
                Model.Picker.UpdateTable(dataGridProps);
            }
            else
            {
                TSButtonBullEye.Checked = true;
                TSButtonPicker.Checked = false;
                mode = 1;
                State.Text = "Pick center for Bulleye";
                Model.Bulleye.UpdateTable(dataGridProps);
            }
            TSButtonReduce.Checked = false;
            TSButtonRestore.Checked = false;
        }

        private void TSButtonReduce_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            if (TSButtonReduce.Checked == true)
            {
                TSButtonPicker.Checked = true;
                TSButtonReduce.Checked = false;
                mode = 0;
                Model.Picker.UpdateTable(dataGridProps);
            }
            else
            {
                TSButtonReduce.Checked = true;
                TSButtonPicker.Checked = false;
                mode = 2;
                Model.Reduce.UpdateTable(dataGridProps);
            }
            TSButtonBullEye.Checked = false;
            TSButtonRestore.Checked = false;
        }

        private void TSButtonRestore_Click(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            if (TSButtonRestore.Checked == true)
            {
                TSButtonPicker.Checked = true;
                TSButtonRestore.Checked = false;
                mode = 0;
                Model.Picker.UpdateTable(dataGridProps);
            }
            else
            {
                TSButtonRestore.Checked = true;
                TSButtonPicker.Checked = false;
                mode = 3;
                Model.Restore.UpdateTable(dataGridProps);
            }
            TSButtonBullEye.Checked = false;
            TSButtonReduce.Checked = false;
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            TreeNode tn = new TreeNode();
            tn.Checked = true;
            switch (mode)
            {
                case 0:
                    tn=editingNode.node;
                    // Editing stance for selected node
                    // TODO remaking node
                    switch (editingNode.type)
                    {
                        case 1: // BE
                            if (editingNode.renameable)
                            {
                                tn.Text = Model.Bulleye.GetName();
                                tn.Name = Model.Bulleye.GetName();
                            }
                            tn.Tag = Model.Bulleye.GetTag();
                            break;
                        case 2: // RD
                            if (editingNode.renameable)
                            {
                                tn.Text = Model.Reduce.GetName();
                                tn.Name = Model.Reduce.GetName();
                            }
                            tn.Tag = Model.Reduce.GetTag();
                            break;
                        case 3: // RS
                            if (editingNode.renameable)
                            {
                                tn.Text = Model.Restore.GetName();
                                tn.Name = Model.Restore.GetName();
                            }
                            tn.Tag = Model.Restore.GetTag();
                            break;
                    }
                    
                    editingNode.node.BackColor = TV_boxes.BackColor;
                    TV_boxes.Invalidate();
                    editingNode.Clear();
                    Model.Picker.UpdateTable(dataGridProps);
                    break;
                case 1: // BE
                    Model.Bulleye.Process();
                    tn.Text = Model.Bulleye.GetName();
                    tn.Name = Model.Bulleye.GetName();
                    tn.Tag = Model.Bulleye.GetTag();
                    TV_boxes.Nodes.Add(tn);
                    Model.Bulleye.New();
                    Model.Bulleye.UpdateTable(dataGridProps);            
                    break;
                case 2: // RD
                    Model.Reduce.Process();
                    tn.Text = Model.Reduce.GetName();
                    tn.Name = Model.Reduce.GetName();
                    tn.Tag = Model.Reduce.GetTag();
                    TV_boxes.Nodes.Add(tn);
                    Model.Reduce.New();
                    Model.Reduce.UpdateTable(dataGridProps);
                    break;
                case 3: // RS
                    Model.Restore.Process();
                    tn.Text = Model.Restore.GetName();
                    tn.Name = Model.Restore.GetName();
                    tn.Tag = Model.Restore.GetTag();
                    TV_boxes.Nodes.Add(tn);
                    Model.Restore.New();
                    Model.Restore.UpdateTable(dataGridProps);
                    break;
            }
            Model.actnum.CheckNTGPORO();
            if (mode == 0) BW_ProcessNodes.RunWorkerAsync(); else Model.BW_RecalculateValues.RunWorkerAsync();
            Button_OK.Visible = false;
            Button_Cancel.Visible = false;
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            switch (mode)
            {
                case 0:
                    Model.Bulleye.New();
                    Model.Reduce.New();
                    Model.Restore.New();
                    Model.Picker.UpdateTable(dataGridProps);
                    editingNode.node.BackColor = TV_boxes.BackColor;
                    TV_boxes.Invalidate();
                    editingNode.Clear();
                    break;
                case 1:
                    Model.Bulleye.New();
                    Model.Bulleye.UpdateTable(dataGridProps);
                    break;
                case 2:
                    Model.Reduce.New();
                    Model.Reduce.UpdateTable(dataGridProps);
                    break;
                case 3:
                    Model.Restore.New();
                    Model.Restore.UpdateTable(dataGridProps);
                    break;
            }
            Button_OK.Visible = false;
            Button_Cancel.Visible = false;
            glc_map.Invalidate();
        }

        private void B_KRangeApply_Click(object sender, EventArgs e)
        {
            if (TB_Krange.Text != "" || !Model.IsInitialized)
            {
                string[] str = TB_Krange.Text.Split(' ', '\t', '-', ',', '.');
                if (str.Length < 3)
                {
                    int[] kr = new int[2];
                    if (str.Length == 2)
                    {
                        kr[0] = Convert.ToInt32(str[0]) - 1;
                        kr[1] = Convert.ToInt32(str[1]) - 1;

                    }
                    else
                    {
                        kr[0] = Convert.ToInt32(str[0]) - 1;
                        kr[1] = Convert.ToInt32(str[0]) - 1;
                    }
                    Model.KRange = kr;
                    UpdateDL_Map();
                    glc_map.Invalidate();

                }
            }
            TB_Krange.Text = (Model.KRange[0] + 1) + "-" + (Model.KRange[1] + 1);
        }

        private void dataGridProps_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!Model.IsInitialized) return;
            switch (dataGridProps.Tag.ToString())
            {
                #region Bulleye
                case "BE":
                    if (e.RowIndex == 0) Model.Bulleye.a = Convert.ToDouble(dataGridProps.Rows[0].Cells[1].Value);
                    if (e.RowIndex == 1) Model.Bulleye.c = Convert.ToDouble(dataGridProps.Rows[1].Cells[1].Value);
                    if (e.RowIndex == 2) Model.Bulleye.i0 = Convert.ToInt32(dataGridProps.Rows[2].Cells[1].Value) - 1;
                    if (e.RowIndex == 3) Model.Bulleye.j0 = Convert.ToInt32(dataGridProps.Rows[3].Cells[1].Value) - 1;
                    if (e.RowIndex == 4) Model.Bulleye.k1 = Convert.ToInt32(dataGridProps.Rows[4].Cells[1].Value) - 1;
                    if (e.RowIndex == 5) Model.Bulleye.k2 = Convert.ToInt32(dataGridProps.Rows[5].Cells[1].Value) - 1;
                    if (e.RowIndex == 6) Model.Bulleye.angle = Convert.ToDouble(dataGridProps.Rows[6].Cells[1].Value);
                    if (e.RowIndex == 7) Model.Bulleye.r1 = Convert.ToDouble(dataGridProps.Rows[7].Cells[1].Value);
                    if (e.RowIndex == 8) Model.Bulleye.r2 = Convert.ToDouble(dataGridProps.Rows[8].Cells[1].Value);
                    if (e.RowIndex == 9)
                    {
                        switch ((dataGridProps.Rows[9].Cells[1] as DataGridViewComboBoxCell).Value.ToString())
                        {
                            case "Set":
                                Model.Bulleye.mode = 0;
                                break;
                            case "Merge":
                                Model.Bulleye.mode = 1;
                                break;
                            case "Min":
                                Model.Bulleye.mode = 2;
                                break;
                            case "Max":
                                Model.Bulleye.mode = 3;
                                break;
                        }
                    }
                    break;
                #endregion
                #region Reduce
                case "RD":
                    switch (e.RowIndex)
                    {
                        case 0:
                            Model.Reduce.i0 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 1:
                            Model.Reduce.j0 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 2:
                            Model.Reduce.k1 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 3:
                            Model.Reduce.k2 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 4:
                            Model.Reduce.r1 = Convert.ToDouble(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                        case 5:
                            Model.Reduce.r2 = Convert.ToDouble(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                        case 6:
                            Model.Reduce.r_inter = Convert.ToDouble(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                        case 7:
                            Model.Reduce.angle = Convert.ToDouble(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                        case 8:
                            Model.Reduce.M = Convert.ToDouble(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                        case 9:
                            Model.Reduce.dis = Convert.ToDouble(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                        case 10:
                            Model.Reduce.freq = Convert.ToDouble(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                        case 11:
                            Model.Reduce.mincoef = Convert.ToSingle(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                        case 12:
                            Model.Reduce.maxcoef = Convert.ToSingle(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                    }
                    break;
                #endregion
                #region Restore
                case "RS":
                    switch (e.RowIndex)
                    {
                        case 0:
                            Model.Restore.i1 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 1:
                            Model.Restore.j1 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 2:
                            Model.Restore.i2 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 3:
                            Model.Restore.j2 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 4:
                            Model.Restore.k1 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 5:
                            Model.Restore.k2 = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value) - 1;
                            break;
                        case 6:
                            Model.Restore.width = Convert.ToInt32(dataGridProps.Rows[e.RowIndex].Cells[1].Value);
                            break;
                    }
                    break;
                #endregion
            }
            glc_map.Invalidate();
            glc_intersectionI.Invalidate();
            glc_intersectionJ.Invalidate();
        }

        private void TV_boxes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!Model.IsInitialized) return;
            switch (Model.Left(e.Node.Tag.ToString(), 2))
            {
                case "BE":
                    Model.Restore.New();
                    Model.Reduce.New();
                    Model.Bulleye.Read(e.Node.Tag.ToString());
                    Model.Bulleye.state = 10;
                    break;
                case "RD":
                    Model.Bulleye.New();
                    Model.Restore.New();
                    Model.Reduce.Read(e.Node.Tag.ToString());
                    Model.Reduce.state = 10;
                    break;
                case "RS":
                    Model.Reduce.New();
                    Model.Bulleye.New();
                    Model.Restore.Read(e.Node.Tag.ToString());
                    Model.Restore.state = 10;
                    break;
            }
            glc_map.Invalidate();
        }

        private void GarbageCollector_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }
        #endregion

        #region DrawLists
        /// <summary>
        /// Обновляет DrawList для Map-окна
        /// Исполнять при изменении свойства/типа карты
        /// </summary>
        public void UpdateDL_Map()
        {
            if (!_loaded_map||!Model.IsInitialized||!glc_map.Enabled) return;
            int x = 0, y = 0;
            GL.NewList(hDLMap, ListMode.Compile);
            #region draw cells
            GL.Begin(PrimitiveType.Triangles);
            for (x = 0; x < Model.NI; x++)
                for (y = 0; y < Model.NJ; y++)
                    if (!double.IsNaN(Model.Props[Model.CurrentProperty].Maps[0].Value[x, y]))
                    {
                        GL.Color3(Model.GetColor(x, y));
                        GL.Vertex2(Model.coord[x, y, 0].X, Model.coord[x, y, 0].Y);
                        GL.Vertex2(Model.coord[x + 1, y, 0].X, Model.coord[x + 1, y, 0].Y);
                        GL.Vertex2(Model.coord[x, y + 1, 0].X, Model.coord[x, y + 1, 0].Y);
                        GL.Vertex2(Model.coord[x + 1, y, 0].X, Model.coord[x + 1, y, 0].Y);
                        GL.Vertex2(Model.coord[x, y + 1, 0].X, Model.coord[x, y + 1, 0].Y);
                        GL.Vertex2(Model.coord[x + 1, y + 1, 0].X, Model.coord[x + 1, y + 1, 0].Y);
                    }
            GL.End();
            #endregion
            #region draw borders
            if (Model.IsBordered)
            {
                GL.LineWidth(1f);
                if (!Model.BlackBack) GL.Color3(Color.Black); else GL.Color3(Color.WhiteSmoke);
                GL.Begin(PrimitiveType.Lines);
                for (x = 0; x < Model.NI; x++)
                    for (y = 0; y < Model.NJ; y++)
                        if (!double.IsNaN(Model.Props[Model.CurrentProperty].Maps[0].Value[x, y]))
                        {
                            GL.Vertex2(Model.coord[x, y, 0].X, Model.coord[x, y, 0].Y);
                            GL.Vertex2(Model.coord[x + 1, y, 0].X, Model.coord[x + 1, y, 0].Y);
                            GL.Vertex2(Model.coord[x + 1, y, 0].X, Model.coord[x + 1, y, 0].Y);
                            GL.Vertex2(Model.coord[x + 1, y + 1, 0].X, Model.coord[x + 1, y + 1, 0].Y);
                            GL.Vertex2(Model.coord[x + 1, y + 1, 0].X, Model.coord[x + 1, y + 1, 0].Y);
                            GL.Vertex2(Model.coord[x, y + 1, 0].X, Model.coord[x, y + 1, 0].Y);
                            GL.Vertex2(Model.coord[x, y + 1, 0].X, Model.coord[x, y + 1, 0].Y);
                            GL.Vertex2(Model.coord[x, y, 0].X, Model.coord[x, y, 0].Y);
                        }
                GL.End();
            }
            #endregion
            #region draw wells
            if (Model.Wells != null)
            {
                GL.Color3(Color.Black);
                GL.LineWidth(4.0f);
                foreach (CModel.Well well in Model.Wells)
                    if (well.Connections!=null)
                    {
                        GL.Begin(PrimitiveType.LineStrip);
                        double xw, yw;
                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.K1 >= Model.KRange[0] && con.K2 <= Model.KRange[1])
                            {

                                xw = (Model.coord[con.I, con.J, 0].X + Model.coord[con.I + 1, con.J, 0].X + Model.coord[con.I, con.J + 1, 0].X + Model.coord[con.I + 1, con.J + 1, 0].X) / 4;
                                yw = (Model.coord[con.I, con.J, 0].Y + Model.coord[con.I + 1, con.J, 0].Y + Model.coord[con.I, con.J + 1, 0].Y + Model.coord[con.I + 1, con.J + 1, 0].Y) / 4;
                                GL.Vertex2(xw, yw);
                            }
                        GL.End();
                    }
                GL.PointSize(8.0f);
                GL.Begin(PrimitiveType.Points);
                foreach (CModel.Well well in Model.Wells)
                    if (well.Connections != null)
                    {
                        double xw, yw;
                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.K1 >= Model.KRange[0] && con.K2 <= Model.KRange[1])
                            {
                                xw = (Model.coord[con.I, con.J, 0].X + Model.coord[con.I + 1, con.J, 0].X + Model.coord[con.I, con.J + 1, 0].X + Model.coord[con.I + 1, con.J + 1, 0].X) / 4;
                                yw = (Model.coord[con.I, con.J, 0].Y + Model.coord[con.I + 1, con.J, 0].Y + Model.coord[con.I, con.J + 1, 0].Y + Model.coord[con.I + 1, con.J + 1, 0].Y) / 4;
                                GL.Vertex2(xw, yw);
                            }
                    }
                GL.End();
                GL.Color3(Color.White);
                GL.PointSize(4.0f);
                GL.Begin(PrimitiveType.Points);
                foreach (CModel.Well well in Model.Wells)
                    if (well.Connections != null)
                    {
                        double xw, yw;
                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.K1 >= Model.KRange[0] && con.K2 <= Model.KRange[1])
                            {

                                xw = Model.Cell.CenterX(con.I, con.J, 0);
                                yw = Model.Cell.CenterY(con.I, con.J, 0);
                                GL.Vertex2(xw, yw);
                            }
                    }
                GL.End();
            }
            #endregion
            GL.EndList();
        }

        /// <summary>
        /// Перекрестие IJ
        /// </summary>
        public void UpdateDL_Cross()
        {
            if (!_loaded_map || !Model.IsInitialized || !glc_map.Enabled) return;
            GL.NewList(hDLCross, ListMode.Compile);

            GL.LineWidth(3);
            
            GL.Color4(1, 0, 0, 0.5);
            GL.Begin(PrimitiveType.LineStrip);
            for (int x = -Model.IJRange; x <= Model.IJRange; x++)
                if ((Model.SelectedI + x) >= 0 && (Model.SelectedI + x) < Model.NI)
                {
                    GL.Vertex2(Model.Cell.CenterX(Model.SelectedI + x, Model.SelectedJ), Model.Cell.CenterY(Model.SelectedI + x, Model.SelectedJ));
                }
            GL.End();

            GL.Color4(0, 0, 1, 0.5);
            GL.Begin(PrimitiveType.LineStrip);
            for (int x = -Model.IJRange; x <= Model.IJRange; x++)
                if ((Model.SelectedJ + x) >= 0 && (Model.SelectedJ + x) < Model.NJ)
                {
                    GL.Vertex2(Model.Cell.CenterX(Model.SelectedI, Model.SelectedJ + x), Model.Cell.CenterY(Model.SelectedI, Model.SelectedJ + x));
                }
            GL.End();
            GL.EndList();
        }

        public void UpdateTextMap()
        {
            if (!_loaded_map || !Model.IsInitialized || !glc_map.Enabled) return;
            if (Model.Wells == null) return;

            #region Scale
            double dx = (Model.xmax - Model.xmin);
            double dy = (Model.ymax - Model.ymin);
            double scale;

            if (glc_map.Width / dx > glc_map.Height / dy)
            {
                //scale = (double)(1) / (glc_map.Width) * dy * ratio * Model.ZoomMap;
                scale = (double)(1) / (glc_map.Height) * dy * Model.ZoomMap;
            }
            else
            {
                scale = (double)(1) / (glc_map.Width) * dx * Model.ZoomMap;
                //scale = (double)(1) / (glc_map.Height) * dx / ratio * Model.ZoomMap;
            }
            #endregion
            GL.PushMatrix();
            GL.Color4(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            
            foreach (CModel.Well well in Model.Wells)
            {
                if (well.texHandle == 0) well.texHandle = GenTexture(well.Name);
                double xw, yw;
                xw = Model.Cell.CenterX(well.WellHead[0], well.WellHead[1], 0) + 0 * scale;
                yw = Model.Cell.CenterY(well.WellHead[0], well.WellHead[1], 0) + 0 * scale;

                GL.BindTexture(TextureTarget.Texture2D, well.texHandle);
                
                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0.0, 0.0); GL.Vertex2(xw, yw);
                GL.TexCoord2(1.0, 0.0); GL.Vertex2(xw + well.Name.Length * (int)font.Size * scale+2, yw);
                GL.TexCoord2(1.0, 1.0); GL.Vertex2(xw + well.Name.Length * (int)font.Size * scale+2, yw + font.Height * scale+2);
                GL.TexCoord2(0.0, 1.0); GL.Vertex2(xw, yw + font.Height * scale+2);
                GL.End();
            }
            //GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();
            //GL.BlendEquation(BlendEquationMode.FuncAdd);
        }

        /// <summary>
        /// Обновляет DrawList для IntersectionI-окна
        /// Исполнять при изменении свойства/разреза
        /// </summary>
        public void UpdateDL_InterI()
        {
            if (!_loaded_intI || !Model.IsInitialized || !glc_intersectionI.Enabled) return;
            GL.NewList(hDLInteI, ListMode.Compile);
            #region cells
            GL.Begin(PrimitiveType.Triangles);
            for (int x = Model.SelectedI - Model.IJRange; x <= Model.SelectedI + Model.IJRange; x++)
                if (x >= 0 && x < Model.NI)
                    for (int z = 0; z < Model.NK; z++)
                    {
                        if ((Model.actnum[x, Model.SelectedJ, z]) != 0)
                        {
                            //double cX = (Model.Cell.CenterX(x, Model.SelectedJ, z) + Model.Cell.CenterX(x + 1, Model.SelectedJ, z) + Model.Cell.CenterX(x, Model.SelectedJ, z + 1) + Model.Cell.CenterX(x + 1, Model.SelectedJ, z + 1)) * 0.25d;
                            //double cZ = (Model.Cell.CenterZ(x, Model.SelectedJ, z) + Model.Cell.CenterZ(x + 1, Model.SelectedJ, z) + Model.Cell.CenterZ(x, Model.SelectedJ, z + 1) + Model.Cell.CenterZ(x + 1, Model.SelectedJ, z + 1)) * 0.25d;

                            double x00, x01, x10, x11, z00, z01, z10, z11;
                            x00 = Model.CellSizeI * (x - Model.SelectedI);
                            x01 = Model.CellSizeI * (x - Model.SelectedI + 1);
                            x10 = Model.CellSizeI * (x - Model.SelectedI);
                            x11 = Model.CellSizeI * (x - Model.SelectedI + 1);
                            z00 = Model.zcorn[x * 2, Model.SelectedJ * 2, z * 2];
                            z01 = Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, z * 2];
                            z10 = Model.zcorn[x * 2, Model.SelectedJ * 2, z * 2 + 1];
                            z11 = Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, z * 2 + 1];

                            GL.Color3(Model.GetColor(x, Model.SelectedJ, z));

                            GL.Vertex2(x00, z00);
                            GL.Vertex2(x01, z01);
                            GL.Vertex2(x10, z10);
                            GL.Vertex2(x01, z01);
                            GL.Vertex2(x10, z10);
                            GL.Vertex2(x11, z11);
                        }
                    }
            GL.End();
            #endregion
            #region borders
            if (Model.IsBordered)
            {
                if (!Model.BlackBack) GL.Color3(Color.Black); else GL.Color3(Color.WhiteSmoke);
                GL.LineWidth(1f);

                for (int x = Model.SelectedI - Model.IJRange; x <= Model.SelectedI + Model.IJRange; x++)
                    if (x >= 0 && x < Model.NI)
                        for (int z = 0; z < Model.NK; z++)
                        {
                            if (Model.actnum[x, Model.SelectedJ, z] != 0)
                            {
                                double x00, x01, x10, x11, z00, z01, z10, z11;

                                x00 = Model.CellSizeI * (x - Model.SelectedI);
                                x01 = Model.CellSizeI * (x - Model.SelectedI + 1);
                                x10 = Model.CellSizeI * (x - Model.SelectedI);
                                x11 = Model.CellSizeI * (x - Model.SelectedI + 1);
                                z00 = Model.zcorn[x * 2, Model.SelectedJ * 2, z * 2];
                                z01 = Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, z * 2];
                                z10 = Model.zcorn[x * 2, Model.SelectedJ * 2, z * 2 + 1];
                                z11 = Model.zcorn[x * 2 + 1, Model.SelectedJ * 2, z * 2 + 1];

                                GL.Begin(PrimitiveType.LineStrip);
                                GL.Vertex2(x00, z00);
                                GL.Vertex2(x01, z01);
                                GL.Vertex2(x11, z11);
                                GL.Vertex2(x10, z10);
                                GL.Vertex2(x00, z00);
                                GL.End();
                            }
                        }
            }
            #endregion
            #region wells
            if (Model.Wells != null)
            {

                foreach (CModel.Well well in Model.Wells)
                    if (well.Connections != null)
                    {
                        #region Line
                        if (Model.BlackBack) GL.Color3(Color.White); else GL.Color3(Color.Black);
                        GL.LineWidth(3.0f);
                        GL.Begin(PrimitiveType.LineStrip);

                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.J == Model.SelectedJ)
                            {
                                double xw, yw;
                                double x00, x01, x10, x11, z00, z01, z10, z11;
                                x00 = Model.CellSizeI * (con.I - Model.SelectedI);
                                x01 = Model.CellSizeI * (con.I - Model.SelectedI + 1);
                                x10 = Model.CellSizeI * (con.I - Model.SelectedI);
                                x11 = Model.CellSizeI * (con.I - Model.SelectedI + 1);
                                z00 = Model.zcorn[con.I * 2, con.J * 2, con.K1 * 2];
                                z01 = Model.zcorn[con.I * 2 + 1, con.J * 2, con.K1 * 2];
                                z10 = Model.zcorn[con.I * 2, con.J * 2, con.K1 * 2 + 1];
                                z11 = Model.zcorn[con.I * 2 + 1, con.J * 2, con.K1 * 2 + 1];

                                xw = (x00 + x01 + x10 + x11) * 0.25;
                                yw = (z00 + z01 + z10 + z11) * 0.25;

                                GL.Vertex2(xw, yw);
                            }
                        GL.End();
                        #endregion
                        #region black dot
                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.J == Model.SelectedJ)
                            {
                                double xw, yw;
                                double x00, x01, x10, x11, z00, z01, z10, z11;
                                x00 = Model.CellSizeI * (con.I - Model.SelectedI);
                                x01 = Model.CellSizeI * (con.I - Model.SelectedI + 1);
                                x10 = Model.CellSizeI * (con.I - Model.SelectedI);
                                x11 = Model.CellSizeI * (con.I - Model.SelectedI + 1);
                                z00 = Model.zcorn[con.I * 2, con.J * 2, con.K1 * 2];
                                z01 = Model.zcorn[con.I * 2 + 1, con.J * 2, con.K1 * 2];
                                z10 = Model.zcorn[con.I * 2, con.J * 2, con.K1 * 2 + 1];
                                z11 = Model.zcorn[con.I * 2 + 1, con.J * 2, con.K1 * 2 + 1];

                                xw = (x00 + x01 + x10 + x11) * 0.25;
                                yw = (z00 + z01 + z10 + z11) * 0.25;

                                GL.Color3(Color.Black);
                                GL.PointSize(6.0f);
                                GL.Begin(PrimitiveType.Points);
                                GL.Vertex2(xw, yw);
                                GL.End();
                            }
                        #endregion
                        #region white dot
                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.J == Model.SelectedJ)
                            {
                                double xw, yw;
                                double x00, x01, x10, x11, z00, z01, z10, z11;
                                x00 = Model.CellSizeI * (con.I - Model.SelectedI);
                                x01 = Model.CellSizeI * (con.I - Model.SelectedI + 1);
                                x10 = Model.CellSizeI * (con.I - Model.SelectedI);
                                x11 = Model.CellSizeI * (con.I - Model.SelectedI + 1);
                                z00 = Model.zcorn[con.I * 2, con.J * 2, con.K1 * 2];
                                z01 = Model.zcorn[con.I * 2 + 1, con.J * 2, con.K1 * 2];
                                z10 = Model.zcorn[con.I * 2, con.J * 2, con.K1 * 2 + 1];
                                z11 = Model.zcorn[con.I * 2 + 1, con.J * 2, con.K1 * 2 + 1];

                                xw = (x00 + x01 + x10 + x11) * 0.25;
                                yw = (z00 + z01 + z10 + z11) * 0.25;

                                GL.Color3(Color.White);
                                GL.PointSize(2.0f);
                                GL.Begin(PrimitiveType.Points);
                                GL.Vertex2(xw, yw);
                                GL.End();
                            }
                        #endregion
                    }
            }
            #endregion
            GL.EndList();
        }

        /// <summary>
        /// Обновляет DrawList для IntersectionJ-окна
        /// Исполнять при изменении свойства/разреза
        /// </summary>
        public void UpdateDL_InterJ()
        {
            if (!_loaded_intI || !Model.IsInitialized || !glc_intersectionJ.Enabled) return;
            GL.NewList(hDLInteJ, ListMode.Compile);
            #region cells
            GL.Begin(PrimitiveType.Triangles);
            for (int x = Model.SelectedJ - Model.IJRange; x <= Model.SelectedJ + Model.IJRange; x++)
                if (x >= 0 && x < Model.NJ)
                    for (int z = 0; z < Model.NK; z++)
                    {
                        if (Model.actnum[Model.SelectedI, x, z] != 0)
                        {
                            double x00, x01, x10, x11, z00, z01, z10, z11;
                            x00 = Model.CellSizeJ * (x - Model.SelectedJ);
                            x01 = Model.CellSizeJ * (x - Model.SelectedJ + 1);
                            x10 = Model.CellSizeJ * (x - Model.SelectedJ);
                            x11 = Model.CellSizeJ * (x - Model.SelectedJ + 1);
                            z00 = Model.zcorn[Model.SelectedI * 2, x * 2, z * 2];
                            z01 = Model.zcorn[Model.SelectedI * 2, x * 2 + 1, z * 2];
                            z10 = Model.zcorn[Model.SelectedI * 2, x * 2, z * 2 + 1];
                            z11 = Model.zcorn[Model.SelectedI * 2, x * 2 + 1, z * 2 + 1];

                            GL.Color3(Model.GetColor(Model.SelectedI, x, z));

                            GL.Vertex2(x00, z00);
                            GL.Vertex2(x01, z01);
                            GL.Vertex2(x10, z10);
                            GL.Vertex2(x01, z01);
                            GL.Vertex2(x10, z10);
                            GL.Vertex2(x11, z11);
                        }
                    }
            GL.End();
            #endregion
            #region borders
            if (Model.IsBordered)
            {
                if (!Model.BlackBack) GL.Color3(Color.Black); else GL.Color3(Color.WhiteSmoke);
                GL.LineWidth(1f);

                for (int x = Model.SelectedJ - Model.IJRange; x <= Model.SelectedJ + Model.IJRange; x++)
                    if (x >= 0 && x < Model.NJ)
                        for (int z = 0; z < Model.NK; z++)
                        {
                            if (Model.actnum[Model.SelectedI, x, z] != 0)
                            {
                                double x00, x01, x10, x11, z00, z01, z10, z11;

                                x00 = Model.CellSizeJ * (x - Model.SelectedJ);
                                x01 = Model.CellSizeJ * (x - Model.SelectedJ + 1);
                                x10 = Model.CellSizeJ * (x - Model.SelectedJ);
                                x11 = Model.CellSizeJ * (x - Model.SelectedJ + 1);
                                z00 = Model.zcorn[Model.SelectedI * 2, x * 2, z * 2];
                                z01 = Model.zcorn[Model.SelectedI * 2, x * 2 + 1, z * 2];
                                z10 = Model.zcorn[Model.SelectedI * 2, x * 2, z * 2 + 1];
                                z11 = Model.zcorn[Model.SelectedI * 2, x * 2 + 1, z * 2 + 1];

                                GL.Begin(PrimitiveType.LineStrip);
                                GL.Vertex2(x00, z00);
                                GL.Vertex2(x01, z01);
                                GL.Vertex2(x11, z11);
                                GL.Vertex2(x10, z10);
                                GL.Vertex2(x00, z00);
                                GL.End();
                            }
                        }
            }
            #endregion
            #region wells
            if (Model.Wells != null)
            {
                foreach (CModel.Well well in Model.Wells)
                    if (well.Connections != null)
                    {                
                        #region Line
                        if (Model.BlackBack) GL.Color3(Color.White); else GL.Color3(Color.Black);
                        GL.LineWidth(3.0f);
                        GL.Begin(PrimitiveType.LineStrip);

                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.I == Model.SelectedI)
                            {
                                double xw, yw;
                                double x00, x01, x10, x11, z00, z01, z10, z11;
                                x00 = Model.CellSizeJ * (con.J - Model.SelectedJ);
                                x01 = Model.CellSizeJ * (con.J - Model.SelectedJ + 1);
                                x10 = Model.CellSizeJ * (con.J - Model.SelectedJ);
                                x11 = Model.CellSizeJ * (con.J - Model.SelectedJ + 1);
                                z00 = Model.zcorn[Model.SelectedI * 2, con.J * 2, con.K1 * 2];
                                z01 = Model.zcorn[Model.SelectedI * 2, con.J * 2 + 1, con.K1 * 2];
                                z10 = Model.zcorn[Model.SelectedI * 2, con.J * 2, con.K1 * 2 + 1];
                                z11 = Model.zcorn[Model.SelectedI * 2, con.J * 2 + 1, con.K1 * 2 + 1];

                                xw = (x00 + x01 + x10 + x11) * 0.25;
                                yw = (z00 + z01 + z10 + z11) * 0.25;

                                GL.Vertex2(xw, yw);
                            }

                        GL.End();
                #endregion
                        #region black dot
                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.I == Model.SelectedI)
                            {
                                double xw, yw;
                                double x00, x01, x10, x11, z00, z01, z10, z11;
                                x00 = Model.CellSizeJ * (con.J - Model.SelectedJ);
                                x01 = Model.CellSizeJ * (con.J - Model.SelectedJ + 1);
                                x10 = Model.CellSizeJ * (con.J - Model.SelectedJ);
                                x11 = Model.CellSizeJ * (con.J - Model.SelectedJ + 1);
                                z00 = Model.zcorn[Model.SelectedI * 2, con.J * 2, con.K1 * 2];
                                z01 = Model.zcorn[Model.SelectedI * 2, con.J * 2 + 1, con.K1 * 2];
                                z10 = Model.zcorn[Model.SelectedI * 2, con.J * 2, con.K1 * 2 + 1];
                                z11 = Model.zcorn[Model.SelectedI * 2, con.J * 2 + 1, con.K1 * 2 + 1];

                                xw = (x00 + x01 + x10 + x11) * 0.25;
                                yw = (z00 + z01 + z10 + z11) * 0.25;

                                GL.Color3(Color.Black);
                                GL.PointSize(6.0f);
                                GL.Begin(PrimitiveType.Points);
                                GL.Vertex2(xw, yw);
                                GL.End();
                            }
                        #endregion
                        #region white dot
                        foreach (CModel.Well.Connection con in well.Connections)
                            if (con.I == Model.SelectedI)
                            {
                                double xw, yw;
                                double x00, x01, x10, x11, z00, z01, z10, z11;
                                x00 = Model.CellSizeJ * (con.J - Model.SelectedJ);
                                x01 = Model.CellSizeJ * (con.J - Model.SelectedJ + 1);
                                x10 = Model.CellSizeJ * (con.J - Model.SelectedJ);
                                x11 = Model.CellSizeJ * (con.J - Model.SelectedJ + 1);
                                z00 = Model.zcorn[Model.SelectedI * 2, con.J * 2, con.K1 * 2];
                                z01 = Model.zcorn[Model.SelectedI * 2, con.J * 2 + 1, con.K1 * 2];
                                z10 = Model.zcorn[Model.SelectedI * 2, con.J * 2, con.K1 * 2 + 1];
                                z11 = Model.zcorn[Model.SelectedI * 2, con.J * 2 + 1, con.K1 * 2 + 1];

                                xw = (x00 + x01 + x10 + x11) * 0.25;
                                yw = (z00 + z01 + z10 + z11) * 0.25;

                                GL.Color3(Color.White);
                                GL.PointSize(2.0f);
                                GL.Begin(PrimitiveType.Points);
                                GL.Vertex2(xw, yw);
                                GL.End();
                            }
                        #endregion
                    }
            }
            #endregion
            GL.EndList();
        }

        /// <summary>
        /// Создание текстур с имененем скважин для последующей отрисовки
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public int GenTexture(string Name)
        {
            Bitmap BM;
            int _texHandle;
            GL.ActiveTexture(TextureUnit.Texture0);
            //GL.Disable(EnableCap.DepthTest);
            BM = new Bitmap(Name.Length * (int)font.Size + 2, font.Height + 2);
            using (Graphics gfx = Graphics.FromImage(BM))
            {
                gfx.Clear(Color.Transparent);
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                gfx.DrawString(Name, font, new SolidBrush(Color.Black), new PointF(1, 1));
            }
            GL.Enable(EnableCap.Texture2D);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Replace);

            GL.GenTextures(1, out _texHandle);

            System.Drawing.Imaging.BitmapData data = BM.LockBits(
                new Rectangle(0, 0, BM.Width, BM.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, _texHandle);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba,
                BM.Width, BM.Height,
                0, PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            /*GL.TexSubImage2D(TextureTarget.Texture2D, 0, 
                0, 0, BM.Width, BM.Height, 
                PixelFormat.Bgra, 
                PixelType.UnsignedByte, 
                data.Scan0);*/

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.Finish();
            BM.UnlockBits(data);
            BM.Dispose();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.Disable(EnableCap.Texture2D);
            return _texHandle;
        }
        #endregion

        #region TreeView
        private void TV_boxes_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (BW_ProcessNodes.IsBusy)
                BW_ProcessNodes.CancelAsync();
            else
                BW_ProcessNodes.RunWorkerAsync();
        }

        private void BW_ProcessNodesComplited(object sender, RunWorkerCompletedEventArgs e)
        {
            Model.actnum.CheckNTGPORO();
            if (Model.Edited)
                Model.RecalculateValues();
        }

        private void BW_ProcessNodesEntryPoint(object sender, DoWorkEventArgs e)
        {
            foreach (CModel.DProperty prop in Model.Props)
                for (int i = 0; i < Model.NI; i++)
                    for (int j = 0; j < Model.NJ; j++)
                        for (int k = 0; k < Model.NK; k++)
                        {
                            prop.Mult[i, j, k] = 1.0f;
                            prop.Add[i, j, k] = 0.0f;
                        }

            Model.actnum.Reset();

            foreach (TreeNode tn in TV_boxes.Nodes)
            {
                if (e.Cancel) return;
                if (tn.Checked == true) ProcessNodes(tn, e);
            }
        }

        private void ProcessNodes(TreeNode tn, DoWorkEventArgs e)
        {
            if (tn.Nodes.Count == 0)
            {
                if (Model.Left(tn.Tag.ToString(), 2) == "BE")
                {
                    Model.Bulleye.Read(tn.Tag.ToString());
                    Model.Bulleye.Process();
                }
                if (Model.Left(tn.Tag.ToString(), 2) == "RD")
                {
                    Model.Reduce.Read(tn.Tag.ToString());
                    Model.Reduce.Process();
                }
                if (Model.Left(tn.Tag.ToString(), 2) == "RS")
                {
                    Model.Restore.Read(tn.Tag.ToString());
                    Model.Restore.Process();
                }
            }
            else
                foreach (TreeNode node in tn.Nodes)
                    if (node.Checked == true)
                    {
                        if (e.Cancel) return;
                        ProcessNodes(node, e);
                    }
        }

        private void ReadNode(TreeNode tn)
        {
            if (tn.Nodes.Count == 0)
            {
                if (Model.Left(tn.Tag.ToString(), 2) == "BE")
                {
                    Model.Bulleye.Read(tn.Tag.ToString());
                    Model.Bulleye.UpdateTable(dataGridProps);
                    if (Model.Bulleye.GetName() == tn.Text)
                        editingNode.renameable = true;
                    editingNode.type = 1;
                }
                if (Model.Left(tn.Tag.ToString(), 2) == "RD")
                {
                    Model.Reduce.Read(tn.Tag.ToString());
                    Model.Reduce.UpdateTable(dataGridProps);
                    if (Model.Reduce.GetName() == tn.Text)
                        editingNode.renameable = true;
                    editingNode.type = 2;
                }
                if (Model.Left(tn.Tag.ToString(), 2) == "RS")
                {
                    Model.Restore.Read(tn.Tag.ToString());
                    Model.Restore.UpdateTable(dataGridProps);
                    if (Model.Restore.GetName() == tn.Text)
                        editingNode.renameable = true;
                    editingNode.type = 3;
                }
            }
        }

        private void groupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TV_boxes.GroupNodes();

            if (BW_ProcessNodes.IsBusy)
                while (BW_ProcessNodes.IsBusy)
                {
                    BW_ProcessNodes.CancelAsync();
                    System.Threading.Thread.Sleep(50);
                }
            BW_ProcessNodes.RunWorkerAsync();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TV_boxes.DelNodes();
            
            if (BW_ProcessNodes.IsBusy)
                while (BW_ProcessNodes.IsBusy)
                {
                    BW_ProcessNodes.CancelAsync();
                    System.Threading.Thread.Sleep(50);
                }
            BW_ProcessNodes.RunWorkerAsync();
        }

        private void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TV_boxes.UngroupNodes();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Windows.Rename_Form Form = new Windows.Rename_Form();
            Form.Owner = this;
            Form.textBox1.Text = (TV_boxes.SelectedNodes[0] as TreeNode).Text;
            if (Form.ShowDialog(this) == DialogResult.OK)
                foreach (TreeNode tn in TV_boxes.SelectedNodes)
                    tn.Text = Form.textBox1.Text;
            Form.Dispose();
        }

        private void TV_boxes_MouseDown(object sender, MouseEventArgs e)
        {
            if (TV_boxes.SelectedNodes != null && TV_boxes.SelectedNodes.Count == 0)
            {
                Point pt = TV_boxes.PointToClient(new Point(e.X, e.Y));
                TreeViewHitTestInfo hitInfo = TV_boxes.HitTest(pt);
                TV_boxes.SelectedNodes.Add(hitInfo.Node);
            }
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO Dialog with props to dup
            Color fore = TV_boxes.ForeColor;
            Color back = TV_boxes.BackColor;
            foreach (TreeNode tn in TV_boxes.SelectedNodes)
            {
                TreeNodeCollection appendTo = ((tn as TreeNode).Parent == null) ? TV_boxes.Nodes : (tn as TreeNode).Parent.Nodes;
                TreeNode appendNode = (TreeNode)tn.Clone();
                appendNode.Checked = false;
                appendNode.BackColor = back;
                appendNode.ForeColor = fore;
                appendTo.Insert(tn.Index + TV_boxes.SelectedNodes.Count, appendNode);
            }
        }
        #endregion

        private void setupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Windows.Setups FormSetup = new Windows.Setups(Model);
            FormSetup.ShowDialog();
            
            if (FormSetup.DialogResult == DialogResult.OK)
            {
                //TODO renew the flags and save the setup-file!
            }

            FormSetup.Dispose();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //Windows.TestingRange TR = new Windows.TestingRange(Model.Wells[0].BMName);
            //TR.ShowDialog();
            //TR.Dispose();
        }

        private void contextMenuStrip1_VisibleChanged(object sender, EventArgs e)
        {
            if (TV_boxes.SelectedNodes == null)
            {
                duplicateToolStripMenuItem.Enabled = false;
                renameToolStripMenuItem.Enabled = false;
                groupToolStripMenuItem.Enabled = false;
                ungroupToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                return;
            }

            duplicateToolStripMenuItem.Enabled = true;
            renameToolStripMenuItem.Enabled = true;
            groupToolStripMenuItem.Enabled = true;
            ungroupToolStripMenuItem.Enabled = true;
            deleteToolStripMenuItem.Enabled = true;
            return;

        }

        #region Save-Load
        private void MenuFileSave_Click(object sender, EventArgs e)
        {
            Windows.Export exportForm = new Windows.Export(this, Model);
            exportForm.ShowDialog();
            if (exportForm.DialogResult == DialogResult.OK)
            {
                bool prevEd = Model.Edited;
                Model.Edited = true;
                for (int prop = 0; prop < Model.Props.Count; prop++)
                {
                    FileStream fs = new FileStream(exportForm.dataGridView1.Rows[prop].Cells[1].Value.ToString(), FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(Model.Props[prop].Name);
                    StringBuilder sb = new StringBuilder();
                    byte counter = 0;
                    for (int k = 0; k < Model.NK; k++)
                        for (int j=0;j<Model.NJ;j++)
                            for (int i = 0; i < Model.NI; i++)        
                            {
                                sb.Append((Model.Props[prop].Value[i, j, k] * Model.Props[prop].Mult[i, j, k] + Model.Props[prop].Add[i, j, k]).ToString() + " ");
                                counter++;
                                if (counter == 4)
                                {
                                    sw.WriteLine(sb.ToString());
                                    sb.Clear();
                                    counter = 0;
                                }
                            }
                    sw.WriteLine("/");
                    sw.Flush();
                    sw.Dispose();
                    fs.Dispose();
                }
                Model.Edited = prevEd;
            }
            exportForm.Dispose();
        }

        public void ReadEssFile()
        {
            EssFile.Position = 0;
            TV_boxes.BeginUpdate();
            ReadEssFileBody(TV_boxes.Nodes, ref EssFileRead);
            TV_boxes.EndUpdate();
        }

        private void ReadEssFileBody(TreeNodeCollection nodeCollection, ref StreamReader reader)
        {
            string curStr;
            string[] split;
            string Name;
            string Tag;
            bool Checked;
            curStr = reader.ReadLine();

            while (curStr != null) 
            {
                split = curStr.Split('\t');
                if (split[0] == "True") Checked = true; else Checked = false;
                Name = split[1];
                Tag = split[2];

                TreeNode tn = new TreeNode();
                tn.Text = Name;
                tn.Name = Name;
                tn.Tag = Tag;
                tn.Checked = Checked;

                if (Tag == "endgroup")
                    return;

                if (Tag == "Group")
                {                    
                    ReadEssFileBody(tn.Nodes,ref reader);
                }

                nodeCollection.Add(tn);

                curStr = reader.ReadLine();
            }
            
        }

        public void WriteEssFile()
        {
            EssFile.Dispose();
            EssFile = new FileStream(openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.Length - 5) + ".ess", FileMode.Create, FileAccess.ReadWrite);
            EssFileWrite = new StreamWriter(EssFile);
            
            WriteEssFileBody(TV_boxes.Nodes, ref EssFileWrite);
            
            EssFileWrite.Flush();
            EssFileWrite.Dispose();
        }

        private void WriteEssFileBody(TreeNodeCollection nodeCollection, ref StreamWriter writer)
        {
            foreach (TreeNode node in nodeCollection)
            {
                if (node.Nodes.Count == 0)
                    writer.WriteLine(node.Checked + "\t" + node.Text + "\t" + node.Tag);
                else
                {
                    writer.WriteLine(node.Checked + "\t" + node.Text + "\t" + node.Tag);
                    WriteEssFileBody(node.Nodes,ref writer);
                    writer.WriteLine(node.Checked + "\t" + node.Text + "\t" + "endgroup");
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteEssFile();
        }
        #endregion

        private void TV_boxes_DoubleClick(object sender, EventArgs e)
        {
            if (!Model.IsInitialized) return;
            // TODO Read node, update table and enter the modification mode
            // Works only with Picker-Instrument
            if (mode == 0)
            {
                if (editingNode.node != null)
                {
                    editingNode.node.BackColor = TV_boxes.BackColor;
                    editingNode.Clear();
                    TV_boxes.Invalidate();
                }
                editingNode.node = TV_boxes.SelectedNode;
                editingNode.node.BackColor = Color.Red;
                TV_boxes.Invalidate();
                ReadNode(TV_boxes.SelectedNode);
                Button_OK.Visible = true;
                Button_Cancel.Visible = true;
            }
        }

        private void InputChecker(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                case Keys.NumPad0:
                case Keys.NumPad1:
                case Keys.NumPad2:
                case Keys.NumPad3:
                case Keys.NumPad4:
                case Keys.NumPad5:
                case Keys.NumPad6:
                case Keys.NumPad7:
                case Keys.NumPad8:
                case Keys.NumPad9:
                case Keys.OemPeriod:
                case Keys.Oemcomma:
                case Keys.Decimal:
                case Keys.Left:
                case Keys.Right:
                case Keys.Back:
                case Keys.Delete:
                case Keys.OemMinus:
                case Keys.Subtract:
                case Keys.Oemplus:
                    e.SuppressKeyPress = false;
                    break;
                default:
                    e.SuppressKeyPress = true;
                    break;
            }
        }
    }
}