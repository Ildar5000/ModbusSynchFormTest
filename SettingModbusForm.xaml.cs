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

        List<string> Listtime_check_connection;

        List<string> type_ModbusList;

        List<string> time_rebootaftercrash = new List<string>();

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

            //Parity
            typeParitylist.Add("None");
            typeParitylist.Add("Even");
            typeParitylist.Add("Mark");
            typeParitylist.Add("Odd");
            typeParitylist.Add("Space");
            ListTypePartyComboBox.ItemsSource = typeParitylist;
            ListTypePartyComboBox.SelectedIndex = 0;


            //typeStopBitslist.Add("None");
            typeStopBitslist.Add("One");
            typeStopBitslist.Add("OnePointFive");
            typeStopBitslist.Add("Two");


            //type modbus
            type_ModbusList.Add("RTU");
            type_ModbusList.Add("ASCII");
            type_ModbusList.Add("TCP");
            TypeModbuscB.ItemsSource = type_ModbusList;
            TypeModbuscB.SelectedIndex = 0;
            TypeViewModbus.SelectedIndex = 0;
            ListTypeStopbitsComboBox.ItemsSource = typeStopBitslist;
            ListTypeStopbitsComboBox.SelectedIndex = 0;
            tab2.Visibility = Visibility.Hidden;

            // время опрооса
            Listtime_check_connection = new List<string>();
            Listtime_check_connection.Add("Никогда");
            Listtime_check_connection.Add("1 секунда");
            Listtime_check_connection.Add("2 секунуды");
            Listtime_check_connection.Add("5 секунуд");
            Listtime_check_connection.Add("10 секунуд");
            TimeDeltacheckCH.ItemsSource = Listtime_check_connection;
            TimeDeltacheckCH.SelectedIndex = 2;
            ///

            time_rebootaftercrash = new List<string>();

            time_rebootaftercrash.Add("30 секунд");
            time_rebootaftercrash.Add("1 минута");
            time_rebootaftercrash.Add("2 минуты");
            time_rebootaftercrash.Add("5 минут");
            RebootConnection_after_crash_cb.ItemsSource = time_rebootaftercrash;
            RebootConnection_after_crash_cb.SelectedIndex = 0;

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


                    int selectedtimecheck = settings.deltatimeManager;

                    try_reboot_connection_сh.IsChecked = settings.try_reboot_connection;
                    bool check_connection= settings.try_reboot_connection;
                    switch (selectedtimecheck)
                    {
                        case 0:
                            TimeDeltacheckCH.SelectedIndex = 0;
                            break;

                        case 1000:
                            TimeDeltacheckCH.SelectedIndex = 1;
                            break;

                        case 2000:
                            if (check_connection==false)
                            {
                                TimeDeltacheckCH.SelectedIndex = 0;
                            }
                            
                            break;
                        case 5000:
                            TimeDeltacheckCH.SelectedIndex = 3;
                            break;
                        case 10000:
                            TimeDeltacheckCH.SelectedIndex = 4;
                            break;

                        default:
                            TimeDeltacheckCH.SelectedIndex = 2;
                            break;
                    }


                    int selectedtimecheckaftercrash_int = settings.tryconnectionaftercrash;

                    //time_rebootaftercrash.Add("30 секунд");
                    //time_rebootaftercrash.Add("1 минута");
                    //time_rebootaftercrash.Add("2 минуты");
                    //time_rebootaftercrash.Add("5 минут");

                    
                    switch (selectedtimecheckaftercrash_int)
                    {
                        case 3 * 1000:
                            RebootConnection_after_crash_cb.SelectedIndex = 0;
                            break;
                        case 60 * 1000:
                            RebootConnection_after_crash_cb.SelectedIndex = 1;
                            break;
                        case 2 * 60 * 1000:
                            RebootConnection_after_crash_cb.SelectedIndex = 2;
                            break;
                        case 5 * 60 * 1000:
                            RebootConnection_after_crash_cb.SelectedIndex = 3;
                            break;
                        default:
                            RebootConnection_after_crash_cb.SelectedIndex = 0;
                            break;
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

                #region checkuserwrite
                string regexCOM = @"Com\d";

                string regex_decre = @"\d";

                //string iprex = @"\d{3}.";
                Regex iprex = new Regex(@"[0-3][0-9][0-9].[0-3][0-9][0-9].[0-3][0-9][0-9].[0-3][0-9][0-9]");


                if (Regex.IsMatch(Com_name_txb.Text, regexCOM, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Введите правильный Com", "My App", MessageBoxButton.OK);
                    Console.WriteLine("Введите правильный Com");
                    logger.Warn("Введите правильный Com");
                    return;
                }

                if (Regex.IsMatch(BaudRate_txb.Text, regex_decre, RegexOptions.IgnoreCase))
                {
                    
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Введите Цифры в BaudRate", "My App", MessageBoxButton.OK);
                    logger.Warn("Введите Цифры в BaudRate");
                    return;
                }

                if (Regex.IsMatch(DataBits_lb_txb.Text, regex_decre, RegexOptions.IgnoreCase))
                {
                    
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Введите Цифры в DataBits", "My App", MessageBoxButton.OK);
                    logger.Warn("Введите правильный DataBits");
                    return;
                }


                if (Regex.IsMatch(ReadTimeout_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Введите цифры", "My App", MessageBoxButton.OK);
                    logger.Warn("Введите цифры");
                    return;
                }

                if (Regex.IsMatch(WriteTimeout_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Введите цифры", "My App", MessageBoxButton.OK);
                    logger.Warn("Введите цифры");
                    return;
                }

                if (Regex.IsMatch(slaveID_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Введите цифры", "My App", MessageBoxButton.OK);
                    logger.Warn("Введите цифры");
                    return;
                }

                if (Regex.IsMatch(DeltaTime_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Введите цифры", "My App", MessageBoxButton.OK);
                    logger.Warn("Введите цифры");
                    return;
                }

                if (Regex.IsMatch(Port_lb_txt.Text, regex_decre, RegexOptions.IgnoreCase))
                {

                }
                else
                {
                    MessageBoxResult result = MessageBox.Show("Введите цифры", "My App", MessageBoxButton.OK);
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
                            MessageBoxResult result = MessageBox.Show("Введите правильный ip", "My App", MessageBoxButton.OK);
                            logger.Warn("Введите правильный ip");
                            return;
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBoxResult result = MessageBox.Show("Введите правильный ip", "My App", MessageBoxButton.OK);
                    logger.Warn("Введите правильный ip");
                    return;
                }
                #endregion

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

                int timedelta = 2 * 1000;
                //check manager
                #region check manager

                bool check_connection = true;

                string selectedtimecheck = (string)TimeDeltacheckCH.SelectedItem;

                switch (selectedtimecheck)
                {
                    case "Никогда":
                        check_connection = false;
                        timedelta = 2 * 1000;
                        break;

                    case "1 секунда":
                        check_connection = true;
                        timedelta = 1 * 1000;
                        break;

                    case "2 секунуды":
                        check_connection = true;
                        timedelta = 2 * 1000;
                        break;
                    case "5 секунуд":
                        check_connection = true;
                        timedelta = 5 * 1000;
                        break;
                    case "10 секунуд":
                        check_connection = true;
                        timedelta = 10 * 1000;
                        break;

                    default:
                        check_connection = true;
                        timedelta = 2 * 1000;
                        break;
                }
                #endregion

                //time_rebootaftercrash.Add("30 секунд");
                //time_rebootaftercrash.Add("1 минута");
                //time_rebootaftercrash.Add("2 минута");
                //time_rebootaftercrash.Add("5 минут");

                string selectedtimecheckaftercrash = (string)RebootConnection_after_crash_cb.SelectedItem;
                int selectedtimecheckaftercrash_int=3000;
                switch (selectedtimecheckaftercrash)
                {
                    case "30 секунд":
                        selectedtimecheckaftercrash_int = 3 * 1000;
                        break;
                    case "1 минута":
                        selectedtimecheckaftercrash_int = 60 * 1000;
                        break;
                    case "2 минуты":
                        selectedtimecheckaftercrash_int = 2*60 * 1000;
                        break;
                    case "5 минут":
                        selectedtimecheckaftercrash_int = 5 * 60 * 1000;
                        break;
                    default:
                        selectedtimecheckaftercrash_int = 3 * 1000;
                        break;
                }



                SettingsModbus settings = new SettingsModbus(Com_name_txb.Text, Convert.ToInt32(BaudRate_txb.Text), 
                    Convert.ToInt32(DataBits_lb_txb.Text), selectedItem, selectedItem2, 
                    Convert.ToInt32(ReadTimeout_txt.Text), Convert.ToInt32(WriteTimeout_txt.Text), 
                    IpAdressLb_txt.Text, Convert.ToInt32(Port_lb_txt.Text), 
                    Type_modbus_choose, Convert.ToByte(slaveID_txt.Text), 
                    selectedItem3, defaulttypemodbus, Convert.ToDouble(DeltaTime_txt.Text), 
                    check_connection, Convert.ToInt32(timedelta),
                    selectedtimecheckaftercrash_int
                    );
                XmlSerializer formatter = new XmlSerializer(typeof(SettingsModbus));

                // получаем поток, куда будем записывать сериализованный объект
                using (FileStream fs = new FileStream("Settingsmodbus.xml", FileMode.Create))
                {
                    formatter.Serialize(fs, settings);

                    logger.Info("Кофигурация создана");
                }
                this.Hide();

                structSync.UpdateRadio();
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

        private void MasterRB_Checked(object sender, RoutedEventArgs e)
        {
            IpAdress_Lb.Content = "IP Adress Slave";
        }

        private void SlaveRB_Checked(object sender, RoutedEventArgs e)
        {
            IpAdress_Lb.Content = "IP Adress Slave";
        }
    }
}
