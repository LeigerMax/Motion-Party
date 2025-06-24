# 📦 LogParadeCalibrationInteractive

Système de calibration interactive avant le début du jeu. Le joueur doit se déplacer sur la lane 1, puis la lane 4 pour valider la calibration.

## 🔧 Fonctionnalités principales
- Demande de placement successif sur lane 1 puis lane 4
- UI guidée affichée à l'écran
- Détection de présence sur la bonne lane
- Timeout automatique et relance si inactivité
- Rondins fixes visibles pour aider le déplacement

## 🏗️ Structure du dossier
```
/LogParadeCalibrationInteractive/
├── LogParadeCalibrationInteractive.cs
├── CalibrationTextUI.cs (ou intégré)
└── README.md
```

## 🧪 Instructions pour tester
- Ajouter le script sur un GameObject de scène
- Assigner un prefab de rondin fixe à utiliser sur les lanes
- Assigner les lanes manuelles dans l'inspecteur
- Vérifier que les instructions s'affichent et se mettent à jour selon la position du joueur

## 🚀 Instructions d'intégration
- Ajouter `LogParadeCalibrationInteractive.cs` dans une scène avant le jeu
- Assigner :
   - Le prefab de rondin utilisé pendant la calibration
   - Les Transforms représentant les 4 lanes
   - Le player (souvent LogParadePlayerAvatar)
   - Le TextMeshProUGUI pour afficher les instructions

- À la fin de la calibration, appeler `StartGame()` ou émettre un événement vers le GameManager

## 👤 Auteurs / liens utiles
- Basé sur un système de calibration interactive inspiré de Wii Fit / Ring Fit

## ✅ Bonnes pratiques
- Fichiers courts et bien nommés
- Architecture modulaire, comme LogParadePlayerAvatar, LogParadeUIManager, etc.
- Respecter les conventions du projet : MiniGameBase, ScriptableObject si besoin, UDPReceive, système d'événements
- Ne pas créer de code "magique" sans commentaire
- Si un composant est optionnel → le rendre visible et désactivable depuis l'inspecteur Unity
- Aucun score ou gameplay ne démarre avant fin de calibration

## ⚙️ Setup manuel des éléments détaillé dans un readme
