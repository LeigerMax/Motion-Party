using UnityEngine;
using Core; // Ajout pour UDPReceive

/// <summary>
/// Utilitaire pour valider la configuration complète du système LogParade
/// À utiliser dans l'éditeur Unity pour s'assurer que tout est bien configuré
/// </summary>
public class LogParadeSetupValidator : MonoBehaviour
{
    [Header("Validation")]
    [Tooltip("Cliquer pour valider la configuration complète")]
    public bool validateSetup = false;
    
    [Header("Résultats de validation")]
    [TextArea(10, 15)]
    public string validationResults = "Cliquez 'Validate Setup' pour démarrer la validation...";

    void OnValidate()
    {
        if (validateSetup)
        {
            ValidateCompleteSetup();
            validateSetup = false; // Reset pour éviter les validations répétées
        }
    }

    /// <summary>
    /// Valide la configuration complète du système LogParade
    /// </summary>
    [ContextMenu("Validate LogParade Setup")]
    public void ValidateCompleteSetup()
    {
        System.Text.StringBuilder results = new System.Text.StringBuilder();
        results.AppendLine("=== VALIDATION LOGPARADE SETUP ===\n");
        
        int errors = 0;
        int warnings = 0;
        int successes = 0;

        // 1. Valider les composants principaux
        results.AppendLine("🔍 COMPOSANTS PRINCIPAUX :");
        errors += ValidateComponent<LogParadeGameController>("LogParadeGameController", results);
        errors += ValidateComponent<LogParadeLateralTracker>("LogParadeLateralTracker", results);
        errors += ValidateComponent<LogParadeUIManager>("LogParadeUIManager", results);
        errors += ValidateComponent<UDPReceive>("UDPReceive (Core)", results);
        
        // 2. Valider les lanes manuelles
        results.AppendLine("\n🏗️ LANES MANUELLES :");
        var laneVisualizer = FindObjectOfType<LogParadeLaneVisualizer>();
        if (laneVisualizer != null)
        {
            bool allLanesValid = true;
            for (int i = 0; i < 4; i++)
            {
                if (laneVisualizer.manualLanes[i] == null)
                {
                    results.AppendLine($"❌ Lane {i + 1} non assignée !");
                    allLanesValid = false;
                    errors++;
                }
                else
                {
                    results.AppendLine($"✅ Lane {i + 1} : {laneVisualizer.manualLanes[i].name}");
                    successes++;
                }
            }
            
            if (allLanesValid)
            {
                float spacing = laneVisualizer.GetLaneSpacing();
                results.AppendLine($"📏 Espacement des lanes : {spacing:F2} unités");
            }
        }
        else
        {
            results.AppendLine("❌ LogParadeLaneVisualizer non trouvé !");
            errors++;
        }

        // 3. Valider la configuration de tracking
        results.AppendLine("\n🎯 CONFIGURATION TRACKING :");
        var tracker = FindObjectOfType<LogParadeLateralTracker>();
        if (tracker != null)
        {
            results.AppendLine($"📷 Caméra : {tracker.cameraInputWidth}x{tracker.cameraInputHeight}");
            results.AppendLine($"⚖️ Échelle : {tracker.trackingScale}");
            results.AppendLine($"🎯 Zone morte : {tracker.centralDeadZone}px");
            results.AppendLine($"⏱️ Validation voie : {tracker.laneChangeValidationTime}s");
            results.AppendLine($"📏 Seuil mouvement : {tracker.laneChangeThreshold}");
            
            // Recommandations selon les valeurs
            if (tracker.laneChangeValidationTime < 1.0f)
            {
                results.AppendLine("⚠️ Validation rapide - peut convenir aux jeunes mais attention aux seniors !");
                warnings++;
            }
            else
            {
                results.AppendLine("✅ Validation adaptée au public senior");
                successes++;
            }
            
            if (tracker.centralDeadZone < 25f)
            {
                results.AppendLine("⚠️ Zone morte petite - peut causer des micro-changements");
                warnings++;
            }
        }

        // 4. Vérifier la configuration UDP
        results.AppendLine("\n📡 CONFIGURATION UDP :");
        var udpReceive = FindObjectOfType<UDPReceive>();
        if (udpReceive != null)
        {
            results.AppendLine($"🌐 Port : {udpReceive.port}");
            if (udpReceive.port == 5052)
            {
                results.AppendLine("✅ Port correct pour MediaPipe");
                successes++;
            }
            else
            {
                results.AppendLine("⚠️ Port différent de 5052 - vérifier la configuration Python");
                warnings++;
            }
        }

        // 5. Résumé final
        results.AppendLine("\n" + new string('=', 40));
        results.AppendLine($"📊 RÉSUMÉ :");
        results.AppendLine($"✅ Succès : {successes}");
        results.AppendLine($"⚠️ Avertissements : {warnings}");  
        results.AppendLine($"❌ Erreurs : {errors}");
        
        if (errors == 0)
        {
            results.AppendLine("\n🎉 CONFIGURATION VALIDE ! Prêt pour les tests !");
        }
        else
        {
            results.AppendLine($"\n🚨 {errors} erreur(s) à corriger avant les tests !");
        }

        validationResults = results.ToString();
        
        // Log dans la console aussi
        Debug.Log(validationResults);
    }

    /// <summary>
    /// Valide la présence d'un composant dans la scène
    /// </summary>
    private int ValidateComponent<T>(string componentName, System.Text.StringBuilder results) where T : Object
    {
        var component = FindObjectOfType<T>();
        if (component != null)
        {
            results.AppendLine($"✅ {componentName} trouvé");
            return 0; // Pas d'erreur
        }
        else
        {
            results.AppendLine($"❌ {componentName} MANQUANT !");
            return 1; // Une erreur
        }
    }
}
