using MyDataGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Tischfussball_TurnierManager.Data
{
    [Serializable]
    public class Player : INotifyPropertyChanged
    {
        #region Properties

        #region StartNumber

        [LocalizedDisplayName("ColStartNumber")]
        [Viewable(ViewType.AttendanceView)]
        [ReadOnly(true)]
        [Ordering(0)]
        public int StartNumber
        {
            get { return startNumber; }
            set
            {
                if (value != startNumber)
                {
                    startNumber = value;
                    OnNotifyPropertyChanged("StartNumber");
                }
            }
        }

        private int startNumber;

        #endregion StartNumber

        #region FirstName

        [LocalizedDisplayName("ColFirstName")]
        [MyDataGrid.Editable(ViewType.AttendanceView)]
        [Ordering(1)]
        public string FirstName
        {
            get { return firstName; }
            set
            {
                if (value != firstName)
                {
                    firstName = value;
                    int test = 1;
                    OnNotifyPropertyChanged("FirstName");
                }
            }
        }

        private string firstName;

        #endregion FirstName

        #region LastName

        [LocalizedDisplayName("ColLastName")]
        [MyDataGrid.Editable(ViewType.AttendanceView)]
        [Ordering(2)]
        public string LastName
        {
            get { return lastName; }
            set
            {
                if (value != lastName)
                {
                    lastName = value;
                    OnNotifyPropertyChanged("LastName");
                }
            }
        }

        private string lastName;

        #endregion LastName

        #region BeginningStrength

        [LocalizedDisplayName("ColBeginningStrength")]
        [MyDataGrid.Editable(ViewType.AttendanceView)]
        [Ordering(3)]
        public double BeginningStrength
        {
            get { return beginningStrength; }
            set
            {
                if (value != beginningStrength)
                {
                    beginningStrength = value;
                    OnNotifyPropertyChanged("BeginningStrength");
                }
            }
        }

        private double beginningStrength;

        #endregion BeginningStrength

        #region IsActive

        [LocalizedDisplayName("ColIsActive")]
        [MyDataGrid.Editable(ViewType.AttendanceView)]
        [Ordering(4)]
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (value != isActive)
                {
                    isActive = value;
                    OnNotifyPropertyChanged("IsActive");
                }
            }
        }

        private bool isActive;

        #endregion IsActive

        #region DisplayName

        [Browsable(false)]
        [Viewable(ViewType.RankingView)]
        [LocalizedDisplayName("ColPlayer")]
        [XmlIgnore]
        [Ordering(6)]
        public string DisplayName
        {
            get { return displayName; }
            internal set
            {
                if (value != displayName)
                {
                    displayName = value;
                    OnNotifyPropertyChanged("DisplayName");
                }
            }
        }

        private string displayName;

        #endregion DisplayName

        #region GamesPlayed

        [XmlIgnore]
        [Viewable(ViewType.RankingView)]
        [LocalizedDisplayName("ColGamesPlayed")]
        [Browsable(false)]
        [Ordering(10)]
        public int GamesPlayed
        {
            get { return gamesPlayed; }
            internal set
            {
                if (value != gamesPlayed)
                {
                    gamesPlayed = value;
                    OnNotifyPropertyChanged("GamesPlayed");
                }
            }
        }

        private int gamesPlayed;

        #endregion GamesPlayed

        #region GamesDrawnLotsFor

        [XmlIgnore]
        [Browsable(false)]
        public int GamesDrawnLotsFor
        {
            get { return gamesDrawnLotsFor; }
            internal set
            {
                if (value != gamesDrawnLotsFor)
                {
                    gamesDrawnLotsFor = value;
                    OnNotifyPropertyChanged("GamesDrawnLotsFor");
                }
            }
        }

        private int gamesDrawnLotsFor;

        #endregion GamesDrawnLotsFor

        #region Points

        [XmlIgnore]
        [Viewable(ViewType.RankingView)]
        [LocalizedDisplayName("ColPoints")]
        [Browsable(false)]
        [Ordering(8)]
        public int Points
        {
            get { return points; }
            internal set
            {
                if (value != points)
                {
                    points = value;
                    OnNotifyPropertyChanged("Points");
                }
            }
        }

        private int points;

        #endregion Points

        #region GoalDifference

        [XmlIgnore]
        [Viewable(ViewType.RankingView)]
        [LocalizedDisplayName("ColGoaldifference")]
        [Browsable(false)]
        [Ordering(9)]
        public int GoalDifference
        {
            get { return goalDifference; }
            internal set
            {
                if (value != goalDifference)
                {
                    goalDifference = value;
                    OnNotifyPropertyChanged("GoalDifference");
                }
            }
        }

        private int goalDifference;

        #endregion GoalDifference

        #region Rank

        [XmlIgnore]
        [Viewable(ViewType.RankingView)]
        [LocalizedDisplayName("ColRank")]
        [Browsable(false)]
        [Ordering(5)]
        public int Rank
        {
            get { return rank; }
            internal set
            {
                if (value != rank)
                {
                    rank = value;
                    OnNotifyPropertyChanged("Rank");
                }
            }
        }

        private int rank;

        #endregion Rank

        [XmlIgnore]
        [Viewable(ViewType.RankingView)]
        [LocalizedDisplayName("ColPointsPerGame")]
        [Ordering(7)]
        [Browsable(false)]
        public double PointsPerGame
        {
            get
            {
                if (GamesPlayed == 0)
                {
                    return 0.5;
                }
                else
                {
                    return (double)Points / (double)GamesPlayed;
                }
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public double Strength
        {
            get
            {
                if (GamesPlayed == 0)
                {
                    return BeginningStrength;
                }
                return PointsPerGame;
            }
        }

        #endregion Properties

        public Player()
        {
            BeginningStrength = 0.5;
            GamesPlayed = 0;
            IsActive = true;
            StartNumber = -1;
        }

        public Player(string firstName, string lastName)
            : this()
        {
            FirstName = firstName;
            LastName = lastName;
        }

        private void OnNotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
            if (propName == "Points" || propName == "GamesPlayed" || propName == "BeginningStrength")
            {
                OnNotifyPropertyChanged("PointsPerGame");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetStartNumberAutomatic(int startNumber)
        {
            StartNumber = startNumber;
        }
    }
}