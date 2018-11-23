using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SQLite;



// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace ASU_KV_001
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        private SolidColorBrush BrushOff;
        private SolidColorBrush BrushOn;
        private SolidColorBrush BrushParamOn;

        private bool par_flag = false;
        private UInt16 term_now = 0;
        private UInt16 arc_read_term = 0; //номер треминала для считывания архив
        private UInt16 mode = 0;
        private ASU_KV_001.KV001 kv_par;
        private ASU_KV_001.Program_Par prg_par;
        private ASU_KV_001.Products products;
        private Common.Archive db_archive;
        private string param_filename = "kv_par.ini";
        private string prg_filename = "prg_par.ini";
        private string prod_filename = "product_par.ini";

        //Здесь у нас все что относится к модбасу.
        private ModBus_Class serial_port;

        private DispatcherTimer tmOutScreen;
        private DispatcherTimer tmComReq;
        private byte num_reg_read = 0;
        private byte reg_try = 0;
        private UInt16 write_reg_wait = 0;
        private byte reg_status_visible = 0;
        private byte term_num_req = 0;
        // Конец модбаса//

        private short next_adress = -1;

        public async System.Threading.Tasks.Task InitKV()
        {
            // param.SmenaHour = 0;
            // param.SmenaMinute = 0;
            // param.SmenaNum = 0;
            //  
            try
            {
                await kv_par.OpenKVFile(param_filename);
            }
            catch
            {
                await kv_par.SaveKVFile(param_filename);
            }
        }
        public async System.Threading.Tasks.Task InitProduct()
        {
            try
            {
                await products.OpenProdFile(prod_filename);
            }
            catch
            {
                await products.SaveProductFile(prod_filename);
           }
        }

        public async System.Threading.Tasks.Task InitPrg()
        {
            // param.SmenaHour = 0;
            // param.SmenaMinute = 0;
            // param.SmenaNum = 0;
            //  
            try
            {
                await prg_par.OpenParFile(prg_filename);
            }
            catch
            {
                await prg_par.SaveParFile(prg_filename);
            }
        }

        private async void Main_Page_Loaded(object sender, RoutedEventArgs e)
        {
            await InitKV();

            await InitPrg();


            Text_Doza_1.Text = "ДОЗА: " + Convert.ToString(kv_par.doze[0, 0]);
            Text_Doza_2.Text = "ДОЗА: " + Convert.ToString(kv_par.doze[0, 1]);
            Text_Doza_3.Text = "ДОЗА: " + Convert.ToString(kv_par.doze[0, 2]);
            Text_Doza_1.Text = Text_Doza_1.Text.Replace(",", ".");
            Text_Doza_2.Text = Text_Doza_3.Text.Replace(",", ".");
            Text_Doza_3.Text = Text_Doza_3.Text.Replace(",", ".");


            UpdatePostEnabled();

            SolidColorBrush BrushOnPress = new SolidColorBrush();
            BrushOnPress.Color = Windows.UI.Color.FromArgb(0, 0x5C, 0x76, 0x82);
            Grid_Post_1.Background = BrushOnPress;

            Combo_Post_ID.SelectedIndex = prg_par.selected_id[0];

            btnComConnect.IsEnabled = false;
            btnComTry.IsEnabled = false;

            btnComConnect.IsEnabled = true;
            //  ConnectDevices.SelectedIndex = -1;
            await serial_port.ListAvailablePorts();
            tbModbusStatus.Text = serial_port.status;
            int i = prg_par.ComDevice;
            if (!serial_port.error)
            {
                DeviceListSource.Source = serial_port.listOfDevices;
                if (ConnectDevices.Items.Count > i)
                    ConnectDevices.SelectedIndex = i;
                else prg_par.ComMonitor = 0;
            }

            if ((prg_par.ComMode == 1) && (prg_par.ComMonitor == 1))
            {
                btnComConnect.IsEnabled = false;
                await serial_port.OpenDevices(ConnectDevices);
                tbModbusStatus.Text = serial_port.status;
                btnComTry.IsEnabled = true;
                if (serial_port.error)
                {
                    btnComConnect.IsEnabled = true;
                    btnComTry.IsEnabled = false;
                }
                else
                {
                    serial_port.wait_answer = 0;
                    tmComReq.Start();
                    serial_port.rd_mode = 1;
                }
            }

            await InitProduct();

            Combo_Arc_ProdId.SelectedIndex = 0;


            Grid_Par_Port.Visibility = Visibility.Collapsed;
            Grid_Par_Term.Visibility = Visibility.Visible;
            Grid_Archive_Settings.Visibility = Visibility.Collapsed;
            //  float[] kv_par.doze = new float[10];
            tmOutScreen.Start();



        }

        public MainPage()
        {
            UInt16 i = 0;

            this.InitializeComponent();
            BrushOn = new SolidColorBrush();
            BrushParamOn = new SolidColorBrush();
            BrushOn.Color = Windows.UI.Color.FromArgb(255, 54, 58, 107);
            BrushParamOn.Color = Windows.UI.Color.FromArgb(255, 0x60, 0x6F, 0xA2);
            BrushOff = new SolidColorBrush();
            BrushOff.Color = Windows.UI.Color.FromArgb(255, 0x36, 0x6B, 0x53);
            Grid_Left_Monitor.Background = BrushOn;   //.SolidColorBrush("#FF7E7C91");
            Grid_Right.Background = BrushOff;
            Grid_Main.Background = BrushOff;
            Grid_Center.Background = BrushOn;
            Grid_Param_Levels.Background = BrushParamOn;

            for (i=1; i<=20; i++)
            { 
                Combo_Post_ID.Items.Add("ПРОДУКТ "+i);
                Combo_Arc_ProdId.Items.Add("ПРОДУКТ "+i);
            }

            Combo_F1.Items.Add("0");
            Combo_F1.Items.Add("4");
            Combo_F1.Items.Add("8");
            Combo_F1.Items.Add("16");
            Combo_F1.Items.Add("32");
            Combo_F2.Items.Add("0");
            Combo_F2.Items.Add("4");
            Combo_F2.Items.Add("8");
            Combo_F2.Items.Add("16");
            Combo_F2.Items.Add("32");
            Combo_Baud.Items.Add("4800");
            Combo_Baud.Items.Add("9600");
            Combo_Baud.Items.Add("19200");
            Combo_Baud.Items.Add("57600");


            Combo_Prg_Ver_1.Items.Add("Lite v2");
            Combo_Prg_Ver_1.Items.Add("v11.02");
            Combo_Prg_Ver_2.Items.Add("Lite v2");
            Combo_Prg_Ver_2.Items.Add("v11.02");
            Combo_Prg_Ver_3.Items.Add("Lite v2");
            Combo_Prg_Ver_3.Items.Add("v11.02");
            Combo_Prg_Ver_4.Items.Add("Lite v2");
            Combo_Prg_Ver_4.Items.Add("v11.02");
            Combo_Prg_Ver_5.Items.Add("Lite v2");
            Combo_Prg_Ver_5.Items.Add("v11.02");
            Combo_Prg_Ver_6.Items.Add("Lite v2");
            Combo_Prg_Ver_6.Items.Add("v11.02");
            Combo_Prg_Ver_7.Items.Add("Lite v2");
            Combo_Prg_Ver_7.Items.Add("v11.02");
            Combo_Prg_Ver_8.Items.Add("Lite v2");
            Combo_Prg_Ver_8.Items.Add("v11.02");
            Combo_Prg_Ver_9.Items.Add("Lite v2");
            Combo_Prg_Ver_9.Items.Add("v11.02");
            Combo_Prg_Ver_10.Items.Add("Lite v2");
            Combo_Prg_Ver_10.Items.Add("v11.02");

            Combo_Out_Mode.Items.Add("0 - (0 В)");
            Combo_Out_Mode.Items.Add("1 - (+24В)");
            Combo_Autozero.Items.Add("Выкл.");
            Combo_Autozero.Items.Add("Вкл.");
            Combo_Polar.Items.Add("Униполярный");
            Combo_Polar.Items.Add("Биполярный");


            Combo_Discr.Items.Add("1");
            Combo_Discr.Items.Add("2");
            Combo_Discr.Items.Add("5");
            Combo_Discr.Items.Add("10");
            Combo_Discr.Items.Add("20");
            Combo_Discr.Items.Add("50");
            Combo_Discr.Items.Add("100");
            Combo_Zpt.Items.Add("0");
            Combo_Zpt.Items.Add("1");
            Combo_Zpt.Items.Add("2");
            Combo_Hz.Items.Add("125");
            Combo_Hz.Items.Add("62.6");
            Combo_Hz.Items.Add("50");
            Combo_Hz.Items.Add("39.2");
            Combo_Hz.Items.Add("33.3");
            Combo_Hz.Items.Add("19.6");
            Combo_Hz.Items.Add("16.7");
            Combo_Hz.Items.Add("16.7*");
            Combo_Hz.Items.Add("12.5");
            Combo_Hz.Items.Add("10");
            Combo_Hz.Items.Add("8.33");
            Combo_Hz.Items.Add("6.25");
            Combo_Hz.Items.Add("4.17");
            Combo_Mv.Items.Add("2500");
            Combo_Mv.Items.Add("1250");
            Combo_Mv.Items.Add("625");
            Combo_Mv.Items.Add("312.5");
            Combo_Mv.Items.Add("156.5");
            Combo_Mv.Items.Add("78.125");
            Combo_Mv.Items.Add("39.06");
            Combo_Mv.Items.Add("19.53");



            Combo_Lite_Zpt.Items.Add("0");
            Combo_Lite_Zpt.Items.Add("1");
            Combo_Lite_Zpt.Items.Add("2");
            Combo_Lite_Polar.Items.Add("Униполярный");
            Combo_Lite_Polar.Items.Add("Биполярный");

            Combo_Lite_Discr.Items.Add("1");
            Combo_Lite_Discr.Items.Add("2");
            Combo_Lite_Discr.Items.Add("5");
            Combo_Lite_Discr.Items.Add("10");
            Combo_Lite_Discr.Items.Add("20");
            Combo_Lite_Discr.Items.Add("50");
            Combo_Lite_Discr.Items.Add("100");

            Combo_Lite_Hz.Items.Add("500");
            Combo_Lite_Hz.Items.Add("250");
            Combo_Lite_Hz.Items.Add("125");
            Combo_Lite_Hz.Items.Add("62.6");
            Combo_Lite_Hz.Items.Add("50");
            Combo_Lite_Hz.Items.Add("39.2");
            Combo_Lite_Hz.Items.Add("33.3");
            Combo_Lite_Hz.Items.Add("19.6");
            Combo_Lite_Hz.Items.Add("16.7");
            Combo_Lite_Hz.Items.Add("16.7*");
            Combo_Lite_Hz.Items.Add("12.5");
            Combo_Lite_Hz.Items.Add("10");
            Combo_Lite_Hz.Items.Add("8.33");
            Combo_Lite_Hz.Items.Add("6.25");
            Combo_Lite_Hz.Items.Add("4.17");
            Combo_Lite_Mv.Items.Add("2500");
            Combo_Lite_Mv.Items.Add("1250");
            Combo_Lite_Mv.Items.Add("625");
            Combo_Lite_Mv.Items.Add("312.5");
            Combo_Lite_Mv.Items.Add("156.5");
            Combo_Lite_Mv.Items.Add("78.125");
            Combo_Lite_Mv.Items.Add("39.06");
            Combo_Lite_Mv.Items.Add("19.53");

            Combo_Lite_FStab.Items.Add("0");
            Combo_Lite_FStab.Items.Add("4");
            Combo_Lite_FStab.Items.Add("8");
            Combo_Lite_FStab.Items.Add("16");
            Combo_Lite_FStab.Items.Add("32");


            Grid_Prog_Settings.Visibility = Visibility.Collapsed;
            Grid_Post_Main.Visibility = Visibility.Visible;
            Grid_Post_Settings.Visibility = Visibility.Collapsed;
            Grid_Post_Bottom.Visibility = Visibility.Visible;
            Grid_Post_Top.Visibility = Visibility.Visible;
            Grid_Par_Smena.Visibility = Visibility.Collapsed;

            Combo_Lite_F1.Items.Add("0");
            Combo_Lite_F1.Items.Add("4");
            Combo_Lite_F1.Items.Add("8");
            Combo_Lite_F1.Items.Add("16");
            Combo_Lite_F1.Items.Add("32");
            Combo_Lite_F2.Items.Add("0");
            Combo_Lite_F2.Items.Add("4");
            Combo_Lite_F2.Items.Add("8");
            Combo_Lite_F2.Items.Add("16");
            Combo_Lite_F2.Items.Add("32");

            Combo_Lite_Mode.Items.Add("Выкл.");
            Combo_Lite_Mode.Items.Add("По последнему");
            Combo_Lite_Mode.Items.Add("По наибольшему");

            Combo_Lite_Direction.Items.Add("Младшим");
            Combo_Lite_Direction.Items.Add("Старшим");

            Combo_Lite_Baud.Items.Add("4800");
            Combo_Lite_Baud.Items.Add("9600");
            Combo_Lite_Baud.Items.Add("19200");
            Combo_Lite_Baud.Items.Add("57600");
            //  Grid_Right_Time.Visibility = Visibility.Visible;
            //  Grid_Right_Calc.Visibility = Visibility.Collapsed;
            //  Grid_Right.Visibility = Visibility.Visible;
            Combo_Lite_PointNum.Items.Add("0");
            Combo_Lite_PointNum.Items.Add("1");
            Combo_Lite_PointNum.Items.Add("2");
            Combo_Lite_PointNum.Items.Add("3");
            Combo_Lite_PointNum.Items.Add("4");
            Combo_Lite_PointNum.Items.Add("5");
            Combo_Lite_PointNum.Items.Add("6");
            Combo_Lite_PointNum.Items.Add("7");
            Combo_Lite_PointNum.Items.Add("8");
            Combo_Lite_PointNum.Items.Add("9");
            Combo_Lite_PointNum.Items.Add("10");

            kv_par = new ASU_KV_001.KV001();
            prg_par = new ASU_KV_001.Program_Par();
            serial_port = new ASU_KV_001.ModBus_Class();
            products= new ASU_KV_001.Products();

            db_archive = new Common.Archive();

            ///            tmOutScreen = new DispatcherTimer();
            //tmOutScreen.Interval = TimeSpan.FromMilliseconds(1000);
            //tmOutScreen.Tick += Timer_Tick;
            //tmOutScreen.Stop();
            

            tmComReq = new DispatcherTimer();
            tmComReq.Interval = TimeSpan.FromMilliseconds(80);
            tmComReq.Tick += ComReq_Tick;
            tmComReq.Stop();


            tmOutScreen = new DispatcherTimer();
            tmOutScreen.Interval = TimeSpan.FromMilliseconds(200);
            tmOutScreen.Tick += Timer_Tick;
            tmOutScreen.Stop();
        }

        private void Timer_Tick(object sender, object e)
        {
            double dw;
            string s,s1,s3;


            int t_count;

            DateTime ThToday = DateTime.Now;
            string ThData = ThToday.ToString("dd.MM.yyyy   HH:mm:ss");
            this.tbCurTime.Text = ThData;


            byte sm = prg_par.SmenaNow((byte)ThToday.Hour, (byte)ThToday.Minute, (byte)ThToday.Second);
            Text_CurSmena.Text = "СМЕНА: " + sm;
            for (t_count = 0; t_count <= 9; t_count++)
            {
                if (prg_par.enable[t_count])
                {
                    dw = 0;
                    if (prg_par.ComMode == 0)
                    {
                        // Демо режим
                        Random r = new Random();
                        dw = r.Next(1, 10);
                        dw = dw - 5;
                        dw /= 100;
                        dw += kv_par.weight[t_count];
                    }
                    else
                    {
                        dw += kv_par.weight[t_count];
                    }

                    dw = Math.Round(dw, 2);
                    s = Convert.ToString(dw);
                    s= s.Replace(",", ".");
                    switch (kv_par.state[t_count])
                    {
                        case 0:
                        case 1: s1 = "МЕНЮ"; break;
                        case 2: s1 = "ОЖИДАНИЕ";  break;
                        case 3: 
                        case 4: 
                        case 5:
                            s1 = "КОМПОНЕНТ 1";
                            if (kv_par.arc_state[t_count] == 0) kv_par.arc_state[t_count] = 1; // обнаружено начало дозирования - фиксируем
                                break;
                        case 6: 
                        case 7: 
                        case 8:
                            s1 = "КОМПОНЕНТ 2";
                            if (kv_par.arc_state[t_count] == 0) kv_par.arc_state[t_count] = 1; // обнаружено начало дозирования - фиксируем
                            break;
                        case 9: 
                        case 10: 
                        case 11:
                            s1 = "КОМПОНЕНТ 3";
                            if (kv_par.arc_state[t_count] == 0) kv_par.arc_state[t_count] = 1; // обнаружено начало дозирования - фиксируем
                            break;
                        case 12:
                            s1 = "ВЕС УШЕЛ";
                            if (kv_par.arc_state[t_count] == 1) kv_par.arc_state[t_count] = 2; // обнаружено окончание дозирования - можно считывать результат
                            break;
                        case 13:
                            s1 = "ВЫГРУЗКА";
                            if (kv_par.arc_state[t_count] == 0) kv_par.arc_state[t_count] = 1; // обнаружено начало дозирования - фиксируем
                            break;
                        case 14:
                            s1 = "ВЫГРУЗКА";
                            if (kv_par.arc_state[t_count] == 1) kv_par.arc_state[t_count] = 2; // обнаружено окончание дозирования - можно считывать результат
                            break;
                        default: s1 = "ОШИБКА"; break;
                    }
                    dw = kv_par.sum_doze[t_count];
                    dw = Math.Round(dw, 2);
                    s3 = Convert.ToString(dw);
                    s3 = s3.Replace(",", ".");


                    switch (t_count)
                    {
                        case 0: tb_Weight_Post_1.Text = "ВЕС: " + s; tb_State_Post_1.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_1.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 1: tb_Weight_Post_2.Text = "ВЕС: " + s; tb_State_Post_2.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_2.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 2: tb_Weight_Post_3.Text = "ВЕС: " + s; tb_State_Post_3.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_3.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 3: tb_Weight_Post_4.Text = "ВЕС: " + s; tb_State_Post_4.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_4.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 4: tb_Weight_Post_5.Text = "ВЕС: " + s; tb_State_Post_5.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_5.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 5: tb_Weight_Post_6.Text = "ВЕС: " + s; tb_State_Post_6.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_6.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 6: tb_Weight_Post_7.Text = "ВЕС: " + s; tb_State_Post_7.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_7.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 7: tb_Weight_Post_8.Text = "ВЕС: " + s; tb_State_Post_8.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_8.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 8: tb_Weight_Post_9.Text = "ВЕС: " + s; tb_State_Post_9.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_9.Text = "ОТГРУЖЕНО: " + s3; break;
                        case 9: tb_Weight_Post_10.Text = "ВЕС: " + s; tb_State_Post_10.Text = "СОСТОЯНИЕ: " + s1; tb_OutWeight_Post_10.Text = "ОТГРУЖЕНО: " + s3; break;

                    }
                    if (term_now == t_count)
                    {
                        Post_Weight.Text = s;
                        Post_State.Text = s1;

                        dw = Math.Round(kv_par.doze[term_now, 0], 2);
                        s = Convert.ToString(dw);
                        s = s.Replace(",", ".");
                        Text_Doza_1.Text = "ДОЗА: "+s;
                        dw = Math.Round(kv_par.doze[term_now, 1], 2);
                        s = Convert.ToString(dw);
                        s = s.Replace(",", ".");
                        Text_Doza_2.Text = "ДОЗА: " + s;
                        dw = Math.Round(kv_par.doze[term_now, 2], 2);
                        s = Convert.ToString(dw);
                        s = s.Replace(",", ".");
                        Text_Doza_3.Text = "ДОЗА: " + s;
                        dw = Math.Round(kv_par.last_doze[term_now, 0], 2);
                        s = Convert.ToString(dw);
                        s = s.Replace(",", ".");
                        Text_Last_1.Text = "ПОСЛЕДНИЙ: " + s;
                        dw = Math.Round(kv_par.last_doze[term_now, 1], 2);
                        s = Convert.ToString(dw);
                        s = s.Replace(",", ".");
                        Text_Last_2.Text = "ПОСЛЕДНИЙ: " + s;
                        dw = Math.Round(kv_par.last_doze[term_now, 2], 2);
                        s = Convert.ToString(dw);
                        s = s.Replace(",", ".");
                        Text_Last_3.Text = "ПОСЛЕДНИЙ: " + s;

                    }
                    //Работаем с правым архивом
                    if (kv_par.arc_state[t_count] == 3)
                    {
                        Text_Arc_Center_Weight_6.Text = Text_Arc_Center_Weight_5.Text;
                        Text_Arc_Center_Weight_5.Text = Text_Arc_Center_Weight_4.Text;
                        Text_Arc_Center_Weight_4.Text = Text_Arc_Center_Weight_3.Text;
                        Text_Arc_Center_Weight_3.Text = Text_Arc_Center_Weight_2.Text;
                        Text_Arc_Center_Weight_2.Text = Text_Arc_Center_Weight_1.Text;
                        Text_Arc_Center_Weight_1.Text = "" + (kv_par.last_doze[t_count, 0] + kv_par.last_doze[t_count, 1] + kv_par.last_doze[t_count, 2]);
                        ThData = ThToday.ToString("HH:mm");
                        Text_Arc_Center_Time_6.Text = Text_Arc_Center_Time_5.Text;
                        Text_Arc_Center_Time_5.Text = Text_Arc_Center_Time_4.Text;
                        Text_Arc_Center_Time_4.Text = Text_Arc_Center_Time_3.Text;
                        Text_Arc_Center_Time_3.Text = Text_Arc_Center_Time_2.Text;
                        Text_Arc_Center_Time_2.Text = Text_Arc_Center_Time_1.Text;
                        Text_Arc_Center_Time_1.Text = ThData;
                        Text_Arc_Center_Term_6.Text = Text_Arc_Center_Term_5.Text;
                        Text_Arc_Center_Term_5.Text = Text_Arc_Center_Term_4.Text;
                        Text_Arc_Center_Term_4.Text = Text_Arc_Center_Term_3.Text;
                        Text_Arc_Center_Term_3.Text = Text_Arc_Center_Term_2.Text;
                        Text_Arc_Center_Term_2.Text = Text_Arc_Center_Term_1.Text;
                        Text_Arc_Center_Term_1.Text = ""+(t_count+1);

                        Text_Arc_Center_Id_6.Text = Text_Arc_Center_Id_5.Text;
                        Text_Arc_Center_Id_5.Text = Text_Arc_Center_Id_4.Text;
                        Text_Arc_Center_Id_4.Text = Text_Arc_Center_Id_3.Text;
                        Text_Arc_Center_Id_3.Text = Text_Arc_Center_Id_2.Text;
                        Text_Arc_Center_Id_2.Text = Text_Arc_Center_Id_1.Text;
                        Text_Arc_Center_Id_1.Text =""+Combo_Post_ID.Items[prg_par.selected_id[t_count]];
                        kv_par.arc_state[t_count] = 0;
                        if (term_now == t_count)
                        {
                            Text_Arc_Top_Weight_5.Text = Text_Arc_Top_Weight_4.Text;
                            Text_Arc_Top_Weight_4.Text = Text_Arc_Top_Weight_3.Text;
                            Text_Arc_Top_Weight_3.Text = Text_Arc_Top_Weight_2.Text;
                            Text_Arc_Top_Weight_2.Text = Text_Arc_Top_Weight_1.Text;
                            Text_Arc_Top_Weight_1.Text = "" + (kv_par.last_doze[t_count, 0] + kv_par.last_doze[t_count, 1] + kv_par.last_doze[t_count, 2]);
                            Text_Arc_Top_Time_5.Text = Text_Arc_Top_Time_4.Text;
                            Text_Arc_Top_Time_4.Text = Text_Arc_Top_Time_3.Text;
                            Text_Arc_Top_Time_3.Text = Text_Arc_Top_Time_2.Text;
                            Text_Arc_Top_Time_2.Text = Text_Arc_Top_Time_1.Text;
                            Text_Arc_Top_Time_1.Text = ThData;
                            Text_Arc_Top_Id_5.Text = Text_Arc_Top_Id_5.Text;
                            Text_Arc_Top_Id_4.Text = Text_Arc_Top_Id_4.Text;
                            Text_Arc_Top_Id_3.Text = Text_Arc_Top_Id_3.Text;
                            Text_Arc_Top_Id_2.Text = Text_Arc_Top_Id_2.Text;
                            Text_Arc_Top_Id_1.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[t_count]]; 
                        }
                    }

                }
                
            }

                /*           DateTime ThToday = DateTime.Now;
                           string ThData = ThToday.ToString("dd.MM.yyyy HH:mm:ss");
                           this.tbCurTime.Text = ThData;

                           sm = param.SmenaNow((byte)ThToday.Hour, (byte)ThToday.Minute, (byte)ThToday.Second);
                           tbSmena.Text = "СМЕНА №" + sm;
                           dw = 0;
                           if (param.ComMode == 0)
                           {
                               Random r = new Random();
                               dw = r.Next(1, 10);
                               dw = dw - 5;
                               dw /= 100;
                               if ((KV.State == 2) && KV.Archive_State == 0) KV.State = 3;
                               if (KV.State == 2) KV.Archive_State = 0;
                               if ((KV.State == 3) || (KV.State == 5))
                               {
                                   KV.Archive_State = 1;
                                   if (KV.State == 3) KV.Weight += KV.Doza / 10;
                                   if (KV.State == 5) KV.Weight += KV.Doza / 50;
                                   if ((KV.State == 3) && (KV.Weight >= (KV.Doza - KV.Dw)))
                                   {
                                       BrushOut3.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                                       KV.State = 5;
                                   }
                                   if ((KV.State == 5) && (KV.Weight >= (KV.Doza - KV.Dwi)))
                                   {
                                       KV.State = 4;
                                       tmr = 0;
                                       BrushOut2.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                                   }

                               }

                               if (KV.State == 6)
                               {
                                   if (KV.Archive_State == 1) KV.Archive_State = 2;
                                   KV.Weight -= KV.Doza / 20;
                                   if (KV.Weight <= 1)
                                   {
                                       BrushOut4.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                                       BrushOut6.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                                       KV.Weight = 0; KV.State = 2;
                                   }
                               }

                               dw += KV.Weight;

                               tmr++;
                               if ((KV.State == 4) && (tmr >= 5)) { serial_port.doza_last = (float)dw; serial_port.last_flag = 1; KV.Archive_State = 1; BrushOut6.ImageSource = GetImageFromPath(this, @"Assets/out_on.jpg"); tmr = 0; KV.State = 9; }
                               if ((KV.State == 9) && (tmr >= 5)) { BrushOut4.ImageSource = GetImageFromPath(this, @"Assets/out_on.jpg"); tmr = 0; KV.State = 6; }
                           }

                           else
                           {

                               if ((KV.outputs & 0x01) == 0x01)
                                   BrushOut1.ImageSource = GetImageFromPath(this, @"Assets/out_on.jpg");
                               else
                                   BrushOut1.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                               if ((KV.outputs & 0x02) == 0x02)
                                   BrushOut2.ImageSource = GetImageFromPath(this, @"Assets/out_on.jpg");
                               else
                                   BrushOut2.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                               if ((KV.outputs & 0x04) == 0x04)
                                   BrushOut3.ImageSource = GetImageFromPath(this, @"Assets/out_on.jpg");
                               else
                                   BrushOut3.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                               if ((KV.outputs & 0x08) == 0x08)
                                   BrushOut4.ImageSource = GetImageFromPath(this, @"Assets/out_on.jpg");
                               else
                                   BrushOut4.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                               if ((KV.outputs & 0x10) == 0x10)
                                   BrushOut5.ImageSource = GetImageFromPath(this, @"Assets/out_on.jpg");
                               else
                                   BrushOut5.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");
                               if ((KV.outputs & 0x20) == 0x20)
                                   BrushOut6.ImageSource = GetImageFromPath(this, @"Assets/out_on.jpg");
                               else
                                   BrushOut6.ImageSource = GetImageFromPath(this, @"Assets/out_off.jpg");

                               dw = KV.sum_last;
                               dw = Math.Round(dw, 2);
                               s = Convert.ToString(dw);
                               tbSum_Last.Text = s.Replace(",", ".");

                               dw = KV.doza_last;
                               dw = Math.Round(dw, 2);
                               s = Convert.ToString(dw);
                               tbDoza_Last.Text = s.Replace(",", ".");
                               tbCount_Last.Text = Convert.ToString(KV.count_last);
                               dw = KV.Weight;
                               if ((KV.State == 3) || (KV.State == 4) || (KV.State == 5)) { KV.Archive_State = 1; }
                               if (((KV.State == 9) || (KV.State == 6) || (KV.State == 2)) && (KV.Archive_State == 1)) { serial_port.last_flag = 0; KV.Archive_State = 2; }
                           }
                           dw = Math.Round(dw, 2);
                           s = Convert.ToString(dw);
                           tbKVWeight.Text = s.Replace(",", ".");
                           tbWeight.Text = tbKVWeight.Text;
                           dw = KV.Doza;
                           dw = Math.Round(dw, 2);
                           s = Convert.ToString(dw);
                           tbDoza.Text = s.Replace(",", ".");
                           if ((KV.Archive_State == 2) && (serial_port.last_flag == 1))
                           {
                               serial_port.doza_last = (float)Math.Round(serial_port.doza_last, 2);
                               Archive.Sql_Add(sm, ThToday, 1, serial_port.doza_last, (byte)param.SmenaHour, (byte)param.SmenaMinute);
                               KV.Archive_State = 0;
                               tbLastRec5.Text = "" + Archive.Rec_Last[4];
                               tbLastRec4.Text = "" + Archive.Rec_Last[3];
                               tbLastRec3.Text = "" + Archive.Rec_Last[2];
                               tbLastRec2.Text = "" + Archive.Rec_Last[1];
                               tbLastRec1.Text = "" + Archive.Rec_Last[0];

                               tbLastDoza5.Text = "" + Archive.Doza_Last[4];
                               tbLastDoza4.Text = "" + Archive.Doza_Last[3];
                               tbLastDoza3.Text = "" + Archive.Doza_Last[2];
                               tbLastDoza2.Text = "" + Archive.Doza_Last[1];
                               tbLastDoza1.Text = "" + Archive.Doza_Last[0];

                               tbLastTime5.Text = "" + Archive.Time_Last[4];
                               tbLastTime4.Text = "" + Archive.Time_Last[3];
                               tbLastTime3.Text = "" + Archive.Time_Last[2];
                               tbLastTime2.Text = "" + Archive.Time_Last[1];
                               tbLastTime1.Text = "" + Archive.Time_Last[0];

                           }
                           switch (KV.State)
                           {
                               case 0: tbKVState.Text = "МЕНЮ"; break;
                               case 1: tbKVState.Text = "КАЛИБРОВКА"; break;
                               case 2: tbKVState.Text = "ОЖИДАНИЕ"; break;
                               case 3: tbKVState.Text = "ДОЗИРОВАНИЕ ГРУБО"; break;
                               case 4: tbKVState.Text = "УСПОКОЕНИЕ СИСТЕМЫ"; break;
                               case 5: tbKVState.Text = "ДОЗИРОВАНИЕ ТОЧНО"; break;
                               case 6: tbKVState.Text = "ВЫГРУЗКА"; break;
                               case 7: tbKVState.Text = "ПРЕВЫШЕН НПВ"; break;
                               case 9: tbKVState.Text = "ОЖИДАНИЕ ВЫГРУЗКИ"; break;

                           }*/
            }
        private async void ComReq_Tick(object sender, object e)
        {

            
            if (reg_status_visible == 0) tbParStatus.Visibility = Visibility.Collapsed;
            if (reg_status_visible == 1) tbParStatus.Visibility = Visibility.Visible;
            if ((reg_status_visible >= 20) && (reg_status_visible <= 24)) { tbParStatus.Visibility = Visibility.Collapsed; reg_status_visible++; }
            if ((reg_status_visible >= 30) && (reg_status_visible <= 34)) { tbParStatus.Visibility = Visibility.Collapsed; reg_status_visible++; }
            if ((reg_status_visible >= 40) && (reg_status_visible <= 44)) { tbParStatus.Visibility = Visibility.Collapsed; reg_status_visible++; }
            if ((reg_status_visible >= 25) && (reg_status_visible <= 29)) { tbParStatus.Visibility = Visibility.Visible; reg_status_visible++; }
            if ((reg_status_visible >= 35) && (reg_status_visible <= 39)) { tbParStatus.Visibility = Visibility.Visible; reg_status_visible++; }
            if ((reg_status_visible >= 45) && (reg_status_visible <= 49)) { tbParStatus.Visibility = Visibility.Visible; reg_status_visible++; }
            if ((reg_status_visible >= 50)) { tbParStatus.Visibility = Visibility.Collapsed; reg_status_visible = 0; }
            
            if (serial_port.wait_answer == 2)
            {
                next_adress = -1;
                switch (serial_port.rd_mode)
                {
                    case 1:
                        kv_par.weight[term_num_req] = serial_port.weight;
                        kv_par.state[term_num_req]=(byte)serial_port.q_state;
                        // (byte)prg_par.num[term_num_req];
                        /*   tmp = (short)(serial_port.q_state >> 8);
                           KV.State = (byte)(serial_port.q_state - tmp * 0x100);
                           tmp = (short)(serial_port.in_outs >> 8);
                           KV.inputs = (byte)tmp;
                           KV.outputs = (byte)(serial_port.in_outs - tmp * 0x100);*/
                        serial_port.rd_mode = 1;
                        if ((num_reg_read == 10)&&(Grid_Post_Main.Visibility == Visibility.Visible)) { serial_port.rd_mode = 2; next_adress = (byte)prg_par.num[term_now]; }
                        if (num_reg_read == 20) {serial_port.rd_mode = 8; next_adress = (byte)prg_par.num[arc_read_term]; }
                        for (int i = 0; i < 10; i++)
                        {
                            if (kv_par.arc_state[i] == 2) { serial_port.rd_mode = 8; next_adress = (byte)prg_par.num[i]; }
                        }

                            break;
                    case 2:
                        kv_par.doze[term_now, 0] = serial_port.doze[0];

                        serial_port.rd_mode = 3;
                        break;
                    case 3:
                        kv_par.doze[term_now, 1] = serial_port.doze[1];

                        serial_port.rd_mode = 4;
                        break;
                    case 4:
                        kv_par.doze[term_now, 2] = serial_port.doze[2];
                        serial_port.rd_mode = 1;
                        break;
                    case 8:
                        kv_par.last_doze[arc_read_term, 0] = serial_port.doza_last[0];
                        kv_par.last_doze[arc_read_term, 1] = serial_port.doza_last[1];
                        kv_par.last_doze[arc_read_term, 2] = serial_port.doza_last[2];
                        kv_par.count_doze[arc_read_term] = serial_port.count_last;
                        kv_par.sum_doze[arc_read_term] = serial_port.sum_last;
                        if (kv_par.arc_state[arc_read_term] == 2) kv_par.arc_state[arc_read_term] = 3; // считали результат - надо в архив
                            serial_port.rd_mode = 1;
                        arc_read_term++; if (arc_read_term > 9) arc_read_term = 0;
                        for (int i = 0; i <= 9; i++)
                        {
                            if (!prg_par.enable[arc_read_term])
                            {
                                arc_read_term++; if (arc_read_term > 9) arc_read_term = 0;
                            }
                            else i = 10;
                        }

                        break;
                    case 20:
                        par_flag = true;
                        reg_status_visible = 1;
                        tbParStatus.Text = "Идет считывание параметров вкладки LEVELS: 16%";
                        kv_par.doze[term_now, 0] = serial_port.reg_fl; Doza11.Text = "КОМПОНЕНТ 1: " + kv_par.doze[term_now, 0]; serial_port.rd_mode++;
                        break;
                    case 21:
                        tbParStatus.Text = "Идет считывание параметров вкладки LEVELS: 33%";
                        kv_par.doze[term_now, 3] = serial_port.reg_fl; Doza21.Text = "КОМПОНЕНТ 1: " + kv_par.doze[term_now, 3]; serial_port.rd_mode++;
                        break;
                    case 22:
                        tbParStatus.Text = "Идет считывание параметров вкладки LEVELS: 48%";
                        kv_par.doze[term_now, 1] = serial_port.reg_fl; Doza12.Text = "КОМПОНЕНТ 2: " + kv_par.doze[term_now, 1]; serial_port.rd_mode++;
                        break;
                    case 23:
                        tbParStatus.Text = "Идет считывание параметров вкладки LEVELS: 67%";
                        kv_par.doze[term_now, 4] = serial_port.reg_fl; Doza22.Text = "КОМПОНЕНТ 2: " + kv_par.doze[term_now, 4]; serial_port.rd_mode++;
                        break;
                    case 24:
                        tbParStatus.Text = "Идет считывание параметров вкладки LEVELS: 84%";
                        kv_par.doze[term_now, 2] = serial_port.reg_fl; Doza13.Text = "КОМПОНЕНТ 3: " + kv_par.doze[term_now, 2]; serial_port.rd_mode++;
                        break;
                    case 25:
                        tbParStatus.Text = "Идет считывание параметров вкладки LEVELS: 100%";
                        kv_par.doze[term_now, 5] = serial_port.reg_fl; Doza23.Text = "КОМПОНЕНТ 3:" + kv_par.doze[term_now, 5]; serial_port.rd_mode=1;
                        reg_status_visible = 20;
                        break;
                    case 40:
                        tbParStatus.Text = "Идет запись параметров вкладки LEVELS: 16%";
                        reg_status_visible = 1;
                        write_reg_wait = 41;
                        serial_port.write_reg_fl = (float)kv_par.doze[term_now, 3];
                        break;
                    case 41:
                        tbParStatus.Text = "Идет запись параметров вкладки LEVELS: 33%";
                        write_reg_wait = 42;
                        serial_port.write_reg_fl = (float)kv_par.doze[term_now, 1];
                        break;
                    case 42:
                        tbParStatus.Text = "Идет запись параметров вкладки LEVELS: 48%";
                        write_reg_wait = 43;
                        serial_port.write_reg_fl = (float)kv_par.doze[term_now, 4];
                        break;
                    case 43:
                        tbParStatus.Text = "Идет запись параметров вкладки LEVELS: 67%";
                        write_reg_wait = 44;
                        serial_port.write_reg_fl = (float)kv_par.doze[term_now, 2];
                        break;
                    case 44:
                        tbParStatus.Text = "Идет запись параметров вкладки LEVELS: 84%";
                        write_reg_wait = 45;
                        serial_port.write_reg_fl = (float)kv_par.doze[term_now, 5];
                        break;
                    case 45:
                        tbParStatus.Text = "Идет запись параметров вкладки LEVELS: 100%";
                        serial_port.rd_mode = 20;
                        reg_status_visible = 20;

                        break;
                    case 60:
                        par_flag = true;
                        reg_status_visible = 1;
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 8%";
                        kv_par.dw[term_now, 0] = serial_port.reg_fl; Dw1.Text = "НЕДОВЕС ГРУБО: " + kv_par.dw[term_now, 0]; serial_port.rd_mode++;
                        break;
                    case 61:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 17%";
                        kv_par.dw[term_now, 1] = serial_port.reg_fl; Dw2.Text = "НЕДОВЕС ГРУБО: " + kv_par.dw[term_now, 1]; serial_port.rd_mode++;
                        break;
                    case 62:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 25%";
                        kv_par.dw[term_now, 2] = serial_port.reg_fl; Dw3.Text = "НЕДОВЕС ГРУБО: " + kv_par.dw[term_now, 2]; serial_port.rd_mode++;
                        break;
                    case 63:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 33%";
                        kv_par.dwi[term_now, 0] = serial_port.reg_fl; Dwi1.Text = "НЕДОВЕС ТОЧНО: " + kv_par.dwi[term_now, 0]; serial_port.rd_mode++;
                        break;
                    case 64:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 42%";
                        kv_par.dwi[term_now, 1] = serial_port.reg_fl; Dwi2.Text = "НЕДОВЕС ТОЧНО: " + kv_par.dwi[term_now, 1]; serial_port.rd_mode++;
                        break;
                    case 65:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 50%";
                        kv_par.dwi[term_now, 2] = serial_port.reg_fl; Dwi3.Text = "НЕДОВЕС ТОЧНО: " + kv_par.dwi[term_now, 2]; serial_port.rd_mode++;
                        break;
                    case 66:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 58%";
                        kv_par.pause[term_now, 0] = serial_port.reg_fl; Pause1.Text = "ПАУЗА: " + kv_par.pause[term_now, 0]; serial_port.rd_mode++;
                        break;
                    case 67:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 67%";
                        kv_par.pause[term_now, 1] = serial_port.reg_fl; Pause2.Text = "ПАУЗА: " + kv_par.pause[term_now, 1]; serial_port.rd_mode++;
                        break;
                    case 68:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 75%";
                        kv_par.pause[term_now, 2] = serial_port.reg_fl; Pause3.Text = "ПАУЗА: " + kv_par.pause[term_now, 2]; serial_port.rd_mode++;
                        break;
                    case 69:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 83%";
                        kv_par.impulse[term_now, 0] = serial_port.reg_fl; Impulse1.Text = "ИМПУЛЬС: " + kv_par.impulse[term_now, 0]; serial_port.rd_mode++;
                        break;
                    case 70:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 92%";
                        kv_par.impulse[term_now, 1] = serial_port.reg_fl; Impulse2.Text = "ИМПУЛЬС: " + kv_par.impulse[term_now, 1]; serial_port.rd_mode++;
                        break;
                    case 71:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 100%";
                        kv_par.impulse[term_now, 2] = serial_port.reg_fl; Impulse3.Text = "ИМПУЛЬС: " + kv_par.impulse[term_now, 2]; serial_port.rd_mode = 1;
                        reg_status_visible = 20;
                        break;
                    case 80:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 8%";
                        write_reg_wait = 81;
                        serial_port.write_reg_fl = (float)kv_par.dw[term_now, 1];
                        break;
                    case 81:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 17%";
                        write_reg_wait = 82;
                        serial_port.write_reg_fl = (float)kv_par.dw[term_now, 2];
                        break;
                    case 82:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 25%";
                        write_reg_wait = 83;
                        serial_port.write_reg_fl = (float)kv_par.dwi[term_now, 0];
                        break;
                    case 83:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 33%";
                        write_reg_wait = 84;
                        serial_port.write_reg_fl = (float)kv_par.dwi[term_now, 1];
                        break;
                    case 84:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 42%";
                        write_reg_wait = 85;
                        serial_port.write_reg_fl = (float)kv_par.dwi[term_now, 2];
                        break;
                    case 85:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 50%";
                        write_reg_wait = 86;
                        serial_port.write_reg_fl = (float)kv_par.pause[term_now, 0];
                        break;
                    case 86:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 58%";
                        write_reg_wait = 87;
                        serial_port.write_reg_fl = (float)kv_par.pause[term_now, 1];
                        break;
                    case 87:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 67%";
                        write_reg_wait = 88;
                        serial_port.write_reg_fl = (float)kv_par.pause[term_now, 2];
                        break;
                    case 88:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 75%";
                        write_reg_wait = 89; 
                        serial_port.write_reg_fl = (float)kv_par.impulse[term_now, 0];
                        break;
                    case 89:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 83%";
                        write_reg_wait = 90;
                        serial_port.write_reg_fl = (float)kv_par.impulse[term_now, 1];
                        break;
                    case 90:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 92%";
                        write_reg_wait = 91;
                        serial_port.write_reg_fl = (float)kv_par.impulse[term_now, 2];
                        break;
                    case 91:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 100%";
                        serial_port.rd_mode = 60;
                        reg_status_visible = 20;
                        break;

                    case 100:
                        par_flag = true;
                        reg_status_visible = 1;
                        tbParStatus.Text = "Идет считывание параметров вкладки PAR: 12%";
                        kv_par.tzero[term_now] = serial_port.reg_fl; Tzero.Text = "ВРЕМЯ НУЛЯ: " + kv_par.tzero[term_now]; serial_port.rd_mode++;
                        break;
                    case 101:
                        tbParStatus.Text = "Идет считывание параметров вкладки PAR: 25%";
                        kv_par.tg[term_now] = serial_port.reg_fl; Tg.Text = "ВЕС УШЕЛ: " + kv_par.tg[term_now]; serial_port.rd_mode++;
                       
                        break;
                    case 102:
                        kv_par.f2[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.f1[term_now] = (byte)(serial_port.reg_int - kv_par.f2[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки PAR: 50%";
                        Combo_F1.SelectedIndex = kv_par.f1[term_now];
                        Combo_F2.SelectedIndex = kv_par.f2[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 103:
                        //   cbA.SelectedIndex = serial_port.reg_int;
                        kv_par.baud[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.num[term_now] = (byte)(serial_port.reg_int - kv_par.f2[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки PAR: 75%";
                        Num.Text ="СЕТЕВОЙ НОМЕР: " + kv_par.num[term_now];
                        Combo_Baud.SelectedIndex = kv_par.baud[term_now];
                        serial_port.rd_mode++;
                        
                        break;
                    case 104:
                        kv_par.autozero[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.out_mode[term_now] = (byte)(serial_port.reg_int - kv_par.f2[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки PAR: 100%";
                        Combo_Autozero.SelectedIndex = kv_par.autozero[term_now];
                        Combo_Out_Mode.SelectedIndex = kv_par.out_mode[term_now];
                        serial_port.rd_mode=1;
                        reg_status_visible = 20;
                        break;
                    case 120:
                        tbParStatus.Text = "Идет запись параметров вкладки PAR: 12%";
                        write_reg_wait = 121;
                        serial_port.write_reg_fl = (float)kv_par.tg[term_now];
                        break;
                    case 121:
                        tbParStatus.Text = "Идет запись параметров вкладки PAR: 25%";
                        write_reg_wait = 122;
                        serial_port.write_reg_int = (UInt16)(kv_par.f2[term_now] * 0x100 + kv_par.f1[term_now]);
                        break;
                    case 122:
                        tbParStatus.Text = "Идет запись параметров вкладки PAR: 50%";
                        write_reg_wait = 123;
                        serial_port.write_reg_int = (UInt16)(kv_par.baud[term_now] * 0x100 + kv_par.num[term_now]);
                        break;
                    case 123:
                        tbParStatus.Text = "Идет запись параметров вкладки PAR: 75%";
                        write_reg_wait = 124;
                        serial_port.write_reg_int = (UInt16)(kv_par.autozero[term_now] * 0x100 + kv_par.out_mode[term_now]);
                        break;
                    case 124:
                        tbParStatus.Text = "Идет запись параметров вкладки PAR: 100%";
                        serial_port.rd_mode = 100;
                        reg_status_visible = 20;
                        break;
                    case 140:
                        kv_par.hz[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.mv[term_now] = (byte)(serial_port.reg_int - kv_par.hz[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 22%";
                        Combo_Mv.SelectedIndex = kv_par.mv[term_now];
                        Combo_Hz.SelectedIndex = kv_par.hz[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 141:
                        kv_par.polar[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.zpt[term_now] = (byte)(serial_port.reg_int - kv_par.polar[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 44%";
                        Combo_Polar.SelectedIndex = kv_par.polar[term_now];
                        Combo_Zpt.SelectedIndex = kv_par.zpt[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 142:
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 55%";
                        kv_par.npv[term_now] = serial_port.reg_fl; NPV.Text = "НПВ: " + kv_par.npv[term_now]; serial_port.rd_mode++;
                        break;
                    case 143:
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 66%";
                        kv_par.cal_weight[term_now] = serial_port.reg_fl; Cal_Weight.Text = "КАЛИБР. ВЕС: " + kv_par.cal_weight[term_now]; serial_port.rd_mode++;
                        break;
                    case 144:
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 77%";
                        kv_par.coeff[term_now] = serial_port.reg_fl; Cal_Coeff.Text = "КАЛИБР. КОЭФ.: " + kv_par.coeff[term_now]; serial_port.rd_mode++;
                        break;
                    case 145:
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 88%";
                        kv_par.cal_zero[term_now] = serial_port.reg_long; Cal_Zero.Text = "КОД НУЛЯ: " + kv_par.cal_zero[term_now]; serial_port.rd_mode++;
                        break;
                    case 146:
                         kv_par.discr[term_now] = (ushort)(serial_port.reg_int);
                         tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 100%";
                        Combo_Discr.SelectedIndex = kv_par.discr[term_now];
                        serial_port.rd_mode = 1;
                        reg_status_visible = 20;
                        break;
                    case 160:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 22%";
                        write_reg_wait = 161;
                        serial_port.write_reg_int = (UInt16)(kv_par.polar[term_now] * 0x100 + kv_par.zpt[term_now]);
                        break;
                    case 161:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 44%";
                        write_reg_wait = 162;
                        serial_port.write_reg_fl = (float)kv_par.npv[term_now];
                        break;
                    case 162:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 55%";
                        write_reg_wait = 163;
                        serial_port.write_reg_fl = (float)kv_par.cal_weight[term_now];
                        break;
                    case 163:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 66%";
                        write_reg_wait = 164;
                        serial_port.write_reg_fl = (float)kv_par.coeff[term_now];
                        break;
                    case 164:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 77%";
                        write_reg_wait = 165;
                        serial_port.write_reg_long = kv_par.cal_zero[term_now];
                        break;
                    case 165:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 88%";
                        write_reg_wait = 166;
                        serial_port.write_reg_int = kv_par.discr[term_now];
                        break;
                    case 166:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 100%";
                        serial_port.rd_mode = 140;
                        reg_status_visible = 20;
                        break;

// Считываем Calibr для лайт
                    case 180:
                        kv_par.hz[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.mv[term_now] = (byte)(serial_port.reg_int - kv_par.hz[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 25%";
                        Combo_Lite_Mv.SelectedIndex = kv_par.mv[term_now];
                        Combo_Lite_Hz.SelectedIndex = kv_par.hz[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 181:
                        kv_par.lite_point_num[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.zpt[term_now] = (byte)(serial_port.reg_int - kv_par.lite_point_num[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 50%";
                        if (Combo_Lite_PointNum.Items.Count > kv_par.lite_point_num[term_now])
                            Combo_Lite_PointNum.SelectedIndex = kv_par.lite_point_num[term_now];
                        if (Combo_Lite_Zpt.Items.Count > kv_par.zpt[term_now])
                            Combo_Lite_Zpt.SelectedIndex = kv_par.zpt[term_now];
                        //.SelectedIndex = kv_par.zpt[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 182:
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 62%";
                        kv_par.npv[term_now] = serial_port.reg_fl; Lite_NPV.Text = "НПВ: " + kv_par.npv[term_now]; serial_port.rd_mode++;
                        break;
                    case 183:
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 75%";
                        kv_par.discr[term_now] = (ushort)(serial_port.reg_int);
                        switch (kv_par.discr[term_now])
                        {
                            case 1: Combo_Lite_Discr.SelectedIndex = 0; break;
                            case 2: Combo_Lite_Discr.SelectedIndex = 1; break;
                            case 5: Combo_Lite_Discr.SelectedIndex = 2; break;
                            case 10: Combo_Lite_Discr.SelectedIndex = 3; break;
                            case 20: Combo_Lite_Discr.SelectedIndex = 4; break;
                            case 50: Combo_Lite_Discr.SelectedIndex = 5; break;
                            case 100: Combo_Lite_Discr.SelectedIndex = 6; break;
                        }
                        serial_port.rd_mode++;
                        break;
                    case 184:
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 85%";
                        kv_par.lite_zero_weight[term_now] = serial_port.reg_fl; Cal_Lite_Zero.Text = "СМЕЩЕНИЕ НУЛЯ: " + kv_par.lite_zero_weight[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 185:
                        kv_par.polar[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.polar[term_now] = (byte)(serial_port.reg_int - kv_par.polar[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 100%";
                        if (Combo_Lite_Polar.Items.Count > kv_par.polar[term_now])
                            Combo_Lite_Polar.SelectedIndex = kv_par.polar[term_now];
                        if (kv_par.polar[term_now] > 1) kv_par.polar[term_now] = 0;
                        
                        serial_port.rd_mode = 1;
                        reg_status_visible = 20;
                        break;
                    case 200:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 25%";
                        write_reg_wait = 201;
                        serial_port.write_reg_int = (UInt16)(kv_par.lite_point_num[term_now] * 0x100 + kv_par.zpt[term_now]);
                        break;
                    case 201:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 50%";
                        write_reg_wait = 202;
                        serial_port.write_reg_fl = (float)kv_par.npv[term_now];
                        break;
                    case 202:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 62%";
                        write_reg_wait = 203;
                        switch (Combo_Lite_Discr.SelectedIndex)
                        {
                            case 0: serial_port.write_reg_int = 1; break;
                            case 1: serial_port.write_reg_int = 2; break;
                            case 2: serial_port.write_reg_int = 5; break;
                            case 3: serial_port.write_reg_int = 10; break;
                            case 4: serial_port.write_reg_int = 20; break;
                            case 5: serial_port.write_reg_int = 50; break;
                            case 6: serial_port.write_reg_int = 100; break;
                        }

                        break;
                    case 203:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 75%";
                        write_reg_wait = 204;
                        serial_port.write_reg_fl = (float)kv_par.lite_zero_weight[term_now];
                        break;
                    case 204:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 87%";
                        write_reg_wait = 205;
                        serial_port.write_reg_int = (UInt16)(kv_par.polar[term_now] * 0x100 + kv_par.polar[term_now]);
                        break;
                    case 205:
                        tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 100%";
                        serial_port.rd_mode = 180;
                        reg_status_visible = 1;
                        break;
                    // Считываем Feed для лайт
                    case 220:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 25%";
                        kv_par.lite_stab_weight[term_now] = serial_port.reg_fl; Lite_StabWeigth.Text = "ДИАПАЗОН СТАБИЛЬНОГО ВЕСА: " + Convert.ToString(kv_par.lite_stab_weight[term_now]);
                        serial_port.rd_mode++;
                        break;
                    case 221:
                        kv_par.lite_stab_f1[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.lite_stab_f1[term_now] = (byte)(serial_port.reg_int - kv_par.lite_stab_f1[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 50%";
                        Combo_Lite_FStab.SelectedIndex = kv_par.lite_stab_f1[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 222:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 75%";
                        kv_par.lite_tzero[term_now] = serial_port.reg_fl; Lite_Zero_Time.Text = "ВРЕМЯ УСТАНОВКИ НУЛЯ: " + kv_par.lite_tzero[term_now]; serial_port.rd_mode++;
                        break;
                    case 223:
                        tbParStatus.Text = "Идет считывание параметров вкладки FEED: 100%";
                        kv_par.lite_w_zero[term_now] = serial_port.reg_fl; Lite_Zero_Weight.Text = "ДИАПАЗОН НУЛЕВОГО ВЕСА: " + kv_par.lite_w_zero[term_now];
                        UpdateSettings();
                        serial_port.rd_mode = 1;
                        reg_status_visible = 20;
                        break;
                    case 240:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 25%";
                        write_reg_wait = 241;
                        serial_port.write_reg_int = (UInt16)(kv_par.lite_stab_f1[term_now] * 0x100 + kv_par.lite_stab_f1[term_now]);
                        break;
                    case 241:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 50%";
                        write_reg_wait = 242;
                        serial_port.write_reg_fl = (float)(kv_par.lite_tzero[term_now]);
                        break;
                    case 242:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 75%";
                        write_reg_wait = 243;
                        serial_port.write_reg_fl = (float)(kv_par.lite_w_zero[term_now]);
                        break;
                    case 243:
                        tbParStatus.Text = "Идет запись параметров вкладки FEED: 100%";
                        serial_port.rd_mode = 220;
                        reg_status_visible = 1;
                        break;
                    case 260:
                        kv_par.f1[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.lite_mode[term_now] = (byte)(serial_port.reg_int - kv_par.f1[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки PAR: 33%";
                        if (Combo_Lite_Mode.Items.Count > kv_par.lite_mode[term_now])
                            Combo_Lite_Mode.SelectedIndex = kv_par.lite_mode[term_now];
                        if (Combo_Lite_F1.Items.Count > kv_par.f1[term_now])
                            Combo_Lite_F1.SelectedIndex = kv_par.f1[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 261:
                        kv_par.num[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.f2[term_now] = (byte)(serial_port.reg_int - kv_par.num[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки PAR: 67%";
                        Lite_Num.Text = "СЕТЕВОЙ НОМЕР: " + kv_par.num[term_now];
                        if (Combo_Lite_F2.Items.Count > kv_par.f2[term_now])
                            Combo_Lite_F2.SelectedIndex = kv_par.f2[term_now];
                        serial_port.rd_mode++;
                        break;
                    case 262:
                        kv_par.lite_direction[term_now] = (byte)(serial_port.reg_int / 0x100);
                        kv_par.baud[term_now] = (byte)(serial_port.reg_int - kv_par.lite_direction[term_now] * 0x100);
                        tbParStatus.Text = "Идет считывание параметров вкладки PAR: 100%";
                        if (Combo_Lite_Direction.Items.Count > kv_par.lite_direction[term_now])
                            Combo_Lite_Direction.SelectedIndex = kv_par.lite_direction[term_now];
                        if (Combo_Lite_Baud.Items.Count > kv_par.baud[term_now])
                            Combo_Lite_Baud.SelectedIndex = kv_par.baud[term_now];
                       // UpdateSettings();
                        serial_port.rd_mode = 1;
                        reg_status_visible = 20;
                        break;
                    case 280:
                        tbParStatus.Text = "Идет запись параметров вкладки PAR: 33%";
                        write_reg_wait = 281;
                        serial_port.write_reg_int = (UInt16)(kv_par.num[term_now] * 0x100 + kv_par.f2[term_now]);
                        break;
                    case 281:
                        tbParStatus.Text = "Идет запись параметров вкладки PAR: 67%";
                        write_reg_wait = 282;
                        serial_port.write_reg_int = (UInt16)(kv_par.lite_direction[term_now] * 0x100 + kv_par.baud[term_now]);
                        break;
                    case 282:
                        tbParStatus.Text = "Идет запись параметров вкладки PAR: 100%";
                        serial_port.rd_mode = 260;
                        reg_status_visible = 1;
                       break;
                    default:

                            serial_port.rd_mode = 1;
                        break;
                }
                serial_port.wait_answer = 0;

                reg_try = 0;
                num_reg_read++;
                if (num_reg_read > 21) num_reg_read = 0;


            }
            else
            {
                reg_try++;
                if (reg_try >= 40) { serial_port.wait_answer = 0; reg_try = 0; serial_port.rd_mode = 1; }
            }
            if (serial_port.wait_answer == 0)
            {
                if (write_reg_wait != 0)
                    serial_port.rd_mode = write_reg_wait;
                write_reg_wait = 0;

                term_num_req++; if (term_num_req > 9) term_num_req = 0;
                for (int i = 0; i <= 9; i++)
                {
                    if (!prg_par.enable[term_num_req])
                    {
                        term_num_req++; if (term_num_req > 9) term_num_req = 0;
                    }
                    else i = 10;
                }
                if (next_adress==-1)
                serial_port.term_adress = (byte)prg_par.num[term_num_req];
                else serial_port.term_adress = (byte)prg_par.num[term_now];


                await serial_port.WriteToPort();

            }
            tbModbusStatus.Text = serial_port.status;

        }

        private void UpdatePostEnabled()
        {
            Text_ID_Post_1.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[0]];
            Text_ID_Post_2.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[1]];
            Text_ID_Post_3.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[2]];
            Text_ID_Post_4.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[3]];
            Text_ID_Post_5.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[4]];
            Text_ID_Post_6.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[5]];
            Text_ID_Post_7.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[6]];
            Text_ID_Post_8.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[7]];
            Text_ID_Post_9.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[8]];
            Text_ID_Post_10.Text = "" + Combo_Post_ID.Items[prg_par.selected_id[9]];
            if (prg_par.enable[0])
            {
                tb_Weight_Post_1.Visibility = Visibility.Visible;
                tb_OutWeight_Post_1.Visibility = Visibility.Visible;
                tb_State_Post_1.Visibility = Visibility.Visible;
                Text_ID_Post_1.Visibility = Visibility.Visible;
                
            }
            else
            {
                tb_Weight_Post_1.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_1.Visibility = Visibility.Collapsed;
                tb_State_Post_1.Visibility = Visibility.Collapsed;
                Text_ID_Post_1.Visibility = Visibility.Collapsed;
            }
            if (prg_par.enable[1])
            {
                tb_Weight_Post_2.Visibility = Visibility.Visible;
                tb_OutWeight_Post_2.Visibility = Visibility.Visible;
                tb_State_Post_2.Visibility = Visibility.Visible;
                Text_ID_Post_2.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_2.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_2.Visibility = Visibility.Collapsed;
                tb_State_Post_2.Visibility = Visibility.Collapsed;
                Text_ID_Post_2.Visibility = Visibility.Collapsed;
            }
            if (prg_par.enable[2])
            {
                tb_Weight_Post_3.Visibility = Visibility.Visible;
                tb_OutWeight_Post_3.Visibility = Visibility.Visible;
                tb_State_Post_3.Visibility = Visibility.Visible;
                Text_ID_Post_3.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_3.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_3.Visibility = Visibility.Collapsed;
                tb_State_Post_3.Visibility = Visibility.Collapsed;
                Text_ID_Post_3.Visibility = Visibility.Collapsed;
            }

            if (prg_par.enable[3])
            {
                tb_Weight_Post_4.Visibility = Visibility.Visible;
                tb_OutWeight_Post_4.Visibility = Visibility.Visible;
                tb_State_Post_4.Visibility = Visibility.Visible;
                Text_ID_Post_4.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_4.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_4.Visibility = Visibility.Collapsed;
                tb_State_Post_4.Visibility = Visibility.Collapsed;
                Text_ID_Post_4.Visibility = Visibility.Collapsed;
            }
            if (prg_par.enable[4])
            {
                tb_Weight_Post_5.Visibility = Visibility.Visible;
                tb_OutWeight_Post_5.Visibility = Visibility.Visible;
                tb_State_Post_5.Visibility = Visibility.Visible;
                Text_ID_Post_5.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_5.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_5.Visibility = Visibility.Collapsed;
                tb_State_Post_5.Visibility = Visibility.Collapsed;
                Text_ID_Post_5.Visibility = Visibility.Collapsed;
            }
            if (prg_par.enable[5])
            {
                tb_Weight_Post_6.Visibility = Visibility.Visible;
                tb_OutWeight_Post_6.Visibility = Visibility.Visible;
                tb_State_Post_6.Visibility = Visibility.Visible;
                Text_ID_Post_6.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_6.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_6.Visibility = Visibility.Collapsed;
                tb_State_Post_6.Visibility = Visibility.Collapsed;
                Text_ID_Post_6.Visibility = Visibility.Collapsed;
            }
            if (prg_par.enable[6])
            {
                tb_Weight_Post_7.Visibility = Visibility.Visible;
                tb_OutWeight_Post_7.Visibility = Visibility.Visible;
                tb_State_Post_7.Visibility = Visibility.Visible;
                Text_ID_Post_7.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_7.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_7.Visibility = Visibility.Collapsed;
                tb_State_Post_7.Visibility = Visibility.Collapsed;
                Text_ID_Post_7.Visibility = Visibility.Collapsed;
            }
            if (prg_par.enable[7])
            {
                tb_Weight_Post_8.Visibility = Visibility.Visible;
                tb_OutWeight_Post_8.Visibility = Visibility.Visible;
                tb_State_Post_8.Visibility = Visibility.Visible;
                Text_ID_Post_8.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_8.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_8.Visibility = Visibility.Collapsed;
                tb_State_Post_8.Visibility = Visibility.Collapsed;
                Text_ID_Post_8.Visibility = Visibility.Collapsed;
            }
            if (prg_par.enable[8])
            {
                tb_Weight_Post_9.Visibility = Visibility.Visible;
                tb_OutWeight_Post_9.Visibility = Visibility.Visible;
                tb_State_Post_9.Visibility = Visibility.Visible;
                Text_ID_Post_9.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_9.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_9.Visibility = Visibility.Collapsed;
                tb_State_Post_9.Visibility = Visibility.Collapsed;
                Text_ID_Post_9.Visibility = Visibility.Collapsed;
            }
            if (prg_par.enable[9])
            {
                tb_Weight_Post_10.Visibility = Visibility.Visible;
                tb_OutWeight_Post_10.Visibility = Visibility.Visible;
                tb_State_Post_10.Visibility = Visibility.Visible;
                Text_ID_Post_10.Visibility = Visibility.Visible;
            }
            else
            {
                tb_Weight_Post_10.Visibility = Visibility.Collapsed;
                tb_OutWeight_Post_10.Visibility = Visibility.Collapsed;
                tb_State_Post_10.Visibility = Visibility.Collapsed;
                Text_ID_Post_10.Visibility = Visibility.Collapsed;
            }
        }
        private void UpdatePrgParam()
        {
            Prg_Adr_1.Text = "АДРЕС: " + prg_par.num[0];
            Prg_Adr_2.Text = "АДРЕС: " + prg_par.num[1];
            Prg_Adr_3.Text = "АДРЕС: " + prg_par.num[2];
            Prg_Adr_4.Text = "АДРЕС: " + prg_par.num[3];
            Prg_Adr_5.Text = "АДРЕС: " + prg_par.num[4];
            Prg_Adr_6.Text = "АДРЕС: " + prg_par.num[5];
            Prg_Adr_7.Text = "АДРЕС: " + prg_par.num[6];
            Prg_Adr_8.Text = "АДРЕС: " + prg_par.num[7];
            Prg_Adr_9.Text = "АДРЕС: " + prg_par.num[8];
            Prg_Adr_10.Text = "АДРЕС: " + prg_par.num[9];


            Combo_Prg_Ver_1.SelectedIndex = prg_par.ver[0];
            Combo_Prg_Ver_2.SelectedIndex = prg_par.ver[1];
            Combo_Prg_Ver_3.SelectedIndex = prg_par.ver[2];
            Combo_Prg_Ver_4.SelectedIndex = prg_par.ver[3];
            Combo_Prg_Ver_5.SelectedIndex = prg_par.ver[4];
            Combo_Prg_Ver_6.SelectedIndex = prg_par.ver[5];
            Combo_Prg_Ver_7.SelectedIndex = prg_par.ver[6];
            Combo_Prg_Ver_8.SelectedIndex = prg_par.ver[7];
            Combo_Prg_Ver_9.SelectedIndex = prg_par.ver[8];
            Combo_Prg_Ver_10.SelectedIndex = prg_par.ver[9];

            cb_Term_Enable_1.IsChecked = prg_par.enable[0];
            cb_Term_Enable_2.IsChecked = prg_par.enable[1];
            cb_Term_Enable_3.IsChecked = prg_par.enable[2];
            cb_Term_Enable_4.IsChecked = prg_par.enable[3];
            cb_Term_Enable_5.IsChecked = prg_par.enable[4];
            cb_Term_Enable_6.IsChecked = prg_par.enable[5];
            cb_Term_Enable_7.IsChecked = prg_par.enable[6];
            cb_Term_Enable_8.IsChecked = prg_par.enable[7];
            cb_Term_Enable_9.IsChecked = prg_par.enable[8];
            cb_Term_Enable_10.IsChecked = prg_par.enable[9];


            cbComBaud.SelectedIndex = prg_par.ComBaud;
            cbComMode.SelectedIndex = prg_par.ComMode;
            cbComMonitor.SelectedIndex = prg_par.ComMonitor;

            Combo_SmenaNum.SelectedIndex = prg_par.SmenaNum;
            Combo_SmenaHour.SelectedIndex =  prg_par.SmenaHour;
            Combo_SmenaMinute.SelectedIndex =  prg_par.SmenaMinute;


        }
        private void UpdateSettings()
        {
            Doza11.Text = "КОМПОНЕНТ: " + Convert.ToSingle(kv_par.doze[term_now, 0]);
            Doza11.Text = Doza11.Text.Replace(",", ".");
            Doza12.Text = "КОМПОНЕНТ: " + Convert.ToSingle(kv_par.doze[term_now, 1]);
            Doza12.Text = Doza12.Text.Replace(",", ".");
            Doza13.Text = "КОМПОНЕНТ: " + Convert.ToSingle(kv_par.doze[term_now, 2]);
            Doza13.Text = Doza13.Text.Replace(",", ".");
            Doza21.Text = "КОМПОНЕНТ: " + Convert.ToSingle(kv_par.doze[term_now, 3]);
            Doza21.Text = Doza21.Text.Replace(",", ".");
            Doza22.Text = "КОМПОНЕНТ: " + Convert.ToSingle(kv_par.doze[term_now, 4]);
            Doza22.Text = Doza22.Text.Replace(",", ".");
            Doza23.Text = "КОМПОНЕНТ: " + Convert.ToSingle(kv_par.doze[term_now, 5]);
            Doza23.Text = Doza23.Text.Replace(",", ".");
            Dw1.Text = "НЕДОВЕС ГРУБО: " + Convert.ToSingle(kv_par.dw[term_now, 0]);
            Dw1.Text = Dw1.Text.Replace(",", ".");
            Dw2.Text = "НЕДОВЕС ГРУБО: " + Convert.ToSingle(kv_par.dw[term_now, 1]);
            Dw2.Text = Dw2.Text.Replace(",", ".");
            Dw3.Text = "НЕДОВЕС ГРУБО: " + Convert.ToSingle(kv_par.dw[term_now, 2]);
            Dw3.Text = Dw3.Text.Replace(",", ".");
            Dwi1.Text = "НЕДОВЕС ТОЧНО: " + Convert.ToSingle(kv_par.dwi[term_now, 0]);
            Dwi1.Text = Dwi1.Text.Replace(",", ".");
            Dwi2.Text = "НЕДОВЕС ТОЧНО: " + Convert.ToSingle(kv_par.dwi[term_now, 1]);
            Dwi2.Text = Dwi2.Text.Replace(",", ".");
            Dwi3.Text = "НЕДОВЕС ТОЧНО: " + Convert.ToSingle(kv_par.dwi[term_now, 2]);
            Dwi3.Text = Dwi3.Text.Replace(",", ".");
            Impulse1.Text = "ИМПУЛЬС: " + Convert.ToSingle(kv_par.impulse[term_now, 0]);
            Impulse1.Text = Impulse1.Text.Replace(",", ".");
            Impulse2.Text = "ИМПУЛЬС: " + Convert.ToSingle(kv_par.impulse[term_now, 1]);
            Impulse2.Text = Impulse2.Text.Replace(",", ".");
            Impulse3.Text = "ИМПУЛЬС: " + Convert.ToSingle(kv_par.impulse[term_now, 2]);
            Impulse3.Text = Impulse3.Text.Replace(",", ".");
            Pause1.Text = "ПАУЗА: " + Convert.ToSingle(kv_par.pause[term_now, 0]);
            Pause1.Text = Pause1.Text.Replace(",", ".");
            Pause2.Text = "ПАУЗА: " + Convert.ToSingle(kv_par.pause[term_now, 1]);
            Pause2.Text = Pause2.Text.Replace(",", ".");
            Pause3.Text = "ПАУЗА: " + Convert.ToSingle(kv_par.pause[term_now, 2]);
            Pause3.Text = Pause3.Text.Replace(",", ".");
            Tzero.Text = "ВРЕМЯ НУЛЯ: " + Convert.ToSingle(kv_par.tzero[term_now]);
            Tzero.Text = Tzero.Text.Replace(",", ".");
            Tg.Text = "ВЕС УШЕЛ: " + Convert.ToSingle(kv_par.tg[term_now]);
            Tg.Text = Tg.Text.Replace(",", ".");
            Num.Text = "СЕТЕВОЙ НОМЕР: " + kv_par.num[term_now];
            Combo_F1.SelectedIndex = kv_par.f1[term_now];
            Combo_F2.SelectedIndex = kv_par.f2[term_now];
            Combo_Baud.SelectedIndex = kv_par.baud[term_now];
            Combo_Autozero.SelectedIndex = kv_par.autozero[term_now];
            Combo_Out_Mode.SelectedIndex = kv_par.out_mode[term_now];

            Combo_Polar.SelectedIndex = kv_par.polar[term_now];
            Combo_Discr.SelectedIndex = kv_par.discr[term_now];
            Combo_Zpt.SelectedIndex = kv_par.zpt[term_now];
            Combo_Hz.SelectedIndex = kv_par.hz[term_now];
            Combo_Mv.SelectedIndex = kv_par.mv[term_now];
            Cal_Zero.Text = "КОД НУЛЯ: " + Convert.ToUInt32(kv_par.cal_zero[term_now]);
            Cal_Zero.Text = Cal_Zero.Text.Replace(",", ".");
            NPV.Text = "НПВ: " + Convert.ToSingle(kv_par.npv[term_now]);
            NPV.Text = NPV.Text.Replace(",", ".");
            Cal_Weight.Text = "КАЛИБР. ВЕС: " + Convert.ToSingle(kv_par.cal_weight[term_now]);
            Cal_Weight.Text = Cal_Weight.Text.Replace(",", ".");
            Cal_Coeff.Text = "КАЛИБР. КОЭФ.: " + Convert.ToSingle(kv_par.coeff[term_now]);
            Cal_Coeff.Text = Cal_Coeff.Text.Replace(",", ".");

            Grid_Left_Monitor.Background = BrushOff;
            Grid_Left_KV_Param.Background = BrushOn;
            Grid_Left_Prg_Param.Background = BrushOff;
            Grid_Left_Archive.Background = BrushOff;
            Grid_KV_Levels.Visibility = Visibility.Collapsed;
            Grid_KV_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Calibr.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Calibr.Visibility = Visibility.Collapsed;

            Combo_Lite_Polar.SelectedIndex = kv_par.polar[term_now];
            Combo_Lite_Discr.SelectedIndex = kv_par.discr[term_now];
            Combo_Lite_Zpt.SelectedIndex = kv_par.zpt[term_now];
            Combo_Lite_Hz.SelectedIndex = kv_par.hz[term_now];
            Combo_Lite_Mv.SelectedIndex = kv_par.mv[term_now];
            Lite_NPV.Text = "НПВ: " + Convert.ToSingle(kv_par.npv[term_now]);
            Lite_NPV.Text = Lite_NPV.Text.Replace(",", ".");
            Cal_Lite_Zero.Text="СМЕЩЕНИЕ НУЛЯ: "+ Convert.ToSingle(kv_par.lite_zero_weight[term_now]);
            Cal_Lite_Zero.Text = Cal_Lite_Zero.Text.Replace(",", ".");

            Lite_StabWeigth.Text = "ДИАПАЗОН СТАБИЛЬНОГО ВЕСА: " + Convert.ToSingle(kv_par.lite_stab_weight[term_now]);
            Lite_StabWeigth.Text = Lite_StabWeigth.Text.Replace(",", ".");

            Combo_Lite_FStab.SelectedIndex= kv_par.lite_stab_f1[term_now];

            Lite_Zero_Time.Text = "ВРЕМЯ УСТАНОВКИ НУЛЯ: " + Convert.ToSingle(kv_par.lite_tzero[term_now]);
            Lite_Zero_Time.Text = Lite_Zero_Time.Text.Replace(",", ".");
            Lite_Zero_Weight.Text = "ДИАПАЗОН НУЛЕВОГО ВЕСА: " + Convert.ToSingle(kv_par.lite_w_zero[term_now]);
            Lite_Zero_Weight.Text = Lite_Zero_Weight.Text.Replace(",", ".");

            Combo_Lite_Mode.SelectedIndex = kv_par.lite_mode[term_now];
            Combo_Lite_F1.SelectedIndex = kv_par.f1[term_now];
            Combo_Lite_F2.SelectedIndex = kv_par.f2[term_now];
            Combo_Lite_F2.SelectedIndex = kv_par.f2[term_now];
            Combo_Lite_Direction.SelectedIndex = kv_par.lite_direction[term_now];
            Combo_Lite_Baud.SelectedIndex = kv_par.baud[term_now];
            Lite_Num.Text = "СЕТЕВОЙ НОМЕР: " + Convert.ToUInt32(kv_par.num[term_now]);
            Combo_Lite_PointNum.SelectedIndex = kv_par.lite_point_num[term_now];
            if (prg_par.ver[term_now] == 1)
            {
                Button_Param_Levels.Visibility = Visibility.Visible;
                Grid_Param_Levels.Background = BrushOn;
                Grid_Param_Feed.Background = BrushOff;
                Grid_Param_Par.Background = BrushOff;
                Grid_Param_Calibr.Background = BrushOff;
                mode = 1;
                par_flag = false;
                Grid_Post_Bottom.Visibility = Visibility.Collapsed;

                Grid_KV_Levels.Visibility = Visibility.Visible;

            }
            else
            {
                Button_Param_Levels.Visibility = Visibility.Collapsed;
                Grid_Param_Levels.Background = BrushOff;
                Grid_Param_Feed.Background = BrushOn;
                Grid_Param_Par.Background = BrushOff;
                Grid_Param_Calibr.Background = BrushOff;
                mode = 1;
                par_flag = false;
                Grid_Post_Bottom.Visibility = Visibility.Collapsed;

                Grid_KV_Lite_Feed.Visibility = Visibility.Visible;

            }
            Grid_Prog_Settings.Visibility = Visibility.Collapsed;
            Grid_Post_Main.Visibility = Visibility.Collapsed;
            Grid_Post_Settings.Visibility = Visibility.Visible;
            Grid_Archive_Settings.Visibility = Visibility.Collapsed;


        }
        private async void btnKVSettings_Click(object sender, RoutedEventArgs e)
        {

            if ((par_flag) && (mode == 2)) { await prg_par.SaveParFile(prg_filename); par_flag = false; UpdatePostEnabled(); }

            if (mode != 1) UpdateSettings();
            /*

            if (prg_par.ver[term_now] == 1)
            {
                Button_Param_Levels.Visibility = Visibility.Visible;
                Grid_Param_Levels.Background = BrushOn;
                Grid_Param_Feed.Background = BrushOff;
                Grid_Param_Par.Background = BrushOff;
                Grid_Param_Calibr.Background = BrushOff;
                mode = 1;
                par_flag = false;
                Grid_Post_Bottom.Visibility = Visibility.Collapsed;

                Grid_KV_Levels.Visibility = Visibility.Visible;

            }
            else
            {
                Button_Param_Levels.Visibility = Visibility.Collapsed;
                Grid_Param_Levels.Background = BrushOff;
                Grid_Param_Feed.Background = BrushOn;
                Grid_Param_Par.Background = BrushOff;
                Grid_Param_Calibr.Background = BrushOff;
                mode = 1;
                par_flag = false;
                Grid_Post_Bottom.Visibility = Visibility.Collapsed;

                Grid_KV_Lite_Feed.Visibility = Visibility.Visible;

            }
            Grid_Prog_Settings.Visibility = Visibility.Collapsed;
            Grid_Post_Main.Visibility = Visibility.Collapsed;
            Grid_Post_Settings.Visibility = Visibility.Visible;*/
        }

        private async void btnMonitor_Click(object sender, RoutedEventArgs e)
        {
            Grid_Left_Monitor.Background = BrushOn;
            Grid_Left_KV_Param.Background = BrushOff;
            Grid_Left_Prg_Param.Background = BrushOff;
            Grid_Left_Archive.Background = BrushOff;
            if ((par_flag) && (mode == 1)) { await kv_par.SaveKVFile(param_filename); par_flag = false; }
            if ((par_flag) && (mode == 2)) { await prg_par.SaveParFile(prg_filename); par_flag = false; UpdatePostEnabled(); }
            if ((par_flag) && (mode == 3)) { await products.SaveProductFile(prod_filename); par_flag = false; }
            Text_Doza_1.Text = "ДОЗА: " + Convert.ToString(kv_par.doze[term_now, 0]);
            Text_Doza_2.Text = "ДОЗА: " + Convert.ToString(kv_par.doze[term_now, 1]);
            Text_Doza_3.Text = "ДОЗА: " + Convert.ToString(kv_par.doze[term_now, 2]);
            Text_Doza_1.Text = Text_Doza_1.Text.Replace(",", ".");
            Text_Doza_2.Text = Text_Doza_3.Text.Replace(",", ".");
            Text_Doza_3.Text = Text_Doza_3.Text.Replace(",", ".");
            mode = 0;
            Grid_Post_Main.Visibility = Visibility.Visible;
            Grid_Post_Settings.Visibility = Visibility.Collapsed;
            Grid_Post_Bottom.Visibility = Visibility.Visible;
            Grid_Post_Top.Visibility = Visibility.Visible;
            Grid_Prog_Settings.Visibility = Visibility.Collapsed;
            Grid_Archive_Settings.Visibility = Visibility.Collapsed;

        }

        private async void btnPrgSettings_Click(object sender, RoutedEventArgs e)
        {
            if ((par_flag) && (mode == 1)) { await kv_par.SaveKVFile(param_filename); par_flag = false; }
            if ((par_flag) && (mode == 2)) { await prg_par.SaveParFile(prg_filename); par_flag = false; UpdatePostEnabled(); }
            if ((par_flag) && (mode == 3)) { await products.SaveProductFile(prod_filename); par_flag = false; }

            UpdatePrgParam();
            tbParStatus.Visibility = Visibility.Collapsed;
            Grid_Left_Monitor.Background = BrushOff;
            Grid_Left_KV_Param.Background = BrushOff;
            Grid_Left_Prg_Param.Background = BrushOn;
            Grid_Left_Archive.Background = BrushOff;
            Grid_Post_Settings.Visibility = Visibility.Collapsed;
            Grid_Post_Main.Visibility = Visibility.Collapsed;
            Grid_Post_Bottom.Visibility = Visibility.Collapsed;
            Grid_Post_Top.Visibility = Visibility.Collapsed;
            Grid_Prog_Settings.Visibility = Visibility.Visible;
            Grid_Archive_Settings.Visibility = Visibility.Collapsed;
            mode = 2;
            par_flag = false;

        }

        private async void btnArchive_Click(object sender, RoutedEventArgs e)
        {
            if ((par_flag) && (mode == 1)) { await kv_par.SaveKVFile(param_filename); par_flag = false; }
            if ((par_flag) && (mode == 2)) { await prg_par.SaveParFile(prg_filename); par_flag = false; UpdatePostEnabled(); }
            if ((par_flag) && (mode == 3)) { await products.SaveProductFile(prod_filename); par_flag = false; }

            Grid_Left_Monitor.Background = BrushOff;
            Grid_Left_KV_Param.Background = BrushOff;
            Grid_Left_Prg_Param.Background = BrushOff;
            Grid_Left_Archive.Background = BrushOn;
            Grid_Post_Settings.Visibility = Visibility.Collapsed;
            Grid_Post_Main.Visibility = Visibility.Collapsed;
            Grid_Post_Bottom.Visibility = Visibility.Collapsed;
            Grid_Post_Top.Visibility = Visibility.Collapsed;
            Grid_Prog_Settings.Visibility = Visibility.Collapsed;

            Grid_Arc_Main.Background = BrushOn;
            Grid_Arc_Product.Background = BrushOff;
            Grid_Archive_Settings.Visibility = Visibility.Visible;
            Grid_Arc_Prod.Visibility = Visibility.Collapsed;
            Grid_Arc_Arc.Visibility = Visibility.Visible;

            Combo_Arc_ProdId.SelectedIndex = 0;
            mode = 3;
            par_flag = false;

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Doza11.Text = "КОМПОНЕНТ 1: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 0] = Convert.ToSingle(text);
                par_flag = true;
            }
        }

        private void Button_Param_Levels_Click(object sender, RoutedEventArgs e)
        {
            Grid_KV_Levels.Visibility = Visibility.Visible;
            Grid_KV_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Calibr.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Calibr.Visibility = Visibility.Collapsed;

            Grid_Param_Levels.Background = BrushOn;
            Grid_Param_Feed.Background = BrushOff;
            Grid_Param_Par.Background = BrushOff;
            Grid_Param_Calibr.Background = BrushOff;
        }

        private void Button_Param_Feed_Click(object sender, RoutedEventArgs e)
        {
            Grid_KV_Levels.Visibility = Visibility.Collapsed;
            Grid_KV_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Calibr.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Calibr.Visibility = Visibility.Collapsed;

            Grid_Param_Levels.Background = BrushOff;
            Grid_Param_Feed.Background = BrushOn;
            Grid_Param_Par.Background = BrushOff;
            Grid_Param_Calibr.Background = BrushOff;

            switch (prg_par.ver[term_now])
            {
                case 0:
                    Grid_KV_Lite_Feed.Visibility = Visibility.Visible;
                    break;
                case 1:
                    Grid_KV_Feed.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Button_Param_Par_Click(object sender, RoutedEventArgs e)
        {
            Grid_KV_Levels.Visibility = Visibility.Collapsed;
            Grid_KV_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Calibr.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Calibr.Visibility = Visibility.Collapsed;
            Grid_Param_Levels.Background = BrushOff;
            Grid_Param_Feed.Background = BrushOff;
            Grid_Param_Par.Background = BrushOn;
            Grid_Param_Calibr.Background = BrushOff;

            switch (prg_par.ver[term_now])
            {
                case 0:
                    Grid_KV_Lite_Par.Visibility = Visibility.Visible;
                    break;
                case 1:
                    Grid_KV_Par.Visibility = Visibility.Visible;
                    break;
            }


        }

        private void Button_Param_Calibr_Click(object sender, RoutedEventArgs e)
        {
            Grid_KV_Levels.Visibility = Visibility.Collapsed;
            Grid_KV_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Calibr.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Feed.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Par.Visibility = Visibility.Collapsed;
            Grid_KV_Lite_Calibr.Visibility = Visibility.Collapsed;
            Grid_Param_Levels.Background = BrushOff;
            Grid_Param_Feed.Background = BrushOff;
            Grid_Param_Par.Background = BrushOff;
            Grid_Param_Calibr.Background = BrushOn;

            switch (prg_par.ver[term_now])
            {
                case 0:
                    Grid_KV_Lite_Calibr.Visibility = Visibility.Visible;
                    break;
                case 1:
                    Grid_KV_Calibr.Visibility = Visibility.Visible;
                    break;
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void btn_Doza_12_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Doza12.Text = "КОМПОНЕНТ 2: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 1] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Doza_13_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Doza13.Text = "КОМПОНЕНТ 3: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 2] = Convert.ToSingle(text);
                par_flag = true;
            }
        }

        private async void btn_Doza_21_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 3]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Doza21.Text = "КОМПОНЕНТ 1: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 3] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Doza_22_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 4]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Doza22.Text = "КОМПОНЕНТ 2: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 4] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Doza_23_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 5]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Doza23.Text = "КОМПОНЕНТ 3: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 5] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnDw1_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.dw[term_now, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Dw1.Text = "НЕДОВЕС ГРУБО: " + text;
                text = text.Replace(",", ".");
                kv_par.dw[term_now, 0] = Convert.ToSingle(text);
                par_flag = true;
            }
        }

        private async void btnDw2_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.dw[term_now, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Dw2.Text = "НЕДОВЕС ГРУБО: " + text;
                text = text.Replace(",", ".");
                kv_par.dw[term_now, 1] = Convert.ToSingle(text);
                par_flag = true;
            }
        }

        private async void btnDw3_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.dw[term_now, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Dw3.Text = "НЕДОВЕС ГРУБО: " + text;
                text = text.Replace(",", ".");
                kv_par.dw[term_now, 2] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnDwi1_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.dwi[term_now, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Dwi1.Text = "НЕДОВЕС ТОЧНО: " + text;
                text = text.Replace(",", ".");
                kv_par.dwi[term_now, 0] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnDwi2_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.dwi[term_now, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Dwi2.Text = "НЕДОВЕС ТОЧНО: " + text;
                text = text.Replace(",", ".");
                kv_par.dwi[term_now, 1] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnDwi3_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.dwi[term_now, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Dwi3.Text = "НЕДОВЕС ТОЧНО: " + text;
                text = text.Replace(",", ".");
                kv_par.dwi[term_now, 2] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnPause1_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.pause[term_now, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Pause1.Text = "ПАУЗА: " + text;
                text = text.Replace(",", ".");
                kv_par.pause[term_now, 0] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnPause2_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.pause[term_now, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Pause2.Text = "ПАУЗА: " + text;
                text = text.Replace(",", ".");
                kv_par.pause[term_now, 1] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnPause3_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.pause[term_now, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Pause3.Text = "ПАУЗА: " + text;
                text = text.Replace(",", ".");
                kv_par.pause[term_now, 2] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnImpulse1_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.impulse[term_now, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Impulse1.Text = "ИМПУЛЬС: " + text;
                text = text.Replace(",", ".");
                kv_par.impulse[term_now, 0] = Convert.ToSingle(text);
                par_flag = true;
            }
        }

        private async void btnImpulse2_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.impulse[term_now, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Impulse2.Text = "ИМПУЛЬС: " + text;
                text = text.Replace(",", ".");
                kv_par.impulse[term_now, 1] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnImpulse3_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.impulse[term_now, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Impulse3.Text = "ИМПУЛЬС: " + text;
                text = text.Replace(",", ".");
                kv_par.impulse[term_now, 2] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

   //     private async void btn_Tp_Click(object sender, RoutedEventArgs e)
   //     {


    //    }

        private async void btn_Tg_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.tg[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Tg.Text = "ВЕС УШЕЛ: " + text;
                text = text.Replace(",", ".");
                kv_par.tg[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Num_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.num[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Num.Text = "СЕТЕВОЙ НОМЕР: " + text;
                text = text.Replace(",", ".");
                kv_par.num[term_now] = Convert.ToUInt16(text);
                par_flag = true;
            }

        }

        private void Combo_F1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.f1[term_now] = (UInt16)Combo_F1.SelectedIndex;
        }

        private void Combo_F2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.f2[term_now] = (UInt16)Combo_F2.SelectedIndex;
        }

        private void Combo_Baud_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.baud[term_now] = (UInt16)Combo_Baud.SelectedIndex;
        }

        private void Combo_Out_Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.out_mode[term_now] = (UInt16)Combo_Out_Mode.SelectedIndex;
        }

        private void Combo_Autozero_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.autozero[term_now] = (UInt16)Combo_Autozero.SelectedIndex;
        }

        private void Combo_Polar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.polar[term_now] = (UInt16)Combo_Polar.SelectedIndex;
        }

        private void Combo_Discr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.discr[term_now] = (UInt16)Combo_Discr.SelectedIndex;
        }

        private void Combo_Zpt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.zpt[term_now] = (UInt16)Combo_Zpt.SelectedIndex;
        }

        private void Combo_Hz_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.hz[term_now] = (UInt16)Combo_Hz.SelectedIndex;
        }

        private void Combo_Mv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.mv[term_now] = (UInt16)Combo_Mv.SelectedIndex;
        }

        private async void btnCalZero_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.cal_zero[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                text = text.Replace(",", ".");
                float fl = Convert.ToSingle(text);
                UInt32 i = (UInt32)fl;
                Cal_Zero.Text = "КОД НУЛЯ: " + i;
                //text = text.Replace(".", ",");
                kv_par.cal_zero[term_now] = i;
                par_flag = true;
            }
        }

        private async void btnNpv_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.npv[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                NPV.Text = "НПВ: " + text;
                Lite_NPV.Text = "НПВ: " + text;
                text = text.Replace(".", ",");
                kv_par.npv[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }
        }

        private async void btnCalWeight_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.cal_weight[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Cal_Weight.Text = "КАЛИБР. ВЕС: " + text;
                text = text.Replace(",", ".");
                kv_par.cal_weight[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btnCoeff_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.coeff[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Cal_Coeff.Text = "КАЛИБР. КОЭФ.: " + text;
                text = text.Replace(",", ".");
                kv_par.coeff[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }
        }


        private void Grid_Post_1_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            string n1 = (sender as Grid).Name;
            n1 = n1.Replace("Grid_Post_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            SolidColorBrush BrushOnDark = new SolidColorBrush();
            i1--;
            if (prg_par.enable[i1])
            {
                if (term_now != i1)
                {
                    BrushOnDark.Color = Windows.UI.Color.FromArgb(200, 0x5C, 0x76, 0x82);
                    (sender as Grid).Background = BrushOnDark;//"#5C7682"
                }
                else
                {
                    BrushOnDark.Color = Windows.UI.Color.FromArgb(50, 0x5C, 0x76, 0x82);
                    (sender as Grid).Background = BrushOnDark;//"#5C7682"
                }
            }

        }

        private void Grid_Post_1_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            string n1 = (sender as Grid).Name;
            n1 = n1.Replace("Grid_Post_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            SolidColorBrush BrushOnDark = new SolidColorBrush();
            i1--;
            if (prg_par.enable[i1])
            {
                if (term_now != i1)
                {
                    BrushOnDark.Color = Windows.UI.Color.FromArgb(255, 0x5C, 0x76, 0x82);
                    (sender as Grid).Background = BrushOnDark;//"#5C7682"
                }
                else
                {
                    BrushOnDark.Color = Windows.UI.Color.FromArgb(0, 0x5C, 0x76, 0x82);
                    (sender as Grid).Background = BrushOnDark;//"#5C7682"
                }
            }


        }
        private void Grid_Post_2_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            string n1 = (sender as Grid).Name;
            n1 = n1.Replace("Grid_Post_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            SolidColorBrush BrushOnDark = new SolidColorBrush();
            i1--;
            if (prg_par.enable[i1])
            {
                if (term_now != i1)
                {
                    BrushOnDark.Color = Windows.UI.Color.FromArgb(200, 0x73, 0x81, 0x88);
                    (sender as Grid).Background = BrushOnDark;//"#738188
                }
                else
                {
                    BrushOnDark.Color = Windows.UI.Color.FromArgb(50, 0x5C, 0x76, 0x82);
                    (sender as Grid).Background = BrushOnDark;//"#5C7682"
                }
            }
        }
        private void Grid_Post_2_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            string n1 = (sender as Grid).Name;
            n1 = n1.Replace("Grid_Post_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            SolidColorBrush BrushOnDark = new SolidColorBrush();
            i1--;
            if (prg_par.enable[i1])
            {
                if (term_now != i1)
                {
                    BrushOnDark.Color = Windows.UI.Color.FromArgb(255, 0x73, 0x81, 0x88);
                    (sender as Grid).Background = BrushOnDark;//"#738188
                }
                else
                {
                    BrushOnDark.Color = Windows.UI.Color.FromArgb(0, 0x5C, 0x76, 0x82);
                    (sender as Grid).Background = BrushOnDark;//"#5C7682"
                }
            }
        }

        private void Grid_Post_1_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            string n1 = (sender as Grid).Name;
            n1 = n1.Replace("Grid_Post_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            if (prg_par.enable[i1 - 1])
            {
                SolidColorBrush BrushOnPress = new SolidColorBrush();
                BrushOnPress.Color = Windows.UI.Color.FromArgb(0, 0x5C, 0x76, 0x82);

                SolidColorBrush BrushOnDark = new SolidColorBrush();
                BrushOnDark.Color = Windows.UI.Color.FromArgb(255, 0x5C, 0x76, 0x82);
                SolidColorBrush BrushOnLight = new SolidColorBrush();
                BrushOnLight.Color = Windows.UI.Color.FromArgb(255, 0x73, 0x81, 0x88);

                Grid_Post_1.Background = BrushOnDark;
                Grid_Post_3.Background = BrushOnDark;
                Grid_Post_5.Background = BrushOnDark;
                Grid_Post_7.Background = BrushOnDark;
                Grid_Post_9.Background = BrushOnDark;
                Grid_Post_2.Background = BrushOnLight;
                Grid_Post_4.Background = BrushOnLight;
                Grid_Post_6.Background = BrushOnLight;
                Grid_Post_8.Background = BrushOnLight;
                Grid_Post_10.Background = BrushOnLight;

                term_now = (ushort)(i1 - 1);
                Text_TermNum.Text = "КОНТРОЛЛЕР: " + i1;
                if (mode == 1)
                {
                    UpdateSettings();
                }
            (sender as Grid).Background = BrushOnPress;//"#5C7682"
                Combo_Post_ID.SelectedIndex = prg_par.selected_id[term_now];

                Arc_Top_Num.Text="КОНТРОЛЛЕР: "+ prg_par.selected_id[term_now];
            }
        }

        private async void btn_Prg_Adr_1_Click(object sender, RoutedEventArgs e)
        {
            string n1 = (sender as Button).Name;
            n1 = n1.Replace("btn_Prg_Adr_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            i1--;
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(prg_par.num[i1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                text = text.Replace(".", ",");
                prg_par.num[i1] = Convert.ToUInt16(text);
                par_flag = true;
                switch (i1)
                {
                    case 0: Prg_Adr_1.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 1: Prg_Adr_2.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 2: Prg_Adr_3.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 3: Prg_Adr_4.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 4: Prg_Adr_5.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 5: Prg_Adr_6.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 6: Prg_Adr_7.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 7: Prg_Adr_8.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 8: Prg_Adr_9.Text = "АДРЕС: " + prg_par.num[i1]; break;
                    case 9: Prg_Adr_10.Text = "АДРЕС: " + prg_par.num[i1]; break;
                }


            }

        }



        private void cb_Term_Enable_1_Checked(object sender, RoutedEventArgs e)
        {
            string n1 = (sender as CheckBox).Name;
            n1 = n1.Replace("cb_Term_Enable_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            i1--;
            prg_par.enable[i1] = true;
            par_flag = true;
        }

        private void cb_Term_Enable_1_Unchecked(object sender, RoutedEventArgs e)
        {
            string n1 = (sender as CheckBox).Name;
            n1 = n1.Replace("cb_Term_Enable_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            i1--;
            prg_par.enable[i1] = false;
            par_flag = true;

        }

        private void Combo_Prg_Ver_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string n1 = (sender as ComboBox).Name;
            n1 = n1.Replace("Combo_Prg_Ver_", "");
            UInt16 i1 = Convert.ToUInt16(n1);
            i1--;
            prg_par.ver[i1] = Convert.ToUInt16((sender as ComboBox).SelectedIndex);
            par_flag = true;

        }

        private async void btnComConnect_Click(object sender, RoutedEventArgs e)
        {
            btnComConnect.IsEnabled = false;
            await serial_port.OpenDevices(ConnectDevices);
            tbModbusStatus.Text = serial_port.status;
            btnComTry.IsEnabled = true;
            if (serial_port.error)
            {
                btnComConnect.IsEnabled = true;
                btnComTry.IsEnabled = false;
            }

        }
        private async void btnComDisonnect_Click(object sender, RoutedEventArgs e)
        {
            btnComConnect.IsEnabled = true;
            await serial_port.ClosePort();
            //tmComReq.Stop();
            if (!serial_port.error)
            {
                DeviceListSource.Source = serial_port.listOfDevices;
            }
            tbModbusStatus.Text = serial_port.status;
        }

        private void btnComTry_Click(object sender, RoutedEventArgs e)
        {
            if (prg_par.ComMode == 1)
            {
                serial_port.wait_answer = 0;
                tmComReq.Start();
                serial_port.rd_mode = 1;
            }
        }
        private void cbComBaud_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            prg_par.ComBaud = cbComBaud.SelectedIndex;
            par_flag = true;
        }

        private void cbComMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            prg_par.ComMode = cbComMode.SelectedIndex;
            par_flag = true;
        }

        private void ConnectDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            prg_par.ComDevice = ConnectDevices.SelectedIndex;
            par_flag = true;
        }

        private void cbComMonitor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            prg_par.ComMonitor = cbComMonitor.SelectedIndex;
            par_flag = true;

        }



        private void Button_Param_Com_Click(object sender, RoutedEventArgs e)
        {
            Grid_Par_Port.Visibility = Visibility.Visible;
            Grid_Par_Term.Visibility = Visibility.Collapsed;

            Grid_Par_Smena.Visibility = Visibility.Collapsed;
            Grid_Param_Smena.Background = BrushOff;

            Grid_Param_Term.Background = BrushOff;
            Grid_Param_Com.Background = BrushOn;
        }

        private void Button_Param_Term_Click(object sender, RoutedEventArgs e)
        {
            Grid_Par_Port.Visibility = Visibility.Collapsed;
            Grid_Par_Term.Visibility = Visibility.Visible;
            Grid_Par_Smena.Visibility = Visibility.Collapsed;
            Grid_Param_Smena.Background = BrushOff;
            Grid_Param_Term.Background = BrushOn;
            Grid_Param_Com.Background = BrushOff;
        }

        private void Button_Param_Read_Click(object sender, RoutedEventArgs e)
        {
            next_adress = (byte)prg_par.num[term_now];

            if (this.Grid_KV_Levels.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет считывание параметров вкладки LEVELS: 0%";
                write_reg_wait = 20;
            }
            if (this.Grid_KV_Feed.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет считывание параметров вкладки FEED: 0%";
                write_reg_wait = 60;
            }
            if (this.Grid_KV_Par.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет считывание параметров вкладки PAR: 0%";
                write_reg_wait = 100;
            }
            if (Grid_KV_Calibr.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 0%";
                write_reg_wait = 140;
            }
            if (Grid_KV_Lite_Calibr.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет считывание параметров вкладки CALIBR: 0%";
                write_reg_wait = 180;
            }
            if (Grid_KV_Lite_Feed.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет считывание параметров вкладки FEED: 0%";
                write_reg_wait = 220;
            }
            if (Grid_KV_Lite_Par.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет считывание параметров вкладки PAR: 0%";
                write_reg_wait = 260;
            }



            reg_status_visible = 1;
            tbParStatus.Visibility = Visibility.Visible;
        }

        private void Button_Param_Write_Click(object sender, RoutedEventArgs e)
        {
            next_adress = (byte)prg_par.num[term_now];

            if (this.Grid_KV_Levels.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет запись параметров вкладки LEVELS: 0%";
                write_reg_wait = 40;
                num_reg_read = 0;
                serial_port.write_reg_fl = (float)kv_par.doze[term_now, 0];
            }
            if (this.Grid_KV_Feed.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет запись параметров вкладки FEED: 0%";
                write_reg_wait = 80;
                num_reg_read = 0;
                serial_port.write_reg_fl = (float)kv_par.dw[term_now, 0];
            }
            if (this.Grid_KV_Par.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет запись параметров вкладки PAR: 0%";
                write_reg_wait = 120;
                num_reg_read = 0;
                serial_port.write_reg_fl = (float)kv_par.tzero[term_now];
            }
            if (Grid_KV_Calibr.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 0%";
                write_reg_wait = 160;
                num_reg_read = 0;
                serial_port.write_reg_int = (UInt16)(kv_par.hz[term_now] * 0x100 + kv_par.mv[term_now]);
            }
            if (Grid_KV_Lite_Calibr.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет запись параметров вкладки CALIBR: 0%";
                write_reg_wait = 200;
                num_reg_read = 0;
                serial_port.write_reg_int = (UInt16)(kv_par.hz[term_now] * 0x100 + kv_par.mv[term_now]);
            }
            if (Grid_KV_Lite_Feed.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет запись параметров вкладки FEED: 0%";
                write_reg_wait = 240;
                num_reg_read = 0;
                serial_port.write_reg_fl = (float)kv_par.lite_stab_weight[term_now]; ;
            }
            if (Grid_KV_Lite_Par.Visibility == Visibility.Visible)
            {
                tbParStatus.Text = "Идет запись параметров вкладки PAR: 0%";
                write_reg_wait = 280;
                num_reg_read = 0;
                serial_port.write_reg_int = (UInt16)(kv_par.f1[term_now] * 0x100 + kv_par.lite_mode[term_now]);
            }
            reg_status_visible = 1;
            tbParStatus.Visibility = Visibility.Visible;

        }

        private async void btn_Tzero_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.tzero[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Tzero.Text = "ВРЕМЯ НУЛЯ: " + text;
                text = text.Replace(",", ".");
                kv_par.tzero[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }
        }


        private async void Button_Doze_2_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Text_Doza_2.Text = "ДОЗА: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 1] = Convert.ToSingle(text);
                write_reg_wait = 40;
                num_reg_read = 0;
                serial_port.write_reg_fl = (float)kv_par.doze[term_now, 0];
                par_flag = true;
            }
        }


        private async void Button_Doze_3_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Text_Doza_3.Text = "ДОЗА: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 2] = Convert.ToSingle(text);
                write_reg_wait = 40;
                num_reg_read = 0;
                serial_port.write_reg_fl = (float)kv_par.doze[term_now, 0];
                par_flag = true;
            }
        }

        private async void Button_Doze_1_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.doze[term_now, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Text_Doza_1.Text = "ДОЗА: " + text;
                text = text.Replace(",", ".");
                kv_par.doze[term_now, 0] = Convert.ToSingle(text);
                write_reg_wait = 40;
                num_reg_read = 0;
                serial_port.write_reg_fl = (float)kv_par.doze[term_now, 0];
                /*                write_reg_wait = 200;
                                num_reg_read = 0;
                                next_adress = (byte)prg_par.num[term_now];
                                serial_port.write_reg_fl = (float)kv_par.dw[term_now, 0];
                                */
                par_flag = true;
            }

        }

        private async void Combo_Post_ID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            prg_par.selected_id[term_now] = Combo_Post_ID.SelectedIndex;
            await prg_par.SaveParFile(prg_filename); par_flag = false;
            UpdatePostEnabled();
        }

        private void Button_Param_Smena_Click(object sender, RoutedEventArgs e)
        {
            Grid_Par_Port.Visibility = Visibility.Collapsed;
            Grid_Par_Term.Visibility = Visibility.Collapsed;

            Grid_Par_Smena.Visibility = Visibility.Visible;
            //Grid_Par_Smena.Background = BrushOn;
            Grid_Param_Smena.Background = BrushOn;
            Grid_Param_Term.Background = BrushOff;
            Grid_Param_Com.Background = BrushOff;


        }

        private void Combo_SmenaHour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            par_flag = true;
            prg_par.SmenaHour = Combo_SmenaHour.SelectedIndex;

        }

        private void Combo_SmenaMinute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            par_flag = true;
            prg_par.SmenaMinute = Combo_SmenaMinute.SelectedIndex;
        }

        private void Combo_SmenaNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            par_flag = true;
            prg_par.SmenaNum = Combo_SmenaNum.SelectedIndex;

        }

        private void Combo_Lite_Polar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.polar[term_now] = (UInt16)Combo_Lite_Polar.SelectedIndex;
            par_flag = true;
        }

        private void Combo_Lite_Discr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.discr[term_now] = (UInt16)Combo_Lite_Discr.SelectedIndex;
            par_flag = true;
        }

        private void Combo_Lite_Zpt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.zpt[term_now] = (UInt16)Combo_Lite_Zpt.SelectedIndex;
            par_flag = true;
        }

        private void Combo_Lite_Hz_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.hz[term_now] = (UInt16)Combo_Lite_Hz.SelectedIndex;
            par_flag = true;
        }

        private void Combo_Lite_Mv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.mv[term_now] = (UInt16)Combo_Lite_Mv.SelectedIndex;
            par_flag = true;

        }

        private async void btn_Lite_CalZero_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.lite_zero_weight[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Cal_Lite_Zero.Text = "СМЕЩЕНИЕ НУЛЯ: " + text;
                text = text.Replace(".", ",");
                kv_par.lite_zero_weight[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }
        }

        private async void btn_Lite_Stab_Weight_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.lite_stab_weight[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Lite_StabWeigth.Text = "ДИАПАЗОН СТАБИЛЬНОГО ВЕСА: " + text;
                text = text.Replace(".", ",");
                kv_par.lite_stab_weight[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private void Combo_Lite_FStab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.lite_stab_f1[term_now] = (UInt16)Combo_Lite_FStab.SelectedIndex;
            par_flag = true;
        }

        private async void btn_Lite_Zero_Time_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.lite_tzero[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Lite_Zero_Time.Text = "ВРЕМЯ УСТАНОВКИ НУЛЯ: " + text;
                text = text.Replace(".", ",");
                kv_par.lite_tzero[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Lite_Zero_Weight_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.lite_w_zero[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Lite_Zero_Weight.Text = "ДИАПАЗОН НУЛЕВОГО ВЕСА: " + text;
                text = text.Replace(".", ",");
                kv_par.lite_w_zero[term_now] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private void Combo_Lite_Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.lite_mode[term_now] = (UInt16)Combo_Lite_Mode.SelectedIndex;
            par_flag = true;
        }

        private void Combo_Lite_F1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.f1[term_now] = (UInt16)Combo_Lite_F1.SelectedIndex;
            par_flag = true;

        }

        private void Combo_Lite_F2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.f2[term_now] = (UInt16)Combo_Lite_F2.SelectedIndex;
            par_flag = true;

        }

        private void Combo_Lite_Direction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.lite_direction[term_now] = (UInt16)Combo_Lite_Direction.SelectedIndex;
            par_flag = true;

        }

        private void Combo_Lite_Baud_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.baud[term_now] = (UInt16)Combo_Lite_Baud.SelectedIndex;
            par_flag = true;

        }

        private async void btn_Lite_Num_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(kv_par.num[term_now]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Lite_Num.Text = "СЕТЕВОЙ НОМЕР: " + text;
                text = text.Replace(".", ",");
                kv_par.num[term_now] = Convert.ToUInt16(text);
                par_flag = true;
            }

        }

        private void Combo_Lite_PointNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            kv_par.lite_point_num[term_now]= (UInt16)Combo_Lite_PointNum.SelectedIndex;
            par_flag = true;
        }

        private void Button_Arc_Arc_Click(object sender, RoutedEventArgs e)
        {
            Grid_Arc_Main.Background = BrushOn;
            Grid_Arc_Product.Background = BrushOff;
            Grid_Arc_Prod.Visibility = Visibility.Collapsed;
            Grid_Arc_Arc.Visibility = Visibility.Visible;

        }

        private void Button_Arc_Prod_Click(object sender, RoutedEventArgs e)
        {
            Grid_Arc_Main.Background = BrushOff;
            Grid_Arc_Product.Background = BrushOn;
            Grid_Arc_Prod.Visibility = Visibility.Visible;
            Grid_Arc_Arc.Visibility = Visibility.Collapsed;


        }

        private void Combo_Arc_ProdId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Set_Doza1.Text = "" + products.set_doza[Combo_Arc_ProdId.SelectedIndex,0];
            Set_Doza2.Text = "" + products.set_doza[Combo_Arc_ProdId.SelectedIndex,1];
            Set_Doza3.Text = "" + products.set_doza[Combo_Arc_ProdId.SelectedIndex,2];
            Min_Doza1.Text = "" + products.min_doza[Combo_Arc_ProdId.SelectedIndex,0];
            Min_Doza2.Text = "" + products.min_doza[Combo_Arc_ProdId.SelectedIndex,1];
            Min_Doza3.Text = "" + products.min_doza[Combo_Arc_ProdId.SelectedIndex,2];
            Max_Doza1.Text = "" + products.max_doza[Combo_Arc_ProdId.SelectedIndex,0];
            Max_Doza2.Text = "" + products.max_doza[Combo_Arc_ProdId.SelectedIndex,1];
            Max_Doza3.Text = "" + products.max_doza[Combo_Arc_ProdId.SelectedIndex,2];
        }


        private async void btn_Set_Doza1_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.set_doza[Combo_Arc_ProdId.SelectedIndex, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Set_Doza1.Text = "" + text;
                text = text.Replace(".", ",");
                products.set_doza[Combo_Arc_ProdId.SelectedIndex, 0] = Convert.ToSingle(text);
                par_flag = true;
            }


        }

        private async void btn_Set_Doza2_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.set_doza[Combo_Arc_ProdId.SelectedIndex, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Set_Doza2.Text = "" + text;
                text = text.Replace(".", ",");
                products.set_doza[Combo_Arc_ProdId.SelectedIndex, 1] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Set_Doza3_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.set_doza[Combo_Arc_ProdId.SelectedIndex, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Set_Doza3.Text = "" + text;
                text = text.Replace(".", ",");
                products.set_doza[Combo_Arc_ProdId.SelectedIndex, 2] = Convert.ToSingle(text);
                par_flag = true;
            }

        }
        private async void btn_Min_Doza1_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.min_doza[Combo_Arc_ProdId.SelectedIndex, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Min_Doza1.Text = "" + text;
                text = text.Replace(".", ",");
                products.min_doza[Combo_Arc_ProdId.SelectedIndex, 0] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Min_Doza2_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.min_doza[Combo_Arc_ProdId.SelectedIndex, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Min_Doza2.Text = "" + text;
                text = text.Replace(".", ",");
                products.min_doza[Combo_Arc_ProdId.SelectedIndex, 1] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Min_Doza3_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.min_doza[Combo_Arc_ProdId.SelectedIndex, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Min_Doza3.Text = "" + text;
                text = text.Replace(".", ",");
                products.min_doza[Combo_Arc_ProdId.SelectedIndex, 2] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Max_Doza1_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.max_doza[Combo_Arc_ProdId.SelectedIndex, 0]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Max_Doza1.Text = "" + text;
                text = text.Replace(".", ",");
                products.max_doza[Combo_Arc_ProdId.SelectedIndex, 0] = Convert.ToSingle(text);
                par_flag = true;
            }
        }

        private async void btn_Max_Doza2_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.max_doza[Combo_Arc_ProdId.SelectedIndex, 1]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Max_Doza2.Text = "" + text;
                text = text.Replace(".", ",");
                products.max_doza[Combo_Arc_ProdId.SelectedIndex, 1] = Convert.ToSingle(text);
                par_flag = true;
            }

        }

        private async void btn_Max_Doza3_Click(object sender, RoutedEventArgs e)
        {
            var InputDlg = new Input_Num_Dialog();
            InputDlg.Text = Convert.ToString(products.max_doza[Combo_Arc_ProdId.SelectedIndex, 2]);
            var result = await InputDlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = InputDlg.Text;
                Max_Doza3.Text = "" + text;
                text = text.Replace(".", ",");
                products.max_doza[Combo_Arc_ProdId.SelectedIndex, 2] = Convert.ToSingle(text);
                par_flag = true;
            }

        }
    }
}
