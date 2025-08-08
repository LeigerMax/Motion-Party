# Guide d'Utilisation UDP pour les Mini-jeux

## 🎯 Vue d'ensemble

`UDPReceive` est maintenant un **singleton global** accessible depuis tous vos mini-jeux sans configuration manuelle !

## 🚀 Utilisation Simple dans vos Mini-jeux

### Accès direct aux données UDP

```csharp
using Core;

public class MonMiniJeu : MonoBehaviour
{
    void Update()
    {
        // Vérifier si des données sont disponibles
        if (UDPReceive.IsDataAvailable())
        {
            string trackingData = UDPReceive.GetData();
            // Traiter les données JSON ici
            ProcessTrackingData(trackingData);
        }
        
        // Vérifier si le système est prêt
        if (UDPReceive.IsReady())
        {
            // Le système UDP fonctionne
        }
    }
    
    private void ProcessTrackingData(string jsonData)
    {
        // Votre logique pour parser le JSON
        Debug.Log($"Données reçues: {jsonData}");
        
        // Exemple : vérifier si des mains sont détectées
        if (jsonData.Contains("hands"))
        {
            // Des mains sont détectées dans les données
        }
    }
}
```

## 📋 Méthodes Disponibles

### UDPReceive (Singleton)
- `UDPReceive.GetData()` - Données JSON brutes de tracking
- `UDPReceive.IsDataAvailable()` - Vérifie si des données sont reçues
- `UDPReceive.IsReady()` - Vérifie si le système UDP fonctionne
- `UDPReceive.GetPacketCount()` - Nombre de paquets reçus
- `UDPReceive.GetInstance()` - Accès à l'instance pour fonctions avancées

## 🎮 Exemples Concrets pour Mini-jeux

### Détection Simple de Mains
```csharp
public class HandDetectionGame : MonoBehaviour
{
    void Update()
    {
        if (UDPReceive.IsDataAvailable())
        {
            string data = UDPReceive.GetData();
            
            // Vérifier si des mains sont détectées
            if (data.Contains("hands"))
            {
                Debug.Log("Mains détectées !");
                OnHandsDetected();
            }
        }
    }
    
    void OnHandsDetected()
    {
        // Votre logique quand des mains sont détectées
    }
}
```

### Compteur de Mouvements
```csharp
public class MovementCounter : MonoBehaviour
{
    private string lastData = "";
    private int movementCount = 0;
    
    void Update()
    {
        if (UDPReceive.IsDataAvailable())
        {
            string currentData = UDPReceive.GetData();
            
            // Détecter un changement dans les données (= mouvement)
            if (currentData != lastData && !string.IsNullOrEmpty(lastData))
            {
                movementCount++;
                Debug.Log($"Mouvements détectés: {movementCount}");
            }
            
            lastData = currentData;
        }
    }
}
```

### Système de Score basé sur l'Activité
```csharp
public class ActivityScore : MonoBehaviour
{
    public int score = 0;
    private float lastActivityTime;
    
    void Update()
    {
        if (UDPReceive.IsDataAvailable())
        {
            // Activité détectée
            lastActivityTime = Time.time;
            
            // Augmenter le score pendant l'activité
            score += Mathf.RoundToInt(Time.deltaTime * 10);
        }
        else
        {
            // Pas d'activité - diminuer le score
            if (Time.time - lastActivityTime > 2f)
            {
                score = Mathf.Max(0, score - Mathf.RoundToInt(Time.deltaTime * 5));
            }
        }
        
        // Afficher le score
        if (score > 0)
        {
            Debug.Log($"Score: {score}");
        }
    }
}
```

## 🔧 Debug et Monitoring

### Debug Simple
```csharp
void Start()
{
    // Vérifier si UDP est disponible
    if (UDPReceive.IsReady())
    {
        Debug.Log("✅ Système UDP prêt !");
    }
    else
    {
        Debug.LogWarning("⚠️ Système UDP non disponible");
    }
}

void Update()
{
    // Afficher le statut périodiquement
    if (Time.time % 5f < Time.deltaTime) // Toutes les 5 secondes
    {
        Debug.Log($"UDP Status - Packets reçus: {UDPReceive.GetPacketCount()}, Données disponibles: {UDPReceive.IsDataAvailable()}");
    }
}
```

### Interface Debug Visuelle
```csharp
void OnGUI()
{
    GUILayout.BeginArea(new Rect(10, 10, 300, 100));
    GUILayout.Box("UDP Debug");
    
    GUILayout.Label($"UDP Ready: {UDPReceive.IsReady()}");
    GUILayout.Label($"Data Available: {UDPReceive.IsDataAvailable()}");
    GUILayout.Label($"Packets: {UDPReceive.GetPacketCount()}");
    
    if (UDPReceive.IsDataAvailable())
    {
        string data = UDPReceive.GetData();
        string preview = data.Length > 50 ? data.Substring(0, 50) + "..." : data;
        GUILayout.Label($"Data: {preview}");
    }
    
    GUILayout.EndArea();
}
```

## 📝 Notes Importantes

1. **Pas de configuration nécessaire** - `UDPReceive` est automatiquement accessible
2. **Persiste entre les scènes** - Une seule instance pour tout le jeu
3. **Thread-safe** - Utilisation sécurisée depuis n'importe quel script
4. **JSON brut** - Vous gérez le parsing selon vos besoins
5. **Performance optimisée** - Accès direct sans overhead

## 🎯 Checklist pour Nouveau Mini-jeu

- [ ] Importer `using Core;`
- [ ] Vérifier `UDPReceive.IsDataAvailable()` avant traitement
- [ ] Parser le JSON selon vos besoins spécifiques
- [ ] Ajouter du debug visuel avec `OnGUI()` si nécessaire
- [ ] Tester sans webcam (données vides)

**Simple et direct - juste `UDPReceive` et vos propres scripts ! �**
