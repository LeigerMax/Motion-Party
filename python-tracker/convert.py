import numpy as np
import cv2
import os

# Chemin absolu vers le fichier .npy
base_dir = os.path.dirname(__file__)
npy_path = os.path.join(base_dir, "quickdraw_data", "apple.npy")

if not os.path.exists(npy_path):
    raise FileNotFoundError(f"Fichier introuvable : {npy_path}")

# Charger les données
drawings = np.load(npy_path)

# Créer le dossier de sortie
output_dir = os.path.join(base_dir, "reference_drawings")
os.makedirs(output_dir, exist_ok=True)

# Exporter les 5 premiers dessins
for i in range(200):
    img = drawings[i].reshape(28, 28).astype(np.uint8)
    # Inverser les couleurs : 0 = noir, 255 = blanc
    img = 255 - img
    img = cv2.resize(img, (300, 300), interpolation=cv2.INTER_NEAREST)
    cv2.imwrite(os.path.join(output_dir, f"apple_{i}.png"), img)

print("✅ Dessins exportés dans reference_drawings/")