using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tischfussball_TurnierManager.Data
{
    [Serializable]
    public class Fixture : INotifyPropertyChanged
    {
        public Team Team1 { get; set; }

        #region Goals1

        public int Goals1
        {
            get { return goals1; }
            set
            {
                if (value != goals1)
                {
                    goals1 = value;
                    OnNotifyPropertyChanged("Goals1");
                }
            }
        }

        private int goals1;

        #endregion Goals1

        #region Goals2

        public int Goals2
        {
            get { return goals2; }
            set
            {
                if (value != goals2)
                {
                    goals2 = value;
                    OnNotifyPropertyChanged("Goals2");
                }
            }
        }

        private int goals2;

        #endregion Goals2

        public Team Team2 { get; set; }

        public Fixture(Team t1, Team t2)
            : this()
        {
            Team1 = t1;
            Team2 = t2;
        }

        public Fixture()
        {
            Goals1 = 0;
            Goals2 = 0;
        }

        private void OnNotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}