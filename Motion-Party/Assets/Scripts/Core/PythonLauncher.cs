using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;

namespace Core
{
    public class PythonLauncher : MonoBehaviour
    {
        [Header("Python Configuration")]
        [SerializeField] private bool launchPythonOnStart = true;
        [SerializeField] private bool closePythonOnExit = true;
        [SerializeField] private string pythonExecutable = "python";
        [SerializeField] private string pythonScriptPath = "python-tracker/main.py";
        
        private Process pythonProcess;
        private bool pythonStarted = false;

        private void Awake()
        {
            // S'assurer qu'il n'y a qu'une instance
            PythonLauncher[] launchers = FindObjectsOfType<PythonLauncher>();
            if (launchers.Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (launchPythonOnStart)
            {
                LaunchPython();
            }
        }

        public void LaunchPython()
        {
            if (pythonStarted)
            {
                UnityEngine.Debug.LogWarning("[PythonLauncher] Python est déjà en cours d'exécution.");
                return;
            }

            try
            {
                // Construire le chemin complet vers le script Python
                string applicationPath = Application.dataPath;
                string projectRoot = Directory.GetParent(applicationPath).Parent?.FullName;
                string fullPythonPath = Path.Combine(projectRoot, pythonScriptPath);

                if (!File.Exists(fullPythonPath))
                {
                    string errorMsg = $"Script Python non trouvé : {fullPythonPath}";
                    UnityEngine.Debug.LogError($"[PythonLauncher] {errorMsg}");
                    return;
                }

                // Configuration du processus Python
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pythonExecutable,
                    Arguments = $"\"{fullPythonPath}\"",
                    WorkingDirectory = Path.GetDirectoryName(fullPythonPath),
                    UseShellExecute = false,
                    CreateNoWindow = false, // Changez à true pour masquer la fenêtre
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                // Démarrer le processus Python
                pythonProcess = Process.Start(startInfo);
                pythonStarted = true;

                UnityEngine.Debug.Log($"[PythonLauncher] Python démarré avec succès. PID: {pythonProcess.Id}");
                
                // Vérifier que le processus est toujours en cours
                if (pythonProcess.HasExited)
                {
                    string errorMsg = "Le processus Python s'est arrêté immédiatement.";
                    UnityEngine.Debug.LogError($"[PythonLauncher] {errorMsg}");
                    pythonStarted = false;
                }
                else
                {
                    UnityEngine.Debug.Log($"[PythonLauncher] Python est en cours d'exécution.");
                }
            }
            catch (Exception e)
            {
                string errorMsg = $"Erreur lors du démarrage de Python : {e.Message}";
                UnityEngine.Debug.LogError($"[PythonLauncher] {errorMsg}");
                pythonStarted = false;
            }
        }

        public void StopPython()
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                try
                {
                    pythonProcess.Kill();
                    pythonProcess.WaitForExit(3000); // Attendre 3 secondes maximum
                    UnityEngine.Debug.Log("[PythonLauncher] Processus Python arrêté.");
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[PythonLauncher] Erreur lors de l'arrêt de Python : {e.Message}");
                }
            }
            pythonStarted = false;
        }

        public bool IsPythonRunning()
        {
            return pythonProcess != null && !pythonProcess.HasExited && pythonStarted;
        }

        private void OnApplicationQuit()
        {
            if (closePythonOnExit)
            {
                StopPython();
            }
        }

        private void OnDestroy()
        {
            if (closePythonOnExit)
            {
                StopPython();
            }
        }
    }
}
