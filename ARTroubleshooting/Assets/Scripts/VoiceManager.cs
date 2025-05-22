using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Meta.WitAi.CallbackHandlers;
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Utilities;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class VoiceManagerWithGPT : MonoBehaviour
{
    //[Header("Wit configuration")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private WitResponseMatcher responseMatcher;
    [SerializeField] public string transcriptedText;

    //[Header("ChatGPT Settings")]
    [SerializeField] private string apiKey = "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";
    [SerializeField] private string apiUrl = "https://api.openai.com/v1/chat/completions";
    [SerializeField] private string model = "gpt-4-turbo";
    [SerializeField] private float temperature = 0.7f;
    [SerializeField] private int maxTokens = 150;
    [SerializeField] private bool saveConversationHistory = true;
    [SerializeField] private int maxHistoryMessages = 10;

    //[Header("Text-to-Speech")]
    [SerializeField] private TTSSpeaker ttsSpeaker;
    [SerializeField] private bool autoPlayResponse = true;

    //[Header("Voice Events")]
    [SerializeField] private UnityEvent wakeWordDetected;
    [SerializeField] private UnityEvent<string> completeTranscription;
    [SerializeField] private UnityEvent<string> onGPTResponseReceived;
    [SerializeField] private UnityEvent onProcessingStart;
    [SerializeField] private UnityEvent onProcessingComplete;

    private bool _voiceCommandReady;
    private List<ChatMessage> _conversationHistory = new List<ChatMessage>();
    private bool _isProcessingGPT = false;

    void Start()
    {
        if (ttsSpeaker == null)
        {
            ttsSpeaker = GetComponent<TTSSpeaker>();
            if (ttsSpeaker == null)
            {
                ttsSpeaker = gameObject.AddComponent<TTSSpeaker>();
                Debug.Log("TTSSpeaker component added automatically.");
            }
        }

        // Add system message to history
        if (saveConversationHistory)
        {
            _conversationHistory.Add(new ChatMessage 
            { 
                role = "system", 
                content = "You are a helpful virtual assistant in a VR application that helps a user troubleshoot his bambulab A1. Keep responses concise and help the user find the best troubleshooting guide." 
            });
        }
    }

    private void Awake()
    {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.AddListener(WakeWordDetected);
        }

        // should activate voice experience
        appVoiceExperience.Activate();
    }

    private void OnDestroy()
    {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.RemoveListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.RemoveListener(WakeWordDetected);
        }
    }

    private void ReactivateVoice() => appVoiceExperience.Activate();

    private void WakeWordDetected(String[] arg0)
    {
        _voiceCommandReady = true;
        Debug.Log("Wake word detected");
        wakeWordDetected?.Invoke();
    }

    private void OnPartialTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;
        transcriptedText = transcription;
    }

    private void OnFullTranscription(string transcription)
    {
        if (!_voiceCommandReady) return;

        transcriptedText = transcription;
        Debug.Log("Full transcription: " + transcription);
        completeTranscription?.Invoke(transcription);

        SendToChatGPT(transcription);
        _voiceCommandReady = false;
    }

    // Send transcribed text to ChatGPT
    private void SendToChatGPT(string userInput)
    {
        if (_isProcessingGPT || string.IsNullOrEmpty(userInput))
            return;

        _isProcessingGPT = true;
        onProcessingStart?.Invoke();
        Debug.Log("Sending to ChatGPT: " + userInput);
        
        StartCoroutine(SendChatGPTRequest(userInput));
    }

    // For debugging purposes only or triggering from another module/script?
    public void ManualSendToChatGPT(string input)
    {
        if (!_isProcessingGPT && !string.IsNullOrEmpty(input))
        {
            SendToChatGPT(input);
        }
    }

    private IEnumerator SendChatGPTRequest(string userInput)
    {
        List<ChatMessage> messages = new List<ChatMessage>();
        
        if (saveConversationHistory)
        {
            messages.AddRange(_conversationHistory);
        }
        else
        {
            // Add system message if not using history should only happen in debug mode
            messages.Add(new ChatMessage 
            { 
                role = "system", 
                content = "You are a helpful virtual assistant in a VR application that helps a user troubleshoot his bambulab A1. Keep responses concise and help the user find the best troubleshooting guide." 
            });
        }
        
        ChatMessage userMessage = new ChatMessage { role = "user", content = userInput };
        messages.Add(userMessage);
        
        ChatGPTRequestBody requestBody = new ChatGPTRequestBody
        {
            model = model,
            messages = messages,
            temperature = temperature,
            max_tokens = maxTokens
        };
        
        string requestJson;
        try
        {
            requestJson = JsonConvert.SerializeObject(requestBody, Formatting.None);
            Debug.Log("Request JSON: " + requestJson);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to serialize request: " + e.Message);
            _isProcessingGPT = false;
            onProcessingComplete?.Invoke();
            HandleGPTResponse("Sorry, I had trouble preparing the request.");
            yield break;
        }

        // Request creation
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        Debug.Log("Sending request to ChatGPT API...");
        HandleGPTResponse("Sending request to servers");
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Network Error: {request.error}");
            Debug.LogError($"Response Code: {request.responseCode}");
            Debug.LogError($"Response Text: {request.downloadHandler.text}");
            HandleGPTResponse("Sorry, I couldn't connect to my knowledge database right now.");
        }
        else
        {
            try
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log("Full Response JSON: " + responseJson);
                
                ChatGPTResponseBody response = JsonConvert.DeserializeObject<ChatGPTResponseBody>(responseJson);
                
                if (response?.choices != null && response.choices.Count > 0 && response.choices[0].message != null)
                {
                    string aiResponse = response.choices[0].message.content;
                    Debug.Log("AI Response: " + aiResponse);
                    
                    if (saveConversationHistory)
                    {
                        _conversationHistory.Add(userMessage);
                        _conversationHistory.Add(new ChatMessage { role = "assistant", content = aiResponse });
                        
                        while (_conversationHistory.Count > maxHistoryMessages + 1)
                        {
                            _conversationHistory.RemoveAt(1); 
                            if (_conversationHistory.Count > 1)
                                _conversationHistory.RemoveAt(1); 
                        }
                    }
                    
                    HandleGPTResponse(aiResponse);
                }
                else
                {
                    Debug.LogError("Invalid response structure from ChatGPT");
                    Debug.LogError("Response: " + responseJson);
                    HandleGPTResponse("Sorry, I received an unexpected response format.");
                }
            }
            catch (JsonException jsonEx)
            {
                Debug.LogError("JSON parsing error: " + jsonEx.Message);
                Debug.LogError("Response text: " + request.downloadHandler.text);
                HandleGPTResponse("Sorry, I had trouble understanding the response.");
            }
            catch (Exception e)
            {
                Debug.LogError("Error processing GPT response: " + e.Message);
                Debug.LogError("Full response: " + request.downloadHandler.text);
                HandleGPTResponse("Sorry, I had trouble processing the response.");
            }
        }
        
        _isProcessingGPT = false;
        onProcessingComplete?.Invoke();
    }
    
    private void HandleGPTResponse(string response)
    {
        onGPTResponseReceived?.Invoke(response);
        
        if (autoPlayResponse && ttsSpeaker != null)
        {
            if (ttsSpeaker.IsSpeaking)
            {
                ttsSpeaker.Stop();
            }
            
            ttsSpeaker.Speak(response);
        }
    }
    
    public void ClearConversationHistory()
    {
        if (_conversationHistory.Count > 0)
        {
            ChatMessage systemMessage = _conversationHistory[0];
            _conversationHistory.Clear();
            _conversationHistory.Add(systemMessage);
        }
    }
}

// Classes for JSON serialization
[Serializable]
public class ChatGPTRequestBody
{
    public string model;
    public List<ChatMessage> messages;
    public float temperature;
    [JsonProperty("max_tokens")]
    public int max_tokens;
}

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[Serializable]
public class ChatGPTResponseBody
{
    public string id;
    [JsonProperty("object")]
    public string objectType;
    public long created;
    public string model;
    public List<ChatGPTChoice> choices;
    public ChatGPTUsage usage;
}

[Serializable]
public class ChatGPTChoice
{
    public ChatMessage message;
    public int index;
    [JsonProperty("finish_reason")]
    public string finishReason;
}

[Serializable]
public class ChatGPTUsage
{
    [JsonProperty("prompt_tokens")]
    public int promptTokens;
    [JsonProperty("completion_tokens")]
    public int completionTokens;
    [JsonProperty("total_tokens")]
    public int totalTokens;
}