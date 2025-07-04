using UnityEngine;
using Gameplay.FireFlyDance.Core;
using Gameplay.FireFlyDance.Utils;

namespace Gameplay.FireFlyDance.Hand
{
    /// <summary>
    /// Utilitaire pour convertir les coordonnées de position de la main
    /// Entre l'espace caméra MediaPipe et l'espace monde Unity
    /// Version refactorisée pour l'architecture FireflyDance
    /// </summary>
    public static class HandPositionConverter
    {
        /// <summary>
        /// Convertit les coordonnées de la caméra MediaPipe vers l'espace monde Unity
        /// </summary>
        /// <param name="cameraX">Position X dans l'espace caméra (0-1)</param>
        /// <param name="cameraY">Position Y dans l'espace caméra (0-1)</param>
        /// <param name="config">Configuration contenant les limites de la zone de jeu</param>
        /// <returns>Position dans l'espace monde Unity</returns>
        public static Vector2 CameraToWorld(float cameraX, float cameraY, FireflyDanceConfig config)
        {
            if (config == null)
            {
                FireflyDanceLogger.LogError("HandPositionConverter - Config manquante");
                return Vector2.zero;
            }
            
            // Inverser Y car MediaPipe utilise un système où Y=0 est en haut
            float invertedY = 1f - cameraY;
            
            // Mapper vers les limites de la zone de jeu
            float worldX = Mathf.Lerp(config.TopLeft.x, config.BottomRight.x, cameraX);
            float worldY = Mathf.Lerp(config.BottomRight.y, config.TopLeft.y, invertedY);
            
            Vector2 worldPosition = new Vector2(worldX, worldY);
            
            // S'assurer que la position est dans les limites
            worldPosition = config.ClampToBounds(worldPosition);
            
            FireflyDanceLogger.LogVerbose($"Conversion caméra->monde: ({cameraX:F3}, {cameraY:F3}) -> ({worldPosition.x:F3}, {worldPosition.y:F3})");
            
            return worldPosition;
        }
        
        /// <summary>
        /// Convertit les coordonnées du monde Unity vers l'espace caméra MediaPipe
        /// </summary>
        /// <param name="worldPosition">Position dans l'espace monde Unity</param>
        /// <param name="config">Configuration contenant les limites de la zone de jeu</param>
        /// <returns>Position dans l'espace caméra (0-1)</returns>
        public static Vector2 WorldToCamera(Vector2 worldPosition, FireflyDanceConfig config)
        {
            if (config == null)
            {
                FireflyDanceLogger.LogError("HandPositionConverter - Config manquante");
                return Vector2.zero;
            }
            
            // Mapper depuis les limites de la zone de jeu vers 0-1
            float cameraX = Mathf.InverseLerp(config.TopLeft.x, config.BottomRight.x, worldPosition.x);
            float normalizedY = Mathf.InverseLerp(config.BottomRight.y, config.TopLeft.y, worldPosition.y);
            
            // Inverser Y pour correspondre au système MediaPipe
            float cameraY = 1f - normalizedY;
            
            Vector2 cameraPosition = new Vector2(cameraX, cameraY);
            
            FireflyDanceLogger.LogVerbose($"Conversion monde->caméra: ({worldPosition.x:F3}, {worldPosition.y:F3}) -> ({cameraPosition.x:F3}, {cameraPosition.y:F3})");
            
            return cameraPosition;
        }
        
        /// <summary>
        /// Normalise une position dans l'espace de la zone de jeu (0-1)
        /// </summary>
        /// <param name="worldPosition">Position dans l'espace monde</param>
        /// <param name="config">Configuration de la zone de jeu</param>
        /// <returns>Position normalisée (0-1)</returns>
        public static Vector2 WorldToNormalized(Vector2 worldPosition, FireflyDanceConfig config)
        {
            if (config == null)
            {
                FireflyDanceLogger.LogError("HandPositionConverter - Config manquante");
                return Vector2.zero;
            }
            
            float normalizedX = Mathf.InverseLerp(config.TopLeft.x, config.BottomRight.x, worldPosition.x);
            float normalizedY = Mathf.InverseLerp(config.BottomRight.y, config.TopLeft.y, worldPosition.y);
            
            return new Vector2(normalizedX, normalizedY);
        }
        
        /// <summary>
        /// Convertit une position normalisée (0-1) vers l'espace monde
        /// </summary>
        /// <param name="normalizedPosition">Position normalisée (0-1)</param>
        /// <param name="config">Configuration de la zone de jeu</param>
        /// <returns>Position dans l'espace monde</returns>
        public static Vector2 NormalizedToWorld(Vector2 normalizedPosition, FireflyDanceConfig config)
        {
            if (config == null)
            {
                FireflyDanceLogger.LogError("HandPositionConverter - Config manquante");
                return Vector2.zero;
            }
            
            float worldX = Mathf.Lerp(config.TopLeft.x, config.BottomRight.x, normalizedPosition.x);
            float worldY = Mathf.Lerp(config.BottomRight.y, config.TopLeft.y, normalizedPosition.y);
            
            return new Vector2(worldX, worldY);
        }
        
        /// <summary>
        /// Vérifie si une position caméra est dans les limites valides (0-1)
        /// </summary>
        /// <param name="cameraPosition">Position caméra à vérifier</param>
        /// <returns>True si la position est valide</returns>
        public static bool IsValidCameraPosition(Vector2 cameraPosition)
        {
            return cameraPosition.x >= 0f && cameraPosition.x <= 1f &&
                   cameraPosition.y >= 0f && cameraPosition.y <= 1f;
        }
        
        /// <summary>
        /// Contraint une position caméra dans les limites 0-1
        /// </summary>
        /// <param name="cameraPosition">Position caméra à contraindre</param>
        /// <returns>Position contrainte</returns>
        public static Vector2 ClampCameraPosition(Vector2 cameraPosition)
        {
            return new Vector2(
                Mathf.Clamp01(cameraPosition.x),
                Mathf.Clamp01(cameraPosition.y)
            );
        }
    }
}
