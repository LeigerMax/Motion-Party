using UnityEngine;
using UnityEngine.SceneManagement;
using Core;

namespace Core
{
    /// <summary>
    /// Gestionnaire pour les changements de scène avec UDPReceive
    /// Assure que chaque scène utilise son propre UDPReceive
    /// </summary>
    public class UDPSceneManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool isMainScene = false;
        [SerializeField] private bool isMiniGameScene = false;
        
        private void Start()
        {
            // Vérifier le type de scène
            string sceneName = SceneManager.GetActiveScene().name;
            
            if (isMainScene)
            {
                Debug.Log($"[UDPSceneManager] Scène principale détectée: {sceneName}");
            }
            else if (isMiniGameScene)
            {
                Debug.Log($"[UDPSceneManager] Scène de mini-jeu détectée: {sceneName}");
                
                // S'assurer que l'UDPReceive local est activé
                EnsureLocalUDPReceive();
            }
        }
        
        private void EnsureLocalUDPReceive()
        {
            // Chercher l'UDPReceive dans la scène actuelle
            UDPReceive localUDP = FindObjectOfType<UDPReceive>();
            
            if (localUDP == null)
            {
                Debug.LogWarning("[UDPSceneManager] Aucun UDPReceive trouvé dans cette scène de mini-jeu!");
                return;
            }
            
            // Vérifier si c'est bien l'instance active
            if (UDPReceive.Instance != localUDP)
            {
                Debug.Log("[UDPSceneManager] Activation de l'UDPReceive local pour le mini-jeu");
            }
        }
        
        /// <summary>
        /// Méthode utilitaire pour changer de scène avec gestion UDP
        /// </summary>
        public static void LoadMiniGameScene(string sceneName)
        {
            Debug.Log($"[UDPSceneManager] Chargement du mini-jeu: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        
        /// <summary>
        /// Retourner à la scène principale
        /// </summary>
        public static void LoadMainScene(string mainSceneName = "MainScene")
        {
            Debug.Log($"[UDPSceneManager] Retour à la scène principale: {mainSceneName}");
            SceneManager.LoadScene(mainSceneName);
        }
        
        /// <summary>
        /// Obtenir des informations sur l'état actuel d'UDP
        /// </summary>
        [ContextMenu("Debug UDP Status")]
        public void DebugUDPStatus()
        {
            Debug.Log("=== UDP STATUS DEBUG ===");
            Debug.Log($"Scène actuelle: {SceneManager.GetActiveScene().name}");
            Debug.Log($"UDPReceive.Instance existe: {UDPReceive.Instance != null}");
            
            if (UDPReceive.Instance != null)
            {
                Debug.Log($"UDPReceive actif sur: {UDPReceive.Instance.gameObject.scene.name}");
                Debug.Log($"UDP prêt: {UDPReceive.IsReady()}");
                Debug.Log($"Données disponibles: {UDPReceive.IsDataAvailable()}");
                Debug.Log($"Paquets reçus: {UDPReceive.GetPacketCount()}");
            }
            
            // Compter tous les UDPReceive dans la scène
            UDPReceive[] allUDP = FindObjectsOfType<UDPReceive>();
            Debug.Log($"Nombre d'UDPReceive dans la scène: {allUDP.Length}");
            
            for (int i = 0; i < allUDP.Length; i++)
            {
                Debug.Log($"  [{i}] {allUDP[i].gameObject.name} - Actif: {allUDP[i].gameObject.activeInHierarchy}");
            }
            Debug.Log("========================");
        }
    }
}
