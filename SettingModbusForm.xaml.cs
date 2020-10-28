using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ModbusSynchFormTest
{
    /// <summary>
    /// Логика взаимодействия для SettingModbusForm.xaml
    /// </summary>
    public partial class SettingModbusForm : Window
    {
        List<string> typeParitylist;
        List<string> typeStopBitslist;
        public SettingModbusForm()
        {
            InitializeComponent();
            typeParitylist = new List<string>();
            typeStopBitslist = new List<string>();
            
            typeParitylist.Add("None");
            typeParitylist.Add("Even");
            typeParitylist.Add("Mark");
            typeParitylist.Add("Odd");
            typeParitylist.Add("Space");

            typeStopBitslist.Add("None");
            typeStopBitslist.Add("One");
            typeStopBitslist.Add("OnePointFive");
            typeStopBitslist.Add("Two");

            ListTypePartyComboBox.ItemsSource = typeParitylist;
            ListTypeStopbitsComboBox.ItemsSource = typeStopBitslist;
        }

        /// <summary>
        /// Применить настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Apply_btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
