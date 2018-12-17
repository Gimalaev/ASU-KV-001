using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using System;
using System.Text.RegularExpressions;
using SQLite;


// https://code.msdn.microsoft.com/windowsapps/Reading-data-from-multiple-ceb58872#content
// https://msdn.microsoft.com/ru-ru/windows/uwp/files/quickstart-reading-and-writing-files

namespace Common
{

    public class Full_List
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Дата { get; set; }
        public string Время { get; set; }
        public byte Смена { get; set; }
        public byte Контроллер { get; set; }
        public string Продукт { get; set; }
        public float Полная_Доза { get; set; }
        public float Компонент_1 { get; set; }
        public float Компонент_2 { get; set; }
        public float Компонент_3 { get; set; }

    }

    class Archive
    {


        public string[,] Last_Time = new string[10, 5];
        public string[,] Last_Product = new string[10, 5];
        public float[,] Last_Dose = new float[10, 5];

        public void Drop_Base_Full()
            {

            var db = new SQLiteConnection("bd_archive.db", true);
            //db.DropTable<Full_List>();
            db.CreateTable<Full_List>();
//            db.CreateTable<Payment>();
//            db.CreateTable<Company>();
//            Make_Company();
        db.Dispose();
//            Company_To_List();
    }


        public void Sql_Add(byte sm, DateTime dt, byte term, string rec, float dose0, float dose1, float dose2, float dose3)
        {
            string date, time;
            date = dt.ToString("dd.MM.yyyy");
            time = dt.ToString("HH:mm");

            var db = new SQLiteConnection("bd_archive.db", true);
            var pm = new Full_List();

            pm.Время = time;
            pm.Дата = date;
            pm.Смена = sm;
            pm.Контроллер = term;
            pm.Продукт = rec;
            pm.Полная_Доза = dose0;
            pm.Компонент_1 = dose1;
            pm.Компонент_2 = dose2;
            pm.Компонент_3 = dose3;
            db.Insert(pm);

            Last_Time[term - 1, 4] = Last_Time[term - 1, 3];
            Last_Time[term - 1, 3] = Last_Time[term - 1, 2];
            Last_Time[term - 1, 2] = Last_Time[term - 1, 1];
            Last_Time[term - 1, 1] = Last_Time[term - 1, 0];
            Last_Time[term - 1, 0] = time;

            Last_Product[term - 1, 4] = Last_Product[term - 1, 3];
            Last_Product[term - 1, 3] = Last_Product[term - 1, 2];
            Last_Product[term - 1, 2] = Last_Product[term - 1, 1];
            Last_Product[term - 1, 1] = Last_Product[term - 1, 0];
            Last_Product[term - 1, 0] = rec;

            Last_Dose[term - 1, 4] = Last_Dose[term - 1, 3];
            Last_Dose[term - 1, 3] = Last_Dose[term - 1, 2];
            Last_Dose[term - 1, 2] = Last_Dose[term - 1, 1];
            Last_Dose[term - 1, 1] = Last_Dose[term - 1, 0];
            Last_Dose[term - 1, 4] = dose0;



            db.Dispose();


            /*     Sql_Start(dt, sm, smena_st_hour, smena_st_minute);
                 var s = conn.Insert(new Sql_Smena()
                 { Arc_Smena = sm, Arc_Date = date, Arc_Time = time, Arc_Rec = rec, Arc_Doza = fl });
                 conn.Dispose();
                 Time_Last[4] = Time_Last[3]; Time_Last[3] = Time_Last[2]; Time_Last[2] = Time_Last[1]; Time_Last[1] = Time_Last[0];
                 Time_Last[0] = time;
                 Smena_Last[4] = Smena_Last[3]; Smena_Last[3] = Smena_Last[2]; Smena_Last[2] = Smena_Last[1]; Smena_Last[1] = Smena_Last[0];
                 Smena_Last[0] = sm;
                 Doza_Last[4] = Doza_Last[3]; Doza_Last[3] = Doza_Last[2]; Doza_Last[2] = Doza_Last[1]; Doza_Last[1] = Doza_Last[0];
                 Doza_Last[0] = fl;
                 Rec_Last[4] = Rec_Last[3]; Rec_Last[3] = Rec_Last[2]; Rec_Last[2] = Rec_Last[1]; Rec_Last[1] = Rec_Last[0];
                 Rec_Last[0] = rec;
                 conn.Dispose();*/

        }
        /*        public string path;

                public SQLite.Net.SQLiteConnection conn;


                public string[] Time_Last = { "-", "-", "-", "-", "-", "-" };
                public byte[] Smena_Last = { 0, 0, 0, 0, 0 };
                public float[] Doza_Last = { 0, 0, 0, 0, 0 };
                public byte[] Rec_Last = { 0, 0, 0, 0, 0 };


                public void Sql_Start(DateTime dt, byte smena, byte smena_st_hour, byte smena_st_minute)
                {
                    string filename = StartSmenaDate(dt, smena, smena_st_hour, smena_st_minute);
                    path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "db_asu.sqlite");
                    conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
                    conn.CreateTable<Sql_Smena>();
                }
                public void Sql_Add(byte sm, DateTime dt, byte rec, float fl, byte smena_st_hour, byte smena_st_minute)
                {
                    string date, time;
                    date = dt.ToString("dd.MM.yyyy");
                    time = dt.ToString("HH:mm:ss");
                    Sql_Start(dt, sm, smena_st_hour, smena_st_minute);
                    var s = conn.Insert(new Sql_Smena()
                    { Arc_Smena = sm, Arc_Date = date, Arc_Time = time, Arc_Rec = rec, Arc_Doza = fl });
                    conn.Dispose();
                    Time_Last[4] = Time_Last[3]; Time_Last[3] = Time_Last[2]; Time_Last[2] = Time_Last[1]; Time_Last[1] = Time_Last[0];
                    Time_Last[0] = time;
                    Smena_Last[4] = Smena_Last[3]; Smena_Last[3] = Smena_Last[2]; Smena_Last[2] = Smena_Last[1]; Smena_Last[1] = Smena_Last[0];
                    Smena_Last[0] = sm;
                    Doza_Last[4] = Doza_Last[3]; Doza_Last[3] = Doza_Last[2]; Doza_Last[2] = Doza_Last[1]; Doza_Last[1] = Doza_Last[0];
                    Doza_Last[0] = fl;
                    Rec_Last[4] = Rec_Last[3]; Rec_Last[3] = Rec_Last[2]; Rec_Last[2] = Rec_Last[1]; Rec_Last[1] = Rec_Last[0];
                    Rec_Last[0] = rec;
                    conn.Dispose();

                }
                public void Sql_Read_Last(byte sm, DateTime dt, byte smena_st_hour, byte smena_st_minute)
                {
                    Sql_Start(dt, sm, smena_st_hour, smena_st_minute);
                    var query = conn.Table<Sql_Smena>();
                    foreach (var message in query)
                    {
                        Time_Last[4] = Time_Last[3]; Time_Last[3] = Time_Last[2]; Time_Last[2] = Time_Last[1]; Time_Last[1] = Time_Last[0];
                        Time_Last[0] = message.Arc_Time;
                        Smena_Last[4] = Smena_Last[3]; Smena_Last[3] = Smena_Last[2]; Smena_Last[2] = Smena_Last[1]; Smena_Last[1] = Smena_Last[0];
                        Smena_Last[0] = message.Arc_Smena;
                        Doza_Last[4] = Doza_Last[3]; Doza_Last[3] = Doza_Last[2]; Doza_Last[2] = Doza_Last[1]; Doza_Last[1] = Doza_Last[0];
                        Doza_Last[0] = message.Arc_Doza;
                        Rec_Last[4] = Rec_Last[3]; Rec_Last[3] = Rec_Last[2]; Rec_Last[2] = Rec_Last[1]; Rec_Last[1] = Rec_Last[0];
                        Rec_Last[0] = message.Arc_Rec;
                    }




                    conn.Dispose();

                }
                public string StartSmenaDate(DateTime dt, byte smena, byte smena_st_hour, byte smena_st_minute)
                {
                    int a, b, c, d, e, m, y, JDN;
                    long secnow, secsmen;


                    //вычисляем день по юлианскому календарю.
                    a = (int)((14 - dt.Month) / 12);
                    y = (int)(dt.Year + 4800 - a);
                    m = (int)(dt.Month + (12 * a) - 3);
                    JDN = (int)(dt.Day + (int)((153 * m + 2) / 5) + 365 * y + (int)(y / 4) - (int)(y / 100) + (int)(y / 400) - 32045);

                    secnow = dt.Second + dt.Minute * 60 + (dt.Hour) * 60 * 60;
                    secsmen = (smena_st_minute * 30) * 60 + (smena_st_hour) * 60 * 60;

                    if (secnow < secsmen) JDN--;

                    a = JDN + 32044;
                    b = (int)((4 * a + 3) / 146097);
                    c = a - (int)((146097 * b) / 4);
                    d = (int)((4 * c + 3) / 1461);
                    e = c - (int)((1461 * d) / 4);
                    m = (5 * e + 2) / 153;

                    DateTime start_sm_dt = new DateTime(100 * b + d - 4800 + ((int)(m / 10)), m + 3 - 12 * ((int)(m / 10)), (int)(e - (int)((153 * m + 2) / 5) + 1));

                    string rts = start_sm_dt.ToString("dd_MM_yyyy") + "_smena_" + smena;

                    return (rts);
                }*/
    }
    class Prg_Par
    {
        public int SmenaNum;                //Количество смен
        public int SmenaHour;               // Начало первой смены - час
        public int SmenaMinute;             //Минуты начала первой смены 0 - 0минут, 1 - 30 минут.

        public int ComAdress;
        public int ComBaud;
        public int ComMode;
        public int ComDevice;
        public int ComMonitor;

        public string out_str;



        public async Task SavePrgFile(string filename)
        {

            string content = "";
            content += "SmenaNum="; content += Convert.ToString(this.SmenaNum); content += ";\n"; //Количество смен
            content += "SmenaHour="; content += Convert.ToString(this.SmenaHour); content += ";\n"; // Начало первой смены - час
            content += "SmenaMinute="; content += Convert.ToString(this.SmenaMinute); content += ";\n"; //Минуты начала первой смены 0 - 0минут, 1 - 30 минут.
            content += "ComAdress="; content += Convert.ToString(this.ComAdress); content += ";\n"; //Минуты начала первой смены 0 - 0минут, 1 - 30 минут.
            content += "ComBaud="; content += Convert.ToString(this.ComBaud); content += ";\n"; //Минуты начала первой смены 0 - 0минут, 1 - 30 минут.
            content += "ComMode="; content += Convert.ToString(this.ComMode); content += ";\n"; //Минуты начала первой смены 0 - 0минут, 1 - 30 минут.
            content += "ComDevice="; content += Convert.ToString(this.ComDevice); content += ";\n"; //Минуты начала первой смены 0 - 0минут, 1 - 30 минут.
            content += "ComMonitor="; content += Convert.ToString(this.ComMonitor); content += ";\n"; //Минуты начала первой смены 0 - 0минут, 1 - 30 минут.


            content = content.Replace(",", ".");

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            // write the char array created from the content string into the file
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
            }


        }
        public async System.Threading.Tasks.Task OpenPrgFile(string fileName)
        {
            StorageFile sampleFile;
            var _folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            sampleFile = await _folder.GetFileAsync(fileName);
            out_str = "";
            if (sampleFile != null)
            {
                out_str = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
                string separator = ";";
                string pattern = string.Format("{0}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", separator);
                string[] result = Regex.Split(out_str, pattern);

                out_str = result[0];
                out_str = out_str.Replace("SmenaNum=", "");
                out_str = out_str.Replace(".", ",");
                this.SmenaNum = Convert.ToInt16(out_str);
                out_str = result[1];
                out_str = out_str.Replace("SmenaHour=", "");
                out_str = out_str.Replace(".", ",");
                this.SmenaHour = Convert.ToInt16(out_str);
                out_str = result[2];
                out_str = out_str.Replace("SmenaMinute=", "");
                out_str = out_str.Replace(".", ",");
                this.SmenaMinute = Convert.ToInt16(out_str);
                out_str = result[3];
                out_str = out_str.Replace("ComAdress=", "");
                out_str = out_str.Replace(".", ",");
                this.ComAdress = Convert.ToInt16(out_str);
                out_str = result[4];
                out_str = out_str.Replace("ComBaud=", "");
                out_str = out_str.Replace(".", ",");
                this.ComBaud = Convert.ToInt16(out_str);
                out_str = result[5];
                out_str = out_str.Replace("ComMode=", "");
                out_str = out_str.Replace(".", ",");
                this.ComMode = Convert.ToInt16(out_str);
                out_str = result[6];
                out_str = out_str.Replace("ComDevice=", "");
                out_str = out_str.Replace(".", ",");
                this.ComDevice = Convert.ToInt16(out_str);
                out_str = result[7];
                out_str = out_str.Replace("ComMonitor=", "");
                out_str = out_str.Replace(".", ",");
                this.ComMonitor = Convert.ToInt16(out_str);



            }
            else
            {
                await SavePrgFile(fileName);
            }

        }
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
            h1 = 86400; m1 = (UInt32)(SmenaNum+1);
            pl_tm = h1 / m1;
            i = 0;
            for (o = 1; o <= (SmenaNum+1); o++)
            {

                if (n_tm >= f_tm) i++;
                f_tm += pl_tm;
            }

            if (i == 0) i = (byte)(SmenaNum+1);
            return (i);

        }
        /*
                void StartSmenaDate(SYSTEMTIME dt, unsigned char smena)
                {
                    int a, b, c, d, e, m, y, JDN;
                    long secnow, secsmen;

                    //вычисляем день по юлианскому календарю.
                    a = int((14 - dt.wMonth) / 12);
                    y = dt.wYear + 4800 - a;
                    m = dt.wMonth + (12 * a) - 3;
                    JDN = dt.wDay + int((153 * m + 2) / 5) + 365 * y + int(y / 4) - int(y / 100) + int(y / 400) - 32045;

                    secnow = dt.wSecond + dt.wMinute * 60 + (dt.wHour) * 60 * 60;
                    secsmen = smena_par.second + smena_par.minute * 60 + (smena_par.hour) * 60 * 60;

                    if (secnow < secsmen) JDN--;

                    a = JDN + 32044;
                    b = int((4 * a + 3) / 146097);
                    c = a - int((146097 * b) / 4);
                    d = int((4 * c + 3) / 1461);
                    e = c - int((1461 * d) / 4);
                    m = (5 * e + 2) / 153;

                    start_sm_dt.wDay = e - int((153 * m + 2) / 5) + 1;
                    start_sm_dt.wMonth = m + 3 - 12 * (int(m / 10));
                    start_sm_dt.wYear = 100 * b + d - 4800 + (int(m / 10));
                }
        */


    }
    class KV001
    {
        public float Doza=30;
        public float Dw = 5;
        public float Dwi = 2;
        public float Zero = 4;
        public float TZero = 2;
        public float TPause = 3;
        public byte DozeMode = 1;
        public byte F1 = 1;
        public byte F2 = 2;
        public byte Baud = 1;
        public byte Adr = 1;
        public byte Type =0; 
        public byte Auto_Zero =0; 
        public byte Modbus_Time =0;

        public byte Mv; 
        public byte Hz; 
        public byte Discr; 
        public byte Point; 
        public float Npv; 
        public float Cal_Weight;
        public float Coef;
        public uint Shift; 
        public byte Polar;

   //     public byte inputs;
   //     public byte outputs;
        public float Weight = 0;
        public byte State = 0;
        public byte Archive_State = 0;
        public string out_str;

        public float sum_last;
        public UInt32 count_last;
        public float doza_last;


        public async Task SaveKVFile(string filename)
        {
            // StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            // var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            // saves the string 'content' to a file 'filename' in the app's local storage folder

            string content="";
            content += "DOZA=";
            content += Convert.ToString(this.Doza);
            content += ";\n";

            content += "DW=";
            content += Convert.ToString(this.Dw);
            content += ";\n";

            content += "DWi=";
            content += Convert.ToString(this.Dwi);
            content += ";\n";

            content += "ZERO=";
            content += Convert.ToString(this.Zero);
            content += ";\n";

            content += "TZERO=";
            content += Convert.ToString(this.TZero);
            content += ";\n";

            content += "TPAUSE=";
            content += Convert.ToString(this.TPause);
            content += ";\n";

            content += "DOZE_MODE=";
            content += Convert.ToString(this.DozeMode);
            content += ";\n";

            content += "F1=";
            content += Convert.ToString(this.F1);
            content += ";\n";

            content += "F2=";
            content += Convert.ToString(this.F2);
            content += ";\n";

            content += "BAUD=";
            content += Convert.ToString(this.Baud);
            content += ";\n";

            content += "ADR=";
            content += Convert.ToString(this.Adr);
            content += ";\n";

            content += "TYPE=";
            content += Convert.ToString(this.Type);
            content += ";\n";

            content += "AUTOZERO=";
            content += Convert.ToString(this.Auto_Zero);
            content += ";\n";

            content += "MODBUSTIME=";
            content += Convert.ToString(this.Modbus_Time);
            content += ";\n";

            content += "MV="; content += Convert.ToString(this.Mv); content += ";\n";
            content += "HZ="; content += Convert.ToString(this.Hz); content += ";\n";
            content += "DISCR="; content += Convert.ToString(this.Discr); content += ";\n";
            content += "POINT="; content += Convert.ToString(this.Point); content += ";\n";
            content += "NPV="; content += Convert.ToString(this.Npv); content += ";\n";
            content += "CAL_WEIGHT="; content += Convert.ToString(this.Cal_Weight); content += ";\n";
            content += "COEF="; content += Convert.ToString(this.Coef); content += ";\n";
            content += "SHIFT="; content += Convert.ToString(this.Shift); content += ";\n";
            content += "POLAR="; content += Convert.ToString(this.Polar); content += ";\n";


            content = content.Replace(",", ".");

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            // write the char array created from the content string into the file
            using (var stream = await file.OpenStreamForWriteAsync())
                            {
                                stream.Write(fileBytes, 0, fileBytes.Length);
                            }

                
        }
        public async System.Threading.Tasks.Task OpenFile(string fileName)
        {
            StorageFile sampleFile;
            var _folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            sampleFile = await _folder.GetFileAsync(fileName);
            out_str = "";
            if (sampleFile != null)
            {
                out_str = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
                string separator = ";";
                string pattern = string.Format("{0}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", separator);
                string[] result = Regex.Split(out_str, pattern);
                out_str = result[0];
                out_str = out_str.Replace("DOZA=", "");
                out_str = out_str.Replace(".", ",");
                this.Doza =  Convert.ToSingle(out_str);

                out_str = result[1];
                out_str = out_str.Replace("DW=", "");
                out_str = out_str.Replace(".", ",");
                this.Dw = Convert.ToSingle(out_str);

                out_str = result[2];
                out_str = out_str.Replace("DWi=", "");
                out_str = out_str.Replace(".", ",");
                this.Dwi = Convert.ToSingle(out_str);

                out_str = result[3];
                out_str = out_str.Replace("ZERO=", "");
                out_str = out_str.Replace(".", ",");
                this.Zero = Convert.ToSingle(out_str);

                out_str = result[4];
                out_str = out_str.Replace("TZERO=", "");
                out_str = out_str.Replace(".", ",");
                this.TZero = Convert.ToSingle(out_str);

                out_str = result[5];
                out_str = out_str.Replace("TPAUSE=", "");
                out_str = out_str.Replace(".", ",");
                this.TPause = Convert.ToSingle(out_str);

                out_str = result[6];
                out_str = out_str.Replace("DOZE_MODE=", "");
                out_str = out_str.Replace(".", ",");
                this.DozeMode = Convert.ToByte(out_str);

               out_str = result[7];
                out_str = out_str.Replace("F1=", "");
                out_str = out_str.Replace(".", ",");
                this.F1 = Convert.ToByte(out_str);

                out_str = result[8];
                out_str = out_str.Replace("F2=", "");
                out_str = out_str.Replace(".", ",");
                this.F2 = Convert.ToByte(out_str);

                out_str = result[9];
                out_str = out_str.Replace("BAUD=", "");
                out_str = out_str.Replace(".", ",");
                this.Baud = Convert.ToByte(out_str);

                out_str = result[10];
                out_str = out_str.Replace("ADR=", "");
                out_str = out_str.Replace(".", ",");
                this.Adr = Convert.ToByte(out_str);

                out_str = result[11];
                out_str = out_str.Replace("TYPE=", "");
                out_str = out_str.Replace(".", ",");
                this.Type = Convert.ToByte(out_str);

                out_str = result[12];
                out_str = out_str.Replace("AUTOZERO=", "");
                out_str = out_str.Replace(".", ",");
                this.Auto_Zero = Convert.ToByte(out_str);

                out_str = result[13];
                out_str = out_str.Replace("MODBUSTIME=", "");
                out_str = out_str.Replace(".", ",");
                this.Modbus_Time = Convert.ToByte(out_str);

                out_str = result[14];
                out_str = out_str.Replace("MV=", "");
                out_str = out_str.Replace(".", ",");
                this.Mv = Convert.ToByte(out_str);

                out_str = result[15];
                out_str = out_str.Replace("HZ=", "");
                out_str = out_str.Replace(".", ",");
                this.Hz = Convert.ToByte(out_str);

                out_str = result[16];
                out_str = out_str.Replace("DISCR=", "");
                out_str = out_str.Replace(".", ",");
                this.Discr = Convert.ToByte(out_str);

                out_str = result[17];
                out_str = out_str.Replace("POINT=", "");
                out_str = out_str.Replace(".", ",");
                this.Point = Convert.ToByte(out_str);

                out_str = result[18];
                out_str = out_str.Replace("NPV=", "");
                out_str = out_str.Replace(".", ",");
                this.Npv = Convert.ToByte(out_str);

                out_str = result[19];
                out_str = out_str.Replace("CAL_WEIGHT=", "");
                out_str = out_str.Replace(".", ",");
                this.Cal_Weight = Convert.ToSingle(out_str);

                out_str = result[20];
                out_str = out_str.Replace("COEF=", "");
                out_str = out_str.Replace(".", ",");
                this.Coef = Convert.ToSingle(out_str);

                out_str = result[21];
                out_str = out_str.Replace("SHIFT=", "");
                out_str = out_str.Replace(".", ",");
                this.Shift = Convert.ToUInt16(out_str);

                out_str = result[22];
                out_str = out_str.Replace("POLAR=", "");
                out_str = out_str.Replace(".", ",");
                this.Polar = Convert.ToByte(out_str);


            }
            else
            {
                await SaveKVFile(fileName);
            }

        }



    }
    }