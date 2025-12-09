"""
Training script for fine-tuning YOLOv8 on custom VR object detection dataset.

This script:
1. Loads pre-trained YOLOv8 model (keeps COCO knowledge)
2. Fine-tunes on your custom labeled dataset (adds new classes)
3. Saves the trained model for deployment

Usage:
    python train.py --data ./Labeled-Images.v1-2025-12.yolov8/data.yaml --epochs 50
"""

import argparse
import os
from pathlib import Path
from ultralytics import YOLO


def train_model(
    data_yaml: str,
    model: str = "yolov8n.pt",
    epochs: int = 50,
    imgsz: int = 640,
    batch: int = 16,
    project: str = "runs/train",
    name: str = "vr-custom",
    exist_ok: bool = False,
):
    """
    Fine-tune YOLOv8 model on custom dataset
    
    Args:
        data_yaml: Path to data.yaml configuration file
        model: Base model to start from (yolov8n.pt, yolov8s.pt, etc.)
        epochs: Number of training epochs
        imgsz: Image size for training
        batch: Batch size (reduce if GPU memory issues)
        project: Project directory for results
        name: Name for this training run
        exist_ok: Whether to overwrite existing training run
    """
    
    print("=" * 60)
    print("🚀 VR Object Detector - Custom Training")
    print("=" * 60)
    
    # Verify data.yaml exists
    if not os.path.exists(data_yaml):
        raise FileNotFoundError(f"Data config not found: {data_yaml}")
    
    print(f"\n📁 Dataset: {data_yaml}")
    print(f"🎯 Base Model: {model}")
    print(f"📊 Epochs: {epochs}")
    print(f"🖼️  Image Size: {imgsz}")
    print(f"📦 Batch Size: {batch}")
    print(f"💾 Output: {project}/{name}")
    print()
    
    # Load pre-trained model
    print("Loading pre-trained YOLO model...")
    yolo_model = YOLO(model)
    print(f"✅ Model loaded: {model}")
    print(f"   - Model will retain COCO classes (car, person, truck, etc.)")
    print(f"   - Model will learn NEW classes from your dataset")
    print()
    
    # Start training
    print("🏋️  Starting training...")
    print("-" * 60)
    
    results = yolo_model.train(
        data=data_yaml,
        epochs=epochs,
        imgsz=imgsz,
        batch=batch,
        project=project,
        name=name,
        exist_ok=exist_ok,
        patience=10,  # Early stopping patience
        save=True,
        plots=True,
        verbose=True,
    )
    
    print("-" * 60)
    print("✅ Training complete!")
    print()
    
    # Get paths
    output_dir = Path(project) / name
    best_model = output_dir / "weights" / "best.pt"
    last_model = output_dir / "weights" / "last.pt"
    
    print("📊 Training Results:")
    print(f"   - Best model: {best_model}")
    print(f"   - Last model: {last_model}")
    print(f"   - Results directory: {output_dir}")
    print()
    
    print("🎯 Next Steps:")
    print(f"   1. Review training metrics in: {output_dir}")
    print(f"   2. Test your model: python test_model.py --model {best_model}")
    print(f"   3. Deploy to API: Update docker-compose.yml to use your trained model")
    print()
    
    return results


def main():
    parser = argparse.ArgumentParser(
        description="Train YOLOv8 on custom VR object detection dataset"
    )
    
    parser.add_argument(
        "--data",
        type=str,
        required=True,
        help="Path to data.yaml file (e.g., ./Labeled-Images.v1-2025-12.yolov8/data.yaml)",
    )
    
    parser.add_argument(
        "--model",
        type=str,
        default="yolov8n.pt",
        choices=["yolov8n.pt", "yolov8s.pt", "yolov8m.pt", "yolov8l.pt", "yolov8x.pt"],
        help="Base YOLO model (default: yolov8n.pt - fastest)",
    )
    
    parser.add_argument(
        "--epochs",
        type=int,
        default=50,
        help="Number of training epochs (default: 50)",
    )
    
    parser.add_argument(
        "--imgsz",
        type=int,
        default=640,
        help="Training image size (default: 640)",
    )
    
    parser.add_argument(
        "--batch",
        type=int,
        default=16,
        help="Batch size (reduce if out of memory, default: 16)",
    )
    
    parser.add_argument(
        "--project",
        type=str,
        default="runs/train",
        help="Project directory for saving results",
    )
    
    parser.add_argument(
        "--name",
        type=str,
        default="vr-custom",
        help="Name for this training run",
    )
    
    parser.add_argument(
        "--exist-ok",
        action="store_true",
        help="Allow overwriting existing training run",
    )
    
    args = parser.parse_args()
    
    # Train model
    train_model(
        data_yaml=args.data,
        model=args.model,
        epochs=args.epochs,
        imgsz=args.imgsz,
        batch=args.batch,
        project=args.project,
        name=args.name,
        exist_ok=args.exist_ok,
    )


if __name__ == "__main__":
    main()
