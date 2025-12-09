# QuickDraw VR

A Unity-based VR drawing application for Meta Quest that uses machine learning to recognize hand-drawn sketches in real-time.

## Features

- **VR Drawing**: Draw in 3D space using Meta Quest controllers
- **Real-time ML Recognition**: Instant predictions of your drawings using a trained neural network
- **49 Drawing Categories**: Recognizes a wide variety of objects including animals, vehicles, everyday items, and more
- **Live Feedback**: See prediction confidence scores as you draw

## Technology Stack

- **Unity 6000.2.10f1** with Universal Render Pipeline (URP)
- **Meta Quest SDK** (OVR/OpenXR) for VR interaction
- **FastAPI** machine learning prediction service
- **Hugging Face Spaces** for API deployment
- **C# + UnityWebRequest** for API communication

## Project Structure

```
Assets/
├── Scripts/
│   ├── BotAPI.cs          # Handles ML API communication
│   ├── GameController.cs  # Main game loop and prediction management
│   ├── BotBrain.cs        # Stores prediction state
│   └── DrawController.cs  # VR drawing input handling
├── Scenes/
│   └── SampleScene.unity  # Main VR scene
└── Materials/
    └── GlowBrush.mat      # Drawing trail material
```

## Setup

### Prerequisites

- Unity 6000.2.10f1 or later
- Meta Quest headset
- Meta Quest SDK packages (included)

### Installation

1. Clone the repository:

```bash
git clone git@github.com:MohammadA98/QuickDraw-VR.git
cd QuickDraw-VR
```

2. Open the project in Unity:

   - Launch Unity Hub
   - Click "Add" and select the project directory
   - Open with Unity 6000.2.10f1

3. Configure the API endpoint (if not using default):
   - Open `SampleScene.unity`
   - Select the `GameController` GameObject
   - In the Inspector, find the `BotAPI` component
   - Update the `Python URL` field to your API endpoint (default: `https://issa-ennab-quickdraw-api.hf.space/predict/base64`)

### Building for Quest

1. Connect your Quest headset via USB
2. Enable Developer Mode on your Quest
3. In Unity: **File → Build Settings**
4. Select **Android** platform
5. Click **Build and Run**

## How It Works

1. **Drawing**: Use the right controller trigger to draw in 3D space
2. **Initial Delay**: 4-second delay before starting predictions (gives time to draw)
3. **Screenshot Capture**: Every 2 seconds, the app captures the current headset view
4. **API Call**: The screenshot is converted to PNG, base64 encoded, and sent to the ML API
5. **Prediction**: The API returns top-3 predictions with confidence scores
6. **Display**: Cycles through top-3 predictions every 0.5 seconds for better visibility
7. **Win Condition**: If any of the top-3 predictions matches the target word, you win!

## ML API

The project uses a FastAPI-based prediction service deployed on Hugging Face Spaces:

- **Endpoint**: `https://issa-ennab-quickdraw-api.hf.space/predict/base64`
- **Method**: POST
- **Input**: JSON with base64-encoded PNG image
- **Output**: JSON with top_k predictions and confidence scores

## Configuration

### Prediction Timing

The game uses optimized timing for better UX:

- **Initial Delay**: 4 seconds (gives time to start drawing)
- **Prediction Interval**: 2 seconds (API call frequency)
- **Cycle Interval**: 0.5 seconds (how fast predictions rotate)

To adjust these values:

1. Open `SampleScene.unity`
2. Select the `ControlPanel` GameObject
3. In the Inspector, find the `Game Controller (Script)` component
4. Modify the timing values:
   - `Delay Before Predictions`: Initial waiting time
   - `Prediction Interval`: Time between API calls
   - `Prediction Cycle Interval`: Display rotation speed

### Supported Categories (49 classes)

cat, dog, bird, fish, bear, butterfly, spider, house, castle, barn, bridge, lighthouse, church, car, airplane, bicycle, truck, train, tree, flower, sun, moon, cloud, mountain, apple, banana, book, chair, table, cup, umbrella, face, eye, hand, foot, circle, triangle, square, star, sword, axe, hammer, key, crown, guitar, piano

## Game Controls

- **Right Trigger**: Draw in 3D space
- **Y Button**: Skip current word and start new round
- **B Button**: Clear the drawing surface
- **Left Thumbstick**: Move around (if locomotion enabled)

## Development

### Key Scripts

**BotAPI.cs**

- Handles HTTP communication with ML API
- Converts screenshots to base64
- Parses JSON predictions
- Includes verbose logging for debugging

**GameController.cs**

- Implements singleton pattern to prevent duplicate instances
- Manages game state and prediction timing
- Captures screenshots every 2 seconds after 4-second initial delay
- Cycles through top-3 predictions for display
- Checks all top-3 predictions for win condition (not just #1)
- Updates UI with real-time feedback
- Handles VR controller shortcuts (Y=Skip, B=Clear)

**DrawController.cs**

- Handles VR controller input
- Manages TrailRenderer for drawing
- Uses OVR input system

## Troubleshooting

### Duplicate GameController Instances

If you see duplicate predictions or timing issues:

- Check that only **one** GameObject has `Game Controller (Script)` enabled
- The singleton pattern will log warnings if duplicates are detected
- Look for `[SINGLETON]` and `[CRITICAL]` messages in logs

### Checking Logs on Quest

```bash
adb logcat -s Unity | grep "SINGLETON\|TIMING\|ROUND"
```

This shows:

- Singleton initialization
- Prediction timing intervals
- Round start events

### API not connecting

- **Verify API endpoint**: Open `SampleScene.unity`, select the `BotAPI` GameObject, check the `Python URL` field in Inspector (should be `https://issa-ennab-quickdraw-api.hf.space/predict/base64`)
- **Check Quest internet**: Ensure your Quest headset is connected to WiFi
- **Review Unity logs**: Filter Console by `>>>` to see API request/response logs
- **Check Hugging Face status**: Visit the API URL in a browser to verify it's online

### Low prediction confidence scores

If predictions are always low (< 10%):

- **Drawing quality**: Try drawing larger, clearer shapes
- **Camera view**: Ensure drawings are centered and visible in the headset view
- **Screenshot timing**: Wait for the full drawing before the first prediction (4-second delay)
- **Check received images**: API logs save images to `api_logs/received_images/` - verify they show your drawings clearly

### "Non-secure network connections disabled" error

- Go to **Edit → Project Settings → Player → Android → Other Settings**
- Under **Configuration**, set "Allow downloads over HTTP" to **"Always allowed"**
- This is required because the HTTPS API may redirect through HTTP

### USB Connection Issues

If building via USB:

```bash
adb devices  # Verify device is connected
adb reverse tcp:8001 tcp:8001  # For local development
```

## License

MIT

## Contributors

- Beakal Zekaryas
- Issa-Ennab
- Mohammad Alhabli
