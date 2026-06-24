using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace DriveVision
{
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
            ImpostaTestiLingua();
        }

        private void ImpostaTestiLingua()
        {
            this.Title = Translator.Get("InfoTitolo");
            TxtDescrizione.Text = Translator.Get("InfoDesc");
            TxtSviluppatore.Text = Translator.Get("InfoSviluppatore");
            RunSito.Text = Translator.Get("InfoSito") + " : ";
            RunBlog.Text = Translator.Get("InfoBlog") + " : ";
            RunAltro.Text = Translator.Get("InfoAltro") + " : ";
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true; // Segnala a WPF che abbiamo gestito noi il click
            }
            catch { }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}