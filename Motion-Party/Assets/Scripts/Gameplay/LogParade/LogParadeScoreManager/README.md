# 📦 LogParadeScoreManager
Gère les points du joueur en fonction de sa position sur les rondins. Intègre un système de gain et pénalité adapté au public senior.

# 🔧 Fonctionnalités principales
- Gain progressif de score si sur un rondin (+1 point/seconde)
- Pénalité ponctuelle si le joueur tombe à l'eau (-5 points)
- Protection contre les pénalités répétées tant que le joueur ne remonte pas
- Détection basée sur le module PlayerOnLogChecker
- Affichage en temps réel du score (si UIManager connecté)

# 🏗️ Structure du dossier
```
/LogParadeScoreManager/
├── LogParadeScoreManager.cs
├── TestLogParadeScoreManager.cs
└── README.md
```

# 🧪 Instructions pour tester
## Étapes :
1. **Crée une scène avec :**
   - Un PlayerOnLogChecker fonctionnel sur l'avatar
   - Des rondins détectables (tag "Log", trigger, etc.)
   - Le script LogParadeScoreManager attaché à un GameObject (ex : le GameController)
   - (Optionnel) Le script TestLogParadeScoreManager pour tester facilement

2. **Lance la scène :**
   - Le score doit augmenter régulièrement tant que le joueur reste sur un rondin
   - Si le joueur tombe, il perd 5 points une seule fois
   - Il regagne le droit à la pénalité en remontant puis retombant
   - Les logs doivent indiquer les événements

3. **Test avec TestLogParadeScoreManager :**
   - Touche 1 : Démarrer le système de score
   - Touche 2 : Arrêter le système de score
   - Touche 3 : Reset le score
   - Touche 4 : Ajouter 10 points bonus
   - Touche 5 : Afficher le score actuel dans la console

## Comportement attendu :
- Pas de perte de points lors d'un changement rapide de voie
- Le système est tolérant, stable, fluide
- Le score ne peut jamais devenir négatif
- Les logs indiquent clairement les événements (gains, pénalités, transitions)

# 🚀 Instructions pour utiliser / intégrer
1. **Attacher le script LogParadeScoreManager à un GameObject central** (ex : GameController)

2. **Assigner :**
   - La référence au PlayerOnLogChecker
   - (Optionnel) Un champ texte UI pour afficher le score

3. **Le module commence à compter dès l'appel de StartScoring()** (sinon, le faire manuellement)

4. **Interrogation possible du score via :** `GetCurrentScore()`

# 👤 Auteurs / liens utiles
- Inspiré par des mécaniques classiques de jeux de rythme
- Conçu pour un public senior : gameplay lent, feedback doux
