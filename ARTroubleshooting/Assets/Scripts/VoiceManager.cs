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
    [SerializeField] private string fallbackModel = "gpt-3.5-turbo"; 
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
            ttsSpeaker = GetComponent<TTSSpeaker>() ?? gameObject.AddComponent<TTSSpeaker>();
            Debug.Log("TTSSpeaker component initialized.");
        }

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

        SendToChatGPT(transcription, model);
        _voiceCommandReady = false;
    }

    public void ManualSendToChatGPT(string input) {
        if (!_isProcessingGPT && !string.IsNullOrEmpty(input)) {
            SendToChatGPT(input, model);
        }
    }

    private void SendToChatGPT(string userInput, string modelToUse, bool isRetry = false)
    {
        if (_isProcessingGPT || string.IsNullOrEmpty(userInput))
            return;

        _isProcessingGPT = true;
        onProcessingStart?.Invoke();
        Debug.Log("Sending to ChatGPT: " + userInput);

        StartCoroutine(SendChatGPTRequest(userInput, modelToUse, isRetry));
    }

    private IEnumerator SendChatGPTRequest(string userInput, string currentModel, bool isRetry)
    {
        List<ChatMessage> messages = new List<ChatMessage>();

        if (saveConversationHistory) {
            messages.AddRange(_conversationHistory);
        } else {
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
            model = currentModel,
            messages = messages,
            temperature = temperature,
            max_tokens = maxTokens
        };

        string requestJson = JsonConvert.SerializeObject(requestBody);
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        Debug.Log("Sending request to ChatGPT API...");


        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error using model {currentModel}: {request.error}");
            Debug.LogError($"Response Text: {request.downloadHandler.text}");

            if (!isRetry && currentModel == model && !string.IsNullOrEmpty(fallbackModel))
            {
                Debug.LogWarning($"Retrying with fallback model: {fallbackModel}");
                SendToChatGPT(userInput, fallbackModel, true);
                yield break;
            }

            HandleGPTResponse("Sorry, I couldn't connect to my knowledge database right now.");
        }
        else
        {
            try
            {
                string responseJson = request.downloadHandler.text;
                ChatGPTResponseBody response = JsonConvert.DeserializeObject<ChatGPTResponseBody>(responseJson);

                if (response?.choices?.Count > 0 && response.choices[0].message != null)
                {
                    string aiResponse = response.choices[0].message.content;

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
                    HandleGPTResponse("Sorry, I received an unexpected response format.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing GPT response: " + e.Message);
                HandleGPTResponse("Sorry, I had trouble processing the response.");
            }
        }
        
        _isProcessingGPT = false;
        onProcessingComplete?.Invoke();
    }
    
    private void HandleGPTResponse(string response)
    {
        Debug.Log($"GPT Response: {response}");

        onGPTResponseReceived?.Invoke(response);

        if (autoPlayResponse && ttsSpeaker != null)
        {
            if (ttsSpeaker.IsSpeaking)
                ttsSpeaker.Stop();

            Debug.Log("Sending response to TTS...");

            ttsSpeaker.Speak(response);
        }
        else
        {
            Debug.Log("TTS not triggered: either autoPlayResponse is false or ttsSpeaker is null.");
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

// JSON Classes
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
