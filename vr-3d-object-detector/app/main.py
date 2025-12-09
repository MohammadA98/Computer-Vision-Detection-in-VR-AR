import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from app.models import DetectionRequest, DetectionResponse
from app.detector import YOLODetector
from app.dual_detector import DualYOLODetector

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Global detector instance
detector = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Lifespan context manager for startup/shutdown events"""
    global detector
    
    # Startup: Initialize YOLO model
    logger.info("🚀 Starting VR Object Detector API...")
    
    # Support custom model via environment variable
    import os
    model_path = os.getenv("MODEL_PATH", "yolov8n.pt")
    custom_model_path = os.getenv("CUSTOM_MODEL_PATH", None)
    confidence = float(os.getenv("CONFIDENCE_THRESHOLD", "0.4"))
    
    # Use dual detector if custom model is specified
    if custom_model_path:
        logger.info(f"🔄 Using DUAL detector mode")
        logger.info(f"COCO Model: {model_path}, Custom Model: {custom_model_path}")
        detector = DualYOLODetector(
            coco_model_path=model_path,
            custom_model_path=custom_model_path,
            confidence=confidence
        )
        logger.info("✅ Dual YOLO models loaded and ready!")
    else:
        logger.info(f"📦 Using SINGLE detector mode")
        logger.info(f"Model: {model_path}, Confidence: {confidence}")
        detector = YOLODetector(model_name=model_path, confidence_threshold=confidence)
        logger.info("✅ YOLO model loaded and ready!")
    
    yield
    
    # Shutdown
    logger.info("🛑 Shutting down VR Object Detector API...")


# Create FastAPI app
app = FastAPI(
    title="VR 3D Object Detector API",
    description="YOLO-based object detection API for Unity VR screenshots",
    version="1.0.0",
    lifespan=lifespan
)

# Add CORS middleware (allow Unity to call from anywhere)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify Unity's origin
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/", tags=["Health"])
async def root():
    """Root endpoint - API info"""
    return {
        "service": "VR 3D Object Detector API",
        "status": "running",
        "model": "yolov8n.pt",
        "confidence_threshold": 0.4,
        "endpoints": {
            "health": "/health",
            "detect": "/detect (POST)"
        }
    }


@app.get("/health", tags=["Health"])
async def health_check():
    """Health check endpoint"""
    if detector is None:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="YOLO model not loaded"
        )
    
    return {
        "status": "healthy",
        "model_loaded": True,
        "model": "yolov8n.pt"
    }


@app.post("/detect", response_model=DetectionResponse, tags=["Detection"])
async def detect_objects(request: DetectionRequest):
    """
    Detect objects in a base64-encoded image from Unity VR
    
    Args:
        request: DetectionRequest with image_base64 field
        
    Returns:
        DetectionResponse with detections, image_width, image_height
    """
    if detector is None:
        logger.error("Detector not initialized")
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="YOLO detector not initialized"
        )
    
    try:
        logger.info("📸 Received detection request from Unity")
        
        # Process image and detect objects
        result = detector.process_base64_image(request.image_base64)
        
        logger.info(f"✅ Detection complete: {len(result['detections'])} objects found")
        
        return result
    
    except ValueError as e:
        logger.error(f"Invalid input: {e}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Invalid image data: {str(e)}"
        )
    
    except RuntimeError as e:
        logger.error(f"Detection error: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Detection failed: {str(e)}"
        )
    
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Unexpected error: {str(e)}"
        )
