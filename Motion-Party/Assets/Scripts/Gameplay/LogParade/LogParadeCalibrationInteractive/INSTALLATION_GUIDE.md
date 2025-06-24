# 🎮 Guide d'Installation et d'Utilisation - Calibration Interactive LogParade

## 📋 Vue d'ensemble

Le système de calibration interactive LogParade demande au joueur de se déplacer successivement sur la lane 1 puis la lane 4 pour valider la calibration avant de commencer le jeu. Ce système garantit que le tracking fonctionne correctement et que le joueur comprend les mouvements requis.

## �️ Corrections Apportées

### ✅ Problèmes Résolus

#### 1. **Joueur ne bouge pas pendant la calibration**
- **Cause**: Le `LogParadeLateralTracker` attendait une calibration automatique
- **Solution**: Ajout du paramètre `bypassCalibrationForInteractiveMode = true`
- **Résultat**: Le joueur peut maintenant bouger librement pendant la calibration interactive

#### 2. **Score démarre avant la fin de la calibration**
- **Cause**: Les systèmes de score et de gameplay démarraient indépendamment
- **Solution**: Ajout d'un système de verrouillage global via `LogParadeCalibrationManager`
- **Résultat**: Aucun score ni gameplay ne peut démarrer avant la fin de la calibration

### 🔧 Nouveaux Composants Ajoutés

#### LogParadeCalibrationBootstrap
- **Rôle**: S'assure que la calibration démarre correctement au lancement
- **Fonctionnalités**:
  - Validation de tous les systèmes au démarrage
  - Initialisation automatique de la calibration
  - Détection et correction des problèmes courants

#### LogParadeCalibrationDiagnostic
- **Rôle**: Diagnostic en temps réel du système de calibration
- **Fonctionnalités**:
  - Vérification continue de l'état du système
  - Interface de debug visuelle
  - Correction automatique des problèmes détectés

## 🚀 Installation Rapide (MISE À JOUR)

### Étape 0: Correction des problèmes (NOUVEAU)
1. Ajoutez le script `LogParadeCalibrationBootstrap` à un GameObject dans votre scène
2. Ajoutez le script `LogParadeCalibrationDiagnostic` pour le debug (optionnel)
3. Ces scripts corrigeront automatiquement les problèmes de mouvement et de score

### Étape 1: Setup automatique
1. Dans Unity, créez un GameObject vide nommé "CalibrationSetup"
2. Ajoutez le script `LogParadeCalibrationSetup` à ce GameObject
3. Dans l'inspecteur, cliquez sur les trois points `⋯` du script
4. Sélectionnez **"Perform Auto Setup"** dans le menu contextuel
5. Le système configurera automatiquement tous les composants nécessaires

### Étape 2: Validation
1. Cliquez sur **"Validate Setup"** dans le menu contextuel du script
2. Vérifiez que tous les éléments sont verts ✅
3. Votre système de calibration est prêt !

## 🔧 Installation Manuelle (Méthode Avancée)

### Composants Requis

#### 1. LogParadeCalibrationInteractive (Script Principal)
```csharp
// À ajouter sur un GameObject "CalibrationSystem"
public class LogParadeCalibrationInteractive : MonoBehaviour
```

**Configuration dans l'inspecteur :**
- **Player Avatar**: Assigner `LogParadePlayerAvatar`
- **Lateral Tracker**: Assigner `LogParadeLateralTracker` (si disponible)
- **Lane Transforms**: Assigner les 4 Transforms représentant les voies
- **Log Prefab**: Prefab de rondin à utiliser pour la calibration
- **Instruction Text**: TextMeshProUGUI pour afficher les instructions
- **Calibration UI**: GameObject contenant l'interface de calibration

#### 2. LogParadeCalibrationManager (Gestionnaire d'Intégration)
```csharp
// À ajouter sur un GameObject "CalibrationManager"
public class LogParadeCalibrationManager : MonoBehaviour
```

**Configuration dans l'inspecteur :**
- **Calibration System**: Référence vers `LogParadeCalibrationInteractive`
- **Player Avatar**: Référence vers `LogParadePlayerAvatar`
- **UI Manager**: Référence vers `LogParadeUIManager`
- **Game Controller**: Référence vers `LogParadeGameController`

#### 3. CalibrationTextUI (UI Spécialisée - Optionnel)
```csharp
// À ajouter sur un Canvas ou Panel UI
public class CalibrationTextUI : MonoBehaviour
```

### Structure des Lanes

Le système nécessite 4 Transforms représentant les voies :
```
Lane 1: Position X = -3.0 (gauche)
Lane 2: Position X = -1.0 
Lane 3: Position X = +1.0
Lane 4: Position X = +3.0 (droite)
```

## 🎯 Utilisation

### Séquence de Calibration

1. **Début** : L'UI affiche "Placez-vous sur la lane 1..."
   - La lane 1 est mise en évidence visuellement
   - Un rondin fixe est affiché sur chaque voie
   - Timer de 15 secondes démarre

2. **Lane 1 atteinte** : L'UI affiche "Très bien ! Maintenant, allez sur la lane 4"
   - Son de succès joué
   - Lane 1 devient verte (complétée)
   - Lane 4 est mise en évidence

3. **Lane 4 atteinte** : L'UI affiche "Parfait ! Calibration terminée 🎉"
   - Son de succès joué
   - Lane 4 devient verte
   - Transition vers le jeu après 2 secondes

4. **Timeout** : Si aucun mouvement détecté après 15 secondes
   - Message "Timeout... Recommençons!"
   - Calibration redémarre automatiquement

### Intégration avec le Jeu

Après calibration réussie :
- L'événement `OnCalibrationCompleted` est déclenché
- Le `LogParadeCalibrationManager` démarre automatiquement le jeu
- L'UI de calibration se masque
- Le jeu LogParade normal commence

## 🔧 Configuration Avancée

### Paramètres de Calibration

Dans `LogParadeCalibrationInteractive` :
- **Timeout Duration** : 15s par défaut
- **Lane Detection Tolerance** : 0.5f par défaut
- **Show Debug Info** : true pour développement

### Personnalisation Visuelle

- **Highlight Color** : Couleur de surbrillance des lanes (jaune par défaut)
- **Completed Color** : Couleur des lanes complétées (vert par défaut)
- **Highlight Material** : Matériau pour mettre en évidence les rondins

### Audio

- **Success Sound** : Son joué quand une lane est atteinte
- **Timeout Sound** : Son joué en cas de timeout

## 🎨 Customisation de l'UI

### Texte d'Instructions

Modifier les messages dans `LogParadeCalibrationInteractive` :
```csharp
UpdateInstructionText("Votre message personnalisé");
```

### Indicateurs Visuels

Ajouter des éléments UI dans `CalibrationTextUI` :
- Barres de progression
- Indicateurs de voies
- Animations personnalisées

## 🧪 Tests et Debug

### Mode Debug

Activez `showDebugInfo = true` pour afficher :
- État actuel de la calibration
- Position du joueur
- Lane détectée
- Timer restant

### Interface de Debug

En mode Play, l'interface debug affiche :
- **État** : WaitingForLane1, Lane1Completed, etc.
- **Actif** : Si la calibration est en cours
- **Timer** : Temps écoulé / temps total
- **Lane joueur** : Lane actuellement détectée

### Boutons de Test

- **Start Calibration** : Démarre manuellement la calibration
- **Stop Calibration** : Arrête la calibration
- **Bypass Calibration** : Passe directement au jeu (pour tests)

## 🔗 Événements Disponibles

```csharp
// S'abonner aux événements de calibration
LogParadeCalibrationInteractive.OnCalibrationCompleted += () => {
    Debug.Log("Calibration terminée !");
};

LogParadeCalibrationInteractive.OnCalibrationFailed += () => {
    Debug.Log("Calibration échouée");
};

LogParadeCalibrationInteractive.OnLaneReached += (laneNumber) => {
    Debug.Log($"Lane {laneNumber} atteinte");
};
```

## 🛠️ Dépannage

### Problèmes Courants

1. **"PlayerAvatar non assigné!"**
   - Solution : Assigner `LogParadePlayerAvatar` dans l'inspecteur

2. **"Lane Transform X non assigné!"**
   - Solution : Créer 4 GameObjects vides positionnés sur les voies

3. **"Système de calibration non trouvé!"**
   - Solution : Vérifier que `LogParadeCalibrationInteractive` est dans la scène

4. **Le joueur ne bouge pas**
   - ✅ **RÉSOLU** : Le système configure automatiquement le `LogParadeLateralTracker`
   - Vérifier que le système de tracking UDP est actif

5. **L'UI ne s'affiche pas**
   - Vérifier qu'un Canvas est présent dans la scène
   - Vérifier que `instructionText` est assigné

6. **"Méthode StartGame non trouvée sur le GameController" (NOUVEAU)**
   - ✅ **RÉSOLU** : Le système utilise maintenant `ForceStartGame()` pour un démarrage immédiat
   - Le jeu démarre automatiquement après la calibration

7. **Erreurs de compilation (NOUVEAU)**
   - ✅ **RÉSOLU** : `ShowGameUI()` ajoutée à `LogParadeUIManager`
   - ✅ **RÉSOLU** : `Launch()` n'est plus appelée directement (protection level)
   - Utilisez `LogParadeCompilationTest` pour vérifier la compilation

### Logs Utiles

Le système affiche des logs détaillés :
```
LogParadeCalibrationInteractive: Calibration interactive démarrée
LogParadeCalibrationInteractive: Lane 1 atteinte avec succès!
LogParadeCalibrationInteractive: Calibration interactive terminée avec succès!
LogParadeGameController: Démarrage forcé du jeu après calibration
✅ Tous les systèmes de jeu ont démarré avec succès!
```

## 📝 Fichiers du Système

```
/LogParadeCalibrationInteractive/
├── LogParadeCalibrationInteractive.cs    # Script principal
├── LogParadeCalibrationManager.cs        # Gestionnaire d'intégration  
├── CalibrationTextUI.cs                  # UI spécialisée (optionnel)
├── LogParadeCalibrationSetup.cs          # Setup automatique
├── LogParadeCalibrationBootstrap.cs      # Validation démarrage (NOUVEAU)
├── LogParadeCalibrationDiagnostic.cs     # Debug temps réel (NOUVEAU)
├── LogParadeCalibrationTest.cs           # Tests unitaires (NOUVEAU)
├── LogParadeCompilationTest.cs           # Test compilation (NOUVEAU)
├── CORRECTIONS_APPLIQUEES.md             # Doc corrections (NOUVEAU)
└── README.md                             # Documentation
```

## 🏆 Bonnes Pratiques

1. **Toujours tester** la calibration avec de vrais mouvements
2. **Ajuster la tolérance** selon la précision du tracking
3. **Personnaliser les messages** selon votre public (seniors, enfants, etc.)
4. **Utiliser l'auto-setup** pour un déploiement rapide
5. **Garder les debug logs** actifs en développement

## 🚨 Notes Importantes

- ⚠️ **Le jeu ne démarre qu'après calibration réussie**
- ⚠️ **La calibration se relance automatiquement en cas de timeout**
- ⚠️ **Les rondins de calibration sont temporaires et fixes**
- ⚠️ **Le système respecte l'architecture modulaire LogParade existante**

---

✅ **Votre système de calibration interactive est maintenant prêt !**

Pour toute question ou personnalisation avancée, consultez les commentaires dans les scripts ou utilisez le mode debug.
