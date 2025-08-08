# Système d'Analyse des Tremblements - Motion Party

## Vue d'ensemble

Le système d'analyse des tremblements a été intégré dans Motion Party pour détecter et quantifier les tremblements des mains lors des sessions de jeu. Cette fonctionnalité est particulièrement utile pour l'évaluation des capacités motrices des résidents en maison de repos.

## Fonctionnalités

### Métriques Calculées

1. **Intensité des tremblements** (`tremor_intensity`)
   - Valeur: 0-100
   - Description: Mesure l'amplitude moyenne des tremblements
   - Utilité: Indicateur de la sévérité des tremblements

2. **Fréquence des tremblements** (`tremor_frequency`)
   - Valeur: Hz (cycles par seconde)
   - Description: Fréquence dominante des mouvements oscillatoires
   - Plage normale: 3-12 Hz pour les tremblements pathologiques

3. **Nombre d'épisodes** (`tremor_episodes_count`)
   - Valeur: Nombre entier
   - Description: Nombre de périodes distinctes de tremblements détectés
   - Utilité: Évaluation de la consistance des tremblements

4. **Pourcentage du temps affecté** (`tremor_time_percentage`)
   - Valeur: 0-100%
   - Description: Proportion du temps de jeu avec tremblements détectés
   - Utilité: Impact global sur la performance

5. **Durée moyenne des épisodes** (`average_tremor_duration`)
   - Valeur: Secondes
   - Description: Durée moyenne d'un épisode de tremblement
   - Utilité: Caractérisation de la persistance

6. **État actuel** (`is_tremoring`)
   - Valeur: 0 (Non) / 1 (Oui)
   - Description: Détection en temps réel des tremblements
   - Utilité: Feedback immédiat

## Configuration

### Dans HandTracker

```csharp
[Header("Analyse des Tremblements")]
[SerializeField] private bool enableTremorAnalysis = true;
[SerializeField] private float tremorUpdateInterval = 5f; // Intervalle de mise à jour (secondes)
```

### Paramètres de l'analyseur

- **bufferSize**: Nombre de positions analysées (défaut: 30)
- **tremorThreshold**: Seuil de détection (défaut: 0.02 unités Unity)
- **minTremorFrequency**: Fréquence minimale (défaut: 3 Hz)
- **maxTremorFrequency**: Fréquence maximale (défaut: 12 Hz)

## Utilisation

### Intégration Automatique

L'analyse des tremblements s'active automatiquement dans le `HandTracker` du jeu FireflyDance lorsque `enableTremorAnalysis = true`.

### Accès aux Métriques

```csharp
// Via AnalyticsHelper
float intensity = AnalyticsHelper.GetPlayerTremorIntensity(playerId);
float frequency = AnalyticsHelper.GetPlayerTremorFrequency(playerId);
bool isCurrentlyTremoring = AnalyticsHelper.IsPlayerCurrentlyTremoring(playerId);

// Résumé complet
TremorSummary summary = AnalyticsHelper.GetPlayerTremorSummary(playerId);
TremorSeverity severity = summary.GetSeverity();
```

### Méthodes du HandTracker

```csharp
// Obtenir les métriques actuelles
TremorMetrics currentMetrics = handTracker.GetCurrentTremorMetrics();

// Réinitialiser l'analyse
handTracker.ResetTremorAnalysis();
```

## Interprétation Clinique

### Niveaux de Sévérité

1. **Minimal** (TremorSeverity.Minimal)
   - Intensité ≤ 10, Temps ≤ 5%
   - Tremblements négligeables ou physiologiques

2. **Léger** (TremorSeverity.Mild)
   - Intensité ≤ 25, Temps ≤ 15%
   - Tremblements perceptibles mais peu gênants

3. **Modéré** (TremorSeverity.Moderate)
   - Intensité ≤ 50, Temps ≤ 30%
   - Impact sur la précision des mouvements

4. **Sévère** (TremorSeverity.Severe)
   - Intensité > 50 ou Temps > 30%
   - Impact significatif sur la fonctionnalité

### Recommandations par Sévérité

- **Minimal**: Maintenir l'activité physique
- **Léger**: Surveiller l'évolution, exercices de coordination
- **Modéré**: Évaluation médicale, rééducation motrice
- **Sévère**: Consultation médicale urgente

## Test et Validation

### Script de Démonstration

Utilisez `TremorAnalysisDemo` pour tester et calibrer le système:

1. Attachez le script à un GameObject
2. Activez `enableAutoTest` pour la simulation
3. Utilisez `simulateTremor` pour tester la détection
4. Observez les métriques en temps réel

### Commandes de Test

- **Générer Rapport**: Analyse complète avec recommandations
- **Toggle Simulation**: Active/désactive les tremblements simulés
- **Réinitialiser**: Remet à zéro l'analyse

## Intégration dans les Rapports

### SessionAnalyzer

Les métriques de tremblements sont automatiquement incluses dans l'analyse de session:

```csharp
// Dans AnalyzeFireflyPerformance
specialMetrics = new Dictionary<string, float>
{
    { "tremor_intensity", playerMetrics.GetMetric("tremor_intensity") },
    { "tremor_frequency", playerMetrics.GetMetric("tremor_frequency") },
    { "tremor_episodes_count", playerMetrics.GetMetric("tremor_episodes_count") }
};
```

### Formatage des Métriques

Les métriques sont automatiquement formatées pour l'affichage:
- Intensité: Format décimal (ex: "15.32")
- Fréquence: Format avec unité (ex: "6.5 Hz")
- Pourcentage: Format pourcentage (ex: "12.5%")
- État: Format texte (ex: "Oui"/"Non")

## Considérations Techniques

### Performance

- Analyse en temps réel sans impact significatif sur les performances
- Buffer circulaire pour une utilisation mémoire optimale
- Mise à jour périodique configurable des métriques

### Précision

- Basé sur l'analyse des variations de position des landmarks de main
- Filtrage par fréquence pour éliminer les faux positifs
- Seuils configurables selon les besoins cliniques

### Limitations

- Nécessite une détection stable de la main
- Sensible à la qualité de la caméra et de l'éclairage
- Calibration requise selon l'environnement d'utilisation

## Extension Future

### Améliorations Possibles

1. **Analyse multi-axes**: Séparation des tremblements X/Y
2. **Classification automatique**: ML pour identifier les types de tremblements
3. **Tendances temporelles**: Évolution des tremblements sur plusieurs sessions
4. **Comparaison normative**: Base de données de référence par âge/condition

### Intégration avec d'Autres Mini-jeux

Le système peut être étendu aux autres mini-jeux en:
1. Ajoutant l'analyseur aux trackers de mouvement existants
2. Adaptant les seuils selon le type d'activité
3. Intégrant les métriques dans les analyses spécifiques

## Support et Maintenance

### Debugging

- Activez `enableDebugMode` dans HandTracker pour les logs détaillés
- Utilisez `TremorAnalysisDemo` pour valider le fonctionnement
- Vérifiez les métriques via `AnalyticsDebugTools.ShowPlayerMetrics()`

### Calibration

Ajustez les paramètres selon l'environnement:
- `tremorThreshold`: Plus bas pour plus de sensibilité
- Fréquences min/max: Selon les populations ciblées
- `updateInterval`: Balance entre réactivité et performance
