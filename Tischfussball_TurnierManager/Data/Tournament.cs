using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Serialization;

namespace Tischfussball_TurnierManager.Data
{
    [Serializable]
    public class Tournament : INotifyPropertyChanged
    {
        private int maxStartNumber;

        public ObservableCollection<Player> AttendanceList { get; private set; }

        [XmlIgnore]
        public ListCollectionView AttendanceListView { get; private set; }

        [XmlIgnore]
        public ListCollectionView RankingListView { get; private set; }

        public List<Round> Rounds { get; private set; }

        #region Name

        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnNotifyPropertyChanged("Name");
                }
            }
        }

        private string name;

        #endregion Name

        #region Description

        public string Description
        {
            get { return description; }
            set
            {
                if (value != description)
                {
                    description = value;
                    OnNotifyPropertyChanged("Description");
                }
            }
        }

        private string description;

        #endregion Description

        #region Date

        public DateTime Date
        {
            get { return date; }
            set
            {
                if (value != date)
                {
                    date = value;
                    OnNotifyPropertyChanged("Date");
                }
            }
        }

        private DateTime date;

        #endregion Date

        public Tournament()
        {
            Rounds = new List<Round>();
            AttendanceList = new ObservableCollection<Player>();
            AttendanceListView = new ListCollectionView(AttendanceList);
            RankingListView = new ListCollectionView(AttendanceList);
            AttendanceList.CollectionChanged += AttendanceList_CollectionChanged;
            maxStartNumber = 0;
            Name = Manager.GetUIString("NewTournamentName");
            Date = DateTime.Now;
        }

        public void DummyInit()
        {
            AttendanceList.Add(new Player("Test1", ""));
            AttendanceList.Add(new Player("Test2", ""));
            AttendanceList.Add(new Player("Test3", ""));
            AttendanceList.Add(new Player("Test4", ""));
            AttendanceList.Add(new Player("Test5", ""));
            AttendanceList.Add(new Player("Test6", ""));
            AttendanceList.Add(new Player("Test7", ""));
            AttendanceList.Add(new Player("Test8", ""));
            AttendanceList.Add(new Player("Test9", ""));
            AttendanceList.Add(new Player("Test10", ""));
            AttendanceList.Add(new Player("Test11", ""));
        }

        private void AttendanceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (maxStartNumber < AttendanceList.Count)
            {
                maxStartNumber = Math.Max(0, AttendanceList.Max(p => p.StartNumber));
            }
            if (e.NewItems != null)
            {
                foreach (object o in e.NewItems)
                {
                    if (o is Player && ((Player)o).StartNumber < 0)
                    {
                        maxStartNumber++;
                        ((Player)o).SetStartNumberAutomatic(maxStartNumber);
                    }
                    else if (((Player)o).StartNumber > maxStartNumber)
                    {
                        maxStartNumber = ((Player)o).StartNumber;
                    }
                }
            }
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