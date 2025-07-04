using System;
using UnityEngine;
using Newtonsoft.Json;
using Core;

namespace Gameplay.FireFlyDance.Player.HandTracker
{
    /// <summary>
    /// Composant principal pour le suivi de la main du joueur
    /// Lit les données UDP, convertit les coordonnées et affiche visuellement la position de la main
    /// </summary>
    public class HandTracker : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private FireflyDanceConfig config;
        [SerializeField] private UDPReceive udpReceiver;
        
        [Header("Affichage visuel")]
        [SerializeField] private GameObject handVisual;
        [SerializeField] private Transform handVisualParent;
        
        [Header("Paramètres de suivi")]
        [SerializeField] private int handLandmarkIndex = 8; // Index tip par défaut (point 8 de MediaPipe)
        [SerializeField] private bool enableSmoothing = true;
        [SerializeField] private float smoothingFactor = 0.8f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebug = false;
        [SerializeField] private bool enableGizmos = false;
        
        // État interne
        private HandData currentHandData;
        private Vector2 smoothedPosition;
        private bool isInitialized = false;
        private bool wasHandDetected = false;
        private string lastGesture = "";
        
        // Événements publics pour d'autres systèmes
        public event Action<HandData> OnHandDataUpdated;
        public event Action<Vector2> OnHandPositionChanged;
        
        // Propriétés publiques en lecture seule
        public HandData CurrentHandData => currentHandData;
        public Vector2 CurrentPosition => currentHandData.position;
        public bool IsHandDetected => currentHandData.isDetected;
        
        private void Start()
        {
            InitializeHandTracker();
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            UpdateHandPosition();
            UpdateVisualPosition();
        }
        
        /// <summary>
        /// Initialise le tracker de main
        /// </summary>
        private void InitializeHandTracker()
        {
            // Validation des références
            if (config == null)
            {
                Debug.LogError("[HandTracker] FireflyDanceConfig manquant !");
                return;
            }
            
            if (udpReceiver == null)
            {
                udpReceiver = FindObjectOfType<UDPReceive>();
                if (udpReceiver == null)
                {
                    Debug.LogError("[HandTracker] UDPReceive introuvable !");
                    return;
                }
            }
            
            if (handVisual == null)
            {
                Debug.LogWarning("[HandTracker] Visuel de la main manquant !");
            }
            
            // Initialisation des données
            currentHandData = HandData.Empty;
            smoothedPosition = config.Center;
            
            isInitialized = true;
            
            if (enableDebug)
            {
                Debug.Log("[HandTracker] Initialisé avec succès");
            }
        }
        
        /// <summary>
        /// Met à jour la position de la main à partir des données UDP
        /// </summary>
        private void UpdateHandPosition()
        {
            if (string.IsNullOrEmpty(udpReceiver.data)) return;
            
            try
            {
                // Parse des données JSON UDP
                var udpData = JsonConvert.DeserializeObject<UDPHandData>(udpReceiver.data);
                
                if (udpData.hand_positions != null && udpData.hand_positions.Count > handLandmarkIndex)
                {
                    // Récupération de la position du point souhaité (index par défaut)
                    var handPoint = udpData.hand_positions[handLandmarkIndex];
                    
                    // Conversion des coordonnées caméra vers monde Unity
                    Vector2 worldPosition = HandPositionConverter.CameraToWorld(
                        handPoint[0], handPoint[1], config);
                    
                    // Contrainte dans les limites de la zone de jeu
                    worldPosition = config.ClampToBounds(worldPosition);
                    
                    // Lissage optionnel
                    if (enableSmoothing)
                    {
                        worldPosition = Vector2.Lerp(smoothedPosition, worldPosition, 1f - smoothingFactor);
                        smoothedPosition = worldPosition;
                    }
                    
                    // Mise à jour des données
                    currentHandData = new HandData(
                        worldPosition,
                        udpData.gesture ?? "hand_open",
                        udpData.open_fingers,
                        true
                    );
                    
                    // Déclenchement des événements locaux
                    OnHandDataUpdated?.Invoke(currentHandData);
                    OnHandPositionChanged?.Invoke(worldPosition);
                    
                    // Déclenchement des événements globaux
                    HandEvents.BroadcastHandDataUpdate(currentHandData);
                    HandEvents.BroadcastHandPositionChange(worldPosition);
                    
                    // Détection de nouvelle main
                    if (!wasHandDetected)
                    {
                        HandEvents.BroadcastHandDetected(worldPosition);
                        wasHandDetected = true;
                    }
                    
                    // Changement de geste
                    if (lastGesture != currentHandData.gesture)
                    {
                        HandEvents.BroadcastGestureChange(currentHandData.gesture);
                        lastGesture = currentHandData.gesture;
                    }
                }
                else
                {
                    // Aucune main détectée
                    currentHandData = HandData.Empty;
                    OnHandDataUpdated?.Invoke(currentHandData);
                    
                    // Perte de la main
                    if (wasHandDetected)
                    {
                        HandEvents.BroadcastHandLost();
                        wasHandDetected = false;
                    }
                }
            }
            catch (Exception e)
            {
                if (enableDebug)
                {
                    Debug.LogWarning($"[HandTracker] Erreur parsing UDP: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Met à jour la position du visuel de la main
        /// </summary>
        private void UpdateVisualPosition()
        {
            if (handVisual == null) return;
            
            if (currentHandData.isDetected)
            {
                // Afficher et positionner le visuel
                handVisual.SetActive(true);
                
                Vector3 targetPosition = new Vector3(
                    currentHandData.position.x, 
                    currentHandData.position.y, 
                    handVisual.transform.position.z
                );
                
                handVisual.transform.position = targetPosition;
            }
            else
            {
                // Cacher le visuel si pas de main détectée
                handVisual.SetActive(false);
            }
        }
        
        /// <summary>
        /// Méthode publique pour obtenir la position actuelle de la main
        /// </summary>
        /// <returns>Position de la main dans le monde Unity</returns>
        public Vector2 GetCurrentPosition()
        {
            return currentHandData.position;
        }
        
        /// <summary>
        /// Méthode publique pour vérifier si la main est dans une zone donnée
        /// </summary>
        /// <param name="center">Centre de la zone</param>
        /// <param name="radius">Rayon de la zone</param>
        /// <returns>True si la main est dans la zone</returns>
        public bool IsHandInArea(Vector2 center, float radius)
        {
            if (!currentHandData.isDetected) return false;
            
            float distance = Vector2.Distance(currentHandData.position, center);
            return distance <= radius;
        }
        
        /// <summary>
        /// Validation des paramètres dans l'éditeur
        /// </summary>
        private void OnValidate()
        {
            // Validation de l'index du landmark
            handLandmarkIndex = Mathf.Clamp(handLandmarkIndex, 0, 20); // MediaPipe a 21 points (0-20)
            
            // Validation du facteur de lissage
            smoothingFactor = Mathf.Clamp01(smoothingFactor);
            
            // Validation des références
            if (config != null && handVisual != null)
            {
                // S'assurer que le visuel est dans les limites au démarrage
                if (Application.isPlaying && currentHandData.isDetected)
                {
                    Vector2 clampedPos = config.ClampToBounds(currentHandData.position);
                    if (clampedPos != currentHandData.position)
                    {
                        currentHandData = new HandData(
                            clampedPos, 
                            currentHandData.gesture, 
                            currentHandData.openFingers, 
                            currentHandData.isDetected
                        );
                    }
                }
            }
        }
        
        /// <summary>
        /// Affichage des Gizmos pour le debug
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!enableGizmos || config == null) return;
            
            // Zone de jeu
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(config.Center.x, config.Center.y, 0);
            Vector3 size = new Vector3(config.Width, config.Height, 0.1f);
            Gizmos.DrawWireCube(center, size);
            
            // Position actuelle de la main
            if (currentHandData.isDetected)
            {
                Gizmos.color = Color.green;
                Vector3 handPos = new Vector3(currentHandData.position.x, currentHandData.position.y, 0);
                Gizmos.DrawSphere(handPos, 0.1f);
                
                // Affichage du rayon de capture
                if (config != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(handPos, config.CaptureRadius);
                }
            }
        }
        
        /// <summary>
        /// Structure pour deserializer les données UDP
        /// </summary>
        [Serializable]
        private class UDPHandData
        {
            public System.Collections.Generic.List<float[]> hand_positions;
            public string gesture;
            public int open_fingers;
        }
    }
}
