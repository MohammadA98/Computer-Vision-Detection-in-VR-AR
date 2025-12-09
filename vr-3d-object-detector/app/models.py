from pydantic import BaseModel, Field
from typing import List


class DetectionRequest(BaseModel):
    """Request model for object detection"""
    image_base64: str = Field(..., description="Base64 encoded JPG image (no data URI prefix)")


class Detection(BaseModel):
    """Individual detection result"""
    class_name: str = Field(..., alias="class", description="Detected object class")
    confidence: float = Field(..., description="Confidence score (0-1)")
    x1: int = Field(..., description="Top-left X coordinate")
    y1: int = Field(..., description="Top-left Y coordinate")
    x2: int = Field(..., description="Bottom-right X coordinate")
    y2: int = Field(..., description="Bottom-right Y coordinate")

    class Config:
        populate_by_name = True


class DetectionResponse(BaseModel):
    """Response model for object detection"""
    detections: List[Detection]
    image_width: int
    image_height: int
