"""
Dual YOLO Detector - Uses both COCO and custom models
"""
import logging
from typing import List, Dict, Any
from ultralytics import YOLO
import base64
from io import BytesIO
from PIL import Image

logger = logging.getLogger(__name__)


class DualYOLODetector:
    """Detector that runs both COCO and custom models and merges results"""
    
    def __init__(self, coco_model_path: str, custom_model_path: str, confidence: float = 0.4):
        self.confidence = confidence
        
        # Load COCO model (80 classes)
        logger.info(f"Loading COCO model: {coco_model_path}")
        self.coco_model = YOLO(coco_model_path)
        logger.info(f"COCO model loaded with {len(self.coco_model.names)} classes")
        
        # Load custom model (5 classes)
        logger.info(f"Loading custom model: {custom_model_path}")
        self.custom_model = YOLO(custom_model_path)
        logger.info(f"Custom model loaded with {len(self.custom_model.names)} classes")
        
        logger.info(f"Dual detector ready with confidence threshold: {confidence}")
    
    def decode_base64_image(self, base64_string: str) -> Image.Image:
        """Decode base64 string to PIL Image"""
        image_data = base64.b64decode(base64_string)
        image = Image.open(BytesIO(image_data))
        if image.mode != 'RGB':
            image = image.convert('RGB')
        logger.info(f"Decoded image: {image.width}x{image.height}, mode: {image.mode}")
        return image
    
    def detect_objects(self, image: Image.Image) -> List[Dict[str, Any]]:
        """Run both models and merge detections"""
        detections = []
        
        # Run COCO model
        coco_results = self.coco_model(image, conf=self.confidence, verbose=False)
        for result in coco_results:
            boxes = result.boxes
            for box in boxes:
                x1, y1, x2, y2 = box.xyxy[0].tolist()
                conf = float(box.conf[0])
                cls = int(box.cls[0])
                class_name = self.coco_model.names[cls]
                
                detections.append({
                    "class": class_name,
                    "confidence": conf,
                    "x1": int(x1),
                    "y1": int(y1),
                    "x2": int(x2),
                    "y2": int(y2)
                })
        
        # Run custom model
        custom_results = self.custom_model(image, conf=self.confidence, verbose=False)
        for result in custom_results:
            boxes = result.boxes
            for box in boxes:
                x1, y1, x2, y2 = box.xyxy[0].tolist()
                conf = float(box.conf[0])
                cls = int(box.cls[0])
                class_name = self.custom_model.names[cls]
                
                detections.append({
                    "class": class_name,
                    "confidence": conf,
                    "x1": int(x1),
                    "y1": int(y1),
                    "x2": int(x2),
                    "y2": int(y2)
                })
        
        logger.info(f"Detected {len(detections)} objects (COCO + custom)")
        return detections
    
    def process_base64_image(self, base64_string: str) -> Dict[str, Any]:
        """Process base64 image and return detections in API format"""
        image = self.decode_base64_image(base64_string)
        detections = self.detect_objects(image)
        
        return {
            "detections": detections,
            "image_width": image.width,
            "image_height": image.height
        }
