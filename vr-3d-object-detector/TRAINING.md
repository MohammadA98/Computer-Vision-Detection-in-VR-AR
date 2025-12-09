# 🎓 Training Your Custom VR Object Detection Model

This guide shows you how to fine-tune YOLOv8 on your custom labeled dataset to detect VR-specific objects while keeping the ability to detect common objects (car, person, truck, etc.).

---

## 📋 Prerequisites

- ✅ Labeled dataset from Roboflow (YOLOv8 format)
- ✅ Python 3.11+ installed
- ✅ GPU recommended (but works on CPU too, just slower)

---

## 🚀 Quick Start

### 1️⃣ Install Training Dependencies

```bash
pip install -r requirements-train.txt
```

### 2️⃣ Verify Your Dataset

Your Roboflow dataset should look like this:

```
Labeled-Images.v1-2025-12.yolov8/
├── data.yaml          # Dataset configuration
├── train/
│   ├── images/        # Training images
│   └── labels/        # Training labels (.txt files)
├── valid/             # Validation set
│   ├── images/
│   └── labels/
└── test/              # Test set (optional)
    ├── images/
    └── labels/
```

Check your classes:

```bash
cat Labeled-Images.v1-2025-12.yolov8/data.yaml
```

You should see:

```yaml
names: ["Barrel", "Construction Pylon", "House", "Toys", "Trees"]
```

### 3️⃣ Train the Model

**Basic training (50 epochs, ~10-15 minutes on GPU):**

```bash
python train.py --data ./Labeled-Images.v1-2025-12.yolov8/data.yaml --epochs 50
```

**Advanced options:**

```bash
# Longer training for better accuracy
python train.py --data ./Labeled-Images.v1-2025-12.yolov8/data.yaml --epochs 100

# Use larger model for better accuracy (slower)
python train.py --data ./Labeled-Images.v1-2025-12.yolov8/data.yaml --model yolov8s.pt --epochs 50

# Reduce batch size if you run out of memory
python train.py --data ./Labeled-Images.v1-2025-12.yolov8/data.yaml --epochs 50 --batch 8
```

### 4️⃣ Monitor Training

Training results are saved in `runs/train/vr-custom/`:

```
runs/train/vr-custom/
├── weights/
│   ├── best.pt        # Best model (use this!)
│   └── last.pt        # Last epoch model
├── results.png        # Training metrics plot
├── confusion_matrix.png
├── F1_curve.png
└── ...
```

**View training progress:**

- Open `runs/train/vr-custom/results.png` to see loss curves
- Lower loss = better training
- Check that validation loss decreases

---

## 🧪 Test Your Model

### Test on validation set:

```bash
python test_model.py \
  --model runs/train/vr-custom/weights/best.pt \
  --data ./Labeled-Images.v1-2025-12.yolov8/data.yaml
```

### Test on a single image:

```bash
python test_model.py \
  --model runs/train/vr-custom/weights/best.pt \
  --image path/to/your/test_image.jpg
```

This will:

- Print detected objects
- Save annotated image as `test_image_detected.jpg`

---

## 🎯 Understanding Results

### Key Metrics:

- **mAP50**: Main accuracy metric (higher = better)

  - > 0.7 = Good
  - > 0.8 = Very good
  - > 0.9 = Excellent

- **Precision**: How many detections were correct
- **Recall**: How many objects were found

### What If Results Are Poor?

1. **Low mAP (< 0.5)?**

   - Add more labeled images
   - Train longer (100-200 epochs)
   - Check that labels are accurate

2. **Model missing objects?**

   - Lower confidence threshold in API
   - Add more examples of missed objects
   - Use larger model (yolov8s.pt)

3. **Too many false positives?**
   - Add more "negative" examples
   - Train longer
   - Increase confidence threshold

---

## 🚀 Deploy Your Model to API

### Option 1: Replace the default model

Copy your trained model to the app directory:

```bash
cp runs/train/vr-custom/weights/best.pt app/vr_custom_model.pt
```

Update `app/main.py`:

```python
# Change this line:
detector = YOLODetector(model_name="yolov8n.pt", confidence_threshold=0.4)

# To this:
detector = YOLODetector(model_name="app/vr_custom_model.pt", confidence_threshold=0.4)
```

### Option 2: Use Docker volume mount

Update `docker-compose.yml`:

```yaml
services:
  vr-detector:
    volumes:
      - ./runs/train/vr-custom/weights/best.pt:/app/custom_model.pt
    environment:
      - MODEL_PATH=/app/custom_model.pt
```

Update `app/main.py`:

```python
import os
model_path = os.getenv("MODEL_PATH", "yolov8n.pt")
detector = YOLODetector(model_name=model_path, confidence_threshold=0.4)
```

### Rebuild and test:

```bash
docker compose down
docker compose up --build
```

Test the API:

```bash
curl http://localhost:8000/health
```

---

## 📊 Your Custom Classes

Your trained model will detect:

**From COCO (pre-trained, kept during fine-tuning):**

- car, truck, person, bicycle, motorcycle, bus, etc. (80 classes)

**Your custom VR classes:**

- Barrel
- Construction Pylon
- House
- Toys
- Trees

Total: **85 classes** (80 COCO + 5 custom)

---

## 🔄 Adding More Data Later

1. Label more images in Roboflow
2. Export new dataset version (YOLOv8 format)
3. Re-run training:

```bash
python train.py --data ./Labeled-Images.v2-2025-12.yolov8/data.yaml --epochs 50
```

4. Test and deploy new model

---

## 💡 Tips for Better Results

### Label Quality:

- ✅ Label ALL objects in each image (not just some)
- ✅ Tight bounding boxes (not too loose)
- ✅ Consistent labeling (same object = same class)

### Dataset Size:

- Minimum: 50 images per class
- Good: 100-200 images per class
- Great: 500+ images per class

### Variety:

- Different lighting conditions
- Different angles
- Different distances
- Different backgrounds

### Data Augmentation:

YOLOv8 automatically applies:

- Rotation, scaling, flipping
- Color adjustments
- Mosaic augmentation

---

## 🐛 Troubleshooting

### Out of Memory Error

```bash
# Reduce batch size
python train.py --data ./data.yaml --batch 8

# Or use smaller image size
python train.py --data ./data.yaml --imgsz 416
```

### Training is too slow

```bash
# Use GPU if available (CUDA)
python train.py --data ./data.yaml --device 0

# Or train with fewer epochs for testing
python train.py --data ./data.yaml --epochs 10
```

### Model not detecting custom objects

1. Check if training completed successfully
2. Verify mAP > 0.5
3. Test with images from validation set first
4. Lower confidence threshold (0.3 or 0.2)

---

## 📚 Next Steps

1. ✅ Train your first model
2. ✅ Test accuracy on validation set
3. ✅ Deploy to API
4. ✅ Test with Unity VR
5. 🔄 Collect more data based on results
6. 🔄 Retrain and improve

---

**Questions?** Check the [Ultralytics YOLOv8 documentation](https://docs.ultralytics.com/)
