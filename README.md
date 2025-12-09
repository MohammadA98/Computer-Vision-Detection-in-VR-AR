# Computer Vision Detection in VR/AR 🎮🔍🎨

A comprehensive suite of VR/AR computer vision projects featuring machine learning-powered sketch recognition and object detection. This monorepo contains both backend FastAPI services and Unity VR frontend applications for Meta Quest.

## 📋 Table of Contents

- [Overview](#overview)
- [Projects](#projects)
  - [1. QuickDraw Sketch Recognition API](#1-quickdraw-sketch-recognition-api)
  - [2. VR 3D Object Detector API](#2-vr-3d-object-detector-api)
  - [3. QuickDraw VR Unity App](#3-quickdraw-vr-unity-app)
  - [4. XR Object Detection Demo](#4-xr-object-detection-demo)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Contributors](#contributors)
- [License](#license)

## 🎯 Overview

This repository contains four interconnected projects that demonstrate computer vision capabilities in VR/AR environments:

**Backend APIs** (Python/FastAPI):

- **QuickDraw API**: CNN-based sketch recognition trained on Google's QuickDraw dataset
- **YOLO Object Detector API**: Real-time object detection using YOLOv8

**Frontend Apps** (Unity/C#):

- **QuickDraw VR**: Interactive drawing game with ML recognition for Meta Quest
- **XR Object Detection Demo**: Frame capture and object detection prototype

## 🚀 Projects

### 1. QuickDraw Sketch Recognition API

**Location**: `Augmented-Reality--Image-Detector-Final-Project-/`

FastAPI-based REST API for real-time sketch recognition, designed for VR/AR drawing applications. Uses a CNN model trained on Google's QuickDraw dataset.

#### Features

- ✏️ Recognizes **55 common drawing objects** (animals, buildings, vehicles, nature, shapes, tools)
- 🔌 RESTful API with base64 image support (perfect for VR)
- 🐳 Docker support for easy deployment
- 📊 ~95% training accuracy, ~90% validation accuracy
- ⚡ ~20-50ms inference time

#### Quick Start

```bash
cd Augmented-Reality--Image-Detector-Final-Project-

# Docker (Recommended)
docker-compose up -d

# Or Local Python
pip install -r requirements.txt
python Model-Training.py  # First time only (10-30 min)
python main.py
```

**API Endpoints**:

- `GET /health` - Health check
- `GET /classes` - List supported classes
- `POST /predict` - Upload image file
- `POST /predict/base64` - Send base64 image (recommended for VR)

**Documentation**: http://localhost:8000/docs

#### Supported Classes (55)

**Animals**: cat, dog, bird, fish, bear, butterfly, bee, spider  
**Buildings**: house, castle, barn, bridge, lighthouse, church  
**Transportation**: car, airplane, bicycle, boat, train, truck, bus  
**Nature**: tree, flower, sun, moon, cloud, mountain, river  
**Objects**: apple, banana, book, chair, table, cup, umbrella  
**Body Parts**: face, eye, hand, foot  
**Shapes**: circle, triangle, square, star, heart  
**Tools**: sword, axe, hammer, key, crown

_Easily extendable to 345+ classes from Google QuickDraw dataset!_

---

### 2. VR 3D Object Detector API

**Location**: `vr-3d-object-detector/`

YOLO-based object detection API for Unity VR screenshots. Detects objects in virtual environments in real-time.

#### Features

- 🎯 Detects **80 COCO classes** (cars, people, traffic signs, animals, furniture, etc.)
- 🔥 YOLOv8 Nano model (fast inference ~50-200ms)
- 🐳 Docker support with docker-compose
- 📦 Base64 image input for seamless Unity integration
- 🎨 Returns detected objects text with confidence scores

#### Quick Start

```bash
cd vr-3d-object-detector

# Docker (Recommended)
docker-compose up -d --build

# Or Local Python
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

**API Endpoints**:

- `GET /health` - Health check
- `POST /detect` - Object detection from base64 image

#### Object Detection Classes (80)

**Vehicles**: car, truck, bus, motorcycle, bicycle, train, boat, airplane  
**People**: person  
**Traffic**: traffic light, fire hydrant, stop sign, parking meter  
**Animals**: bird, cat, dog, horse, sheep, cow, elephant, bear, zebra, giraffe  
**Accessories**: backpack, umbrella, handbag, tie, suitcase  
**Sports**: frisbee, skis, snowboard, sports ball, kite, baseball bat, skateboard, surfboard, tennis racket  
**Food**: banana, apple, sandwich, orange, pizza, donut, cake  
**Furniture**: chair, couch, bed, dining table, toilet  
**Electronics**: tv, laptop, mouse, keyboard, cell phone

_Plus 40+ more everyday objects!_

#### Model Options

- `yolov8n.pt` - Nano (fastest, current default)
- `yolov8s.pt` - Small (more accurate)
- `yolov8m.pt` - Medium (most accurate)

---

### 3. QuickDraw VR Unity App

**Location**: `QuickDraw/`

A Unity-based VR drawing game for Meta Quest with real-time ML sketch recognition.

#### Features

- 🎨 **VR Drawing**: Draw in 3D space using Quest controllers
- 🧠 **Real-time ML Recognition**: Instant predictions as you draw
- 🎯 **49 Drawing Categories**: Wide variety of recognizable objects
- 🎮 **Game Mode**: Guess the drawing challenges
- 📊 **Live Feedback**: See prediction confidence scores
- ☁️ **Cloud API**: Powered by Hugging Face Spaces

#### Technology Stack

- Unity 6000.2.10f1 with URP
- Meta Quest SDK (OVR/OpenXR)
- FastAPI ML prediction service
- C# + UnityWebRequest for API communication

#### Setup

1. **Clone and Open**:

```bash
git clone git@github.com:MohammadA98/QuickDraw-VR.git
cd QuickDraw-VR
```

2. **Open in Unity Hub** (Unity 6000.2.10f1)

3. **Configure API** (optional):

   - Open `SampleScene.unity`
   - Select `GameController` GameObject
   - Update `Python URL` in Inspector (default uses Hugging Face)

4. **Build for Quest**:
   - Connect Quest via USB
   - Enable Developer Mode
   - File → Build Settings → Android → Build and Run

#### How It Works

1. Draw with right controller trigger
2. 4-second initial delay before predictions start
3. Screenshots captured every 2 seconds
4. Base64-encoded PNG sent to ML API
5. Top-3 predictions displayed with confidence scores
6. Win when any top-3 prediction matches target word!

#### Game Controls

- **Right Trigger**: Draw
- **Y Button**: Skip word
- **X Button**: Clear drawing

---

### 4. XR Object Detection Demo

**Location**: `XRObjectDetectionDemo/`

Unity XR prototype for frame capture and real-time object detection using YOLO API.

#### Features

- 📸 **Screenshot Capture**: Automatic or manual frame capture
- 🔍 **YOLO Integration**: Sends frames to YOLO API for detection
- 🎯 **In-VR Display**: Shows detection results with confidence scores
- 🎮 **Multiple Inputs**: Keyboard (C), Mouse, Right Trigger, A Button
- 💾 **Dataset Builder**: Optional disk saving for custom datasets
- 🖼️ **UI Overlay**: Simple detection message display

#### Requirements

**Unity**:

- Unity 2022.3 LTS+
- XR Interaction Toolkit
- Oculus XR Plugin or OpenXR Plugin
- TextMeshPro

**Backend**:

- YOLO FastAPI server (from `vr-3d-object-detector/`)

#### Setup

1. **Clone and Open**:

```bash
git clone https://github.com/issaennab/XRObjectDetectionDemo.git
cd XRObjectDetectionDemo
```

2. **Open in Unity Hub**

3. **Configure Scene**:

   - Open `Assets/SampleScene.unity`
   - Select `ScreenshotCapture` GameObject
   - Assign **Capture Camera** (XR Main Camera)
   - Set **API URL** (e.g., `http://localhost:8000/detect`)

4. **Start YOLO Backend**:

```bash
cd ../vr-3d-object-detector
docker-compose up -d --build
```

5. **Build for Quest** or **Play in Editor**

#### Controls

- **Editor**: Press **C** key to capture
- **Quest**: Right Trigger or A Button to capture
- Results shown in Console and UI overlay

---

## 🚀 Quick Start

### Prerequisites

- **Docker Desktop** (recommended for APIs)
- **Python 3.10+** (for local development)
- **Unity 6000.2.10f1** or Unity 2022.3 LTS (for VR apps)
- **Meta Quest** headset (for VR deployment)

### Start All Backend Services

```bash
# Terminal 1: QuickDraw API
cd Augmented-Reality--Image-Detector-Final-Project-
docker-compose up -d

# Terminal 2: YOLO Object Detector API
cd vr-3d-object-detector
docker-compose up -d --build

# Verify both APIs are running
curl http://localhost:8000/health  # QuickDraw API
curl http://localhost:8001/health  # YOLO API (if on different port)
```

### Run Unity Projects

1. Open **Unity Hub**
2. Add projects: `QuickDraw/` and `XRObjectDetectionDemo/`
3. Open either project
4. Configure API endpoints in Inspector
5. Build to Quest or test in Editor

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Meta Quest VR Headset                  │
│                                                             │
│  ┌─────────────────┐          ┌─────────────────┐         │
│  │  QuickDraw VR   │          │   XR Detection  │         │
│  │   Unity App     │          │   Demo (Unity)  │         │
│  │                 │          │                 │         │
│  │ • Draw in 3D    │          │ • Frame Capture │         │
│  │ • Game Mode     │          │ • Live Detect   │         │
│  │ • UI Feedback   │          │ • Dataset Build │         │
│  └────────┬────────┘          └────────┬────────┘         │
└───────────┼──────────────────────────┼──────────────────────┘
            │                          │
            │ Base64 PNG               │ Base64 JPG
            │ via UnityWebRequest      │ via UnityWebRequest
            │                          │
            ▼                          ▼
┌─────────────────────┐    ┌─────────────────────┐
│  QuickDraw Sketch   │    │   YOLO Object       │
│  Recognition API    │    │   Detector API      │
│  (FastAPI/Python)   │    │  (FastAPI/Python)   │
│                     │    │                     │
│ • CNN Model         │    │ • YOLOv8 Nano       │
│ • 55 Classes        │    │ • 80 COCO Classes   │
│ • Google QuickDraw  │    │ • Bounding Boxes    │
│ • Port 8000         │    │ • Port 8000/8001    │
└─────────────────────┘    └─────────────────────┘
```

### Technology Stack

**Backend**:

- Python 3.10+
- FastAPI
- TensorFlow/Keras (CNN)
- Ultralytics YOLOv8
- Docker & Docker Compose
- Uvicorn

**Frontend**:

- Unity 6000.2.10f1 / 2022.3 LTS
- Universal Render Pipeline (URP)
- Meta Quest SDK (OVR)
- XR Interaction Toolkit
- C# / .NET

**ML/CV**:

- Google QuickDraw Dataset
- COCO Dataset
- Custom CNN Architecture
- YOLOv8 Object Detection

---

## 📊 Performance

### QuickDraw API

- **Training Accuracy**: ~95%
- **Validation Accuracy**: ~90%
- **Inference Time**: 20-50ms per image
- **Model Size**: ~10MB

### YOLO Object Detector API

- **Model**: YOLOv8 Nano (~6MB)
- **Inference Time**: 50-200ms (CPU, 1920x1080)
- **Confidence Threshold**: 0.4
- **Classes**: 80 COCO objects

### Unity VR Apps

- **QuickDraw VR**:
  - Initial delay: 4 seconds
  - Prediction interval: 2 seconds
  - Prediction cycle: 0.5 seconds
- **XR Detection Demo**:
  - Manual/trigger-based capture
  - Real-time detection feedback
  - Optional frame saving

---

## 🛠️ Development

### Project Structure

```
Computer-Vision-Detection-in-VR-AR/
├── Augmented-Reality--Image-Detector-Final-Project-/  # QuickDraw API
│   ├── main.py                    # FastAPI app
│   ├── model.py                   # CNN inference
│   ├── Model-Training.py          # Training script
│   ├── requirements.txt
│   └── docker-compose.yml
├── vr-3d-object-detector/         # YOLO Object Detector API
│   ├── app/
│   │   ├── main.py               # FastAPI app
│   │   ├── detector.py           # YOLO inference
│   │   └── models.py
│   ├── requirements.txt
│   └── docker-compose.yml
├── QuickDraw/                     # QuickDraw VR Unity App
│   ├── Assets/
│   │   ├── Scripts/
│   │   │   ├── BotAPI.cs         # API communication
│   │   │   ├── GameController.cs # Game logic
│   │   │   └── DrawController.cs # VR input
│   │   └── Scenes/
│   └── ProjectSettings/
└── XRObjectDetectionDemo/         # XR Detection Unity App
    ├── Assets/
    │   ├── Scripts/
    │   │   ├── ScreenshotCapture.cs  # Capture & API
    │   │   └── TestRequest.cs        # Testing
    │   └── Scenes/
    └── ProjectSettings/
```

### Testing APIs

**QuickDraw API**:

```bash
# Test health
curl http://localhost:8000/health

# Test prediction with base64
curl -X POST "http://localhost:8000/predict/base64" \
  -H "Content-Type: application/json" \
  -d '{"image_base64": "data:image/png;base64,...", "top_k": 3}'
```

**YOLO API**:

```bash
# Test health
curl http://localhost:8000/health

# Test detection
base64 -i test_image.jpg | tr -d '\n' > image.txt
curl -X POST "http://localhost:8000/detect" \
  -H "Content-Type: application/json" \
  -d "{\"image_base64\": \"$(cat image.txt)\"}"
```

### Docker Management

```bash
# View logs
docker-compose logs -f

# Restart services
docker-compose restart

# Stop services
docker-compose down

# Rebuild after changes
docker-compose up -d --build

# Clean everything
docker-compose down -v
```

---

## 🐛 Troubleshooting

### API Issues

**Port Already in Use**:

```bash
# macOS/Linux
lsof -ti:8000 | xargs kill -9

# Windows
netstat -ano | findstr :8000
taskkill /PID <PID> /F
```

**Model Not Found**:

```bash
# QuickDraw API - train model first
cd Augmented-Reality--Image-Detector-Final-Project-
python Model-Training.py

# YOLO API - model downloads automatically on first run
cd vr-3d-object-detector
python -c "from ultralytics import YOLO; YOLO('yolov8n.pt')"
```

**Docker Not Starting**:

1. Ensure Docker Desktop is running
2. Check: `docker info`
3. Restart Docker Desktop
4. Verify: `docker ps`

### Unity Issues

**API Connection Fails**:

- Verify API is running: `curl http://localhost:8000/health`
- Use PC's local IP instead of `localhost` when running on Quest
- Check firewall allows port 8000/8001
- For Quest: ensure WiFi connection

**Build Errors**:

- Verify Unity version matches requirements
- Check XR Plugin Management settings
- Enable Developer Mode on Quest
- For Android builds: install Android SDK/NDK

**Low Prediction Confidence**:

- Ensure drawings are clear and centered
- Wait for full drawing before prediction
- Check screenshot quality in API logs
- Verify camera view captures drawing area

**Quest Deployment Issues**:

```bash
# Verify Quest connection
adb devices

# For local API development
adb reverse tcp:8000 tcp:8000

# View Unity logs
adb logcat -s Unity
```

---

## 📝 Configuration

### API Ports

- **QuickDraw API**: Default `8000` (configurable in `docker-compose.yml`)
- **YOLO API**: Default `8000` (or `8001` if running both locally)

### CORS Settings

Both APIs allow all origins by default. For production:

```python
# main.py
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://your-domain.com"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)
```

### Unity API Endpoints

Update in Unity Inspector:

- **QuickDraw VR**: `GameController` → `BotAPI` → `Python URL`
- **XR Detection**: `ScreenshotCapture` → `API URL`

---

## 🎓 Training Custom Models

### QuickDraw API - Add More Classes

Edit `Model-Training.py`:

```python
CLASS_NAMES = ["house", "cat", "dog", "car", "tree", "sun", "flower"]
MAX_ITEMS_PER_CLASS = 2000  # Samples per class
```

Train:

```bash
cd Augmented-Reality--Image-Detector-Final-Project-
python Model-Training.py  # Takes 10-30 minutes
```

### YOLO API - Use Different Model

Edit `app/main.py`:

```python
detector = YOLODetector(
    model_name="yolov8s.pt",  # s=small, m=medium, l=large, x=xlarge
    confidence_threshold=0.4
)
```

---

## 📚 Documentation Links

- **Interactive API Docs**:
  - QuickDraw: http://localhost:8000/docs
  - YOLO: http://localhost:8000/docs
- **Google QuickDraw Dataset**: https://github.com/googlecreativelab/quickdraw-dataset
- **YOLOv8 Docs**: https://docs.ultralytics.com/
- **Unity XR Toolkit**: https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest
- **Meta Quest SDK**: https://developer.oculus.com/documentation/unity/

---

## 👥 Contributors

- **Beakal Zekaryas** - [@BeakalZekaryas](https://github.com/BeakalZekaryas)
- **Issa Ennab** - [@issaennab](https://github.com/issaennab)
- **Mohammad Alhabli** - [@MohammadA98](https://github.com/MohammadA98)

---

## 📄 License

MIT License - See individual project directories for details.

This project uses:

- **Google QuickDraw Dataset** - Creative Commons Attribution 4.0 International License
- **COCO Dataset** - Creative Commons Attribution 4.0 License
- **YOLOv8** - AGPL-3.0 License (Ultralytics)

---

## 🙏 Acknowledgments

- Google Creative Lab for the [QuickDraw dataset](https://github.com/googlecreativelab/quickdraw-dataset)
- Ultralytics for [YOLOv8](https://github.com/ultralytics/ultralytics)
- FastAPI framework
- TensorFlow/Keras team
- Unity Technologies
- Meta Quest development team
- Hugging Face Spaces for hosting

---

## 🚀 Future Enhancements

- [ ] Custom YOLO training for VR-specific objects
- [ ] Real-time continuous detection (streaming)
- [ ] 3D bounding box visualization in Unity
- [ ] Multi-user VR collaboration
- [ ] Hand tracking integration
- [ ] Voice commands for control
- [ ] Performance optimization for Quest 2/3
- [ ] Dataset recording/labeling tools
- [ ] Local inference using ONNX Runtime
- [ ] Authentication for production APIs

---

**Built with ❤️ for VR/AR Computer Vision** 🎮🔍🎨
