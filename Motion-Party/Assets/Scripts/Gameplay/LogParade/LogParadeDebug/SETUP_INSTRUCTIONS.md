# Instructions de Setup pour LogParadeDebugManager

## 🚀 Setup Rapide (Recommandé)

### 1. Créer la structure de base
1. Dans la scène LogParade, créer un GameObject vide : `LogParadeDebugManager`
2. Ajouter le script `LogParadeDebugManager.cs`
3. Configurer les paramètres de base :
   - `debugEnabledAtStart` : false (pour production)
   - `autoFindPanels` : true
   - `searchInEntireScene` : true

### 2. Ajouter les panneaux (Auto-découverte)
1. Créer des GameObjects enfants de `LogParadeDebugManager` :
   - `DebugPanel_Score`
   - `DebugPanel_Status` 
   - `DebugPanel_Calibration`

2. Ajouter les scripts correspondants à chaque GameObject

### 3. Configuration des positions
Exemples de configuration pour éviter les chevauchements :

**DebugPanel_Score :**
- anchor : (10, 10)
- width : 200

**DebugPanel_Status :**
- anchor : (220, 10) 
- width : 250

**DebugPanel_Calibration :**
- anchor : (-220, 10) (depuis le bord droit)
- width : 200

## 🎯 Setup Manuel (Contrôle total)

### 1. Structure hiérarchique
```
LogParadeDebugManager
├── DebugPanel_Score
├── DebugPanel_Status
└── DebugPanel_Calibration
```

### 2. Configuration individuelle
Pour chaque panneau, configurer :
- Position (anchor + offset)
- Taille (width + height)
- Refresh rate selon les besoins
- Auto-find des références

### 3. Références manuelles (optionnel)
Si auto-find ne fonctionne pas, assigner manuellement :
- `DebugPanel_Score` → scoreManager
- `DebugPanel_Status` → gameController, gameTimer, etc.
- `DebugPanel_Calibration` → calibrationManager

## ⚡ Test rapide

1. **Lancer la scène**
2. **F1** → Vérifier que les panneaux apparaissent
3. **F1** → Vérifier qu'ils disparaissent
4. **Modifier positions** dans l'inspecteur → Voir ajustement temps réel

## 🔧 Dépannage

### Panneaux vides ou "non trouvé"
- Vérifier que les managers LogParade sont présents dans la scène
- Utiliser les boutons "Rechercher" dans chaque panneau
- Activer les logs de debug pour diagnostic

### Performance
- Augmenter `refreshRate` si trop de lag (ex: 0.5 au lieu de 0.1)
- Désactiver panneaux non nécessaires

### Positionnement
- Utiliser des valeurs négatives pour ancrer depuis les bords opposés
- Tester sur différentes résolutions d'écran

## 🚀 Nouveau : Système de Lancement Coordonné

**Problème résolu** : Après calibration, le timer démarrait mais aucun rondin n'apparaissait.

**Solution** : Nouveau système `LogParadeGameLauncher` qui coordonne tous les composants.

### Installation du GameLauncher

1. **Création du GameObject** :
   ```
   - Clic droit dans la Hiérarchie → Create Empty
   - Nommer : "LogParade_GameLauncher"
   - Ajouter le script : LogParadeGameLauncher
   ```

2. **Configuration (Automatique recommandée)** :
   ```
   ✅ Auto Find Components = true
   ✅ Enable Detailed Logs = true
   Delay After Calibration = 1.5
   ```

3. **Configuration Manuelle (si nécessaire)** :
   - Game Controller → Votre LogParadeGameController
   - Game Timer → Votre LogParadeGameTimer  
   - Log Generator → Votre LogParadeLogGenerator
   - Score Manager → Votre LogParadeScoreManager
   - Calibration Manager → Votre LogParadeCalibrationManager

### Vérification du Bon Fonctionnement

1. **Lancement du jeu** et calibration complète
2. **F1** pour ouvrir le debug → Nouveau panneau "🚀 Game Launcher"
3. **Vérification** :
   - Systèmes détectés : **4/4** ✅
   - Tous les systèmes avec **✅** verts
   - Jeu : **DÉMARRÉ** ✅

4. **Test visuel** : Les rondins doivent maintenant défiler après la calibration !
