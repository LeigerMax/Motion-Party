using UnityEngine;

namespace Gameplay.FireFlyDance.Player.HandTracker
{
    /// <summary>
    /// Convertisseur de coordonnées de la caméra vers le monde Unity
    /// Responsable de la transformation des coordonnées reçues via UDP
    /// </summary>
    public static class HandPositionConverter
    {
        /// <summary>
        /// Convertit les coordonnées de la caméra Python (640x480) vers les coordonnées du monde Unity
        /// </summary>
        /// <param name="cameraX">Coordonnée X de la caméra (0-640)</param>
        /// <param name="cameraY">Coordonnée Y de la caméra (0-480, déjà inversée côté Python)</param>
        /// <param name="config">Configuration du jeu pour les limites de la zone</param>
        /// <returns>Position dans le monde Unity</returns>
        public static Vector2 CameraToWorld(float cameraX, float cameraY, FireflyDanceConfig config)
        {
            // Normaliser les coordonnées de la caméra (0-1)
            float normalizedX = cameraX / 640f;
            float normalizedY = cameraY / 480f;
            
            // Mapper sur la zone de jeu définie dans le config
            float worldX = Mathf.Lerp(config.TopLeft.x, config.BottomRight.x, normalizedX);
            float worldY = Mathf.Lerp(config.BottomRight.y, config.TopLeft.y, normalizedY);
            
            return new Vector2(worldX, worldY);
        }
        
        /// <summary>
        /// Convertit une position du monde Unity vers les coordonnées de la caméra
        /// Utile pour le debug ou l'affichage d'informations
        /// </summary>
        /// <param name="worldPosition">Position dans le monde Unity</param>
        /// <param name="config">Configuration du jeu</param>
        /// <returns>Position de la caméra</returns>
        public static Vector2 WorldToCamera(Vector2 worldPosition, FireflyDanceConfig config)
        {
            // Normaliser par rapport à la zone de jeu
            float normalizedX = Mathf.InverseLerp(config.TopLeft.x, config.BottomRight.x, worldPosition.x);
            float normalizedY = Mathf.InverseLerp(config.BottomRight.y, config.TopLeft.y, worldPosition.y);
            
            // Convertir en coordonnées caméra
            float cameraX = normalizedX * 640f;
            float cameraY = normalizedY * 480f;
            
            return new Vector2(cameraX, cameraY);
        }
    }
}
