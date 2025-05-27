using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Text;

[System.Serializable]
public class OpenAITTSRequest
{
    public string model = "tts-1"; // or "tts-1-hd" for higher quality
    public string input;
    public string voice = "nova"; // alloy, echo, fable, onyx, nova, shimmer          alloy
    public string response_format = "mp3"; // mp3, opus, aac, flac
    public float speed = 1.0f; // 0.25 to 4.0
}

public class OpenAITTSManager : MonoBehaviour
{
    [Header("OpenAI Settings")]
    [SerializeField] private string apiKey = "APIKEYEHER"; 
    [SerializeField] private string apiUrl = "https://api.openai.com/v1/audio/speech";
    
    [Header("TTS Settings")]
    [SerializeField] private string voice = "nova"; // alloy, echo, fable, onyx, nova, shimmer            alloy
    [SerializeField] private string model = "tts-1"; // tts-1 or tts-1-hd
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private bool autoPlay = true;
    
    [Header("Audio Settings")]
    [SerializeField] public AudioSource audioSource;
    [SerializeField] private float volume = 1.0f;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onTTSStart;
    [SerializeField] private UnityEvent onTTSComplete;
    [SerializeField] private UnityEvent<string> onTTSError;
    
    private bool isProcessing = false;
    private Coroutine currentTTSCoroutine;
    
    void Start()
    {
        InitializeAudioSource();
    }
    
    private void InitializeAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure for Quest
        audioSource.spatialBlend = 0f; // 2D audio
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
    
    public void Speak(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("Cannot speak empty text");
            return;
        }
        
        if (isProcessing)
        {
            Debug.Log("TTS already processing, stopping current...");
            StopSpeaking();
        }
        
        currentTTSCoroutine = StartCoroutine(SpeakCoroutine(text));
    }
    
    public void StopSpeaking()
    {
        if (currentTTSCoroutine != null)
        {
            StopCoroutine(currentTTSCoroutine);
            currentTTSCoroutine = null;
        }
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        isProcessing = false;
    }
    
    public bool IsSpeaking()
    {
        return isProcessing && audioSource != null && audioSource.isPlaying;
    }
    
    private IEnumerator SpeakCoroutine(string text)
    {
        isProcessing = true;
        onTTSStart?.Invoke();
        
        Debug.Log($"Starting TTS for: {text}");
        
        // Create request body
        OpenAITTSRequest requestBody = new OpenAITTSRequest
        {
            model = model,
            input = text,
            voice = voice,
            response_format = "mp3",
            speed = speed
        };
        
        string jsonBody = JsonUtility.ToJson(requestBody);
        
        // Create web request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        request.downloadHandler = new DownloadHandlerBuffer();
        
        // Set headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        
        // Send request
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"TTS API Error: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
            onTTSError?.Invoke($"API Error: {request.error}");
        }
        else
        {
            // Get audio data
            byte[] audioData = request.downloadHandler.data;
            
            if (audioData != null && audioData.Length > 0)
            {
                Debug.Log($"Received audio data: {audioData.Length} bytes");
                
                // Convert to AudioClip and play
                yield return StartCoroutine(PlayAudioFromBytes(audioData));
            }
            else
            {
                Debug.LogError("No audio data received");
                onTTSError?.Invoke("No audio data received");
            }
        }
        
        isProcessing = false;
        onTTSComplete?.Invoke();
    }
    
    private IEnumerator PlayAudioFromBytes(byte[] audioData)
    {
        // Create temporary file path
        string tempPath = System.IO.Path.Combine(Application.persistentDataPath, "temp_tts.mp3");
        bool success = false;
        
        // Write audio data to temporary file
        try
        {
            System.IO.File.WriteAllBytes(tempPath, audioData);
            success = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error writing temp file: {e.Message}");
            onTTSError?.Invoke($"File write error: {e.Message}");
            yield break;
        }
        
        if (success)
        {
            // Load as AudioClip
            string fileUrl = "file://" + tempPath;
            
            using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.MPEG))
            {
                yield return audioRequest.SendWebRequest();
                
                if (audioRequest.result == UnityWebRequest.Result.Success)
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                    
                    if (audioClip != null && autoPlay)
                    {
                        audioSource.clip = audioClip;
                        audioSource.Play();
                        
                        // Wait for audio to finish
                        while (audioSource.isPlaying)
                        {
                            yield return null;
                        }
                        
                        Debug.Log("TTS playback completed");
                    }
                    else
                    {
                        Debug.LogError("Failed to create AudioClip from data");
                        onTTSError?.Invoke("Failed to create AudioClip");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to load audio: {audioRequest.error}");
                    onTTSError?.Invoke($"Audio load error: {audioRequest.error}");
                }
            }
        }
        
        // Clean up temporary file
        if (System.IO.File.Exists(tempPath))
        {
            try
            {
                System.IO.File.Delete(tempPath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not delete temp file: {e.Message}");
            }
        }
    }
    
    // Public methods for voice selection
    public void SetVoice(string newVoice)
    {
        // Available voices: alloy, echo, fable, onyx, nova, shimmer
        voice = newVoice;
    }
    
    public void SetSpeed(float newSpeed)
    {
        // Speed range: 0.25 to 4.0
        speed = Mathf.Clamp(newSpeed, 0.25f, 4.0f);
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}