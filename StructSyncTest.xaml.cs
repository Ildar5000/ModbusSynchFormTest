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
using ModbusSyncStructLIb;
using ModbusSyncStructLIb.EvenBase;
using NLog;
using NLog.Config;
using StructAllforTest;

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
        #endregion


        public StructSyncTest()
        {
            InitializeComponent();
            var loggerconf = new XmlLoggingConfiguration("NLog.config");
            logger = LogManager.GetCurrentClassLogger();
            queueOf = new QueueOfSentMessagesForSlave();

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            
            if (radioButton.IsChecked==true)
            {
                //Master
                try
                {
                    logger.Info("Создание мастера");
                    masterSyncStruct = new MasterSyncStruct(textBox.Text);
                    thread = new Thread(masterSyncStruct.Open);
                    thread.Start();
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
                    logger.Info("Создание Slave");
                    slaveSyncSruct = new SlaveSyncSruct();

                    //Ловим при обработке (произвольная структура)
                    ms = new MMS();
                    vc = new VMS();

                    logger.Info("Создание подписок");
                    slaveSyncSruct.SignalFormedMetaClass += ms.execution_processing_reguest;
                    slaveSyncSruct.SignalFormedMetaClass += vc.execution_processing_reguest;

                    ms.SignalFormedMetaClass += DisplayStruct;
                    
                    vc.SignalFormedMetaClass += DisplayStruct;

                    //slaveSyncSruct.SignalFormedMetaClass += DisplayStruct;

                    thread = new Thread(slaveSyncSruct.Open);
                    thread.Start();

                    //Console.WriteLine("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
                    logger.Trace("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    //Console.WriteLine(ex);
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
        }


        //синхронизировать
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            sendfirst_struct();
        }

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
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// 2 структура
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            structsecond_struct();
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
                testSendStruct.count2 = new int[10000];

                int[] ab = new int[10000];

                for (int i = 0; i < 1000; i++)
                {
                    Random rand = new Random();
                    ab[i] = rand.Next(0, 1000);
                }

                testSendStruct.count2 = ab;
                vc = new VMS(testSendStruct);

                queueOf.master = masterSyncStruct;
                try
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
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// Синхронизировать все
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            sendfirst_struct();
            structsecond_struct();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (radioButton.IsChecked == true)
            {
                masterSyncStruct.close();

            }

            if (radioButton1.IsChecked == true)
            {
                slaveSyncSruct.close();
            }
        }
    }
}
