using ModbusSyncStructLIb;
using System;
using System.Collections.Generic;
using System.Linq;
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
            masterSyncStruct = new MasterSyncStruct();
            masterSyncStruct.Open();
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
                string[] words = textBox2.Text.Split(new char[] {' '});

                ushort[] data = new ushort[words.Length];
                for (int i=0;i<words.Length;i++)
                {
                    if (words[i]!="")
                    {
                        data[i] = (ushort)Convert.ToInt16(words[i]);
                    }
                    
                }
                
                masterSyncStruct.send_multi_message(data);
            }
           
        }
    }
}
