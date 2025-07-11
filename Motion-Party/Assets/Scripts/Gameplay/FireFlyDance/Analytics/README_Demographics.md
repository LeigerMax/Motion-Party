# GameAnalytics avec Analyse Démographique

## 🎯 Vue d'ensemble

Le système GameAnalytics a été étendu pour inclure l'analyse démographique complète, permettant de tracker et analyser les performances des joueurs en fonction de leur âge. Cette fonctionnalité est particulièrement utile pour la recherche sur les seniors et l'analyse des capacités cognitives par groupe d'âge.

## 📊 Nouvelles Fonctionnalités Démographiques

### 1. Tracking de l'Âge
- **Date de naissance** : Format YYYY-MM-DD
- **Calcul automatique de l'âge** au moment de la session
- **Classification par groupe d'âge** :
  - Mineur (< 18 ans)
  - Jeune Adulte (18-29 ans)
  - Adulte (30-49 ans)
  - Adulte Mature (50-64 ans)
  - Senior (65-79 ans)
  - Senior Avancé (80+ ans)

### 2. Analyse Comparative par Âge
- **Temps de réaction attendus** selon l'âge
- **Évaluation de performance** relative au groupe d'âge
- **Recommandations personnalisées** selon l'âge
- **Rapports comparatifs** entre groupes d'âge

### 3. Export Enrichi
- **CSV avec données démographiques** pour Excel/R/Python
- **Colonnes ajoutées** : Age, AgeGroup, BirthDate
- **Compatible** avec outils de recherche statistique

## 🛠️ Configuration

### Ajout de Joueurs avec Données Démographiques

```csharp
// Créer un joueur avec date de naissance
var player = new PlayerData("Marie Dupont", "Maison du Bonheur", "1952-03-15");
int age = player.CalculateAge(); // Calcule automatiquement l'âge
```

### Utilisation dans l'Analytique

Le système récupère automatiquement les informations du joueur actuel :

```csharp
// Le GameStatsRecorder utilise automatiquement les données démographiques
// du joueur actuel depuis le GamePlayerSelector
statsRecorder.StartRecording(); // Récupère auto le joueur et son âge
```

## 📈 Analyses Disponibles

### 1. Résumé de Session avec Âge

```csharp
var session = GetCurrentSession();
string summary = session.GetSummary();
// Inclut maintenant : âge, groupe d'âge, analyse comparative
```

### 2. Rapport Détaillé par Âge

```csharp
string report = SessionAnalyzer.GenerateDetailedReport(session);
// Inclut : performance relative à l'âge, recommandations spécifiques
```

### 3. Comparaison par Groupe d'Âge

```csharp
var sessions = LoadAllSessions();
string analysis = SessionAnalyzer.CompareSessionsByAgeGroup(sessions);
// Compare les performances entre différents groupes d'âge
```

## 🎮 Interface Éditeur

### Configuration Joueurs
- **Menu** : `GameAnalytics > Configuration Joueurs`
- **Fonctionnalités** :
  - Création de joueurs avec données démographiques
  - Validation automatique des dates
  - Calcul d'âge en temps réel
  - Test d'analytics démographiques

### Tests Intégrés
- **Menu contextuel** sur `GameAnalyticsExample`
- **Option** : `Test - Analyse Démographique`
- **Génère** : Rapports d'exemple avec différents âges

## 📊 Structure des Données

### SessionData Étendue
```csharp
public class SessionData
{
    // Nouvelles propriétés démographiques
    public string playerBirthDate;     // "YYYY-MM-DD"
    public int playerAge;              // Calculé automatiquement
    public string ageGroup;            // Classification automatique
    
    // Méthodes d'analyse par âge
    public int CalculatePlayerAge();
    public string DetermineAgeGroup(int age);
    public string GetAgeSpecificAnalysis(); // Dans GetSummary()
}
```

### Export CSV Enrichi
```
SessionID,PlayerName,Age,AgeGroup,BirthDate,SessionStart,Duration,...
abc123,Marie,72,Senior,1952-03-15,2024-01-15 14:30:00,120.5,...
```

## 🔬 Applications Recherche

### Métriques par Âge
- **Temps de réaction moyens** par groupe d'âge
- **Taux de réussite** comparatifs
- **Patterns de mouvement** selon l'âge
- **Progression** dans le temps

### Analyses Statistiques
- **Corrélations âge-performance**
- **Identification des outliers**
- **Tendances cognitives** par groupe
- **Efficacité des interventions**

### Format Compatible Recherche
- **CSV standardisé** pour SPSS/R/Python
- **Métadonnées complètes** sur chaque session
- **Horodatage précis** pour analyses longitudinales
- **Anonymisation** possible via ID unique

## 📝 Exemples d'Usage

### 1. Recherche sur les Seniors
```csharp
// Filtrer les sessions de seniors
var seniorSessions = sessions.Where(s => s.ageGroup == "Senior" || s.ageGroup == "Senior Avancé");

// Analyser l'évolution des performances
var progressAnalysis = SessionAnalyzer.CompareSessionsByAgeGroup(seniorSessions);
```

### 2. Comparaison Intergénérationnelle
```csharp
// Comparer tous les groupes d'âge
var allGroups = SessionAnalyzer.CompareSessionsByAgeGroup(allSessions);

// Export pour analyse statistique externe
var csvData = SessionAnalyzer.ExportToCSV(allSessions);
File.WriteAllText("research_data.csv", csvData);
```

### 3. Suivi Longitudinal
```csharp
// Analyser l'évolution d'un joueur dans le temps
var playerSessions = sessions.Where(s => s.playerName == "Marie Dupont")
                           .OrderBy(s => s.sessionStart);

// Tracker l'amélioration au fil des sessions
```

## 🚀 Migration

### Données Existantes
- Les sessions **sans date de naissance** fonctionnent normalement
- Les **nouveaux champs** sont optionnels (âge = 0 si non renseigné)
- **Compatibilité** assurée avec le système existant

### Ajout Progressif
1. **Configurer** les joueurs existants avec leurs dates de naissance
2. **Tester** avec l'outil éditeur intégré
3. **Analyser** les nouvelles données démographiques
4. **Exporter** pour analyses externes

## 📞 Support

- **Interface éditeur** : Menu `GameAnalytics > Configuration Joueurs`
- **Tests intégrés** : Menu contextuel sur composants
- **Logs détaillés** : Console Unity pour debugging
- **Validation** : Contrôles automatiques des formats de date

---

*Système GameAnalytics v2.0 - Avec support complet de l'analyse démographique pour la recherche sur les performances cognitives par âge.*
