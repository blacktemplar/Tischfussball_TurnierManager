using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tischfussball_TurnierManager.Data
{
    [Serializable]
    public class Team : INotifyPropertyChanged
    {
        #region Properties

        public Player Player1
        {
            get
            {
                return player1;
            }
            set
            {
                player1 = value;
                player1.PropertyChanged -= Player_PropertyChanged;
                player1.PropertyChanged += Player_PropertyChanged;
                OnNotifyPropertyChanged("Player1");
            }
        }

        private Player player1;

        public Player Player2
        {
            get
            {
                return player2;
            }
            set
            {
                player2 = value;
                player2.PropertyChanged -= Player_PropertyChanged;
                player2.PropertyChanged += Player_PropertyChanged;
                OnNotifyPropertyChanged("Player2");
            }
        }

        private Player player2;

        public string DisplayName
        {
            get
            {
                return Player1.DisplayName + " und " + Player2.DisplayName;
            }
        }

        #endregion Properties

        public Team(Player p1, Player p2)
        {
            Player1 = p1;
            Player2 = p2;
        }

        public Team()
        {
        }

        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DisplayName")
            {
                OnNotifyPropertyChanged("DisplayName");
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

        public override string ToString()
        {
            return DisplayName;
        }

        public Player getPartner(Player p)
        {
            if (Player1 == p)
            {
                return Player2;
            }
            else if (Player2 == p)
            {
                return Player1;
            }
            return null;
        }

        public double Strength
        {
            get
            {
                return Player1.Strength + Player2.Strength;
            }
        }
    }
}