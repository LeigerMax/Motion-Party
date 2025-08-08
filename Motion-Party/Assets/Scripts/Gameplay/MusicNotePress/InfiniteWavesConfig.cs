using UnityEngine;

namespace Gameplay.MusicNotePress
{
    /// <summary>
    /// Configuration pour le système de vagues infinies du jeu MusicNote
    /// </summary>
    [CreateAssetMenu(fileName = "InfiniteWavesConfig", menuName = "MusicNote/Infinite Waves Config")]
    public class InfiniteWavesConfig : ScriptableObject
    {
        [Header("Configuration des Vagues")]
        [Tooltip("Nombre de notes pour la première vague")]
        [Range(2, 5)]
        public int startingNotesCount = 2;
        
        [Tooltip("Nombre maximum de notes par vague")]
        [Range(5, 15)]
        public int maxNotesPerWave = 10;
        
        [Tooltip("Activer le système de vagues infinies")]
        public bool enableInfiniteWaves = true;
        
        [Header("Timing")]
        [Tooltip("Délai avant le démarrage de chaque vague")]
        [Range(0.5f, 3f)]
        public float startDelay = 1f;
        
        [Tooltip("Délai entre les vagues")]
        [Range(1f, 5f)]
        public float delayBetweenWaves = 3f;
        
        [Tooltip("Temps de validation pour l'input du joueur")]
        [Range(2f, 5f)]
        public float validationTime = 3f;
        
        [Header("Scoring")]
        [Tooltip("Multiplicateur de score par vague (score = points * numéro de vague)")]
        [Range(50, 200)]
        public int scoreMultiplier = 100;
        
        [Header("Difficulté")]
        [Tooltip("Mode de progression de la difficulté")]
        public DifficultyProgression difficultyMode = DifficultyProgression.Linear;
        
        [Tooltip("Vitesse de progression de la difficulté (pour mode Custom)")]
        [Range(1, 3)]
        public int difficultyStep = 1;
        
        public enum DifficultyProgression
        {
            Linear,      // +1 note par vague
            Gradual,     // +1 note tous les 2 vagues
            Custom       // Utilise difficultyStep
        }
        
        /// <summary>
        /// Calcule le nombre de notes pour une vague donnée
        /// </summary>
        public int GetNotesCountForWave(int waveNumber)
        {
            int notesCount = startingNotesCount;
            
            switch (difficultyMode)
            {
                case DifficultyProgression.Linear:
                    notesCount = startingNotesCount + (waveNumber - 1);
                    break;
                    
                case DifficultyProgression.Gradual:
                    notesCount = startingNotesCount + ((waveNumber - 1) / 2);
                    break;
                    
                case DifficultyProgression.Custom:
                    notesCount = startingNotesCount + ((waveNumber - 1) / difficultyStep);
                    break;
            }
            
            return Mathf.Min(notesCount, maxNotesPerWave);
        }
        
        /// <summary>
        /// Calcule le score pour une vague donnée
        /// </summary>
        public int GetScoreForWave(int waveNumber)
        {
            return scoreMultiplier * waveNumber;
        }
        
        /// <summary>
        /// Applique cette configuration à un MusicNoteGameController
        /// </summary>
        public void ApplyTo(MusicNoteGameController controller)
        {
            if (controller == null) return;
            
            // Utilisation de la réflexion pour appliquer les propriétés privées
            var controllerType = typeof(MusicNoteGameController);
            
            // Chercher et définir les champs accessibles
            var startDelayField = controllerType.GetField("startDelay");
            if (startDelayField != null) startDelayField.SetValue(controller, startDelay);
            
            var delayBetweenWavesField = controllerType.GetField("delayBetweenWaves");
            if (delayBetweenWavesField != null) delayBetweenWavesField.SetValue(controller, delayBetweenWaves);
            
            var validationTimeField = controllerType.GetField("validationTime");
            if (validationTimeField != null) validationTimeField.SetValue(controller, validationTime);
            
            var startingNotesCountField = controllerType.GetField("startingNotesCount");
            if (startingNotesCountField != null) startingNotesCountField.SetValue(controller, startingNotesCount);
            
            var maxNotesPerWaveField = controllerType.GetField("maxNotesPerWave");
            if (maxNotesPerWaveField != null) maxNotesPerWaveField.SetValue(controller, maxNotesPerWave);
            
            var enableInfiniteWavesField = controllerType.GetField("enableInfiniteWaves");
            if (enableInfiniteWavesField != null) enableInfiniteWavesField.SetValue(controller, enableInfiniteWaves);
            
            Debug.Log($"[InfiniteWavesConfig] Configuration appliquée - Vagues infinies: {enableInfiniteWaves}, Notes de départ: {startingNotesCount}, Max: {maxNotesPerWave}");
        }
    }
}
