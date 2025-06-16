import cv2
import numpy as np
import os
from skimage.metrics import structural_similarity as ssim
import random

DRAWING = False
LAST_POS = None
CANVAS_SIZE = 300
BRUSH_SIZE = 8
WHITE = 255
SIMILARITY_THRESHOLD = 0.92
WINDOW_NAME = "Dessine ici"

OBJECTS = ["apple", "car"]
chosen_object = random.choice(OBJECTS)

canvas = np.ones((CANVAS_SIZE, CANVAS_SIZE), dtype=np.uint8) * WHITE

def preprocess(img):
    blur = cv2.GaussianBlur(img, (5,5), 0)
    _, thresh = cv2.threshold(blur, 180, 255, cv2.THRESH_BINARY_INV)
    return thresh

def crop_and_center(img):
    # Trouve le bounding box du dessin et centre-le dans le canvas
    coords = cv2.findNonZero(255 - img)
    if coords is not None:
        x, y, w, h = cv2.boundingRect(coords)
        cropped = img[y:y+h, x:x+w]
        result = np.ones((CANVAS_SIZE, CANVAS_SIZE), dtype=np.uint8) * 255
        scale = min((CANVAS_SIZE-20)/w, (CANVAS_SIZE-20)/h)
        new_w, new_h = int(w*scale), int(h*scale)
        resized = cv2.resize(cropped, (new_w, new_h), interpolation=cv2.INTER_NEAREST)
        x_offset = (CANVAS_SIZE - new_w)//2
        y_offset = (CANVAS_SIZE - new_h)//2
        result[y_offset:y_offset+new_h, x_offset:x_offset+new_w] = resized
        return result
    return img

def is_drawing_sufficient(img, min_black_pixels=800):
    black_pixels = np.sum(img < 200)
    print(f"[LOG] Pixels noirs détectés : {black_pixels} (min requis : {min_black_pixels})")
    return black_pixels > min_black_pixels

def hu_similarity(img1, img2):
    moments1 = cv2.moments(img1)
    moments2 = cv2.moments(img2)
    hu1 = cv2.HuMoments(moments1).flatten()
    hu2 = cv2.HuMoments(moments2).flatten()
    hu1_log = -np.sign(hu1) * np.log10(np.abs(hu1) + 1e-10)
    hu2_log = -np.sign(hu2) * np.log10(np.abs(hu2) + 1e-10)
    dist = np.linalg.norm(hu1_log - hu2_log)
    sim = np.exp(-dist)
    print(f"[LOG] Hu moments user: {np.round(hu1_log, 3)}")
    print(f"[LOG] Hu moments ref : {np.round(hu2_log, 3)}")
    print(f"[LOG] Distance Hu     : {dist:.4f} => Similarité Hu: {sim:.4f}")
    return sim

def best_aligned_score(user_img, ref_img):
    best_score = 0
    best_params = {}
    user_bin = preprocess(user_img)
    ref_bin = preprocess(ref_img)
    user_bin = crop_and_center(user_bin)
    ref_bin = crop_and_center(ref_bin)
    print("[LOG] Début de l'alignement multi-échelle et translation...")
    for scale in np.linspace(0.85, 1.15, 5):  # moins de tolérance à l'échelle
        scaled = cv2.resize(user_bin, None, fx=scale, fy=scale, interpolation=cv2.INTER_NEAREST)
        h, w = scaled.shape
        if h > CANVAS_SIZE or w > CANVAS_SIZE:
            scaled = scaled[0:CANVAS_SIZE, 0:CANVAS_SIZE]
        else:
            pad_y = CANVAS_SIZE - h
            pad_x = CANVAS_SIZE - w
            scaled = cv2.copyMakeBorder(scaled, pad_y//2, pad_y - pad_y//2, pad_x//2, pad_x - pad_x//2, cv2.BORDER_CONSTANT, value=255)
        for dy in range(-10, 11, 5):  # moins de tolérance au déplacement
            for dx in range(-10, 11, 5):
                M = np.float32([[1, 0, dx], [0, 1, dy]])
                shifted = cv2.warpAffine(scaled, M, (CANVAS_SIZE, CANVAS_SIZE), borderValue=255)
                ssim_score = ssim(shifted, ref_bin)
                hu_score = hu_similarity(shifted, ref_bin)
                score = 0.85 * ssim_score + 0.15 * hu_score
                print(f"[LOG] scale={scale:.2f}, dx={dx}, dy={dy}, SSIM={ssim_score:.4f}, Hu={hu_score:.4f}, Score={score:.4f}")
                if score > best_score:
                    best_score = score
                    best_params = {
                        "scale": scale,
                        "dx": dx,
                        "dy": dy,
                        "ssim": ssim_score,
                        "hu": hu_score
                    }
    print(f"[LOG] Meilleur score trouvé : {best_score:.4f} avec paramètres {best_params}")
    return best_score

def draw(event, x, y, *_):
    global DRAWING, LAST_POS
    if event == cv2.EVENT_LBUTTONDOWN:
        DRAWING = True
        LAST_POS = (x, y)
    elif event == cv2.EVENT_MOUSEMOVE and DRAWING:
        cv2.line(canvas, LAST_POS, (x, y), 0, BRUSH_SIZE)
        LAST_POS = (x, y)
    elif event == cv2.EVENT_LBUTTONUP:
        DRAWING = False

def generate_reference_images(label, n=500):
    base_dir = os.path.dirname(__file__)
    npy_path = os.path.join(base_dir, "quickdraw_data", f"{label}.npy")
    output_dir = os.path.join(base_dir, "reference_drawings")
    os.makedirs(output_dir, exist_ok=True)
    if not os.path.exists(npy_path):
        raise FileNotFoundError(f"Fichier introuvable : {npy_path}")
    drawings = np.load(npy_path)
    for i in range(n):
        img = drawings[i].reshape(28, 28).astype(np.uint8)
        img = 255 - img
        img = cv2.resize(img, (CANVAS_SIZE, CANVAS_SIZE), interpolation=cv2.INTER_NEAREST)
        cv2.imwrite(os.path.join(output_dir, f"{label}_{i}.png"), img)
    print(f"✅ {n} images de '{label}' générées dans {output_dir}")

def load_reference_images(path="reference_drawings"):
    refs = []
    base_dir = os.path.dirname(__file__)
    abs_path = os.path.join(base_dir, path)
    if not os.path.exists(abs_path):
        print(f"⚠️  Le dossier '{abs_path}' est introuvable. Place tes images de référence dans ce dossier.")
        return refs
    for filename in os.listdir(abs_path):
        if filename.endswith(".png") and filename.startswith(chosen_object):
            img = cv2.imread(os.path.join(abs_path, filename), cv2.IMREAD_GRAYSCALE)
            img = cv2.resize(img, (CANVAS_SIZE, CANVAS_SIZE))
            refs.append(img)
    return refs

print(f"Génération des images de référence pour '{chosen_object}'...")
generate_reference_images(label=chosen_object, n=500)
reference_images = load_reference_images()

reference_img = random.choice(reference_images)
cv2.imshow("Dessin à reproduire", reference_img)
print("Reproduis ce dessin le plus fidèlement possible dans la fenêtre de dessin.")

cv2.namedWindow(WINDOW_NAME)
cv2.setMouseCallback(WINDOW_NAME, draw)

print("Appuie sur 'v' pour valider, 'c' pour effacer, 'q' pour quitter.")

validated = False
while not validated:
    cv2.imshow(WINDOW_NAME, canvas)
    key = cv2.waitKey(1) & 0xFF

    if key == ord('q'):
        break
    elif key == ord('c'):
        canvas[:] = WHITE
    elif key == ord('v'):
        user_input = cv2.resize(canvas, (CANVAS_SIZE, CANVAS_SIZE))
        if not is_drawing_sufficient(user_input):
            print("Dessin trop vide. Essaie de faire quelque chose de plus visible !")
            continue

        score = best_aligned_score(user_input, reference_img)
        print(f"Score de fidélité (ajusté) : {score:.4f} (1.00 = identique)")

        cv2.putText(canvas, f"Score : {score:.2f}", (10, 290), cv2.FONT_HERSHEY_SIMPLEX, 0.7, 128, 2)
        cv2.imshow(WINDOW_NAME, canvas)
        cv2.waitKey(2000)
        validated = True

cv2.destroyAllWindows()