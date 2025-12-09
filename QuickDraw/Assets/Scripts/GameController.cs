using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main game controller for the VR QuickDraw game.
/// Manages the game loop, UI updates, API communication, and win conditions.
/// 
/// Game Flow:
/// 1. Select random target word from CLASS_NAMES
/// 2. Player draws in VR
/// 3. After 4 seconds, start sending screenshots to ML API every 2 seconds
/// 4. Cycle through top 3 predictions every 0.5 seconds
/// 5. If any prediction matches target word, trigger win state
/// </summary>
public class GameController : MonoBehaviour
{
    // =========================
    //  SINGLETON PATTERN
    // =========================
    // Ensures only ONE GameController exists in the scene
    // Prevents duplicate API calls and timing conflicts

    private static GameController instance;

    void Awake()
    {
        // Check if another instance already exists
        if (instance != null && instance != this)
        {
            Debug.LogError($">>> [CRITICAL] DUPLICATE GameController detected! Destroying duplicate instance on GameObject '{gameObject.name}'. " +
                          $"Original instance is on '{instance.gameObject.name}'. " +
                          $"Check your scene Hierarchy for multiple GameController objects!");
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        Debug.Log($">>> [SINGLETON] GameController initialized on GameObject '{gameObject.name}'");
    }

    // =========================
    //  WORD BANK
    // =========================
    // Available drawing categories that the ML model can recognize
    // These are the 49 classes the model was trained on

    private static readonly string[] CLASS_NAMES =
    {
        "cat","dog","bird","fish","bear","butterfly","spider",
        "house","castle","barn","bridge","lighthouse","church",
        "car","airplane","bicycle","truck","train",
        "tree","flower","sun","moon","cloud","mountain",
        "apple","banana","book","chair","table","cup","umbrella",
        "face","eye","hand","foot",
        "circle","triangle","square","star",
        "sword","axe","hammer","key","crown",
        "guitar","piano"
    };

    // =========================
    //  UI REFERENCES
    // =========================
    // All UI elements referenced in Unity Inspector
    // Assign these in the Inspector to connect UI to code logic

    [Header("Target Word & Timer")]
    public TextMeshProUGUI TargetWordText;        // Displays the word player needs to draw
    public TextMeshProUGUI TargetWordLabel;       // Label for target word section
    public TextMeshProUGUI TimerText;             // Shows elapsed time since drawing started

    [Header("Guess Box")]
    public TextMeshProUGUI BotGuessText;          // Displays current ML prediction (cycles through top 3)
    public TextMeshProUGUI ConfidenceText;        // Shows confidence percentage for current prediction

    [Header("Stats Panel")]
    public GameObject StatsPanel;                 // Panel that shows when player wins
    public TextMeshProUGUI TimeTakenText;         // Final time taken to complete the drawing
    public TextMeshProUGUI FinalConfidenceText;   // Final confidence of winning prediction
    public TextMeshProUGUI TriesTakenText;        // Number of API calls made before winning

    [Header("Buttons")]
    public Button SkipButton;                     // Button to skip current word and start new round
    public Button ClearButton;                    // Button to clear the drawing

    [Header("Gameplay")]
    public DrawController drawController;         // Controller that handles VR drawing mechanics

    [Header("Bot API")]
    public BotAPI bot;                            // API handler for ML predictions
    public BotBrain botBrain;                     // Stores prediction history and state

    [Header("Prediction Settings")]
    public float delayBeforePredictions = 4f;     // Wait time before first API call (seconds)
    public float predictionInterval = 2f;         // Time between API calls (seconds)

    // =========================
    //  INTERNAL STATE
    // =========================

    private string targetWord = "";
    private float roundTimer = 0f;
    private float predictionTimer = 0f;

    private bool hasStartedDrawing = false;
    private bool isPredicting = false;
    private bool roundWon = false;
    private bool isRequestInProgress = false;

    private int triesCount = 0;
    private string lastTopGuess = "";
    private float lastTopConfidence = 0f;

    // Prediction cycling
    private PredictionResponse lastPredictions = null;
    private int currentPredictionIndex = 0;
    private float predictionCycleTimer = 0f;
    [SerializeField] private float predictionCycleInterval = 0.5f;

    private Color targetWordDefaultColor;
    [SerializeField] private Color targetWordWinColor = Color.green;

    private const string BOT_GUESS_PLACEHOLDER = "(BOT GUESS)";
    private const string CONFIDENCE_PLACEHOLDER = "Confidence: --%";


    // =========================
    //  UNITY LIFECYCLE
    // =========================

    /// <summary>
    /// Unity Start - Called once when script initializes
    /// Sets up UI references, button listeners, and event subscriptions
    /// </summary>
    void Start()
    {
        // Store the default color of target word text for resetting later
        if (TargetWordText != null)
            targetWordDefaultColor = TargetWordText.color;

        // Connect button click events to their respective methods
        if (SkipButton != null)
            SkipButton.onClick.AddListener(StartNewRound);

        if (ClearButton != null)
            ClearButton.onClick.AddListener(ClearDrawing);

        // Validate that BotAPI is assigned and subscribe to its events
        if (bot == null)
        {
            Debug.LogError(">>> BotAPI NOT ASSIGNED!");
            TargetWordText.text = "NO BOT API";
        }
        else
        {
            // Subscribe to prediction events from BotAPI
            bot.OnGuessReceived += OnBotGuess;              // Called when single guess received
            bot.OnPredictionsReceived += OnPredictionsReceived;  // Called when full prediction array received
        }

        // Start the first round
        StartNewRound();
    }


    /// <summary>
    /// Unity Update - Called every frame
    /// Handles:
    /// 1. VR controller button input for Skip/Clear
    /// 2. Round timer updates
    /// 3. Triggering API calls at intervals
    /// 4. Cycling through prediction display
    /// </summary>
    void Update()
    {
        // VR Controller shortcuts (Button Y = Skip, Button B = Clear)
        if (OVRInput.GetDown(OVRInput.Button.Three)) StartNewRound();  // Y button
        if (OVRInput.GetDown(OVRInput.Button.Four)) ClearDrawing();    // B button

        // Check if player is currently drawing
        bool isDrawing = drawController != null && drawController.IsDrawing();

        // Start tracking when player begins drawing for the first time
        if (isDrawing && !hasStartedDrawing && !roundWon)
        {
            hasStartedDrawing = true;
        }

        // Main game loop - only runs when round is active and player has started drawing
        if (!roundWon && hasStartedDrawing)
        {
            // Update round timer and display
            roundTimer += Time.deltaTime;
            TimerText.text = roundTimer.ToString("0.0") + "s";

            // After initial delay, enable prediction mode
            if (!isPredicting && roundTimer >= delayBeforePredictions)
            {
                isPredicting = true;
                predictionTimer = 0f;
                Debug.Log($">>> [TIMING] Prediction mode ENABLED at {Time.time:F2}s. Interval set to: {predictionInterval}s");
            }

            // When in prediction mode, make API calls at regular intervals
            if (isPredicting)
            {
                predictionTimer += Time.deltaTime;

                // Time for next API call? Check that no request is already in progress
                if (predictionTimer >= predictionInterval && !isRequestInProgress)
                {
                    Debug.Log($">>> [TIMING] ⚠️ API CALL TRIGGERED! Timer: {predictionTimer:F3}s >= Interval: {predictionInterval}s | Game Time: {Time.time:F2}s | Request in progress: {isRequestInProgress}");
                    predictionTimer = 0f;  // Reset timer for next interval
                    StartCoroutine(CaptureAndSendScreenshot());  // Capture screen and send to API
                }
            }
        }

        // Cycle through the top 3 predictions to show all options
        // Only cycle when: round active, drawing started, predicting, and predictions available
        if (!roundWon && hasStartedDrawing && isPredicting &&
            lastPredictions != null && lastPredictions.predictions != null && lastPredictions.predictions.Length > 0)
        {
            predictionCycleTimer += Time.deltaTime;

            // Time to show next prediction?
            if (predictionCycleTimer >= predictionCycleInterval)
            {
                predictionCycleTimer = 0f;  // Reset cycle timer

                // Move to next prediction (wraps around to 0 after last one)
                currentPredictionIndex = (currentPredictionIndex + 1) % lastPredictions.predictions.Length;

                // Update UI with new prediction
                UpdatePredictionDisplay();
            }
        }
    }


    // =========================
    //  ROUND CONTROL
    // =========================

    /// <summary>
    /// Starts a new game round
    /// - Selects random target word from CLASS_NAMES
    /// - Resets all timers and state flags
    /// - Resets UI to default state
    /// - Clears the drawing surface
    /// </summary>
    public void StartNewRound()
    {
        Debug.Log($">>> [ROUND] ========== NEW ROUND STARTING ==========");

        // Select a random word for player to draw
        string newWord = CLASS_NAMES[Random.Range(0, CLASS_NAMES.Length)];
        targetWord = newWord.ToLower().Trim();

        // Reset all timers
        roundTimer = 0f;
        predictionTimer = 0f;

        // Reset all state flags
        hasStartedDrawing = false;
        isPredicting = false;
        roundWon = false;
        isRequestInProgress = false;

        Debug.Log($">>> [ROUND] Target word: {targetWord} | Prediction interval: {predictionInterval}s | Delay: {delayBeforePredictions}s");        // Reset statistics
        triesCount = 0;
        lastTopGuess = "";
        lastTopConfidence = 0f;

        // Reset prediction cycling
        lastPredictions = null;
        currentPredictionIndex = 0;
        predictionCycleTimer = 0f;

        // Reset UI - Timer
        TimerText.text = "0.0s";

        // Reset UI - Target Word
        TargetWordText.text = targetWord.ToUpper();
        TargetWordText.color = targetWordDefaultColor;

        // Reset UI - Guess Box (show placeholders)
        BotGuessText.text = BOT_GUESS_PLACEHOLDER;
        ConfidenceText.text = CONFIDENCE_PLACEHOLDER;

        // Reset UI - Stats Panel (hide and reset values)
        StatsPanel.SetActive(false);
        TimeTakenText.text = "0.0s";
        FinalConfidenceText.text = "--%";
        TriesTakenText.text = "0";

        // Clear the drawing surface
        ClearDrawing();

        Debug.Log(">>> New round started. Target word: " + targetWord);
    }


    /// <summary>
    /// Clears the drawing surface
    /// Delegates to DrawController which handles trail renderer and particles
    /// </summary>
    public void ClearDrawing()
    {
        if (drawController != null)
            drawController.ClearPanel();
    }


    // =========================
    //  BOT COMMUNICATION
    // =========================
    // ⭐ Restored the ORIGINAL screenshot logic (works perfectly)

    IEnumerator CaptureAndSendScreenshot()
    {
        if (bot == null || roundWon || isRequestInProgress)
            yield break;

        isRequestInProgress = true;
        Debug.Log($">>> [GameController] Starting API request at {Time.time:F2}s");

        yield return new WaitForEndOfFrame();

        // Capture the player’s real headset view — this is what works best with your model
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        if (screenshot == null)
        {
            Debug.LogError("Screenshot failed.");
            isRequestInProgress = false;
            yield break;
        }

        bot.RequestGuess(screenshot);

        // Wait a frame to ensure the request has started before allowing another
        yield return new WaitForEndOfFrame();
        isRequestInProgress = false;
        Debug.Log($">>> [GameController] API request completed at {Time.time:F2}s");
    }



    // =========================
    //  PREDICTION HANDLING
    // =========================

    /// <summary>
    /// Legacy callback for single guess (currently just logs)
    /// OnPredictionsReceived is the main handler used
    /// </summary>
    void OnBotGuess(string guess)
    {
        Debug.Log("Bot guessed: " + guess);
    }

    /// <summary>
    /// Main callback when ML API returns predictions
    /// 
    /// Process:
    /// 1. Store predictions for cycling display
    /// 2. Update statistics (tries count)
    /// 3. Display first prediction immediately
    /// 4. Check if ANY of top 3 predictions match target word
    /// 5. If match found, trigger win state
    /// 
    /// The key feature: checks ALL predictions (not just #1) for a match,
    /// so if target is "umbrella" and it's prediction #2, we still win!
    /// </summary>
    void OnPredictionsReceived(PredictionResponse response)
    {
        // Ignore if round already won or invalid response
        if (roundWon) return;
        if (response == null || response.predictions == null || response.predictions.Length == 0)
            return;

        // Store predictions for cycling through them in Update()
        lastPredictions = response;
        currentPredictionIndex = 0;  // Start from first prediction
        predictionCycleTimer = 0f;   // Reset cycle timer

        // Track statistics from top prediction
        var top = response.predictions[0];
        string topGuess = top.@class.ToLower().Trim();

        triesCount++;  // Increment API call counter
        lastTopGuess = topGuess;
        lastTopConfidence = top.confidence;

        // Immediately display the first prediction
        UpdatePredictionDisplay();

        // Update BotBrain with new predictions (optional tracking)
        botBrain?.UpdatePredictions(response);

        // Check if ANY of the top 3 predictions matches the target word
        // This allows winning even if correct answer is 2nd or 3rd prediction
        foreach (var prediction in response.predictions)
        {
            string guess = prediction.@class.ToLower().Trim();

            // Found a match!
            if (guess == targetWord)
            {
                // Update UI to show the winning prediction immediately
                BotGuessText.text = prediction.@class.ToUpper();
                ConfidenceText.text = "Confidence: " + prediction.confidence_percent;

                // Trigger win state with this prediction
                HandleWin(prediction);
                break;  // Stop checking after first match
            }
        }
    }

    /// <summary>
    /// Updates the UI to show the current prediction in the cycle
    /// Called automatically by Update() every predictionCycleInterval seconds
    /// Rotates through: prediction[0] → prediction[1] → prediction[2] → back to [0]
    /// </summary>
    void UpdatePredictionDisplay()
    {
        // Safety check: ensure predictions exist
        if (lastPredictions == null || lastPredictions.predictions == null || lastPredictions.predictions.Length == 0)
            return;

        // Get the prediction at current index
        var prediction = lastPredictions.predictions[currentPredictionIndex];

        // Update UI with this prediction's class name and confidence
        BotGuessText.text = prediction.@class.ToUpper();
        ConfidenceText.text = "Confidence: " + prediction.confidence_percent;
    }


    // =========================
    //  WIN HANDLING
    // =========================

    /// <summary>
    /// Handles win state when player successfully draws the target word
    /// 
    /// Actions:
    /// 1. Set roundWon flag to stop game loop
    /// 2. Stop prediction cycling and API calls
    /// 3. Change target word color to green
    /// 4. Show stats panel with final results
    /// 5. Display time taken, confidence, and number of tries
    /// </summary>
    void HandleWin(Prediction winningPrediction)
    {
        // Prevent multiple win triggers
        if (roundWon) return;

        // Set win state - stops timers and API calls in Update()
        roundWon = true;
        isPredicting = false;

        // Stop prediction cycling by clearing stored predictions
        lastPredictions = null;

        // Visual feedback: change target word to green
        TargetWordText.color = targetWordWinColor;

        // Show the stats panel with final results
        StatsPanel.SetActive(true);

        // Populate stats with final values
        TimeTakenText.text = roundTimer.ToString("0.0") + "s";           // How long it took
        FinalConfidenceText.text = winningPrediction.confidence_percent;  // ML confidence in prediction
        TriesTakenText.text = triesCount.ToString();                      // Number of API calls made

        Debug.Log(">>> ROUND WON! Prediction: " + winningPrediction.@class + " (" + winningPrediction.confidence_percent + ")");
    }
}