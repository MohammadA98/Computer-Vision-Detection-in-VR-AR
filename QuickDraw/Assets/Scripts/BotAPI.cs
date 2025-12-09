using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Prediction
{
    public string @class;
    public float confidence;
    public string confidence_percent;
}

[System.Serializable]
public class PredictionResponse
{
    public Prediction[] predictions;
    public bool success;
    public string message;
}

public class BotAPI : MonoBehaviour
{
    [Header("API Configuration")]
    public string pythonURL = "https://issa-ennab-quickdraw-api.hf.space/predict/base64";
    public int topK = 3;
    
    [Header("Debug")]
    public bool verboseLogging = true;
    public TextMeshProUGUI statusText;  // Optional: shows status in VR

    public event Action<string> OnGuessReceived;
    public event Action<PredictionResponse> OnPredictionsReceived;

    void Start()
    {
        Debug.Log($">>> [BotAPI] Initialized with URL: {pythonURL}");
        Debug.Log($">>> [BotAPI] IMPORTANT: If running on Quest, make sure URL uses your PC's local IP, not localhost!");
        
        // Test API connection on startup
        StartCoroutine(TestAPIConnection());
    }

    IEnumerator TestAPIConnection()
    {
        yield return new WaitForSeconds(2f);
        
        Debug.Log(">>> [BotAPI TEST] Creating test image...");
        
        // Create a simple test image (white 100x100 square)
        Texture2D testImage = new Texture2D(100, 100, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[100 * 100];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        testImage.SetPixels(pixels);
        testImage.Apply();
        
        Debug.Log(">>> [BotAPI TEST] Sending test image to API...");
        RequestGuess(testImage);
    }

    public void RequestGuess(Texture2D screenshot)
    {
        if (screenshot == null)
        {
            Debug.LogError(">>> [BotAPI] ERROR: Screenshot is null, cannot send request");
            return;
        }
        
        Debug.Log($">>> [BotAPI] RequestGuess called with {screenshot.width}x{screenshot.height} image");
        StartCoroutine(SendScreenshot(screenshot));
    }

    IEnumerator SendScreenshot(Texture2D img)
    {
        Debug.Log("*************  API CALL START  *****************");

        Debug.Log($">>> API CALL START - Image size: {img.width}x{img.height}");
        UpdateStatus($"Sending {img.width}x{img.height} image...");
        
        // Convert image to PNG bytes
        byte[] pngBytes = img.EncodeToPNG();
        Debug.Log($">>> PNG bytes: {pngBytes.Length} bytes");
        
        // Convert to base64 string
        string base64Image = Convert.ToBase64String(pngBytes);
        Debug.Log($">>> Base64 length: {base64Image.Length} chars");

        // Create JSON request body
        string jsonBody = JsonUtility.ToJson(new RequestData 
        { 
            image_base64 = base64Image, 
            top_k = topK 
        });
        Debug.Log($">>> JSON body length: {jsonBody.Length} chars");

        // Create POST request with JSON
        UnityWebRequest req = new UnityWebRequest(pythonURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($">>> SENDING REQUEST TO: {pythonURL}");
        UpdateStatus($"Calling API...\n{pythonURL}");
        
        yield return req.SendWebRequest();

        Debug.Log($">>> REQUEST COMPLETE - Result: {req.result}");

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($">>> BotAPI ERROR: {req.error}");
            Debug.LogError($">>> Response Code: {req.responseCode}");
            UpdateStatus($"ERROR: {req.error}\nCode: {req.responseCode}");
            yield break;
        }

        // Parse JSON response
        string responseText = req.downloadHandler.text;
        Debug.Log($">>> API Response ({responseText.Length} chars): {responseText}");

        try
        {
            PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(responseText);
            
            if (response.success && response.predictions != null && response.predictions.Length > 0)
            {
                // Get top prediction
                string topGuess = response.predictions[0].@class;
                float confidence = response.predictions[0].confidence;
                
                Debug.Log($">>> TOP PREDICTION: {topGuess} ({confidence * 100:F2}%)");
                UpdateStatus($"✓ API Connected!\nTop: {topGuess} ({confidence * 100:F0}%)");
                
                // Invoke both events
                OnGuessReceived?.Invoke(topGuess);
                OnPredictionsReceived?.Invoke(response);
            }
            else
            {
                Debug.LogWarning($">>> No predictions - success: {response.success}, predictions: {response.predictions?.Length ?? 0}");
                UpdateStatus($"No predictions received");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($">>> Failed to parse API response: {e.Message}");
            Debug.LogError($">>> Stack trace: {e.StackTrace}");
            UpdateStatus($"Parse Error: {e.Message}");
        }

        Debug.Log("*************  API CALL END  *****************");
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    [System.Serializable]
    private class RequestData
    {
        public string image_base64;
        public int top_k;
    }
}
