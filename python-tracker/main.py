import cv2
from hand_tracker import HandTracker
from pose_tracker import PoseTracker
from udp_sender import UDPSender
from config import CONFIG

# Initialisation des composants
cap = cv2.VideoCapture(CONFIG["CAM_INDEX"])
cap.set(3, CONFIG["WIDTH"])
cap.set(4, CONFIG["HEIGHT"])
cap.set(5, CONFIG["FPS"])

hand_tracker = HandTracker(CONFIG["DETECTION_CONFIDENCE"], CONFIG["MAX_HANDS"])
pose_tracker = PoseTracker(CONFIG["DETECTION_CONFIDENCE"])
udp_sender = UDPSender(CONFIG["UDP_IP"], CONFIG["UDP_PORT"])

while True:
    success, img = cap.read()
    hands, img = hand_tracker.detector.findHands(img)
    results_pose = pose_tracker.track_pose(img)

    data = {
        "hand_positions": [],
        "pose_landmarks": [],
        "gesture": "hand_open",
        "open_fingers": 0
    }

    if hands:
        hand = hands[0]
        lmList = hand["lmList"]
        data["gesture"] = hand_tracker.detect_gestures(lmList)
        data["open_fingers"] = hand_tracker.count_open_fingers(lmList)

        for lm in lmList:
            x, y, z = lm[0], CONFIG["HEIGHT"] - lm[1], lm[2] * 2
            data["hand_positions"].append([x, y, z])

    if results_pose.pose_landmarks:
        pose_tracker.draw_pose_landmarks(img, results_pose)

        for id, lm in enumerate(results_pose.pose_landmarks.landmark):
            x, y, z = int(lm.x * CONFIG["WIDTH"]), int(lm.y * CONFIG["HEIGHT"]), lm.z
            data["pose_landmarks"].append([x, CONFIG["HEIGHT"] - y, z])

    udp_sender.send(data)

    if CONFIG["DEBUG"]:
        cv2.putText(img, f"Open Fingers: {data['open_fingers']}", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2)
        cv2.putText(img, data["gesture"], (50, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2)
        cv2.imshow("Image", img)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
udp_sender.close()
