import cv2
import os
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class ObjectIdentifier:
    def __init__(self, images_dir=None, min_match_count=10, distance_threshold=50):
        self.orb = cv2.ORB_create()
        self.bf = cv2.BFMatcher(cv2.NORM_HAMMING, crossCheck=True)
        self.min_match_count = min_match_count
        self.distance_threshold = distance_threshold

        if images_dir is None:
            images_dir = os.path.join(os.path.dirname(__file__), "images")

        if not os.path.exists(images_dir):
            logger.error(f"Le dossier d'images n'existe pas : {images_dir}")
            raise FileNotFoundError(f"Le dossier d'images n'existe pas : {images_dir}")

        self.reference_images = self._load_reference_descriptors(images_dir)

    def _load_reference_descriptors(self, directory):
        descriptors = {}
        for filename in os.listdir(directory):
            filepath = os.path.join(directory, filename)
            img = cv2.imread(filepath, cv2.IMREAD_GRAYSCALE)

            if img is None:
                logger.warning(f"Image illisible : {filename}")
                continue

            _, des = self.orb.detectAndCompute(img, None)
            if des is None or len(des) == 0:
                logger.warning(f"Aucun descripteur détecté dans : {filename}")
                continue

            label = os.path.splitext(filename)[0]
            descriptors[label] = des
        logger.info(f"{len(descriptors)} images de référence chargées.")
        return descriptors

    def _compute_best_match(self, des_frame):
        best_match = None
        best_score = float('inf')
        for label, des_ref in self.reference_images.items():
            if des_ref is None or len(des_ref) == 0:
                continue
            matches = self.bf.match(des_ref, des_frame)
            if len(matches) < self.min_match_count:
                continue
            avg_distance = sum(m.distance for m in matches) / len(matches)
            logger.debug(f"Label: {label}, Avg distance: {avg_distance:.2f}, Matches: {len(matches)}")
            if avg_distance < best_score:
                best_score = avg_distance
                best_match = label
        return best_match, best_score

    def identify(self, frame):
        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
        scales = [1.0, 0.8, 0.6, 0.4, 0.3]
        best_match = None
        best_score = float('inf')

        for scale in scales:
            small = cv2.resize(gray, (0, 0), fx=scale, fy=scale)
            _, des_frame = self.orb.detectAndCompute(small, None)
            if des_frame is None or len(des_frame) == 0:
                logger.debug(f"Aucun descripteur détecté à l'échelle {scale}")
                continue
            match, score = self._compute_best_match(des_frame)
            if match and score < best_score:
                best_score = score
                best_match = match

        if best_score > self.distance_threshold:
            return None
        logger.info(f"Objet reconnu : {best_match} (score: {best_score:.2f})")
        return best_match