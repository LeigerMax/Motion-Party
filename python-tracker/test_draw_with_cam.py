import cv2
import numpy as np
import os
from skimage.metrics import structural_similarity as ssim
import random

CANVAS_SIZE = 300
SIMILARITY_THRESHOLD = 0.78

OBJECTS = ["apple", "car"]
chosen_object = random.choice(OBJECTS)

def extract_features(img):
    moments = cv2.moments(img)
    hu_moments = cv2.HuMoments(moments).flatten()
    return hu_moments

def preprocess(img):
    blur = cv2.GaussianBlur(img, (5,5), 0)
    _, thresh = cv2.threshold(blur, 180, 255, cv2.THRESH_BINARY_INV)
    return thresh

def is_drawing_sufficient(img, min_black_pixels=800):
    black_pixels = np.sum(img < 200)
    return black_pixels > min_black_pixels

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
    refs = {}
    base_dir = os.path.dirname(__file__)
    abs_path = os.path.join(base_dir, path)
    if not os.path.exists(abs_path):
        print(f"⚠️  Le dossier '{abs_path}' est introuvable. Place tes images de référence dans ce dossier.")
        return refs
    for filename in os.listdir(abs_path):
        if filename.endswith(".png"):
            label = os.path.splitext(filename)[0]
            img = cv2.imread(os.path.join(abs_path, filename), cv2.IMREAD_GRAYSCALE)
            img = cv2.resize(img, (CANVAS_SIZE, CANVAS_SIZE))
            refs[label] = img
    return refs

def find_best_match(user_drawing, references, chosen_object):
    if not references:
        return None, 0.0
    scores = {}
    for label, ref_img in references.items():
        if not label.startswith(chosen_object):
            continue
        user_drawing_blur = cv2.GaussianBlur(user_drawing, (3, 3), 0)
        ref_img_blur = cv2.GaussianBlur(ref_img, (3, 3), 0)
        score, _ = ssim(user_drawing_blur, ref_img_blur, full=True)
        scores[label] = score
    if not scores:
        return None, 0.0
    best_label = max(scores, key=scores.get)
    return best_label, scores[best_label]

def delete_reference_images(label, path="reference_drawings"):
    base_dir = os.path.dirname(__file__)
    abs_path = os.path.join(base_dir, path)
    deleted = 0
    for filename in os.listdir(abs_path):
        if filename.startswith(label) and filename.endswith(".png"):
            os.remove(os.path.join(abs_path, filename))
            deleted += 1
    print(f"🗑️ {deleted} images '{label}' supprimées de {abs_path}")

print(f"📝 Dessine un(e) {chosen_object} sur une feuille blanche et montre-le à la caméra !")
generate_reference_images(label=chosen_object, n=500)
reference_images = load_reference_images()

cap = cv2.VideoCapture(0)
print(f"� Place ton dessin de {chosen_object} devant la caméra. Appuie sur 'v' pour valider, 'q' pour quitter.")

validated = False
while not validated:
    ret, frame = cap.read()
    if not ret:
        print("Erreur caméra.")
        break

    # Convertir en niveaux de gris et redimensionner
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    # Optionnel : crop ou détection automatique de la feuille ici
    user_input = cv2.resize(gray, (CANVAS_SIZE, CANVAS_SIZE))
    user_input = preprocess(user_input)

    cv2.imshow("Camera", frame)
    cv2.imshow("Dessin detecte", user_input)

    key = cv2.waitKey(1) & 0xFF
    if key == ord('q'):
        break
    elif key == ord('v'):
        if not is_drawing_sufficient(user_input):
            print("✍️ Dessin trop vide ou mal détecté. Essaie encore !")
            continue

        best_label, confidence = find_best_match(user_input, reference_images, chosen_object)
        print(f"Score de similarité max : {confidence:.2f}")
        if best_label is not None and confidence >= SIMILARITY_THRESHOLD and best_label.startswith(chosen_object):
            print(f"🧠 Bravo ! {chosen_object.capitalize()} reconnu ({best_label}, confiance : {confidence:.2f})")
            cv2.putText(frame, f"{chosen_object.capitalize()} valide !", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0,255,0), 2)
            cv2.imshow("Camera", frame)
            cv2.waitKey(1500)
            delete_reference_images(label=chosen_object)
            validated = True
        else:
            print("❌ Essaie encore !")

cap.release()
cv2.destroyAllWindows()