using UnityEngine;

/// <summary>
/// Panneau de debug d'exemple pour démontrer l'utilisation du système de debug UI
/// </summary>
public class DebugPanel_Example : BaseDebugPanel
{
    [Header("Configuration Exemple")]
    [SerializeField] private bool showAdvancedControls = true;
    [SerializeField] private string exampleMessage = "Ceci est un panneau d'exemple";
      // Variables de démonstration
    private int counterValue = 0;
    private float sliderValue = 0.5f;
    private bool toggleValue = false;
    
    protected override void Start()
    {
        base.Start();
          // Configuration spécifique à ce panneau
        panelTitle = "Exemple";
        width = 250;
        height = 300;
        
        // Position par défaut (utilise anchor de BaseDebugPanel)
        anchor = new Vector2(Screen.width - width - 20, 150);
        
        Debug.Log("[DebugPanel_Example] Panneau d'exemple initialisé");
    }
    
    protected override void DrawPanelContent()
    {
        try
        {
            // Style pour les labels importants
            var boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            var wrapStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            
            GUILayout.Label("🎮 PANNEAU D'EXEMPLE", boldStyle);
            GUILayout.Space(5);
            
            // Message d'exemple
            GUILayout.Label(exampleMessage, wrapStyle);
            GUILayout.Space(10);
            
            // Compteur
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Compteur: {counterValue}");
            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                counterValue--;
                Debug.Log($"[DebugPanel_Example] Compteur: {counterValue}");
            }
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                counterValue++;
                Debug.Log($"[DebugPanel_Example] Compteur: {counterValue}");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Slider
            GUILayout.Label($"Valeur: {sliderValue:F2}");
            sliderValue = GUILayout.HorizontalSlider(sliderValue, 0f, 1f);
            
            GUILayout.Space(5);
            
            // Toggle
            toggleValue = GUILayout.Toggle(toggleValue, "Option exemple");
            
            GUILayout.Space(10);
            
            // Boutons d'action
            if (GUILayout.Button("Action Exemple"))
            {
                Debug.Log($"[DebugPanel_Example] Action exécutée ! Compteur={counterValue}, Slider={sliderValue:F2}, Toggle={toggleValue}");
            }
            
            if (GUILayout.Button("Reset Valeurs"))
            {
                counterValue = 0;
                sliderValue = 0.5f;
                toggleValue = false;
                Debug.Log("[DebugPanel_Example] Valeurs réinitialisées");
            }
            
            // Contrôles avancés
            if (showAdvancedControls)
            {
                GUILayout.Space(10);
                GUILayout.Label("🔧 CONTRÔLES AVANCÉS:", boldStyle);
                
                if (GUILayout.Button("Test Log Info"))
                {
                    Debug.Log("[DebugPanel_Example] Ceci est un message d'information");
                }
                
                if (GUILayout.Button("Test Log Warning"))
                {
                    Debug.LogWarning("[DebugPanel_Example] Ceci est un avertissement");
                }
                
                if (GUILayout.Button("Test Log Error"))
                {
                    Debug.LogError("[DebugPanel_Example] Ceci est une erreur (test)");
                }
            }
        }
        catch (System.Exception ex)
        {
            GUILayout.Label($"❌ Erreur: {ex.Message}");
            Debug.LogError($"[DebugPanel_Example] Erreur DrawPanelContent: {ex}");
        }
    }
    
    protected override void DrawControlButtons()
    {
        base.DrawControlButtons();
        
        // Boutons supplémentaires spécifiques à ce panneau
        GUILayout.Space(5);
        
        if (GUILayout.Button("Contrôles Avancés"))
        {
            showAdvancedControls = !showAdvancedControls;
            Debug.Log($"[DebugPanel_Example] Contrôles avancés: {showAdvancedControls}");
        }
    }
    
    /// <summary>
    /// Méthode pour tester les fonctionnalités du panneau
    /// </summary>
    [ContextMenu("Test Panel Functions")]
    public void TestPanelFunctions()
    {
        Debug.Log($"[DebugPanel_Example] Test des fonctions du panneau:");
        Debug.Log($"  - Compteur: {counterValue}");
        Debug.Log($"  - Slider: {sliderValue:F2}");
        Debug.Log($"  - Toggle: {toggleValue}");
        Debug.Log($"  - Contrôles avancés: {showAdvancedControls}");
        Debug.Log($"  - Position: {panelRect.position}");
        Debug.Log($"  - Taille: {panelRect.size}");
    }
}
