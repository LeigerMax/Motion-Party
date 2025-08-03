# Motion Party 🎮

A real-time motion tracking party game that combines computer vision with Unity gameplay. Players use hand gestures and body movements captured via webcam to interact with various mini-games in an immersive party experience.

## 📋 Overview

Motion Party is an interactive gaming system that consists of two main components:
- **Python Motion Tracker**: A computer vision system that tracks hand gestures, body poses, and objects in real-time
- **Unity Game Engine**: A collection of mini-games that respond to the tracked motion data

The system uses UDP communication to send tracking data from the Python tracker to Unity, enabling real-time interaction between physical movements and digital gameplay.

## 🎯 Features

### Motion Tracking
- **Hand Gesture Recognition**: Detects hand poses including open hand, closed fist, and index finger pointing
- **Pose Tracking**: Full body pose estimation and tracking
- **Object Detection**: Recognition of common objects like balls, bottles, and fruits
- **Real-time Processing**: Optimized for low-latency gaming experience

### Mini-Games
- **Log Parade**: Navigate through obstacles using body movements
- **Firefly Dance**: Create magical patterns with hand gestures
- **Music Note**: Musical interaction through motion
- **Balloons**: Pop balloons with precise hand movements
- **Demo Scenes**: Various test and demonstration scenarios

## 🛠️ Tech Stack

### Python Components
- **OpenCV**: Computer vision and image processing
- **CVZone**: Simplified computer vision operations
- **NumPy**: Numerical computations
- **MediaPipe**: Hand and pose tracking (via CVZone)
- **Socket Programming**: UDP communication

### Unity Components
- **Unity 6000.0.54f1**: Game engine
- **C# Scripts**: Game logic and motion data processing
- **MediaPipe Unity Integration**: Additional pose tracking capabilities
- **Custom Shaders**: Enhanced visual effects

## 📦 Installation

### Prerequisites
- **Python 3.7+**
- **Unity Hub** and **Unity 6000.0.54f1**
- **Webcam** (for motion tracking)
- **Windows** (primary development platform)

### Python Tracker Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/LeigerMax/Motion-Party.git
   cd Motion-Party
   ```

2. **Navigate to the Python tracker directory**
   ```bash
   cd python-tracker
   ```

3. **Install Python dependencies**
   ```bash
   pip install -r requirements.txt
   ```

4. **Configure settings** (optional)
   Edit `config.py` to adjust:
   - Camera settings (resolution, FPS)
   - Detection confidence levels
   - UDP communication settings
   - Object detection parameters

### Unity Project Setup

1. **Open Unity Hub**
2. **Add the project** by selecting the `Motion-Party` folder
3. **Open with Unity 6000.0.54f1**
4. **Wait for project compilation** (first time may take several minutes)

## 🚀 Usage

### Starting the Motion Tracker

1. **Run the Python tracker**
   ```bash
   cd python-tracker
   python main.py
   ```

2. **Verify camera feed** opens and tracking is working
3. **Check console** for UDP connection status

### Running Unity Games

1. **Open Unity project**
2. **Select a scene** from `Assets/Scenes/MiniGames/`
3. **Press Play** to start the game
4. **Ensure Python tracker is running** for motion input

### Available Test Scenes
- `Demo.unity`: Basic functionality demonstration
- `Test_S2D_O3D_NumberFinger.unity`: Finger counting test
- `Test_S2D_O3D_GrabAndMove.unity`: Object manipulation test
- `Test_S2D_O3D_Destroy.unity`: Object destruction test

## ⚙️ Configuration

### Python Tracker Settings (`config.py`)
```python
CONFIG = {
    "WIDTH": 640,              # Camera resolution width
    "HEIGHT": 480,             # Camera resolution height
    "CAM_INDEX": 0,            # Camera device index
    "FPS": 30,                 # Target frame rate
    "DETECTION_CONFIDENCE": 0.6, # Detection confidence threshold
    "MAX_HANDS": 1,            # Maximum hands to track
    "UDP_IP": "127.0.0.1",     # Unity communication IP
    "UDP_PORT": 5052,          # Unity communication port
}
```

### Unity Communication
The Unity project listens on port `5052` for incoming motion data. Ensure this port is available and not blocked by firewall.

## 🎮 Game Controls

### Hand Gestures
- **Open Hand**: Selection and interaction
- **Closed Fist**: Grabbing objects
- **Index Finger**: Pointing and precise selection

### Body Movements
- **Pose Tracking**: Full body movement detection
- **Position Mapping**: Real-world coordinates to game space

## 🏗️ Project Structure

```
Motion-Party/
├── python-tracker/           # Motion tracking system
│   ├── main.py              # Main application entry
│   ├── hand_tracker.py      # Hand gesture detection
│   ├── pose_tracker.py      # Body pose tracking
│   ├── object_identifier.py # Object detection
│   ├── udp_sender.py        # Unity communication
│   ├── config.py            # Configuration settings
│   └── requirements.txt     # Python dependencies
├── Motion-Party/            # Unity project (nested structure)
│   ├── Assets/              # Unity assets
│   │   ├── Scenes/          # Game scenes
│   │   ├── Scripts/         # C# game scripts
│   │   └── ...              # Other Unity assets
│   └── ProjectSettings/     # Unity project settings
└── README.md               # This file
```

## 🐛 Troubleshooting

### Common Issues

**Camera not detected**
- Check camera permissions
- Verify `CAM_INDEX` in config.py
- Ensure no other applications are using the camera

**Unity not receiving data**
- Verify Python tracker is running
- Check UDP port 5052 is available
- Confirm IP address settings match

**Poor tracking performance**
- Adjust `DETECTION_CONFIDENCE` in config.py
- Ensure adequate lighting
- Position camera for optimal view

**Frame rate issues**
- Lower camera resolution in config.py
- Reduce `MAX_HANDS` if not needed
- Close unnecessary background applications

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

## 📄 License

This project is available under the [MIT License](LICENSE).

## 👨‍💻 Author

**LeigerMax**
- GitHub: [@LeigerMax](https://github.com/LeigerMax)
- Project: [Motion-Party](https://github.com/LeigerMax/Motion-Party)

## 🙏 Acknowledgments

- **OpenCV Community** for computer vision tools
- **MediaPipe Team** for hand and pose tracking
- **Unity Technologies** for the game engine
- **CVZone** for simplified computer vision operations

## 🔄 Version History

- **Current Branch**: RECOVER
- **Development**: Active
- **Status**: Alpha

---

*Ready to party with motion? Get moving and start playing! 🎉*
