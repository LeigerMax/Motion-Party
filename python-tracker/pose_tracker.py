import cv2
import mediapipe as mp

# Classe pour détecter et suivre les landmarks de pose humaine à l'aide de MediaPipe.
class PoseTracker:

    def __init__(self, detection_confidence):
        self.pose = mp.solutions.pose.Pose(min_detection_confidence=detection_confidence)
        self.mp_draw = mp.solutions.drawing_utils

    # Méthode pour détecter la pose humaine dans une image donnée.
    # Elle retourne les résultats de la détection.
    def track_pose(self, img):
        img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)  
        results = self.pose.process(img_rgb) 
        return results

    # Dessine les repères de la pose humaine sur l'image donnée si la pose est détectée.
    def draw_pose_landmarks(self, img, results):
        if results.pose_landmarks:
            self.mp_draw.draw_landmarks(img, results.pose_landmarks, mp.solutions.pose.POSE_CONNECTIONS)