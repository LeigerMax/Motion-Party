📦 HandTracker - Suivi de la main du joueur

Ce module lit la position d'un point de la main reçue via UDP (ex: index) et représente visuellement cette position dans la scène avec un cercle. Il est utilisé pour interagir avec les lucioles du mini-jeu.

## 🔧 Fonctionnalités principales

🖐️ Suivi de la position d'un point de la main (via UDP)

🔵 Affichage visuel avec un cercle 2D dans la scène

🚫 Position automatiquement contrainte à la zone de jeu (via FireflyDanceConfig)

🐞 Mode Debug pour afficher un Gizmo dans la scène

## 🏗️ Structure du dossier

```
HandTracker/
├── README.md
├── HandTracker.cs            # Composant principal de suivi de la main
├── HandData.cs              # Structure de données pour les informations de la main
├── HandPositionConverter.cs # Convertisseur de coordonnées caméra vers monde Unity
├── HandEvents.cs            # Gestionnaire d'événements globaux pour la main
└── HandTrackerExample.cs    # Exemple d'utilisation du HandTracker
```

## 📦 Prefabs

Un prefab `HandVisual.prefab` est disponible dans `Assets/Prefabs/Player/` avec :
- Un SpriteRenderer avec un cercle bleu semi-transparent
- Un CircleCollider2D pour les interactions
- Échelle optimisée pour la zone de jeu

## 🚀 Instructions pour utiliser / intégrer

1. Ajouter un GameObject vide nommé HandTracker dans la scène.

2. Ajouter le composant HandTracker.cs.

3. Assigner :
   - le prefab `HandVisual.prefab` dans le champ handVisual.
   - une référence vers le FireflyDanceConfig déjà existant.
   - une référence vers le UDPReceive (ou laisser vide pour auto-détection).

4. S'assurer que le module UDPReceive fonctionne et fournit les données.

5. Configurer les paramètres :
   - `handLandmarkIndex` : Point de la main à suivre (8 = index tip par défaut)
   - `enableSmoothing` : Active le lissage de position
   - `smoothingFactor` : Force du lissage (0-1)
   - `enableDebug` : Active les logs de débogage
   - `enableGizmos` : Affiche les Gizmos dans la vue Scene

## ✅ Bonnes pratiques

✅ Code clair, modulaire, commenté

✅ Position lue uniquement depuis UDPReceive

✅ Support du OnValidate pour s'assurer des limites

✅ Position clampée dans la zone via config.ClampToBounds()

✅ Tous les paramètres optionnels sont visibles et désactivables

## 🛑 Important

⛔ Ce module ne doit rien faire d'autre que gérer la position d'un point de la main et son affichage.

⛔ Pas de logique de capture ou de score ici.

✅ Tout autre système (Validator, Score, etc.) écoutera cette position via un Event ou une méthode publique.
