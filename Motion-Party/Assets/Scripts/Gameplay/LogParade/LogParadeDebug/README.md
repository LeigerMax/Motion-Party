# Système de Debug UI personnalisable pour LogParade

## 🎯 Objectif
Système d'affichage de debug lisible, désactivable, et déplaçable librement pour faciliter les tests de LogParade.

## ✨ NOUVEAUTÉS RÉCENTES (Mise à jour)

### 🎯 Panneau Score amélioré
- ✅ **Problème résolu** : Méthodes AddPointsManual/SubtractPointsManual maintenant détectées
- ✅ Logs détaillés avec affichage du score actuel après chaque opération
- ✅ Boutons de test améliorés : +1, +10, +50, -1, -5, -10
- ✅ Contrôles avancés : Reset, Score x2, Toggle scoring
- ✅ Méthode séparée SubtractTestScore() pour plus de clarté

### 🚀 Panneau GameLauncher
- ✅ **Déjà fonctionnel** : Contrôle du lancement coordonné des systèmes
- ✅ Statut en temps réel (Lancement/Démarré/Prêt)
- ✅ Diagnostic complet des composants (✅/❌)
- ✅ Boutons de contrôle : Lancer, Arrêter, Relancer, Diagnostic
- ✅ Arrêt d'urgence pour situations critiques

### 🧪 Script de validation
- ✅ **Nouveau** : LogParadeDebugTestValidator
- ✅ Tests automatiques des fonctionnalités clés
- ✅ Raccourcis clavier : T (tests complets), P (score), L (launcher)
- ✅ Validation en temps réel des méthodes de score

## ✅ Fonctionnalités

### Activation / Désactivation
- **Touche F1** : Active ou masque tous les panneaux de debug LogParade
- **État persistant** : L'état (activé/désactivé) est maintenu pendant la session
- **Mode production** : Tous les panneaux sont désactivés par défaut (sauf si `debugEnabledAtStart = true`)

### Affichage personnalisable
- **Canvas indépendants** : Chaque panneau est dans son propre système UI
- **Position customisable** : Anchor et Offset configurables dans l'inspecteur Unity
- **Taille adaptable** : Largeur et hauteur personnalisables
- **Refresh rate** : Taux de rafraîchissement configurable pour les performances

### Modularité
- **Panneaux indépendants** : Chaque panneau peut être activé/désactivé individuellement
- **Auto-découverte** : Le système trouve automatiquement les panneaux dans la scène
- **Extensible** : Facilité d'ajout de nouveaux panneaux

## 🏗️ Structure

```
/LogParadeDebug/
├── LogParadeDebugManager.cs         ← Gestionnaire principal (écoute F1 + toggle global)
├── BaseDebugPanel.cs               ← Classe de base pour tous les panneaux
├── DebugPanel_Score.cs             ← Panneau de score et points
├── DebugPanel_Status.cs            ← Panneau général (lane, IsOnLog, timer, etc.)
├── DebugPanel_Calibration.cs       ← État de calibration
└── README.md                       ← Ce fichier
```

## 📊 Contenus des panneaux

### DebugPanel_Score
- **Score actuel** : Valeur en temps réel
- **État du scoring** : Actif/Inactif
- **Points par seconde** : Configuration
- **Pénalités** : Points perdus
- **Taux actuel** : Calcul en temps réel
- **Boutons de test** : +10, -5, Reset

### DebugPanel_Status
- **IsOnLog** : État actuel (true/false) ✅
- **Lane actuelle** : Voie où se trouve le joueur ✅
- **Timer** : Temps restant avec barre de progression ✅
- **Rondins actifs** : Nombre de rondins dans la scène ✅
- **État du jeu** : Démarré/En attente
- **Mode debug** : Actif/Inactif ✅
- **Position joueur** : Coordonnées X,Z

### DebugPanel_Calibration
- **Statut de calibration** : Terminée/En cours/Non calibré ✅
- **Étape actuelle** : Progression de la calibration
- **Temps de calibration** : Durée
- **Contrôles** : Démarrer, Arrêter, Reset, Bypass

## 🧪 Instructions d'utilisation

### Installation
1. **Copier les scripts** dans `Assets/Scripts/Gameplay/LogParade/LogParadeDebug/`
2. **Créer un GameObject** vide nommé "LogParadeDebugManager"
3. **Ajouter le script** `LogParadeDebugManager.cs` au GameObject
4. **Créer des GameObjects enfants** pour chaque panneau (optionnel, l'auto-découverte fonctionne)

### Configuration rapide
1. **LogParadeDebugManager** :
   - `debugEnabledAtStart` : false (pour production)
   - `autoFindPanels` : true
   - `searchInEntireScene` : true si panneaux dispersés

2. **Panneaux individuels** :
   - `anchor` : Position de base (ex: (10, 10) pour haut-gauche, (-220, 10) pour haut-droite)
   - `offset` : Décalage supplémentaire
   - `width` : Largeur du panneau
   - `refreshRate` : 0.1 seconde recommandée

### Test
1. **Lancer la scène** avec LogParade
2. **Appuyer sur F1** : Tous les panneaux apparaissent
3. **Re-appuyer sur F1** : Tous les panneaux disparaissent
4. **Modifier positions** dans l'inspecteur : Les panneaux s'ajustent en temps réel

## ⚙️ Configuration avancée

### Positionnement
```csharp
// Exemples de positions
anchor = new Vector2(10, 10);      // Haut-gauche
anchor = new Vector2(-220, 10);    // Haut-droite (220px depuis le bord)
anchor = new Vector2(10, -100);    // Bas-gauche (100px depuis le bas)
anchor = new Vector2(-220, -100);  // Bas-droite
```

### Auto-découverte
- **Dans les enfants** : `searchInEntireScene = false`
- **Dans toute la scène** : `searchInEntireScene = true`
- **Manuel** : `autoFindPanels = false` et assigner manuellement

### Références automatiques
Les panneaux recherchent automatiquement leurs composants cibles :
- `DebugPanel_Score` → `LogParadeScoreManager`
- `DebugPanel_Status` → `LogParadeGameController`, `LogParadeGameTimer`, etc.
- `DebugPanel_Calibration` → `LogParadeCalibrationManager`

## 🔧 Extensions possibles

### Nouveau panneau personnalisé
```csharp
public class DebugPanel_MonNouveau : BaseDebugPanel
{
    protected override void RefreshData()
    {
        // Rafraîchir les données
    }
    
    protected override void DrawPanelContent()
    {
        GUILayout.Label("Mon contenu personnalisé");
        // Votre interface
    }
}
```

### ScriptableObject centralisé
Créer un `LogParadeDebugSettings.asset` pour centraliser :
- Positions des panneaux
- États par défaut
- Couleurs et styles
- Touches de raccourci

## ✅ Bonnes pratiques respectées

- **Désactivés par défaut** : Mode production sûr
- **Scripts commentés** : Documentation complète
- **Champs publics nommés** : Interface claire dans l'inspecteur
- **Aucune dépendance externe** : Système autonome
- **Performance optimisée** : Refresh rate configurable, Update désactivé si invisible

## 🐛 Dépannage

### Panneaux non trouvés
- Vérifier `autoFindPanels = true`
- Vérifier `searchInEntireScene = true` si nécessaire
- Cliquer "Redécouvrir panneaux" dans le Debug Manager

### Données non affichées
- Vérifier que les composants cibles existent dans la scène
- Regarder la console pour les messages d'auto-découverte
- Utiliser les boutons "Rechercher" dans les panneaux individuels

### F1 ne fonctionne pas
- Vérifier que `LogParadeDebugManager` est actif dans la scène
- Changer `toggleKey` si F1 est utilisé ailleurs
- Tester avec `SetDebugEnabled(true)` via script

## 🧪 Tests et Validation des Corrections

### Tests des Bugs Corrigés

#### 1. Test du Statut de Calibration
```
1. Lancez le jeu
2. Appuyez sur F1 pour ouvrir le debug
3. Dans le panneau "Calibration" → vérifiez que le statut indique "NON CALIBRÉ"
4. Effectuez la calibration interactive complète
5. ✅ Le statut devrait maintenant afficher "CALIBRÉ" (au lieu de rester "NON CALIBRÉ")
```

#### 2. Test du Système de Score
```
1. Avec le debug ouvert (F1)
2. Effectuez la calibration puis démarrez une partie
3. Dans le panneau "Score Debug" :
   - ✅ Le "Score actuel" devrait s'afficher et augmenter pendant le jeu
   - ✅ L'état "Scoring" devrait passer à "ACTIF" 
   - ✅ Les "Points/sec" devraient être visibles (ex: 1)
4. Testez les boutons "+10" et "-5" pour vérifier l'interactivité
```

#### 3. Test du Comptage des Rondins
```
1. Avec le debug ouvert (F1)
2. Pendant une partie active
3. Dans le panneau "Statut Général" → section "🪵 RONDINS" :
   - ✅ "Actifs" devrait afficher un nombre > 0 (ex: 8, 12, 15...)
   - ✅ Le nombre devrait changer dynamiquement au cours de la partie
   - ✅ Utilisez le bouton "Recompter" pour rafraîchir
```

### Tests Supplémentaires Recommandés

#### Test d'Intégrité Générale
1. **Ouverture/Fermeture** : F1 doit ouvrir/fermer le debug sans erreur
2. **Déplacement des Panneaux** : Glisser-déposer les barres de titre
3. **Redimensionnement** : Les panneaux doivent s'adapter au contenu
4. **Performance** : Aucun lag visible avec le debug ouvert

#### Test en Conditions Réelles
1. **Partie Complète** : Jouer une partie entière avec le debug ouvert
2. **Transitions d'État** : Vérifier les changements lors des transitions de gameplay
3. **Stabilité** : Le debug ne doit pas planter ni interférer avec le jeu

### Résolution de Problèmes

Si les corrections ne fonctionnent pas :

1. **Vérifiez les logs Unity** pour des erreurs de compilation
2. **Redémarrez Unity** après avoir appliqué les corrections
3. **Consultez `BUGFIXES.md`** pour les détails techniques
4. **Activez les logs détaillés** dans les composants debug si besoin

---

## 📝 Notes de développement

Le système utilise la réflexion pour accéder aux champs privés des composants LogParade existants, permettant l'affichage sans modifier le code source original. Cela garantit une intégration non-intrusive et facilite la maintenance.
