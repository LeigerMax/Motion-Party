using UnityEngine;
using Gameplay.LogParade.Utils;

namespace Gameplay.LogParade.Tests.Debug
{
    /// <summary>
    /// Classe de base pour tous les panneaux de debug LogParade.
    /// Gère l'affichage, le positionnement et les paramètres communs.
    /// </summary>
    public abstract class BaseDebugPanel : MonoBehaviour
    {
        #region Champs & Références
        [Header("Panel Settings")]
        [Tooltip("Titre affiché en haut du panneau")]
        [SerializeField] protected string panelTitle = "Debug Panel";
        [Tooltip("Visible dès le démarrage")]
        [SerializeField] protected bool visibleAtStart = false;
        [Header("Position")]
        [Tooltip("Position du panneau sur l'écran")]
        [SerializeField] protected Vector2 anchor = new Vector2(10, 10);
        [Tooltip("Décalage par rapport à l'anchor")]
        [SerializeField] protected Vector2 offset = Vector2.zero;
        [Header("Size")]
        [Tooltip("Largeur du panneau")]
        [SerializeField] protected float width = 200f;
        [Tooltip("Hauteur du panneau (0 = auto)")]
        [SerializeField] protected float height = 0f;
        [Header("Style")]
        [Tooltip("Taux de rafraîchissement des données (secondes)")]
        [SerializeField] protected float refreshRate = 0.1f;
        [Tooltip("Utiliser un style encadré")]
        [SerializeField] protected bool useBoxStyle = true;
        // État du panneau
        protected bool isVisible = false;
        protected float lastRefreshTime = 0f;
        // Position calculée
        protected Rect panelRect;
        // Variables pour la détection de changement de résolution
        private int lastScreenWidth = 0;
        private int lastScreenHeight = 0;
        // Variables pour le glisser-déposer
        protected bool isDragging = false;
        protected Vector2 dragOffset;
        #endregion

        #region Cycle de Vie & Initialisation
        /// <summary>
        /// Indique si le panneau est actuellement visible
        /// </summary>
        public bool IsVisible => isVisible;

        protected virtual void Start()
        {
            // Charger la position sauvegardée
            LoadPosition();
            
            // Calculer la position initiale
            UpdatePanelPosition();
            
            // Appliquer l'état initial
            SetVisible(visibleAtStart);
            
            // Enregistrer auprès du manager si présent
            RegisterWithManager();
            
            // Initialiser les variables de résolution
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        protected virtual void Update()
        {
            if (isVisible && Time.time - lastRefreshTime >= refreshRate)
            {
                lastRefreshTime = Time.time;
                RefreshData();
            }
            // Suppression de HandleMouseInput ici (géré dans OnGUI)
        }
        #endregion

        #region Affichage & IMGUI
        /// <summary>
        /// Méthode à surcharger pour dessiner le contenu spécifique du panneau.
        /// </summary>
        protected abstract void DrawPanelContent();

        /// <summary>
        /// Affiche le contenu du panneau
        /// </summary>
        protected virtual void OnGUI()
        {
            if (!isVisible) return;
            try
            {
                // Gérer les interactions de souris
                HandleMouseInput();
                
                // Recalculer la position seulement si nécessaire (changement de résolution)
                if (panelRect.width != width || ShouldUpdatePosition())
                {
                    UpdatePanelPosition();
                }
                
                // Style du panneau (changement de couleur si en cours de glisser-déposer)
                Color originalColor = GUI.backgroundColor;
                bool areaStarted = false;
                
                try
                {
                    if (isDragging)
                    {
                        GUI.backgroundColor = Color.yellow;
                    }
                    
                    // Afficher le panneau
                    if (useBoxStyle)
                    {
                        GUILayout.BeginArea(panelRect, $"[{panelTitle}] {(isDragging ? "- Déplacement..." : "")}", "box");
                    }
                    else
                    {
                        GUILayout.BeginArea(panelRect);
                        GUILayout.Label($"[{panelTitle}] {(isDragging ? "- Déplacement..." : "")}", GUI.skin.label);
                    }
                    areaStarted = true;
                    
                    // Contenu spécifique
                    DrawPanelContent();
                    
                    // Boutons de contrôle en bas
                    DrawControlButtons();
                }
                catch (System.Exception ex)
                {
                    try
                    {
                        if (areaStarted)
                        {
                            GUILayout.Label($"Erreur: {ex.Message}");
                        }
                    }
                    catch { }
                    
                    LogParadeLogger.LogError($"[BaseDebugPanel] Erreur dans DrawPanelContent: {ex}");
                    
                    // Nettoyer l'état si possible
                    if (areaStarted)
                    {
                        try { GUILayout.EndArea(); areaStarted = false; } catch { }
                    }
                    
                    GUIUtility.ExitGUI();
                }
                finally
                {
                    // Assurer le nettoyage final
                    if (areaStarted)
                    {
                        try
                        {
                            GUILayout.EndArea();
                        }
                        catch (System.Exception ex)
                        {
                            LogParadeLogger.LogError($"[BaseDebugPanel] Erreur EndArea: {ex}");
                        }
                    }
                    
                    // Restaurer la couleur originale
                    GUI.backgroundColor = originalColor;
                }
            }
            catch (System.Exception ex)
            {
                LogParadeLogger.LogError($"[BaseDebugPanel] Erreur OnGUI: {ex}");
                // S'assurer que le GUI est dans un état cohérent
                GUIUtility.ExitGUI();
            }
        }

        /// <summary>
        /// Dessine les boutons de contrôle du panneau
        /// </summary>
        protected virtual void DrawControlButtons()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset Pos", GUILayout.Width(70)))
            {
                ResetPosition();
            }
            
            if (GUILayout.Button("Save", GUILayout.Width(50)))
            {
                SavePosition();
            }
            
            GUILayout.EndHorizontal();
        }
        #endregion

        #region Position & Persistance
        /// <summary>
        /// Met à jour la position du panneau en fonction des paramètres
        /// </summary>
        protected virtual void UpdatePanelPosition()
        {
            float x = anchor.x;
            float y = anchor.y;
            
            // Gérer les positions négatives (depuis le bord opposé)
            if (anchor.x < 0)
            {
                x = Screen.width + anchor.x - width;
            }
            
            if (anchor.y < 0)
            {
                y = Screen.height + anchor.y;
            }
            
            // Appliquer l'offset
            x += offset.x;
            y += offset.y;
            
            // Créer le rect
            float finalHeight = height > 0 ? height : GetEstimatedHeight();
            panelRect = new Rect(x, y, width, finalHeight);
        }

        /// <summary>
        /// Estime la hauteur nécessaire pour le contenu
        /// </summary>
        /// <returns>Hauteur estimée</returns>
        protected virtual float GetEstimatedHeight()
        {
            // Hauteur par défaut, à surcharger dans les classes filles
            return 100f;
        }

        /// <summary>
        /// Rafraîchit les données affichées
        /// </summary>
        protected virtual void RefreshData()
        {
            // À implémenter dans les classes filles
        }    /// <summary>
        /// Vérifie si la position doit être mise à jour
        /// </summary>
        /// <returns>True si la position doit être recalculée</returns>
        private bool ShouldUpdatePosition()
        {
            // Vérifier si la résolution de l'écran a changé
            if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Déplace le panneau à une nouvelle position
        /// </summary>
        /// <param name="newAnchor">Nouvelle position d'ancrage</param>
        public virtual void MoveTo(Vector2 newAnchor)
        {
            anchor = newAnchor;
            UpdatePanelPosition();
        }

        /// <summary>
        /// Déplace le panneau avec un décalage relatif
        /// </summary>
        /// <param name="delta">Décalage à appliquer</param>
        public virtual void MoveBy(Vector2 delta)
        {
            anchor += delta;
            UpdatePanelPosition();
        }

        /// <summary>
        /// Sauvegarde la position actuelle dans PlayerPrefs
        /// </summary>
        public virtual void SavePosition()
        {
            string key = $"DebugPanel_{GetType().Name}";
            PlayerPrefs.SetFloat($"{key}_X", anchor.x);
            PlayerPrefs.SetFloat($"{key}_Y", anchor.y);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Charge la position depuis PlayerPrefs
        /// </summary>
        public virtual void LoadPosition()
        {
            string key = $"DebugPanel_{GetType().Name}";
            if (PlayerPrefs.HasKey($"{key}_X") && PlayerPrefs.HasKey($"{key}_Y"))
            {
                anchor.x = PlayerPrefs.GetFloat($"{key}_X");
                anchor.y = PlayerPrefs.GetFloat($"{key}_Y");
                UpdatePanelPosition();
            }
        }

        /// <summary>
        /// Remet la position par défaut
        /// </summary>
        public virtual void ResetPosition()
        {
            // Retourner aux positions par défaut selon le type de panneau
            switch (GetType().Name)
            {
                case "DebugPanel_Score":
                    anchor = new Vector2(10, 10);
                    break;
                case "DebugPanel_Status":
                    anchor = new Vector2(220, 10);
                    break;
                case "DebugPanel_Calibration":
                    anchor = new Vector2(430, 10);
                    break;
                case "DebugPanel_GameLauncher":
                    anchor = new Vector2(640, 10);
                    break;
                default:
                    anchor = new Vector2(10, 10);
                    break;
            }
            UpdatePanelPosition();
            SavePosition();
        }
        #endregion

        #region Interaction & Drag
        /// <summary>
        /// Gère les interactions de souris pour le glisser-déposer
        /// </summary>
        protected virtual void HandleMouseInput()
        {
            if (!isVisible) return;
            
            Event currentEvent = Event.current;
            if (currentEvent == null) return;
            
            Vector2 mousePos = currentEvent.mousePosition;
            
            // Vérifier si la souris est sur le panneau
            bool mouseOnPanel = panelRect.Contains(mousePos);
            
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && mouseOnPanel)
            {
                // Commencer le glisser-déposer
                isDragging = true;
                dragOffset = mousePos - new Vector2(panelRect.x, panelRect.y);
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseDrag && isDragging)
            {
                // Continuer le glisser-déposer
                Vector2 newPosition = mousePos - dragOffset;
                MoveTo(newPosition);
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && isDragging)
            {
                // Finir le glisser-déposer
                isDragging = false;
                SavePosition(); // Sauvegarder la nouvelle position
                currentEvent.Use();
            }
        }
        #endregion

        #region Rafraîchissement & Données
        /// <summary>
        /// Méthode utilitaire pour formater les booléens
        /// </summary>
        /// <param name="value">Valeur booléenne</param>
        /// <returns>Texte formaté avec couleur</returns>
        protected string FormatBool(bool value)
        {
            return value ? "<color=green>TRUE</color>" : "<color=red>FALSE</color>";
        }

        /// <summary>
        /// Méthode utilitaire pour formater les nombres
        /// </summary>
        /// <param name="value">Valeur numérique</param>
        /// <param name="decimals">Nombre de décimales</param>
        /// <returns>Texte formaté</returns>
        protected string FormatNumber(float value, int decimals = 2)
        {
            return value.ToString($"F{decimals}");
        }

        /// <summary>
        /// Validation des paramètres dans l'inspecteur
        /// </summary>
        protected virtual void OnValidate()
        {
            // Contraintes de taille
            width = Mathf.Max(100f, width);
            refreshRate = Mathf.Max(0.05f, refreshRate);
            
            if (Application.isPlaying)
            {
                UpdatePanelPosition();
            }
        }
        #endregion

        #region Utilitaires & Validation
        /// <summary>
        /// Enregistre le panneau auprès du LogParadeDebugManager
        /// </summary>
        protected virtual void RegisterWithManager()
        {
            var manager = FindFirstObjectByType<LogParadeDebugManager>();
            if (manager != null)
            {
                manager.AddDebugPanel(this);
            }
        }
        #endregion

        #region Visibilité
        /// <summary>
        /// Active ou désactive la visibilité du panneau
        /// </summary>
        /// <param name="visible">Nouvel état de visibilité</param>
        public virtual void SetVisible(bool visible)
        {
            isVisible = visible;
            enabled = visible; // Optimisation: désactiver Update si invisible
        }
        #endregion
    }
}