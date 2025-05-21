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
    def count_open_fingers(self, lm_list):
        finger_tips = [4, 8, 12, 16, 20]
        finger_pip = [2, 6, 10, 14, 18]
        open_fingers = 0

        # Index, majeur, annulaire, auriculaire
        for i in range(1, 5):
            if lm_list[finger_tips[i]][1] < lm_list[finger_pip[i]][1]:
                open_fingers += 1

        # Pour le pouce, on ne le compte pas si le point 4 est dans le polygone paume élargi
        # Polygone : 0, 5, 9, 13, 17, 0, 7
        palm_poly = [lm_list[i][:2] for i in [0, 5, 9, 13, 17, 0, 7]]
        thumb_tip = lm_list[4][:2]

        def point_in_poly(pt, poly):
            x, y = pt
            n = len(poly)
            inside = False
            px1, py1 = poly[0]
            for i in range(n+1):
                px2, py2 = poly[i % n]
                if min(py1, py2) < y <= max(py1, py2) and x <= max(px1, px2):
                    if py1 != py2:
                        xinters = (y - py1) * (px2 - px1) / (py2 - py1 + 1e-6) + px1
                    if px1 == px2 or x <= xinters:
                        inside = not inside
                px1, py1 = px2, py2
            return inside

        if not point_in_poly(thumb_tip, palm_poly):
            # Le pouce est ouvert si la pointe n'est pas dans la paume
            if lm_list[finger_tips[0]][1] < lm_list[finger_pip[0]][1] and lm_list[finger_tips[0]][1] < lm_list[3][1]:
                open_fingers += 1

        return open_fingers