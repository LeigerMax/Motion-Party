# 📦 FireflyController - Contrôleur de comportement des lucioles

Ce module gère le comportement individuel des lucioles dans la zone de jeu.

## 🔧 Fonctionnalités principales
- Mouvement autonome dans la zone définie par FireflyDanceConfig
- Gestion des vitesses min/max
- Respect des limites (rebond, repositionnement ou redirection)
- Préparation à la variation de comportements futurs

## 🏗️ Structure du dossier
```
/Firefly/
├── FireflyController.cs
└── README.md
```

## 🚀 Instructions pour utiliser / intégrer
- Ce script doit être ajouté sur le prefab de luciole
- Le GameObject doit contenir un Renderer (SpriteRenderer ou MeshRenderer)
- Le prefab doit être utilisé par le FireflySpawnManager
- Assigner l'asset FireflyDanceConfig dans l'inspecteur du script
- Aucun autre composant n'est requis pour le moment

## ✅ Bonnes pratiques à respecter
- Script court, structuré, bien nommé
- Respect de l'architecture modulaire (un seul rôle par script)
- Pas de code magique, commentaires obligatoires pour tout comportement non trivial
- Ne pas dépendre d'autres scripts externes non validés
- Comportements optionnels désactivables dans l'inspecteur
