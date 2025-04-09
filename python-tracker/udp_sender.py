import socket
import json

# Classe pour envoyer des données au serveur UDP
class UDPSender:
    def __init__(self, ip, port):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.server_address = (ip, port)

    # Envoie les données au serveur UDP
    # Les données doivent être au format JSON
    def send(self, data):
        self.sock.sendto(json.dumps(data).encode(), self.server_address)

    # Ferme le socket UDP
    def close(self):
        self.sock.close()
