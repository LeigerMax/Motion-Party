import os
import cv2
import threading
import queue
import time
import logging
from hand_tracker import HandTracker
from pose_tracker import PoseTracker
from udp_sender import UDPSender
from object_identifier import ObjectIdentifier
from config import CONFIG

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def video_capture_thread(cap, frame_queue, stop_event):
    while not stop_event.is_set():
        success, img = cap.read()
        if not success:
            continue
        # Frame dropping intelligent
        if frame_queue.full():
            try:
                frame_queue.get_nowait()
            except queue.Empty:
                pass
        frame_queue.put(img)


def udp_sender_thread(udp_sender, data_queue, stop_event):
    while not stop_event.is_set():
        try:
            data = data_queue.get(timeout=0.1)
            try:
                udp_sender.send(data)
            except Exception as e:
                logger.error(f"Erreur d'envoi UDP : {e}")
        except queue.Empty:
            continue

def initialize_components():
    cap = cv2.VideoCapture(CONFIG["CAM_INDEX"])
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, CONFIG["WIDTH"])
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, CONFIG["HEIGHT"])
    cap.set(cv2.CAP_PROP_FPS, CONFIG["FPS"])

    hand_tracker = HandTracker(CONFIG["DETECTION_CONFIDENCE"], CONFIG["MAX_HANDS"])
    pose_tracker = PoseTracker(CONFIG["DETECTION_CONFIDENCE"])
    udp_sender = UDPSender(CONFIG["UDP_IP"], CONFIG["UDP_PORT"])
    object_identifier = ObjectIdentifier()

    return cap, hand_tracker, pose_tracker, udp_sender, object_identifier

def main():
    cap, hand_tracker, pose_tracker, udp_sender, object_identifier = initialize_components()
    frame_queue = queue.Queue(maxsize=2)
    data_queue = queue.Queue(maxsize=2)
    stop_event = threading.Event()

    # Threads
    t_video = threading.Thread(target=video_capture_thread, args=(cap, frame_queue, stop_event))
    t_udp = threading.Thread(target=udp_sender_thread, args=(udp_sender, data_queue, stop_event))
    t_video.start()
    t_udp.start()

    prev_time = time.time()
    
    try:
        while True:
            if frame_queue.empty():
                time.sleep(0.01)
                continue
            img = frame_queue.get()
            hands, img = hand_tracker.detector.findHands(img)
            results_pose = pose_tracker.track_pose(img)

            data = {
                "hand_positions": [],
                "pose_landmarks": [],
                "gesture": "hand_open",
                "open_fingers": 0
            }

            # Détection des mains
            if hands:
                hand = hands[0]
                lmList = hand["lmList"]
                data["gesture"] = hand_tracker.detect_gestures(lmList)
                data["open_fingers"] = hand_tracker.count_open_fingers(lmList)
                for lm in lmList:
                    # Inverser la coordonnée X pour corriger l'effet miroir (CONFIG["WIDTH"] - lm[0])
                    x, y, z = CONFIG["WIDTH"] - lm[0], CONFIG["HEIGHT"] - lm[1], lm[2] * 2
                    data["hand_positions"].append([x, y, z])

            # Détection de la pose
            if results_pose and results_pose.pose_landmarks:
                pose_tracker.draw_pose_landmarks(img, results_pose)
                for id, lm in enumerate(results_pose.pose_landmarks.landmark):
                    x, y, z = int(lm.x * CONFIG["WIDTH"]), int(lm.y * CONFIG["HEIGHT"]), lm.z
                    data["pose_landmarks"].append([x, CONFIG["HEIGHT"] - y, z])

            # Identification de l'objet
            detected_object = object_identifier.identify(img)
            if detected_object:
                data["object"] = detected_object

            # Envoi des données par UDP (thread séparé)
            if not data_queue.full():
                data_queue.put(data)

            # Affichage des données
            if CONFIG["DEBUG"]:
                # FPS
                curr_time = time.time()
                fps = 1 / (curr_time - prev_time)
                prev_time = curr_time
                cv2.putText(img, f"FPS: {int(fps)}", (50, 200), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 255), 2)

                cv2.putText(img, f"Open Fingers: {data['open_fingers']}", (50, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2)
                cv2.putText(img, data["gesture"], (50, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2)
                if detected_object:
                    cv2.putText(img, f"Detected: {detected_object}", (50, 150),
                                cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
                cv2.imshow("Image", img)

            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
    finally:
        stop_event.set()
        t_video.join()
        t_udp.join()
        cap.release()
        cv2.destroyAllWindows()
        udp_sender.close()

if __name__ == "__main__":
    main()