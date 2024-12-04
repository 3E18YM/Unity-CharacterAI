using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrazyMinnow.SALSA;
using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

using Unity.Mathematics;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Serialization;


public class CAI_handler : MonoBehaviour
{
    #region LoaderSetting
    [Header("LoaderSetting")]
    [SerializeField] private bool LoadOnStart;
    public Transform characterContainer;
    public Transform eyesTarget;
    #endregion

    #region regionGameObject
    [Header("regionGameObject")]
    [Inject] public DiContainer container;
    [InjectOptional] public ISpeechToText STT;
    [InjectOptional] public ITextToSpeech TTS;
    [InjectOptional] public IAIServices aIServices;
    [Inject] public CharacterData characterData;
    [Inject] public SpeechQueue speechQueue;
    public ListenHandler listenHandler = new ListenHandler();
    [InjectOptional] Status_Manage status_Manage;
    public UnityEvent<CharacterData> OnCharacterChanged;


    #endregion
    
    #region PrivateValue
    
    private ModelController modelController;
    private List<ModelData> ModelDatas;
    private SemaphoreSlim loadCharacterSemaphore = new SemaphoreSlim(1, 1);
    private readonly AsyncQueue<string> queue = new AsyncQueue<string>();
    private bool isProcessing = false;
    
    #endregion

    async void Start()
    {
       
        container.Inject(listenHandler);
        listenHandler.initial();

        status_Manage.AddStatusListener<string>(Status.AIReply, (text) => queue.Enqueue(text));
        if (LoadOnStart)
        {
            await LoadCharacter(characterData);

        }
    }

  
    [Button]
    public async Task LoadCharacter([NotNull] CharacterData characterData)
    {
        if (characterData == null) return; 

        await loadCharacterSemaphore.WaitAsync();

        try
        {
            LoadModel(characterData.modelID);
            container.Inject(modelController.cAI_Behavior);
            await modelController.cAI_Behavior.Initial(this, characterData.behaviorData);
            aIServices?.SetAssistant(characterData.assistantID);
            this.characterData = characterData;
            OnCharacterChanged?.Invoke(characterData);


        }
        finally
        {
            loadCharacterSemaphore.Release();

        }

    }
    [Button]
    public void LoadModel(int characterIndex)
    {   
        if(ModelDatas == null) return;
        
        foreach (Transform Child in characterContainer)
        {
            if (Child.gameObject.tag == "Character")
            {
                Destroy(Child.gameObject);
            }


        }
        
        GameObject prefab = ModelDatas[characterIndex].prefab;
        var newGameObject = Instantiate(
           prefab,
           Vector3.zero,
           Quaternion.identity,
           characterContainer
       );
        newGameObject.transform.SetAsFirstSibling();

        //設定說話動畫
        modelController = newGameObject.GetComponent<ModelController>();

        //初始化位置
        newGameObject.transform.localPosition = Vector3.zero;
        newGameObject.transform.localRotation = quaternion.Euler(Vector3.zero);

        Animator animator = modelController.animator;
        
        try
        {
            Eyes eyes = animator.GetComponent<Eyes>();

            if (eyesTarget == null)
            {
                eyesTarget = Camera.main.transform;
            }

            eyes.lookTarget = eyesTarget;

        }
        catch
        {
            Debug.Log("Eyes Aim Target not found");
        }

        TTS._audioSource = modelController.audioSource;

    }
    public async Task Speak(SpeechCommand speechCommand, bool insert = false)
    {
        TTS._audioSource = modelController.audioSource;
        if (insert && status_Manage.CheckStatus(Status.Speech))
        {
            speechQueue.ForceStopAndResetQueue();
            await Task.Delay(100);
        }
        await speechQueue.SendToSpeechQueue(speechCommand);
    }
    public async Task Speech(string message, string transition = null, string language = null, string VoiceID = null, float Rate = -1)
    {
        TTS._audioSource = modelController.audioSource;
        SpeechCommand speechCommand =
        new SpeechCommand(
                message.ToString(),
                transition,
                new TTSData(
                    Rate: Rate == -1 ? characterData.tTSData.rate : Rate,
                    Language: language ?? characterData.tTSData.language,
                    VoiceID: VoiceID ?? characterData.tTSData.voiceID
                )
        );

        await speechQueue.SendToSpeechQueue(speechCommand);


    }
    [Button]
    public async void SpeechTest(string text)
    {
        await Speech(text);
    }
    public void SendToAI(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        aIServices.SendMessages(message).ConfigureAwait(false);
    }
    public async Task SendToAIAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        string autoDetectLanguage = $"reply in this language: {STT.currentDetectLanguage}";
        await aIServices.SendMessages(message + autoDetectLanguage);
    }

    


    private async Task ProcessQueue()
    {
        if (!isProcessing && queue.Count > 0)
        {
            isProcessing = true;
            var _text = await queue.DequeueAsync();
            await Speech(_text);
            isProcessing = false;
        }
    }

    public async Task MoveToSpot(Transform Target, float speed = 1)
    {
        await modelController.cAI_Behavior.MoveToSpot(Target, speed);
    }
    [Button]
    public void ForceStop()
    {
        speechQueue?.ForceStopAndResetQueue();
        listenHandler?.ForceStop();
        aIServices?.ForceStop();
        STT?.ForceStop();
        queue.Clear();
        isProcessing = false;

    }
    void OnDestroy()
    {
        ForceStop();
    }
    // Update is called once per frame
    async void Update()
    {
        await ProcessQueue();

    }
}

