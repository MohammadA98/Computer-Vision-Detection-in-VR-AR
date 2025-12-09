import base64
import io
import logging
from typing import List, Tuple
from PIL import Image
from ultralytics import YOLO
import numpy as np
import torch

logger = logging.getLogger(__name__)

# Add safe globals for PyTorch 2.6+ compatibility with YOLO models
try:
    from ultralytics.nn.tasks import DetectionModel
    torch.serialization.add_safe_globals([DetectionModel])
except Exception as e:
    logger.warning(f"Could not add safe globals: {e}")


class YOLODetector:
    """YOLO object detector for VR screenshots"""
    
    def __init__(self, model_name: str = "yolov8n.pt", confidence_threshold: float = 0.4):
        """
        Initialize YOLO detector
        
        Args:
            model_name: YOLO model to use (yolov8n.pt, yolov8s.pt, etc.)
            confidence_threshold: Minimum confidence for detections
        """
        self.confidence_threshold = confidence_threshold
        logger.info(f"Loading YOLO model: {model_name}")
        self.model = YOLO(model_name)
        logger.info(f"YOLO model loaded successfully with confidence threshold: {confidence_threshold}")
    
    def decode_base64_image(self, base64_string: str) -> Tuple[Image.Image, int, int]:
        """
        Decode base64 string to PIL Image
        
        Args:
            base64_string: Base64 encoded image (no data URI prefix)
            
        Returns:
            Tuple of (PIL Image, width, height)
        """
        try:
            # Remove any potential data URI prefix (just in case Unity sends it)
            if ',' in base64_string:
                base64_string = base64_string.split(',', 1)[1]
            
            # Decode base64
            image_bytes = base64.b64decode(base64_string)
            image = Image.open(io.BytesIO(image_bytes))
            
            # Convert to RGB if needed (YOLO expects RGB)
            if image.mode != 'RGB':
                image = image.convert('RGB')
            
            width, height = image.size
            logger.info(f"Decoded image: {width}x{height}, mode: {image.mode}")
            
            return image, width, height
        
        except Exception as e:
            logger.error(f"Error decoding base64 image: {e}")
            raise ValueError(f"Failed to decode base64 image: {e}")
    
    def detect_objects(self, image: Image.Image) -> List[dict]:
        """
        Run YOLO detection on image
        
        Args:
            image: PIL Image
            
        Returns:
            List of detections with class, confidence, and bounding box
        """
        try:
            # Run YOLO inference
            results = self.model(image, conf=self.confidence_threshold, verbose=False)
            
            detections = []
            
            # Process results
            for result in results:
                boxes = result.boxes
                
                for box in boxes:
                    # Get box coordinates (xyxy format)
                    x1, y1, x2, y2 = box.xyxy[0].cpu().numpy()
                    
                    # Get confidence and class
                    confidence = float(box.conf[0].cpu().numpy())
                    class_id = int(box.cls[0].cpu().numpy())
                    class_name = self.model.names[class_id]
                    
                    detection = {
                        "class": class_name,
                        "confidence": round(confidence, 2),
                        "x1": int(x1),
                        "y1": int(y1),
                        "x2": int(x2),
                        "y2": int(y2)
                    }
                    
                    detections.append(detection)
            
            logger.info(f"Detected {len(detections)} objects")
            return detections
        
        except Exception as e:
            logger.error(f"Error during YOLO detection: {e}")
            raise RuntimeError(f"Detection failed: {e}")
    
    def process_base64_image(self, base64_string: str) -> dict:
        """
        Complete pipeline: decode base64 -> detect objects -> return results
        
        Args:
            base64_string: Base64 encoded image
            
        Returns:
            Dictionary with detections, image_width, image_height
        """
        # Decode image
        image, width, height = self.decode_base64_image(base64_string)
        
        # Detect objects
        detections = self.detect_objects(image)
        
        return {
            "detections": detections,
            "image_width": width,
            "image_height": height
        }
