using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Essence_graphics
{
    public partial class CModel
    {
        public class Well
        {
            /// <summary>
            /// Содержит массив коннекшенов скважины.
            /// </summary>
            public List<Connection> Connections;
            /// <summary>
            /// Возвращает имя скважины
            /// </summary>
            public string Name;
            /// <summary>
            /// Возвращает двумерный массив с координатой скважины из WELSPECS
            /// </summary>
            public int[] WellHead = new int[2];
            //public Bitmap BMName;
            public int texHandle;
            public class Connection
            {
                public int I = -1, J = -1, K1 = -1, K2 = -1;
                public Connection(int i, int j, int k1, int k2)
                {
                    I = i; J = j; K1 = k1; K2 = k2;
                }
            }
        }

        public class tXYZ
        {
            public double X = 0;
            public double Y = 0;
            public double Z = 0;
        }

        public class DProperty
        {
            /// <summary>
            /// Имя содержащегося свойства.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Массив значений свойств в ячейках
            /// </summary>
            public double[, ,] Value;
            /// <summary>
            /// Массив модификаторов-множителей
            /// </summary>
            public float[, ,] Mult;
            /// <summary>
            /// Массив модификаторов-слагаемых
            /// </summary>
            public float[, ,] Add;
            /// <summary>
            /// Набор карт.
            /// </summary>
            public Map[] Maps;
            /// <summary>
            /// Минимальное значение свойства
            /// </summary>
            public double ValueMin = double.MaxValue;
            /// <summary>
            /// Максимальное значение свойства
            /// </summary>
            public double ValueMax = double.MinValue;
        }

        public class Map
        {
            /// <summary>
            /// Двумерный массив значений карты
            /// </summary>
            public double[,] Value;
            /// <summary>
            /// Минимальное значение карты
            /// </summary>
            public double Min = double.MaxValue;
            /// <summary>
            /// Максимальное значение карты
            /// </summary>
            public double Max = double.MinValue;
        }

        /// <summary>
        /// класс для хранения расположения грида в датнике.
        /// </summary>
        public class grid
        {
            public string Name;
            public string FileLocation;
            public string FilePosition;
        }

        /// <summary>
        /// Полная реплика куба ZCORN.
        /// </summary>
        public class CZcorn
        {
            private double[] _zcorn;
            private int NI;
            private int NJ;
            private int NK;
            public double this[int i, int j, int k]
            {
                get
                {
                    return _zcorn[i + j * NI * 2 + k * NI * NJ * 4];
                }
                set
                {
                    _zcorn[i + j * NI * 2 + k * NI * NJ * 4] = value;
                }
            }
            public CZcorn(int NI, int NJ, int NK)
            {
                this.NI = NI;
                this.NJ = NJ;
                this.NK = NK;
                _zcorn = new double[NI * NJ * NK * 8];
            }
        }

        public class CCoord
        {
            private tXYZ[] _coord;
            private int NI;
            private int NJ;
            private int NK;
            public tXYZ this[int i, int j, int k]
            {
                get
                {
                    return _coord[i + j * (NI + 1) + k * (NI + 1) * (NJ + 1)];
                }
                set
                {
                    _coord[i + j * (NI + 1) + k * (NI + 1) * (NJ + 1)] = value;
                }
            }
            public CCoord(int NI, int NJ, int NK)
            {
                this.NI = NI;
                this.NJ = NJ;
                this.NK = NK;
                _coord = new tXYZ[(NI + 1) * (NJ + 1) * 2];
                for (int i=0;i<=NI;i++)
                    for (int j = 0; j <= NJ; j++)
                    {
                        _coord[i + j * (NI + 1)] = new tXYZ();
                        _coord[i + j * (NI + 1) + (NI + 1) * (NJ + 1)] = new tXYZ();
                    }
            }
        }

        public class CActnum
        {
            bool isFound = false;
            private byte[] _actnum_ini;
            private byte[] _actnum_ed;
            private CModel parent;
            private int NI;
            private int NJ;
            private int NK;
            
            public CActnum(int NI, int NJ, int NK, object sender)
            {
                this.parent = (CModel)sender;
                this.NI = NI;
                this.NJ = NJ;
                this.NK = NK;
                _actnum_ini = new byte[NI * NJ * NK];
                for (int i = 0; i < NI; i++)
                    for (int j = 0; j < NJ; j++)
                        for (int k = 0; k < NK; k++)
                            this[i, j, k] = 1;
                _actnum_ed = new byte[NI * NJ * NK];
                Reset();
            }

            public byte this[int i, int j, int k]
            {
                get 
                {
                    return parent.Edited ? _actnum_ed[i + j * NI + k * NI * NJ] : _actnum_ini[i + j * NI + k * NI * NJ];
                }
                set 
                {
                    if (!parent.Edited)
                        _actnum_ini[i + j * NI + k * NI * NJ] = value;
                    else
                        _actnum_ed[i + j * NI + k * NI * NJ] = value;
                }
            }
            
            public void Reset()
            {
                _actnum_ini.CopyTo(_actnum_ed, 0);
            }

            public void CheckNTGPORO()
            {
                int ntg=-1;
                int poro=-1;

                if (parent.Props!=null)
                    for (int i = 0; i < parent.Props.Count; i++)
                    {
                        if (parent.Props[i].Name == "NTG")
                            ntg = i;
                        if (parent.Props[i].Name == "PORO")
                            poro = i;
                    }

                if (ntg!=-1 && poro!=-1)
                    if (parent.Edited)
                    {
                        for (int i = 0; i < NI; i++)
                            for (int j = 0; j < NJ; j++)
                                for (int k = 0; k < NK; k++)
                                    if (this[i, j, k] == 1)
                                    {
                                        double val = (parent.Props[ntg].Value[i, j, k] * parent.Props[ntg].Mult[i, j, k] + parent.Props[ntg].Add[i, j, k]) * (parent.Props[poro].Value[i, j, k] * parent.Props[poro].Mult[i, j, k] + parent.Props[poro].Add[i, j, k]);
                                        if (val == 0)
                                            this[i, j, k] = 0;
                                    }
                    }
                    else
                    {
                        for (int i = 0; i < NI; i++)
                            for (int j = 0; j < NJ; j++)
                                for (int k = 0; k < NK; k++)
                                    if (this[i, j, k] == 1)
                                    {
                                        double val = (parent.Props[ntg].Value[i, j, k]) * (parent.Props[poro].Value[i, j, k]);
                                        if (val == 0)
                                            this[i, j, k] = 0;
                                    }
                    }
            }
        }
    }
}
