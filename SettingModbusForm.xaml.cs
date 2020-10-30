using ModbusSyncStructLIb.Settings;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Serialization;

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

            loadSettings();

        }

        private void loadSettings()
        {
            var path = System.IO.Path.GetFullPath(@"Settingsmodbus.xml");

            if (File.Exists(path)==true)
            {
                SettingsModbus settings;
                // десериализация
                using (FileStream fs = new FileStream("Settingsmodbus.xml", FileMode.OpenOrCreate))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(SettingsModbus));
                    settings = (SettingsModbus)formatter.Deserialize(fs);

                    Console.WriteLine("Объект десериализован");
                }
                Com_name_txb.Text = settings.ComName;
                BaudRate_txb.Text = settings.BoudRate.ToString();
                DataBits_lb_txb.Text = settings.DataBits.ToString();
                ReadTimeout_txt.Text = settings.ReadTimeout.ToString();
                WriteTimeout_txt.Text = settings.WriteTimeout.ToString();
                IpAdressLb_txt.Text = settings.IP_client;
                Port_lb_txt.Text = settings.port_IP_client.ToString();
                Type_modbus_txt.Text = settings.typeModbus.ToString();
                slaveID_txt.Text = settings.slaveID.ToString();

                foreach (var pt in typeParitylist)
                {
                    if (pt==settings.Party_type_str)
                    {
                        int index = typeParitylist.IndexOf(pt);
                        ListTypePartyComboBox.SelectedIndex = index;
                    }
                }

                foreach (var pt in typeStopBitslist)
                {
                    if (pt == settings.StopBits_type_str)
                    {
                        int index = typeStopBitslist.IndexOf(pt);
                        ListTypeStopbitsComboBox.SelectedIndex = index;
                    }
                }


            }
            else
            {
                Console.WriteLine("Создайте файл ");
            }
        }

        /// <summary>
        /// Применить настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Apply_btn_Click(object sender, RoutedEventArgs e)
        {

            
            string selectedItem = (string)ListTypePartyComboBox.SelectedItem;

            string selectedItem2 = (string)ListTypeStopbitsComboBox.SelectedItem;

            SettingsModbus settings = new SettingsModbus(Com_name_txb.Text,Convert.ToInt32(BaudRate_txb.Text),Convert.ToInt32(DataBits_lb_txb.Text), selectedItem, selectedItem2, Convert.ToInt32(ReadTimeout_txt.Text),Convert.ToInt32(WriteTimeout_txt.Text), IpAdressLb_txt.Text, Convert.ToInt32(Port_lb_txt.Text), Convert.ToInt32(Type_modbus_txt.Text), Convert.ToByte(slaveID_txt.Text));
            XmlSerializer formatter = new XmlSerializer(typeof(SettingsModbus));

            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream("Settingsmodbus.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, settings);

                Console.WriteLine("Объект сериализован");
            }

        }
    }
}
