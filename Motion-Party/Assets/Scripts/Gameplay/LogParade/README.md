# Le Défilé des Rondins - Mini-jeu

## 🎯 Description

**"Le Défilé des Rondins"** est un mini-jeu de tracking latéral où le joueur se déplace physiquement de gauche à droite devant une webcam pour contrôler un avatar sur 4 voies verticales à l'écran. Le système utilise les données MediaPipe pour détecter la position latérale du joueur et mapper cette position sur les voies.

---

## 📁 Scripts créés

### Scripts principaux

1. **`LogParadeGameController.cs`** - Contrôleur principal du mini-jeu
   - Hérite de `MiniGameBase` (comme les autres mini-jeux)
   - Gère le cycle de vie du jeu
   - Orchestre les interactions entre les composants
   - Traite les données MediaPipe via UDPReceive

2. **`LogParadeLateralTracker.cs`** - Système de tracking latéral
   - Analyse les données MediaPipe pour extraire la position X
   - Calibration automatique pour s'adapter à l'utilisateur
   - Mapping de la position physique sur 4 voies
   - Événements pour notifier les changements de voie

3. **`LogParadePlayerAvatar.cs`** - Gestion de l'avatar joueur
   - Déplacement fluide entre les voies
   - Animation et rotation vers la direction du mouvement
   - Effets visuels et sonores lors des changements de voie
   - Visualisation des voies dans l'éditeur

4. **`LogParadeUIManager.cs`** - Interface utilisateur
   - Affichage de la voie actuelle et position du joueur
   - Interface de calibration avec barre de progression
   - Mode debug avec informations détaillées
   - Indicateurs visuels des 4 voies

### Scripts utilitaires

5. **`LogParadeLaneVisualizer.cs`** - Visu

   - Génère automatiquement les objets visuels pour les 4 voies
   - Affichage avec couleurs distinctes et labels
   - Gizmos dans l'éditeur pour faciliter le setup
   - Fonctions pour mettre en surbrillance une voie

6. **`LogParadeInputSimulator.cs`** - Simulateur pour tests sans webcam
   - Simule les données MediaPipe avec le clavier
   - Mouvements manuels (A/D) et automatiques (Espace)
   - Interface de debug avec boutons pour chaque voie
   - Idéal pour le développement et tests

### Scripts de configuration

7. **`LogParadeConfig.cs`** - Configuration globale (ScriptableObject)
   - Paramètres réutilisables pour tous les composants
   - Profils de configuration pour différents environnements
   - Application automatique des paramètres à la scène

8. **`LogParadeSceneSetup.cs`** - Outil de setup automatique
   - Création automatique de la hiérarchie d'objets
   - Configuration des références entre composants
   - Nettoyage et reconfiguration de scène

---

## 🛠️ Intégration dans Unity

### 1. Création de la scène

1. **Créer une nouvelle scène** : `Assets/Scenes/MiniGames/MiniGame_LogParade.unity`
   
   **OU** utiliser le setup automatique :
   - Ajouter `LogParadeSceneSetup` à un GameObject temporaire
   - Cliquer "Setup LogParade Scene" dans le menu contextuel
   - Supprimer le GameObject de setup

2. **Créer manuellement la hiérarchie d'objets** (si pas de setup auto) :
   ```
   LogParadeManager (GameObject)
   ├── LogParadeGameController (Script)
   ├── UDPReceive (Script - de Core namespace)
   ├── PlayerAvatar (GameObject)
   │   ├── LogParadePlayerAvatar (Script)
   │   └── [Modèle 3D ou Cube temporaire]
   ├── LateralTracker (GameObject)
   │   └── LogParadeLateralTracker (Script)
   ├── LaneVisualizer (GameObject)
   │   └── LogParadeLaneVisualizer (Script)
   ├── InputSimulator (GameObject) [Optionnel - pour tests]
   │   └── LogParadeInputSimulator (Script)
   └── UI (Canvas)
       ├── LogParadeUIManager (Script)
       ├── CurrentLaneText (TextMeshPro)
       ├── PositionText (TextMeshPro)
       ├── GameStatusText (TextMeshPro)
       ├── LaneIndicators (4 x Image)
       ├── CalibrationPanel (Panel)
       │   ├── CalibrationSlider (Slider)
       │   └── CalibrationText (TextMeshPro)
       └── DebugPanel (Panel)
           ├── DebugToggle (Toggle)
           └── DebugInfoText (TextMeshPro)
   ```

### 2. Configuration des objets

#### LogParadeGameController
- **UdpReceive** : Assigner le composant UDPReceive
- **UiManager** : Assigner LogParadeUIManager
- **LateralTracker** : Assigner LogParadeLateralTracker
- **PlayerAvatar** : Assigner LogParadePlayerAvatar
- **Lane Markers** : Créer 4 GameObjects positionnés sur les voies
- **Start Delay** : 1f (par défaut)

#### LogParadeLateralTracker
- **UdpReceive** : Assigner le même UDPReceive que le GameController
- **Smoothing Factor** : 0.8f (ajustable)
- **Left/Right Boundary** : -1.5f / 1.5f (ajustable selon l'espace physique)
- **Enable Auto Calibration** : true
- **Calibration Time** : 3f

#### LogParadePlayerAvatar
- **Move Speed** : 5f
- **Lane Width** : 2f (distance entre les voies)
- **Base Position** : Position de référence au centre
- **Avatar Model** : Assigner le GameObject visuel (cube, modèle 3D)
- **Lane Change Effects** : Optionnel - ParticleSystem
- **Lane Change Sound** : Optionnel - AudioClip

#### LogParadeUIManager
- Assigner tous les éléments UI créés dans le Canvas

#### LogParadeLaneVisualizer
- **Lane Width** : 2f (doit correspondre à LogParadePlayerAvatar)
- **Lane Length** : 20f (longueur des voies visuelles)
- **Base Position** : Position de référence (0,0,0)
- **Lane Colors** : Rouge, Jaune, Vert, Bleu (par défaut)
- Cliquer sur "Generate Lanes" dans l'inspecteur ou menu contextuel

#### LogParadeInputSimulator (Optionnel - Tests)
- **Target UDPReceive** : Assigner le même UDPReceive
- **Enable Simulation** : true pour activer les tests au clavier
- **Simulation Speed** : 2f (vitesse de mouvement simulé)
- **Simulation Range** : 3f (amplitude des mouvements)

#### Configuration avec ScriptableObject (Recommandé)
1. **Créer une configuration** : Clic droit → Create → LogParade → Game Configuration
2. **Assigner la config** : Glisser vers LogParadeSceneSetup ou appliquer manuellement
3. **Appliquer** : Utiliser `config.ApplyToComponents()` ou le bouton dans l'inspecteur

### 3. Configuration des composants système

#### UDPReceive (namespace Core)
- **Port** : 5052 (par défaut)
- **Start Receiving** : true
- **Print To Console** : false (sauf pour debug)

---

## 🔌 Branchement des données MediaPipe

### Flux de données actuel
Le système utilise maintenant **prioritairement les données de pose** pour un tracking plus précis :

```json
{
  "pose_landmarks": [
    [x, y, z],  // Position de la tête/nez (landmark 0)
    [x, y, z],  // Autres landmarks...
    ...
  ],
  "hand_positions": [
    [x, y, z],  // Position du poignet (fallback)
    ...
  ]
}
```

### Ordre de priorité du tracking
1. **Position de la tête** (landmark 0 de `pose_landmarks`) - **Recommandé**
2. **Position de la main** (premier élément de `hand_positions`) - Fallback

### Données envoyées par le script Python
Le script `python-tracker/main.py` envoie déjà les bonnes données :
- `pose_landmarks` : Array de 33 landmarks incluant la tête (index 0)
- `hand_positions` : Array de 21 landmarks de la main

**✅ Aucune modification nécessaire** dans les scripts Python - le système LogParade s'adapte automatiquement aux données disponibles.

### Avantages du tracking par la tête
- **Plus stable** : Moins de tremblements que les mains
- **Plus fiable** : Position du corps plus représentative
- **Meilleure précision** : Pour le mouvement latéral global
- **Compatibilité** : Fonctionne même si les mains ne sont pas visibles

---

## 🧪 Test et calibration

### 1. Test basique
1. Lancer la scène `MiniGame_LogParade`
2. **Avec MediaPipe** : S'assurer que le tracker Python envoie des données sur le port 5052
3. **Sans MediaPipe** : Activer LogParadeInputSimulator et utiliser les touches A/D pour tester
4. Observer le debug GUI in-game pour voir les données reçues
5. Bouger latéralement devant la caméra (ou utiliser le clavier en mode simulation)

### 2. Test avec simulateur (sans webcam)
1. Ajouter LogParadeInputSimulator à la scène
2. Activer "Enable Simulation" dans l'inspecteur
3. Utiliser les contrôles clavier :
   - **A/D** : Bouger gauche/droite
   - **S** : Retour au centre
   - **Espace** : Mouvement automatique
   - **Boutons GUI** : Positionnement direct sur chaque voie

### 2. Calibration
- La calibration se lance automatiquement au démarrage (3 secondes)
- Le joueur doit se tenir au centre pendant la calibration
- Utiliser le bouton "Recalibrer" dans le debug GUI si nécessaire
- **En mode simulation** : La calibration se fait automatiquement au centre

### 3. Ajustements
- **Smoothing Factor** : Réduire si l'avatar réagit trop lentement
- **Lane Boundaries** : Ajuster selon l'amplitude des mouvements du joueur
- **Lane Width** : Modifier selon la taille de l'écran/projection
- **Simulation Range** : Ajuster pour les tests clavier

### 4. Debug
- Activer "Show Debug Info" dans les composants
- Utiliser le Toggle Debug dans l'UI pour voir les informations détaillées
- Observer les Gizmos dans la Scene View pour visualiser les voies
- Interface du simulateur pour tests rapides
- **Interface Debug du tracker** : Affiche la source utilisée ("Head (Pose)" ou "Hand (Fallback)")

### 5. Vérification de la source de tracking
Dans l'interface debug de `LogParadeLateralTracker`, vérifiez que :
- **Source : "Head (Pose)"** = Utilise correctement la position de la tête ✅
- **Source : "Hand (Fallback)"** = Utilise les mains (données de pose indisponibles)
- **Source : "No Data"** = Aucune donnée reçue (problème de connexion)

---

## 🎮 Contrôles et gameplay

### Contrôles physiques
- **Bouger à gauche** → Avatar vers la voie 1
- **Légèrement à gauche** → Avatar vers la voie 2  
- **Légèrement à droite** → Avatar vers la voie 3
- **Bouger à droite** → Avatar vers la voie 4

### Interface
- **Zone de voie actuelle** : Indique la voie où se trouve l'avatar (1-4)
- **Position du joueur** : Affiche les coordonnées de tracking
- **Barre de calibration** : Progrès de la calibration automatique

---

## 🔧 Dépannage

### Problèmes courants

1. **Avatar ne bouge pas**
   - Vérifier que UDPReceive reçoit des données (`printToConsole = true`)
   - S'assurer que le port correspond (5052)
   - Vérifier la calibration

2. **Mouvement trop sensible**
   - Augmenter le Smoothing Factor (0.9f)
   - Ajuster les Lane Boundaries
   - Recalibrer le système

3. **Avatar reste dans une voie**
   - Vérifier les boundaries (-1.5f / 1.5f)
   - S'assurer que la calibration est terminée
   - Regarder les données brutes dans le debug

4. **UI ne s'affiche pas**
   - Vérifier que tous les composants UI sont assignés dans LogParadeUIManager
   - S'assurer que le Canvas est bien configuré

### Logs utiles
- `Debug.Log` dans tous les scripts avec le flag `showDebugInfo`
- GUI on-screen dans LogParadeLateralTracker et LogParadePlayerAvatar
- Console Unity pour les erreurs de parsing JSON

---

## ✅ Checklist de validation

### Configuration minimale fonctionnelle
- [ ] Scène `MiniGame_LogParade.unity` créée
- [ ] `LogParadeGameController` présent et configuré
- [ ] `UDPReceive` configuré (port 5052)
- [ ] `LogParadeLateralTracker` configuré avec références
- [ ] `LogParadePlayerAvatar` avec modèle visuel
- [ ] Connexions entre composants établies

### Test de base (avec simulateur)
- [ ] `LogParadeInputSimulator` ajouté et activé
- [ ] Touches A/D déplacent l'avatar entre les voies
- [ ] Debug GUI affiche les informations
- [ ] Boutons GUI permettent le positionnement direct
- [ ] Calibration automatique fonctionne

### Test avec MediaPipe
- [ ] Tracker Python envoie des données (port 5052)
- [ ] Mouvement latéral physique contrôle l'avatar
- [ ] Calibration se lance au démarrage
- [ ] Suivi fluide sans tremblements excessifs

### Intégration avec MiniGameManager
- [ ] LogParade listé dans GameSessionManager.miniGames
- [ ] Scene Name = "MiniGame_LogParade"
- [ ] Transition depuis/vers autres mini-jeux fonctionne
- [ ] Héritage de MiniGameBase respecté

**Pour ajouter LogParade au GameSessionManager :**
1. Ouvrir la scène `MiniGameManager.unity`
2. Trouver le GameObject `GameSessionManager`
3. Dans l'inspecteur, ajouter un élément à `Mini Games`
4. Définir `Scene Name` = "MiniGame_LogParade"

---

## 🚀 Prochaines étapes (versions futures)

1. **Ajout des rondins** : GameObjects défilant de haut en bas
2. **Système de collision** : Détecter quand l'avatar "attrape" un rondin
3. **Système de score** : Points pour les rondins collectés
4. **Difficulté progressive** : Plus de rondins, vitesse croissante
5. **Effets visuels avancés** : Particules, animations, feedback visuel
6. **Sons et musique** : Ambiance forestière, sons de réussite/échec

---

## 💡 Notes techniques

- **Architecture** : Respecte les patterns des autres mini-jeux (MusicNotePress)
- **Événements** : Utilise des events C# pour découpler les composants
- **Performance** : Données lissées pour éviter les micro-tremblements
- **Extensibilité** : Structure modulaire pour ajouter facilement le gameplay des rondins
- **Debug** : Outils intégrés pour faciliter le développement et tests

---

Cette première version se concentre sur le tracking et l'avatar. Le système est prêt pour accueillir la logique de gameplay (rondins, scores, etc.) dans les versions suivantes.
