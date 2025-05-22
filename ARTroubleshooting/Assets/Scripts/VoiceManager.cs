using System;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.Events;

public class VoiceManager : MonoBehaviour
{


    //[Header("Wit configuration")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private WitResponseMatcher responseMatcher;
    [SerializeField] public string transcriptedText;

    //[Header("Voice Events")]
    [SerializeField] private UnityEvent wakeWordDetected;
    [SerializeField] private UnityEvent<string> completeTranscription;

    private bool _voiceCommandReady;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
        Debug.Log("WAKEWORD DETECTEDDDD");
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
        _voiceCommandReady = false;
        transcriptedText = transcription;
        Debug.Log("FULL TRANSCRIPTION: " + transcription);
        completeTranscription?.Invoke(transcription);
    }

}
