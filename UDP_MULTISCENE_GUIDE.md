# Guide UDP Multi-Scène pour Mini-jeux

## 🎯 Architecture Multi-Scène

Chaque scène (principale et mini-jeux) a son propre `UDPReceive`. Le système gère automatiquement le passage d'une scène à l'autre en activant la bonne instance.

### 🏗️ Comment ça marche

**Scène Principale :**
- UDPReceive pour l'écran de chargement et menu principal
- Se désactive automatiquement quand on charge un mini-jeu

**Scènes de Mini-jeux :**
- Chaque mini-jeu a son propre UDPReceive
- Prend automatiquement le relais quand la scène se charge
- Configuration indépendante (port, paramètres)

## 🚀 Utilisation (Code inchangé !)

```csharp
using Core;

public class MonMiniJeu : MonoBehaviour
{
    void Update()
    {
        // Fonctionne toujours pareil !
        if (UDPReceive.IsDataAvailable())
        {
            string trackingData = UDPReceive.GetData();
            ProcessTrackingData(trackingData);
        }
    }
}
```

**💡 Le système gère automatiquement quelle instance UDP utiliser !**

## 📋 Configuration des Scènes

### 1. Scène Principale
- Ajouter `UDPSceneManager` avec `isMainScene = true`
- Garder votre `UDPReceive` existant

### 2. Scène de Mini-jeu
- Ajouter `UDPSceneManager` avec `isMiniGameScene = true`
- S'assurer qu'il y a un `UDPReceive` dans la scène

## 🔄 Transitions Automatiques

```csharp
// Changement normal - le système gère tout
SceneManager.LoadScene("MiniJeu1");

// Ou avec logs supplémentaires
UDPSceneManager.LoadMiniGameScene("MiniJeu1");
UDPSceneManager.LoadMainScene("MainMenu");
```

## 🎮 Exemple Complet Mini-jeu

```csharp
using Core;
using UnityEngine;

public class MiniGameExample : MonoBehaviour
{
    void Start()
    {
        // Optionnel : vérifier l'UDP local
        UDPReceive localUDP = FindObjectOfType<UDPReceive>();
        if (localUDP != null)
        {
            Debug.Log($"✅ UDP trouvé sur {gameObject.scene.name}");
        }
    }
    
    void Update()
    {
        // Utilisation normale - automatique !
        if (UDPReceive.IsDataAvailable())
        {
            string data = UDPReceive.GetData();
            
            if (data.Contains("hands"))
            {
                Debug.Log("🖐️ Mains détectées dans le mini-jeu !");
                OnHandsDetected();
            }
        }
    }
    
    void OnHandsDetected()
    {
        // Votre logique spécifique au mini-jeu
    }
}
```

## 🔧 Debug Multi-Scène

### UDPSceneManager
Ajoutez `UDPSceneManager` pour :
- Logs automatiques des transitions
- Debug contextuel avec `[ContextMenu] "Debug UDP Status"`

### Debug Visuel Amélioré
```csharp
void OnGUI()
{
    GUILayout.BeginArea(new Rect(10, 10, 300, 150));
    GUILayout.Box($"UDP Debug - {SceneManager.GetActiveScene().name}");
    
    GUILayout.Label($"UDP Instance: {(UDPReceive.Instance != null ? "✅" : "❌")}");
    GUILayout.Label($"Données: {(UDPReceive.IsDataAvailable() ? "✅" : "❌")}");
    GUILayout.Label($"Paquets: {UDPReceive.GetPacketCount()}");
    
    // Vérifier l'instance locale
    UDPReceive local = FindObjectOfType<UDPReceive>();
    GUILayout.Label($"UDP Local: {(local != null ? "✅" : "❌")}");
    
    if (local != null && UDPReceive.Instance != null)
    {
        bool isActive = (UDPReceive.Instance == local);
        GUILayout.Label($"Est Actif: {(isActive ? "✅" : "❌")}");
    }
    
    GUILayout.EndArea();
}
```

## 📝 Avantages du Système

✅ **Pas de conflit** entre les scènes
✅ **Configuration indépendante** par mini-jeu  
✅ **Transition automatique** transparente
✅ **Code inchangé** dans vos mini-jeux
✅ **Debug facilité** avec UDPSceneManager

## 🎯 Checklist Nouveau Mini-jeu

- [ ] GameObject avec `UDPReceive` dans la scène
- [ ] `UDPSceneManager` avec `isMiniGameScene = true` 
- [ ] Tester la transition depuis la scène principale
- [ ] Code normal : `UDPReceive.GetData()` fonctionne
- [ ] Vérifier les logs de changement de scène

## 🚨 Troubleshooting

### Pas de données dans le mini-jeu ?
```csharp
// Debug dans le mini-jeu :
Debug.Log($"UDP local: {FindObjectOfType<UDPReceive>() != null}");
Debug.Log($"Instance globale: {UDPReceive.Instance != null}");
Debug.Log($"Scène instance: {UDPReceive.Instance?.gameObject.scene.name}");
```

### Plusieurs UDPReceive dans une scène ?
- Le système garde automatiquement celui de la scène active
- Utilisez `UDPSceneManager.DebugUDPStatus()` pour diagnostiquer

**Maintenant chaque mini-jeu fonctionne avec son propre UDP ! 🎉**
