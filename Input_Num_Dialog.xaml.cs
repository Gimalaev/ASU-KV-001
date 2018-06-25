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

// Документацию по шаблону элемента "Диалоговое окно содержимого" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace ASU_KV_001
{


    public sealed partial class Input_Num_Dialog : ContentDialog
    {

        private double r1, r2;

        public string Text
        {
            get { return (string)tbCalc.Text; }
            set { tbCalc.Text = value; }
        }




        //Для калькулятора
        private void Calc_Press(int i)
        {
            string s = this.tbCalc.Text.Replace(".", ",");
            if (s.Length <= 7)
            {
                if ((Convert.ToDouble(s) != 0) || (s.Contains(',')))
                    s += Convert.ToString(i);
                else s = Convert.ToString(i);
            }
            this.tbCalc.Text = s.Replace(",", ".");
            r1 = Convert.ToDouble(s);
        }

        public Input_Num_Dialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            r2 = r1;
        }

        private void btn_7_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(7);
        }

        private void btn_8_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(8);
        }

        private void btn_9_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(9);
        }

        private void btn_4_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(4);
        }

        private void btn_5_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(5);
        }

        private void btn_6_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(6);
        }

        private void btn_1_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(1);
        }

        private void btn_2_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(2);
        }

        private void btn_3_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(3);
        }

        private void btn_0_Click(object sender, RoutedEventArgs e)
        {
            Calc_Press(0);
        }

        private void btn_Del_Click(object sender, RoutedEventArgs e)
        {
            string s;
            s = this.tbCalc.Text;
            s = s.Substring(0, s.Length - 1);
            if (s.Length == 0) s = "0";
            this.tbCalc.Text = s;
        }

        private void btn_Dot_Click(object sender, RoutedEventArgs e)
        {
            string s;
            //s = this.tbCalc.Text;
            s = this.tbCalc.Text;
            if (!s.Contains('.'))
                s += ".";
            this.tbCalc.Text = s;
        }

        private void Dlg_Numeric_Input_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // this.tbCalc.Text = e.Key.ToString();
            // if (e.Key == Key.L) Calc_Press(0);
            if ((e.Key.ToString() == "Number0") || (e.Key.ToString() == "NumberPad0")) Calc_Press(0);
            if ((e.Key.ToString() == "Number1") || (e.Key.ToString() == "NumberPad1")) Calc_Press(1);
            if ((e.Key.ToString() == "Number2") || (e.Key.ToString() == "NumberPad2")) Calc_Press(2);
            if ((e.Key.ToString() == "Number3") || (e.Key.ToString() == "NumberPad3")) Calc_Press(3);
            if ((e.Key.ToString() == "Number4") || (e.Key.ToString() == "NumberPad4")) Calc_Press(4);
            if ((e.Key.ToString() == "Number5") || (e.Key.ToString() == "NumberPad5")) Calc_Press(5);
            if ((e.Key.ToString() == "Number6") || (e.Key.ToString() == "NumberPad6")) Calc_Press(6);
            if ((e.Key.ToString() == "Number7") || (e.Key.ToString() == "NumberPad7")) Calc_Press(7);
            if ((e.Key.ToString() == "Number8") || (e.Key.ToString() == "NumberPad8")) Calc_Press(8);
            if ((e.Key.ToString() == "Number9") || (e.Key.ToString() == "NumberPad9")) Calc_Press(9);
            if (e.Key.ToString() == "Delete") btn_Del_Click(sender, e);
            //if (e.Key.ToString() == "Back") btn_Del_Click(sender, e); //Почемуто два раза срабатывает
            if ((e.Key.ToString() == "191") || (e.Key.ToString() == "Decimal")) btn_Dot_Click(sender, e);

        }

        private void Dlg(object sender, KeyRoutedEventArgs e)
        {

        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {

            //this.Content = tbCalc;
        }
    }
}
