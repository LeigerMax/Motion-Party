using UnityEngine;
using Gameplay.Common.Badges;
using Systems;

namespace UI.Team
{
    /// <summary>
    /// Utilitaire pour tester et déboguer le profil joueur
    /// Peut être attaché temporairement pour validation
    /// </summary>
    public class PlayerProfileTestHelper : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private string testPlayerName = "TestPlayer";
        [SerializeField] private PlayerDetailsPopup playerDetailsPopup;

        [Header("Test Actions")]
        [SerializeField] private bool runTestsOnStart = false;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunDelayedTests());
            }
        }

        private System.Collections.IEnumerator RunDelayedTests()
        {
            yield return new WaitForSeconds(1f);
            TestPlayerProfile();
        }

        /// <summary>
        /// Test complet du profil joueur
        /// </summary>
        [ContextMenu("Tester le Profil Joueur")]
        public void TestPlayerProfile()
        {
            Debug.Log("🧪 === TEST PROFIL JOUEUR ===");

            // Créer un joueur test
            var testPlayer = CreateTestPlayer();
            
            // Créer quelques badges test
            CreateTestBadges();

            // Tester l'affichage
            TestBadgeDisplay();
            TestStatsDisplay(testPlayer);

            Debug.Log("✅ Tests du profil joueur terminés");
        }

        /// <summary>
        /// Crée un joueur test avec des données réalistes
        /// </summary>
        private PlayerData CreateTestPlayer()
        {
            var player = new PlayerData(testPlayerName, "Résidence Test");
            
            // Simuler quelques parties
            for (int i = 0; i < 5; i++)
            {
                player.IncrementGamesPlayed();
                player.AddScore(UnityEngine.Random.Range(50, 300));
            }

            Debug.Log($"👤 Joueur test créé: {player}");
            return player;
        }

        /// <summary>
        /// Crée quelques badges test pour validation
        /// </summary>
        private void CreateTestBadges()
        {
            var storage = GlobalPlayerBadgeStorage.Instance;
            if (storage == null)
            {
                Debug.LogWarning("⚠️ GlobalPlayerBadgeStorage non trouvé - impossible de créer des badges test");
                return;
            }

            Debug.Log("🏆 Création de badges test...");

            // Créer des badges test via le système existant
            // (ceci nécessiterait l'accès aux définitions de badges du projet)
        }

        /// <summary>
        /// Test l'affichage des badges
        /// </summary>
        private void TestBadgeDisplay()
        {
            Debug.Log("🎯 Test affichage badges...");
            
            var badges = BadgeUIHelper.GetPlayerBadgesForUI(testPlayerName);
            Debug.Log($"   Badges trouvés: {badges.Count}");

            var stats = BadgeUIHelper.GetPlayerBadgeStats(testPlayerName);
            Debug.Log($"   Stats badges: Total={stats.TotalBadges}, Firefly={stats.FireflyBadges}");

            // Test du helper debug
            BadgeUIHelper.DebugShowPlayerBadges(testPlayerName);
        }

        /// <summary>
        /// Test l'affichage des statistiques
        /// </summary>
        private void TestStatsDisplay(PlayerData player)
        {
            Debug.Log("📊 Test affichage stats...");
            Debug.Log($"   Score moyen: {player.GetAverageScore():F1}");
            Debug.Log($"   Parties jouées: {player.GamesPlayed}");
            Debug.Log($"   Score total: {player.TotalScore}");
        }

        /// <summary>
        /// Ouvre le popup pour test visuel
        /// </summary>
        [ContextMenu("Ouvrir Popup Test")]
        public void OpenTestPopup()
        {
            if (playerDetailsPopup == null)
            {
                Debug.LogWarning("PlayerDetailsPopup non assigné");
                return;
            }

            var testPlayer = CreateTestPlayer();
            playerDetailsPopup.ShowPlayerDetails(testPlayer, 
                (player) => Debug.Log($"✏️ Édition demandée: {player.Nickname}"),
                (player) => Debug.Log($"🗑️ Suppression demandée: {player.Nickname}"));
        }

        /// <summary>
        /// Nettoie les données test
        /// </summary>
        [ContextMenu("Nettoyer Données Test")]
        public void CleanTestData()
        {
            var storage = GlobalPlayerBadgeStorage.Instance;
            if (storage != null)
            {
                // Note: ajoutez ici une méthode pour nettoyer les données test spécifiquement
                Debug.Log("🧹 Nettoyage des données test (à implémenter)");
            }
        }

        /// <summary>
        /// Affiche l'état du système de badges
        /// </summary>
        [ContextMenu("État Système Badges")]
        public void ShowBadgeSystemStatus()
        {
            Debug.Log("🔍 === ÉTAT SYSTÈME BADGES ===");
            
            var storage = GlobalPlayerBadgeStorage.Instance;
            Debug.Log($"   GlobalPlayerBadgeStorage: {(storage != null ? "✅ Trouvé" : "❌ Non trouvé")}");

            // Vérifier les composants UI
            if (playerDetailsPopup != null)
            {
                Debug.Log("   PlayerDetailsPopup: ✅ Assigné");
                // Ici vous pourriez vérifier les références internes
            }
            else
            {
                Debug.Log("   PlayerDetailsPopup: ❌ Non assigné");
            }

            Debug.Log("✅ Vérification terminée");
        }

        /// <summary>
        /// Créer un joueur avec badges variés pour test UI
        /// </summary>
        [ContextMenu("Créer Joueur avec Badges Variés")]
        public void CreatePlayerWithVariedBadges()
        {
            Debug.Log("🏆 Création joueur avec badges variés...");
            
            // Cette méthode nécessiterait l'accès aux définitions de badges
            // pour créer des instances de badges de différentes raretés
            
            Debug.Log("💡 À implémenter avec les définitions de badges du projet");
        }
    }
}
