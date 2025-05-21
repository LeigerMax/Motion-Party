# Configuration globale
CONFIG = {
    "WIDTH": 640,
    "HEIGHT": 480,
    "CAM_INDEX": 0,
    "FPS": 30,
    "DETECTION_CONFIDENCE": 0.6,
    "MAX_HANDS": 1,
    "UDP_IP": "127.0.0.1",
    "UDP_PORT": 5052,
    "DEBUG": True,
    "OBJECT_MODEL_PATH": "python-tracker/Models/efficientdet_lite0.tflite",
    "ALLOWED_OBJECTS": ["person", "bottle", "cup", "banana", "apple", "sports ball"] 
}