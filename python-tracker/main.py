import cv2
import socket
import json
import mediapipe as mp
from cvzone.HandTrackingModule import HandDetector

### --- PARAMÈTRES --- ###
CONFIG = {
    "WIDTH": 1280,
    "HEIGHT": 720,
    "CAM_INDEX": 0,
    "FPS": 60,
    "DETECTION_CONFIDENCE": 0.6,
    "MAX_HANDS": 1,
    "UDP_IP": "127.0.0.1",
    "UDP_PORT": 5052,
    "DEBUG": True
}

# --- Initialisation webcam ---
cap = cv2.VideoCapture(CONFIG["CAM_INDEX"])
cap.set(3, CONFIG["WIDTH"])
cap.set(4, CONFIG["HEIGHT"])
cap.set(5, CONFIG["FPS"])

# --- Initialisation détection main et pose ---
detector = HandDetector(detectionCon=CONFIG["DETECTION_CONFIDENCE"], maxHands=CONFIG["MAX_HANDS"])
mp_pose = mp.solutions.pose
pose = mp_pose.Pose(min_detection_confidence=CONFIG["DETECTION_CONFIDENCE"])
mp_draw = mp.solutions.drawing_utils

# --- Socket UDP ---
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
serverAddressPort = (CONFIG["UDP_IP"], CONFIG["UDP_PORT"])

# --- Fonctions de détection de gestes ---
def is_fist(lmList):
    finger_tips = [4, 8, 12, 16, 20]
    finger_pip = [2, 6, 10, 14, 18]
    return all(lmList[finger_tips[i]][1] > lmList[finger_pip[i]][1] for i in range(1, 5))

def is_index_up(lmList):
    finger_tips = [4, 8, 12, 16, 20]
    finger_pip = [2, 6, 10, 14, 18]
    return lmList[finger_tips[1]][1] < lmList[finger_pip[1]][1] and all(
        lmList[finger_tips[i]][1] > lmList[finger_pip[i]][1] for i in range(2, 5)
    )

# --- Boucle principale ---
while True:
    success, img = cap.read()
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    hands, img = detector.findHands(img)
    results_pose = pose.process(img_rgb)

    data = {
        "hand_positions": [],
        "pose_landmarks": [],
        "gesture": "hand_open"
    }

    # --- Mains ---
    if hands:
        hand = hands[0]
        lmList = hand["lmList"]

        if is_fist(lmList):
            data["gesture"] = "hand_close"
        elif is_index_up(lmList):
            data["gesture"] = "index_up"

        for lm in lmList:
            x, y, z = lm[0], CONFIG["HEIGHT"] - lm[1], lm[2] * 2
            data["hand_positions"].append([x, y, z])

    # --- Pose (corps) ---
    if results_pose.pose_landmarks:
        for id, lm in enumerate(results_pose.pose_landmarks.landmark):
            x, y, z = int(lm.x * CONFIG["WIDTH"]), int(lm.y * CONFIG["HEIGHT"]), lm.z
            data["pose_landmarks"].append([x, CONFIG["HEIGHT"] - y, z])

        if CONFIG["DEBUG"]:
            mp_draw.draw_landmarks(img, results_pose.pose_landmarks, mp_pose.POSE_CONNECTIONS)

    # --- Envoi UDP ---
    sock.sendto(json.dumps(data).encode(), serverAddressPort)

    # --- Debug / Affichage ---
    if CONFIG["DEBUG"]:
        cv2.putText(img, data["gesture"], (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2)
        cv2.imshow("Image", img)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
sock.close()
