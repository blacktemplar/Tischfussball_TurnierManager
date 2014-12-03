using ExcelLibrary.SpreadSheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Tischfussball_TurnierManager.Data;

namespace Tischfussball_TurnierManager
{
    public class Manager
    {
        private Random rnd;
        private MainWindow mainWindow;
        private DataCollection data;
        private string tempFile = "temp.xml";

        private static List<string> saveablePlayerProperties;

        public Manager(MainWindow mainWindow, DataCollection data)
        {
            this.mainWindow = mainWindow;
            this.data = data;
            data.ActiveTournament = null;
            data.PropertyChanged += Data_PropertyChanged;
            rnd = new Random();
        }

        public void CreateNewTournament()
        {
            setTournament(new Tournament());
        }

        public void setTournament(Tournament t)
        {
            data.ActiveTournament = t;
            data.ActiveTournament.AttendanceList.CollectionChanged -= AttendanceList_CollectionChanged;
            data.ActiveTournament.AttendanceList.CollectionChanged += AttendanceList_CollectionChanged;
        }

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveTournament" || e.PropertyName == "ActiveRound")
            {
                if (data.ActiveTournament != null && data.ActiveRound > 0 && data.ActiveRound <= data.ActiveTournament.Rounds.Count)
                {
                    mainWindow.SetRound(data.ActiveTournament.Rounds[data.ActiveRound - 1]);
                }
                else
                {
                    mainWindow.SetRound(null);
                }
            }
        }

        private void AttendanceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object o in e.NewItems)
                {
                    if (o is Player)
                    {
                        ((Player)o).PropertyChanged -= Player_PropertyChanged;
                        ((Player)o).PropertyChanged += Player_PropertyChanged;
                    }
                }
            }
            calculateDisplayNames();
            trySaveTemporaryFile();
        }

        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FirstName" || e.PropertyName == "LastName")
            {
                calculateDisplayNames();
            }
            if (saveablePlayerProperties == null || saveablePlayerProperties.Count == 0)
            {
                createSaveAblePlayerProperties();
            }
            if (saveablePlayerProperties.Contains(e.PropertyName))
            {
                trySaveTemporaryFile();
            }
        }

        private static void createSaveAblePlayerProperties()
        {
            saveablePlayerProperties = new List<string>();
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(typeof(Player)))
            {
                bool ignored = false;

                if (!ignored)
                {
                    foreach (Attribute att in prop.Attributes)
                    {
                        if (att is XmlIgnoreAttribute)
                        {
                            ignored = true;
                            break;
                        }
                    }
                    if (!ignored)
                    {
                        saveablePlayerProperties.Add(prop.Name);
                    }
                }
            }
        }

        public void NewRound(int ind = -1)
        {
            if (data.ActiveTournament != null)
            {
                List<Player> activePlayers = new List<Player>(
                    (from p in data.ActiveTournament.AttendanceList
                     where p.IsActive
                     select p).OrderBy(player => player.GamesDrawnLotsFor));

                int numberOfTables = activePlayers.Count / 4;
                int count = numberOfTables * 4;
                activePlayers.RemoveRange(count, activePlayers.Count - count);
                activePlayers = activePlayers.OrderBy(player => rnd.Next()).ToList();
                double[] probabilities = new double[activePlayers.Count];
                List<Team> teams = new List<Team>();
                for (int i = 0; i < numberOfTables * 4; i = i + 2)
                {
                    Dictionary<Player, int> dict = new Dictionary<Player, int>();
                    foreach (Round r in data.ActiveTournament.Rounds)
                    {
                        foreach (Fixture f in r)
                        {
                            Player partner = f.Team1.getPartner(activePlayers[i]);
                            if (partner == null)
                            {
                                partner = f.Team2.getPartner(activePlayers[i]);
                            }
                            if (partner != null)
                            {
                                if (!dict.ContainsKey(partner))
                                {
                                    dict.Add(partner, 0);
                                }
                                dict[partner]++;
                            }
                        }
                    }
                    double sum = 0;
                    for (int j = i + 1; j < activePlayers.Count; j++)
                    {
                        double prob = 1;
                        if (dict.ContainsKey(activePlayers[j]))
                        {
                            prob = prob * Math.Pow(2, -dict[activePlayers[j]]);
                        }
                        sum += prob;
                        probabilities[j] = sum;
                    }
                    Double rand = rnd.NextDouble() * sum;
                    int j0 = -1;
                    for (int j = i + 1; j < activePlayers.Count; j++)
                    {
                        if (j == activePlayers.Count - 1 || rand < probabilities[j])
                        {
                            j0 = j;
                            j = activePlayers.Count;
                        }
                    }
                    Player tmp = activePlayers[i + 1];
                    activePlayers[i + 1] = activePlayers[j0];
                    activePlayers[j0] = tmp;
                    teams.Add(new Team(activePlayers[i], activePlayers[i + 1]));
                }
                teams = teams.OrderBy(t => t.Strength).ToList();
                {
                    Round r = new Round();
                    for (int i = 0; i < teams.Count - 1; i = i + 2)
                    {
                        Fixture f = new Fixture(teams[i], teams[i + 1]);
                        r.Add(f);
                        f.PropertyChanged += fixture_PropertyChanged;
                    }
                    if (ind == -1)
                    {
                        ind = data.ActiveTournament.Rounds.Count;
                    }
                    data.ActiveTournament.Rounds.Insert(ind, r);
                }
                mainWindow.SetMaxRound(data.ActiveTournament.Rounds.Count);

                data.ActiveRound = ind + 1;

                mainWindow.SetRound(data.ActiveTournament.Rounds[ind]);
            }
            CalculatePlayerStats();
            trySaveTemporaryFile();
        }

        private void fixture_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CalculatePlayerStats();
            trySaveTemporaryFile();
        }

        public void CalculatePlayerStats()
        {
            foreach (Player p in data.ActiveTournament.AttendanceList)
            {
                p.GamesPlayed = 0;
                p.Points = 0;
                p.GoalDifference = 0;
                p.GamesDrawnLotsFor = 0;
            }
            foreach (Round r in data.ActiveTournament.Rounds)
            {
                foreach (Fixture f in r)
                {
                    f.Team1.Player1.GamesDrawnLotsFor++;
                    f.Team1.Player2.GamesDrawnLotsFor++;
                    f.Team2.Player1.GamesDrawnLotsFor++;
                    f.Team2.Player2.GamesDrawnLotsFor++;
                    if (f.Goals1 != f.Goals2)
                    {
                        f.Team1.Player1.GamesPlayed++;
                        f.Team1.Player2.GamesPlayed++;
                        f.Team2.Player1.GamesPlayed++;
                        f.Team2.Player2.GamesPlayed++;
                        if (f.Goals1 > f.Goals2)
                        {
                            f.Team1.Player1.Points++;
                            f.Team1.Player2.Points++;
                        }
                        else
                        {
                            f.Team2.Player1.Points++;
                            f.Team2.Player2.Points++;
                        }
                        f.Team1.Player1.GoalDifference += f.Goals1 - f.Goals2;
                        f.Team1.Player2.GoalDifference += f.Goals1 - f.Goals2;
                        f.Team2.Player1.GoalDifference += f.Goals2 - f.Goals1;
                        f.Team2.Player2.GoalDifference += f.Goals2 - f.Goals1;
                    }
                }
            }
            int rank = 1;
            foreach (Player p in data.ActiveTournament.AttendanceList
                .OrderByDescending(pl => pl.GoalDifference)
                .OrderByDescending(pl => pl.Points)
                .OrderByDescending(pl => pl.PointsPerGame))
            {
                p.Rank = rank++;
            }
        }

        private void calculateDisplayNames()
        {
            foreach (Player p in data.ActiveTournament.AttendanceList)
            {
                string displayName = p.FirstName;
                int max = -1;
                foreach (Player q in data.ActiveTournament.AttendanceList)
                {
                    if (q != p && q.FirstName == p.FirstName)
                    {
                        int i;
                        for (i = 0; i < p.LastName.Length; i++)
                        {
                            if (q.LastName.Length <= i || q.LastName[i] != p.LastName[i])
                            {
                                if (i > max)
                                {
                                    max = i;
                                }
                                i = p.LastName.Length + 1;
                            }
                        }
                        if (i == p.LastName.Length)
                        {
                            max = p.LastName.Length - 1;
                        }
                    }
                }
                if (max > -1)
                {
                    displayName += " " + p.LastName.Substring(0, max + 1);
                    if (max < p.LastName.Length - 1)
                    {
                        displayName += ".";
                    }
                }
                p.DisplayName = displayName;
            }
        }

        public bool Save(string path = null)
        {
            if (String.IsNullOrEmpty(path))
            {
                path = data.ActiveTournamentSavePath;
            }
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Tournament));
                using (TextWriter writer = new StreamWriter(path))
                {
                    ser.Serialize(writer, data.ActiveTournament);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool Load(string path = null)
        {
            if (String.IsNullOrEmpty(path))
            {
                path = data.ActiveTournamentSavePath;
            }
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Tournament));
                Tournament tournament = null;
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    tournament = (Tournament)ser.Deserialize(stream);
                }
                setTournament(tournament);
                ReconnectPlayers();
                SetFixtureListeners();
                calculateDisplayNames();
                CalculatePlayerStats();
                mainWindow.ResetMaxRound();
                mainWindow.SetMaxRound(data.ActiveTournament.Rounds.Count);
                if (data.ActiveRound == 0)
                {
                    mainWindow.SetRound(null);
                }
                else
                {
                    mainWindow.SetRound(data.ActiveTournament.Rounds[data.ActiveRound - 1]);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void SetFixtureListeners()
        {
            foreach (Round r in data.ActiveTournament.Rounds)
            {
                foreach (Fixture f in r)
                {
                    f.PropertyChanged -= new PropertyChangedEventHandler(fixture_PropertyChanged);
                    f.PropertyChanged += new PropertyChangedEventHandler(fixture_PropertyChanged);
                }
            }
        }

        private void ReconnectPlayers()
        {
            Dictionary<int, Player> players = new Dictionary<int, Player>();
            foreach (Player p in data.ActiveTournament.AttendanceList)
            {
                players.Add(p.StartNumber, p);
                p.PropertyChanged -= Player_PropertyChanged;
                p.PropertyChanged += Player_PropertyChanged;
            }
            foreach (Round r in data.ActiveTournament.Rounds)
            {
                foreach (Fixture f in r)
                {
                    f.Team1.Player1 = players[f.Team1.Player1.StartNumber];
                    f.Team1.Player2 = players[f.Team1.Player2.StartNumber];
                    f.Team2.Player1 = players[f.Team2.Player1.StartNumber];
                    f.Team2.Player2 = players[f.Team2.Player2.StartNumber];
                }
            }
        }

        public void exportXLS(string filePath, bool roundsTogether)
        {
            Workbook workbook = new Workbook();
            Worksheet worksheet = new Worksheet("Turnierdaten");
            worksheet.Cells[0, 0] = new Cell("Name");
            worksheet.Cells[1, 0] = new Cell("Datum");
            worksheet.Cells[2, 0] = new Cell("Beschreibung");
            worksheet.Cells[0, 1] = new Cell(data.ActiveTournament.Name);
            worksheet.Cells[1, 1] = new Cell(data.ActiveTournament.Date, CellFormat.Date);
            worksheet.Cells[2, 1] = new Cell(data.ActiveTournament.Description);
            workbook.Worksheets.Add(worksheet);

            worksheet = new Worksheet("Teilnehmerliste");
            worksheet.Cells[0, 0] = new Cell("Startnummer");
            worksheet.Cells[0, 1] = new Cell("Vorname");
            worksheet.Cells[0, 2] = new Cell("Nachname");
            worksheet.Cells[0, 3] = new Cell("Anfangsstärke");
            worksheet.Cells[0, 4] = new Cell("Wird ausgelost");

            int row = 1;
            foreach (Player p in data.ActiveTournament.AttendanceList)
            {
                worksheet.Cells[row, 0] = new Cell(p.StartNumber);
                worksheet.Cells[row, 1] = new Cell(p.FirstName);
                worksheet.Cells[row, 2] = new Cell(p.LastName);
                worksheet.Cells[row, 3] = new Cell(p.BeginningStrength);
                worksheet.Cells[row, 4] = new Cell(p.IsActive ? "Ja" : "Nein");
                row++;
            }

            workbook.Worksheets.Add(worksheet);

            row = 2;

            worksheet = createRoundWorksheetWithHeaders("Auslosungen", 1);

            int roundNumber = 1;

            foreach (Round r in data.ActiveTournament.Rounds)
            {
                int colOffset = 0;
                if (!roundsTogether)
                {
                    worksheet = createRoundWorksheetWithHeaders("Runde " + roundNumber.ToString());
                    row = 2;
                }
                else
                {
                    colOffset = 1;
                    worksheet.Cells[row, 0] = new Cell("Runde " + roundNumber.ToString());
                }

                foreach (Fixture f in r)
                {
                    worksheet.Cells[row, colOffset + 0] = new Cell(f.Team1.DisplayName);
                    worksheet.Cells[row, colOffset + 1] = new Cell(f.Goals1);
                    worksheet.Cells[row, colOffset + 2] = new Cell(":");
                    worksheet.Cells[row, colOffset + 3] = new Cell(f.Goals2);
                    worksheet.Cells[row, colOffset + 4] = new Cell(f.Team2.DisplayName);
                    row++;
                }
                row++;

                if (!roundsTogether)
                {
                    workbook.Worksheets.Add(worksheet);
                }

                roundNumber++;
            }

            if (roundsTogether)
            {
                workbook.Worksheets.Add(worksheet);
            }

            worksheet = new Worksheet("Rangliste");
            worksheet.Cells[0, 0] = new Cell("Rang");
            worksheet.Cells[0, 1] = new Cell("Spieler");
            worksheet.Cells[0, 2] = new Cell("Punkte pro Spiel");
            worksheet.Cells[0, 3] = new Cell("Punkte");
            worksheet.Cells[0, 4] = new Cell("Tordifferenz");
            worksheet.Cells[0, 5] = new Cell("Spiele gespielt");

            row = 1;
            foreach (Player p in
                from player in data.ActiveTournament.AttendanceList
                orderby player.Rank
                select player)
            {
                worksheet.Cells[row, 0] = new Cell(p.Rank);
                worksheet.Cells[row, 1] = new Cell(p.DisplayName);
                worksheet.Cells[row, 2] = new Cell(p.PointsPerGame);
                worksheet.Cells[row, 3] = new Cell(p.Points);
                worksheet.Cells[row, 4] = new Cell(p.GoalDifference);
                worksheet.Cells[row, 5] = new Cell(p.GamesPlayed);
                row++;
            }

            workbook.Worksheets.Add(worksheet);

            workbook.Save(filePath);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = filePath;
            List<string> possibleExcelPrograms = new List<string>() { "EXCEL", "SCALC" };
            foreach (string p in possibleExcelPrograms)
            {
                try
                {
                    startInfo.FileName = p + ".EXE";
                    Process.Start(startInfo);
                    break;
                }
                catch
                {
                }
            }
        }

        private Worksheet createRoundWorksheetWithHeaders(string name, ushort beginColumn = 0)
        {
            Worksheet res = new Worksheet(name);
            res.Cells[0, beginColumn + 0] = new Cell("Team1");
            res.Cells[0, beginColumn + 1] = new Cell("Tore Team1");
            res.Cells[0, beginColumn + 3] = new Cell("Tore Team2");
            res.Cells[0, beginColumn + 4] = new Cell("Team2");
            res.Cells.ColumnWidth[(ushort)(beginColumn + 1)] = 1000;
            res.Cells.ColumnWidth[(ushort)(beginColumn + 2)] = 500;
            res.Cells.ColumnWidth[(ushort)(beginColumn + 3)] = 1000;
            return res;
        }

        public void tryLoadTemporaryFile()
        {
            try
            {
                Load(tempFile);
                data.ActiveTournamentSavePath = data.ActiveTournament.LastSavePath;
            }
            catch
            {
            }
        }

        public void trySaveTemporaryFile()
        {
            try
            {
                Save(tempFile);
            }
            catch
            {
            }
        }

        public bool TournamentChangedSinceLastSave()
        {
            if (data == null || data.ActiveTournament == null)
            {
                return false;
            }
            if (String.IsNullOrEmpty(data.ActiveTournamentSavePath))
            {
                return true;
            }
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Tournament));
                using (StringWriter writer = new StringWriter())
                {
                    ser.Serialize(writer, data.ActiveTournament);
                    string s1 = writer.ToString();
                    s1 = s1.Substring(s1.IndexOf('\n') + 1);
                    using (StreamReader reader = new StreamReader(data.ActiveTournamentSavePath))
                    {
                        reader.ReadLine();
                        return s1 != reader.ReadToEnd();
                    }
                }
            }
            catch
            { return false; }
        }

        public void ForgetChanges()
        {
            try
            {
                File.Copy(data.ActiveTournamentSavePath, tempFile, true);
            }
            catch
            {
            }
        }

        public void RecalculatePlayerstats(int ind)
        {
            Round r = data.ActiveTournament.Rounds[ind];
            data.ActiveTournament.Rounds.RemoveAt(ind);
            CalculatePlayerStats();
            NewRound(ind);
        }
    }
}