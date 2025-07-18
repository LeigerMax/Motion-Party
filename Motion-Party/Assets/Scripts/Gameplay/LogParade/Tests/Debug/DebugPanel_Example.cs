using UnityEngine;
using Gameplay.LogParade.Utils;

namespace Gameplay.LogParade.Tests.Debug
{
    /// <summary>
    /// Panneau de debug d'exemple pour démontrer l'utilisation du système de debug UI
    /// </summary>
    public class DebugPanel_Example : BaseDebugPanel
    {
    #region Champs & Configuration
        [Header("Configuration Exemple")]
        [SerializeField] private bool showAdvancedControls = true;
        [SerializeField] private string exampleMessage = "Ceci est un panneau d'exemple";
        // Variables de démonstration
        private int counterValue = 0;
        private float sliderValue = 0.5f;
        private bool toggleValue = false;
    #endregion

    #region Initialisation
        protected override void Start()
        {
            base.Start();
            panelTitle = "Exemple";
            width = 250;
            height = 300;
            anchor = new Vector2(Screen.width - width - 20, 150);
            // Log supprimé (inutile en démo)
        }
    #endregion

    #region Affichage
        protected override void DrawPanelContent()
        {
            try
            {
                var boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                var wrapStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
                GUILayout.Label("🎮 PANNEAU D'EXEMPLE", boldStyle);
                GUILayout.Space(5);
                GUILayout.Label(exampleMessage, wrapStyle);
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Compteur: {counterValue}");
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    counterValue--;
                    LogParadeLogger.Log($"[DebugPanel_Example] Compteur: {counterValue}");
                }
                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    counterValue++;
                    LogParadeLogger.Log($"[DebugPanel_Example] Compteur: {counterValue}");
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.Label($"Valeur: {sliderValue:F2}");
                sliderValue = GUILayout.HorizontalSlider(sliderValue, 0f, 1f);
                GUILayout.Space(5);
                toggleValue = GUILayout.Toggle(toggleValue, "Option exemple");
                GUILayout.Space(10);
                if (GUILayout.Button("Action Exemple"))
                {
                    LogParadeLogger.Log($"[DebugPanel_Example] Action exécutée ! Compteur={counterValue}, Slider={sliderValue:F2}, Toggle={toggleValue}");
                }
                if (GUILayout.Button("Reset Valeurs"))
                {
                    counterValue = 0;
                    sliderValue = 0.5f;
                    toggleValue = false;
                    LogParadeLogger.Log("[DebugPanel_Example] Valeurs réinitialisées");
                }
                if (showAdvancedControls)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("🔧 CONTRÔLES AVANCÉS:", boldStyle);
                    if (GUILayout.Button("Test Log Info"))
                    {
                        LogParadeLogger.Log("[DebugPanel_Example] Ceci est un message d'information");
                    }
                    if (GUILayout.Button("Test Log Warning"))
                    {
                        LogParadeLogger.LogWarning("[DebugPanel_Example] Ceci est un avertissement");
                    }
                    if (GUILayout.Button("Test Log Error"))
                    {
                        LogParadeLogger.LogError("[DebugPanel_Example] Ceci est une erreur (test)");
                    }
                }
            }
            catch (System.Exception ex)
            {
                GUILayout.Label($"❌ Erreur: {ex.Message}");
                LogParadeLogger.LogError($"[DebugPanel_Example] Erreur DrawPanelContent: {ex}");
            }
        }
    #endregion

    #region Contrôles & Actions
        protected override void DrawControlButtons()
        {
            base.DrawControlButtons();
            GUILayout.Space(5);
            if (GUILayout.Button("Contrôles Avancés"))
            {
                showAdvancedControls = !showAdvancedControls;
                LogParadeLogger.Log($"[DebugPanel_Example] Contrôles avancés: {showAdvancedControls}");
            }
        }
    #endregion

    #region Méthodes de Test
        [ContextMenu("Test Panel Functions")]
        public void TestPanelFunctions()
        {
            LogParadeLogger.Log($"[DebugPanel_Example] Test des fonctions du panneau:");
            LogParadeLogger.Log($"  - Compteur: {counterValue}");
            LogParadeLogger.Log($"  - Slider: {sliderValue:F2}");
            LogParadeLogger.Log($"  - Toggle: {toggleValue}");
            LogParadeLogger.Log($"  - Contrôles avancés: {showAdvancedControls}");
            LogParadeLogger.Log($"  - Position: {panelRect.position}");
            LogParadeLogger.Log($"  - Taille: {panelRect.size}");
        }
    #endregion
    }
}