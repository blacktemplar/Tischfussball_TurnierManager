using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tischfussball_TurnierManager.Data;
using WPFLocalizeExtension.Engine;

namespace Tischfussball_TurnierManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Manager man;
        private int maxRound;
        private DataCollection data;

        public MainWindow()
        {
            maxRound = 0;
            InitializeComponent();
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            data = new DataCollection();
            data.ActiveLanguage = Data.Language.English;
            man = new Manager(this, data);
            data = man.tryLoadTemporaryFile();
            if (man.TournamentChangedSinceLastSave())
            {
                MessageBoxResult res = MessageBox.Show("Das zuletzt geöffnete Turnier wurde nicht richtig abgespeichert, wollen Sie die letzten Änderungen wiederherstellen?", "Änderungen wiederherstellen?", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No)
                {
                    man.Load();
                }
            }
            this.DataContext = data;
        }

        private void MI_CreateTournament_Click(object sender, RoutedEventArgs e)
        {
            if (trySaveIfNeeded() == false)
            {
                return;
            }
            man.CreateNewTournament();
        }

        private bool? trySaveIfNeeded()
        {
            if (man.TournamentChangedSinceLastSave())
            {
                MessageBoxResult res = MessageBox.Show("Wollen Sie die Änderungen am aktuell geladenen Turnier abspeichern?", "Änderungen speichern?", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel)
                    return false;
                if (res == MessageBoxResult.Yes)
                {
                    if (!Save())
                    {
                        return false;
                    }
                }
                if (res == MessageBoxResult.No)
                {
                    man.ForgetChanges();
                    return null;
                }
            }
            return true;
        }

        private void MINewRound_Click(object sender, RoutedEventArgs e)
        {
            man.NewRound();
        }

        public void ResetMaxRound()
        {
            MIRounds.Items.Clear();
            this.maxRound = 0;
        }

        public void SetRound(Round round)
        {
            this.TIFixtures.DataContext = round;
        }

        public void SetMaxRound(int maxRound)
        {
            if (maxRound > this.maxRound)
            {
                for (int i = this.maxRound + 1; i <= maxRound; i++)
                {
                    MenuItem newItem = new MenuItem();
                    newItem.Header = "Runde " + i.ToString();
                    newItem.Tag = i;
                    newItem.Click += MIRound_Click;
                    MIRounds.Items.Add(newItem);
                }
                this.maxRound = maxRound;
            }
        }

        public int MaxRound
        {
            get
            {
                return maxRound;
            }
        }

        private void MIRound_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem && ((MenuItem)sender).Tag is int)
            {
                int round = (int)((MenuItem)sender).Tag;
                data.ActiveRound = round;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((TabControl)sender).SelectedItem == TIRanking)
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(dgRanking.ItemsSource);
                dataView.SortDescriptions.Clear();
                dataView.SortDescriptions.Add(new SortDescription("Rank", ListSortDirection.Ascending));
            }
        }

        private void MI_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            Save_As();
        }

        private bool Save_As()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = data.ActiveTournament.Name; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML document (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                data.ActiveTournamentSavePath = dlg.FileName;
                if (!man.Save())
                {
                    MessageBox.Show("Das Turnier konnte am angegebenen Pfad nicht abgespeichert werden!");
                    return false;
                }
            }
            return true;
        }

        private bool Save()
        {
            if (String.IsNullOrEmpty(data.ActiveTournamentSavePath))
            {
                return Save_As();
            }
            else
            {
                if (!man.Save())
                {
                    MessageBox.Show("Das Turnier konnte an folgendem Pfad nicht abgespeichert werden: " + data.ActiveTournamentSavePath);
                    return false;
                }
            }
            return true;
        }

        private void MI_Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MI_Load_Click(object sender, RoutedEventArgs e)
        {
            if (trySaveIfNeeded() == false)
            {
                return;
            }
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML document (.xml)|*.xml";
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                data.ActiveTournamentSavePath = filename;
                if (!man.Load())
                {
                    MessageBox.Show("Das Turnier konnte nicht geladen werden!");
                }
            }
        }

        private void MI_Export_XLS(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = data.ActiveTournament.Name; // Default file name
            dlg.DefaultExt = ".xls"; // Default file extension
            dlg.Filter = "Microsoft Excel document (.xls)|*.xls"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            if (result != true)
                return;

            MessageBoxResult res = MessageBox.Show("Wollen Sie alle Runden in einer Tabelle abspeichern oder jede einzeln? (Ja für, alle in einer)", "Runden zusammen abspeichern?", MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Cancel)
                return;
            bool retry = true;
            while (retry)
            {
                retry = false;
                try
                {
                    man.exportXLS(dlg.FileName, res == MessageBoxResult.Yes);
                }
                catch
                {
                    retry = MessageBoxResult.Yes == MessageBox.Show("Es ist ein Fehler beim exportieren aufgetreten? Ist das Dokument schon geöffnet? Überprüfen Sie ob Sie ausreichend Berechtigung besitzen um an diesen Ort zu exportieren. \n \n Wiederholen?", "Ein Fehler ist aufgetreten, wiederholen?", MessageBoxButton.YesNo);
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            bool? res = trySaveIfNeeded();
            if (res == false)
            {
                e.Cancel = true;
            }
            else if (res == true)
            {
                man.trySaveTemporaryFile();
            }
        }

        private void MIReCalculateRound_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes != MessageBox.Show("Sind Sie sich sicher, dass Sie die Auslung und die Ergebnisse der " + data.ActiveRound + ". Runde überschreiben wollen?", "Sind sie sich sicher?", MessageBoxButton.YesNo))
                return;
            man.RecalculatePlayerstats(data.ActiveRound - 1);
        }

        private void MILanguage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Control && (sender as Control).Tag is Language)
            {
                Language newL = (Language)(sender as Control).Tag;
                data.ActiveLanguage = newL;
            }
        }

        public void RefreshBindings()
        {
            this.DataContext = null;
            this.DataContext = data;
            SetRound(null);
            if (this.data.ActiveTournament != null && this.data.ActiveRound > 0 && this.data.ActiveTournament.Rounds.Count >= this.data.ActiveRound)
            {
                SetRound(data.ActiveTournament.Rounds[data.ActiveRound - 1]);
            }
        }
    }
}