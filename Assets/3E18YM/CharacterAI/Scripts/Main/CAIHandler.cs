using System;
using System.Threading;
using System.Threading.Tasks;
using CrazyMinnow.SALSA;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace CharacterAI
{
    public class CAIHandler : MonoBehaviour
    {

        #region regionGameObject
        [Header("regionGameObject")]
        [InjectOptional] public DiContainer DiContainer;
        [SerializeReference, OdinSerialize, InjectOptional] public ITextToSpeech textToSpeech;
        [SerializeReference, OdinSerialize, InjectOptional] public ISpeechToText speechToText;
        [SerializeReference, OdinSerialize, InjectOptional] public IAIServices aIServices;
        [InjectOptional] public ModelController modelController;

        #endregion characterData

        [TabGroup("characterData")] public CharacterData characterData;
        #region CharacterElement
        [TabGroup("CharacterElement")] public StatusManage StatusManage = new StatusManage ();
        [TabGroup("CharacterElement")] [SerializeField]public SpeechQueue speechQueue = new SpeechQueue();
        [TabGroup("CharacterElement")] [SerializeField]public ListenHandler listenHandler = new ListenHandler();
        #endregion
        #region LoaderSetting
        [TabGroup("LoaderSetting")] public bool loadOnStart;
        [TabGroup("LoaderSetting")] public Transform characterContainer;
        [TabGroup("LoaderSetting")] public Transform eyesTarget;
        [TabGroup("LoaderSetting")][SerializeField] private CharacterLoadingList loadingDatas;
        [TabGroup("LoaderSetting"), InjectOptional] public UnityEvent<CharacterData> onCharacterChanged;
        #endregion

        #region PrivateValue
        private SemaphoreSlim loadCharacterSemaphore = new SemaphoreSlim(1, 1);
        private readonly AsyncQueue<string> queue = new AsyncQueue<string>();
        private bool isProcessing = false;
        #endregion

        void BindToDiContainer()
        {
            DiContainer.BindInstance(textToSpeech).AsSingle();
            DiContainer.BindInstance(speechToText).AsSingle();
            DiContainer.BindInstance(aIServices).AsSingle();
            DiContainer.BindInstance(characterData).AsSingle();
            DiContainer.BindInstance(speechQueue).AsSingle();
            DiContainer.BindInstance(listenHandler).AsSingle();
            DiContainer.BindInstance(StatusManage).AsSingle();
            DiContainer.BindInstance(this).AsSingle();

        }

        async void Start()
        {   
            StatusManage = new StatusManage();
            if (DiContainer == null)
            {
                DiContainer = new DiContainer();
                BindToDiContainer();
            }
            DiContainer.Inject(listenHandler);
            DiContainer.Inject(speechQueue);
            
            StatusManage.AddStatusListener<string>(Status.AIReply, (text) => queue.Enqueue(text));

            if (loadOnStart)
            {
                await LoadCharacter(characterData);
            }
        }


        [Button]
        public async Task LoadCharacter(CharacterData characterData = null)
        {
            this.characterData = characterData ?? this.characterData;
            await loadCharacterSemaphore.WaitAsync();

            try
            {
                LoadModel(this.characterData.modelID);
            
                DiContainer.Inject(modelController.cAIBehavior);
                await modelController.cAIBehavior.Initial(this, characterData.behaviorData);
            
                aIServices?.SetAssistant(characterData.assistantID);
                onCharacterChanged?.Invoke(characterData);
               
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                loadCharacterSemaphore.Release();
                throw;
            }


        }
        [Button]
        public void LoadModel(int characterIndex)
        {
            if (loadingDatas == null) return;

            foreach (Transform child in characterContainer)
            {
                if (child.gameObject.tag == "Character")
                {
                    Destroy(child.gameObject);
                }


            }

            GameObject prefab = loadingDatas.modelDataList[characterIndex].prefab;
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

            textToSpeech.audioSource = modelController.audioSource;

        }
        public async Task Speak(SpeechCommand speechCommand, bool insert = false)
        {
            if (insert && StatusManage.CheckStatus(Status.Speech))
            {
                speechQueue.ForceStopAndResetQueue();
                await Task.Delay(100);
            }
            await speechQueue.SendToSpeechQueue(speechCommand);
        }
        public async Task Speech(string message, string transition = null, string language = null, string voiceID = null, float rate = -1)
        {

            SpeechCommand speechCommand =
                new SpeechCommand(
                    message.ToString(),
                    transition,
                    new TtsData(
                        rate: rate == -1 ? characterData.tTsData.rate : rate,
                        language: language ?? characterData.tTsData.language,
                        voiceID: voiceID ?? characterData.tTsData.voiceID
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
            string autoDetectLanguage = $"reply in this language: {speechToText.currentDetectLanguage}";
            await aIServices.SendMessages(message + autoDetectLanguage);
        }




        private async Task ProcessQueue()
        {
            if (!isProcessing && queue.Count > 0)
            {
                isProcessing = true;
                var text = await queue.DequeueAsync();
                await Speech(text);
                isProcessing = false;
            }
        }

        public async Task MoveToSpot(Transform target, float speed = 1)
        {
            await modelController.cAIBehavior.MoveToSpot(target, speed);
        }
        [Button]
        public void ForceStop()
        {
            speechQueue?.ForceStopAndResetQueue();
            listenHandler?.ForceStop();
            aIServices?.ForceStop();
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
}

