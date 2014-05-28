using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Essence_graphics
{
    public partial class CModel
    {
        public BackgroundWorker BW_RecalculateValues = new BackgroundWorker();
        
        #region Recalculate Values
        public void RecalculateValues()
        {
            if (BW_RecalculateValues.IsBusy)
            {
                BW_RecalculateValues.CancelAsync();
                while (BW_RecalculateValues.IsBusy)
                    System.Threading.Thread.Sleep(50);
            }
            BW_RecalculateValues.RunWorkerAsync();
        }

        public void BW_RecalculateValuesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MF.UpdateDL_Map();
            MF.UpdateDL_InterI();
            MF.UpdateDL_InterJ();
            MF.glc_map.Invalidate();
            MF.glc_intersectionI.Invalidate();
            MF.glc_intersectionJ.Invalidate();
        }

        public void RecalculateValues(object sender, DoWorkEventArgs e)
        {
            if (Props == null || Props.Count == 0) return;
            int k1 = KRange[0]; int k2 = KRange[1];
            double[,] c = new double[NI, NJ];
            double ValMin = double.MaxValue;
            double ValMax = double.MinValue;
            double M1min = double.MaxValue;
            double M1max = double.MinValue;
            double M2min = double.MaxValue;
            double M2max = double.MinValue;
            double M3min = double.MaxValue;
            double M3max = double.MinValue;
            double M4min = double.MaxValue;
            double M4max = double.MinValue;
            double temp;

            for (int i = 0; i < NI; i++)
                for (int j = 0; j < NJ; j++)
                {
                    Props[CurrentProperty].Maps[1].Value[i, j] = double.MaxValue;
                    Props[CurrentProperty].Maps[2].Value[i, j] = double.MinValue;
                    Props[CurrentProperty].Maps[3].Value[i, j] = 0;
                    for (int k = k1; k <= k2; k++)
                        if (actnum[i, j, k] != 0)
                        {
                            temp = Cell[i, j, k];
                            c[i, j]++;
                            Props[CurrentProperty].Maps[3].Value[i, j] = Props[CurrentProperty].Maps[3].Value[i, j] + temp;
                            if (temp < Props[CurrentProperty].Maps[1].Value[i, j]) Props[CurrentProperty].Maps[1].Value[i, j] = temp;
                            if (temp > Props[CurrentProperty].Maps[2].Value[i, j]) Props[CurrentProperty].Maps[2].Value[i, j] = temp;
                            if (temp < ValMin) ValMin = temp;
                            if (temp > ValMax) ValMax = temp;
                        }
                    if (c[i, j] > 0)
                    {
                        Props[CurrentProperty].Maps[0].Value[i, j] = Props[CurrentProperty].Maps[3].Value[i, j] / c[i, j];
                        if (Props[CurrentProperty].Maps[0].Value[i, j] < M1min) M1min = Props[CurrentProperty].Maps[0].Value[i, j];
                        if (Props[CurrentProperty].Maps[0].Value[i, j] > M1max) M1max = Props[CurrentProperty].Maps[0].Value[i, j];
                        if (Props[CurrentProperty].Maps[1].Value[i, j] < M2min) M2min = Props[CurrentProperty].Maps[1].Value[i, j];
                        if (Props[CurrentProperty].Maps[1].Value[i, j] > M2max) M2max = Props[CurrentProperty].Maps[1].Value[i, j];
                        if (Props[CurrentProperty].Maps[2].Value[i, j] < M3min) M3min = Props[CurrentProperty].Maps[2].Value[i, j];
                        if (Props[CurrentProperty].Maps[2].Value[i, j] > M3max) M3max = Props[CurrentProperty].Maps[2].Value[i, j];
                        if (Props[CurrentProperty].Maps[3].Value[i, j] < M4min) M4min = Props[CurrentProperty].Maps[3].Value[i, j];
                        if (Props[CurrentProperty].Maps[3].Value[i, j] > M4max) M4max = Props[CurrentProperty].Maps[3].Value[i, j];
                    }
                    else
                    {
                        Props[CurrentProperty].Maps[0].Value[i, j] = double.NaN;
                        Props[CurrentProperty].Maps[1].Value[i, j] = double.NaN;
                        Props[CurrentProperty].Maps[2].Value[i, j] = double.NaN;
                        Props[CurrentProperty].Maps[3].Value[i, j] = double.NaN;
                    }
                }

            Props[CurrentProperty].ValueMax = ValMax;
            Props[CurrentProperty].ValueMin = ValMin;
            Props[CurrentProperty].Maps[0].Min = M1min;
            Props[CurrentProperty].Maps[0].Max = M1max;
            Props[CurrentProperty].Maps[1].Min = M2min;
            Props[CurrentProperty].Maps[1].Max = M2max;
            Props[CurrentProperty].Maps[2].Min = M3min;
            Props[CurrentProperty].Maps[2].Max = M3max;
            Props[CurrentProperty].Maps[3].Min = M4min;
            Props[CurrentProperty].Maps[3].Max = M4max;

            if (Props[CurrentProperty].ValueMin == Props[CurrentProperty].ValueMax)
            {
                Props[CurrentProperty].ValueMin = Props[CurrentProperty].ValueMin - 0.1;
                Props[CurrentProperty].ValueMax = Props[CurrentProperty].ValueMax + 0.1;
            }

            for (int i = 0; i < 4; i++)
                if (Props[CurrentProperty].Maps[i].Min == Props[CurrentProperty].Maps[i].Max)
                {
                    Props[CurrentProperty].Maps[i].Min = Props[CurrentProperty].Maps[i].Min - 0.1;
                    Props[CurrentProperty].Maps[i].Max = Props[CurrentProperty].Maps[i].Max + 0.1;
                }

            double d1 = 1 / (Props[CurrentProperty].Maps[CurrentMapType].Max - Props[CurrentProperty].Maps[CurrentMapType].Min);
            double d2 = 1 / (Props[CurrentProperty].ValueMax - Props[CurrentProperty].ValueMin);
            for (int x = 0; x < NI; x++)
                for (int y = 0; y < NJ; y++)
                {
                    MapColor[x, y] = Convert.ToSingle((Props[CurrentProperty].Maps[CurrentMapType].Value[x, y] - Props[CurrentProperty].Maps[CurrentMapType].Min) * d1);
                    for (int z = 0; z < NK; z++)
                        InterColor[x, y, z] = Convert.ToSingle((Cell[x, y, z] - Props[CurrentProperty].ValueMin) * d2);
                }
        }
        #endregion

    }
}
