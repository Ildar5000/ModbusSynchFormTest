using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;
using ModbusSyncStructLIb;
using ModbusSyncStructLIb.EvenBase;
using NLog;
using NLog.Config;
using StructAllforTest;
using ModbusSyncStructLIb.Settings;
using Microsoft.Win32;
using System.Diagnostics;
using ModbusSyncStructLIb.CheckConnect;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ModbusSynchFormTest
{
    /// <summary>
    /// Логика взаимодействия для StructSyncTest.xaml
    /// </summary>
    public partial class StructSyncTest : Window
    {
        private static Logger logger;
        Thread statusbar;

        Thread diagnostik_show;
        Task sendfile;

        bool StopOrStart = false;

        List<string> pathFiles = new List<string>();

        List<string> pathFolder = new List<string>();

        bool timeprocessing_file = false;

        #region Base Modbus
        MasterSyncStruct masterSyncStruct;
        SlaveSyncSruct slaveSyncSruct;

        ManagerConnectionModbus managerConnectionModbus;


        #endregion

        #region struct
        MMS ms;
        VMS vc;
        #endregion

        #region metaclass
        MetaClassForStructAndtherData metaClassFor;
        QueueOfSentMessagesForSlave queueOf;
        string nameFile;
        #endregion

        #region settings

        int timecheckconnection = 2000;
        #endregion


        #region init
        public StructSyncTest()
        {
            InitializeComponent();
            var loggerconf = new XmlLoggingConfiguration("NLog.config");
            logger = LogManager.GetCurrentClassLogger();
            queueOf = new QueueOfSentMessagesForSlave();
            loadsetings();
            this.WindowStartupLocation= System.Windows.WindowStartupLocation.CenterScreen;
            HaveConnectionlbSignal.Fill = Brushes.Red;

            StopTransfer.Visibility = Visibility.Hidden;
            logCtrl.LevelWidth = 50;
            logCtrl.MessageWidth = 500;

        }


        private void loadsetings()
        {
            try
            {
                var path = System.IO.Path.GetFullPath(@"Settingsmodbus.xml");
                if (File.Exists(path) == true)
                {
                    using (FileStream fs = new FileStream("Settingsmodbus.xml", FileMode.Open))
                    {
                        XmlSerializer formatter = new XmlSerializer(typeof(SettingsModbus));
                        var msload = (SettingsModbus)formatter.Deserialize(fs);
                        if (msload.defaulttypemodbus==0)
                        {
                            radioButton.IsChecked = true;
                        }
                        if (msload.defaulttypemodbus == 1)
                        {
                            radioButton1.IsChecked = true;
                        }
                        timecheckconnection = msload.deltatimeManager;

                        logger.Info("Данные загружены");
                    }
                }



                var pathVMS = System.IO.Path.GetFullPath(@"dataSaveStruct2.xml");

                var pathMMS = System.IO.Path.GetFullPath(@"dataSaveStruct1.xml");

                if (File.Exists(pathMMS) == true)
                {
                    MMS msload;
                    // десериализация
                    using (FileStream fs = new FileStream("dataSaveStruct1.xml", FileMode.Open))
                    {
                        XmlSerializer formatter = new XmlSerializer(typeof(MMS));
                        msload = (MMS)formatter.Deserialize(fs);

                        logger.Info("Данные загружены");
                    }

                    textBox1.Text = msload.testSendStruct.ab;
                    textBox2.Text = msload.testSendStruct.cd;
                    textBox3.Text = msload.testSendStruct.fre;
                    textBox4.Text = msload.testSendStruct.name;
                }

                if (File.Exists(pathVMS) == true)
                {
                    VMS VMSload;
                    // десериализация
                    using (FileStream fs = new FileStream("dataSaveStruct2.xml", FileMode.Open))
                    {
                        XmlSerializer formatter = new XmlSerializer(typeof(VMS));
                        VMSload = (VMS)formatter.Deserialize(fs);

                        logger.Info("Данные загружены");
                    }

                    textBox5.Text = VMSload.test4SendStruct.ab;
                    textBox6.Text = VMSload.test4SendStruct.cd;
                    textBox7.Text = VMSload.test4SendStruct.fre;
                    textBox8.Text = VMSload.test4SendStruct.name;

                    for (int i = 0; i < VMSload.test4SendStruct.count2.Length; i++)
                    {
                        richTextBox.AppendText(VMSload.test4SendStruct.count2[i].ToString());
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);

            }
            
        }



        #endregion

        Thread porok;
        Thread managerConnection;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (StopOrStart == false)
            {
                StopOrStart = true;
                stoptransfer = false;
                button.Content = "Стоп";
                var path = System.IO.Path.GetFullPath(@"Settingsmodbus.xml");
                diagnostik_show = new Thread(CheckConnection);
                diagnostik_show.Start();
                if (radioButton.IsChecked == true)
                {
                    //Master
                    try
                    {
                        HideButtonsIfConnectionMaster();
                        if (File.Exists(path) == true)
                        {
                            logger.Info("Создание мастера");
                            masterSyncStruct = new MasterSyncStruct();

                            //masterSyncStruct.Open();

                            //thread = new Thread(masterSyncStruct.Open);
                            //thread.Start();

                            managerConnectionModbus = new ManagerConnectionModbus(masterSyncStruct);
                            managerConnection = new Thread(managerConnectionModbus.Start);
                            managerConnection.Start();


                            this.Title = "StructSyncTest -Master";
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show("Создайте файл конфигурацию", "My App", MessageBoxButton.OK);

                            logger.Info("Настройки мастера");
                            SettingModbusForm settingModbusForm = new SettingModbusForm(this);
                            settingModbusForm.Show();
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }

                }
                if (radioButton1.IsChecked == true)
                {
                    statusbar = new Thread(timerprogressbarSlave);
                    statusbar.Start();
                    //Slave
                    try
                    {
                        HideButtonsIfConnectionSlave();
                        
                        if (File.Exists(path) == true)
                        {
                            logger.Info("Создание Slave");
                            slaveSyncSruct = new SlaveSyncSruct();

                            //Ловим при обработке (произвольная структура)
                            ms = new MMS();
                            vc = new VMS();

                            logger.Info("Создание подписок");
                            slaveSyncSruct.SignalFormedMetaClass += ms.ExecutionProcessingReguest;
                            slaveSyncSruct.SignalFormedMetaClass += vc.ExecutionProcessingReguest;

                            slaveSyncSruct.SignalFormedMetaClassAll += DisplayStruct;
                            ms.SignalFormedMetaClass += DisplayStruct;

                            vc.SignalFormedMetaClass += DisplayStruct;

                            managerConnectionModbus = new ManagerConnectionModbus(slaveSyncSruct);
                            managerConnection = new Thread(managerConnectionModbus.Start);
                            managerConnection.Start();

                            //thread = new Thread(slaveSyncSruct.Open);
                            //thread.Start();
                            this.Title = "StructSyncTest-slave";
                            //Console.WriteLine("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
                            //logger.Trace("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show("Создайте файл конфигурацию", "My App", MessageBoxButton.OK);
                            logger.Info("Настройки Slave");
                            SettingModbusForm settingModbusForm = new SettingModbusForm(this);
                            settingModbusForm.Show();
                        }



                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        //Console.WriteLine(ex);
                    }
                }
            }
            ///стоп
            else
            {
                button.Content = "Пуск";

                stoptransfer = true;
                
                StopOrStart = false;
                pessButtonStop();
                diagnostik_show.Abort();

                if (radioButton.IsChecked == true && masterSyncStruct != null)
                {
                    masterSyncStruct.Close();
                    if (managerConnectionModbus != null)
                    {
                        managerConnectionModbus.CloseManager();
                        managerConnection.Abort();
                    }

                    pessButtonStop();
                    
                    
                }

                if (radioButton1.IsChecked == true && slaveSyncSruct != null)
                {
                    slaveSyncSruct.Close();
                    if(managerConnectionModbus!=null)
                    {
                        managerConnectionModbus.CloseManager();
                        managerConnection.Abort();
                    }
                    
                    

                    pessButtonStop();
                }
            }


        }

        public bool stoptransfer;

        #region buttonhideorview and update
        private void HideButtonsIfConnectionMaster()
        {
            radioButton.IsEnabled = false;
            radioButton1.IsEnabled = false;
            btn_settings_modbus.IsEnabled = false;
            //StopTransfer.Visibility = Visibility.Visible;
        }

        private void HideButtonsIfConnectionSlave()
        {
            FileTab.Visibility = Visibility.Hidden;
            radioButton.IsEnabled = false;
            radioButton1.IsEnabled = false;
            btn_settings_modbus.IsEnabled = false;
            button2.Visibility = Visibility.Hidden;
            button3.Visibility = Visibility.Hidden;
           //StopTransfer.Visibility = Visibility.Visible;
        }

        private void pessButtonStop()
        {
            //StopTransfer.Visibility = Visibility.Visible;
            FileTab.Visibility = Visibility.Visible;
            radioButton.IsEnabled = true;
            radioButton1.IsEnabled = true;
            btn_settings_modbus.IsEnabled = true;
            button2.Visibility = Visibility.Visible;
            button3.Visibility = Visibility.Visible;
            StopTransfer.Visibility = Visibility.Hidden;
            HaveConnectionlbSignal.Fill = Brushes.Red;
            
            
            OpenFiledialog.IsEnabled = true;
            ClearSelectbr.IsEnabled = true;
            clearAllBT.IsEnabled = true;
            OpenFiledialog_folder.IsEnabled = true;
            OpenFiledialogSend.IsEnabled = true;

        }

        public void ifbuttonsendfile()
        {
            OpenFiledialog.IsEnabled = false;
            lB_PathFileView.IsEnabled = false;
            OpenFiledialogSend.IsEnabled = false;
            OpenFiledialog_folder.IsEnabled = false;
            clearAllBT.IsEnabled = false;
            ClearSelectbr.IsEnabled = false;

        }

        public void ifbuttonsendfileend()
        {
            OpenFiledialog.IsEnabled = true;
            lB_PathFileView.IsEnabled = true;
            OpenFiledialogSend.IsEnabled = true;
            OpenFiledialog_folder.IsEnabled = true;
            clearAllBT.IsEnabled = true;
            ClearSelectbr.IsEnabled = true;
        }

        private void timerprogressbar()
        {
            double value = 0;
            double date_value = 0;
            double sentpacket_value = 0;

            double timetrasfer = 0;
            //сколько осталось по времени
            double timetrasferhave = 0;

            long ellapledTicks = DateTime.Now.Ticks;
            TimeSpan elapsedSpan;
            bool enable_button = false;

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    //TickTimeLB.Content = "";
                    //ProgressSendFile.Value = 0;
                    //TickTimeShow.Text = "";
                    StopTransfer.Visibility = Visibility.Visible;
                }
                );

            while (true)
            {
                try
                {
                    double alltranferendpacket = masterSyncStruct.GetDataTrasferNow();
                    date_value = masterSyncStruct.GetDataTrasfer();
                    sentpacket_value = alltranferendpacket;
                    value = masterSyncStruct.status_bar;

                    timetrasfer = (date_value - sentpacket_value) / 1200;

                    //timetrasferhave = timetrasfer / date_value;
                }
                catch(Exception ex)
                {
                    logger.Error(ex);
                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        //value = ProgressSendFile.Value;
                        //TickTimeLB.Content = "Передано" + Math.Round(sentpacket_value / 1024, 3) + " из " + Math.Round(date_value / 1024, 2) + " КБайт";
                        //TickTimeShow.Text = "Осталось " + Math.Round(timetrasfer, 1) + "сек";
                        ProgressSendFile.Value = value;
                        if (value == 0)
                        {
                            ifbuttonsendfileend();
                        }
                        else
                        {
                            ifbuttonsendfile();
                        }

                        if (masterSyncStruct.havetrasfer==true)
                        {
                            StopTransfer.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ifbuttonsendfileend();
                            StopTransfer.Visibility = Visibility.Hidden;
                        }

                    }

                        
                    );
                Thread.Sleep(500);
            }
            /*    
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    ellapledTicks = DateTime.Now.Ticks - ellapledTicks;
                    elapsedSpan = new TimeSpan(ellapledTicks);
                    //TickTimeLB.Content = "Передано" + Math.Round(date_value / 1024, 2) + " из " + Math.Round(date_value / 1024, 2) + " КБайт";

                    //TickTimeShow.Text = "Передалось за " + Math.Round(elapsedSpan.TotalSeconds,1) + "сек";

                    if (masterSyncStruct.havetrasfer == true)
                    {
                        StopTransfer.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        StopTransfer.Visibility = Visibility.Hidden;
                    }
                    ProgressSendFile.Value = 0;
                    ifbuttonsendfileend();

                }
            );
            */

            return;
            
        }

        private void timerprogressbarSlave()
        {
            double value = 0;

            long ellapledTicks = DateTime.Now.Ticks;

            while (true)
            {
                if (slaveSyncSruct != null)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            
                            value = ProgressSendFile.Value;
                            //TickTimeLB.Content = Math.Round(slaveSyncSruct.get_all_getpacket() / 1024, 3)+"из"+Math.Round(slaveSyncSruct.get_all_packet() / 1024, 3)+"кб";

                            ProgressSendFile.Value = slaveSyncSruct.status_bar;

                            if (slaveSyncSruct.have_trasfer==true)
                            {
                                StopTransfer.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                StopTransfer.Visibility = Visibility.Hidden;
                            }

                        }
                        );

                    if (value>=95)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            value = ProgressSendFile.Value;
                            //TickTimeLB.Content = "Передано" + Math.Round(slaveSyncSruct.get_all_packet() / 1024, 3) + "кб";

                            ProgressSendFile.Value = slaveSyncSruct.status_bar;

                            if (slaveSyncSruct.have_trasfer == true)
                            {
                                StopTransfer.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                ProgressSendFile.Value = 0;
                                StopTransfer.Visibility = Visibility.Hidden;
                            }

                        }
                        );
                    }

                }
                Thread.Sleep(500);
            }


        }

        public void UpdateRadio()
        {
            loadsetings();
        }

        public void CheckConnection()
        {
            while (true)
            {
                if (managerConnection != null)
                {
                    if (managerConnectionModbus.have_connection == true)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                HaveConnectionlbSignal.Fill = Brushes.Green;
                            }
                        );
                    }
                    else
                    {
                        if (managerConnectionModbus.master!=null)
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                                {
                                    HaveConnectionlbSignal.Fill = Brushes.Red;
                                }
                            );
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                HaveConnectionlbSignal.Fill = Brushes.Yellow;
                            }
                            );
                        }
                    }

                }

                Thread.Sleep(2000);
            }
        }

        
        #endregion


        #region DisplayStruct
        private void DisplayStruct(MetaClassForStructAndtherData metaobj)
        {
            check_folder();
            FileAttributes attributes = new FileAttributes();
            string fullname = "innerfile/" + metaobj.name_file.ToString();

            if (metaobj.this_is_file==true)
            {
                MemoryStream destination = new MemoryStream();

                destination = (MemoryStream)metaobj.struct_which_need_transfer;
                //attributes = (FileAttributes)metaobj.metattributes;

                using (FileStream fs = new FileStream(fullname, FileMode.Create))
                {
                    destination.Position = 0;
                    destination.CopyTo(fs);
                    
                    
                }
                try
                {
                    File.SetAttributes(fullname, attributes);
                    File.SetCreationTime(fullname, metaobj.CreationTime_file);
                    File.SetLastWriteTime(fullname, metaobj.LastWriteTime);

                }
                catch(Exception ex)
                {
                    logger.Error(ex);
                }
            }



        }

        private void DisplayStruct(VMS v1)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        textBox5.Text = v1.test4SendStruct.ab;
                        textBox6.Text = v1.test4SendStruct.cd;
                        textBox7.Text = v1.test4SendStruct.fre;
                        textBox8.Text = v1.test4SendStruct.name;

                        for (int i=0;i<v1.test4SendStruct.count2.Length;i++)
                        {
                            richTextBox.AppendText(v1.test4SendStruct.count2[i].ToString());
                        }

                    });

            XmlSerializer XMLformatter = new XmlSerializer(typeof(VMS));

            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream("dataSaveStruct2.xml", FileMode.Create))
            {
                XMLformatter.Serialize(fs, v1);

                Console.WriteLine("Объект сериализован");
            }
        }

        private void DisplayStruct(MMS m1)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        textBox1.Text = m1.testSendStruct.ab;
                        textBox2.Text = m1.testSendStruct.cd;
                        textBox3.Text = m1.testSendStruct.fre;
                        textBox4.Text = m1.testSendStruct.name;

                    });

            XmlSerializer XMLformatter = new XmlSerializer(typeof(MMS));

            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream("dataSaveStruct1.xml", FileMode.Create))
            {
                XMLformatter.Serialize(fs, m1);

                Console.WriteLine("Объект сериализован");
            }
        }

        #endregion

        public void check_folder()
        {
            var path = System.IO.Path.GetFullPath("innerfile");

            if (!Directory.Exists("path")) 
                Directory.CreateDirectory("innerfile");
        }

        //синхронизировать
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            sendfirst_struct();
        }


        #region sendstruct
        private void sendfirst_struct()
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "")
            {

                TestSendStruct testSendStruct;

                testSendStruct.ab = textBox1.Text;
                testSendStruct.cd = textBox2.Text;
                testSendStruct.fre = textBox3.Text;
                testSendStruct.name = textBox4.Text;

                ms = new MMS(testSendStruct);
                queueOf.master = masterSyncStruct;
                try
                {
                    XmlSerializer XMLformatter = new XmlSerializer(typeof(MMS));

                    // получаем поток, куда будем записывать сериализованный объект
                    using (FileStream fs = new FileStream("dataSaveStruct1.xml", FileMode.Create))
                    {
                        XMLformatter.Serialize(fs, ms);

                        logger.Info("Данные примены");
                    }

                    if (masterSyncStruct != null)
                    {
                        metaClassFor = new MetaClassForStructAndtherData(ms);
                        metaClassFor.type_archv = 1;

                        // создаем объект BinaryFormatter
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;

                        var stream = new MemoryStream();
                        var outStream = new MemoryStream();
                        formatter.Serialize(stream, metaClassFor);

                        outStream = masterSyncStruct.Compress(stream, false);

                        // отправка данных 
                       

                        queueOf.AddQueue(outStream);
                        logger.Info("Данные добавлены в очереди");
                        //masterSyncStruct.send_multi_message(outStream);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }


        private void structsecond_struct()
        {
            if (textBox5.Text != "" && textBox6.Text != "" && textBox7.Text != "" && textBox8.Text != "")
            {
                Test4SendStruct testSendStruct;

                testSendStruct.ab = textBox5.Text;
                testSendStruct.cd = textBox6.Text;
                testSendStruct.fre = textBox7.Text;
                testSendStruct.name = textBox8.Text;
                testSendStruct.count2 = new int[100];

                int[] ab = new int[100];
                Random rand = new Random();

                for (int i = 0; i < 100; i++)
                {
                    ab[i] = rand.Next(0, 100);
                }

                testSendStruct.count2 = ab;
                vc = new VMS(testSendStruct);

                queueOf.master = masterSyncStruct;
                try
                {
                    XmlSerializer XMLformatter = new XmlSerializer(typeof(VMS));

                    // получаем поток, куда будем записывать сериализованный объект
                    using (FileStream fs = new FileStream("dataSaveStruct2.xml", FileMode.Create))
                    {
                        XMLformatter.Serialize(fs, vc);

                        Console.WriteLine("Объект сериализован");
                    }

                    if (masterSyncStruct != null)
                    {
                        metaClassFor = new MetaClassForStructAndtherData(vc);
                        // создаем объект BinaryFormatter
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                        var stream = new MemoryStream();

                        formatter.Serialize(stream, metaClassFor);

                        var oustream = masterSyncStruct.Compress(stream, false);

                        // отправка данных 
                        queueOf.AddQueue(oustream);
                        //masterSyncStruct.send_multi_message(oustream);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        #endregion


        /// <summary>
        /// 2 структура
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            structsecond_struct();
        }

        

        /// <summary>
        /// Синхронизировать все
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            sendfirst_struct();
            //Thread.Sleep(1000);
            structsecond_struct();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btn_settings_modbus_Click(object sender, RoutedEventArgs e)
        {
            SettingModbusForm settingModbusForm = new SettingModbusForm(this);
            settingModbusForm.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (radioButton.IsChecked == true)
                {
                    if (masterSyncStruct!=null)
                    {
                        masterSyncStruct.Close();
                    }
                    pessButtonStop();
                }

                if (radioButton1.IsChecked == true)
                {
                    if (masterSyncStruct != null)
                    {
                        slaveSyncSruct.Close();
                    }
                        
                    pessButtonStop();
                }

                if (statusbar!=null)
                {
                    statusbar.Abort();
                }
                Environment.Exit(0);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
            


        }

        private void OpenFiledialogSend_Click(object sender, RoutedEventArgs e)
        {
            if (masterSyncStruct!=null)
            {
              
                send_files();
            }
            else
            {
                logger.Warn("Подключитите Master");
            }
            
            
        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void StopTransfer_Click(object sender, RoutedEventArgs e)
        {
            ProgressSendFile.Value = 0;
            if (masterSyncStruct!=null)
            {
                queueOf.StopTransfer();
                queueOf.ClearQueue();
                ProgressSendFile.Value = 0;
               
                ifbuttonsendfileend();
                Thread.Sleep(100);
                stoptransfer = true;
            }
            
            if (porok!=null)
            {
                porok.Abort();
            }

            if (slaveSyncSruct != null)
            {

                slaveSyncSruct.StopTransfer();
                Thread.Sleep(100);
                //queueOf.stoptransfer();
                ProgressSendFile.Value = 0;
                
            }
            StopTransfer.Visibility = Visibility.Hidden;
        }

        #region MenuItem Контекстное меню
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            string selectedObject = (string)lB_PathFileView.SelectedItem;
            if (selectedObject != null)
            {
                pathFiles.Remove(selectedObject);
                lB_PathFileView.Items.Remove(selectedObject);

            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (masterSyncStruct != null)
            {
                string selectedObject = (string)lB_PathFileView.SelectedItem;
                if (selectedObject != null)
                {
                    send_file(selectedObject);
                }
            }
            else
            {
                logger.Warn("Подключите Master");
            }
        }

        #endregion 

        #region sendFile Отправка файло

        private void OpenFiledialog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                nameFile = "nofile.txt";
                lB_PathFileView.ItemsSource = null;
                string filestr = null;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    filestr = openFileDialog.FileName;
                    nameFile = openFileDialog.SafeFileName;
                    pathFiles.Add(filestr);
                }
                lB_PathFileView.Items.Add(filestr);
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void send_file(string path)
        {
            try
            {
                if (masterSyncStruct!=null)
                {
                    MemoryStream destination = new MemoryStream();
                    ifbuttonsendfile();

                    double valuefile = 0;

                    if (path != "")
                    {
                        porok = new Thread(timerprogressbar);

                        //new Thread(() => fileread(path, destination, valuefile)).Start();

                        sendfile = Task.Run(() => fileread(path, destination, valuefile));

                    }
                    else
                    {
                        logger.Warn("откройте файл");
                    }

                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
            
        }

        public void send_files()
        {

            foreach (var path in pathFiles)
            {

                if (path != ""|| path!=null)
                {
                    send_file(path);
                }
                else
                {
                    logger.Warn("откройте файл");
                }
            }

            foreach (var path in pathFolder)
            {
                if (path != "" || path != null)
                {
                    DirectoryInfo dir = new DirectoryInfo(path);

                    string[] file_list = Directory.GetFiles(path);
                    foreach (var item in file_list)
                    {
                        send_file(item);
                    }


                }
                else
                {
                    logger.Warn("откройте файл");
                }
            }

        }

        public void fileread(string path, MemoryStream destination, double valuefile)
        {
            FileAttributes attributes = new FileAttributes();
            DateTime dtFirstCreate=DateTime.Now;
            DateTime dTLASTWRITE = DateTime.Now;

            try
            {

                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                
                    queueOf.master = masterSyncStruct;

                    byte[] vs;

                    fs.CopyTo(destination);
                    valuefile = fs.Length;                    
                    attributes = File.GetAttributes(path);
                    dtFirstCreate = File.GetCreationTime(path);
                    dTLASTWRITE = File.GetLastWriteTime(path);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                porok.Abort();
            }

            
            try
            {
                string[] words = path.Split('\\');

                metaClassFor = new MetaClassForStructAndtherData(destination, true, words[words.Length - 1], attributes, dtFirstCreate, dTLASTWRITE);
                // создаем объект BinaryFormatter
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                var stream = new MemoryStream();

                formatter.Serialize(stream, metaClassFor);

                var oustream = masterSyncStruct.Compress(stream, false);

                logger.Info("Будет отправлен следущий файл " + words[words.Length - 1] + "(" + valuefile + "байт)");

                // отправка данных 
                queueOf.AddQueue(oustream);
                
                porok.Start();
              
            }
            catch (Exception ex)
            {
                timeprocessing_file = false;
                return;
            }




        }

        private void ClearSelectbr_Click(object sender, RoutedEventArgs e)
        {
            List<string> listpath = new List<string>();
            foreach(var path in lB_PathFileView.SelectedItems)
            {
                listpath.Add((string)path);
            }

            foreach (var path in listpath)
            {
                if (path!=""&& path!=null)
                {
                    lB_PathFileView.Items.Remove(path);
                    pathFolder.Remove(path);
                    pathFiles.Remove(path);
                }
                

            }

        }

        private void clearAllBT_Click(object sender, RoutedEventArgs e)
        {
            lB_PathFileView.Items.Clear();
            pathFiles.Clear();
            pathFolder.Clear();

        }

        private void OpenFiledialog_folder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "C:\\Users";
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    //MessageBox.Show("You selected: " + dialog.FileName);
                    pathFolder.Add(dialog.FileName);
                    lB_PathFileView.Items.Add(dialog.FileName);
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }

        }

        #endregion
    }
}
