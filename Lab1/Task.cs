using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Lab1
{
    public class Task : INotifyPropertyChanged
    {
        public long Id { get; set; }

        private string name;
        private string discription;
        private States state;

        public string Name
        {
            get { return name; }

            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Discription
        {
            get { return discription; }

            set
            {
                discription = value;
                OnPropertyChanged("Discription");
            }
        }
        public States State
        {
            get { return state; }

            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

}
