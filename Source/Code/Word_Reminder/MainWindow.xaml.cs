using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Threading;
using System.Windows.Data;
using System.Media;
using System.Configuration;
using System.Reflection;

namespace Word_Reminder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notify;
        private CollectionViewSource viewSource;
        private List<ClzzWord> wordlist;
        private XElement doc;
        private bool flagNewWord, playSound, autoStart;
        private int currentId, time;
        private DispatcherTimer dispatch;
        SoundPlayer simpleSound;
        public MainWindow()
        {
            InitializeComponent();

            //init
            wordlist = new List<ClzzWord>();
            doc = XElement.Load(@"Database/Words.xml");
            notify = new System.Windows.Forms.NotifyIcon();
            dispatch = new DispatcherTimer();
            viewSource = ((CollectionViewSource)(this.FindResource("viewSource")));
            flagNewWord = false;
            simpleSound = new SoundPlayer(Word_Reminder.Icon.audio);
            time = 30;
            playSound = true;

            // Read Config File
            readConfig();

            /// Update Data form xml
            loadData();
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

            //this.notify.BalloonTipClosed += new EventHandler(notify_BalloonTipClosed);
            dispatch = new System.Windows.Threading.DispatcherTimer();
            dispatch.Interval = TimeSpan.FromSeconds(time);
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
            this.showBalloon(currentId);
        }

        private void showBalloon(int currentId)
        {
            ClzzWord _word = this.wordlist[currentId];
            this.notify.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notify.BalloonTipTitle = wordlist[currentId].Word;
            if (wordlist[currentId].Mean != "")
                this.notify.BalloonTipText = wordlist[currentId].Mean;
            else
                this.notify.BalloonTipText = string.Format(" Still haven't had any mean for '{0}' ", wordlist[currentId].Word);

            this.notify.ShowBalloonTip(10);

            if (playSound)
                simpleSound.Play();


        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            this.notify.Visible = false;
            base.OnClosed(e);
        }

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
            if (wordlist.Count > 0)
                showBalloon(0);
            else
            {
                this.notify.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                this.notify.BalloonTipTitle = " Still haven't had any word.";
                this.notify.BalloonTipText = " Still haven't had any mean.";
                //this.notify.ShowBalloonTip(10);
            }
        }



        private void btnSaveWord_Click(object sender, RoutedEventArgs e)
        {
            // Add new word
            if (flagNewWord)
            {
                int i = checkDuplicate(txtWord.Text);
                if (i != -1)
                {
                    viewSource.View.MoveCurrentToPosition(i);
                    string sMessageBoxText = " This word has existed. Please use it again ^^!";
                    MessageBox.Show(sMessageBoxText, "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    XElement xml = new XElement("WordReminder",
                                        new XElement("Words", txtWord.Text),
                                            new XElement("Means", txtMean.Text),
                                                new XElement("Notes", ""));
                    doc.Add(xml);
                    doc.Save("Database/Words.xml");
                    loadData();

                    viewSource.View.MoveCurrentToLast();
                    lstWords.ScrollIntoView(viewSource.View.CurrentItem);
                    updateStatus("Save successful !");
                }
            }
            // Edit existent word
            else
            {
                int index = viewSource.View.CurrentPosition;
                string wordcurrent = wordlist[index].Word;

                var singleword = (from w in doc.Elements("WordReminder")
                                  where w.Element("Words").Value.Equals(wordcurrent)
                                  select w).Single();

                singleword.Element("Words").Value = txtWord.Text;
                singleword.Element("Means").Value = txtMean.Text;
                singleword.Element("Notes").Value = txtNote.Text;

                doc.Save("Database/Words.xml");
                loadData();

                viewSource.View.MoveCurrentToPosition(index);
                lstWords.ScrollIntoView(viewSource.View.CurrentItem);
                updateStatus("Save successful !");
            }

        }

        private void lstWords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            flagNewWord = false;
            updateStatus("Ready");
        }

        private void btnNewWord_Click(object sender, RoutedEventArgs e)
        {
            lstWords.SelectedIndex = -1;
            flagNewWord = true;
            clearText();
            txtWord.Focus();
        }

        public void loadData()
        {
            wordlist = loadFile();
            viewSource.Source = wordlist;
        }

        public void updateStatus(String str)
        {
            lblStatus.Content = str;
        }

        public void clearText()
        {
            txtWord.Text = ""; txtMean.Text = ""; txtNote.Text = "";
        }

        private int checkDuplicate(string str)
        {
            int count = (from w in wordlist
                         where w.Word.Equals(str)
                         select w).Count();

            if (count > 0)
                return count;
            else
                return -1;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteWord();
            updateStatus("Delete successful !");
        }

        public void deleteWord()
        {
            flagNewWord = false;
            int index = viewSource.View.CurrentPosition;
            string wordcurrent = wordlist[index].Word;
            string meancurrent = wordlist[index].Mean;
            var deleteword = (from d in doc.Elements("WordReminder")
                              where d.Element("Words").Value.Equals(wordcurrent) && d.Element("Means").Value.Equals(meancurrent)
                              select d).Single();

            deleteword.Remove();
            doc.Save("Database/Words.xml");

            loadData();

            viewSource.View.MoveCurrentToFirst();
            lstWords.ScrollIntoView(viewSource.View.CurrentItem);
        }

        private List<ClzzWord> loadFile()
        {
            List<ClzzWord> listword = new List<ClzzWord>();

            var q1 = from q in doc.Elements("WordReminder")
                     select q;

            foreach (var w1 in q1)
            {
                ClzzWord clzz = new ClzzWord
                {
                    Word = w1.Element("Words").Value,
                    Mean = w1.Element("Means").Value,
                    Note = w1.Element("Notes").Value
                };
                listword.Add(clzz);
            }
            return listword;
        }

        private void mnuOption_Click(object sender, RoutedEventArgs e)
        {
            windowOption w = new windowOption(time, autoStart, playSound);
            w.ShowDialog();
            if (w.DialogResult.HasValue && w.DialogResult.Value)
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                Configuration config;
                fileMap.ExeConfigFilename = "main.config";
                config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                bool auto = (bool)w.cAuto.IsChecked;

                string keyName = "Word_Reminder";
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;  // Or the EXE path.
                if (auto)
                {
                    // Set Auto-start.
                    if (!Util.IsAutoStartEnabled(keyName, assemblyLocation))
                        Util.SetAutoStart(keyName, assemblyLocation);
                }
                else
                {
                    // Unset Auto-start.
                    if (Util.IsAutoStartEnabled(keyName, assemblyLocation))
                        Util.UnSetAutoStart(keyName);
                }
                config.AppSettings.Settings["autostart"].Value = auto.ToString();
                autoStart = auto;

                config.AppSettings.Settings["playsound"].Value = w.cSound.IsChecked.ToString();
                playSound = (bool)w.cSound.IsChecked;
                config.AppSettings.Settings["time"].Value = w.txtTime.Text;
                time = int.Parse(w.txtTime.Text);

                config.Save(ConfigurationSaveMode.Modified);
            }
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Written by :\n-folksonomy.hv\n-vohlevu\n\nCN07B - HCMUTRANS", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void readConfig()
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            Configuration config;
            fileMap.ExeConfigFilename = "main.config";
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            String temp = config.AppSettings.Settings["time"].Value;
            if (!int.TryParse(temp, out time))
                time = 30;
            temp = config.AppSettings.Settings["playsound"].Value;
            if (!bool.TryParse(temp, out playSound))
                playSound = true;
            temp = config.AppSettings.Settings["autostart"].Value;
            if (!bool.TryParse(temp, out autoStart))
                autoStart = false;
        }
    }
}
