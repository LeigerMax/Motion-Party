# GameAnalytics

Système de collecte de statistiques en jeu pour permettre l'analyse du comportement des joueurs dans le mini-jeu "Danse des Lucioles".

## 🔧 Fonctionnalités principales

- **Écoute automatique des événements** : S'intègre parfaitement au système d'événements existant `FireflyDanceEvents`
- **Calcul des stats de performance** : Temps de réaction, échecs, réussites, mouvements vides
- **Export JSON simple** : Données facilement utilisables dans Excel ou outils d'analyse
- **Intégration future** : Prêt pour écran d'analyse ou visualisations avancées
- **Mode debug** : Logs détaillés pour développement et tests

## 📊 Données trackées

### ⏱️ Temps de réaction
- Temps entre l'apparition d'une luciole et sa capture
- Calcul automatique des moyennes, min, max

### ❌ Clics sans capture
- Nombre de fermetures de main sans luciole à proximité
- Tracking des tentatives échouées

### 🔁 Mouvements vides
- Déplacements de main sans but apparent
- Seuil configurable pour filtrer les petits mouvements

### 🕒 Durée moyenne de capture
- Temps moyen pour capturer une luciole
- Historique des temps par luciole

### ✅ Statistiques avancées
- Distance totale parcourue par la main
- Tentatives par luciole avant capture
- Durée de vie des lucioles manquées

## 🏗️ Structure des composants

```
Analytics/
├── GameStatsRecorder.cs      # Collecteur principal d'événements
├── SessionData.cs            # Structure de données de session
├── GameStatEvent.cs          # Définition des événements trackés
└── README.md                 # Cette documentation
```

### GameStatsRecorder.cs
- **Rôle** : Collecteur principal qui écoute les événements FireflyDanceEvents
- **Fonctionnalités** : 
  - Enregistrement automatique des sessions
  - Calcul en temps réel des métriques
  - Sauvegarde JSON automatique
  - Interface de contrôle (enable/disable)

### SessionData.cs
- **Rôle** : Structure de données complète d'une session de jeu
- **Contenu** :
  - Informations de session (joueur, durée, timestamps)
  - Événements détaillés
  - Statistiques calculées
  - Méthodes d'export et de résumé

### GameStatEvent.cs
- **Rôle** : Définition des types d'événements et structures de données
- **Types trackés** :
  - Événements de lucioles (spawn, capture, expiration)
  - Événements de main (mouvement, ouverture/fermeture)
  - Événements de tentatives de capture

## 🚀 Instructions d'utilisation

### Installation
1. Ajouter le composant `GameStatsRecorder` sur un GameObject dans la scène de jeu
2. Le système s'auto-configure et se connecte aux événements existants
3. Aucune modification requise dans les autres scripts

### Configuration
- **isEnabled** : Active/désactive le système
- **enableDetailedLogging** : Logs détaillés pour debug
- **autoSaveOnGameEnd** : Sauvegarde automatique à la fin du jeu
- **handMovementThreshold** : Seuil minimum pour compter un mouvement
- **captureDistanceThreshold** : Distance max pour considérer une tentative

### Utilisation programmatique
```csharp
// Démarrer l'enregistrement manuellement
gameStatsRecorder.StartRecording("Nom du Joueur");

// Exporter les données actuelles
string jsonData = gameStatsRecorder.ExportCurrentSession();

// Obtenir un résumé
string summary = gameStatsRecorder.GetCurrentSessionSummary();

// Terminer et sauvegarder
gameStatsRecorder.EndRecording();
```

### Accès aux données
- **Dossier de sauvegarde** : `Application.persistentDataPath/GameAnalytics/`
- **Format** : JSON lisible, compatible Excel/Python/R
- **Nom des fichiers** : `FireflySession_YYYYMMDD_HHMMSS_[SessionID].json`

## 📈 Exemple de données exportées

```json
{
  "sessionId": "a1b2c3d4",
  "playerName": "Joueur Test",
  "totalDuration": 120.5,
  "capturedFireflies": 15,
  "totalFireflies": 20,
  "averageReactionTime": 2.3,
  "minReactionTime": 1.1,
  "maxReactionTime": 4.2,
  "emptyHandClosures": 3,
  "handMovementDistance": 156.7,
  "events": [
    {
      "eventType": "FireflySpawned",
      "timestamp": 5.2,
      "position": {"x": 2.1, "y": 1.5},
      "fireflyId": 12345
    }
  ]
}
```

## ✅ Bonnes pratiques respectées

- **Intégration non-intrusive** : Aucune modification des systèmes existants
- **Performance optimisée** : Calculs légers, pas d'impact sur le gameplay
- **Modularité** : Peut être activé/désactivé facilement
- **Extensibilité** : Facile d'ajouter de nouvelles métriques
- **Code documenté** : Commentaires détaillés en français
- **Gestion d'erreurs** : Try/catch pour robustesse

## 🔧 Intégration au système existant

Le système s'intègre automatiquement aux événements `FireflyDanceEvents` :

- `OnGameStarted` → Démarre l'enregistrement
- `OnGameEnded` → Termine et sauvegarde
- `OnFireflySpawned` → Track l'apparition
- `OnFireflyCaptured` → Calcule le temps de réaction
- `OnFireflyExpired` → Track les échecs
- `OnHandPositionChanged` → Track les mouvements
- `OnHandStateChanged` → Track ouverture/fermeture

## 🎯 Suggestions d'améliorations futures

### Statistiques supplémentaires utiles pour les seniors
- **Fatigue détectée** : Ralentissement progressif des réactions
- **Zones de préférence** : Régions de l'écran plus souvent visées
- **Patterns de mouvement** : Analyse des trajectoires de main
- **Pauses détectées** : Moments d'inactivité
- **Progression intra-session** : Amélioration au cours du jeu
- **Stress indicators** : Mouvements erratiques ou précipités

### Intégrations possibles
- **Dashboard temps réel** : Affichage pendant le jeu
- **Comparaison multi-sessions** : Évolution dans le temps
- **Export vers outils scientifiques** : CSV pour SPSS, R, Python
- **Alertes adaptatives** : Ajustement de difficulté basé sur les performances

## 🏷️ Version et compatibilité

- **Version** : 1.0.0
- **Unity Version** : 2021.3 LTS ou supérieur
- **Dépendances** : System.IO, Newtonsoft.Json (optionnel)
- **Namespace** : `Gameplay.FireFlyDance.Analytics`
