using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;

namespace Core
{
    /// <summary>
    /// Version optimisée du PythonLauncher pour les builds de production
    /// </summary>
    public class PythonBuildLauncher : MonoBehaviour
    {
        [Header("Build Configuration")]
        [SerializeField] private bool enableInBuildOnly = true;
        [SerializeField] private bool hideConsoleWindow = true;
        [SerializeField] private float startupDelay = 1.0f;
        
        private Process pythonProcess;
        private static bool pythonLaunched = false;

        private void Awake()
        {
            // Ne fonctionne que dans les builds, pas dans l'éditeur
            if (enableInBuildOnly && Application.isEditor)
            {
                Destroy(gameObject);
                return;
            }

            // S'assurer qu'il n'y a qu'une instance
            if (pythonLaunched)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!pythonLaunched)
            {
                Invoke(nameof(LaunchPython), startupDelay);
            }
        }

        private void LaunchPython()
        {
            try
            {
                // Dans un build, chercher le dossier python-tracker à côté de l'exécutable
                string executablePath = Application.dataPath;
                string buildDirectory = Directory.GetParent(executablePath).FullName;
                string pythonTrackerPath = Path.Combine(buildDirectory, "python-tracker");
                string mainPyPath = Path.Combine(pythonTrackerPath, "main.py");

                UnityEngine.Debug.Log($"[PythonBuildLauncher] Recherche de Python dans : {mainPyPath}");

                if (!File.Exists(mainPyPath))
                {
                    UnityEngine.Debug.LogError($"[PythonBuildLauncher] main.py non trouvé dans : {mainPyPath}");
                    
                    // Essayer avec le chemin relatif depuis l'exécutable
                    string altPath = Path.Combine(buildDirectory, "..", "python-tracker", "main.py");
                    altPath = Path.GetFullPath(altPath);
                    
                    if (File.Exists(altPath))
                    {
                        mainPyPath = altPath;
                        pythonTrackerPath = Path.GetDirectoryName(altPath);
                        UnityEngine.Debug.Log($"[PythonBuildLauncher] Trouvé à : {mainPyPath}");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"[PythonBuildLauncher] main.py non trouvé non plus dans : {altPath}");
                        return;
                    }
                }

                // Vérifier si Python est installé
                string pythonCmd = "python";
                if (!IsPythonInstalled(pythonCmd))
                {
                    pythonCmd = "py"; // Essayer avec py (Python Launcher sur Windows)
                    if (!IsPythonInstalled(pythonCmd))
                    {
                        UnityEngine.Debug.LogError("[PythonBuildLauncher] Python n'est pas installé ou n'est pas dans le PATH.");
                        return;
                    }
                }

                // Configuration du processus
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pythonCmd,
                    Arguments = $"\"{mainPyPath}\"",
                    WorkingDirectory = pythonTrackerPath,
                    UseShellExecute = false,
                    CreateNoWindow = hideConsoleWindow,
                    RedirectStandardOutput = hideConsoleWindow,
                    RedirectStandardError = hideConsoleWindow
                };

                // Démarrer Python
                pythonProcess = Process.Start(startInfo);
                pythonLaunched = true;

                UnityEngine.Debug.Log($"[PythonBuildLauncher] Python tracker démarré avec succès! PID: {pythonProcess.Id}");

                // Vérification que le processus fonctionne
                if (pythonProcess.HasExited)
                {
                    UnityEngine.Debug.LogError("[PythonBuildLauncher] Le processus Python s'est arrêté immédiatement. Vérifiez les dépendances.");
                    pythonLaunched = false;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[PythonBuildLauncher] Erreur lors du démarrage de Python : {e.Message}");
                pythonLaunched = false;
            }
        }

        private bool IsPythonInstalled(string command)
        {
            try
            {
                ProcessStartInfo testInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = "--version",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process testProcess = Process.Start(testInfo))
                {
                    testProcess.WaitForExit(3000);
                    return testProcess.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void OnApplicationQuit()
        {
            StopPython();
        }

        private void OnDestroy()
        {
            StopPython();
        }

        private void StopPython()
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                try
                {
                    pythonProcess.Kill();
                    pythonProcess.WaitForExit(2000);
                    UnityEngine.Debug.Log("[PythonBuildLauncher] Processus Python arrêté.");
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[PythonBuildLauncher] Erreur lors de l'arrêt : {e.Message}");
                }
            }
        }
    }
}
