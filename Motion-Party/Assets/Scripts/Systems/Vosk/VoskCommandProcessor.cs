using UnityEngine;
using System;
using System.Collections.Generic;

public class VoskCommandProcessor : MonoBehaviour
{
    public VoskSpeechToText VoskSpeechToText;

    // Liste des commandes et synonymes attendus
    private Dictionary<string, Action> commandActions;
    private List<string> repeatRulesSynonyms = new List<string>
    {
        "répète les règles", "redis les règles", "rappelle les règles", "rappelle moi les règles", "peux tu répéter les règles", "peux-tu répéter les règles", "répète règles"
    };

    void Awake()
    {
        commandActions = new Dictionary<string, Action>
        {
            { "répète les règles", RepeatRules }
            // Ajoute ici d'autres commandes et leurs actions associées
        };

        if (VoskSpeechToText != null)
            VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
    }

    private void OnTranscriptionResult(string json)
    {
        var result = new RecognitionResult(json);
        foreach (var phrase in result.Phrases)
        {
            string userText = phrase.Text.ToLower().Trim();
            string matchedCommand = FindClosestCommand(userText);

            if (!string.IsNullOrEmpty(matchedCommand) && commandActions.ContainsKey(matchedCommand))
            {
                commandActions[matchedCommand]?.Invoke();
                Debug.Log($"Commande reconnue : {matchedCommand} (via '{userText}')");
                // On ne fait pas return ici, on continue pour traiter toutes les alternatives
                break; // On sort de la boucle dès qu'une commande est trouvée
            }
        }
    }

    // Recherche la commande la plus proche avec la distance de Levenshtein
    private string FindClosestCommand(string userText)
    {
        int minDistance = int.MaxValue;
        string bestMatch = null;
        userText = userText.Trim();

        foreach (string synonym in repeatRulesSynonyms)
        {
            int dist = LevenshteinDistance(userText, synonym.Trim());
            if (dist < 3 && dist < minDistance) // tolère 2 erreurs max
            {
                minDistance = dist;
                bestMatch = "répète les règles";
            }
        }

        // Ajoute ici d'autres groupes de synonymes si besoin

        return bestMatch;
    }

    // Distance de Levenshtein 
    private int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b.Length;
        if (string.IsNullOrEmpty(b)) return a.Length;

        int[,] d = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) d[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }
        return d[a.Length, b.Length];
    }

    // Action à déclencher
    private void RepeatRules()
    {
        Debug.Log("Action : Répéter les règles du jeu !");
        // Ici tu peux appeler une méthode de ton jeu pour afficher ou lire les règles
    }
}