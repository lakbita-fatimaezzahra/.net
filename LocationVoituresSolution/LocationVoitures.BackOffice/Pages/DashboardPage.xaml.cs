using LiveCharts;
using LiveCharts.Wpf;
using LocationVoitures.BackOffice.Pages;
using LocationVoitures.Data.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LocationVoitures.BackOffice
{
    public partial class DashboardPage : Page, System.ComponentModel.INotifyPropertyChanged
    {
        private readonly DashboardService _dashboardService;
        private readonly MaintenanceService _maintenanceService;
        private readonly AlerteService _alerteService;
        private DispatcherTimer _timer;

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

        // Charger les graphiques
        private void LoadCharts()
        {
            try
            {
                // ✅ Locations par mois
                var data = _dashboardService.GetLocationsParMois();
                
                if (data != null && data.Any())
                {
                    Months = data.Select(x => x.Key).ToArray();
                    MonthlyRentalsSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Locations",
                            Values = new ChartValues<int>(data.Select(x => x.Value)),
                            Fill = System.Windows.Media.Brushes.CornflowerBlue,
                            Stroke = System.Windows.Media.Brushes.DarkBlue,
                            StrokeThickness = 2
                        }
                    };
                }
                else
                {
                    // Si pas de données, créer un graphique vide
                    Months = new[] { "Aucune donnée" };
                    MonthlyRentalsSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Locations",
                            Values = new ChartValues<int> { 0 },
                            Fill = System.Windows.Media.Brushes.LightGray
                        }
                    };
                }

                // ✅ Répartition par type de voiture
                var types = _dashboardService.GetVoituresParType();
                
                System.Diagnostics.Debug.WriteLine($"[LoadCharts] Types de véhicules récupérés: {types?.Count ?? 0}");
                
                if (types != null && types.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[LoadCharts] Création du graphique avec {types.Count} types");
                    
                    // Couleurs pour le graphique en camembert
                    var colors = new[]
                    {
                        System.Windows.Media.Brushes.CornflowerBlue,
                        System.Windows.Media.Brushes.LightGreen,
                        System.Windows.Media.Brushes.Orange,
                        System.Windows.Media.Brushes.Pink,
                        System.Windows.Media.Brushes.Purple,
                        System.Windows.Media.Brushes.Teal,
                        System.Windows.Media.Brushes.Gold,
                        System.Windows.Media.Brushes.Crimson,
                        System.Windows.Media.Brushes.DarkCyan,
                        System.Windows.Media.Brushes.IndianRed
                    };

                    // Créer la collection de séries
                    var pieSeriesList = new List<PieSeries>();
                    
                    int index = 0;
                    foreach (var type in types)
                    {
                        var pieSeries = new PieSeries
                        {
                            Title = $"{type.Key}",
                            Values = new ChartValues<int> { type.Value },
                            DataLabels = true,
                            LabelPoint = point => $"{point.Y} ({point.Participation:P0})",
                            Fill = colors[index % colors.Length],
                            StrokeThickness = 2,
                            Stroke = System.Windows.Media.Brushes.White
                        };
                        
                        pieSeriesList.Add(pieSeries);
                        System.Diagnostics.Debug.WriteLine($"[LoadCharts] Ajouté: {type.Key} = {type.Value} véhicules");
                        index++;
                    }

                    // ✅ Créer la SeriesCollection et l'assigner
                    var newSeries = new SeriesCollection(pieSeriesList);
                    VehicleTypeSeries = newSeries;
                    
                    System.Diagnostics.Debug.WriteLine($"[LoadCharts] VehicleTypeSeries créé avec {VehicleTypeSeries.Count} séries");
                    
                    // ✅ Forcer la notification de changement
                    OnPropertyChanged(nameof(VehicleTypeSeries));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LoadCharts] Aucun type de véhicule trouvé, création d'un graphique vide");
                    
                    // Si pas de données, créer un graphique avec un message
                    VehicleTypeSeries = new SeriesCollection
                    {
                        new PieSeries
                        {
                            Title = "Aucune donnée",
                            Values = new ChartValues<int> { 1 },
                            Fill = System.Windows.Media.Brushes.LightGray,
                            DataLabels = false
                        }
                    };
                }

                // ✅ Forcer la mise à jour du binding immédiatement
                OnPropertyChanged(nameof(MonthlyRentalsSeries));
                OnPropertyChanged(nameof(Months));
                OnPropertyChanged(nameof(VehicleTypeSeries));
                
                System.Diagnostics.Debug.WriteLine($"[LoadCharts] Binding mis à jour - VehicleTypeSeries.Count = {VehicleTypeSeries?.Count ?? 0}");
                
                // ✅ Forcer le rafraîchissement de l'interface sur le thread UI
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Recharger le DataContext pour forcer la mise à jour des graphiques
                    var currentContext = DataContext;
                    DataContext = null;
                    DataContext = currentContext ?? this;
                }), System.Windows.Threading.DispatcherPriority.Loaded);
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