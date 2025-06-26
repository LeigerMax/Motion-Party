using UnityEngine;
using Core;

/// <summary>
/// Simulateur de données MediaPipe pour tester le système LogParade sans webcam
/// Simule les mouvements latéraux avec les touches du clavier
/// À utiliser uniquement pour les tests et le développement
/// </summary>
public class LogParadeInputSimulator : MonoBehaviour
{
    [Header("Simulation Settings")]
    public bool enableSimulation = true;
    public float simulationSpeed = 2f;
    public float simulationRange = 3f;
    
    [Header("Keyboard Controls")]
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;
    public KeyCode centerKey = KeyCode.S;
    public KeyCode autoMoveKey = KeyCode.Space;
    
    [Header("Target UDPReceive")]
    public UDPReceive targetUDPReceive;
    
    // Simulation state
    private float currentSimulatedX = 0f;
    private bool autoMoveEnabled = false;
    private float autoMoveTimer = 0f;
    
    void Start()
    {        if (targetUDPReceive == null)
        {
            targetUDPReceive = FindFirstObjectByType<UDPReceive>();
        }
        
        if (enableSimulation)
        {
            Debug.Log("LogParade Input Simulator activé. Utilisez A/D pour bouger, S pour centrer, Espace pour mouvement auto.");
        }
    }
    
    void Update()
    {
        if (!enableSimulation || targetUDPReceive == null) return;
        
        HandleKeyboardInput();
        HandleAutoMovement();
        SendSimulatedData();
    }
    
    /// <summary>
    /// Gère les entrées clavier pour contrôler la simulation
    /// </summary>
    private void HandleKeyboardInput()
    {
        float deltaTime = Time.deltaTime;
        float moveAmount = simulationSpeed * deltaTime;
        
        // Mouvement gauche/droite
        if (Input.GetKey(moveLeftKey))
        {
            currentSimulatedX -= moveAmount;
            autoMoveEnabled = false;
        }
        
        if (Input.GetKey(moveRightKey))
        {
            currentSimulatedX += moveAmount;
            autoMoveEnabled = false;
        }
        
        // Retour au centre
        if (Input.GetKeyDown(centerKey))
        {
            currentSimulatedX = 0f;
            autoMoveEnabled = false;
        }
        
        // Toggle mouvement automatique
        if (Input.GetKeyDown(autoMoveKey))
        {
            autoMoveEnabled = !autoMoveEnabled;
            autoMoveTimer = 0f;
            Debug.Log($"Mouvement automatique : {(autoMoveEnabled ? "ACTIVÉ" : "DÉSACTIVÉ")}");
        }
        
        // Limiter la plage de mouvement
        currentSimulatedX = Mathf.Clamp(currentSimulatedX, -simulationRange, simulationRange);
    }
    
    /// <summary>
    /// Gère le mouvement automatique (oscillation)
    /// </summary>
    private void HandleAutoMovement()
    {
        if (!autoMoveEnabled) return;
        
        autoMoveTimer += Time.deltaTime;
        
        // Oscillation sinusoïdale entre -simulationRange et +simulationRange
        currentSimulatedX = Mathf.Sin(autoMoveTimer * 0.5f) * simulationRange;
    }
      /// <summary>
    /// Envoie les données simulées au UDPReceive
    /// Utilise le même format JSON que les vraies données MediaPipe
    /// Inclut maintenant les données de pose avec la tête (landmark 0)
    /// </summary>
    private void SendSimulatedData()
    {
        // Simuler les données de pose et de main
        float simulatedY = 0f;
        float simulatedZ = 0f;
        
        // Convertir en format MediaPipe (coordonnées écran simulées)
        int offset = 70; // Même offset que LogParadeLateralTracker
        float screenX = (7 - currentSimulatedX) * offset;
        float screenY = simulatedY * offset;
        float screenZ = simulatedZ * offset;
          // Créer le JSON dans le format attendu avec données de pose
        // Utiliser la culture invariante pour éviter les problèmes de localisation
        string jsonData = $@"{{
            ""pose_landmarks"": [
                [{screenX.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}, {screenY.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}, {screenZ.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}]
            ],
            ""hand_positions"": [
                [{screenX.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}, {screenY.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}, {screenZ.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}]
            ],
            ""gesture"": ""hand_open"",
            ""open_fingers"": 5,
            ""simulated"": true,
            ""simulator_x"": {currentSimulatedX.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}        }}";
        
        // Debug du JSON généré (temporaire)
        if (enableSimulation && Time.frameCount % 60 == 0) // Log toutes les secondes environ
        {
            Debug.Log($"JSON Simulator: {jsonData}");
        }
        
        // Injecter directement dans UDPReceive
        targetUDPReceive.data = jsonData;
    }
    
    /// <summary>
    /// Définit manuellement la position X simulée
    /// </summary>
    public void SetSimulatedPosition(float x)
    {
        currentSimulatedX = Mathf.Clamp(x, -simulationRange, simulationRange);
        autoMoveEnabled = false;
    }
    
    /// <summary>
    /// Active/désactive la simulation
    /// </summary>
    public void SetSimulationEnabled(bool enabled)
    {
        enableSimulation = enabled;
        
        if (!enabled)
        {
            // Nettoyer les données du UDPReceive
            if (targetUDPReceive != null)
            {                targetUDPReceive.data = "";
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!enableSimulation) return;
        
        // Visualiser la position simulée
        Vector3 simulatedPos = Vector3.right * currentSimulatedX;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(simulatedPos, 0.2f);
        
        // Dessiner la plage de simulation
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.right * -simulationRange, Vector3.right * simulationRange);
    }
}
