using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Word_Reminder
{
    /// <summary>
    /// Interaction logic for windowOption.xaml
    /// </summary>
    public partial class windowOption : Window
    {
        public windowOption(int time, bool autoStart, bool playSound)
        {
            InitializeComponent();

            txtTime.Text = time.ToString();
            if (autoStart)
                cAuto.IsChecked = true;
            if (playSound)
                cSound.IsChecked = true;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
