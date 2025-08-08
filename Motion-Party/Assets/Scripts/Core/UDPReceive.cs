using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core
{
    public class UDPReceive : MonoBehaviour
    {
        private static UDPReceive _instance;
        public static UDPReceive Instance => _instance;

        private Thread receiveThread;
        private UdpClient client;
        public int port = 5052;
        public bool startRecieving = true;
        public bool printToConsole = false;
        public string data;
        private bool isShuttingDown = false;
        private bool hasReceivedData = false;
        private int dataReceivedCount = 0;
        private bool shouldNotifyLoadingScreen = false;

        private void Awake()
        {
            // Singleton pattern avec gestion des changements de scène
            if (_instance != null && _instance != this)
            {
                // Si l'instance existante est sur une autre scène, la remplacer
                if (_instance.gameObject.scene != this.gameObject.scene)
                {
                    Debug.Log($"[UDPReceive] Changement de scène détecté - Transfert de {_instance.gameObject.scene.name} vers {this.gameObject.scene.name}");
                    
                    // Arrêter l'ancienne instance
                    _instance.StopReceiving();
                    _instance = this;
                }
                else
                {
                    // Même scène, détruire le doublon
                    Debug.LogWarning("[UDPReceive] Instance dupliquée dans la même scène - Destruction");
                    Destroy(this.gameObject);
                    return;
                }
            }
            else
            {
                _instance = this;
            }
            
            Debug.Log($"[UDPReceive] Instance active sur la scène: {this.gameObject.scene.name}");
        }

        public void Start()
        {
            if (receiveThread == null)
            {
                receiveThread = new Thread(new ThreadStart(ReceiveData));
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
        }

        private void Update()
        {
            // Vérifier s'il faut notifier l'écran de chargement (depuis le thread principal)
            if (shouldNotifyLoadingScreen)
            {
                shouldNotifyLoadingScreen = false;
                
                SimpleLoadingScreen loadingScreen = FindObjectOfType<SimpleLoadingScreen>();
                if (loadingScreen != null)
                {
                    loadingScreen.HideLoadingScreen();
                }
            }
        }

        private void OnDestroy()
        {
            StopReceiving();
            if (_instance == this)
                _instance = null;
        }
        
        /// <summary>
        /// Arrêter la réception UDP proprement
        /// </summary>
        public void StopReceiving()
        {
            isShuttingDown = true;
            startRecieving = false;
            
            if (client != null)
            {
                try { client.Close(); } catch { }
                client = null;
            }
            
            if (receiveThread != null && receiveThread.IsAlive)
            {
                try { receiveThread.Join(100); } catch { }
                receiveThread = null;
            }
            
            Debug.Log($"[UDPReceive] Arrêt de la réception UDP sur {gameObject.scene.name}");
        }

        // receive thread
        private void ReceiveData()
        {
            try
            {
                client = new UdpClient(port);
            }
            catch (Exception err)
            {
                print($"UDPReceive: Erreur lors de la création du socket: {err}");
                return;
            }
            while (startRecieving && !isShuttingDown)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] dataByte = client.Receive(ref anyIP);
                    data = Encoding.UTF8.GetString(dataByte);
                    
                    // Première donnée reçue
                    if (!hasReceivedData)
                    {
                        hasReceivedData = true;
                        shouldNotifyLoadingScreen = true;
                        print($"[UDPReceive] Première donnée reçue! Le jeu est prêt.");
                    }
                    
                    dataReceivedCount++;
                    
                    if (printToConsole) 
                    { 
                        print($"[UDPReceive] #{dataReceivedCount}: {data}"); 
                    }
                }
                catch (SocketException se)
                {
                    if (!isShuttingDown)
                        print($"UDPReceive: SocketException: {se}");
                }
                catch (Exception err)
                {
                    if (!isShuttingDown)
                        print($"UDPReceive: Exception: {err}");
                }
            }
        }
        
        // Propriétés publiques pour vérification
        public bool HasReceivedData => hasReceivedData;
        public int DataReceivedCount => dataReceivedCount;
        
        // === MÉTHODES STATIQUES POUR ACCÈS FACILE DEPUIS LES MINI-JEUX ===
        
        /// <summary>
        /// Obtenir les données UDP actuelles depuis n'importe quel script
        /// </summary>
        public static string GetData()
        {
            if (Instance != null)
                return Instance.data;
            return "";
        }
        
        /// <summary>
        /// Vérifier si des données UDP ont été reçues
        /// </summary>
        public static bool IsDataAvailable()
        {
            return Instance != null && Instance.hasReceivedData && !string.IsNullOrEmpty(Instance.data);
        }
        
        /// <summary>
        /// Obtenir le nombre de paquets UDP reçus
        /// </summary>
        public static int GetPacketCount()
        {
            return Instance != null ? Instance.dataReceivedCount : 0;
        }
        
        /// <summary>
        /// Vérifier si le système UDP est prêt et fonctionnel
        /// </summary>
        public static bool IsReady()
        {
            return Instance != null && Instance.startRecieving;
        }
        
        /// <summary>
        /// Obtenir une référence à l'instance (pour accès avancé)
        /// </summary>
        public static UDPReceive GetInstance()
        {
            return Instance;
        }
    }
}
