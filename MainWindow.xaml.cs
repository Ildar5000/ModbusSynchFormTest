using ModbusSyncStructLIb;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            masterSyncStruct = new MasterSyncStruct(textBox6.Text);
            Thread thread = new Thread(masterSyncStruct.Open);
            thread.Start();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SlaveSyncSruct slaveSyncSruct = new SlaveSyncSruct();

            Thread thread = new Thread(slaveSyncSruct.Open);
            thread.Start();
            
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
                MetaClassForStructandtherdata metaClassFor = new MetaClassForStructandtherdata(textBox5.Text);

                // создаем объект BinaryFormatter
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
                var stream = new MemoryStream();

                formatter.Serialize(stream, metaClassFor);

                masterSyncStruct.send_multi_message(stream);

                Console.WriteLine(stream);
            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            ushort status= masterSyncStruct.SendRequestforStatusSlave();

            Console.WriteLine(status);
        }
    }
}
