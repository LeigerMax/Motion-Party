using UnityEngine;

namespace MotionParty.EncouragementSystem
{
    /// <summary>
    /// Utilitaire de test pour vérifier le bon fonctionnement du système d'encouragement
    /// </summary>
    public class EncouragementSystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestOnStart = false;
        [SerializeField] private float testDuration = 30f;
        
        [Header("References")]
        [SerializeField] private EncouragementManager encouragementManager;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                StartTest();
            }
        }
        
        public void StartTest()
        {
            if (encouragementManager == null)
            {
                encouragementManager = FindObjectOfType<EncouragementManager>();
            }
            
            if (encouragementManager == null)
            {
                Debug.LogError("❌ EncouragementSystemTester: Aucun EncouragementManager trouvé dans la scène!");
                return;
            }
            
            Debug.Log("🧪 EncouragementSystemTester: Démarrage du test...");
            encouragementManager.StartEncouragement();
            
            if (testDuration > 0)
            {
                Invoke(nameof(StopTest), testDuration);
            }
        }
        
        public void StopTest()
        {
            if (encouragementManager != null)
            {
                Debug.Log("🧪 EncouragementSystemTester: Arrêt du test");
                encouragementManager.StopEncouragement();
            }
        }
        
        public void ShowRandomMessage()
        {
            if (encouragementManager != null)
            {
                Debug.Log("🧪 EncouragementSystemTester: Affichage d'un message aléatoire");
                encouragementManager.ShowRandomMessage();
            }
        }
        
        [ContextMenu("Test Encouragement System")]
        public void TestFromContextMenu()
        {
            StartTest();
        }
    }
}
