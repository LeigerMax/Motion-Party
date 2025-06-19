## 🎯 Amélioration de l'accès aux lanes extrêmes (1 et 4)

### 🚨 **Problème identifié**
Les lanes 1 (gauche) et 4 (droite) n'étaient pas accessibles car le joueur ne pouvait pas se déplacer assez loin latéralement devant la caméra.

### 🔧 **Solutions implémentées**

#### 1. **Nouveau mapping adaptatif**
- ✅ **Suppression des bornes fixes** : Plus de `leftBoundary` et `rightBoundary` statiques
- ✅ **Utilisation de la largeur effective** : Le système utilise `effectiveTrackingWidth` détectée pendant la calibration
- ✅ **Seuils réduits** : Passage de ±0.5 à ±0.4 pour faciliter l'accès aux extrêmes

#### 2. **Paramètre de sensibilité des lanes extrêmes**
```csharp
[Tooltip("Facteur d'amplification pour atteindre les lanes extrêmes (1 et 4)")]
[Range(0.5f, 2.0f)]
public float extremeLanesSensitivity = 1.2f;
```
- **Défaut : 1.2x** → Amplifie les mouvements de 20%
- **Ajustable** selon le public (plus élevé pour seniors)

#### 3. **Calibration étendue**
- ✅ **Nouvelle méthode** : `RecalibrateForExtremeLanes()`
- ✅ **Durée prolongée** : 5 secondes au lieu de 3
- ✅ **Instructions claires** : "Bougez de GAUCHE à DROITE pendant la calibration"
- ✅ **Bouton UI** : Accessible depuis l'interface utilisateur

#### 4. **Presets de caméra optimisés**
- **640x480** : `extremeLanesSensitivity = 1.3f` (plus de sensibilité)
- **1280x720** : `extremeLanesSensitivity = 1.2f` 
- **1920x1080** : `extremeLanesSensitivity = 1.1f` (moins nécessaire)

### 🎮 **Nouvelle logique de mapping**

#### Avant (problématique) :
```
Position < -0.75  → Lane 1 (difficilement atteignable)
Position < 0      → Lane 2  
Position < 0.75   → Lane 3
Position >= 0.75  → Lane 4 (difficilement atteignable)
```

#### Après (optimisé) :
```
Position < -0.4   → Lane 1 (plus facile ✅)
Position < 0      → Lane 2  
Position < 0.4    → Lane 3
Position >= 0.4   → Lane 4 (plus facile ✅)
```

### 🛠️ **Interface utilisateur**

#### Debug GUI :
- ✅ **Bouton "🔄 Recalibrer"** : Calibration standard (3s)
- ✅ **Bouton "🎯 Calibration étendue"** : Pour les lanes extrêmes (5s)
- ✅ **Affichage sensibilité** : `Sensibilité extrêmes: 1.2x`

#### UI Manager :
- ✅ **Bouton recalibrateButton** : Calibration rapide
- ✅ **Bouton extendedCalibrateButton** : Calibration étendue avec instructions

### 📋 **Instructions pour l'utilisateur**

#### Pour une calibration normale :
1. Cliquer "🔄 Recalibrer"
2. Se placer au **centre** et rester immobile (3s)
3. Le système détecte la position de base

#### Pour accéder aux lanes extrêmes :
1. Cliquer "🎯 Calibration étendue" 
2. **Bouger de gauche à droite** pendant 5 secondes
3. Le système détecte l'amplitude maximale de mouvement
4. Les lanes 1 et 4 deviennent accessibles

### ⚙️ **Paramètres recommandés**

#### Public senior (mouvements limités) :
```csharp
extremeLanesSensitivity = 1.5f;  // Amplification forte
centralDeadZone = 50f;           // Zone morte plus large
```

#### Public standard :
```csharp
extremeLanesSensitivity = 1.2f;  // Amplification modérée
centralDeadZone = 30f;           // Zone morte normale
```

#### Public gaming (mouvements amples) :
```csharp
extremeLanesSensitivity = 1.0f;  // Pas d'amplification
centralDeadZone = 20f;           // Zone morte réduite
```

### 🧪 **Test recommandé**
1. Lancer le jeu avec debug activé
2. Faire une calibration étendue
3. Tester l'accès aux 4 lanes en bougeant progressivement
4. Ajuster `extremeLanesSensitivity` si nécessaire
5. Vérifier que les changements sont fluides et contrôlables

---

*Améliorations v1.2.1 - Accès optimisé aux lanes extrêmes*
