using LocationVoitures.Data.Services;
using System.Windows;
using System.Windows.Controls;

namespace LocationVoitures.BackOffice.Controls
{
    public partial class DashboardAlertes : UserControl
    {
        private readonly AlerteService _alerteService;

        public DashboardAlertes()
        {
            InitializeComponent();
            _alerteService = new AlerteService();
            ChargerStatistiques();
        }

        private void ChargerStatistiques()
        {
            var stats = _alerteService.CompterAlertesParUrgence();
            txtCritique.Text = stats["Critique"].ToString();
            txtUrgent.Text = stats["Urgent"].ToString();
        }

        private void VoirAlertes_Click(object sender, RoutedEventArgs e)
        {
            // Navigation vers la page des alertes
            var window = Window.GetWindow(this) as AdminWindow;
            window?.MainFrame.Navigate(new Pages.AlertesPage());
        }
    }
}