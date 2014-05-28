using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
//using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace Essence_graphics
{
    /// <summary>
    /// Обслуживающий класс, содержащий методы, поля и свойства для работы программы
    /// </summary>
    public partial class CModel
    {
        #region Конструктор
        public CModel(object sender)
        {
            MF = (Main_Form)sender;

            checkSetupFile();

            //BackgroundWorkers Setups
            BW_Reader.WorkerReportsProgress = true;
            BW_Reader.DoWork += BWReadDataFile;
            BW_Reader.RunWorkerCompleted += BWReadDataFileCompleted;
            BW_Reader.ProgressChanged += MF.ReportProgress;

            BW_RecalculateValues.WorkerSupportsCancellation = true;
            BW_RecalculateValues.WorkerReportsProgress = true;
            BW_RecalculateValues.DoWork += RecalculateValues;
            BW_RecalculateValues.ProgressChanged += MF.ReportProgress;
            BW_RecalculateValues.RunWorkerCompleted += BW_RecalculateValuesCompleted;
        }
        #endregion

        #region Переменные и классы
        private Main_Form MF;

        public CCell Cell;
        public CBulleye Bulleye;
        public CReduce Reduce;
        public CRestore Restore;
        public CPicker Picker;
        /// <summary>
        /// Полная реплика ZCORN
        /// </summary>
        public CZcorn zcorn;
        public CCoord coord;
        public CActnum actnum;

        public bool LoadZCorn = true;
        /// <summary>
        /// размер модели по I
        /// </summary>
        public int NI { get; private set; }
        /// <summary>
        /// размер модели по J
        /// </summary>
        public int NJ { get; private set; }
        /// <summary>
        /// размер модели по K
        /// </summary>
        public int NK { get; private set; }

        #region Flags
        /// <summary>
        /// Флаг инициализации. Включается при задании размерности модели
        /// </summary>
        public bool IsInitialized { get; private set; }
        /// <summary>
        /// Флаг отрисовки разреза вдоль I
        /// </summary>
        public bool PaintI { get; set; }
        /// <summary>
        /// Флаг инверсии отрисовки по X
        /// </summary>
        public bool InvertX { get; set; }
        private bool _InvertY = true;
        /// <summary>
        /// Флаг инверсии отрисовки по Y
        /// </summary>
        public bool InvertY {
            get { return _InvertY; }
            set { _InvertY = value; }
        }
        /// <summary>
        /// Возвращает флаг того, что отображаемое в тек. момент свойство - редактированное
        /// </summary>
        private bool _Edited = false;
        public bool Edited
        {
            get { return _Edited; }
            set
            {
                //if (value == _Edited) return;
                _Edited = value;
                //RecalculateValues();
            }
        }
        /// <summary>
        /// Флаг отрисовки границ ячеек.
        /// </summary>
        public bool IsBordered { get; set; }
        /// <summary>
        /// Флаг установки цвета фона. False - светлый, True - темный.
        /// </summary>
        public bool BlackBack { get; set; }
        #endregion

        /// <summary>
        /// Значение индекса I выбранной ячейки для отрисовки разреза
        /// </summary>
        public int SelectedI { get; set; }
        /// <summary>
        /// Значение индекса J выбранной ячейки для отрисовки разреза
        /// </summary>
        public int SelectedJ { get; set; }
        /// <summary>
        /// Возвращает средние размеры ячеек в направлении I. Гео-координаты
        /// </summary>
        public double CellSizeI;
        /// <summary>
        /// Возвращает средние размеры ячеек в направлении J. Гео-координаты
        /// </summary>
        public double CellSizeJ;

        /// <summary>
        /// Коллекция скважин
        /// </summary>
        public List<Well> Wells;

        #region min-max grid sizes
        public double xmin { get; private set; }
        public double ymin { get; private set; }
        public double zmin { get; private set; }
        public double xmax { get; private set; }
        public double ymax { get; private set; }
        public double zmax { get; private set; }
        #endregion

        /// <summary>
        /// Набор свойств с параметрами. Доступ по индексу массива.
        /// </summary>
        public List<DProperty> Props;
        /// <summary>
        /// Массив с нормированными значениями текущего свойства и текущего типа карты. Значения [0..1].
        /// </summary>
        public Single[,] MapColor;
        /// <summary>
        /// Массив с нормированными значениями текущего свойства для пересечения. Значения [0..1].
        /// </summary>
        public Single[,,] InterColor;

        public int[] _KRange = new int[2];
        /// <summary>
        /// K-Range
        /// </summary>
        public int[] KRange
        {
            get
            {
                return _KRange;
            }
            set 
            {
                if (value[1] < value[0]) { int t = value[0]; value[0] = value[1]; value[1] = t; }
                if (value[0] < 0) value[0] = 0;
                if (value[1] > NK - 1) value[1] = NK - 1;
                _KRange = value;

                if (IsInitialized) RecalculateValues();
            }
        }

        private int _CurrentProperty = 0;
        /// <summary>
        /// Поле текущего выбранного свойства.
        /// При изменении обновляет массивы цветовых параметров карты и разреза.
        /// </summary>
        public int CurrentProperty
        {
            get { return _CurrentProperty; }
            set
            {
                if (_CurrentProperty == value) return;
                _CurrentProperty = value;
                //RecalculateValues();
            }
        }
        
        private int _CurrentMapType = 0;
        /// <summary>
        /// Поле текущего выбранного типа карты
        /// При изменении обновляет массив цветовых параметров карты.
        /// </summary>
        public int CurrentMapType
        {
            get { return _CurrentMapType; }
            set 
            {
                if (_CurrentMapType == value) return;
                _CurrentMapType = value;
                for (int x = 0; x < NI; x++)
                    for (int y = 0; y < NJ; y++)
                        MapColor[x, y] = Convert.ToSingle((Props[CurrentProperty].Maps[CurrentMapType].Value[x, y] - Props[CurrentProperty].Maps[CurrentMapType].Min) / (Props[CurrentProperty].Maps[CurrentMapType].Max - Props[CurrentProperty].Maps[CurrentMapType].Min));
            }
        }

        private double _ZoomMap = 1.05;
        /// <summary>
        /// Значение текущего зума карты.
        /// </summary>
        public double ZoomMap
        {
            get { return _ZoomMap; }
            set { if (value >= 0.01 && value <= 2) _ZoomMap = value; }
        }
        
        private double _ZoomIntersection = 1.05;
        /// <summary>
        /// Значение текущего зума разреза.
        /// </summary>
        public double ZoomIntersection
        {
            get { return _ZoomIntersection; }
            set { if (value >= 0.05 && value <= 2) _ZoomIntersection = value; }
        }

        private int _IJRange = 10;
        /// <summary>
        /// Радиус в ячейках для отрисовки разрезов
        /// </summary>
        public int IJRange
        {
            get { return _IJRange; }
            set { if (value >= 5) _IJRange = value; }
        }

        /// <summary>
        /// Значение центра отрисовки.
        /// </summary>
        public tXYZ ViewCenter;

        /// <summary>
        /// Значение текущего смещения отрисовки относительно центра.
        /// </summary>
        public tXYZ ViewOffset;

        #region dataFileStructure
        /// <summary>
        /// Набор названий гридов для поиска.
        /// </summary>
        public List<string> gridsToLook = new List<string>();

        /// <summary>
        /// Лист найденых кубов
        /// </summary>
        public List<grid> GridsDetected = new List<grid>();
        #endregion
        #endregion

        #region GetColor
        /// <summary>
        /// Преобразует входящий параметр со значением [0..1] в RGB цвет. Минимум - фиолетовый. Максимум - красный.
        /// Палитра HSV
        /// </summary>
        /// <param name="H"></param>
        /// <returns></returns>
        private Vector3 HSVtoRGB(Single H)
        {
            H = 300.0f - H * 300.0f;
            Single s = 1.0f;
            Single v = 1.0f;
            Single c = s * v;
            Single hd = H / 60.0f;
            Single x = (float)(c * (1.0f - Math.Abs(hd - Math.Truncate(hd / 2.0f) * 2.0f - 1.0f)));
            Single m = c - v;

            Single r = 0.0f, g = 0.0f, b = 0.0f;

            if (hd < 1.0f && hd >= 0.0f)
            {
                r = c + m;
                g = x + m;
                b = 0 + m;
            }
            else
                if (hd < 2.0f)
                {
                    r = x + m;
                    g = c + m;
                    b = 0 + m;
                }
                else
                    if (hd < 3.0f)
                    {
                        r = 0 + m;
                        g = c + m;
                        b = x + m;
                    }
                    else
                        if (hd < 4.0f)
                        {
                            r = 0 + m;
                            g = x + m;
                            b = c + m;
                        }
                        else
                            if (hd < 5.0f)
                            {
                                r = x + m;
                                g = 0 + m;
                                b = c + m;
                            }
                            else
                                if (hd < 6.0f)
                                {
                                    r = c + m;
                                    g = 0 + m;
                                    b = x + m;
                                }
             return new Vector3(r, g, b);
        }

        /// <summary>
        /// Получает цвет для отрисовки карты по индексам ячейки
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 GetColor(int x, int y)
        {
            return HSVtoRGB(MapColor[x,y]);
        }

        /// <summary>
        /// Получает цвет для отрисовки разреза по индексам ячейки
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Vector3 GetColor(int x, int y, int z)
        {
            return HSVtoRGB(InterColor[x,y,z]);
        }
        #endregion

        #region FindIJ
        /// <summary>
        /// Возвращает индекс ячейки, определенный по входящим координатам.
        /// Возможны баги :)
        /// В случае непопадания ни в один из объектов возвращает массив {-1,-1}
        /// </summary>
        /// <param name="glX"></param>
        /// <param name="glY"></param>
        /// <returns></returns>
        public int[] FindIJ_Map(double glX, double glY)
        {
            int i1 = 0, i2 = NI - 1, j1 = 0, j2 = NJ - 1;
            int pi2 = i2, pj2 = j2;
            double cX = 0.0d, cY = 0.0d;
            bool CutI;
            int counter=0;
            do
            {
                if (i2 - i1 >= j2 - j1)
                {
                    pi2 = i2;
                    i2 = i1 + (i2 - i1) / 2; CutI = true;
                }
                else
                {
                    pj2 = j2;
                    j2 = j1 + (j2 - j1) / 2; CutI = false;
                }

                cX = (coord[i1, j1, 0].X + coord[i1, j2 + 1, 0].X + coord[i2 + 1, j1, 0].X + coord[i2 + 1, j2 + 1, 0].X) / 4;
                cY = (coord[i1, j1, 0].Y + coord[i1, j2 + 1, 0].Y + coord[i2 + 1, j1, 0].Y + coord[i2 + 1, j2 + 1, 0].Y) / 4;

                for (int i = i1; i <= i2; i++)
                {
                    if (Cross(coord[i, j1, 0].X, coord[i, j1, 0].Y, coord[i + 1, j1, 0].X, coord[i + 1, j1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i, j2 + 1, 0].X, coord[i, j2 + 1, 0].Y, coord[i + 1, j2 + 1, 0].X, coord[i + 1, j2 + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                }
                for (int j = j1; j <= j2; j++)
                {
                    if (Cross(coord[i1, j, 0].X, coord[i1, j, 0].Y, coord[i1, j + 1, 0].X, coord[i1, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i2 + 1, j, 0].X, coord[i2 + 1, j, 0].Y, coord[i2 + 1, j + 1, 0].X, coord[i2 + 1, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                }

                if (counter != 1)
                    if (CutI)
                    {
                        i1 = i2 + 1;
                        i2 = pi2;
                    }
                    else
                    {
                        j1 = j2 + 1;
                        j2 = pj2;
                    }
                counter = 0;
            } while ((i2 - i1 > 1) || (j2 - j1 > 1));
            for (int i = i1; i <= i2; i++)
                for (int j = j1; j <= j2; j++)
                {
                    cX = (coord[i, j, 0].X + coord[i, j + 1, 0].X + coord[i + 1, j, 0].X + coord[i + 1, j + 1, 0].X) / 4;
                    cY = (coord[i, j, 0].Y + coord[i, j + 1, 0].Y + coord[i + 1, j, 0].Y + coord[i + 1, j + 1, 0].Y) / 4;
                    if (Cross(coord[i, j, 0].X, coord[i, j, 0].Y, coord[i + 1, j, 0].X, coord[i + 1, j, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i, j, 0].X, coord[i, j, 0].Y, coord[i, j + 1, 0].X, coord[i, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i + 1, j, 0].X, coord[i + 1, j, 0].Y, coord[i + 1, j + 1, 0].X, coord[i + 1, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i, j + 1, 0].X, coord[i, j + 1, 0].Y, coord[i + 1, j + 1, 0].X, coord[i + 1, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (counter == 1) { return new int[2] { i, j }; } //tada!
                }
            return new int[2] { -1, -1 }; // oi-wei. not in the model :(
        }

        /// <summary>
        /// Replica for FindIJ to find same shit in Intersection
        /// TODO...
        /// </summary>
        /// <param name="glX"></param>
        /// <param name="glY"></param>
        /// <returns></returns>
        public int[] FindIJ_Intersection(double glX, double glY)
        {
            int i1 = 0, i2 = PaintI?NI-1:NJ-1, j1 = 0, j2 = NK - 1;
            int pi2 = i2, pj2 = j2;
            double cX = 0.0d, cY = 0.0d;
            bool CutI;
            int counter = 0;
            do
            {
                if (i2 - i1 >= j2 - j1)
                {
                    pi2 = i2;
                    i2 = i1 + (i2 - i1) / 2; CutI = true;
                }
                else
                {
                    pj2 = j2;
                    j2 = j1 + (j2 - j1) / 2; CutI = false;
                }

                cX = (coord[PaintI ? i1 : SelectedI, PaintI ? SelectedJ : i1, j1].X + coord[PaintI ? i1 : SelectedI, PaintI ? SelectedJ : i1, j2 + 1].X + coord[PaintI ? i2 + 1 : SelectedI, PaintI ? SelectedJ : i2 + 1, j1].X + coord[PaintI ? i2+1 : SelectedI, PaintI ? SelectedJ : i2+1, j2+1].X) / 4;
                cY = (coord[PaintI ? i1 : SelectedI, PaintI ? SelectedJ : i1, j1].Z + coord[PaintI ? i1 : SelectedI, PaintI ? SelectedJ : i1, j2 + 1].Z + coord[PaintI ? i2 + 1 : SelectedI, PaintI ? SelectedJ : i2 + 1, j1].Z + coord[PaintI ? i2 + 1 : SelectedI, PaintI ? SelectedJ : i2 + 1, j2 + 1].Z) / 4;

                for (int i = i1; i <= i2; i++)
                {
                    if (Cross(coord[i, j1, 0].X, coord[i, j1, 0].Y, coord[i + 1, j1, 0].X, coord[i + 1, j1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i, j2 + 1, 0].X, coord[i, j2 + 1, 0].Y, coord[i + 1, j2 + 1, 0].X, coord[i + 1, j2 + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                }
                for (int j = j1; j <= j2; j++)
                {
                    if (Cross(coord[i1, j, 0].X, coord[i1, j, 0].Y, coord[i1, j + 1, 0].X, coord[i1, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i2 + 1, j, 0].X, coord[i2 + 1, j, 0].Y, coord[i2 + 1, j + 1, 0].X, coord[i2 + 1, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                }

                if (counter != 1)
                    if (CutI)
                    {
                        i1 = i2 + 1;
                        i2 = pi2;
                    }
                    else
                    {
                        j1 = j2 + 1;
                        j2 = pj2;
                    }
                counter = 0;
            } while ((i2 - i1 > 1) || (j2 - j1 > 1));
            for (int i = i1; i <= i2; i++)
                for (int j = j1; j <= j2; j++)
                {
                    cX = (coord[i, j, 0].X + coord[i, j + 1, 0].X + coord[i + 1, j, 0].X + coord[i + 1, j + 1, 0].X) / 4;
                    cY = (coord[i, j, 0].Y + coord[i, j + 1, 0].Y + coord[i + 1, j, 0].Y + coord[i + 1, j + 1, 0].Y) / 4;
                    if (Cross(coord[i, j, 0].X, coord[i, j, 0].Y, coord[i + 1, j, 0].X, coord[i + 1, j, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i, j, 0].X, coord[i, j, 0].Y, coord[i, j + 1, 0].X, coord[i, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i + 1, j, 0].X, coord[i + 1, j, 0].Y, coord[i + 1, j + 1, 0].X, coord[i + 1, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (Cross(coord[i, j + 1, 0].X, coord[i, j + 1, 0].Y, coord[i + 1, j + 1, 0].X, coord[i + 1, j + 1, 0].Y, cX, cY, glX, glY)) { counter++; }
                    if (counter == 1) { return new int[2] { i, j }; } //tada!
                }
            return new int[2] { -1, -1 }; // oi-wei. not in the model :(
        }

        /// <summary>
        /// Определяет пересечения отрезка [x1,y1]-[x2,y2] с лучом [x3,y3]-(x4,y4)
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <param name="x4"></param>
        /// <param name="y4"></param>
        /// <returns></returns>
        private bool Cross(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            double dx2 = x4 - x3;
            double dx1 = x2 - x1;
            double dy1 = y2 - y1;
            double dy2 = y4 - y3;
            double x = dy1 * dx2 - dx1 * dy2;
            if (x == 0) return false; // parallel lines
            double y;
            y = x3 * y4 - y3 * x4;
            x = ((x1 * y2 - y1 * x2) * dx2 - y * dx1) / x;
            if (dx2 != 0) y = (dy2 * x - y) / dx2; else y = dy1 / dx1 * (x - x1) + y1;
            double check1 = dx1 != 0 ? (x - x1) / dx1 : (y - y1) / dy1;
            double check2 = dx2 != 0 ? (x - x4) / dx2 : (y - y4) / dy2;
            return (check1 <= 1 && check1 >= 0 && check2 >= 0);
        }
        #endregion

        #region Instruments
        public class Interpolate
        {
        }

        public class CPicker
        {
            CModel Model;
            public CPicker(object sender)
            {
                Model = (CModel)sender;
            }

            public void UpdateTable(DataGridView dgv)
            {
                if (dgv.Tag != "PI")
                {
                    dgv.Tag = "PI";
                    dgv.Rows.Clear();
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[0].Cells[0].Value = "I-Size";
                    dgv.Rows[0].Cells[1].Value = Model.NI;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[1].Cells[0].Value = "J-Size";
                    dgv.Rows[1].Cells[1].Value = Model.NJ;
                    dgv.Rows.Add(new DataGridViewRow());
                    dgv.Rows[2].Cells[0].Value = "K-Size";
                    dgv.Rows[2].Cells[1].Value = Model.NK;
                }
                else
                {
                    dgv.Rows[0].Cells[1].Value = Model.NI;
                    dgv.Rows[1].Cells[1].Value = Model.NJ;
                    dgv.Rows[2].Cells[1].Value = Model.NK;

                }
            }
        }
        #endregion

        #region Setups
        public bool checkSetupFile()
        {
            FileStream fs = new FileStream(Path.GetDirectoryName(Application.ExecutablePath) + "\\Essence_graphics.opts", FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(fs);

            bool gridSect = false;
            string[] str;

            string curStr = sr.ReadLine();
            if (curStr == null)
            {
                MessageBox.Show("Error: Empty setup-file. Please check the Setup panel first.");
                return false;
            }

            while (curStr != null)
            {
                str = System.Text.RegularExpressions.Regex.Replace(curStr, @"\s+", " ").TrimStart().ToUpper().Split(' '); // replace all multi-spaces and tabs with a single-space, trim and split all
                
                if (gridSect)
                    gridsToLook.Add(curStr);

                if (str[0] == "LOADZCORN")
                    if (str[1].ToLower() == "false") LoadZCorn = false; else LoadZCorn = true;

                if (str[0] == "GRIDS:")
                    gridSect = true;

                if (gridSect && curStr == "")
                {
                    gridSect = false;
                }

                curStr = sr.ReadLine();
            }

            sr.Dispose();
            fs.Dispose();

            return true;
        }

        public bool saveSetupFile()
        {
            FileStream fs = new FileStream(Path.GetDirectoryName(Application.ExecutablePath) + "\\Essence_graphics.opts", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs);

            sw.WriteLine("LOADZCORN " + LoadZCorn.ToString());

            sw.WriteLine("GRIDS:");
            foreach (string str in gridsToLook)
                sw.WriteLine(str);
            sw.WriteLine();

            return true;
        }
        #endregion
    }
}