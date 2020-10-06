using ModbusSyncStructLIb;
using NLog;
using NLog.Config;
using StructAllforTest;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModbusSynchFormTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MasterSyncStruct masterSyncStruct;
        MetaClassForStructandtherdata metaClassFor;
        SlaveSyncSruct slaveSyncSruct;
        MMS ms;
        private static Logger logger;

        public MainWindow()
        {
            InitializeComponent();

            var loggerconf = new XmlLoggingConfiguration("NLog.config");
            logger = LogManager.GetCurrentClassLogger();


        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info("Создание мастера");
                masterSyncStruct = new MasterSyncStruct(textBox6.Text);
                Thread thread = new Thread(masterSyncStruct.Open);
                thread.Start();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                slaveSyncSruct = new SlaveSyncSruct();
                slaveSyncSruct.SignalFormedMetaClass += DisplayStruct;

                //Ловим при обработке (произвольная структура)
                ms = new MMS();
               
                slaveSyncSruct.SignalFormedMetaClass += ms.execution_processing_reguest;

                Thread thread = new Thread(slaveSyncSruct.Open);
                thread.Start();

                //Console.WriteLine("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
                logger.Trace("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            
        }

        private void DisplayStruct(object meta)
        {
            try
            {
                metaClassFor = slaveSyncSruct.metaClass;
                var strucDeserization = metaClassFor.struct_which_need_transfer;
                Type type = strucDeserization.GetType();



            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }
        
        public void defenitionType(Type type)
        {

        }

        //отправка данных по Master
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text!=""&& textBox1.Text!="")
            {
                ushort address = (ushort)Convert.ToInt16(textBox.Text);
                
                ushort value = (ushort)Convert.ToInt16(textBox1.Text);
                
                masterSyncStruct.send_single_message(value, address);
            }
            
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (textBox2.Text!="")
            {
                if (radioButton_us.IsChecked==true)
                {
                    string[] words = textBox2.Text.Split(new char[] { ' ' });

                    ushort[] data = new ushort[words.Length];
                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i] != "")
                        {
                            data[i] = (ushort)Convert.ToInt16(words[i]);
                        }

                    }

                    masterSyncStruct.send_multi_message(data);
                }

                if (radioButton_str.IsChecked == true)
                {
                    ushort[] bytes = textBox2.Text.Select(c => (ushort)c).ToArray();
                    masterSyncStruct.send_multi_message(bytes);
                }

            }
           
        }
        //создание датагрида
        private void button4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            ushort []data=masterSyncStruct.readHolding();

            for(int i= 0;i<data.Length; i++)
            {
                Console.WriteLine(data[i]);
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            if (textBox5.Text!="")
            {
                try
                {
                    //Console.WriteLine("Запуск");
                    logger.Trace("Запуск");

                    TestSendStruct testSend;
                    testSend.name = textBox5.Text;
                    testSend.fre = textBox5.Text;
                    testSend.ab = textBox5.Text;
                    testSend.cd = textBox5.Text;

                    metaClassFor = new MetaClassForStructandtherdata(testSend);
                    // создаем объект BinaryFormatter
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                    var stream = new MemoryStream();

                    formatter.Serialize(stream, metaClassFor);

                    //Console.WriteLine("Отправка данных");
                    logger.Trace("Отправка данных");

                    masterSyncStruct.send_multi_message(stream);

                    Console.WriteLine(stream);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ushort status = masterSyncStruct.SendRequestforStatusSlave();

                Console.WriteLine(status);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            if (textBox5.Text != "")
            {
                Test2SendStruct test2SendStruct;
                test2SendStruct.name = textBox5.Text;
                test2SendStruct.fre = textBox5.Text;
                test2SendStruct.ab = textBox5.Text;
                test2SendStruct.cd = textBox5.Text;
                test2SendStruct.count = 1;
                test2SendStruct.count2 = 2;

                ms = new MMS(test2SendStruct);


                try
                {
                    metaClassFor = new MetaClassForStructandtherdata(ms);

                    // создаем объект BinaryFormatter
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                    var stream = new MemoryStream();

                    formatter.Serialize(stream, metaClassFor);

                    masterSyncStruct.send_multi_message(stream);

                    Console.WriteLine(stream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }
    }
}
