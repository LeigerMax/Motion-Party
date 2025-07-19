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

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
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

        private void OnDestroy()
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
            if (_instance == this)
                _instance = null;
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
                    if (printToConsole) { print(data); }
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
    }
}
