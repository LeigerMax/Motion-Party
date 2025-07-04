# Documentation technique - HandTracker

## Architecture du module

### 1. HandTracker.cs
- **Responsabilité** : Composant principal qui orchestre le suivi de la main
- **Dépendances** : UDPReceive, FireflyDanceConfig
- **Cycle de vie** : Start() → Update() → OnDestroy()

### 2. HandData.cs
- **Structure** : Données immutables de la main (position, geste, doigts ouverts)
- **Usage** : Transfert de données entre composants

### 3. HandPositionConverter.cs
- **Responsabilité** : Conversion des coordonnées caméra Python (640x480) vers monde Unity
- **Algorithme** : Normalisation puis mapping linéaire sur la zone de jeu

### 4. HandEvents.cs
- **Pattern** : Système d'événements globaux statiques
- **Avantage** : Découplage des systèmes, pas de référence directe nécessaire

## Format des données UDP

```json
{
    "hand_positions": [
        [x, y, z], // Point 0 - wrist
        [x, y, z], // Point 1 - thumb_cmc
        // ... 21 points au total
        [x, y, z]  // Point 20 - pinky_tip
    ],
    "gesture": "hand_open", // ou "hand_close", "index_up"
    "open_fingers": 5       // 0-5 doigts ouverts
}
```

## Points de repère MediaPipe (main)

- **0** : Wrist (poignet)
- **4** : Thumb tip (bout du pouce)
- **8** : Index tip (bout de l'index) - **UTILISÉ PAR DÉFAUT**
- **12** : Middle tip (bout du majeur)
- **16** : Ring tip (bout de l'annulaire)
- **20** : Pinky tip (bout de l'auriculaire)

## Paramètres de configuration

### handLandmarkIndex
- **Défaut** : 8 (index tip)
- **Plage** : 0-20
- **Usage** : Sélectionner le point de la main à suivre

### enableSmoothing
- **Défaut** : true
- **Usage** : Activer le lissage de position pour réduire les tremblements

### smoothingFactor
- **Défaut** : 0.8
- **Plage** : 0.0-1.0
- **Calcul** : `newPos = Lerp(oldPos, targetPos, 1 - smoothingFactor)`

## Événements disponibles

### Événements locaux (HandTracker)
- `OnHandDataUpdated` : Données complètes mises à jour
- `OnHandPositionChanged` : Position changée

### Événements globaux (HandEvents)
- `OnHandDetected` : Main détectée pour la première fois
- `OnHandLost` : Main perdue
- `OnGestureChanged` : Geste changé
- `OnHandDataUpdated` : Données mises à jour
- `OnHandPositionChanged` : Position changée

## Optimisations

1. **Parsing JSON** : Utilisation de Newtonsoft.Json pour la performance
2. **Lissage adaptatif** : Réduction des tremblements sans lag
3. **Événements conditionnels** : Déclenchement uniquement sur changement
4. **Validation continue** : OnValidate() pour maintenir la cohérence
5. **Gestion d'erreurs** : Try-catch pour éviter les crashs sur données corrompues

## Intégration avec FireflyDanceConfig

- **ClampToBounds()** : Contrainte automatique dans la zone de jeu
- **Mapping des coordonnées** : Conversion précise caméra → monde Unity
- **Validation** : Vérification des limites en temps réel

## Debugging

### Logs disponibles
- Initialisation du tracker
- Erreurs de parsing UDP
- Changements de geste
- Détection/perte de main

### Gizmos (Scene view)
- Zone de jeu (rectangle jaune)
- Position de la main (sphère verte)
- Rayon de capture (cercle cyan)

## Performance

- **Fréquence** : 30-60 FPS selon la source UDP
- **Latence** : <50ms de la caméra à l'affichage
- **Mémoire** : ~1KB par frame de données
- **CPU** : <1% sur processeur moderne
