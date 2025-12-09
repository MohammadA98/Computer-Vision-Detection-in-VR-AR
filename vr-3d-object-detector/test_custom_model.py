"""
Test custom model detection on training images
"""
from ultralytics import YOLO
from PIL import Image
import sys

# Load the custom model
model_path = "runs/train/vr-custom-simple2/weights/best.pt"
print(f"Loading custom model: {model_path}")
model = YOLO(model_path)

print(f"Model classes: {model.names}")
print(f"Number of classes: {len(model.names)}")
print()

# Test on a training image
image_path = sys.argv[1] if len(sys.argv) > 1 else "Labeled-Images.v1-2025-12.yolov8/train/images/Screenshot-2025-12-06-at-10_54_56-PM_png.rf.022feb429c7c424bbff0d6e46bbdb53e.jpg"

print(f"Testing on image: {image_path}")
image = Image.open(image_path)
print(f"Image size: {image.width}x{image.height}")
print()

# Run detection with different confidence thresholds
for conf in [0.1, 0.25, 0.4, 0.6]:
    results = model(image, conf=conf, verbose=False)
    detections = []
    
    for result in results:
        boxes = result.boxes
        for box in boxes:
            x1, y1, x2, y2 = box.xyxy[0].tolist()
            confidence = float(box.conf[0])
            cls = int(box.cls[0])
            class_name = model.names[cls]
            detections.append({
                "class": class_name,
                "confidence": confidence
            })
    
    print(f"Confidence={conf}: {len(detections)} detections")
    for det in detections:
        print(f"  - {det['class']}: {det['confidence']:.3f}")
    print()

# Also test with very low confidence to see what the model "sees"
print("=" * 60)
print("Testing with VERY LOW confidence (0.01) to see everything:")
results = model(image, conf=0.01, verbose=False)
for result in results:
    boxes = result.boxes
    print(f"Total raw detections: {len(boxes)}")
    for box in boxes:
        confidence = float(box.conf[0])
        cls = int(box.cls[0])
        class_name = model.names[cls]
        print(f"  - {class_name}: {confidence:.4f}")
