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
using System.Xml.Serialization;
using ModbusSyncStructLIb.Settings;
using Microsoft.Win32;

namespace ModbusSynchFormTest
{
    /// <summary>
    /// Логика взаимодействия для StructSyncTest.xaml
    /// </summary>
    public partial class StructSyncTest : Window
    {
        private static Logger logger;
        Thread thread;

        #region Base Modbus
        MasterSyncStruct masterSyncStruct;
        SlaveSyncSruct slaveSyncSruct;
        #endregion

        #region struct
        MMS ms;
        VMS vc;
        #endregion

        #region metaclass
        MetaClassForStructandtherdata metaClassFor;
        QueueOfSentMessagesForSlave queueOf;
        string nameFile;
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
                        Console.WriteLine("Объект десериализован");
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

                        Console.WriteLine("Объект десериализован");
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

                        Console.WriteLine("Объект десериализован");
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


        private void button_Click(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.GetFullPath(@"Settingsmodbus.xml");
            if (radioButton.IsChecked==true)
            {
                //Master
                try
                {
                    HideButtonsIfConnectionMaster();
                    if (File.Exists(path) == true)
                    {
                        logger.Info("Создание мастера");
                        masterSyncStruct = new MasterSyncStruct();
                        thread = new Thread(masterSyncStruct.Open);
                        thread.Start();
                        this.Title = "StructSyncTest -Master";
                    }
                    else
                    {
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
                        slaveSyncSruct.SignalFormedMetaClass += ms.execution_processing_reguest;
                        slaveSyncSruct.SignalFormedMetaClass += vc.execution_processing_reguest;
                        
                        slaveSyncSruct.SignalFormedMetaClassAll += DisplayStruct;
                        ms.SignalFormedMetaClass += DisplayStruct;

                        vc.SignalFormedMetaClass += DisplayStruct;

                        

                        thread = new Thread(slaveSyncSruct.Open);
                        thread.Start();

                        this.Title = "StructSyncTest-slave";

                        //Console.WriteLine("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
                        //logger.Trace("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
                    }
                    else
                    {
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

        #region buttonhideorview and update
        private void HideButtonsIfConnectionMaster()
        {
            radioButton.IsEnabled = false;
            radioButton1.IsEnabled = false;
            btn_settings_modbus.IsEnabled = false;
        }

        private void HideButtonsIfConnectionSlave()
        {
            button1.Visibility = Visibility.Hidden;
            radioButton.IsEnabled = false;
            radioButton1.IsEnabled = false;
            btn_settings_modbus.IsEnabled = false;
            button2.Visibility = Visibility.Hidden;
            button3.Visibility = Visibility.Hidden;
        }

        private void pessButtonStop()
        {
            button1.Visibility = Visibility.Visible;
            radioButton.IsEnabled = true;
            radioButton1.IsEnabled = true;
            btn_settings_modbus.IsEnabled = true;
            button2.Visibility = Visibility.Visible;
            button3.Visibility = Visibility.Visible;
        }

        public void updateradio()
        {
            loadsetings();
        }

        #endregion


        #region DisplayStruct
        private void DisplayStruct(MetaClassForStructandtherdata metaobj)
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
                //File.SetAttributes(fullname, attributes);
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

                        Console.WriteLine("Объект сериализован");
                    }

                    if (masterSyncStruct != null)
                    {
                        metaClassFor = new MetaClassForStructandtherdata(ms);
                        metaClassFor.type_archv = 1;

                        // создаем объект BinaryFormatter
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;

                        var stream = new MemoryStream();
                        var outStream = new MemoryStream();
                        formatter.Serialize(stream, metaClassFor);

                        outStream = masterSyncStruct.compress(stream, false);

                        // отправка данных 
                        var sss = masterSyncStruct.decompress(outStream, false);

                        queueOf.add_queue(outStream);

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
                        metaClassFor = new MetaClassForStructandtherdata(vc);
                        // создаем объект BinaryFormatter
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                        var stream = new MemoryStream();

                        formatter.Serialize(stream, metaClassFor);

                        var oustream = masterSyncStruct.compress(stream, false);

                        // отправка данных 
                        queueOf.add_queue(oustream);
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
            if (radioButton.IsChecked == true&& masterSyncStruct!=null)
            {
                masterSyncStruct.close();
                pessButtonStop();
            }

            if (radioButton1.IsChecked == true&& slaveSyncSruct!=null)
            {
                slaveSyncSruct.close();
                pessButtonStop();
            }
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
                        masterSyncStruct.close();
                    }
                    pessButtonStop();
                }

                if (radioButton1.IsChecked == true)
                {
                    if (masterSyncStruct != null)
                    {
                        slaveSyncSruct.close();
                    }
                        
                    pessButtonStop();
                }

                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
            


        }

        private void OpenFiledialog_Click(object sender, RoutedEventArgs e)
        {
            nameFile="nofile.txt";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                PAth_lb_file.Content = openFileDialog.FileName;
                nameFile = openFileDialog.SafeFileName;
            }
        }

        private void OpenFiledialogSend_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream destination = new MemoryStream();
            FileAttributes attributes=new FileAttributes();
            if (PAth_lb_file.Content.ToString()!="...")
            {
                using (FileStream fs = new FileStream(PAth_lb_file.Content.ToString(), FileMode.Open))
                {
                    try
                    {
                        queueOf.master = masterSyncStruct;

                        byte[] vs;

                        fs.CopyTo(destination);
                        //attributes = File.GetAttributes(PAth_lb_file.Content.ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }


                }

                try
                {
                    metaClassFor = new MetaClassForStructandtherdata(destination, true, nameFile);
                    // создаем объект BinaryFormatter
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                    var stream = new MemoryStream();

                    formatter.Serialize(stream, metaClassFor);

                    var oustream = masterSyncStruct.compress(stream, false);

                    // отправка данных 
                    queueOf.add_queue(oustream);
                    //masterSyncStruct.send_multi_message(oustream);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            else
            {
                logger.Warn("откройте файл");
            }
            
        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }
    }
}
