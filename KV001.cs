using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

namespace ASU_KV_001
{
    class Program_Par
    {
        private string out_str;

        public int SmenaNum;                //Количество смен
        public int SmenaHour;               // Начало первой смены - час
        public int SmenaMinute;             //Минуты начала первой смены 0 - 0минут, 1 - 30 минут.

        public int ComBaud;
        public int ComMode;
        public int ComDevice;
        public int ComMonitor;

        public UInt16[] num = new UInt16[10];
        public bool[] enable = new bool[10];
        public UInt16[] ver = new UInt16[10];
        public int[] selected_id = new int[10]; //выбраный идентификатор

        public byte SmenaNow(byte hour_t, byte minute_t, byte second_t)
        {
            byte i = 0, o;
            UInt32 n_tm, f_tm, pl_tm, h1, m1, s1;

            h1 = hour_t;
            m1 = minute_t;
            s1 = second_t;
            n_tm = (h1 * 60 + m1) * 60 + s1;

            h1 = (UInt32)SmenaHour;
            if (SmenaMinute == 0)
                m1 = 0;
            else m1 = 30;
            s1 = 0;
            f_tm = (h1 * 60 + m1) * 60 + s1;
            h1 = 86400; m1 = (UInt32)(SmenaNum + 1);
            pl_tm = h1 / m1;
            i = 0;
            for (o = 1; o <= (SmenaNum + 1); o++)
            {

                if (n_tm >= f_tm) i++;
                f_tm += pl_tm;
            }

            if (i == 0) i = (byte)(SmenaNum + 1);
            return (i);

        }
        public async Task SaveParFile(string filename)
        {
            string content = "";

            content += "ЧАСТОТА_ОБМЕНА="; content += ComBaud; content += "\n";
            content += "РЕЖИМ="; content += ComMode; content += "\n";
            content += "УСТРОЙСТВО="; content += ComDevice; content += "\n";
            content += "ЗАПУСК="; content += ComMonitor; content += "\n";
            content += "КОЛВО_СМЕН="; content += SmenaNum; content += "\n";
            content += "СМЕНА_ЧАС="; content += SmenaHour; content += "\n";
            content += "СМЕНА_МИНУТЫ="; content += SmenaMinute; content += "\n";

            for (ushort i = 0; i < 10; i++)
            {
                content += "КОНТОЛЕР="; content += (i + 1); content += "\n";
                content += "АКТИВНОСТЬ="; content += enable[i]; content += "\n";
                content += "СЕТЕВОЙ_НОМЕР="; content += num[i]; content += "\n";
                content += "ВЕРСИЯ="; content += ver[i]; content += "\n";
                content += "ИДЕНТИФИКАТОР="; content += selected_id[i]; content += "\n";
            }
            //   content = content.Replace(".", ",");

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            // write the char array created from the content string into the file
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
            }


        }
        public async System.Threading.Tasks.Task OpenParFile(string fileName)
        {
            string s;
            StorageFile file;
            UInt16 tn = 0;
            var _folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            file = await _folder.GetFileAsync(fileName);

            var lines = await Windows.Storage.FileIO.ReadLinesAsync(file);

            out_str = "";
            if (file != null)
            {
                foreach (var line in lines)
                {


                    if (line.Contains("ЧАСТОТА_ОБМЕНА="))
                    {
                        s = line.Replace("ЧАСТОТА_ОБМЕНА=", ""); ComBaud = Convert.ToUInt16(s);
                    }
                    if (line.Contains("РЕЖИМ="))
                    {
                        s = line.Replace("РЕЖИМ=", ""); ComMode = Convert.ToUInt16(s);
                    }
                    if (line.Contains("УСТРОЙСТВО="))
                    {
                        s = line.Replace("УСТРОЙСТВО=", ""); ComDevice = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ЗАПУСК="))
                    {
                        s = line.Replace("ЗАПУСК=", ""); ComMonitor = Convert.ToUInt16(s);
                    }
                    if (line.Contains("КОЛВО_СМЕН="))
                    {
                        s = line.Replace("КОЛВО_СМЕН=", ""); SmenaNum = Convert.ToUInt16(s);
                    }
                    if (line.Contains("СМЕНА_ЧАС="))
                    {
                        s = line.Replace("СМЕНА_ЧАС=", ""); SmenaHour = Convert.ToUInt16(s);
                    }
                    if (line.Contains("СМЕНА_МИНУТЫ="))
                    {
                        s = line.Replace("СМЕНА_МИНУТЫ=", ""); SmenaMinute = Convert.ToUInt16(s);
                    }

                    if (line.Contains("КОНТОЛЕР="))
                    {
                        s = line.Replace("КОНТОЛЕР=", "");
                        { tn = Convert.ToUInt16(s); tn--; }
                    }


                    if (line.Contains("СЕТЕВОЙ_НОМЕР="))
                    {
                        s = line.Replace("СЕТЕВОЙ_НОМЕР=", ""); num[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("АКТИВНОСТЬ="))
                    {
                        s = line.Replace("АКТИВНОСТЬ=", ""); enable[tn] = Convert.ToBoolean(s);
                    }
                    if (line.Contains("ВЕРСИЯ="))
                    {
                        s = line.Replace("ВЕРСИЯ=", ""); ver[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ИДЕНТИФИКАТОР="))
                    {
                        s = line.Replace("ИДЕНТИФИКАТОР=", ""); selected_id[tn] = Convert.ToUInt16(s);
                    }

                }

            }
            else
            {
                await SaveParFile(fileName);
            }

        }

    }
    class KV001
    {
        public float[] weight = new float[10];
        public short[] state = new short[10];
        public short[] arc_state = new short[10];
        public float[,] last_doze = new float[10, 3];
        public UInt32[] count_doze = new UInt32[10];
        public float[] sum_doze = new float[10];

        public float[,] doze = new float[10, 6];
        public float[,] dw = new float[10, 3];
        public float[,] dwi = new float[10, 3];
        public float[,] pause = new float[10, 3];
        public float[,] impulse = new float[10, 3];
        public float[] tzero = new float[10];
        public float[] tg = new float[10];
        public UInt16[] f1 = new UInt16[10];
        public UInt16[] f2 = new UInt16[10];
        public UInt16[] num = new UInt16[10];
        public UInt16[] baud = new UInt16[10];
        public UInt16[] out_mode = new UInt16[10];
        public UInt16[] autozero = new UInt16[10];

        public UInt16[] polar = new UInt16[10];
        public UInt16[] discr = new UInt16[10];
        public UInt16[] zpt = new UInt16[10];
        public UInt16[] hz = new UInt16[10];
        public UInt16[] mv = new UInt16[10];
        public UInt32[] cal_zero = new UInt32[10];
        public float[] cal_weight = new float[10];
        public float[] coeff = new float[10];
        public float[] npv = new float[10];

        public float[] lite_zero_weight = new float[10];
        public float[] lite_stab_weight = new float[10];
        public UInt16[] lite_stab_f1 = new UInt16[10];
        public float[] lite_tzero = new float[10];
        public float[] lite_w_zero = new float[10];

        public UInt16[] lite_mode = new UInt16[10];
        public UInt16[] lite_direction = new UInt16[10];
        public UInt16[] lite_point_num = new UInt16[10];

        private string out_str;


        public async Task SaveKVFile(string filename)
        {
            string content = "";

            for (ushort i = 0; i < 10; i++)
            {
                content += "КОНТРОЛЕР="; content += (i + 1); content += "\n";
                content += "ВЕРСИЯ=11_02"; content += "\n";
                content += "ДОЗА1="; content += doze[i, 0]; content += "\n";
                content += "ДОЗА2="; content += doze[i, 1]; content += "\n";
                content += "ДОЗА3="; content += doze[i, 2]; content += "\n";
                content += "ДОЗА4="; content += doze[i, 3]; content += "\n";
                content += "ДОЗА5="; content += doze[i, 4]; content += "\n";
                content += "ДОЗА6="; content += doze[i, 5]; content += "\n";
                content += "ГРУБО1="; content += dw[i, 0]; content += "\n";
                content += "ГРУБО2="; content += dw[i, 1]; content += "\n";
                content += "ГРУБО3="; content += dw[i, 2]; content += "\n";
                content += "ТОЧНО1="; content += dwi[i, 0]; content += "\n";
                content += "ТОЧНО2="; content += dwi[i, 1]; content += "\n";
                content += "ТОЧНО3="; content += dwi[i, 2]; content += "\n";
                content += "ПАУЗА1="; content += pause[i, 0]; content += "\n";
                content += "ПАУЗА2="; content += pause[i, 1]; content += "\n";
                content += "ПАУЗА3="; content += pause[i, 2]; content += "\n";
                content += "ИМПУЛЬС1="; content += impulse[i, 0]; content += "\n";
                content += "ИМПУЛЬС2="; content += impulse[i, 1]; content += "\n";
                content += "ИМПУЛЬС3="; content += impulse[i, 2]; content += "\n";
                content += "ВРЕМЯ НУЛЯ="; content += tzero[i]; content += "\n";
                content += "ВЕС_УШЕЛ="; content += tg[i]; content += "\n";
                content += "ФИЛЬТР1="; content += f1[i]; content += "\n";
                content += "ФИЛЬТР2="; content += f2[i]; content += "\n";
                content += "СЕТЕВОЙ_НОМЕР="; content += num[i]; content += "\n";
                content += "СКОРОСТЬ_ОБМЕНА="; content += baud[i]; content += "\n";
                content += "РЕЖИМ_ВЫХ="; content += out_mode[i]; content += "\n";
                content += "АВТОНОЛЬ="; content += autozero[i]; content += "\n";
                content += "ПОЛЯРНОСТЬ="; content += polar[i]; content += "\n";
                content += "ДИСКРЕТНОСТЬ="; content += discr[i]; content += "\n";
                content += "ТОЧНОСТЬ="; content += zpt[i]; content += "\n";
                content += "ЧАСТОТА_АЦП="; content += hz[i]; content += "\n";
                content += "ДИАПАЗОН_АЦП="; content += mv[i]; content += "\n";
                content += "КОД_НУЛЯ="; content += cal_zero[i]; content += "\n";
                content += "НПВ="; content += npv[i]; content += "\n";
                content += "КАЛИБРОВОЧНЫЙ_ВЕС="; content += cal_weight[i]; content += "\n";
                content += "КОЭФФИЦИЕНТ_КАЛИБРОВКИ="; content += coeff[i]; content += "\n";
                content += "СМЕЩЕНИЕ_НУЛЯ_ЛАЙТ="; content += lite_zero_weight[i]; content += "\n";
                content += "ДИАПАЗОН_СТАБИЛЬНОГО_ВЕСА="; content += lite_stab_weight[i]; content += "\n";
                content += "ФИЛЬТР_СТАБИЛЬНОГО_ВЕСА="; content += lite_stab_f1[i]; content += "\n";
                content += "ВРЕМЯ_УСТАНОВКИ_НУЛЯ="; content += lite_tzero[i]; content += "\n";
                content += "ДИАПАЗОН_НУЛЕВОГО_ВЕСА="; content += lite_w_zero[i]; content += "\n";
                content += "РЕЖИМ_РАБОТЫ_УЧЕТА="; content += lite_mode[i]; content += "\n";
                content += "НАПРАВЛЕНИЕ_ПЕРЕДАЧИ="; content += lite_direction[i]; content += "\n";
                content += "КОЛ_ВО_ТОЧЕК_КАЛИБРОВКИ="; content += lite_point_num[i]; content += "\n";
                
            }
            //   content = content.Replace(".", ",");

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            // write the char array created from the content string into the file
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
            }


        }
        public async System.Threading.Tasks.Task OpenKVFile(string fileName)
        {
            string s;
            StorageFile file;
            UInt16 tn = 0;
            var _folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            file = await _folder.GetFileAsync(fileName);

            var lines = await Windows.Storage.FileIO.ReadLinesAsync(file);

            out_str = "";
            if (file != null)
            {
                foreach (var line in lines)
                {
                    if (line.Contains("КОНТРОЛЕР="))
                    {
                        s = line.Replace("КОНТРОЛЕР=", "");
                        { tn = Convert.ToUInt16(s); tn--; }
                    }

                    if (line.Contains("ДОЗА1="))
                    {
                        s = line.Replace("ДОЗА1=", ""); doze[tn, 0] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ДОЗА2="))
                    {
                        s = line.Replace("ДОЗА2=", ""); doze[tn, 1] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ДОЗА3="))
                    {
                        s = line.Replace("ДОЗА3=", ""); doze[tn, 2] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ДОЗА4="))
                    {
                        s = line.Replace("ДОЗА4=", ""); doze[tn, 3] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ДОЗА5="))
                    {
                        s = line.Replace("ДОЗА5=", ""); doze[tn, 4] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ДОЗА6="))
                    {
                        s = line.Replace("ДОЗА6=", ""); doze[tn, 5] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ГРУБО1="))
                    {
                        s = line.Replace("ГРУБО1=", ""); dw[tn, 0] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ГРУБО2="))
                    {
                        s = line.Replace("ГРУБО2=", ""); dw[tn, 1] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ГРУБО3="))
                    {
                        s = line.Replace("ГРУБО3=", ""); dw[tn, 2] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ТОЧНО1="))
                    {
                        s = line.Replace("ТОЧНО1=", ""); dwi[tn, 0] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ТОЧНО2="))
                    {
                        s = line.Replace("ТОЧНО2=", ""); dwi[tn, 1] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ТОЧНО3="))
                    {
                        s = line.Replace("ТОЧНО3=", ""); dwi[tn, 2] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ИМПУЛЬС1="))
                    {
                        s = line.Replace("ИМПУЛЬС1=", ""); impulse[tn, 0] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ИМПУЛЬС2="))
                    {
                        s = line.Replace("ИМПУЛЬС2=", ""); impulse[tn, 1] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ИМПУЛЬС3="))
                    {
                        s = line.Replace("ИМПУЛЬС3=", ""); impulse[tn, 2] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ПАУЗА1="))
                    {
                        s = line.Replace("ПАУЗА1=", ""); pause[tn, 0] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ПАУЗА2="))
                    {
                        s = line.Replace("ПАУЗА2=", ""); pause[tn, 1] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ПАУЗА3="))
                    {
                        s = line.Replace("ПАУЗА3=", ""); pause[tn, 2] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ВРЕМЯ НУЛЯ="))
                    {
                        s = line.Replace("ВРЕМЯ НУЛЯ=", ""); tzero[tn] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ВЕС_УШЕЛ="))
                    {
                        s = line.Replace("ВЕС_УШЕЛ=", ""); tg[tn] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ФИЛЬТР1="))
                    {
                        s = line.Replace("ФИЛЬТР1=", ""); f1[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ФИЛЬТР2="))
                    {
                        s = line.Replace("ФИЛЬТР2=", ""); f2[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("СЕТЕВОЙ_НОМЕР="))
                    {
                        s = line.Replace("СЕТЕВОЙ_НОМЕР=", ""); num[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("СКОРОСТЬ_ОБМЕНА="))
                    {
                        s = line.Replace("СКОРОСТЬ_ОБМЕНА=", ""); baud[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("РЕЖИМ_ВЫХ="))
                    {
                        s = line.Replace("РЕЖИМ_ВЫХ=", ""); out_mode[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("АВТОНОЛЬ="))
                    {
                        s = line.Replace("АВТОНОЛЬ=", ""); autozero[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ПОЛЯРНОСТЬ="))
                    {
                        s = line.Replace("ПОЛЯРНОСТЬ=", ""); polar[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ДИСКРЕТНОСТЬ="))
                    {
                        s = line.Replace("ДИСКРЕТНОСТЬ=", ""); discr[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ТОЧНОСТЬ="))
                    {
                        s = line.Replace("ТОЧНОСТЬ=", ""); zpt[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ЧАСТОТА_АЦП="))
                    {
                        s = line.Replace("ЧАСТОТА_АЦП=", ""); hz[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ДИАПАЗОН_АЦП="))
                    {
                        s = line.Replace("ДИАПАЗОН_АЦП=", ""); mv[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("КОД_НУЛЯ="))
                    {
                        s = line.Replace("КОД_НУЛЯ=", ""); cal_zero[tn] = Convert.ToUInt32(s);
                    }
                    if (line.Contains("НПВ="))
                    {
                        s = line.Replace("НПВ=", ""); npv[tn] = Convert.ToSingle(s);
                    }
                    if (line.Contains("КАЛИБРОВОЧНЫЙ_ВЕС="))
                    {
                        s = line.Replace("КАЛИБРОВОЧНЫЙ_ВЕС=", ""); cal_weight[tn] = Convert.ToSingle(s);
                    }
                    if (line.Contains("КОЭФФИЦИЕНТ_КАЛИБРОВКИ="))
                    {
                        s = line.Replace("КОЭФФИЦИЕНТ_КАЛИБРОВКИ=", ""); coeff[tn] = Convert.ToSingle(s);
                    }
                    // Для лайта
                    if (line.Contains("СМЕЩЕНИЕ_НУЛЯ_ЛАЙТ="))
                    {
                        s = line.Replace("СМЕЩЕНИЕ_НУЛЯ_ЛАЙТ=", ""); lite_zero_weight[tn] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ДИАПАЗОН_СТАБИЛЬНОГО_ВЕСА="))
                    {
                        s = line.Replace("ДИАПАЗОН_СТАБИЛЬНОГО_ВЕСА=", ""); lite_stab_weight[tn] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ФИЛЬТР_СТАБИЛЬНОГО_ВЕСА="))
                    {
                        s = line.Replace("ФИЛЬТР_СТАБИЛЬНОГО_ВЕСА=", ""); lite_stab_f1[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("ВРЕМЯ_УСТАНОВКИ_НУЛЯ="))
                    {
                        s = line.Replace("ВРЕМЯ_УСТАНОВКИ_НУЛЯ=", ""); lite_tzero[tn] = Convert.ToSingle(s);
                    }
                    if (line.Contains("ДИАПАЗОН_НУЛЕВОГО_ВЕСА="))
                    {
                        s = line.Replace("ДИАПАЗОН_НУЛЕВОГО_ВЕСА=", ""); lite_w_zero[tn] = Convert.ToSingle(s);
                    }
                    if (line.Contains("РЕЖИМ_РАБОТЫ_УЧЕТА="))
                    {
                        s = line.Replace("РЕЖИМ_РАБОТЫ_УЧЕТА=", ""); lite_mode[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("НАПРАВЛЕНИЕ_ПЕРЕДАЧИ="))
                    {
                        s = line.Replace("НАПРАВЛЕНИЕ_ПЕРЕДАЧИ=", ""); lite_direction[tn] = Convert.ToUInt16(s);
                    }
                    if (line.Contains("КОЛ_ВО_ТОЧЕК_КАЛИБРОВКИ="))
                    {
                        s = line.Replace("КОЛ_ВО_ТОЧЕК_КАЛИБРОВКИ=", ""); lite_point_num[tn] = Convert.ToUInt16(s);
                    }


                }

            }
            else
            {
                await SaveKVFile(fileName);
            }

        }

    }
}
