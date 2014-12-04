using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tischfussball_TurnierManager.Data
{
    [Serializable]
    public class DataCollection : INotifyPropertyChanged
    {
        public Tournament ActiveTournament
        {
            get { return actualTournament; }
            set
            {
                if (value != actualTournament)
                {
                    actualTournament = value;
                    OnNotifyPropertyChanged("ActiveTournament");
                }
            }
        }

        #region ActiveRound

        public int ActiveRound
        {
            get { if (ActiveTournament == null) activeRound = 0; return activeRound; }
            set
            {
                if (value != activeRound)
                {
                    activeRound = value;
                    OnNotifyPropertyChanged("ActiveRound");
                }
            }
        }

        private int activeRound;

        #endregion ActiveRound

        #region ActiveLanguage

        public Language ActiveLanguage
        {
            get { return activeLanguage; }
            set
            {
                if (value != activeLanguage)
                {
                    activeLanguage = value;
                    OnNotifyPropertyChanged("ActiveLanguage");
                }
            }
        }

        private Language activeLanguage;

        #endregion ActiveLanguage

        public string ActiveRoundDisplay { get { if (ActiveTournament == null) return ""; return ActiveRound.ToString() + "/" + ActiveTournament.Rounds.Count.ToString(); } }

        public string ActiveTournamentSavePath { get; set; }

        private Tournament actualTournament;

        private void OnNotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
            if (propName == "ActiveTournament")
            {
                if (actualTournament == null)
                {
                    ActiveRound = 0;
                }
                else
                {
                    ActiveRound = actualTournament.Rounds.Count;
                }
                OnNotifyPropertyChanged("ActiveRoundDisplay");
            }
            if (propName == "ActiveRound")
            {
                OnNotifyPropertyChanged("ActiveRoundDisplay");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}