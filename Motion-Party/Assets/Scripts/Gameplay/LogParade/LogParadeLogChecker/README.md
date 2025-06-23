# 📦 PlayerOnLogChecker

Vérifie en temps réel si l'avatar du joueur est sur un rondin, avec tolérance et détection douce.

## 🔧 Fonctionnalités principales

✅ Détection de contact entre l'avatar et les rondins

✅ Tolérance de 0.3 seconde après changement de voie

✅ Prise en compte de l'enchaînement de rondins proches

✅ Méthode publique pour interroger l'état "sur rondin"

✅ Logs et feedbacks de debug visibles pendant les tests

## 🏗️ Structure du dossier

```
/LogParadeLogChecker/
├── PlayerOnLogChecker.cs
└── README.md
```

## 🧪 Instructions pour tester

### Création de la scène de test :

1. **Créer une scène avec :**
   - Un LogParadePlayerAvatar avec un collider trigger de détection
   - Des prefabs de rondins bien placés (via le module précédent)
   - Ajouter le script PlayerOnLogChecker à l'avatar

2. **Lancer la scène**

3. **Vérifier :**
   - Que `IsOnLog` passe à `true` en cas de contact
   - Que le changement de voie est toléré 0.3s
   - Que les rondins proches ne causent pas de fausses chutes

### Configuration recommandée :

- **Tag des rondins :** Utiliser le tag `"Log"` sur tous les objets rondin
- **Rayon de détection :** 0.8 unités (configurable dans l'inspecteur)
- **Distance entre rondins :** < 0.5 unités pour être considérés comme "proches"

### Paramètres configurables dans l'inspecteur :

- `Detection Radius` : Rayon du collider de détection (défaut : 0.8)
- `Max Log Gap Distance` : Distance max entre rondins proches (défaut : 0.5)
- `Lane Change Tolerance Time` : Temps de tolérance après changement de voie (défaut : 0.3s)
- `Enable Debug Logs` : Afficher les logs de debug
- `Show Debug Gizmos` : Afficher les gizmos de debug dans la scène

## 🚀 Instructions pour utiliser / intégrer

### Étapes d'intégration :

1. **Ajouter le script PlayerOnLogChecker à l'objet avatar**
   ```csharp
   // Le script se configure automatiquement au Start()
   ```

2. **Configurer son collider de détection (radius ≈ 0.8 recommandé)**
   - Le script crée automatiquement un SphereCollider s'il n'existe pas
   - Configure automatiquement le collider comme trigger

3. **Connecter les événements ou score manager via IsOnLog**
   ```csharp
   // Exemple d'utilisation dans un autre script :
   PlayerOnLogChecker logChecker = player.GetComponent<PlayerOnLogChecker>();
   
   if (logChecker.IsPlayerOnLog())
   {
       // Le joueur est sur un rondin - pas de pénalité
   }
   else
   {
       // Le joueur est dans l'eau - appliquer pénalité/game over
   }
   ```

### API publique disponible :

```csharp
// Propriété principale - état actuel
public bool IsOnLog { get; }

// Méthodes publiques
public bool IsPlayerOnLog()           // Méthode recommandée pour les autres systèmes
public bool IsInTolerancePeriod()     // Vérifier si en période de tolérance
public int GetDetectedLogCount()      // Nombre de rondins en contact
public void UpdateDetectionRadius(float newRadius) // Changer le rayon de détection
```

### Règles de détection implémentées :

**✅ Le joueur est considéré sur un rondin si :**
- Il est physiquiquement au-dessus d'un rondin détecté par un overlap/trigger
- Il vient de changer de voie : on lui accorde 0.3s de tolérance pour atterrir sur un autre rondin
- Il se trouve entre deux rondins proches sur la même voie (espacement < 0.5 unités)

**❌ Le joueur est considéré dans l'eau (donc en danger) si :**
- Il n'est sur aucun rondin et n'est pas en période de tolérance
- Il a quitté un rondin sans en atteindre un autre dans les 0.3s

**🔧 Logique de tolérance corrigée :**
- La période de tolérance s'interrompt automatiquement dès que le joueur touche un nouveau rondin
- Cela évite les faux négatifs où un joueur sur un rondin était considéré "dans l'eau" à la fin du timer

## 👤 Auteurs / liens utiles

Aucun pour le moment, lien vers la logique score à venir

## ✅ Bonnes pratiques respectées

- ✅ **Fichiers bien nommés et courts** : Un seul script de 400 lignes avec noms explicites
- ✅ **Pas de logique de score** : Ce module ne fait que détecter, pas de calcul de points
- ✅ **Bien documenté** : Commentaires XML sur toutes les méthodes publiques
- ✅ **Paramètres exposés** : Tous les réglages sont configurables dans l'inspecteur
- ✅ **Debug lisible** : Logs clairs avec préfixe [PlayerOnLogChecker] et niveaux appropriés

## 🔧 Détails techniques

### Système basé sur zone de détection :
- Utilise un `SphereCollider` configuré comme trigger
- Détection via `OnTriggerEnter/Stay/Exit()`
- Centre du collider décalé légèrement vers le bas (-0.2) pour une détection naturelle

### Gestion de la tolérance :
- Timer activé automatiquement lors des changements de voie
- Détection des changements via comparaison avec `LogParadePlayerAvatar.GetCurrentLane()`
- État maintenu tant que le timer n'expire pas

### Optimisations :
- Cache des colliders détectés avec nettoyage automatique des objets détruits
- Réutilisation des listes pour éviter les allocations GC
- Vérification d'état uniquement lors des changements

### Identification des rondins :
- Priorité au tag `"Log"` (méthode recommandée)
- Fallback sur le nom de l'objet contenant "log" ou "rondin"
- Facilement extensible pour d'autres critères

## 🛑 Limitations et notes importantes

- **Dépendance optionnelle** : Fonctionne mieux avec `LogParadePlayerAvatar` mais peut fonctionner seul
- **Tags requis** : Les rondins doivent avoir le tag `"Log"` pour être détectés correctement
- **Distance entre rondins** : La détection "entre rondins proches" est actuellement basée sur le trigger overlap
- **Module isolé** : Aucune dépendance vers d'autres systèmes (score, UI, etc.)

## 🚨 Guide de dépannage - Rondins non détectés

### ✅ Checklist de configuration des rondins :

1. **Tag "Log" obligatoire :**
   ```
   - Sélectionner tous vos objets rondins
   - Dans l'Inspector : Tag → "Log"
   - Si le tag "Log" n'existe pas : Tag → Add Tag... → créer "Log"
   ```

2. **Collider en Is Trigger :**
   ```
   - BoxCollider (ou autre) sur le rondin
   - ✅ Is Trigger coché
   - ❌ Ne PAS mettre Is Kinematic (sauf si physique spéciale)
   ```

3. **Layer approprié :**
   ```
   - Mettre les rondins sur un layer dédié (ex: "Logs")
   - Vérifier Edit → Project Settings → Physics → Layer Collision Matrix
   - S'assurer que le layer du joueur peut interagir avec celui des rondins
   ```

### ✅ Configuration du joueur (PlayerOnLogChecker) :

1. **Script sur l'objet joueur :**
   ```
   - PlayerOnLogChecker attaché au GameObject du joueur
   - SphereCollider automatiquement créé et configuré comme trigger
   ```

2. **Rigidbody recommandé :**
   ```
   - Ajouter un Rigidbody au joueur (améliore la détection trigger)
   - Is Kinematic = true (si vous gérez le mouvement manuellement)
   ```

### 🔧 Test de diagnostic rapide :

**Ajoutez ce script temporaire sur vos rondins pour tester :**

```csharp
using UnityEngine;

public class LogDiagnostic : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[LOG DIAGNOSTIC] Trigger Enter détecté: {other.name} (Tag: {other.tag})");
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[LOG DIAGNOSTIC] Trigger Exit détecté: {other.name} (Tag: {other.tag})");
    }
}
```

### 🐛 Problèmes fréquents et solutions :

| Problème | Cause probable | Solution |
|----------|---------------|----------|
| Aucun trigger détecté | Tag manquant | Ajouter tag "Log" aux rondins |
| Trigger détecté mais pas par PlayerOnLogChecker | Nom/tag incorrect | Vérifier IsLogObject() dans le script |
| Détection intermittente | Pas de Rigidbody | Ajouter Rigidbody au joueur |
| Collisions au lieu de triggers | Is Trigger non coché | Cocher Is Trigger sur les rondins |
| Rien ne fonctionne | Layer collision désactivée | Vérifier Physics Layer Matrix |

### 📋 Configuration recommandée complète :

**Rondin (GameObject) :**
```
├── Mesh/Model du rondin
├── BoxCollider (Is Trigger = ✅)
├── Tag = "Log"
├── Layer = "Logs" (optionnel mais recommandé)
└── LogDiagnostic (temporaire pour tests)
```

**Joueur (GameObject) :**
```
├── LogParadePlayerAvatar
├── PlayerOnLogChecker
├── SphereCollider (créé automatiquement, Is Trigger = ✅)
├── Rigidbody (Is Kinematic = ✅)
└── Tag = "Player" (optionnel)
```
