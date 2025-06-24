using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire principal pour l'affichage de debug UI personnalisable de LogParade.
/// Gère l'activation/désactivation globale avec F1 et centralise la liste des panneaux.
/// </summary>
public class LogParadeDebugManager : MonoBehaviour
{
    [Header("Global Settings")]
    [Tooltip("Active le système de debug dès le démarrage")]
    [SerializeField] private bool debugEnabledAtStart = false;
    
    [Tooltip("Touche pour activer/désactiver les panneaux de debug")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    
    [Header("Debug Panels")]
    [Tooltip("Liste des panneaux de debug à gérer")]
    [SerializeField] private List<BaseDebugPanel> debugPanels = new List<BaseDebugPanel>();
    
    [Header("Auto-Discovery")]
    [Tooltip("Recherche automatiquement les panneaux de debug dans les enfants")]
    [SerializeField] private bool autoFindPanels = true;
    
    [Tooltip("Recherche automatiquement dans toute la scène")]
    [SerializeField] private bool searchInEntireScene = false;
    
    // État du système
    private bool debugEnabled = false;
    
    /// <summary>
    /// État actuel du système de debug
    /// </summary>
    public bool IsDebugEnabled => debugEnabled;
    
    /// <summary>
    /// Événement déclenché quand l'état du debug change
    /// </summary>
    public System.Action<bool> OnDebugStateChanged;

    void Start()
    {
        // Auto-découverte des panneaux si activée
        if (autoFindPanels)
        {
            AutoDiscoverDebugPanels();
        }
        
        // Initialiser l'état
        debugEnabled = debugEnabledAtStart;
        UpdatePanelsVisibility();
          // Log de démarrage
        Debug.Log($"[LogParadeDebugManager] Système de debug initialisé. " +
                  $"Panneaux trouvés: {debugPanels.Count}. " +
                  $"Appuyez sur {toggleKey} pour activer/désactiver.");
        
        // Afficher l'aide au démarrage si configuré
        if (showHelpAtStart && debugEnabled)
        {
            showHelp = true;
            Debug.Log("[LogParadeDebugManager] Aide affichée au démarrage. Appuyez sur F4 pour la masquer.");
        }
    }

    void Update()
    {
        // Gérer la touche F1
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleDebug();
        }
        
        // Raccourcis clavier pour la gestion des panneaux (seulement si debug activé)
        if (debugEnabled)
        {
            // F2 = Reset toutes les positions
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ResetAllPanelPositions();
            }
            
            // F3 = Sauvegarder toutes les positions
            if (Input.GetKeyDown(KeyCode.F3))
            {
                SaveAllPanelPositions();
            }
            
            // F4 = Afficher aide
            if (Input.GetKeyDown(KeyCode.F4))
            {
                ToggleHelp();
            }
        }
    }

    /// <summary>
    /// Active ou désactive le système de debug
    /// </summary>
    public void ToggleDebug()
    {
        debugEnabled = !debugEnabled;
        UpdatePanelsVisibility();
        
        Debug.Log($"[LogParadeDebugManager] Debug {(debugEnabled ? "ACTIVÉ" : "DÉSACTIVÉ")}");
        
        // Déclencher l'événement
        OnDebugStateChanged?.Invoke(debugEnabled);
    }

    /// <summary>
    /// Force l'état du debug
    /// </summary>
    /// <param name="enabled">Nouvel état</param>
    public void SetDebugEnabled(bool enabled)
    {
        if (debugEnabled != enabled)
        {
            debugEnabled = enabled;
            UpdatePanelsVisibility();
            OnDebugStateChanged?.Invoke(debugEnabled);
        }
    }

    /// <summary>
    /// Ajoute un panneau de debug à la liste
    /// </summary>
    /// <param name="panel">Panneau à ajouter</param>
    public void AddDebugPanel(BaseDebugPanel panel)
    {
        if (panel != null && !debugPanels.Contains(panel))
        {
            debugPanels.Add(panel);
            panel.SetVisible(debugEnabled);
        }
    }

    /// <summary>
    /// Retire un panneau de debug de la liste
    /// </summary>
    /// <param name="panel">Panneau à retirer</param>
    public void RemoveDebugPanel(BaseDebugPanel panel)
    {
        if (panel != null && debugPanels.Contains(panel))
        {
            debugPanels.Remove(panel);
        }
    }

    /// <summary>
    /// Recherche automatiquement les panneaux de debug
    /// </summary>
    private void AutoDiscoverDebugPanels()
    {
        debugPanels.Clear();
        
        BaseDebugPanel[] panels;
        
        if (searchInEntireScene)
        {
            // Recherche dans toute la scène
            panels = FindObjectsOfType<BaseDebugPanel>();
        }
        else
        {
            // Recherche seulement dans les enfants
            panels = GetComponentsInChildren<BaseDebugPanel>(true);
        }
          foreach (var panel in panels)
        {
            if (panel != null)
            {
                debugPanels.Add(panel);
                Debug.Log($"[LogParadeDebugManager] Panneau trouvé: {panel.GetType().Name}");
            }
        }
        
        // Recherche de panneaux spécialisés supplémentaires
        var scorePanels = FindObjectsOfType<DebugPanel_Score>();
        var statusPanels = FindObjectsOfType<DebugPanel_Status>();
        var calibrationPanels = FindObjectsOfType<DebugPanel_Calibration>();
        var launcherPanels = FindObjectsOfType<DebugPanel_GameLauncher>();
        var examplePanels = FindObjectsOfType<DebugPanel_Example>();
        
        // Ajouter les panneaux spécialisés s'ils ne sont pas déjà dans la liste
        AddUniquePanel(scorePanels);
        AddUniquePanel(statusPanels);
        AddUniquePanel(calibrationPanels);
        AddUniquePanel(launcherPanels);
        AddUniquePanel(examplePanels);
    }

    /// <summary>
    /// Ajoute des panneaux uniques à la liste (évite les doublons)
    /// </summary>
    private void AddUniquePanel<T>(T[] panels) where T : BaseDebugPanel
    {
        foreach (var panel in panels)
        {
            if (panel != null && !debugPanels.Contains(panel))
            {
                debugPanels.Add(panel);
                Debug.Log($"[LogParadeDebugManager] Panneau spécialisé ajouté: {panel.GetType().Name}");
            }
        }
    }

    /// <summary>
    /// Met à jour la visibilité de tous les panneaux
    /// </summary>
    private void UpdatePanelsVisibility()
    {
        foreach (var panel in debugPanels)
        {
            if (panel != null)
            {
                panel.SetVisible(debugEnabled);
            }
        }
    }

    /// <summary>
    /// Valide les références dans l'inspecteur
    /// </summary>
    void OnValidate()
    {
        // Retire les références nulles
        debugPanels.RemoveAll(panel => panel == null);
    }    /// <summary>
    /// Affichage d'informations de debug dans la scène
    /// </summary>
    void OnGUI()
    {
        if (!debugEnabled) return;
        
        bool mainAreaStarted = false;
        
        try
        {
            // Affichage d'informations générales en haut à gauche
            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            mainAreaStarted = true;
            
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("LogParade Debug Manager");
            GUILayout.Label($"Panneaux actifs: {debugPanels.Count}");
            GUILayout.Label($"Touche: {toggleKey}");
            
            if (GUILayout.Button("Redécouvrir panneaux"))
            {
                AutoDiscoverDebugPanels();
            }
            
            GUILayout.EndVertical();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LogParadeDebugManager] Erreur OnGUI main: {ex}");
            
            if (mainAreaStarted)
            {
                try { GUILayout.EndArea(); mainAreaStarted = false; } catch { }
            }
            
            GUIUtility.ExitGUI();
        }
        finally
        {
            if (mainAreaStarted)
            {
                try
                {
                    GUILayout.EndArea();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[LogParadeDebugManager] Erreur EndArea main: {ex}");
                }
            }
        }
        
        // Afficher l'aide si activée
        if (showHelp)
        {
            DisplayHelpWindow();
        }
    }    /// <summary>
    /// Affiche la fenêtre d'aide
    /// </summary>
    private void DisplayHelpWindow()
    {
        bool helpAreaStarted = false;
        
        try
        {
            // Position de l'aide (centre de l'écran)
            float helpWidth = 400f;
            float helpHeight = 300f;
            Rect helpRect = new Rect(
                (Screen.width - helpWidth) / 2,
                (Screen.height - helpHeight) / 2,
                helpWidth,
                helpHeight);
            
            // Fond semi-transparent
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUI.Box(helpRect, "");
            GUI.backgroundColor = oldColor;
            
            // Contenu de l'aide
            GUILayout.BeginArea(helpRect);
            helpAreaStarted = true;
            
            GUILayout.BeginVertical();
            
            GUILayout.Space(10);
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 16;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            GUILayout.Label("🎮 AIDE DEBUG UI LOGPARADE", titleStyle);
            
            GUILayout.Space(10);
            GUIStyle helpStyle = new GUIStyle(GUI.skin.label);
            helpStyle.normal.textColor = Color.white;
            helpStyle.wordWrap = true;
            
            GUILayout.Label("📱 RACCOURCIS CLAVIER:", helpStyle);
            GUILayout.Label("• F1 = Ouvrir/Fermer debug UI", helpStyle);
            GUILayout.Label("• F2 = Reset positions des panneaux", helpStyle);
            GUILayout.Label("• F3 = Sauvegarder positions", helpStyle);
            GUILayout.Label("• F4 = Afficher/Masquer cette aide", helpStyle);
            
            GUILayout.Space(10);
            GUILayout.Label("🖱️ DÉPLACEMENT DES PANNEAUX:", helpStyle);
            GUILayout.Label("• Cliquez et glissez le titre du panneau", helpStyle);
            GUILayout.Label("• Le panneau devient jaune pendant le déplacement", helpStyle);
            GUILayout.Label("• Position sauvegardée automatiquement", helpStyle);
            
            GUILayout.Space(10);
            GUILayout.Label("🔧 BOUTONS DE CONTRÔLE:", helpStyle);
            GUILayout.Label("• 'Reset Pos' = Position par défaut", helpStyle);
            GUILayout.Label("• 'Save' = Sauvegarder position", helpStyle);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Fermer l'aide (F4)", GUILayout.Height(30)))
            {
                showHelp = false;
            }
            
            GUILayout.EndVertical();
        }
        catch (System.Exception ex)
        {
            try
            {
                if (helpAreaStarted)
                {
                    GUILayout.Label($"Erreur affichage aide: {ex.Message}");
                }
            }
            catch { }
            
            Debug.LogError($"[LogParadeDebugManager] Erreur DisplayHelpWindow: {ex}");
            
            if (helpAreaStarted)
            {
                try { GUILayout.EndArea(); helpAreaStarted = false; } catch { }
            }
            
            GUIUtility.ExitGUI();
        }
        finally
        {
            if (helpAreaStarted)
            {
                try
                {
                    GUILayout.EndArea();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[LogParadeDebugManager] Erreur EndArea help: {ex}");
                }
            }
        }
    }

    /// <summary>
    /// Remet toutes les positions de panneaux par défaut
    /// </summary>
    public void ResetAllPanelPositions()
    {
        foreach (var panel in debugPanels)
        {
            if (panel != null)
            {
                panel.ResetPosition();
            }
        }
        Debug.Log("[LogParadeDebugManager] Toutes les positions des panneaux ont été remises par défaut");
    }

    /// <summary>
    /// Sauvegarde toutes les positions de panneaux
    /// </summary>
    public void SaveAllPanelPositions()
    {
        foreach (var panel in debugPanels)
        {
            if (panel != null)
            {
                panel.SavePosition();
            }
        }
        Debug.Log("[LogParadeDebugManager] Toutes les positions des panneaux ont été sauvegardées");
    }

    // Variables pour l'aide
    [Header("Help System")]
    [Tooltip("Afficher l'aide au démarrage")]
    [SerializeField] private bool showHelpAtStart = false;
    private bool showHelp = false;

    /// <summary>
    /// Active/désactive l'affichage de l'aide
    /// </summary>
    public void ToggleHelp()
    {
        showHelp = !showHelp;
    }}
