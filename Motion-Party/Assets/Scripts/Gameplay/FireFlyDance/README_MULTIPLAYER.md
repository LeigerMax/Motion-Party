# 🎮 Intégration Système Multi-Joueurs - FireflyDance

## 📋 Vue d'ensemble

Le mini-jeu FireflyDance a été modifié pour supporter complètement le système multi-joueurs. Chaque joueur joue à tour de rôle, et leurs scores sont enregistrés individuellement.

## 🔧 Composants ajoutés

### 1. **FireflyDanceGameManager** (modifié)
- **Gestion des tours de joueurs** : Affiche le joueur actuel, enregistre son score, passe au suivant
- **Intégration GamePlayerSelector** : Utilise le système central de sélection de joueurs
- **Redémarrage automatique** : Relance le jeu pour chaque joueur
- **Configuration** : Délai entre joueurs configurable

### 2. **FireflyScoreManagerPlayerIntegration** (nouveau)
- **Récupération des scores** : Interface pour accéder aux scores du FireflyScoreManager
- **Réinitialisation** : Remet les scores à zéro entre les tours
- **Compatibilité** : Fonctionne avec différentes versions du ScoreManager

### 3. **FireflyPlayerDisplayUI** (nouveau)
- **Affichage du joueur actuel** : Nom, équipe, numéro de tour
- **Messages de transition** : "Au tour de Alice !"
- **Couleurs par joueur** : Arrière-plan coloré pour chaque joueur
- **Auto-configuration** : Trouve automatiquement les composants UI

### 4. **FireflyMultiPlayerTester** (nouveau)
- **Tests complets** : Création de joueurs, sélection, lancement
- **Debug** : Informations sur l'état actuel du système
- **Workflow automatique** : Test de bout en bout

## 🚀 Guide d'utilisation

### Configuration dans Unity

#### A. Modifier le FireflyDanceGameManager
```
1. Ouvrir la scène FireflyDance
2. Sélectionner le GameObject avec FireflyDanceGameManager
3. Dans l'Inspector :
   ✅ Enable Player System = true
   ✅ Delay Between Players = 3f (secondes)
   ✅ Score Integration = (assigner automatiquement)
   ✅ Player Display UI = (assigner automatiquement)
```

#### B. Ajouter l'intégration des scores
```
1. Créer un GameObject vide "PlayerScoreIntegration"
2. Attacher le script FireflyScoreManagerPlayerIntegration
3. Le FireflyDanceGameManager le trouvera automatiquement
```

#### C. Ajouter l'affichage du joueur
```
1. Dans votre Canvas UI, créer un panel "CurrentPlayerDisplay"
2. Ajouter des TextMeshProUGUI :
   - PlayerNameText (nom du joueur)
   - PlayerIndexText (tour X/Y)
   - TeamNameText (équipe - optionnel)
3. Attacher le script FireflyPlayerDisplayUI au panel
4. Le script trouvera automatiquement les composants
```

#### D. Ajouter le testeur (optionnel)
```
1. Créer un GameObject vide "MultiPlayerTester"
2. Attacher FireflyMultiPlayerTester
3. Utiliser les Context Menu pour tester
```

### Workflow de jeu

#### 1. Avant le jeu
```
Menu Principal → Équipe → Créer des joueurs
Menu Principal → Jouer → Sélection des joueurs → Confirmer
```

#### 2. Pendant le jeu
```
Tour 1: Alice joue → Score enregistré → Transition (3s)
Tour 2: Bob joue → Score enregistré → Transition (3s)
...
Fin: Tous les joueurs ont joué → Retour au menu
```

#### 3. Système de scores
```
- Chaque capture de luciole ajoute des points au joueur actuel
- Le score du tour est affiché en temps réel
- À la fin du tour, le score est ajouté au total du joueur
- Le score est sauvegardé dans le système PlayerData
```

## 🎮 Fonctionnalités

### ✅ Gestion multi-joueurs
- **Tours automatiques** : Passage automatique entre joueurs
- **Scores individuels** : Chaque joueur a son propre score
- **Affichage du joueur actuel** : Nom et numéro de tour
- **Messages de transition** : "Au tour de Alice !"

### ✅ Interface utilisateur
- **Affichage temps réel** : Joueur actuel toujours visible
- **Couleurs par joueur** : Identification visuelle
- **Messages de transition** : Feedback clair entre tours
- **Auto-configuration** : Trouve automatiquement les éléments UI

### ✅ Configuration flexible
- **Mode solo/multi** : Détection automatique du nombre de joueurs
- **Délai configurable** : Temps entre les tours ajustable
- **Debug complet** : Outils de test et de diagnostic

### ✅ Intégration système
- **GamePlayerSelector** : Utilise le système central
- **PlayerData** : Sauvegarde automatique des scores
- **Événements** : Communication via le système d'événements

## 🛠️ Test et Debug

### Tests automatiques
```
Context Menu sur FireflyMultiPlayerTester :
- "Test - Full Workflow" : Test complet de bout en bout
- "Test - Create Test Players" : Crée des joueurs Alice, Bob, etc.
- "Test - Select Players for Game" : Sélectionne 2 joueurs
- "Test - Start Game Session" : Lance la session multi-joueurs
```

### Debug en cours de jeu
```
Context Menu sur FireflyDanceGameManager :
- "Debug Current Player" : Infos sur le joueur actuel
- "Debug Player Ranking" : Classement des joueurs
- "Force Next Player" : Passer au joueur suivant
- "Force Restart Game" : Redémarrer le jeu manuellement
- "Force Complete Restart" : Réinitialisation complète du système
- "Force Shutdown All Systems" : Arrêt complet de tous les modules
- "Debug GameController State" : État détaillé du contrôleur
- "Force Finish MiniGame" : Terminer immédiatement
```

### Vérification du système
```
Context Menu sur FireflyMultiPlayerTester :
- "Test - Check Systems" : État de tous les composants
```

## 🔧 Configuration avancée

### Personnalisation de l'UI

#### Couleurs des joueurs
```csharp
// Dans FireflyPlayerDisplayUI
[SerializeField] private Color[] playerColors = new Color[]
{
    Color.blue,    // Joueur 1
    Color.red,     // Joueur 2
    Color.green,   // Joueur 3
    Color.yellow,  // Joueur 4
    // ...
};
```

#### Messages personnalisés
```csharp
// Dans FireflyDanceGameManager
private void ShowTransitionMessage()
{
    playerDisplayUI.ShowTransitionMessage($"🎯 Au tour de {currentPlayer.Nickname} !", delayBetweenPlayers);
}
```

### Configuration du délai
```csharp
// Dans FireflyDanceGameManager Inspector
[Header("Player System")]
public float delayBetweenPlayers = 3f; // Délai en secondes
```

## 🐛 Dépannage

### Problèmes courants

#### Le système multi-joueurs ne se lance pas
```
✅ Vérifier que enablePlayerSystem = true
✅ Vérifier que des joueurs sont sélectionnés
✅ Vérifier les logs pour les erreurs d'initialisation
```

#### L'affichage du joueur ne fonctionne pas
```
✅ Vérifier que FireflyPlayerDisplayUI est attaché
✅ Vérifier que les TextMeshProUGUI sont assignés
✅ Utiliser "Debug UI References" pour diagnostiquer
```

#### Les scores ne sont pas enregistrés
```
✅ Vérifier que FireflyScoreManagerPlayerIntegration est présent
✅ Vérifier que le FireflyScoreManager existe
✅ Utiliser "Debug Score Access Methods" pour diagnostiquer
```

#### Le passage entre joueurs ne fonctionne pas
```
✅ Vérifier que GamePlayerSelector.Instance n'est pas null
✅ Vérifier les logs pour les erreurs de NextPlayerTurn()
✅ Utiliser "Force Next Player" pour tester manuellement
✅ Utiliser "Debug GameController State" pour voir l'état du contrôleur
✅ Utiliser "Force Restart Game" si le jeu ne redémarre pas
✅ Vérifier que le GameController a bien une méthode pour se réinitialiser
```

### Logs de debug
```
Activer debugMode = true dans les composants pour voir les logs détaillés :
- FireflyDanceGameManager.enableDetailedLogs = true
- FireflyScoreManagerPlayerIntegration.debugMode = true
- FireflyMultiPlayerTester.debugMode = true
```

## 📋 Logs attendus

### Lancement normal
```
🚀 Launch() appelé - Début du lancement du mini-jeu
Système de joueurs initialisé - Joueur actuel: Alice
Session multi-joueurs détectée - 2 joueurs
🎮 Jeu démarré pour Alice
```

### Changement de joueur
```
🏁 Fin de tour pour Alice - Score: 150
Score enregistré pour Alice: 150 points
Changement de joueur: Alice -> Bob
Préparation pour Bob - Attente de 3s
Redémarrage du jeu pour Bob
🎮 Jeu démarré pour Bob
```

### Fin de partie
```
🏁 Fin de tour pour Bob - Score: 120
Tous les joueurs ont joué - Fin du mini-jeu
=== CLASSEMENT ===
1. Alice - 150 points
2. Bob - 120 points
```

## 🚀 Évolutions futures

### Fonctionnalités prévues
- **Classement en temps réel** : Affichage du classement pendant le jeu
- **Animations de transition** : Effets visuels entre joueurs
- **Son personnalisé** : Annonces vocales des joueurs
- **Statistiques avancées** : Historique des performances

### Optimisations possibles
- **Pool de GameObjects** : Réutilisation des éléments UI
- **Mise en cache** : Éviter les FindObjectOfType répétés
- **Async loading** : Transitions plus fluides

---

*Ce système respecte l'architecture modulaire du projet et peut être facilement adapté à d'autres mini-jeux.*
