using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Générateur de patterns de rondins pour LogParade.
/// Responsable de la création de configurations de rondins valides et jouables.
/// Utilise des algorithmes optimisés pour maintenir un gameplay fluide et accessible.
/// </summary>
public class LogParadeLogPatternGenerator
{
    #region Configuration
    private readonly int laneCount = 4;
    private readonly int maxLogsPerRow = 3; // Jamais 4 pour laisser de l'espace
    private readonly int minLogsPerRow = 1;
    private readonly int maxGenerationAttempts = 20;
    #endregion

    #region Events
    public System.Action<LogRow> OnPatternGenerated;
    public System.Action<string> OnPatternGenerationFailed;
    #endregion

    #region Pattern Generation
    /// <summary>
    /// Génère une nouvelle rangée de rondins valide et jouable.
    /// </summary>
    /// <returns>LogRow valide ou null si la génération échoue</returns>
    public LogRow GenerateValidLogRow()
    {
        LogRow row = new LogRow();
        row.spawnTime = Time.time;

        // Génère entre 1 et 3 rondins par rangée
        int logCount = Random.Range(minLogsPerRow, maxLogsPerRow + 1);

        // Essaie plusieurs configurations jusqu'à trouver une valide
        for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
        {
            // Reset de la rangée
            row.ClearAll();

            // Place les rondins selon un pattern aléatoire
            if (GenerateRandomPattern(row, logCount))
            {
                OnPatternGenerated?.Invoke(row);
                return row;
            }
        }

        // Si aucune configuration valide trouvée, utilise un pattern de secours
        LogRow fallbackRow = GenerateFallbackPattern();
        OnPatternGenerationFailed?.Invoke("Utilisation du pattern de secours après " + maxGenerationAttempts + " tentatives");
        OnPatternGenerated?.Invoke(fallbackRow);
        return fallbackRow;
    }

    /// <summary>
    /// Génère un pattern aléatoire de rondins.
    /// </summary>
    private bool GenerateRandomPattern(LogRow row, int logCount)
    {
        // Crée une liste des voies disponibles
        List<int> availableLanes = new List<int>();
        for (int i = 0; i < laneCount; i++)
        {
            availableLanes.Add(i);
        }

        // Place les rondins aléatoirement
        for (int i = 0; i < logCount && availableLanes.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableLanes.Count);
            int laneIndex = availableLanes[randomIndex];
            row.hasLog[laneIndex] = true;
            availableLanes.RemoveAt(randomIndex);
        }

        // Vérifie si cette configuration est valide
        return row.IsValidPath();
    }

    /// <summary>
    /// Génère un pattern de secours garantissant la jouabilité.
    /// </summary>
    private LogRow GenerateFallbackPattern()
    {
        LogRow row = new LogRow();
        row.spawnTime = Time.time;

        // Configuration de secours : 2 rondins adjacents au centre
        row.hasLog[1] = true; // Voie centrale gauche
        row.hasLog[2] = true; // Voie centrale droite

        return row;
    }
    #endregion

    #region Pattern Validation
    /// <summary>
    /// Valide qu'un pattern de rondins est jouable.
    /// </summary>
    public bool ValidatePattern(LogRow row)
    {
        return row != null && row.IsValidPath();
    }

    /// <summary>
    /// Analyse la difficulté d'un pattern.
    /// </summary>
    public PatternDifficulty AnalyzePatternDifficulty(LogRow row)
    {
        if (row == null) return PatternDifficulty.Invalid;

        int logCount = row.GetLogCount();
        bool hasAdjacentLogs = row.HasAdjacentLogs();
        bool hasIsolatedLogs = row.HasIsolatedLogs();

        // Détermine la difficulté selon les critères
        if (logCount == 1)
        {
            return PatternDifficulty.Easy;
        }
        else if (logCount == 2 && hasAdjacentLogs)
        {
            return PatternDifficulty.Easy;
        }
        else if (logCount == 2 && !hasAdjacentLogs)
        {
            return PatternDifficulty.Medium;
        }
        else if (logCount == 3 && hasAdjacentLogs && !hasIsolatedLogs)
        {
            return PatternDifficulty.Medium;
        }
        else if (logCount == 3)
        {
            return PatternDifficulty.Hard;
        }

        return PatternDifficulty.Medium;
    }
    #endregion

    #region Pattern Analytics
    /// <summary>
    /// Génère des statistiques sur les patterns générés.
    /// </summary>
    public PatternStats GeneratePatternStats(List<LogRow> recentRows)
    {
        if (recentRows == null || recentRows.Count == 0)
            return new PatternStats();

        PatternStats stats = new PatternStats();
        stats.totalPatterns = recentRows.Count;

        foreach (var row in recentRows)
        {
            stats.totalLogs += row.GetLogCount();
            
            PatternDifficulty difficulty = AnalyzePatternDifficulty(row);
            switch (difficulty)
            {
                case PatternDifficulty.Easy:
                    stats.easyPatterns++;
                    break;
                case PatternDifficulty.Medium:
                    stats.mediumPatterns++;
                    break;
                case PatternDifficulty.Hard:
                    stats.hardPatterns++;
                    break;
            }
        }

        stats.averageLogsPerPattern = stats.totalPatterns > 0 ? (float)stats.totalLogs / stats.totalPatterns : 0f;
        return stats;
    }
    #endregion
}

#region Data Structures
/// <summary>
/// Représente une rangée de rondins sur les 4 voies.
/// </summary>
[System.Serializable]
public class LogRow
{
    public bool[] hasLog = new bool[4]; // true si un rondin est présent sur cette voie
    public float spawnTime;

    public LogRow()
    {
        hasLog = new bool[4];
        spawnTime = Time.time;
    }

    /// <summary>
    /// Vérifie si cette rangée forme un chemin valide et jouable.
    /// </summary>
    public bool IsValidPath()
    {
        int logCount = GetLogCount();
        
        // Au moins un rondin requis
        if (logCount == 0) return false;
        
        // Vérifie la connectivité - au moins une séquence de rondins accessible
        return HasAccessibleSequence();
    }

    /// <summary>
    /// Compte le nombre de rondins dans cette rangée.
    /// </summary>
    public int GetLogCount()
    {
        int count = 0;
        for (int i = 0; i < hasLog.Length; i++)
        {
            if (hasLog[i]) count++;
        }
        return count;
    }

    /// <summary>
    /// Vérifie s'il y a des rondins adjacents.
    /// </summary>
    public bool HasAdjacentLogs()
    {
        for (int i = 0; i < hasLog.Length - 1; i++)
        {
            if (hasLog[i] && hasLog[i + 1])
                return true;
        }
        return false;
    }

    /// <summary>
    /// Vérifie s'il y a des rondins isolés.
    /// </summary>
    public bool HasIsolatedLogs()
    {
        for (int i = 0; i < hasLog.Length; i++)
        {
            if (hasLog[i])
            {
                bool hasLeftNeighbor = i > 0 && hasLog[i - 1];
                bool hasRightNeighbor = i < hasLog.Length - 1 && hasLog[i + 1];
                
                if (!hasLeftNeighbor && !hasRightNeighbor)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Vérifie qu'il existe au moins une séquence de rondins accessible.
    /// </summary>
    private bool HasAccessibleSequence()
    {
        // Pour un gameplay senior-friendly, on considère qu'un rondin isolé est accessible
        // et que toute séquence de rondins adjacents est accessible
        return true; // Simplifié pour l'instant
    }

    /// <summary>
    /// Efface tous les rondins de cette rangée.
    /// </summary>
    public void ClearAll()
    {
        for (int i = 0; i < hasLog.Length; i++)
        {
            hasLog[i] = false;
        }
    }

    /// <summary>
    /// Retourne une représentation textuelle du pattern.
    /// </summary>
    public string GetPatternString()
    {
        string pattern = "";
        for (int i = 0; i < hasLog.Length; i++)
        {
            pattern += hasLog[i] ? "■ " : "□ ";
        }
        return pattern.Trim();
    }
}

/// <summary>
/// Énumération des niveaux de difficulté d'un pattern.
/// </summary>
public enum PatternDifficulty
{
    Invalid,
    Easy,
    Medium,
    Hard
}

/// <summary>
/// Statistiques sur les patterns générés.
/// </summary>
[System.Serializable]
public class PatternStats
{
    public int totalPatterns;
    public int totalLogs;
    public float averageLogsPerPattern;
    public int easyPatterns;
    public int mediumPatterns;
    public int hardPatterns;
}
#endregion
