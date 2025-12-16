using LiveCharts;
using LiveCharts.Wpf;
using LocationVoitures.BackOffice.Pages;
using LocationVoitures.Data.Models;
using LocationVoitures.Data.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LocationVoitures.BackOffice
{
    public partial class DashboardPage : Page, System.ComponentModel.INotifyPropertyChanged
    {
        private readonly DashboardService _dashboardService;
        private readonly MaintenanceService _maintenanceService;
        private readonly AlerteService _alerteService;
        private readonly VoitureService _voitureService;

        private DispatcherTimer _timer;
        private List<Voiture> _allVoitures;

        // Propriétés pour le binding
        private int _usersCount;
        private int _carsCount;
        private int _rentalsCount;
        private int _clientsCount;
        private SeriesCollection _monthlyRentalsSeries;
        private string[] _months;
        private SeriesCollection _vehicleTypeSeries;

        public int UsersCount 
        { 
            get => _usersCount; 
            set { _usersCount = value; OnPropertyChanged(nameof(UsersCount)); } 
        }
        
        public int CarsCount 
        { 
            get => _carsCount; 
            set { _carsCount = value; OnPropertyChanged(nameof(CarsCount)); } 
        }
        
        public int RentalsCount 
        { 
            get => _rentalsCount; 
            set { _rentalsCount = value; OnPropertyChanged(nameof(RentalsCount)); } 
        }
        
        public int ClientsCount 
        { 
            get => _clientsCount; 
            set { _clientsCount = value; OnPropertyChanged(nameof(ClientsCount)); } 
        }
        
        public SeriesCollection MonthlyRentalsSeries 
        { 
            get => _monthlyRentalsSeries; 
            set { _monthlyRentalsSeries = value; OnPropertyChanged(nameof(MonthlyRentalsSeries)); } 
        }
        
        public string[] Months 
        { 
            get => _months; 
            set { _months = value; OnPropertyChanged(nameof(Months)); } 
        }
        
        public SeriesCollection VehicleTypeSeries 
        { 
            get => _vehicleTypeSeries ?? new SeriesCollection(); 
            set 
            { 
                _vehicleTypeSeries = value; 
                OnPropertyChanged(nameof(VehicleTypeSeries));
                System.Diagnostics.Debug.WriteLine($"[VehicleTypeSeries] Set - Count: {value?.Count ?? 0}");
            } 
        }

        public DashboardPage()
        {
            InitializeComponent();

            _dashboardService = new DashboardService();
            _maintenanceService = new MaintenanceService();
            _alerteService = new AlerteService();
            _voitureService = new VoitureService();
            // ✅ Initialiser les collections vides pour éviter les erreurs de binding
            MonthlyRentalsSeries = new SeriesCollection();
            VehicleTypeSeries = new SeriesCollection();
            Months = new string[0];
            
            // ✅ Test initial pour vérifier que le graphique fonctionne
            // Ceci sera remplacé par les vraies données dans LoadCharts()
            VehicleTypeSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Test",
                    Values = new ChartValues<int> { 5 },
                    Fill = System.Windows.Media.Brushes.Blue
                }
            };

            DataContext = this;

            Loaded += DashboardPage_Loaded;
        }
        

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Vérifier les maintenances au chargement
            VerifierMaintenances();

            // Charger toutes les statistiques
            LoadStatistics();
            LoadCharts();
            LoadVoitures();
            LoadTypesChart();
            LoadAlertesMaintenance();
            UpdateDateTime();
            StartTimer();
        }
      
        // Vérifier les maintenances de toutes les voitures
        private void VerifierMaintenances()
        {
            try
            {
                // ✅ Vérifier toutes les voitures et créer des alertes
                _maintenanceService.VerifierToutesLesVoitures();
                // ✅ Cette méthode vérifie aussi les maintenances planifiées automatiquement
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur vérification maintenances : {ex.Message}");
            }
        }

        // Charger les statistiques principales
        private void LoadStatistics()
        {
            UsersCount = _dashboardService.CountUsers();
            CarsCount = _dashboardService.CountVoitures();
            RentalsCount = _dashboardService.CountLocations();
            ClientsCount = _dashboardService.CountClients();

            // Forcer la mise à jour de l'interface
            TxtUsersCount.Text = UsersCount.ToString();
            TxtCarsCount.Text = CarsCount.ToString();
            TxtRentalsCount.Text = RentalsCount.ToString();
            TxtClientsCount.Text = ClientsCount.ToString();
        }
        private void LoadVoitures()
        {
            // Exemple de récupération depuis la base
            _allVoitures = _voitureService.GetVoitureByType();
        }
        private void LoadTypesChart()
        {
            // Inclure le TypeVoiture pour accéder à son nom
            var typesCount = _allVoitures
                .GroupBy(v => v.TypeVoiture.Nom) // On groupe par nom du type
                .Select(g => new { TypeName = g.Key, Count = g.Count() })
                .ToList();

            SeriesCollection series = new SeriesCollection();

            foreach (var t in typesCount)
            {
                series.Add(new PieSeries
                {
                    Title = t.TypeName, // Affiche le nom du type
                    Values = new ChartValues<int> { t.Count },
                    DataLabels = true
                });
            }

            TypeVoituresChart.Series = series;
        }



        // Charger le graphique location
        private void LoadCharts()
        {
            try
            {
                // -------------------------
                // 1️⃣ Locations par mois
                // -------------------------
                var locationsData = _dashboardService.GetLocationsParMois();

                if (locationsData != null && locationsData.Any())
                {
                    Months = locationsData.Select(x => x.Key).ToArray();
                    MonthlyRentalsSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Locations",
                    Values = new ChartValues<int>(locationsData.Select(x => x.Value)),
                    Fill = Brushes.CornflowerBlue,
                    Stroke = Brushes.DarkBlue,
                    StrokeThickness = 2
                }
            };
                }
                else
                {
                    // Aucun donnée → graphique vide
                    Months = new[] { "Aucune donnée" };
                    MonthlyRentalsSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Locations",
                    Values = new ChartValues<int> { 0 },
                    Fill = Brushes.LightGray
                }
            };
                }

              

                // -------------------------
                // 3️⃣ Forcer le Binding
                // -------------------------
                OnPropertyChanged(nameof(MonthlyRentalsSeries));
                OnPropertyChanged(nameof(Months));
                OnPropertyChanged(nameof(VehicleTypeSeries));

                // Optionnel : rafraîchir le DataContext pour s'assurer que LiveCharts voit les changements
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var currentContext = DataContext;
                    DataContext = null;
                    DataContext = currentContext ?? this;
                }), DispatcherPriority.Loaded);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement graphiques : {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des graphiques : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // ✅ Méthode pour notifier les changements de propriétés (pour le binding)
        private void OnPropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        // ✅ Événement pour le binding
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        // NOUVEAU : Charger les alertes de maintenance
        private void LoadAlertesMaintenance()
        {
            try
            {
                var stats = _alerteService.CompterAlertesParUrgence();

                // ✅ Compter toutes les alertes (maintenances + maintenances planifiées)
                int alertesCritiques = stats["Critique"];
                int alertesUrgentes = stats["Urgent"];
                int alertesPrioritaires = alertesCritiques + alertesUrgentes;

                // Afficher la bannière si alertes critiques/urgentes
                if (alertesPrioritaires > 0)
                {
                    AlertesBanner.Visibility = Visibility.Visible;

                    string message = alertesPrioritaires == 1
                        ? "1 alerte prioritaire de maintenance détectée"
                        : $"{alertesPrioritaires} alertes prioritaires de maintenance détectées";

                    // ✅ Ajouter des détails si nécessaire
                    if (alertesCritiques > 0)
                    {
                        message += $" ({alertesCritiques} critique(s))";
                    }

                    txtAlerteBanner.Text = $"⚠️ {message}";
                }
                else
                {
                    AlertesBanner.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur chargement alertes : {ex.Message}");
            }
        }

        // Mettre à jour la date/heure
        private void UpdateDateTime()
        {
            TxtDateHeure.Text = $"Dernière mise à jour : {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        // Démarrer le timer de rafraîchissement automatique
        private void StartTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5) // Actualisation toutes les 5 minutes
            };

            _timer.Tick += (s, e) =>
            {
                UpdateDateTime();
                VerifierMaintenances();
                LoadStatistics();
                LoadCharts();
                LoadAlertesMaintenance();
            };

            _timer.Start();
        }

        // Navigation vers la page des alertes
        private void VoirToutesAlertes_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AlertesPage());
        }

        private void VoirAlertes_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new AlertesPage());
        }

        private void AlertesBanner_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new AlertesPage());
        }

        // Nettoyer le timer lors de la fermeture
        ~DashboardPage()
        {
            _timer?.Stop();
        }
    }
}