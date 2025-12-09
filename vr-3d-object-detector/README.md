# VR 3D Object Detector API 🎮🔍

YOLO-based object detection API for Unity VR screenshots. Detects objects in your virtual city (cars, traffic cones, people, etc.) in real-time.

## 🚀 Quick Start

### Prerequisites

- Docker installed and running
- Python 3.11+ (for local development)

### 1️⃣ Build & Run with Docker Compose (Recommended)

```bash
# Build and start the container
docker-compose up --build

# Or run in detached mode
docker-compose up -d --build
```

The API will be available at: **http://localhost:8000**

### 2️⃣ Alternative: Build & Run with Docker

```bash
# Build the image
docker build -t vr-object-detector .

# Run the container
docker run -p 8000:8000 vr-object-detector
```

### 3️⃣ Local Development (Without Docker)

```bash
# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Run the API
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

## 📡 API Endpoints

### Health Check

```bash
GET http://localhost:8000/health
```

**Response:**

```json
{
  "status": "healthy",
  "model_loaded": true,
  "model": "yolov8n.pt"
}
```

### Object Detection

```bash
POST http://localhost:8000/detect
Content-Type: application/json

{
  "image_base64": "<BASE64_STRING>"
}
```

**Response:**

```json
{
  "detections": [
    {
      "class": "car",
      "confidence": 0.92,
      "x1": 123,
      "y1": 45,
      "x2": 350,
      "y2": 200
    },
    {
      "class": "person",
      "confidence": 0.87,
      "x1": 500,
      "y1": 300,
      "x2": 550,
      "y2": 430
    }
  ],
  "image_width": 1920,
  "image_height": 1080
}
```

## 🎯 Configuration

### Model Settings

- **Model**: YOLOv8 Nano (`yolov8n.pt`)
- **Confidence Threshold**: 0.4
- **Classes**: All 80 COCO classes (car, person, truck, traffic light, etc.)
- **Port**: 8000

### Upgrade to Larger Model (Optional)

Edit `app/main.py`:

```python
detector = YOLODetector(model_name="yolov8s.pt", confidence_threshold=0.4)
```

Available models:

- `yolov8n.pt` - Nano (fastest, current)
- `yolov8s.pt` - Small (slower, more accurate)
- `yolov8m.pt` - Medium (slowest, most accurate)

## 🎮 Unity Integration

### Unity C# Example

```csharp
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[Serializable]
public class Detection
{
    public string @class;
    public float confidence;
    public int x1;
    public int y1;
    public int x2;
    public int y2;
}

[Serializable]
public class DetectionResponse
{
    public List<Detection> detections;
    public int image_width;
    public int image_height;
}

[Serializable]
public class DetectionRequest
{
    public string image_base64;
}

public class VRObjectDetector : MonoBehaviour
{
    private string apiUrl = "http://localhost:8000/detect";

    public void CaptureAndDetect()
    {
        StartCoroutine(CaptureScreenshotAndDetect());
    }

    IEnumerator CaptureScreenshotAndDetect()
    {
        // Wait for end of frame to capture
        yield return new WaitForEndOfFrame();

        // Capture screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        // Convert to JPG
        byte[] bytes = screenshot.EncodeToJPG(60);
        string base64 = Convert.ToBase64String(bytes);

        // Clean up texture
        Destroy(screenshot);

        // Send to API
        DetectionRequest request = new DetectionRequest { image_base64 = base64 };
        string jsonData = JsonUtility.ToJson(request);

        UnityWebRequest www = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseText = www.downloadHandler.text;
            DetectionResponse response = JsonUtility.FromJson<DetectionResponse>(responseText);

            Debug.Log($"Detected {response.detections.Count} objects!");

            foreach (var detection in response.detections)
            {
                Debug.Log($"Found {detection.@class} (confidence: {detection.confidence})");
            }
        }
        else
        {
            Debug.LogError($"Error: {www.error}");
        }
    }
}
```

## 🧪 Testing with cURL

```bash
# Test health endpoint
curl http://localhost:8000/health

# Test detection (with a test image)
# First, encode an image to base64:
base64 -i test_image.jpg | tr -d '\n' > image_base64.txt

# Then send the request:
curl -X POST http://localhost:8000/detect \
  -H "Content-Type: application/json" \
  -d "{\"image_base64\": \"$(cat image_base64.txt)\"}"
```

## 🐳 Docker Management

```bash
# View logs
docker-compose logs -f

# Stop container
docker-compose down

# Rebuild after changes
docker-compose up --build

# Remove everything (including volumes)
docker-compose down -v
```

## 📊 Detected Object Classes

The API detects all 80 COCO classes:

- **Vehicles**: car, truck, bus, motorcycle, bicycle, train, boat, airplane
- **People**: person
- **Traffic**: traffic light, fire hydrant, stop sign, parking meter
- **Animals**: bird, cat, dog, horse, sheep, cow, elephant, bear, zebra, giraffe
- **Accessories**: backpack, umbrella, handbag, tie, suitcase
- **Sports**: frisbee, skis, snowboard, sports ball, kite, baseball bat, skateboard, surfboard, tennis racket
- **Kitchen**: bottle, wine glass, cup, fork, knife, spoon, bowl
- **Food**: banana, apple, sandwich, orange, broccoli, carrot, hot dog, pizza, donut, cake
- **Furniture**: chair, couch, potted plant, bed, dining table, toilet
- **Electronics**: tv, laptop, mouse, remote, keyboard, cell phone
- **Appliances**: microwave, oven, toaster, sink, refrigerator
- **Indoor**: book, clock, vase, scissors, teddy bear, hair drier, toothbrush

## 🔧 Troubleshooting

### Port Already in Use

```bash
# Find process using port 8000
lsof -i :8000

# Kill it
kill -9 <PID>
```

### Model Download Issues

The first run will download YOLOv8 weights (~6MB). If it fails:

```bash
# Download manually
python -c "from ultralytics import YOLO; YOLO('yolov8n.pt')"
```

### Container Won't Start

```bash
# Check Docker logs
docker-compose logs vr-detector

# Verify Docker is running
docker ps
```

## 📝 Notes

- **Image Format**: Unity sends JPG @ quality 60 (Base64)
- **Response Time**: ~50-200ms on CPU for 1920x1080 images
- **Coordinates**: Pixel space (same as screenshot size)
- **No Image Return**: Response contains only detection data (lightweight)

## 🚀 Next Steps

1. ✅ Test with Unity VR integration
2. ⚡ Upgrade to YOLOv8s for better accuracy (if needed)
3. 🎯 Add class filtering (e.g., only cars, cones)
4. 📊 Add detection visualization endpoint
5. 🔒 Add authentication for production deployment

---

**Built for Unity VR Object Detection** 🎮🔍
