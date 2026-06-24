using System.Windows;
using System.Diagnostics;

namespace DriveVision
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplicaBandieraSalvata();
            AggiornaTestiUI();
        }

        private void ApplicaBandieraSalvata()
        {
            switch (Translator.LinguaCorrente)
            {
                case "IT": MnuLinguaCorrente.Header = "🇮🇹"; break;
                case "EN": MnuLinguaCorrente.Header = "🇬🇧"; break;
                case "ES": MnuLinguaCorrente.Header = "🇪🇸"; break;
                case "FR": MnuLinguaCorrente.Header = "🇫🇷"; break;
                case "DE": MnuLinguaCorrente.Header = "🇩🇪"; break;
            }
        }

        private void CaricaDischi()
        {
            var scanner = new DriveScanner();
            ListaDischi.ItemsSource = scanner.OttieniDischiDisponibili();
        }

        private void BtnAggiorna_Click(object sender, RoutedEventArgs e)
        {
            ListaDischi.ItemsSource = null;
            CaricaDischi();
        }

        private void ListaDischi_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListaDischi.SelectedItem is DriveCard discoSelezionato)
            {
                ListaDischi.SelectedItem = null;
                var scanWindow = new ScanWindow(discoSelezionato);
                scanWindow.Show();
                this.Close();
            }
        }

        private void MnuLingua_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem item && item.Tag is string codiceLingua)
            {
                Translator.ImpostaLingua(codiceLingua);
                ApplicaBandieraSalvata();
                AggiornaTestiUI();
            }
        }

        private void AggiornaTestiUI()
        {
            MnuFile.Header = Translator.Get("MenuFile");
            MnuEsci.Header = Translator.Get("MenuEsci");
            MnuRisoluzione.Header = Translator.Get("MenuRisoluzione");
            MnuRis1920.Header = Translator.Get("RisDef");
            MnuInfo.Header = Translator.Get("MenuInfo");
            MnuSupportami.Header = Translator.Get("MenuSupportami");
            TxtSottotitolo.Text = Translator.Get("SelezionaUnita");
            BtnAggiorna.ToolTip = Translator.Get("AggiornaLista");
            MnuLinguaCorrente.ToolTip = Translator.Get("CambiaLingua");

            CaricaDischi();
        }

        private void MnuRis_1920_Click(object sender, RoutedEventArgs e) { ImpostaRisoluzione(1920, 1080); }
        private void MnuRis_1600_Click(object sender, RoutedEventArgs e) { ImpostaRisoluzione(1600, 900); }
        private void MnuRis_1366_Click(object sender, RoutedEventArgs e) { ImpostaRisoluzione(1366, 768); }
        private void MnuRis_1280_Click(object sender, RoutedEventArgs e) { ImpostaRisoluzione(1280, 720); }

        private void ImpostaRisoluzione(double larghezza, double altezza)
        {
            this.WindowState = WindowState.Normal;
            this.Width = larghezza;
            this.Height = altezza;
            this.Left = (SystemParameters.PrimaryScreenWidth - larghezza) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - altezza) / 2;
        }

        private void MnuEsci_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }

        // ECCO LA MODIFICA: Ora apre la tua finestra personalizzata!
        private void MnuInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWin = new InfoWindow();
            infoWin.Owner = this; // Imposta questa finestra come genitore per centrarla
            infoWin.ShowDialog(); // ShowDialog impedisce di cliccare fuori finché non si chiude
        }

        private void MnuSupportami_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo { FileName = "https://ko-fi.com/federicosalis", UseShellExecute = true };
                Process.Start(psi);
            }
            catch
            {
                MessageBox.Show(Translator.Get("ErroreBrowser") + "https://ko-fi.com/federicosalis", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}