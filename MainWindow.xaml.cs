﻿using ModbusSyncStructLIb;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MasterSyncStruct masterSyncStruct = new MasterSyncStruct();
            masterSyncStruct.Open();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SlaveSyncSruct slaveSyncSruct = new SlaveSyncSruct();

            Thread thread = new Thread(slaveSyncSruct.Open);
            thread.Start();
            
        }
    }
}
