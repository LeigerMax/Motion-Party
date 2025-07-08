using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;
using Core;

namespace Gameplay.FireFlyDance.Hand
{
    /// <summary>
    /// Système de suivi de la main avec sphère visuelle
    /// Lit les données UDP et affiche une sphère entre les landmarks 0, 5, 9, 13, 17
    /// </summary>
    public class HandTracker : MonoBehaviour
    {
        #region Fields

        [Header("Configuration")]
        [SerializeField] private FireflyDanceConfig config;
        [SerializeField] private UDPReceive udpReceiver;
        
        [Header("Sphère visuelle")]
        [SerializeField] private GameObject handSphere;
        [SerializeField] private float sphereSize = 0.3f;
        [SerializeField] private Color openHandColor = Color.blue;
        [SerializeField] private Color closedHandColor = Color.red;
        
        [Header("Paramètres")]
        [SerializeField] private bool enableSmoothing = true;
        [SerializeField] private float smoothingFactor = 0.8f;
        [SerializeField] private int closedHandThreshold = 2;
        [SerializeField] private bool enableDebugMode = false;
        
        // Landmarks utilisés pour calculer la position centrale
        private int[] handLandmarkIndices = {0, 5, 9, 13, 17};
        
        // État interne
        private HandData currentHandData;
        private Vector2 smoothedPosition;
        private bool isInitialized = false;
        private bool wasHandDetected = false;
        private bool wasHandClosed = false;
        private Renderer sphereRenderer;

        // Propriétés publiques
        public HandData CurrentHandData => currentHandData;
        public Vector2 CurrentPosition => currentHandData.position;
        public bool IsHandDetected => currentHandData.isDetected;
        public bool IsHandClosed => currentHandData.IsHandClosed(closedHandThreshold) || currentHandData.gesture == "hand_close";

        // Constantes de conversion coordonnées MediaPipe vers Unity
        private const int PIXEL_TO_WORLD_OFFSET = 70; // Facteur de conversion pixels -> unités Unity
        private const float WORLD_X_CORRECTION = 7f; // Correction pour centrer l'axe X dans l'espace Unity

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (!isInitialized) return;
            
            UpdateHandFromUDP();
            UpdateSphereDisplay();
        }

        #endregion

        #region Initialization

        public void Initialize(FireflyDanceConfig providedConfig = null)
        {
            // Utiliser la config fournie ou auto-configuration
            if (providedConfig != null)
            {
                config = providedConfig;
            }
            else if (config == null)
            {
                config = FindFirstObjectByType<FireflyDanceConfig>();
            }
            
            if (udpReceiver == null)
                udpReceiver = FindFirstObjectByType<UDPReceive>();
            
            if (udpReceiver == null)
            {
                FireflyDanceLogger.LogError("HandTracker - UDPReceive introuvable");
                return;
            }
            
            // Créer la sphère
            CreateHandSphere();
            
            // Initialisation des données
            currentHandData = HandData.Empty;
            smoothedPosition = config != null ? config.Center : Vector2.zero;
            
            isInitialized = true;
            FireflyDanceLogger.Log("HandTracker initialisé avec sphère");
        }

        private void CreateHandSphere()
        {
            if (handSphere == null)
            {
                // Créer la sphère visuelle de la main
                handSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                handSphere.name = "HandSphere";
                handSphere.transform.localScale = Vector3.one * sphereSize;
                
                // Configurer le matériau
                sphereRenderer = handSphere.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = openHandColor;
                sphereRenderer.material = mat;
                
                // Désactiver par défaut
                handSphere.SetActive(false);
                
                FireflyDanceLogger.Log("Sphère de main créée automatiquement");
            }
            else
            {
                // Récupérer le renderer de la sphère existante
                sphereRenderer = handSphere.GetComponent<Renderer>();
                if (sphereRenderer == null)
                    sphereRenderer = handSphere.GetComponentInChildren<Renderer>();
            }
        }

        #endregion

        #region Hand Tracking

        private void UpdateHandFromUDP()
        {
            try
            {
                string jsonData = udpReceiver.data;
                if (string.IsNullOrEmpty(jsonData))
                {
                    HandleHandLost();
                    return;
                }

                // Utiliser JObject comme dans HandTracking.cs
                JObject parsedData = JObject.Parse(jsonData);
                JArray positions = (JArray)parsedData["hand_positions"];
                string gesture = parsedData["gesture"]?.ToString() ?? "unknown";
                int openFingers = (int)(parsedData["open_fingers"] ?? 0);

                if (positions == null || positions.Count == 0)
                {
                    HandleHandLost();
                    return;
                }

                // Calculer la position entre les landmarks spécifiés
                Vector2 handPosition = CalculateHandPosition(positions);
                
                // Application du lissage
                if (enableSmoothing && wasHandDetected)
                {
                    smoothedPosition = Vector2.Lerp(smoothedPosition, handPosition, 1f - smoothingFactor);
                }
                else
                {
                    smoothedPosition = handPosition;
                }

                // Mise à jour des données
                currentHandData = new HandData(
                    smoothedPosition,
                    gesture,
                    openFingers,
                    true
                );

                // Debug: Log des données de main pour diagnostiquer le problème de capture
                if (enableDebugMode)
                {
                    FireflyDanceLogger.LogVerbose($"Main: Pos={smoothedPosition}, Geste={gesture}, Doigts={openFingers}, Fermée={IsHandClosed}");
                }

                // Gestion des événements
                HandleHandStateChanges();
                
                wasHandDetected = true;
            }
            catch (Exception e)
            {
                FireflyDanceLogger.LogWarning($"Erreur UDP: {e.Message}");
                HandleHandLost();
            }
        }

        private Vector2 CalculateHandPosition(JArray handPositions)
        {
            if (handPositions.Count < 21) // Besoin de 21 landmarks
            {
                return Vector2.zero;
            }

            float sumX = 0f;
            float sumY = 0f;
            int validCount = 0;

            // Moyenne des landmarks spécifiés (comme dans HandTracking.cs)
            foreach (int landmarkIndex in handLandmarkIndices)
            {
                if (landmarkIndex < handPositions.Count)
                {
                    float x = (float)handPositions[landmarkIndex][0];
                    float y = (float)handPositions[landmarkIndex][1];
                    
                    sumX += x;
                    sumY += y;
                    validCount++;
                }
            }

            if (validCount == 0) return Vector2.zero;

            // Position moyenne en pixels
            float avgX = sumX / validCount;
            float avgY = sumY / validCount;

            // Conversion vers l'espace monde avec inversion horizontale pour corriger l'effet miroir
            float worldX = -(avgX / PIXEL_TO_WORLD_OFFSET - WORLD_X_CORRECTION);
            float worldY = avgY / PIXEL_TO_WORLD_OFFSET;

            Vector2 worldPosition = new Vector2(worldX, worldY);
            
            // Contraindre la position dans les limites de la zone de jeu
            if (config != null)
            {
                Vector2 originalPosition = worldPosition;
                worldPosition = config.ClampToBounds(worldPosition);
                
                // Log seulement si la position a été contrainte (hors limites)
                if (enableDebugMode && Vector2.Distance(originalPosition, worldPosition) > 0.001f)
                {
                    FireflyDanceLogger.LogVerbose($"Position contrainte: {originalPosition} -> {worldPosition}");
                }
            }
            
            return worldPosition;
        }

        private void HandleHandLost()
        {
            if (wasHandDetected)
            {
                currentHandData = HandData.Empty;
                wasHandDetected = false;
                FireflyDanceEvents.OnHandDetectionChanged?.Invoke(false);
            }
        }

        private void HandleHandStateChanges()
        {
            // Changement de détection
            if (!wasHandDetected)
            {
                FireflyDanceEvents.OnHandDetectionChanged?.Invoke(true);
            }

            // Changement de position
            FireflyDanceEvents.OnHandPositionChanged?.Invoke(currentHandData.position);

            // Changement d'état ouvert/fermé
            bool isCurrentlyClosed = IsHandClosed;
            if (isCurrentlyClosed != wasHandClosed)
            {
                wasHandClosed = isCurrentlyClosed;
                FireflyDanceEvents.OnHandStateChanged?.Invoke(isCurrentlyClosed);
            }
        }

        #endregion

        #region Sphere Display

        private void UpdateSphereDisplay()
        {
            if (handSphere == null) return;

            if (currentHandData.isDetected)
            {
                // Activer et positionner la sphère
                if (!handSphere.activeSelf)
                {
                    handSphere.SetActive(true);
                    if (enableDebugMode)
                        FireflyDanceLogger.Log("Sphère activée");
                }
                
                Vector3 newPosition = new Vector3(
                    currentHandData.position.x,
                    currentHandData.position.y,
                    handSphere.transform.position.z
                );
                
                handSphere.transform.position = newPosition;
                
                // Changer la couleur selon l'état de la main
                if (sphereRenderer != null)
                {
                    sphereRenderer.material.color = IsHandClosed ? closedHandColor : openHandColor;
                }
            }
            else
            {
                // Désactiver la sphère
                if (handSphere.activeSelf)
                {
                    handSphere.SetActive(false);
                    if (enableDebugMode)
                        FireflyDanceLogger.Log("Sphère désactivée");
                }
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validation des paramètres dans l'éditeur
        /// </summary>
        private void OnValidate()
        {
            // Garantir les valeurs par défaut des landmarks
            handLandmarkIndices = new int[] {0, 5, 9, 13, 17};
            // Contraindre les paramètres dans les plages valides
            smoothingFactor = Mathf.Clamp01(smoothingFactor);
            closedHandThreshold = Mathf.Clamp(closedHandThreshold, 0, 5);
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (config == null) return;
            
            // Zone de jeu - affichage exact des limites TopLeft/BottomRight
            Gizmos.color = Color.yellow;
            Vector3 topLeft3D = new Vector3(config.TopLeft.x, config.TopLeft.y, 0);
            Vector3 topRight3D = new Vector3(config.BottomRight.x, config.TopLeft.y, 0);
            Vector3 bottomLeft3D = new Vector3(config.TopLeft.x, config.BottomRight.y, 0);
            Vector3 bottomRight3D = new Vector3(config.BottomRight.x, config.BottomRight.y, 0);
            
            // Dessiner les 4 côtés de la zone de jeu
            Gizmos.DrawLine(topLeft3D, topRight3D);        // Haut
            Gizmos.DrawLine(topRight3D, bottomRight3D);    // Droite
            Gizmos.DrawLine(bottomRight3D, bottomLeft3D);  // Bas
            Gizmos.DrawLine(bottomLeft3D, topLeft3D);      // Gauche
            
            // Centre de la zone
            Gizmos.color = Color.green;
            Vector3 center = new Vector3(config.Center.x, config.Center.y, 0);
            Gizmos.DrawWireSphere(center, 0.1f);
            
            // Position de la main
            if (isInitialized && currentHandData.isDetected)
            {
                Gizmos.color = IsHandClosed ? Color.red : Color.blue;
                Vector3 handPos = new Vector3(currentHandData.position.x, currentHandData.position.y, 0);
                Gizmos.DrawSphere(handPos, 0.1f);
                
                // Rayon de capture
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(handPos, config.CaptureRadius);
                
                // Indicateur si la main est hors des limites (avant contrainte)
                if (!config.IsValidPosition(currentHandData.position))
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(handPos, 0.15f);
                }
            }
        }

        #endregion


        #region Tracking Control

        /// <summary>
        /// Démarre le suivi de la main
        /// </summary>
        public void StartTracking()
        {
            if (!isInitialized)
            {
                FireflyDanceLogger.LogWarning("HandTracker - Tentative de démarrage avant initialisation");
                return;
            }

            if (handSphere != null)
                handSphere.SetActive(true);

            enabled = true;
            FireflyDanceLogger.Log("HandTracker - Suivi démarré");
        }

        /// <summary>
        /// Arrête le suivi de la main
        /// </summary>
        public void StopTracking()
        {
            if (handSphere != null)
                handSphere.SetActive(false);

            enabled = false;
            FireflyDanceLogger.Log("HandTracker - Suivi arrêté");
        }

        #endregion
    }
}
