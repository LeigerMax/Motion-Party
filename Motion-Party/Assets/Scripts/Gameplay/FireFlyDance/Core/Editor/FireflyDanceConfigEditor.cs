using UnityEngine;
using UnityEditor;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Hand;

namespace Gameplay.FireFlyDance.Core.Editor
{
    /// <summary>
    /// Éditeur personnalisé pour faciliter la création et la gestion de FireflyDanceConfig
    /// </summary>
    [CustomEditor(typeof(FireflyDanceConfig))]
    public class FireflyDanceConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Réinitialiser aux valeurs par défaut"))
            {
                ResetToDefaults();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Informations calculées", EditorStyles.boldLabel);
            
            var config = (FireflyDanceConfig)target;
            EditorGUILayout.LabelField($"Largeur: {config.Width:F2}");
            EditorGUILayout.LabelField($"Hauteur: {config.Height:F2}");
            EditorGUILayout.LabelField($"Centre: {config.Center}");
        }
        
        private void ResetToDefaults()
        {
            var config = (FireflyDanceConfig)target;
            
            Undo.RecordObject(config, "Reset FireflyDanceConfig to defaults");
            
            // Réinitialiser aux valeurs par défaut
            var defaultConfig = FireflyDanceConfig.CreateDefault();
            
            EditorUtility.CopySerializedManagedFieldsOnly(defaultConfig, config);
            
            EditorUtility.SetDirty(config);
            
            Debug.Log("FireflyDanceConfig réinitialisé aux valeurs par défaut");
        }
    }

    /// <summary>
    /// Menu pour créer facilement une configuration FireflyDance
    /// </summary>
    public static class FireflyDanceConfigMenu
    {
        [MenuItem("Assets/Create/Motion Party/Firefly Dance Config", priority = 1)]
        public static void CreateFireflyDanceConfig()
        {
            // Obtenir le chemin du dossier sélectionné
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = "Assets";
            }
            else if (!AssetDatabase.IsValidFolder(assetPath))
            {
                assetPath = System.IO.Path.GetDirectoryName(assetPath);
            }
            
            // Créer une instance de configuration
            var config = FireflyDanceConfig.CreateDefault();
            
            // Générer un nom unique
            string configPath = AssetDatabase.GenerateUniqueAssetPath($"{assetPath}/FireflyDanceConfig.asset");
            
            // Créer l'asset
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            
            // Sélectionner le nouvel asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            
            Debug.Log($"FireflyDanceConfig créé à: {configPath}");
        }
        
        [MenuItem("Tools/Motion Party/Create Firefly Dance Setup")]
        public static void CreateCompleteSetup()
        {
            // Créer un GameObject pour le setup complet
            var setupRoot = new GameObject("FireflyDance_Setup");
            
            // Ajouter le GameManager
            var gameManager = setupRoot.AddComponent<FireflyDanceGameManager>();
            
            // Créer et assigner une configuration
            var config = FireflyDanceConfig.CreateDefault();
            string configPath = AssetDatabase.GenerateUniqueAssetPath("Assets/FireflyDanceConfig_Generated.asset");
            AssetDatabase.CreateAsset(config, configPath);
            gameManager.config = config;
            
            // Créer un objet HandTracker
            var handTrackerGO = new GameObject("HandTracker");
            handTrackerGO.transform.SetParent(setupRoot.transform);
            var handTracker = handTrackerGO.AddComponent<HandTracker>();
            gameManager.handTracker = handTracker;
            
            AssetDatabase.SaveAssets();
            
            Debug.Log("Setup FireflyDance complet créé!");
            Selection.activeObject = setupRoot;
        }
    }
}
