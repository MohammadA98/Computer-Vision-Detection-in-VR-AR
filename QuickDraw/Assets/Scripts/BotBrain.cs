using UnityEngine;

public class BotBrain : MonoBehaviour
{
    public string currentWord = "";
    public PredictionResponse latestPredictions;

    public void UpdatePredictions(PredictionResponse predictions)
    {
        latestPredictions = predictions;
        
        if (predictions != null && predictions.predictions != null && predictions.predictions.Length > 0)
        {
            currentWord = predictions.predictions[0].@class;
            Debug.Log($"BotBrain: Updated word to '{currentWord}' with {predictions.predictions[0].confidence_percent} confidence");
        }
    }

    public string GetTopPrediction()
    {
        return currentWord;
    }

    public Prediction[] GetAllPredictions()
    {
        return latestPredictions?.predictions;
    }
}
