# Guide de Résolution des Erreurs de Build Unity

## 🚨 Erreurs Communes et Solutions

### 1. Erreur "UnityEditor.SerializedObject does not exist"

**Problème :** Scripts utilisant `UnityEditor` dans le build
**Solution :** Entourer le code d'éditeur avec `#if UNITY_EDITOR`

```csharp
// ❌ Problématique (cause l'erreur)
var so = new UnityEditor.SerializedObject(target);

// ✅ Correct (fonctionne dans le build)
#if UNITY_EDITOR
var so = new UnityEditor.SerializedObject(target);
#endif
```

### 2. Fichiers .asset corrompus

**Problème :** Fichiers YAML mal formatés
**Solution :** Supprimer les fichiers problématiques

```powershell
# Supprimer manuellement
Remove-Item "Assets\Models\...\Readme.asset" -Force
```

### 3. Scripts d'éditeur dans le build

**Problème :** Scripts contenant du code d'éditeur
**Solutions :**
1. Déplacer dans un dossier `Editor/`
2. Utiliser `#if UNITY_EDITOR`
3. Séparer en scripts distincts

### 4. Build Settings incorrects

**Vérifications :**
- Scènes ajoutées dans "Scenes In Build"
- Platform correcte sélectionnée
- Scripts de test exclus du build

## 🔧 Script de Build Automatique

Le script `build_game.ps1` gère automatiquement :
- ✅ Nettoyage des fichiers corrompus
- ✅ Vérification des scripts d'éditeur
- ✅ Copie des fichiers Python
- ✅ Création des scripts de lancement

## 📋 Checklist avant Build

1. **Vérifier la scène principale :**
   - [ ] `PythonIntegrationSetup` présent
   - [ ] Pas de scripts de test/debug

2. **Nettoyer le projet :**
   - [ ] Supprimer fichiers .asset corrompus
   - [ ] Vérifier les scripts d'éditeur
   - [ ] Assets inutiles supprimés

3. **Tester en mode Play :**
   - [ ] Écran de chargement s'affiche
   - [ ] Python se lance correctement
   - [ ] UDP fonctionne

4. **Build Settings :**
   - [ ] Scène principale dans "Scenes In Build"
   - [ ] Platform correcte
   - [ ] Configuration Release

## 🚀 Commandes de Build

```powershell
# Build complet automatique
.\build_game.ps1 -BuildPath "C:\MyGame\MotionParty" -UnityBuildPath "C:\UnityBuild"

# Seulement copie Python (si Unity déjà buildé)
.\build_game.ps1 -BuildPath "C:\MyGame\MotionParty"
```

## 🐛 Debug des Erreurs de Build

### 1. Erreurs de compilation
```
Error building Player because scripts had compiler errors
```
**Solution :** Corriger les erreurs dans Unity Console avant de builder

### 2. Erreurs d'assets
```
Unable to parse file ... Expect ':' between key and value
```
**Solution :** Supprimer les fichiers .asset corrompus

### 3. Erreurs de dépendances
```
Assembly 'Assembly-CSharp' will not be loaded due to errors
```
**Solution :** Vérifier les références et using statements

## 📝 Notes importantes

- **Toujours tester en Play mode** avant de builder
- **Les scripts d'éditeur** ne doivent jamais être inclus dans le build
- **Le dossier python-tracker** doit être copié manuellement après le build Unity
- **L'écran de chargement** gère automatiquement l'initialisation Python
