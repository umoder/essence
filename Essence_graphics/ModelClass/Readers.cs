using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Essence_graphics
{
    public sealed partial class CModel
    {
        /// <summary>
        /// Фоновый процесс для считывания дат-файла
        /// </summary>
        public BackgroundWorker BW_Reader = new BackgroundWorker();

        public void BWReadDataFileCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MF.Model = this;
            //BW_Reader.ReportProgress(100, "Completed");

            actnum.CheckNTGPORO();
            actnum.Reset();

            MF.UpdatePropsList();

            MF.glc_intersectionI.Enabled = true;
            MF.glc_intersectionJ.Enabled = true;
            MF.glc_map.Enabled = true;

            MF.glc_intersectionI.Visible = true;
            MF.glc_intersectionJ.Visible = true;
            MF.glc_map.Visible = true;

            //MF.glc_map
            MF.SetupViewport(ref MF.glc_map);
            MF.SetupViewport(ref MF.glc_intersectionI);
            MF.SetupViewport(ref MF.glc_intersectionJ);

            MF.UpdateDL_Cross();
            MF.UpdateDL_Map();
            MF.UpdateDL_InterI();
            MF.UpdateDL_InterJ();

            MF.glc_intersectionI.Invalidate();
            MF.glc_intersectionJ.Invalidate();
            MF.glc_map.Invalidate();

            MF.TB_Krange.Text = (KRange[0] + 1) + "-" + (KRange[1] + 1);
            
            MF.BW_ProcessNodes.RunWorkerAsync();
        }

        public void BWReadDataFile(object sender, DoWorkEventArgs e)
        {
            ReadDataFile((string)e.Argument);
        }

        /// <summary>
        /// Очищает входную строку от табов/пробелов и возвращает массив подстрок со значениями.
        /// В случае пустой строки или полностью комментной строки возвращает null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string[] Splitter(string str)
        {
            if (str == "") return null;
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim().ToLower(); // replace all multi-spaces and tabs with a single-space

            if (str.IndexOf("--") == 0) return null; // all comments

            if (str.IndexOf("--") > 0)
                str = str.Substring(0, str.IndexOf("--")).Trim(); // cut comments

            Queue<string> result = new Queue<string>();

            if (str.IndexOf(' ') == -1 && str.IndexOf('/') == -1) // single-word
            {
                result.Enqueue(str);
                return result.ToArray();
            }

            bool read = true;
            bool noBreak = false;
            string readed = string.Empty;
            
            for (int i = 0; i < str.Length; i++) // split to words
            {
                if (str[i] == '\'') // 'xxx' flag
                {
                    if (!noBreak)
                        noBreak = true;
                    else
                        noBreak = false;
                    continue;
                }

                if (str[i] == ' ' && !noBreak)
                {
                    result.Enqueue(readed);
                    readed = string.Empty;
                    continue;
                }

                if (str[i] == '/' && !noBreak)
                    if (readed != "")
                    {
                        result.Enqueue(readed);
                        result.Enqueue("/");
                        return result.ToArray();
                    }
                    else
                    {
                        result.Enqueue("/");
                        return result.ToArray();
                    }

                readed = readed + str[i];
            }
            result.Enqueue(readed);
            return result.ToArray();
            //return null;
        }
        /*private static string[] Splitter(string str)
        {
            if (str != "")
            {
                str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim().ToLower(); // replace all multi-spaces and tabs with a single-space
                int i = str.IndexOf("--");
                if (i == -1) return str.Split(' '); // if no '--' found return all
                if (i > 0) // cut the commented part
                    return str.Substring(0, i - 1).Split(' '); // return the array of substrings
                return null; // if all is a comment
            }
            return null;
        }*/

        /// <summary>
        /// Обрабатывает data-файл в поисках нужных данных. В случае ошибки возвращает false.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool ReadDataFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string filePath = fileName.Substring(0, fileName.LastIndexOf('\\') + 1);
            string curStr = sr.ReadLine();
            string[] splitted;
            bool skip = false;
            bool skipKW = false;
            byte dimcounter=0;

            do
            {
                splitted = Splitter(curStr);
                if (splitted == null) { curStr = sr.ReadLine(); continue; }// check for commented line

                if (splitted[0] == "regions" || splitted[0] == "solution" || splitted[0] == "summary") // skipping sections
                    skip = true;
                if (splitted[0] == "runspec" || splitted[0] == "grid" || splitted[0] == "props")
                    skip = false;
                if (splitted[0] == "schedule")
                {
                    //ReadSchedule
                    BW_Reader.ReportProgress(0, "Analyzing SCHEDULE section");
                    ReadSchedule(ref sr, filePath);
                    break;
                }

                if (Supports.CubeEditors.Contains(splitted[0]))
                    skipKW = true;

                if (skipKW && splitted[0] == "/")
                    skipKW = false;

                if (skip || skipKW)
                {
                    curStr = sr.ReadLine();
                    continue;
                }

                if (splitted[0] == "dimens") // set size of model
                    if (dimcounter == 0)
                    {
                        BW_Reader.ReportProgress(0, "Initializing memory");
                        curStr = sr.ReadLine();
                        splitted = Splitter(curStr);
                        while (splitted == null)
                        {
                            curStr = sr.ReadLine();
                            splitted = Splitter(curStr); // looking for value
                        }

                        SetSize(Convert.ToInt32(splitted[0]), Convert.ToInt32(splitted[1]), Convert.ToInt32(splitted[2])); // initializing size of model
                        dimcounter++;
                        skip = true;

                        //curStr = sr.ReadLine();
                        //continue;
                    }
                    else
                    {
                        MessageBox.Show("Error. Double sized model.");
                        return false;
                    }

                if (splitted[0] == "grid" && dimcounter == 0)
                {
                    MessageBox.Show("Error. No DIMENS keyword.");
                    return false;
                }

                if (splitted[0] == "include") // 
                {
                    //recognize the file and recursive looking through it
                    //ReadDataFile
                    curStr = sr.ReadLine();
                    splitted = Splitter(curStr);
                    while (splitted == null)
                    {
                        curStr = sr.ReadLine();
                        splitted = Splitter(curStr); // looking for value
                    }
                    if (splitted[0][splitted[0].Length - 1] == '/') splitted[0] = splitted[0].Substring(0, splitted[0].Length - 1);
                    if (splitted[0].Contains("'"))
                        splitted[0] = splitted[0].Substring(1, splitted[0].Length - 2);
                    splitted[0] = splitted[0].Replace('/', '\\');
                    if (splitted[0][0] == '.' && splitted[0][1] != '.')
                        splitted[0] = splitted[0].Substring(1, splitted[0].Length - 1);
                    ReadDataFile(filePath + splitted[0]);
                    //curStr = sr.ReadLine();
                    //continue;
                }

                if (splitted[0] == "coord")
                {
                    //ReadCoord
                    BW_Reader.ReportProgress(0, "Reading COORD");
                    if (!ReadCoord(ref sr))
                    {
                        MessageBox.Show("Error in COORD");
                        return false;
                    }
                    BW_Reader.ReportProgress(100, "Searching...");
                    //curStr = sr.ReadLine();
                    //continue;
                }

                if (splitted[0] == "zcorn")
                {
                    //ReadZCorn
                    BW_Reader.ReportProgress(0, "Reading ZCORN");
                    if (!ReadZCorn(ref sr))
                    {
                        MessageBox.Show("Error in ZCORN");
                        return false;
                    }
                    BW_Reader.ReportProgress(100, "Searching...");
                    //curStr = sr.ReadLine();
                    //continue;
                }

                if (splitted[0] == "actnum") // actnum reading
                {
                    //ReadActnum
                    BW_Reader.ReportProgress(0, "Reading ACTNUM");
                    if (!ReadActnum(ref sr))
                    {
                        MessageBox.Show("Error in ACTNUM");
                        return false;
                    }
                    BW_Reader.ReportProgress(100, "Searching...");
                    //curStr = sr.ReadLine();
                    //continue;
                }

                if (gridsToLook.Contains(splitted[0]))
                {
                    //ReadDProperty
                    BW_Reader.ReportProgress(0, "Reading " + splitted[0].ToUpper());
                    if (!ReadDProperty(splitted[0].ToUpper(), ref sr))
                    {
                        MessageBox.Show("Error in " + splitted[0].ToUpper());
                        //return false;
                    }
                    BW_Reader.ReportProgress(100, "Searching...");
                    //curStr = sr.ReadLine();
                    //continue;
                }

                curStr = sr.ReadLine();
            }
            while (curStr != null);

            //File.Exists(fileName);
            IsInitialized = true;
            BW_Reader.ReportProgress(100, "Searching...");

            return true;
        }

        public string Left(string str, int Num)
        {
            if (str.Length < Num) return str;
            return str.Substring(0, Num);
        }

        /// <summary>
        /// Метод считывания куба COORD
        /// </summary>
        /// <param name="fileName"></param>
        public bool ReadCoord(ref StreamReader sr)
        {
            //FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            //StreamReader sr = new StreamReader(fs);

            string curStr;
            string[] splitted;
            int x = 0, y = 0, z = 0;
            int counter = 0;
            bool done = false;
            long pos = sr.BaseStream.Position;
            curStr = sr.ReadLine();
            splitted = Splitter(curStr);
            while (splitted == null)
            {
                curStr = sr.ReadLine();
                splitted = Splitter(curStr);
            }
            do
            {
                splitted = Splitter(curStr);
                foreach (string str in splitted)
                {
                    if (str == "/") { done = true; break; }
                    if (str != "")
                    {
                        switch (counter)
                        {
                            case 0:
                                coord[x, y, z].X = Convert.ToDouble(str);
                                /*if (coord[x, y, z].X < xmin) xmin = coord[x, y, z].X;
                                if (coord[x, y, z].X > xmax) xmax = coord[x, y, z].X;*/
                                break;
                            case 1:
                                coord[x, y, z].Y = Convert.ToDouble(str);
                                /*if (coord[x, y, z].Y < ymin) ymin = coord[x, y, z].Y;
                                if (coord[x, y, z].Y > ymax) ymax = coord[x, y, z].Y;*/
                                break;
                            case 2:
                                coord[x, y, z].Z = Convert.ToDouble(str);
                                if (coord[x, y, z].Z < zmin) zmin = coord[x, y, z].Z;
                                if (coord[x, y, z].Z > zmax) zmax = coord[x, y, z].Z;
                                break;
                        }
                        counter++;
                        if (counter > 2)
                        {
                            counter = 0;
                            z++;
                            if (z > 1)
                            {
                                z = 0;
                                x++;
                                if (x > NI)
                                {
                                    x = 0;
                                    y++;
                                    BW_Reader.ReportProgress((int)(y * 100 / (NJ + 1)));
                                }
                            }
                        }
                    }
                }
                if (!done) curStr = sr.ReadLine();
            } while (!done || curStr == null);
            
            sr.BaseStream.Position = sr.BaseStream.Position - curStr.Length - 5;
            
            if (x != 0 || y != NJ + 1)
                return false;

            xmax = Math.Max(coord[0, 0, 0].X, coord[NI, NJ, 0].X);
            xmax = Math.Max(xmax, coord[NI, 0, 0].X);
            xmax = Math.Max(xmax, coord[0, NJ, 0].X);

            xmin = Math.Min(coord[0, 0, 0].X, coord[NI, NJ, 0].X);
            xmin = Math.Min(xmin, coord[NI, 0, 0].X);
            xmin = Math.Min(xmin, coord[0, NJ, 0].X);

            ymax = Math.Max(coord[0, 0, 0].Y, coord[NI, NJ, 0].Y);
            ymax = Math.Max(ymax, coord[NI, 0, 0].Y);
            ymax = Math.Max(ymax, coord[0, NJ, 0].Y);

            ymin = Math.Min(coord[0, 0, 0].Y, coord[NI, NJ, 0].Y);
            ymin = Math.Min(ymin, coord[NI, 0, 0].Y);
            ymin = Math.Min(ymin, coord[0, NJ, 0].Y);

            ViewCenter.X = (xmax + xmin) * 0.5d;
            ViewCenter.Y = (ymin + ymax) * 0.5d;
            ViewCenter.Z = (zmax + zmin) * 0.5d;

            CellSizeI = 0;
            CellSizeJ = 0;
            for (int i = 0; i < NI; i++)
                for (int j = 0; j < NJ; j++)
                {
                    CellSizeI = CellSizeI + Math.Pow(Math.Pow(coord[i + 1, j, 0].X - coord[i, j, 0].X, 2) + Math.Pow(coord[i, j, 0].Y - coord[i + 1, j, 0].Y, 2), 0.5);
                    CellSizeJ = CellSizeJ + Math.Pow(Math.Pow(coord[i, j + 1, 0].X - coord[i, j, 0].X, 2) + Math.Pow(coord[i, j, 0].Y - coord[i, j + 1, 0].Y, 2), 0.5);
                }
            CellSizeI = CellSizeI / NI / NJ;
            CellSizeJ = CellSizeJ / NJ / NI;
            return true;
        }

        /// <summary>
        /// Метод считывания куба ZCORN
        /// Различает конструкцию N*X
        /// </summary>
        /// <param name="fileName"></param>
        public bool ReadZCorn(ref StreamReader sr)
        {
            int buffLen = 4096; // 4 kib
            char[] buffer = new char[buffLen];
            int num = 0;
            bool done = false;
            double value = double.NaN;
            bool point = false;
            bool neg = false;
            int x = 0; int y = 0; int z = 0;
            int dimmer = 0;
            int mult = 1;
            bool isExp=false;
            int exp = 0;
            bool skipLine = false;
            char ch;
            int j;
            long gap = sr.BaseStream.Position % buffLen;
            num = sr.Read(buffer, 0, buffLen - Convert.ToInt32(gap));
            do
            {
                for (j = 0; j < num; j++)
                {
                    ch = buffer[j];
                    switch (ch)
                    {
                        case '-':
                            if ((j + 1 == num ? sr.Peek() : buffer[j + 1]) == '-')
                                skipLine = true; // comments
                            else
                                neg = true; // negative value flag
                            break;
                        case '\n':
                        case '\r':
                            skipLine = false;
                            break;
                    }
                    
                    if (skipLine)
                        continue;

                    switch (ch)
                    {
                        case '/':
                            done = true;
                            break;
                        case '*':
                            mult = Convert.ToInt32(value * Math.Pow(10, -dimmer));
                            value = double.NaN;
                            dimmer = 0;
                            point = false;
                            break;
                        case '.':
                            point = true;
                            break;
                        case 'E':
                        case 'e':
                            isExp = true;
                            value = value * (neg ? -1 : 1);
                            neg = false;
                            break;
                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                            if (double.IsNaN(value)) continue;

                            #region write value
                            exp = exp * (neg ? -1 : 1);
                            value = value * Math.Pow(10, exp - dimmer) * (neg && !isExp ? -1 : 1);
                            
                            for (int i = 0; i < mult; i++)
                            {
                                zcorn[x, y, z] = value;
                                x++;
                                if (x == NI * 2)
                                {
                                    x = 0;
                                    y++;
                                    if (y == NJ * 2)
                                    {
                                        y = 0;
                                        z++;
                                        if (z == NK * 2) done = true; // 
                                        BW_Reader.ReportProgress(z * 100 / NK / 2);
                                    }
                                }
                            }
                            mult = 1;
                            exp = 0;
                            isExp = false;
                            value = double.NaN;
                            point = false;
                            dimmer = 0;
                            neg = false;
                            #endregion

                            break;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            if (isExp)
                            {
                                exp = exp * 10 + ch - 48;
                                break;
                            }

                            if (double.IsNaN(value)) value = 0;
                            value = value * 10 + ch - 48;
                            if (point)
                                dimmer++;
                            break;
                    }
                    if (done) break;
                }
                if (!done) 
                    num = sr.Read(buffer, 0, buffLen);
            } while (!done);

            sr.BaseStream.Position = sr.BaseStream.Position - (num - j + 1);
            if (x != 0 || y != 0 || z != NK * 2) return false; else return true;
        }

        /// <summary>
        /// Метод считывания куба ACTNUM
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public bool ReadActnum(ref StreamReader sr)
        {
            int buffLen = 512 * 1024;
            char[] buffer = new char[buffLen];
            int num = 0;
            bool done = false;
            double value = double.NaN;
            bool point = false;
            int x = 0; int y = 0; int z = 0;
            int dimmer = 0;
            int mult = 1;
            bool skipLine = false;
            char ch;
            int j;

            do
            {
                num = sr.ReadBlock(buffer, 0, buffLen);
                for (j = 0; j < num; j++)
                {
                    ch = buffer[j];
                    switch (ch)
                    {
                        case '-':
                            if ((j+1 == num ? sr.Peek() : buffer[j + 1]) == '-')
                                skipLine = true; // comments
                            //else
                                //neg = true; // negative value flag
                            break;
                        case '\n':
                        case '\r':
                            skipLine = false;
                            break;
                    }

                    if (skipLine)
                        continue;

                    switch (ch)
                    {
                        case '/':
                            done = true;
                            break;
                        case '*':
                            mult = Convert.ToInt32(value);
                            value = 0;
                            dimmer = 0;
                            point = false;
                            break;
                        case '.':
                            point = true;
                            break;
                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                            if (double.IsNaN(value)) continue;

                            #region write value
                            value = value * Math.Pow(0.1, dimmer);
                            for (int i = 0; i < mult; i++)
                            {
                                actnum[x, y, z] = Convert.ToByte(value);
                                x++;
                                if (x == NI)
                                {
                                    x = 0;
                                    y++;
                                    if (y == NJ)
                                    {
                                        y = 0;
                                        z++;
                                        BW_Reader.ReportProgress(z * 100 / NK / 2);
                                    }
                                }
                            }
                            mult = 1;
                            value = double.NaN;
                            point = false;
                            dimmer = 0;
                            #endregion

                            break;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            if (double.IsNaN(value)) value = 0;
                            value = value * 10 + ch - 48;
                            if (point)
                                dimmer++;
                            break;
                    }
                    if (done) break;
                }
            } while (!done);

            sr.BaseStream.Position = sr.BaseStream.Position - (num - j + 1);

            if (x != 0 || y != 0 || z != NK)
            {
                MessageBox.Show("Error in ACTNUM");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Метод считывания свойства.
        /// Различает конструкцию N*X
        /// При выполнении расширяет текущий объект Props
        /// </summary>
        /// <param name="Name">Имя свойства</param>
        /// <param name="fileName"></param>
        public bool ReadDProperty(string Name, ref StreamReader sr)
        {
            DProperty prop = new DProperty();
            prop.Name = Name;
            prop.Value = new double[NI, NJ, NK];
            prop.Mult = new float[NI, NJ, NK];
            prop.Add = new float[NI, NJ, NK];
            prop.Maps = new Map[4];
            for (int N = 0; N < 4; N++)
            {
                prop.Maps[N] = new Map();
                prop.Maps[N].Value = new double[NI, NJ];
            }

            int buffLen = 512 * 1024;
            char[] buffer = new char[buffLen];
            int num = 0;
            bool done = false;
            double value = double.NaN;
            bool point = false;
            bool neg = false;
            int x = 0; int y = 0; int z = 0;
            int dimmer = 0;
            int mult = 1;
            bool skipLine = false;
            char ch;
            int j;
            int exp = 0;
            bool isExp = false;

            do
            {
                num = sr.ReadBlock(buffer, 0, buffLen);
                for (j = 0; j < num; j++)
                {
                    ch = buffer[j];
                    switch (ch)
                    {
                        case '-':
                            if ((j+1 == num ? sr.Peek() : buffer[j + 1]) == '-')
                                skipLine = true; // comments
                            else
                                neg = true; // negative value flag
                            break;
                        case '\n':
                        case '\r':
                            skipLine = false;
                            break;
                    }

                    if (skipLine)
                        continue;

                    switch (ch)
                    {
                        case '/':
                            done = true;
                            break;
                        case '*':
                            mult = Convert.ToInt32(value);
                            value = 0;
                            dimmer = 0;
                            point = false;
                            break;
                        case '.':
                            point = true;
                            break;
                        case 'E':
                        case 'e':
                            isExp = true;
                            value = value * (neg ? -1 : 1);
                            neg = false;
                            break;
                        case ' ':
                        case '\t':
                        case '\n':
                        case '\r':
                            if (double.IsNaN(value)) continue;

                            #region write value
                            value = value * Math.Pow(10, exp - dimmer) ;
                            for (int i = 0; i < mult; i++)
                            {
                                prop.Value[x, y, z] = value ;
                                x++;
                                if (x == NI)
                                {
                                    x = 0;
                                    y++;
                                    if (y == NJ)
                                    {
                                        y = 0;
                                        z++;
                                        BW_Reader.ReportProgress(z * 100 / NK / 2);
                                    }
                                }
                            }
                            mult = 1;
                            exp = 0;
                            isExp = false;
                            value = double.NaN;
                            point = false;
                            dimmer = 0;
                            neg = false;
                            #endregion

                            break;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            if (isExp)
                            {
                                exp = exp * 10 + (ch - 48) * (neg ? -1 : 1);
                                break;
                            }

                            if (double.IsNaN(value)) value = 0;
                            value = value * 10 + ch - 48;
                            if (point)
                                dimmer++;
                            break;
                    }

                    if (done) break;
                }
            } while (!done);

            //----------------------------------------------------
            if (x != 0 || y != 0 || z != NK)
            {
                MessageBox.Show("Error in " + Name);
                return false;
            }

            sr.BaseStream.Position = sr.BaseStream.Position - (num - j + 1);

            if (Props == null)
                Props = new List<DProperty>();
            Props.Capacity = Props.Capacity + 1;
            Props.Add(prop);

            RecalculateValues();

            return true;
        }

        /// <summary>
        /// Считывает WELSPECS для определения скважин и COMPDAT для задания конекшенов
        /// </summary>
        /// <param name="fileName"></param>
        public void ReadSchedule(ref StreamReader sr, string filePath)
        {
            string curStr;
            string[] splitted;
            int counter = 0;
            bool WS = false;
            bool CD = false;

            if (Wells == null)
            {
                Wells = new List<Well>();
                Wells.Capacity = 0;
            }

            curStr = sr.ReadLine();

            do
            {
                splitted = Splitter(curStr);
                if (splitted == null) { curStr = sr.ReadLine(); continue; } // skipping comments

                #region Include processing
                if (splitted[0] == "include")
                {
                    //recall self
                    curStr = sr.ReadLine();
                    splitted = Splitter(curStr);
                    while (splitted == null)
                    {
                        curStr = sr.ReadLine();
                        splitted = Splitter(curStr); // looking for value
                    }
                    if (splitted[0][splitted[0].Length - 1] == '/') splitted[0] = splitted[0].Substring(0, splitted[0].Length - 1);
                    if (splitted[0].Contains("'"))
                        splitted[0] = splitted[0].Substring(1, splitted[0].Length - 2);
                    splitted[0] = splitted[0].Replace('/', '\\');
                    StreamReader subsr = new StreamReader(filePath + splitted[0]);
                    ReadSchedule(ref subsr, filePath);
                    subsr.Dispose();
                    curStr = sr.ReadLine();
                }
                #endregion

                #region connections
                if (CD)
                {
                    if (splitted[0] == "/") { CD = false; curStr = sr.ReadLine(); continue; }
                    Well well = new Well();
                    foreach (Well well_ in Wells)
                        if (well_.Name == splitted[0])
                        {
                            well = well_;
                            int ii = Convert.ToInt32(splitted[1]);
                            int jj = Convert.ToInt32(splitted[2]);
                            int kk1 = Convert.ToInt32(splitted[3]);
                            int kk2 = Convert.ToInt32(splitted[4]);
                            foreach (Well.Connection con in well.Connections)
                                if (con.I == ii && con.J == jj && con.K1 == kk1 && con.K2 == kk2) counter++;
                            if (counter == 0)
                            {
                                //well.Connections.Capacity = well.Connections.Capacity + 1;
                                well.Connections.Add(new Well.Connection(ii - 1, jj - 1, kk1 - 1, kk2 - 1));
                            }
                            break;
                        }
                    counter = 0;

                    curStr = sr.ReadLine();
                    continue;
                }
                #endregion

                #region wells
                if (WS)
                {
                    if (splitted[0] == "/") { WS = false; curStr = sr.ReadLine(); continue; }
                    Wells.Capacity = Wells.Capacity + 1;
                    Well well = new Well();
                    well.Connections = new List<Well.Connection>();

                    well.Name = splitted[0];

                    well.WellHead[0] = Convert.ToInt32(splitted[2]) - 1;
                    well.WellHead[1] = Convert.ToInt32(splitted[3]) - 1;

                    Wells.Add(well);

                    curStr = sr.ReadLine();
                    continue;
                }
                #endregion

                if (splitted[0] == "welspecs")
                    WS = true;
                if (splitted[0] == "compdat")
                    CD = true;

                curStr = sr.ReadLine();
            } while (curStr != null);

            foreach (Well well in Wells)
                if (well.Name.Contains("'")) // Name
                    well.Name = well.Name.Substring(1, well.Name.Length - 2);
        }

        /// <summary>
        /// Выполняет инициализацию массивов.
        /// До выполенения данного метода не позволяет считывать кубы.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        public void SetSize(int i, int j, int k)
        {
            NI = i;
            NJ = j;
            NK = k;

            KRange = new int[2] { 0, k - 1 };

            SelectedI = NI / 2;
            SelectedJ = NJ / 2;

            coord = new CCoord(NI, NJ, NK);

            ViewCenter = new tXYZ();
            ViewOffset = new tXYZ();

            zcorn = new CZcorn(NI, NJ, NK);

            actnum = new CActnum(NI, NJ, NK, this);

            MapColor = new Single[NI, NJ];
            InterColor = new Single[NI, NJ, NK];

            zmin = double.MaxValue;
            zmax = double.MinValue;

            PaintI = true;

            //IsInitialized = true;

            Cell = new CCell(this);
            Bulleye = new CBulleye(this);
            Reduce = new CReduce(this);
            Restore = new CRestore(this);
            Picker = new CPicker(this);
        }
    }
}
