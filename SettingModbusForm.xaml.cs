using ModbusSyncStructLIb.Settings;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        List<string> type_ModbusList;
        StructSyncTest structSync;
        private static Logger logger;

        public SettingModbusForm(StructSyncTest structSync)
        {
            var loggerconf = new XmlLoggingConfiguration("NLog.config");
            logger = LogManager.GetCurrentClassLogger();


            this.WindowStartupLocation = structSync.WindowStartupLocation;

            InitializeComponent();
            this.structSync = structSync;
            typeParitylist = new List<string>();
            typeStopBitslist = new List<string>();
            type_ModbusList = new List<string>();

            typeParitylist.Add("None");
            typeParitylist.Add("Even");
            typeParitylist.Add("Mark");
            typeParitylist.Add("Odd");
            typeParitylist.Add("Space");

            typeStopBitslist.Add("None");
            typeStopBitslist.Add("One");
            typeStopBitslist.Add("OnePointFive");
            typeStopBitslist.Add("Two");

            type_ModbusList.Add("RTU");
            type_ModbusList.Add("ASCII");
            type_ModbusList.Add("TCP");

            ListTypePartyComboBox.ItemsSource = typeParitylist;
            ListTypeStopbitsComboBox.ItemsSource = typeStopBitslist;

            TypeModbuscB.ItemsSource = type_ModbusList;

            TypeModbuscB.SelectedIndex = 0;

            ListTypePartyComboBox.SelectedIndex = 0;
            ListTypeStopbitsComboBox.SelectedIndex = 0;

            TypeViewModbus.SelectedIndex = 0;

            tab2.Visibility = Visibility.Hidden;
            loadSettings();

            if (structSync.radioButton.IsChecked==true)
            {
                MasterRB.IsChecked = true;
            }
            if (structSync.radioButton1.IsChecked == true)
            {
                SlaveRB.IsChecked = true;
            }

        }

        private void loadSettings()
        {
            var path = System.IO.Path.GetFullPath(@"Settingsmodbus.xml");

            if (File.Exists(path)==true)
            {
                try
                {
                    SettingsModbus settings;
                    // десериализация
                    using (FileStream fs = new FileStream("Settingsmodbus.xml", FileMode.Open))
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
                    slaveID_txt.Text = settings.slaveID.ToString();
                    DeltaTime_txt.Text = settings.deltatime.ToString();

                    try_reboot_connection_сh.IsChecked = settings.try_reboot_connection;

                    foreach (var pt in typeParitylist)
                    {
                        if (pt == settings.Party_type_str)
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

                    foreach (var pt in type_ModbusList)
                    {
                        if (pt == settings.typeModbusSTR)
                        {
                            int index = type_ModbusList.IndexOf(pt);
                            TypeModbuscB.SelectedIndex = index;

                            if (index == 2)
                            {
                                tab1.Visibility = Visibility.Hidden;
                                tab2.Visibility = Visibility.Visible;
                                TypeViewModbus.SelectedIndex = 1;
                            }
                            else
                            {
                                tab2.Visibility = Visibility.Hidden;
                                tab1.Visibility = Visibility.Visible;
                                TypeViewModbus.SelectedIndex = 0;
                            }
                        }
                    }

                    if (settings.defaulttypemodbus == 0)
                    {
                        MasterRB.IsChecked = true;
                    }
                    if (settings.defaulttypemodbus == 1)
                    {
                        SlaveRB.IsChecked = true;
                    }
                }
                catch(Exception ex)
                {
                    logger.Error(ex);
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
            try
            {
                string regexCOM = @"Com\d";

                string regex_decre = @"\d";

                string regex_double = @"\d";

                //string iprex = @"\d{3}.";
                Regex iprex = new Regex(@"[0-3][0-9][0-9].[0-3][0-9][0-9].[0-3][0-9][0-9].[0-3][0-9][0-9]");


                if (Regex.IsMatch(Com_name_txb.Text, regexCOM, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    Console.WriteLine("Введите правильный Com");
                    logger.Warn("Введите правильный Com");
                    return;
                }

                if (Regex.IsMatch(BaudRate_txb.Text, regex_decre, RegexOptions.IgnoreCase))
                {
                    
                }
                else
                {
                    Console.WriteLine("ВВ");
                    logger.Warn("Введите Цифры в BaudRate");
                    return;
                }

                if (Regex.IsMatch(DataBits_lb_txb.Text, regex_decre, RegexOptions.IgnoreCase))
                {
                    
                }
                else
                {
                    Console.WriteLine("ВВ");
                    logger.Warn("Введите правильный DataBits");
                    return;
                }


                if (Regex.IsMatch(ReadTimeout_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    logger.Warn("Введите цифры");
                    return;
                }

                if (Regex.IsMatch(WriteTimeout_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    logger.Warn("Введите цифры");
                    return;
                }

                if (Regex.IsMatch(slaveID_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    logger.Warn("Введите цифры");
                    return;
                }

                if (Regex.IsMatch(DeltaTime_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    logger.Warn("Введите цифры");
                    return;
                }

                if (Regex.IsMatch(Port_lb_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    logger.Warn("Введите цифры");
                    return;
                }


                try
                {
                    string[] words = IpAdressLb_txt.Text.Split('.');
                    foreach (var word in words)
                    {
                        if (Convert.ToInt32(word) > 255)
                        {
                            logger.Warn("Введите правильный ip");
                            return;
                        }
                    }
                }
                catch(Exception ex)
                {
                    logger.Warn("Введите правильный ip");
                    return;
                }
                


                string selectedItem = (string)ListTypePartyComboBox.SelectedItem;

                string selectedItem2 = (string)ListTypeStopbitsComboBox.SelectedItem;

                string selectedItem3 = (string)TypeModbuscB.SelectedItem;

                int Type_modbus_choose = (int)TypeModbuscB.SelectedIndex;

                int defaulttypemodbus = 0;
                if (MasterRB.IsChecked == true)
                {
                    defaulttypemodbus = 0;
                }
                if (SlaveRB.IsChecked == true)
                {
                    defaulttypemodbus = 1;
                }

                SettingsModbus settings = new SettingsModbus(Com_name_txb.Text, Convert.ToInt32(BaudRate_txb.Text), Convert.ToInt32(DataBits_lb_txb.Text), selectedItem, selectedItem2, Convert.ToInt32(ReadTimeout_txt.Text), Convert.ToInt32(WriteTimeout_txt.Text), IpAdressLb_txt.Text, Convert.ToInt32(Port_lb_txt.Text), Type_modbus_choose, Convert.ToByte(slaveID_txt.Text), selectedItem3, defaulttypemodbus, Convert.ToDouble(DeltaTime_txt.Text), try_reboot_connection_сh.IsChecked.Value);
                XmlSerializer formatter = new XmlSerializer(typeof(SettingsModbus));

                // получаем поток, куда будем записывать сериализованный объект
                using (FileStream fs = new FileStream("Settingsmodbus.xml", FileMode.Create))
                {
                    formatter.Serialize(fs, settings);

                    Console.WriteLine("Объект сериализован");
                }
                this.Hide();

                structSync.updateradio();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            } 
        }

        private void comboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TypeModbuscB.SelectedIndex==2)
            {
                TypeViewModbus.SelectedIndex = 1;
                tab2.Visibility = Visibility.Visible;
                tab1.Visibility = Visibility.Hidden;
            }
            else
            {
                TypeViewModbus.SelectedIndex = 0;
                tab1.Visibility = Visibility.Visible;
                tab2.Visibility = Visibility.Hidden;
            }
        }

        private void Com_name_txb_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BaudRate_txb_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
