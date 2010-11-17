using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Word_Reminder
{
    class ClzzWord:INotifyPropertyChanged
    {
        public string words { get; set; }
        public string means { get; set; }
        public string notes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        public string Meaning
        {
            get
            {
                return this.means;
            }
            set
            {
                if (this.means != value)
                {
                    this.means = value;
                    this.Notify("Meaning");
                }
            }
        }

        public string Word
        {
            get
            {
                return this.words;
            }
            set
            {
                if (this.words != value)
                {
                    this.words = value;
                    this.Notify("Word");
                }
            }
        }

    }

    //public class Dictionary : ObservableCollection<ClzzWord>
    //{
    //    public Dictionary()
    //    {
    //    }


    //}

 


    // get Words

    class Load_words
    {
        public static List<ClzzWord> loadword()
        {
            List<ClzzWord> listword = new List<ClzzWord>();

            var q1 = from q1_ in XElement.Load(@"Database/Words.xml").Elements("WordReminder")
                     select q1_;

            //listword = q1.ToList();
            foreach (var w1 in q1)
            {
                ClzzWord clzz = new ClzzWord
                {
                    words = w1.Element("Words").Value,
                    means = w1.Element("Means").Value,
                    notes = w1.Element("Notes").Value
                };
                listword.Add(clzz);
            }
            return listword;
        }
    }



}
