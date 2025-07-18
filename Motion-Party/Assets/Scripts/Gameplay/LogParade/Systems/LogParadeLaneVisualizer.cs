using UnityEngine;
using Gameplay.LogParade.Utils;

namespace Gameplay.LogParade.Systems
{
    /// <summary>
    /// Composant helper pour visualiser les 4 voies dans la scène Unity
    /// À attacher à un GameObject parent qui contiendra les indicateurs visuels des voies
    /// </summary>
    public class LogParadeLaneVisualizer : MonoBehaviour
    {
        #region Fields
        [Header("Lane Visualization")]
        public float laneWidth = 2f;
        public float laneLength = 20f;
        public float laneHeight = 0.1f;
        public Vector3 basePosition = Vector3.zero;
        
        [Header("Visual Settings")]
        public Material laneMaterial;
        public Color[] laneColors = new Color[4] 
        { 
            Color.red,      // Voie 1 - Gauche
            Color.yellow,   // Voie 2 - Centre-gauche  
            Color.green,    // Voie 3 - Centre-droite
            Color.blue      // Voie 4 - Droite
        };
        
        private GameObject[] laneObjects = new GameObject[4];
        #endregion

        #region Lane Generation
        /// <summary>
        /// Génère les objets visuels pour chaque voie (seulement si nécessaire)
        /// </summary>
        [ContextMenu("Generate Lanes")]
        public void GenerateLanes()
        {
            // Nettoyer les voies existantes
            ClearLanes();
            
            for (int i = 0; i < 4; i++)
            {
                CreateLaneObject(i + 1);
            }
        }
        
        /// <summary>
        /// Supprime tous les objets de voie générés automatiquement
        /// </summary>
        [ContextMenu("Clear Generated Lanes")]
        public void ClearLanes()
        {
            for (int i = 0; i < 4; i++)
            {
                if (laneObjects[i] != null)
                {
                    // Vérifier si c'est une lane générée automatiquement (enfant de ce transform)
                    if (laneObjects[i].transform.parent == transform)
                    {
                        if (Application.isPlaying)
                            Destroy(laneObjects[i]);
                        else
                            DestroyImmediate(laneObjects[i]);
                    }
                    
                    laneObjects[i] = null;
                }
            }
        }
        
        /// <summary>
        /// Crée un objet visuel pour une voie spécifique
        /// </summary>
        private void CreateLaneObject(int laneNumber)
        {
            int index = laneNumber - 1;
            
            // Créer le GameObject
            GameObject laneObj = new GameObject($"Lane_{laneNumber}");
            laneObj.transform.SetParent(transform);
            
            // Calculer la position de la voie
            Vector3 lanePosition = CalculateLanePosition(laneNumber);
            laneObj.transform.position = lanePosition;
            
            // Ajouter un cube pour représenter la voie
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(laneObj.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(0.5f, laneHeight, laneLength);
            cube.name = "LaneVisual";
            
            // Configurer le matériau et la couleur
            Renderer renderer = cube.GetComponent<Renderer>();
            if (laneMaterial != null)
            {
                renderer.material = new Material(laneMaterial);
            }
            renderer.material.color = laneColors[index];
            
            // Ajouter un label pour identifier la voie
            CreateLaneLabel(laneObj, laneNumber);
            
            // Stocker la référence
            laneObjects[index] = laneObj;
        }
        #endregion

        #region Lane Assignment
        /// <summary>
        /// Assigne des lanes externes (utilisé par LogParadeLaneManager)
        /// </summary>
        public void SetExternalLanes(GameObject[] externalLanes)
        {
            if (externalLanes.Length == 4)
            {
                laneObjects = (GameObject[])externalLanes.Clone();
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Crée un label 3D pour identifier la voie
        /// </summary>
        private void CreateLaneLabel(GameObject parent, int laneNumber)
        {
            GameObject labelObj = new GameObject($"Lane_{laneNumber}_Label");
            labelObj.transform.SetParent(parent.transform);
            labelObj.transform.localPosition = Vector3.up * 2f;
            
            // Ajouter TextMesh
            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = $"VOIE {laneNumber}";
            textMesh.fontSize = 20;
            textMesh.color = laneColors[laneNumber - 1];
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            
            // Faire face à la caméra
            labelObj.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }
        
        /// <summary>
        /// Calcule la position d'une voie donnée
        /// Utilise la même logique que LogParadePlayerAvatar
        /// </summary>
        private Vector3 CalculateLanePosition(int lane)
        {
            // Convertir le numéro de voie (1-4) en offset X
            // Voie 1 = le plus à gauche, Voie 4 = le plus à droite
            float xOffset = (lane - 2.5f) * laneWidth;
            return basePosition + Vector3.right * xOffset;
        }
        #endregion

        #region Gizmos & Editor
        void OnDrawGizmos()
        {
            // Dessiner les voies dans l'éditeur même sans objets
            for (int i = 1; i <= 4; i++)
            {
                Vector3 lanePos = CalculateLanePosition(i);
                
                // Couleur de la voie
                Gizmos.color = laneColors[i - 1];
                
                // Dessiner une boîte pour chaque voie
                Gizmos.DrawWireCube(lanePos, new Vector3(0.5f, laneHeight, laneLength));
                
                // Dessiner une ligne centrale
                Vector3 lineStart = lanePos + Vector3.back * (laneLength * 0.5f);
                Vector3 lineEnd = lanePos + Vector3.forward * (laneLength * 0.5f);
                Gizmos.DrawLine(lineStart, lineEnd);
                
                // Label dans les gizmos
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(lanePos + Vector3.up * 1f, $"Voie {i}");
                #endif
            }
            
            // Dessiner le centre de référence
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(basePosition, 0.2f);
        }
        
        #endregion
    }
}