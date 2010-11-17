using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace Word_Reminder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.NotifyIcon notify = new System.Windows.Forms.NotifyIcon();
        List<ClzzWord> wordlist = new List<ClzzWord>();
        public XElement doc = XElement.Load(@"Database/Words.xml");
        string wordcurrent, meancurrent;
        int flag, point, currentId, countN;
        DispatcherTimer dispatch = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            /// Update Data form xml
            Update();
            ///
            
            ///Notify Word form taskbar
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            this.Deactivated += new EventHandler(MainWindow_Deactivated);
            this.notify.Visible = true;
            this.notify.Icon = Word_Reminder.Icon.Penguin;
            this.notify.ContextMenu = new System.Windows.Forms.ContextMenu();
            this.notify.ContextMenu.MenuItems.Add("Exit");
            this.notify.ContextMenu.MenuItems[0].Click += new EventHandler(this.mnuExit_Click);
            this.notify.DoubleClick += new EventHandler(notify_DoubleClick);
            //this.notify.BalloonTipClicked += new EventHandler(notify_BalloonTipClicked);            
            
            this.notify.BalloonTipClosed += new EventHandler(notify_BalloonTipClosed);
            
            
        }
        void notify_BalloonTipClosed(object sender, EventArgs e)
        {
            dispatch = new System.Windows.Threading.DispatcherTimer();
            dispatch.Interval = TimeSpan.FromSeconds(60);

            dispatch.Start();


            dispatch.Tick += new EventHandler(dispatch_Tick);
        }

        void dispatch_Tick(object sender, EventArgs e)
        {

            if (this.currentId < this.wordlist.Count - 1)
            {
                this.currentId++;
            }
            else
            {
                this.currentId = 0;
            }
            dispatch.Stop();
            this.showBalloon(currentId);

            
            
        }
        
        private void showBalloon(int currentId)
        {
            
            
            ClzzWord _word = this.wordlist[currentId];
            this.notify.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notify.BalloonTipTitle = wordlist[currentId].words;
            if (wordlist[currentId].means != "")
                this.notify.BalloonTipText = wordlist[currentId].means;
            else
                this.notify.BalloonTipText = string.Format(" Still haven't had any mean for '{0}' ",wordlist[currentId].words) ;

            this.notify.ShowBalloonTip(10);
            
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
           
            //base.Close();
            this.notify.Visible = false;
            Application.Current.Shutdown();
        }
        //void notify_BalloonTipClicked(object sender, EventArgs e)
        //{
        //    base.Show();
        //    this.WindowState = System.Windows.WindowState.Normal;
        //}

        void notify_DoubleClick(object sender, EventArgs e)
        {
            base.Show();
            this.WindowState = System.Windows.WindowState.Normal;
        }

        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                this.Hide();
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.Hide();
            if (countN > 0)
                showBalloon(0);
            else
            {
                this.notify.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                this.notify.BalloonTipTitle = " Still haven't had any word ^^!";
                this.notify.BalloonTipText = " Still haven't had any mean ";
                this.notify.ShowBalloonTip(10);
            }
            
        }
        
        
            
        private void Save_Click(object sender, RoutedEventArgs e)
        {

            //---------Add new word---------------------------

            if (flag == 1 || Words.SelectedIndex == -1)
            {

                if (depulicate())
                {
                    for (int i = 0; i < wordlist.Count; i++)
                    {
                        if (wordlist[i].Word == T_Word.Text)
                        {
                            Words.SelectedIndex = i;
                            string sMessageBoxText = " This word has existed. Please use it again ^^!";

                            string sCaption = "Warning";
                            MessageBoxButton btnMessageBox = MessageBoxButton.OK;

                            MessageBoxImage icnMessageBox = MessageBoxImage.Information;
                            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                            break;
                        }
                    }
                }                   

                else
                {
                    XElement xml =
                                        new XElement("WordReminder",
                                        new XElement("Words", T_Word.Text),
                                        new XElement("Means", T_Mean.Text),
                                        new XElement("Notes", ""));
                    doc.Add(xml);
                    doc.Save("Database/Words.xml");
                    MessageBox.Show(" Successful ^^!");
                    Update();

                    Words.SelectedIndex = Words.Items.Count - 1;
                }

                //--------------Edit existent word

            }
            else
            {

                //List <XElement> count = (from cw in doc.Elements("WordReminder")
                //                      where cw.Element("Words").Value.Equals(wordcurrent)
                //                      select cw).ToList();
                //if (count.Count > 1)
                //{
                //    string sMessageBoxText = " This word has existed. Please use it again ^^!";

                //    string sCaption = "Warning";
                //    MessageBoxButton btnMessageBox = MessageBoxButton.OK;

                //    MessageBoxImage icnMessageBox = MessageBoxImage.Information;
                //    MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                //}
                //else
                //{
                    //XElement doc1 = XElement.Load("Database/Words.xml");
                    var singleword = (from w in doc.Elements("WordReminder")
                                      where w.Element("Words").Value.Equals(wordcurrent)
                                      select w).Single();
                    
                    singleword.Element("Words").Value = T_Word.Text;
                    singleword.Element("Means").Value = T_Mean.Text;
                    singleword.Element("Notes").Value = Notes.Text;

                    doc.Save("Database/Words.xml");
                    MessageBox.Show(" Successful ^^!");
                    Update();
                    if (point == -1) Words.SelectedIndex = Words.Items.Count - 1;
                    else
                        Words.SelectedIndex = point;
                //}
            }
            
        }

        private void Words_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Words.SelectedIndex != -1)
            {
                flag = 0;
                Mean.Text = wordlist[Words.SelectedIndex].means;
                Notes.Text = wordlist[Words.SelectedIndex].notes;
                wordcurrent = wordlist[Words.SelectedIndex].words;
                meancurrent = wordlist[Words.SelectedIndex].means;
                point = Words.SelectedIndex;

                T_Word.Text = wordlist[Words.SelectedIndex].words;
                T_Mean.Text = wordlist[Words.SelectedIndex].means;
            }
            else return;
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
           
            flag = 1;
            Words.SelectedIndex = -1;
            T_Word.Text = "";
            T_Mean.Text = "";
            T_Word.Focus();
           
        }

        public void _Delete()
        {
           
            flag = 0;
            var deleteword = (from d in doc.Elements("WordReminder")
                              where d.Element("Words").Value.Equals(wordcurrent) && d.Element("Means").Value.Equals(meancurrent)
                              select d).Single();

            deleteword.Remove();            
            doc.Save("Database/Words.xml");

            Update();

            T_Word.Text = ""; Mean.Text = "";
            T_Mean.Text = ""; Notes.Text = "";
        }


        public void Update()
        {
            wordlist = Load_words.loadword();
            Words.ItemsSource = wordlist;
            countN = wordlist.Count;

        }

        private bool  depulicate()
        {
            wordlist = Load_words.loadword();
            List<XElement> count = (from cw in doc.Elements("WordReminder")
                                    where cw.Element("Words").Value.Equals(T_Word.Text)
                                    select cw).ToList();
           
            if (count.Count >= 1)
            {              
                 return true;
            }
            else return false;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            _Delete();
        }       

       
    }
}
