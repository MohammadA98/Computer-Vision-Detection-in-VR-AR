"""
Simple training script without TensorBoard integration issues
"""
import os
os.environ['WANDB_MODE'] = 'disabled'  # Disable Weights & Biases

# Monkey patch to disable TensorBoard before ultralytics imports it
import sys
from unittest.mock import MagicMock

# Create a mock module for torch.utils.tensorboard
mock_tensorboard = MagicMock()
sys.modules['torch.utils.tensorboard'] = mock_tensorboard

from ultralytics import YOLO

print("=" * 60)
print("🚀 VR Object Detector - Simple Training")
print("=" * 60)

# Load pre-trained model
print("\n📦 Loading YOLOv8 Nano model...")
model = YOLO("yolov8n.pt")
print("✅ Model loaded!")

# Train
print("\n🏋️  Starting training...")
print("This will take a while on CPU (~20-30 minutes)...")
print("-" * 60)

results = model.train(
    data="./Labeled-Images.v1-2025-12.yolov8/data.yaml",
    epochs=50,
    imgsz=640,
    batch=4,  # Small batch for CPU
    patience=10,
    save=True,
    plots=False,  # Disable plots to avoid issues
    verbose=True,
    project="runs/train",
    name="vr-custom-simple",
)

print("-" * 60)
print("✅ Training complete!")
print(f"\n📊 Best model saved to: runs/train/vr-custom-simple/weights/best.pt")
print(f"📊 Last model saved to: runs/train/vr-custom-simple/weights/last.pt")
