using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DriveVision
{
    public partial class ScanWindow : Window
    {
        private readonly DriveCard _disco;
        private DispatcherTimer _timerPuntini;
        private int _contatorePuntini = 0;
        private string _testoBase = "";

        public ScanWindow(DriveCard disco)
        {
            InitializeComponent();
            _disco = disco;

            TxtIcona.Text = _disco.Icona;
            TxtTitolo.Text = Translator.Get("AnalisiDi") + _disco.Lettera;

            _testoBase = Translator.Get("AvvioMotore");
            TxtStato.Text = _testoBase;

            _timerPuntini = new DispatcherTimer();
            _timerPuntini.Interval = TimeSpan.FromMilliseconds(500);
            _timerPuntini.Tick += TimerPuntini_Tick;
            _timerPuntini.Start();

            this.Loaded += ScanWindow_Loaded;
        }

        private void TimerPuntini_Tick(object sender, EventArgs e)
        {
            _contatorePuntini = (_contatorePuntini + 1) % 4;
            string puntini = new string('.', _contatorePuntini);
            TxtStato.Text = _testoBase + puntini;
        }

        private async void ScanWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_disco.FileSystem == "NTFS")
            {
                _testoBase = Translator.Get("EstrazioneMFT") + _disco.Lettera;

                FileNode rootDisco = await Task.Run(() =>
                {
                    var mftEngine = new MftEngine();
                    return mftEngine.AvviaScansione(_disco.Lettera);
                });

                _timerPuntini.Stop();

                if (rootDisco != null)
                {
                    var dashboard = new DashboardWindow(rootDisco);
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    TxtStato.Text = Translator.Get("ErroreAdmin");
                    TxtStato.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            else
            {
                _timerPuntini.Stop();
                TxtStato.Text = $"{_disco.FileSystem} {Translator.Get("NonSupportato")}";
                TxtStato.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
    }
}