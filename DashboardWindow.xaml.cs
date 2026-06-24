using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace DriveVision
{
    public class EstensioneStat
    {
        public string NomeEstensione { get; set; }
        public long DimensioneTotaleByte { get; set; }
        public int ConteggioFiles { get; set; }
        public Brush PennelloColore { get; set; }

        public string DimensioneFormattata
        {
            get
            {
                if (DimensioneTotaleByte == 0) return "0 B";
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                int order = 0;
                double len = DimensioneTotaleByte;
                while (len >= 1024 && order < sizes.Length - 1) { order++; len /= 1024; }
                return $"{len:0.##} {sizes[order]}";
            }
        }
    }

    public static class ShellHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEOPSTRUCT { public IntPtr hwnd; public uint wFunc; public string pFrom; public string pTo; public ushort fFlags; public bool fAnyOperationsAborted; public IntPtr hNameMappings; public string lpszProgressTitle; }
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);
        public const uint FO_DELETE = 0x0003; public const ushort FOF_ALLOWUNDO = 0x0040; public const ushort FOF_NOCONFIRMATION = 0x0010; public const ushort FOF_SILENT = 0x0004;

        public static void SpostaNelCestino(string percorso)
        {
            var shf = new SHFILEOPSTRUCT { wFunc = FO_DELETE, pFrom = percorso + '\0' + '\0', fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT };
            int result = SHFileOperation(ref shf);
            if (result != 0 && result != 1223) throw new Exception("Codice errore di sistema: " + result);
        }
    }

    public partial class DashboardWindow : Window
    {
        private FileNode _nodoCorrente;
        private Stack<FileNode> _storicoNavigazione = new Stack<FileNode>();
        private string _letteraDisco;
        private Dictionary<string, Color> _coloriEstensioni = new Dictionary<string, Color>();
        private Random _rnd = new Random();

        public DashboardWindow(FileNode rootNode)
        {
            InitializeComponent();

            // Applica Traduzioni Testi Colonne e UI
            ((DataGridTextColumn)GridRisultati.Columns[0]).Header = Translator.Get("NomeFile");
            ((DataGridTextColumn)GridRisultati.Columns[1]).Header = Translator.Get("Tipo");
            ((DataGridTextColumn)GridRisultati.Columns[2]).Header = Translator.Get("Dimensione");
            ((DataGridTemplateColumn)GridRisultati.Columns[3]).Header = Translator.Get("Impatto");

            ((DataGridTextColumn)GridEstensioni.Columns[1]).Header = Translator.Get("Estensione");
            ((DataGridTextColumn)GridEstensioni.Columns[2]).Header = Translator.Get("Dimensione");
            ((DataGridTextColumn)GridEstensioni.Columns[3]).Header = Translator.Get("File");

            InizializzaPaletteColori();
            _letteraDisco = rootNode.Nome.Substring(0, 1) + ":\\";
            ImpostaStatisticheDisco(_letteraDisco);
            CaricaNodo(rootNode);
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            if (_storicoNavigazione.Count > 0) CaricaNodo(_storicoNavigazione.Pop());
            else { MainWindow mainWindow = new MainWindow(); mainWindow.Show(); this.Close(); }
        }

        private void BtnAggiorna_Click(object sender, RoutedEventArgs e)
        {
            var discoCorrente = new DriveCard { Lettera = _letteraDisco, FileSystem = "NTFS" };
            ScanWindow scanWindow = new ScanWindow(discoCorrente);
            scanWindow.Show();
            this.Close();
        }

        private void CaricaNodo(FileNode nodo)
        {
            _nodoCorrente = nodo;
            TxtTitolo.Text = Translator.Get("AnalisiDi") + nodo.Nome;

            var risultatiOrdinati = nodo.Figli.Where(f => f.DimensioneByte > 0).OrderByDescending(f => f.DimensioneByte).ToList();
            GridRisultati.ItemsSource = risultatiOrdinati;
            BtnIndietro.ToolTip = _storicoNavigazione.Count > 0 ? Translator.Get("TornaSu") : Translator.Get("TornaHome");

            AggiornaStatisticheEstensioni(nodo);
            DisegnaTreemap();
        }

        private void GridRisultati_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridRisultati.SelectedItem is FileNode selezionato && selezionato.IsDirectory)
            {
                _storicoNavigazione.Push(_nodoCorrente); CaricaNodo(selezionato);
            }
        }

        private void ImpostaStatisticheDisco(string pathRadice)
        {
            try
            {
                DriveInfo drive = new DriveInfo(pathRadice.Substring(0, 1) + ":\\");
                long totale = drive.TotalSize; long libero = drive.TotalFreeSpace; long occupato = totale - libero;

                TxtSpazioTotale.Text = Translator.Get("TotaleDisco") + FormattaByte(totale);
                TxtSpazioOccupato.Text = Translator.Get("Occupato") + FormattaByte(occupato);
                TxtSpazioLibero.Text = Translator.Get("Libero") + FormattaByte(libero);

                TxtSpazioOccupato.Foreground = Brushes.Red;
                TxtSpazioLibero.Foreground = Brushes.LimeGreen;
            }
            catch { TxtSpazioTotale.Text = "Statistiche disco non disponibili"; }
        }

        private string FormattaByte(long bytes)
        {
            if (bytes == 0) return "0 B";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0; double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1) { order++; len /= 1024; }
            return $"{len:0.##} {sizes[order]}";
        }

        private void InizializzaPaletteColori()
        {
            _coloriEstensioni[".exe"] = Color.FromRgb(0, 120, 215); _coloriEstensioni[".dll"] = Color.FromRgb(255, 80, 80);
            _coloriEstensioni[".sys"] = Color.FromRgb(200, 0, 0); _coloriEstensioni[".zip"] = Color.FromRgb(255, 165, 0);
            _coloriEstensioni[".rar"] = Color.FromRgb(255, 140, 0); _coloriEstensioni[".iso"] = Color.FromRgb(255, 215, 0);
            _coloriEstensioni[".mp4"] = Color.FromRgb(138, 43, 226); _coloriEstensioni[".mkv"] = Color.FromRgb(148, 0, 211);
            _coloriEstensioni[".mp3"] = Color.FromRgb(0, 206, 209); _coloriEstensioni[".jpg"] = Color.FromRgb(50, 205, 50);
            _coloriEstensioni[".png"] = Color.FromRgb(34, 139, 34); _coloriEstensioni[".txt"] = Color.FromRgb(169, 169, 169);
            _coloriEstensioni[".dat"] = Color.FromRgb(105, 105, 105);
        }

        private Color OttieniColoreEstensione(string estensione)
        {
            if (!_coloriEstensioni.ContainsKey(estensione))
            {
                byte r = (byte)_rnd.Next(100, 255); byte g = (byte)_rnd.Next(100, 255); byte b = (byte)_rnd.Next(100, 255);
                _coloriEstensioni[estensione] = Color.FromRgb(r, g, b);
            }
            return _coloriEstensioni[estensione];
        }

        private void AggiornaStatisticheEstensioni(FileNode nodoPartenza)
        {
            var dictEstensioni = new Dictionary<string, EstensioneStat>();
            EsploraNodiPerEstensioni(nodoPartenza, dictEstensioni);
            GridEstensioni.ItemsSource = dictEstensioni.Values.OrderByDescending(x => x.DimensioneTotaleByte).ToList();
        }

        private void EsploraNodiPerEstensioni(FileNode nodo, Dictionary<string, EstensioneStat> dict)
        {
            foreach (var figlio in nodo.Figli)
            {
                if (figlio.IsDirectory) EsploraNodiPerEstensioni(figlio, dict);
                else if (figlio.DimensioneByte > 0)
                {
                    string est = System.IO.Path.GetExtension(figlio.Nome).ToLower();
                    if (string.IsNullOrWhiteSpace(est)) est = "Sconosciuto";

                    if (!dict.TryGetValue(est, out EstensioneStat stat))
                    {
                        stat = new EstensioneStat { NomeEstensione = est, PennelloColore = new SolidColorBrush(OttieniColoreEstensione(est)) };
                        dict[est] = stat;
                    }
                    stat.DimensioneTotaleByte += figlio.DimensioneByte; stat.ConteggioFiles++;
                }
            }
        }

        private void GridRisultati_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest(GridRisultati, e.GetPosition(GridRisultati));
            if (hit == null) return;
            var row = TrovaParente<DataGridRow>(hit.VisualHit);
            if (row != null && row.Item is FileNode nodo)
            {
                GridRisultati.SelectedItem = nodo;
                ContextMenu menu = new ContextMenu { Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)), Foreground = Brushes.White, BorderBrush = Brushes.Gray };

                string lblCartellaOFile = nodo.IsDirectory ? Translator.Get("Cartella") : Translator.Get("File");

                if (nodo.IsDirectory)
                {
                    var mnuApri = new MenuItem { Header = Translator.Get("ApriEsplora") };
                    mnuApri.Click += (s, ev) => System.Diagnostics.Process.Start("explorer.exe", $"\"{nodo.PercorsoCompleto}\"");
                    menu.Items.Add(mnuApri);
                    menu.Items.Add(new Separator { Background = Brushes.Gray });
                }

                var mnuCestino = new MenuItem { Header = $"{Translator.Get("Elimina")} {lblCartellaOFile.ToLower()}" };
                mnuCestino.Click += (s, ev) => EliminaElemento(nodo, false);
                menu.Items.Add(mnuCestino);

                var mnuDefinitivo = new MenuItem { Header = $"{Translator.Get("EliminaDef")} {lblCartellaOFile.ToLower()}", Foreground = Brushes.Salmon };
                mnuDefinitivo.Click += (s, ev) => EliminaElemento(nodo, true);
                menu.Items.Add(mnuDefinitivo);

                menu.IsOpen = true; e.Handled = true;
            }
        }

        private T TrovaParente<T>(DependencyObject figlio) where T : DependencyObject
        {
            DependencyObject parente = VisualTreeHelper.GetParent(figlio);
            if (parente == null) return null;
            if (parente is T t) return t;
            return TrovaParente<T>(parente);
        }

        private void EliminaElemento(FileNode nodo, bool definitivo)
        {
            string lbl = nodo.IsDirectory ? Translator.Get("Cartella") : Translator.Get("File");
            string msg = definitivo
                ? $"{Translator.Get("DomandaDefinitiva")} {lbl.ToLower()} '{nodo.Nome}'?{Translator.Get("NonAnnullabile")}"
                : $"{Translator.Get("DomandaCestino")} {lbl.ToLower()} '{nodo.Nome}' {Translator.Get("NelCestino")}";

            var result = MessageBox.Show(msg, "DriveVision", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (definitivo) { if (nodo.IsDirectory) Directory.Delete(nodo.PercorsoCompleto, true); else File.Delete(nodo.PercorsoCompleto); }
                    else ShellHelper.SpostaNelCestino(nodo.PercorsoCompleto);

                    _nodoCorrente.Figli.Remove(nodo);
                    if (_nodoCorrente.PercorsoCompleto.Length >= 3) ImpostaStatisticheDisco(_nodoCorrente.PercorsoCompleto.Substring(0, 3));
                    CaricaNodo(_nodoCorrente);
                }
                catch { MessageBox.Show(Translator.Get("ErroreEli"), "Errore", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void CanvasTreemap_SizeChanged(object sender, SizeChangedEventArgs e) { DisegnaTreemap(); }

        private void DisegnaTreemap()
        {
            if (CanvasTreemap.ActualWidth == 0 || CanvasTreemap.ActualHeight == 0) return;
            CanvasTreemap.Children.Clear();
            var figliValidi = _nodoCorrente.Figli.Where(f => f.DimensioneByte > 0).ToList();
            if (figliValidi.Count == 0) return;
            long spazioTotale = figliValidi.Sum(f => f.DimensioneByte);
            CalcolaRettangoli(0, 0, CanvasTreemap.ActualWidth, CanvasTreemap.ActualHeight, spazioTotale, figliValidi, 0);
        }

        private void CalcolaRettangoli(double x, double y, double width, double height, long totalBytes, List<FileNode> nodi, int depth)
        {
            if (nodi == null || nodi.Count == 0 || depth > 3 || width < 2 || height < 2) return;
            nodi = nodi.OrderByDescending(n => n.DimensioneByte).ToList();
            bool splitOrizzontale = width > height;
            double currentX = x; double currentY = y;

            foreach (var nodo in nodi)
            {
                double ratio = (double)nodo.DimensioneByte / totalBytes;
                double rectWidth = splitOrizzontale ? width * ratio : width;
                double rectHeight = splitOrizzontale ? height : height * ratio;

                if (rectWidth > 1 && rectHeight > 1)
                {
                    DisegnaRettangolo(nodo, currentX, currentY, rectWidth, rectHeight, depth);
                    if (nodo.IsDirectory && nodo.Figli.Count > 0)
                    {
                        double padding = 2;
                        CalcolaRettangoli(currentX + padding, currentY + padding, rectWidth - (padding * 2), rectHeight - (padding * 2), nodo.DimensioneByte, nodo.Figli, depth + 1);
                    }
                }
                if (splitOrizzontale) currentX += rectWidth; else currentY += rectHeight;
            }
        }

        private void DisegnaRettangolo(FileNode nodo, double x, double y, double width, double height, int depth)
        {
            Brush pennelloSfondo;
            if (nodo.IsDirectory)
            {
                byte grigio = (byte)(20 + depth * 10);
                pennelloSfondo = new SolidColorBrush(Color.FromArgb(255, grigio, grigio, (byte)(grigio + 5)));
            }
            else
            {
                string estensione = System.IO.Path.GetExtension(nodo.Nome).ToLower();
                Color coloreBase = OttieniColoreEstensione(estensione);
                pennelloSfondo = CreaEffettoCushion(coloreBase);
            }

            Rectangle rect = new Rectangle { Width = width, Height = height, Fill = pennelloSfondo, Stroke = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)), StrokeThickness = 1, ToolTip = $"{nodo.Nome}\n{nodo.Tipo}\n{nodo.DimensioneFormattata}" };
            Canvas.SetLeft(rect, x); Canvas.SetTop(rect, y); CanvasTreemap.Children.Add(rect);
        }

        private Brush CreaEffettoCushion(Color coloreBase)
        {
            var brush = new RadialGradientBrush { GradientOrigin = new Point(0.3, 0.3), Center = new Point(0.5, 0.5), RadiusX = 0.8, RadiusY = 0.8 };
            brush.GradientStops.Add(new GradientStop(ModificaLuminosita(coloreBase, 1.6), 0.0));
            brush.GradientStops.Add(new GradientStop(coloreBase, 0.4));
            brush.GradientStops.Add(new GradientStop(ModificaLuminosita(coloreBase, 0.4), 1.0));
            return brush;
        }

        private Color ModificaLuminosita(Color colore, double fattore)
        {
            byte r = (byte)Math.Min(255, Math.Max(0, colore.R * fattore));
            byte g = (byte)Math.Min(255, Math.Max(0, colore.G * fattore));
            byte b = (byte)Math.Min(255, Math.Max(0, colore.B * fattore));
            return Color.FromArgb(colore.A, r, g, b);
        }
    }
}