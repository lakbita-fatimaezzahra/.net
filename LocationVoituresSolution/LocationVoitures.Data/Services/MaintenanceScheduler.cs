using LocationVoitures.Data.Services;
using System;
using System.Threading;
using System.Windows.Threading;

namespace LocationVoitures.BackOffice.Services
{
    /// <summary>
    /// Planificateur pour les vérifications automatiques de maintenance
    /// </summary>
    public class MaintenanceScheduler
    {
        private static MaintenanceScheduler _instance;
        private readonly DispatcherTimer _timer;
        private readonly MaintenanceService _maintenanceService;
        private DateTime _lastCheck;

        private MaintenanceScheduler()
        {
            _maintenanceService = new MaintenanceService();
            _lastCheck = DateTime.Now;

            // Configurer le timer pour vérifier toutes les heures
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromHours(1) // Vérifier toutes les heures
            };
            _timer.Tick += Timer_Tick;
        }

        public static MaintenanceScheduler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MaintenanceScheduler();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Démarrer les vérifications automatiques
        /// </summary>
        public void Start()
        {
            System.Diagnostics.Debug.WriteLine("[MaintenanceScheduler] Démarrage du planificateur");

            // Exécuter une première vérification au démarrage
            ExecuterVerification();

            // Démarrer le timer
            _timer.Start();
        }

        /// <summary>
        /// Arrêter les vérifications automatiques
        /// </summary>
        public void Stop()
        {
            System.Diagnostics.Debug.WriteLine("[MaintenanceScheduler] Arrêt du planificateur");
            _timer.Stop();
        }

        /// <summary>
        /// Événement déclenché par le timer
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Vérifier si une journée s'est écoulée depuis la dernière vérification
            if ((DateTime.Now - _lastCheck).TotalHours >= 24)
            {
                ExecuterVerification();
            }

            // Vérifier aussi les maintenances planifiées toutes les heures
            VerifierMaintenancesPlanifiees();
        }

        /// <summary>
        /// Exécuter une vérification complète
        /// </summary>
        private void ExecuterVerification()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MaintenanceScheduler] Vérification automatique - {DateTime.Now:HH:mm:ss}");

                // Vérifier toutes les voitures
                _maintenanceService.VerifierToutesLesVoitures();

                _lastCheck = DateTime.Now;

                System.Diagnostics.Debug.WriteLine("[MaintenanceScheduler] Vérification terminée");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MaintenanceScheduler] Erreur : {ex.Message}");
            }
        }

        /// <summary>
        /// Vérifier uniquement les maintenances planifiées (plus léger)
        /// </summary>
        private void VerifierMaintenancesPlanifiees()
        {
            try
            {
                _maintenanceService.VerifierMaintenancesPlanifiees();
                System.Diagnostics.Debug.WriteLine($"[MaintenanceScheduler] Maintenances planifiées vérifiées - {DateTime.Now:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MaintenanceScheduler] Erreur vérification planifiées : {ex.Message}");
            }
        }

        /// <summary>
        /// Forcer une vérification immédiate
        /// </summary>
        public void ForcerVerification()
        {
            ExecuterVerification();
        }

        /// <summary>
        /// Obtenir l'heure de la dernière vérification
        /// </summary>
        public DateTime GetDerniereVerification()
        {
            return _lastCheck;
        }

        /// <summary>
        /// Configurer l'intervalle de vérification (en heures)
        /// </summary>
        public void SetIntervalle(int heures)
        {
            _timer.Stop();
            _timer.Interval = TimeSpan.FromHours(heures);
            _timer.Start();
            System.Diagnostics.Debug.WriteLine($"[MaintenanceScheduler] Intervalle modifié : {heures} heure(s)");
        }
    }
}

// ====================================================
// INTÉGRATION DANS AdminWindow.xaml.cs
// ====================================================

/*
 * Dans le constructeur de AdminWindow.xaml.cs, ajouter :
 * 
 * // Démarrer le planificateur de maintenance
 * MaintenanceScheduler.Instance.Start();
 * 
 * Dans la méthode OnClosed :
 * 
 * protected override void OnClosed(EventArgs e)
 * {
 *     MaintenanceScheduler.Instance.Stop();
 *     base.OnClosed(e);
 * }
 */