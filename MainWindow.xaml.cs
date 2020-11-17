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
using System.Windows.Threading;
using SevenZip.Compression.LZMA;
using SevenZip;

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
        VMS vc;
        private static Logger logger;
        
        delegate void Message();

        private static Int32 dictionary = 1 << 21; //No dictionary
        private static Int32 posStateBits = 2;
        private static Int32 litContextBits = 3;   // for normal files  // UInt32 litContextBits = 0; // for 32-bit data                                             
        private static Int32 litPosBits = 0;       // UInt32 litPosBits = 2; // for 32-bit data
        private static Int32 algorithm = 2;
        private static Int32 numFastBytes = 128;
        private static bool eos = false;
        private static string mf = "bt4";

        private static CoderPropID[] propIDs =
        {
            CoderPropID.DictionarySize,
            CoderPropID.PosStateBits,
            CoderPropID.LitContextBits,
            CoderPropID.LitPosBits,
            CoderPropID.Algorithm,
            CoderPropID.NumFastBytes,
            CoderPropID.MatchFinder,
            CoderPropID.EndMarker
        };

        private static object[] properties =
        {
            (Int32)(dictionary),
            (Int32)(posStateBits),
            (Int32)(litContextBits),
            (Int32)(litPosBits),
            (Int32)(algorithm),
            (Int32)(numFastBytes),
            mf,
            eos
        };


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
                
                //Ловим при обработке (произвольная структура)
                ms = new MMS();
                vc = new VMS();

                slaveSyncSruct.SignalFormedMetaClass += ms.execution_processing_reguest;
                slaveSyncSruct.SignalFormedMetaClass += vc.execution_processing_reguest;

                slaveSyncSruct.SignalFormedMetaClass += DisplayStruct;
                Thread thread = new Thread(slaveSyncSruct.Open);
                thread.Start();

                //Console.WriteLine("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
                logger.Trace("Slave Запущен на " + slaveSyncSruct.serialPort.PortName);
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                //Console.WriteLine(ex);
            }
            
            
        }

        private void DisplayStruct(object meta)
        {
            try
            {
                Thread.Sleep(1000);
                metaClassFor = slaveSyncSruct.metaClass;
                var strucDeserization = metaClassFor.struct_which_need_transfer;
                Type type = strucDeserization.GetType();

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                        {
                            if (ms.testSendStruct.ab != null)
                            {
                                richTextBox.AppendText("testSendStruct \r\n");
                                richTextBox.AppendText(ms.testSendStruct.ab+ "\r\n");
                                richTextBox.AppendText(ms.testSendStruct.cd + "\r\n");
                                richTextBox.AppendText(ms.testSendStruct.fre + "\r\n");
                                richTextBox.AppendText(ms.testSendStruct.name + "\r\n");
                            }
                            if (ms.test2SendStruct.ab != null)
                            {
                                richTextBox.AppendText("test2SendStruct \r\n");
                                richTextBox.AppendText(ms.test2SendStruct.ab + "\r\n");
                                richTextBox.AppendText(ms.test2SendStruct.cd + "\r\n");
                                richTextBox.AppendText(ms.test2SendStruct.fre + "\r\n");
                                richTextBox.AppendText(ms.test2SendStruct.name + "\r\n");
                                richTextBox.AppendText(Convert.ToString(ms.test2SendStruct.count + "\r\n"));
                                richTextBox.AppendText(Convert.ToString(ms.test2SendStruct.count2 + "\r\n"));
                            }
                        }
                    );
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                //Console.WriteLine(ex);
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
                
                masterSyncStruct.SendSingleMessage(value, address);
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

                    //masterSyncStruct.send_multi_message(data);
                }

                if (radioButton_str.IsChecked == true)
                {
                    ushort[] bytes = textBox2.Text.Select(c => (ushort)c).ToArray();
                    //masterSyncStruct.send_multi_message(bytes);
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
            if (textBox7.Text!="")
            {
                try
                {
                    //Console.WriteLine("Запуск");
                    logger.Trace("Запуск");

                    TestSendStruct testSend;
                    testSend.name = textBox7.Text;
                    testSend.fre = textBox7.Text;
                    testSend.ab = textBox7.Text;
                    testSend.cd = textBox7.Text;

                    MMS ms1 = new MMS(testSend);

                    metaClassFor = new MetaClassForStructandtherdata(ms1);
                    // создаем объект BinaryFormatter
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                    var stream = new MemoryStream();

                    formatter.Serialize(stream, metaClassFor);

                    //Console.WriteLine("Отправка данных");
                    logger.Trace("Отправка данных");

                    masterSyncStruct.SendMultiMessage(stream);

                    Console.WriteLine(stream);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    logger.Error(ex);
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
                //Console.WriteLine(ex);
                logger.Error(ex);
            }
            
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            if (textBox7.Text != "")
            {
                Test2SendStruct test2SendStruct;
                test2SendStruct.name = textBox7.Text;
                test2SendStruct.fre = textBox7.Text;
                test2SendStruct.ab = textBox7.Text;
                test2SendStruct.cd = textBox7.Text;
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

                    masterSyncStruct.SendMultiMessage(stream);

                    Console.WriteLine(stream);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    logger.Error(ex);
                }

            }
        }


        //3 структура
        private void button9_Click(object sender, RoutedEventArgs e)
        {
            if (textBox7.Text != "")
            {
                Test4SendStruct test4SendStruct;
                test4SendStruct.name = textBox7.Text;
                test4SendStruct.fre = textBox7.Text;
                test4SendStruct.ab = textBox7.Text;
                test4SendStruct.cd = textBox7.Text;
                test4SendStruct.count2 = new int[10000];

                int[] ab = new int[10000];

                for(int i=0; i<1000;i++)
                {
                    Random rand = new Random();
                    ab[i] = rand.Next(0, 1000);
                }

                vc = new VMS(test4SendStruct);

                try
                {
                    metaClassFor = new MetaClassForStructandtherdata(vc);

                    // создаем объект BinaryFormatter
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                    var stream = new MemoryStream();

                    formatter.Serialize(stream, metaClassFor);

                    masterSyncStruct.SendMultiMessage(stream);

                    Console.WriteLine(stream);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    logger.Error(ex);
                }

            }

        }


        private void button12_Click(object sender, RoutedEventArgs e)
        {
            if (textBox7.Text != "")
            {
                Test4SendStruct test4SendStruct;
                test4SendStruct.name = textBox7.Text;
                test4SendStruct.fre = textBox7.Text;
                test4SendStruct.ab = textBox7.Text;
                test4SendStruct.cd = textBox7.Text;
                test4SendStruct.count2 = new int[10000];

                int[] ab = new int[10000];
                Random rand = new Random();

                for (int i = 0; i < 1000; i++)
                {
                    ab[i] = rand.Next(1, 1000);
                }
                test4SendStruct.count2 = ab;
                vc = new VMS(test4SendStruct);

                try
                {
                    metaClassFor = new MetaClassForStructandtherdata(vc);
                    metaClassFor.type_archv = 1;

                    // создаем объект BinaryFormatter
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                    var stream = new MemoryStream();
                    var outStream = new MemoryStream();
                    formatter.Serialize(stream, metaClassFor);


                    //архивируем
                    outStream = compress(stream, false);

                    var sss=decompress(outStream, false);

                    /*
                    byte[] date = stream.ToArray();
                    
                    byte[] date2 = outStream.ToArray();

                    byte[] date1 = sss.ToArray();
                    */

                    masterSyncStruct.SendMultiMessage(outStream);

                    Console.WriteLine(stream);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex);
                    logger.Error(ex);
                }

            }

        }

        #region 7zip 
        public MemoryStream compress(MemoryStream inStream, bool closeInStream)
        {
            inStream.Position = 0;
            Int64 fileSize = inStream.Length;
            MemoryStream outStream = new MemoryStream();

            SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);

            if (BitConverter.IsLittleEndian)
            {
                byte[] LengthHeader = BitConverter.GetBytes(fileSize);
                outStream.Write(LengthHeader, 0, LengthHeader.Length);
            }

            encoder.Code(inStream, outStream, -1, -1, null);

            if (closeInStream)
                inStream.Close();

            return outStream;
        }


        public MemoryStream decompress(MemoryStream inStream, bool closeInStream)
        {
            inStream.Position = 0;
            MemoryStream outStream = new MemoryStream();

            byte[] properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw (new Exception("input .lzma is too short"));

            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            decoder.SetDecoderProperties(properties);

            long outSize = 0;

            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 8; i++)
                {
                    int v = inStream.ReadByte();
                    if (v < 0)
                        throw (new Exception("Can't Read 1"));

                    outSize |= ((long)(byte)v) << (8 * i);
                }
            }

            long compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, compressedSize, outSize, null);

            if (closeInStream)
                inStream.Close();

            return outStream;
        }

        #endregion
    }
}
