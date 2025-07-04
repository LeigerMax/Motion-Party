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
        [SerializeField] private GameObject handVisual;
        [SerializeField] private Renderer handRenderer;
        [SerializeField] private float sphereSize = 0.3f;
        [SerializeField] private Color openHandColor = Color.blue;
        [SerializeField] private Color closedHandColor = Color.red;
        
        [Header("Paramètres")]
        [SerializeField] private bool enableSmoothing = true;
        [SerializeField] private float smoothingFactor = 0.8f;
        [SerializeField] private int closedHandThreshold = 2;
        
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
        public bool IsHandClosed => currentHandData.isDetected && currentHandData.openFingers <= closedHandThreshold;

        private int offset = 70; // Ajout de l'offset comme dans HandTracking.cs

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
            UpdateVisualDisplay(); // Ajouter cette ligne pour mettre à jour le handVisual aussi
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
                // Créer la sphère
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
                sphereRenderer = handSphere.GetComponent<Renderer>();
                if (sphereRenderer == null)
                    sphereRenderer = handSphere.GetComponentInChildren<Renderer>();
            }
        }

        private void CreateDefaultHandVisual()
        {
            if (handVisual == null)
            {
                // Créer un visual par défaut
                handVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                handVisual.name = "DefaultHandVisual";
                handVisual.transform.localScale = Vector3.one * sphereSize;
                
                // Configurer le renderer
                handRenderer = handVisual.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = openHandColor;
                handRenderer.material = mat;
                
                // Désactiver par défaut
                handVisual.SetActive(false);
                
                FireflyDanceLogger.Log("HandVisual par défaut créé automatiquement");
            }
            else
            {
                handRenderer = handVisual.GetComponent<Renderer>();
                if (handRenderer == null)
                    handRenderer = handVisual.GetComponentInChildren<Renderer>();
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

            // Conversion vers l'espace monde avec inversion horizontale pour corriger le miroir
            float worldX = avgX / offset - 7;  // Inversé : au lieu de "7 - avgX / offset"
            float worldY = avgY / offset;

            Vector2 worldPosition = new Vector2(worldX, worldY);
            
            // **NOUVEAU : Contraindre la position dans les limites de la zone de jeu**
            if (config != null)
            {
                Vector2 originalPosition = worldPosition;
                worldPosition = config.ClampToBounds(worldPosition);
                
                // Log seulement si la position a été modifiée (contrainte appliquée)
                if (Vector2.Distance(originalPosition, worldPosition) > 0.001f)
                {
                    FireflyDanceLogger.LogVerbose($"Position contrainte: {originalPosition} -> {worldPosition}");
                }
                else
                {
                    FireflyDanceLogger.LogVerbose($"Position calculée: pixels({avgX}, {avgY}) -> monde({worldX}, {worldY})");
                }
            }
            else
            {
                FireflyDanceLogger.LogVerbose($"Position calculée: pixels({avgX}, {avgY}) -> monde({worldX}, {worldY})");
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
                    FireflyDanceLogger.Log("Sphère activée");
                }
                
                Vector3 newPosition = new Vector3(
                    currentHandData.position.x,
                    currentHandData.position.y,
                    handSphere.transform.position.z
                );
                
                handSphere.transform.position = newPosition;
                
                // Debug pour voir si la position change
                FireflyDanceLogger.LogVerbose($"Sphère positionnée à {newPosition}, main à {currentHandData.position}");
                
                // Changer la couleur selon l'état
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
                    FireflyDanceLogger.Log("Sphère désactivée");
                }
            }
        }

        #endregion

        #region Visual Display

        /// <summary>
        /// Met à jour l'affichage visuel de la main
        /// </summary>
        private void UpdateVisualDisplay()
        {
            if (handVisual == null) 
            {
                FireflyDanceLogger.LogWarning("UpdateVisualDisplay - handVisual est null ! Tentative de création...");
                CreateDefaultHandVisual();
                if (handVisual == null) return;
            }

            if (currentHandData.isDetected)
            {
                // Afficher et positionner le visuel
                if (!handVisual.activeSelf)
                {
                    handVisual.SetActive(true);
                    FireflyDanceLogger.LogVerbose("HandVisual activé");
                }
                
                // Positionner la sphère à la position calculée
                Vector3 targetPosition = new Vector3(
                    currentHandData.position.x,
                    currentHandData.position.y,
                    handVisual.transform.position.z
                );
                handVisual.transform.position = targetPosition;
                
                FireflyDanceLogger.LogVerbose($"Sphère de main positionnée à {targetPosition}");

                // Changer la couleur selon l'état
                UpdateHandColor();
            }
            else
            {
                // Cacher le visuel quand pas de main détectée
                if (handVisual.activeSelf)
                {
                    handVisual.SetActive(false);
                    FireflyDanceLogger.LogVerbose("HandVisual désactivé (pas de main détectée)");
                }
            }
        }

        /// <summary>
        /// Met à jour la couleur de la main selon son état
        /// </summary>
        private void UpdateHandColor()
        {
            if (handRenderer == null) return;

            Color targetColor = IsHandClosed ? closedHandColor : openHandColor;
            handRenderer.material.color = targetColor;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Vérifie si la main est dans une zone donnée
        /// </summary>
        public bool IsHandInArea(Vector2 center, float radius)
        {
            if (!currentHandData.isDetected) return false;
            
            float distance = Vector2.Distance(currentHandData.position, center);
            return distance <= radius;
        }

        /// <summary>
        /// Force la mise à jour des couleurs
        /// </summary>
        public void RefreshHandColor()
        {
            UpdateHandColor();
        }

        /// <summary>
        /// Vérifie si la main est dans les limites de la zone de jeu
        /// </summary>
        public bool IsHandInGameBounds()
        {
            if (!currentHandData.isDetected || config == null) return false;
            return config.IsValidPosition(currentHandData.position);
        }

        /// <summary>
        /// Obtient la distance de la main par rapport au centre de la zone de jeu
        /// </summary>
        public float GetDistanceFromCenter()
        {
            if (!currentHandData.isDetected || config == null) return float.MaxValue;
            return Vector2.Distance(currentHandData.position, config.Center);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Valide les composants requis
        /// </summary>
        private bool ValidateComponents()
        {
            // Assurer qu'on a une configuration
            FireflyDanceConfigHelper.EnsureConfig(ref config, "HandTracker");

            if (udpReceiver == null)
            {
                udpReceiver = FindFirstObjectByType<UDPReceive>();
                if (udpReceiver == null)
                {
                    FireflyDanceLogger.LogError("HandTracker - UDPReceive introuvable");
                    return false;
                }
                else
                {
                    FireflyDanceLogger.Log("UDPReceive trouvé automatiquement");
                }
            }

            return true;
        }

        /// <summary>
        /// Validation des paramètres dans l'éditeur
        /// </summary>
        private void OnValidate()
        {
            handLandmarkIndices = new int[] {0, 5, 9, 13, 17}; // Réinitialiser aux valeurs par défaut
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

        #region Debug Methods
        
        /// <summary>
        /// Test le déplacement de la sphère à une position spécifique
        /// </summary>
        [ContextMenu("Test Move Sphere")]
        public void TestMoveSphere()
        {
            if (handSphere == null)
            {
                CreateHandSphere();
            }
            
            if (handSphere != null)
            {
                // Activer la sphère
                handSphere.SetActive(true);
                
                // Positionner à une position visible
                Vector3 testPos = new Vector3(0, 0, 0);
                handSphere.transform.position = testPos;
                
                FireflyDanceLogger.Log($"Sphère forcée à la position {testPos}");
                
                // Test des couleurs
                if (sphereRenderer != null)
                {
                    sphereRenderer.material.color = Color.yellow;
                    FireflyDanceLogger.Log("Couleur de la sphère changée en jaune");
                }
            }
            else
            {
                FireflyDanceLogger.LogError("Impossible de créer la sphère");
            }
        }

        /// <summary>
        /// Debug des positions calculées vs reçues
        /// </summary>
        [ContextMenu("Debug Hand Positions")]
        public void DebugHandPositions()
        {
            FireflyDanceLogger.Log($"=== DEBUG POSITIONS ===");
            FireflyDanceLogger.Log($"isInitialized: {isInitialized}");
            FireflyDanceLogger.Log($"currentHandData.isDetected: {currentHandData.isDetected}");
            FireflyDanceLogger.Log($"currentHandData.position: {currentHandData.position}");
            FireflyDanceLogger.Log($"smoothedPosition: {smoothedPosition}");
            
            if (handSphere != null)
            {
                FireflyDanceLogger.Log($"handSphere.position: {handSphere.transform.position}");
                FireflyDanceLogger.Log($"handSphere.active: {handSphere.activeSelf}");
            }
            else
            {
                FireflyDanceLogger.Log("handSphere est NULL");
            }
            
            if (config != null)
            {
                FireflyDanceLogger.Log($"config.Center: {config.Center}");
                FireflyDanceLogger.Log($"config.Width: {config.Width}, Height: {config.Height}");
                FireflyDanceLogger.Log($"Zone de jeu: TopLeft({config.TopLeft}) -> BottomRight({config.BottomRight})");
            }
            else
            {
                FireflyDanceLogger.Log("config est NULL");
            }
        }

        /// <summary>
        /// Debug des limites de la zone de jeu
        /// </summary>
        [ContextMenu("Debug Game Bounds")]
        public void DebugGameBounds()
        {
            if (config == null)
            {
                FireflyDanceLogger.LogError("Config manquante pour afficher les limites");
                return;
            }
            
            FireflyDanceLogger.Log($"=== LIMITES ZONE DE JEU ===");
            FireflyDanceLogger.Log($"TopLeft: {config.TopLeft}");
            FireflyDanceLogger.Log($"BottomRight: {config.BottomRight}");
            FireflyDanceLogger.Log($"Center: {config.Center}");
            FireflyDanceLogger.Log($"Width: {config.Width}");
            FireflyDanceLogger.Log($"Height: {config.Height}");
            
            // Test de quelques positions
            Vector2[] testPositions = {
                new Vector2(-5, 5),  // Hors limites
                new Vector2(0, 0),   // Centre
                config.TopLeft,      // Coin supérieur gauche
                config.BottomRight,  // Coin inférieur droit
                new Vector2(5, -5)   // Hors limites
            };
            
            foreach (var pos in testPositions)
            {
                bool isValid = config.IsValidPosition(pos);
                Vector2 clamped = config.ClampToBounds(pos);
                FireflyDanceLogger.Log($"Position {pos} -> Valide: {isValid}, Contrainte: {clamped}");
            }
        }

        #endregion

        #region UDP Data Structure

        // Supprimer l'ancienne structure UDPHandData car on utilise maintenant JObject

        #endregion
    }
}
