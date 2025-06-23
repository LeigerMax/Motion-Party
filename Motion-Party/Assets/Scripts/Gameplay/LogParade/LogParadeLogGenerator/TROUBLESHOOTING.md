# 🔧 Guide de Dépannage - LogParadeLogGenerator

## ❌ Problème : Aucun rondin n'apparaît à l'écran

### ✅ **Étape 1 : Vérification de la Console Unity**

1. **Ouvrez la Console Unity** : `Window > General > Console`
2. **Lancez le jeu** en mode Play
3. **Cherchez les messages** `[LogParadeLogGenerator]` dans la console

#### 🔍 Messages attendus :
```
[LogParadeLogGenerator] Start() appelé
[LogParadeLogGenerator] Démarrage automatique pour test  
[LogParadeLogGenerator] Launch() appelé
[LogParadeLogGenerator] Initialisation du générateur...
[LogParadeLogGenerator] Voie 1 assignée : [nom_voie]
[LogParadeLogGenerator] Voie 2 assignée : [nom_voie]
[LogParadeLogGenerator] Voie 3 assignée : [nom_voie]
[LogParadeLogGenerator] Voie 4 assignée : [nom_voie]
[LogParadeLogGenerator] Prefab 1 assigné : [nom_prefab]
[LogParadeLogGenerator] Prefab 2 assigné : [nom_prefab]
[LogParadeLogGenerator] Prefab 3 assigné : [nom_prefab]
[LogParadeLogGenerator] Générateur initialisé avec succès
[LogParadeLogGenerator] StartGeneration() appelé
[LogParadeLogGenerator] Démarrage de la coroutine de génération
[LogParadeLogGenerator] Boucle de génération démarrée
[LogParadeLogGenerator] Génération d'une nouvelle rangée...
```

### ✅ **Étape 2 : Cas d'erreurs fréquentes**

#### 🚨 **Si vous voyez : "Exactement 4 voies sont requises !"**
- **Problème** : Le champ `Lanes` n'est pas correctement configuré
- **Solution** : 
  1. Sélectionnez le GameObject avec LogParadeLogGenerator
  2. Dans l'inspecteur, vérifiez que le tableau `Lanes` a exactement 4 éléments
  3. Créez 4 GameObjects vides dans la scène si besoin
  4. Assignez-les dans le tableau `Lanes`

#### 🚨 **Si vous voyez : "Exactement 3 prefabs de rondins sont requis !"**
- **Problème** : Le champ `Log Prefabs` n'est pas correctement configuré
- **Solution** : Voir **Étape 3** ci-dessous

#### 🚨 **Si vous voyez : "La voie X n'est pas assignée !"**
- **Problème** : Une des voies dans le tableau est `None (Transform)`
- **Solution** : Assignez un GameObject à chaque slot du tableau `Lanes`

#### 🚨 **Si vous voyez : "Le prefab de rondin X n'est pas assigné !"**
- **Problème** : Un des prefabs dans le tableau est `None (GameObject)`
- **Solution** : Voir **Étape 3** ci-dessous

#### 🚨 **Si vous voyez : "Génération désactivée (enableGeneration = false)"**
- **Problème** : La case `Enable Generation` est décochée
- **Solution** : Cochez la case `Enable Generation` dans l'inspecteur

#### 🚨 **Si vous voyez : "Tag: LogParadeLog is not defined"**
- **Problème** : Le tag personnalisé n'existe pas dans Unity
- **Solution** : **CORRIGÉ AUTOMATIQUEMENT** - Le script n'utilise plus de tag personnalisé

### ✅ **Étape 3 : Création rapide de prefabs de test**

Si vous n'avez pas encore créé les 3 prefabs requis :

#### � **Méthode ULTRA-RAPIDE : Setup automatique complet**
1. **Ajoutez le script** `LogParadeQuickSetup` sur le même GameObject que LogParadeLogGenerator
2. **Clic droit** sur le composant LogParadeQuickSetup > "Configuration Complète Auto"
3. **OU appuyez** sur `Ctrl + S` en mode Play
4. **C'est tout !** Les 4 voies et 3 prefabs sont créés et assignés automatiquement

#### �📦 **Méthode 1 : Utiliser le script de test**
1. **Ajoutez le script** `LogParadeTestPrefabCreator` sur le même GameObject
2. **Dans l'inspecteur**, assignez le `LogParadeLogGenerator` dans le champ `Log Generator`
3. **Cliquez** sur le bouton "Créer Prefabs de Test" (menu contextuel)
4. **OU appuyez** sur `Ctrl + P` pendant le jeu

#### 📦 **Méthode 2 : Création manuelle rapide**
1. **Créez 3 GameObjects vides** dans la scène
2. **Nommez-les** : `LogPrefab_Short`, `LogPrefab_Medium`, `LogPrefab_Long`
3. **Pour chaque GameObject** :
   - Ajoutez un `Cube` enfant (Create > 3D Object > Cube)
   - Ajoutez un `BoxCollider` sur le parent
   - Changez la taille du cube (Short: 1x1x1, Medium: 1x1x1.5, Long: 1x1x2)
4. **Glissez-les** dans le dossier Project pour en faire des prefabs
5. **Assignez-les** dans le champ `Log Prefabs` du LogParadeLogGenerator

### ✅ **Étape 4 : Configuration finale**

Une fois les prefabs créés :

1. **Sélectionnez** le GameObject avec LogParadeLogGenerator
2. **Dans l'inspecteur**, vérifiez :
   - ✅ `Lanes` : 4 éléments assignés
   - ✅ `Log Prefabs` : 3 éléments assignés
   - ✅ `Enable Generation` : Coché
   - ✅ `Log Speed` : 1.2
   - ✅ `Generation Interval` : 2.5
   - ✅ `Spawn Height` : 10
3. **Activez** `Show Debug Info` pour voir les gizmos dans la Scene View

### ✅ **Étape 5 : Test final**

1. **Lancez le jeu** en mode Play
2. **Attendez 2-3 secondes** (le premier rondin apparaît après `generationInterval`)
3. **Regardez au-dessus de la scène** (Y = 10) pour voir les rondins apparaître
4. **Vérifiez la console** pour les messages de debug

### 🔍 **Étape 6 : Diagnostic avancé**

Si les rondins n'apparaissent toujours pas :

#### 📹 **Vérification de la caméra**
- Orientez votre caméra Scene View vers Y = 10
- Les rondins apparaissent peut-être hors du champ de vision

#### 📐 **Vérification des positions**
- Activez `Show Debug Info`
- Vous devriez voir des lignes jaunes (voies) et vertes/rouges (spawn/destroy)
- Les rondins apparaissent sur les lignes jaunes

#### 🚀 **Test manuel immédiat**
Le LogParadeLogGenerator inclut maintenant des méthodes de test :

1. **Clic droit** sur le composant LogParadeLogGenerator dans l'inspecteur
2. **Sélectionnez** "Test Spawn Immédiat" pour créer des rondins sur toutes les voies
3. **OU sélectionnez** "Forcer Génération Rangée" pour une génération normale

#### 🛠️ **Méthodes de test rapide disponibles :**
- `Test Spawn Immédiat` : Crée un rondin sur chaque voie instantanément
- `Forcer Génération Rangée` : Force une génération normale avec validation de chemin

### 📞 **Support**

Si le problème persiste, communiquez :
1. **Les messages de la console** (copier/coller)
2. **Configuration de l'inspecteur** (capture d'écran)
3. **Structure de la scène** (GameObjects présents)

---

*Guide de dépannage pour LogParadeLogGenerator v1.0*
