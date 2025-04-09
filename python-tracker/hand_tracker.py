from cvzone.HandTrackingModule import HandDetector

# Cette classe est responsable de la détection des gestes de la main et du comptage des doigts ouverts.
class HandTracker:
    def __init__(self, detection_confidence, max_hands):
        self.detector = HandDetector(detectionCon=detection_confidence, maxHands=max_hands)
    
    # Cette méthode détecte les gestes de la main en fonction des positions des points de repère de la main.
    # Elle renvoie un geste spécifique en fonction de la position des doigts.
    # Les gestes possibles sont "hand_close", "index_up" et "hand_open".
    def detect_gestures(self, lmList):
        def is_fist():
            finger_tips = [4, 8, 12, 16, 20]
            finger_pip = [2, 6, 10, 14, 18]
            return all(lmList[finger_tips[i]][1] > lmList[finger_pip[i]][1] for i in range(1, 5))

        def is_index_up():
            finger_tips = [4, 8, 12, 16, 20]
            finger_pip = [2, 6, 10, 14, 18]
            return lmList[finger_tips[1]][1] < lmList[finger_pip[1]][1] and all(
                lmList[finger_tips[i]][1] > lmList[finger_pip[i]][1] for i in range(2, 5)
            )
        
        if is_fist():
            return "hand_close"
        elif is_index_up():
            return "index_up"
        return "hand_open"

    # Cette méthode compte le nombre de doigts ouverts en fonction des positions des points de repère de la main.
    # Elle renvoie le nombre de doigts ouverts.
    def count_open_fingers(self, lmList):
        finger_tips = [4, 8, 12, 16, 20]
        finger_pip = [2, 6, 10, 14, 18]
        open_fingers = 0

        for i in range(1, 5):  
            if lmList[finger_tips[i]][1] < lmList[finger_pip[i]][1]:
                open_fingers += 1

        # Compte le pouce séparément
        if lmList[finger_tips[0]][1] < lmList[finger_pip[0]][1] and lmList[finger_tips[0]][1] < lmList[3][1]:
            open_fingers += 1

        return open_fingers
