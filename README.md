# Motion-Party
Animation game project for retirement homes in order to obtain my thesis in computer science

## 🎮 Mini-Games

### LogParade - "Le Défilé des Rondins"
Mini-jeu de plateforme adapté aux seniors où le joueur doit naviguer sur des rondins qui défilent.

#### 🆕 LogParadeGameTimer
Nouveau système de gestion du cycle de vie des parties :
- **Timer configurable** avec durée personnalisable
- **Démarrage/arrêt automatique** du score et des rondins
- **Intégration MiniGameBase** pour compatibilité avec le système global
- **Événements UnityEvent** pour l'UI et les écrans de fin
- **Interface de debug** intégrée pour les tests

**Localisation :** `Assets/Scripts/Gameplay/LogParade/LogParadeGameTimer/`

**Utilisation :**
```csharp
// Démarrer une partie
gameTimer.LaunchLevel();

// Configurer la durée
gameTimer.SetGameDuration(60f);

// S'abonner aux événements
gameTimer.OnGameEnd.AddListener(() => {
    Debug.Log($"Partie terminée ! Score : {gameTimer.GetCurrentScore()}");
});
```

**Testing :** Utilisez `TestLogParadeGameTimer.cs` pour tester toutes les fonctionnalités.

## 🛠️ Installation et Configuration

1. Ouvrir le projet dans Unity 2022.3 LTS ou plus récent
2. Assurer que les packages nécessaires sont installés (MediaPipe, TextMeshPro)
3. Configurer les références dans l'inspecteur Unity
4. Tester avec les scripts de test fournis

## 📁 Structure du Projet

```
Motion-Party/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/                          # Scripts de base (MiniGameBase)
│   │   └── Gameplay/
│   │       └── LogParade/                 # Mini-jeu LogParade
│   │           ├── LogParadeGameController.cs
│   │           ├── LogParadeGameTimer/    # 🆕 Système de timer
│   │           ├── LogParadeScoreManager/ # Gestion du score
│   │           └── LogParadeLogGenerator/ # Génération des rondins
│   └── Scenes/                            # Scènes de jeu
├── python-tracker/                        # Système de tracking Python
└── README.md
```

## 🧪 Testing

### LogParadeGameTimer
- Utiliser `TestLogParadeGameTimer.cs` 
- Touches 1-5 pour différents tests
- Interface GUI intégrée pour contrôles visuels
- Vérification automatique des composants requis

---

*Projet développé dans le cadre d'une thèse en informatique*  
*Spécialement conçu pour les maisons de retraite*
