"""
Test script for evaluating trained YOLOv8 model on validation/test images.

Usage:
    python test_model.py --model runs/train/vr-custom/weights/best.pt
    python test_model.py --model runs/train/vr-custom/weights/best.pt --image path/to/test_image.jpg
"""

import argparse
from pathlib import Path
from ultralytics import YOLO
from PIL import Image
import json


def test_model(model_path: str, data_yaml: str = None, image_path: str = None):
    """
    Test trained YOLO model
    
    Args:
        model_path: Path to trained model (.pt file)
        data_yaml: Path to data.yaml (for validation set testing)
        image_path: Path to single image for testing
    """
    
    print("=" * 60)
    print("🧪 Testing Custom YOLOv8 Model")
    print("=" * 60)
    
    # Load model
    print(f"\n📦 Loading model: {model_path}")
    model = YOLO(model_path)
    print("✅ Model loaded successfully")
    
    # Display model info
    print(f"\n📊 Model Classes:")
    for idx, name in model.names.items():
        print(f"   {idx}: {name}")
    print()
    
    # Test on validation set
    if data_yaml:
        print(f"🔍 Running validation on dataset: {data_yaml}")
        metrics = model.val(data=data_yaml)
        print("\n📈 Validation Metrics:")
        print(f"   mAP50: {metrics.box.map50:.4f}")
        print(f"   mAP50-95: {metrics.box.map:.4f}")
        print(f"   Precision: {metrics.box.mp:.4f}")
        print(f"   Recall: {metrics.box.mr:.4f}")
        print()
    
    # Test on single image
    if image_path:
        print(f"🖼️  Testing on image: {image_path}")
        
        # Run inference
        results = model(image_path, conf=0.4)
        
        # Display detections
        print(f"\n🎯 Detections:")
        for result in results:
            boxes = result.boxes
            if len(boxes) == 0:
                print("   No objects detected")
            else:
                for box in boxes:
                    cls = int(box.cls[0])
                    conf = float(box.conf[0])
                    class_name = model.names[cls]
                    print(f"   - {class_name}: {conf:.2f}")
        
        # Save annotated image
        output_path = Path(image_path).stem + "_detected.jpg"
        results[0].save(output_path)
        print(f"\n💾 Annotated image saved: {output_path}")
        print()


def main():
    parser = argparse.ArgumentParser(description="Test trained YOLOv8 model")
    
    parser.add_argument(
        "--model",
        type=str,
        required=True,
        help="Path to trained model (e.g., runs/train/vr-custom/weights/best.pt)",
    )
    
    parser.add_argument(
        "--data",
        type=str,
        help="Path to data.yaml for validation testing",
    )
    
    parser.add_argument(
        "--image",
        type=str,
        help="Path to single image for testing",
    )
    
    args = parser.parse_args()
    
    if not args.data and not args.image:
        print("⚠️  Please provide either --data or --image for testing")
        return
    
    test_model(
        model_path=args.model,
        data_yaml=args.data,
        image_path=args.image,
    )


if __name__ == "__main__":
    main()
