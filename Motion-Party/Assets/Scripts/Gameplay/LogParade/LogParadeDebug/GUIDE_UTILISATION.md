# Guide d'utilisation rapide - Debug UI LogParade

## 🚀 Démarrage rapide

### 1. Activer le debug UI
```
Appuyez sur F1 pour ouvrir/fermer tous les panneaux
```

### 2. Tester le système de score
```
1. Ouvrez le panneau "Score"
2. Utilisez les boutons +1, +10, +50 pour ajouter des points
3. Utilisez les boutons -1, -5, -10 pour retirer des points
4. Observez les logs dans la console Unity
```

### 3. Contrôler le lancement du jeu
```
1. Ouvrez le panneau "🚀 Game Launcher"
2. Vérifiez que tous les composants sont ✅
3. Cliquez "LANCER JEU" pour démarrer tous les systèmes
4. Utilisez "DIAGNOSTIC" pour vérifier l'état
```

## 🎮 Raccourcis clavier

| Touche | Action |
|--------|--------|
| `F1` | Toggle Debug UI (ouvrir/fermer) |
| `T` | Tests de validation complets |
| `P` | Test rapide du système de score |
| `L` | Test rapide du GameLauncher |

## 🔧 Résolution de problèmes

### Score non modifié
```
✅ RÉSOLU : Les méthodes AddPointsManual/SubtractPointsManual 
   sont maintenant correctement détectées
✅ Les logs affichent maintenant le score actuel
✅ Fallbacks multiples en cas de méthode manquante
```

### GameLauncher ne fonctionne pas
```
1. Vérifiez que LogParadeGameLauncher est dans la scène
2. Utilisez le bouton "DIAGNOSTIC" pour voir les composants manquants
3. Vérifiez les logs Unity pour les erreurs
```

### Panneaux invisibles
```
1. Appuyez sur F1 pour activer le debug
2. Vérifiez que debugEnabledAtStart = true dans LogParadeDebugManager
3. Vérifiez que les Canvas sont bien activés
```

## 📋 Tests recommandés

### Test complet du système
```
1. Ajoutez LogParadeDebugTestValidator à votre scène
2. Activez "runTestsOnStart" dans l'Inspector
3. Lancez la scène et observez les logs
4. Ou appuyez sur T pour tester manuellement
```

### Test du score en direct
```
1. F1 pour ouvrir le debug UI
2. Panel Score : +10 points
3. Vérifiez le log : "✅ 10 points ajoutés - Score: X"
4. Panel Score : -5 points  
5. Vérifiez le log : "✅ 5 points retirés - Score: Y"
```

### Test du GameLauncher
```
1. F1 pour ouvrir le debug UI
2. Panel GameLauncher : vérifier les composants ✅
3. Cliquer "LANCER JEU" 
4. Observer les logs de séquence de démarrage
5. Utiliser "DIAGNOSTIC" pour valider l'état
```

## ✅ Validation du système

Tous ces éléments doivent fonctionner :
- [ ] F1 ouvre/ferme les panneaux
- [ ] Panneau Score : boutons +1, +10, +50 ajoutent des points
- [ ] Panneau Score : boutons -1, -5, -10 retirent des points
- [ ] Logs Score : "✅ X points ajoutés - Score: Y"
- [ ] Panneau GameLauncher : affichage des composants ✅/❌
- [ ] Panneau GameLauncher : bouton "LANCER JEU" démarre le jeu
- [ ] Panneau GameLauncher : bouton "DIAGNOSTIC" affiche l'état
- [ ] Tests validation (T, P, L) fonctionnent

## 🎯 Le système est maintenant complet !

- ✅ **Panneau Score** : Méthodes AddPointsManual/SubtractPointsManual fonctionnelles
- ✅ **Panneau GameLauncher** : Contrôle coordonné des systèmes
- ✅ **Tests de validation** : Script automatique pour vérifier le bon fonctionnement
- ✅ **Documentation complète** : Guides d'installation et d'utilisation
- ✅ **Pas d'erreurs de compilation** : Code propre et testé

🎉 **Prêt pour les tests utilisateur !**
