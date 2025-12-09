#!/bin/bash
# Deploy QuickDraw API to Hugging Face Space
# Run this after creating your Space on HF

set -e

SPACE_NAME="issa-ennab/quickdraw-api"
SPACE_URL="https://huggingface.co/spaces/${SPACE_NAME}"

echo "================================================"
echo "Deploying QuickDraw API to Hugging Face Space"
echo "Space: ${SPACE_URL}"
echo "================================================"
echo ""

# Check if Space directory exists
if [ -d "hf_space" ]; then
    echo "⚠️  hf_space directory already exists. Removing..."
    rm -rf hf_space
fi

# Clone the Space
echo "📥 Cloning Space repository..."
git clone ${SPACE_URL} hf_space
cd hf_space

# Copy necessary files
echo "📋 Copying API files..."
cp ../main.py app.py  # Rename to app.py for HF Space
cp ../model.py .
cp ../utils.py .
cp ../config.py .
cp ../requirements.txt .

# Copy model files
echo "📦 Copying trained model files..."
mkdir -p saved_models
cp ../saved_models/quickdraw_house_cat_dog_car.keras saved_models/
cp ../saved_models/quickdraw_house_cat_dog_car.h5 saved_models/
cp ../saved_models/quickdraw_house_cat_dog_car.onnx saved_models/

# Create Dockerfile for HF Space (port 7860)
echo "🐳 Creating Dockerfile..."
cat > Dockerfile << 'EOF'
# Hugging Face Space Dockerfile for QuickDraw API
FROM python:3.10-slim

# Create user
RUN useradd -m -u 1000 user
USER user
ENV PATH="/home/user/.local/bin:$PATH"

WORKDIR /app

# Install system dependencies
USER root
RUN apt-get update && apt-get install -y \
    libgomp1 \
    curl \
    && rm -rf /var/lib/apt/lists/*
USER user

# Copy requirements and install
COPY --chown=user ./requirements.txt requirements.txt
RUN pip install --no-cache-dir --upgrade -r requirements.txt

# Copy application files
COPY --chown=user . /app

# Create directories for logs
RUN mkdir -p api_logs/received_images

# Expose port 7860 (required by HF Spaces)
EXPOSE 7860

# Start the API on port 7860
CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "7860"]
EOF

# Create README for the Space
echo "📝 Creating README..."
cat > README.md << 'EOF'
---
title: QuickDraw Sketch Recognition API
emoji: 🎨
colorFrom: blue
colorTo: purple
sdk: docker
pinned: false
license: mit
---

# QuickDraw Sketch Recognition API

Real-time sketch recognition API for VR/AR applications. Recognizes 46 different hand-drawn objects using a CNN trained on Google's QuickDraw dataset.

## 🎯 Try It Out

Once the Space is running, you can:

### Test via Swagger UI
Visit the API docs at: `https://issa-ennab-quickdraw-api.hf.space/docs`

### Test via cURL
```bash
# Health check
curl https://issa-ennab-quickdraw-api.hf.space/health

# Get supported classes
curl https://issa-ennab-quickdraw-api.hf.space/classes

# Make a prediction (replace with your base64 image)
curl -X POST https://issa-ennab-quickdraw-api.hf.space/predict/base64 \
  -H "Content-Type: application/json" \
  -d '{"image_base64": "YOUR_BASE64_IMAGE", "top_k": 3}'
```

### Unity/VR Integration
```csharp
private string apiUrl = "https://issa-ennab-quickdraw-api.hf.space/predict/base64";
```

## 📋 Supported Classes (46 total)

**Animals:** cat, dog, bird, fish, bear, butterfly, spider  
**Buildings:** house, castle, barn, bridge, lighthouse, church  
**Transportation:** car, airplane, bicycle, truck, train  
**Nature:** tree, flower, sun, moon, cloud, mountain  
**Objects:** apple, banana, book, chair, table, cup, umbrella  
**Body Parts:** face, eye, hand, foot  
**Shapes:** circle, triangle, square, star  
**Tools:** sword, axe, hammer, key, crown  
**Music:** guitar, piano

## 🔧 API Endpoints

- `GET /` - API information
- `GET /health` - Health check
- `GET /classes` - List all supported classes
- `POST /predict` - Upload image file for prediction
- `POST /predict/base64` - Send base64 encoded image (recommended for VR)

## 🎮 Perfect For

- VR/AR drawing applications
- Educational games
- Real-time sketch recognition
- Interactive art tools

## 📊 Model Performance

- **Accuracy:** 84.89% on validation set
- **Inference Time:** ~50-80ms on CPU
- **Model Size:** 2.9 MB
- **Input:** 28x28 grayscale images

## 📖 Full Documentation

[GitHub Repository](https://github.com/Beakal-23/Augmented-Reality--Image-Detector-Final-Project-)

## 🚀 Built With

- FastAPI for the REST API
- TensorFlow/Keras for the CNN model
- Google QuickDraw dataset
- Docker for deployment
EOF

# Stage all files
echo "✅ Staging files..."
git add .

# Commit
echo "💾 Committing changes..."
git commit -m "Deploy QuickDraw API with trained model and comprehensive logging"

# Push to HF Space
echo "🚀 Pushing to Hugging Face Space..."
echo ""
echo "⚠️  You will be prompted for your Hugging Face credentials:"
echo "   Username: issa-ennab"
echo "   Password: Use your HF Access Token (get it from https://huggingface.co/settings/tokens)"
echo ""
git push

echo ""
echo "================================================"
echo "✅ Deployment Complete!"
echo "================================================"
echo ""
echo "Your API will be available at:"
echo "https://issa-ennab-quickdraw-api.hf.space"
echo ""
echo "Monitor build status at:"
echo "https://huggingface.co/spaces/issa-ennab/quickdraw-api"
echo ""
echo "Note: First build may take 5-10 minutes"
echo "================================================"

cd ..
