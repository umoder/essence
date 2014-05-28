using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essence_graphics
{
    public partial class CModel
    {
        /// <summary>
        /// Обслуживающий класс, представляющий ячейки. Обсчитывается при обращении
        /// </summary>
        public class CCell
        {
            private CModel _Model;

            public CCell(object sender)
            {
                _Model = (CModel)sender;
            }

            /// <summary>
            /// Возвращает гео-координату X центра ячейки карты
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <returns></returns>
            public double CenterX(int i, int j)
            {
                return (_Model.coord[i, j, 0].X + _Model.coord[i + 1, j, 0].X + _Model.coord[i, j + 1, 0].X + _Model.coord[i + 1, j + 1, 0].X) * 0.25d;
            }
            /// <summary>
            /// возвращает гео-координату X центра ячейки разреза
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <param name="k"></param>
            /// <returns></returns>
            public double CenterX(int i, int j, int k)
            {
                return (_Model.coord[i, j, 0].X + _Model.coord[i + 1, j, 0].X + _Model.coord[i, j + 1, 0].X + _Model.coord[i + 1, j + 1, 0].X) * 0.25d;
            }
            /// <summary>
            /// Возвращает гео-координату Y центра ячейки карты
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <returns></returns>
            public double CenterY(int i, int j)
            {
                return (_Model.coord[i, j, 0].Y + _Model.coord[i + 1, j, 0].Y + _Model.coord[i, j + 1, 0].Y + _Model.coord[i + 1, j + 1, 0].Y) * 0.25d;
            }
            /// <summary>
            /// Возвращает гео-координату Y центра ячейки разреза
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <param name="k"></param>
            /// <returns></returns>
            public double CenterY(int i, int j, int k)
            {
                return (_Model.coord[i, j, 0].Y + _Model.coord[i + 1, j, 0].Y + _Model.coord[i, j + 1, 0].Y + _Model.coord[i + 1, j + 1, 0].Y) * 0.25d;
            }
            /// <summary>
            /// Возвращает гео-координату Z ячейки разреза
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <param name="k"></param>
            /// <returns></returns>
            public double CenterZ(int i, int j, int k)
            {
                return ((_Model.zcorn[i * 2, j * 2, k * 2] + _Model.zcorn[i * 2 + 1, j * 2, k * 2] + _Model.zcorn[i * 2, j * 2 + 1, k * 2] + _Model.zcorn[i * 2 + 1, j * 2 + 1, k * 2] +
                    _Model.zcorn[i * 2, j * 2, k * 2 + 1] + _Model.zcorn[i * 2 + 1, j * 2, k * 2 + 1] + _Model.zcorn[i * 2, j * 2 + 1, k * 2 + 1] + _Model.zcorn[i * 2 + 1, j * 2 + 1, k * 2 + 1]) * 0.125d);
            }
            /// <summary>
            /// Возвращает значение текущего свойства ячейки
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <param name="k"></param>
            /// <returns></returns>
            public double this[int i, int j, int k]
            {
                get { return !_Model.Edited ? _Model.Props[_Model.CurrentProperty].Value[i, j, k] : (_Model.Props[_Model.CurrentProperty].Value[i, j, k] * _Model.Props[_Model.CurrentProperty].Mult[i, j, k] + _Model.Props[_Model.CurrentProperty].Add[i, j, k]); }
                private set { }
            }
            /// <summary>
            /// возвращает значение текущего свойства ячейки
            /// </summary>
            /// <param name="i"></param>
            /// <param name="j"></param>
            /// <param name="k"></param>
            /// <returns></returns>
            public double Value(int i, int j, int k)
            {
                return !_Model.Edited ? _Model.Props[_Model.CurrentProperty].Value[i, j, k] : (_Model.Props[_Model.CurrentProperty].Value[i, j, k] * _Model.Props[_Model.CurrentProperty].Mult[i, j, k] + _Model.Props[_Model.CurrentProperty].Add[i, j, k]);
            }
        }
    }
}
