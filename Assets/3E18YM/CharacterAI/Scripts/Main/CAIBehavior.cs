using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Animancer;
using CrazyMinnow.SALSA;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ModestTree;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Random = UnityEngine.Random;

namespace CharacterAI
{
    /// <summary>
    /// 處理腳色觸發行為，說話姿態，Idle行為等
    /// </summary>
    public class CAIBehavior : MonoBehaviour
    {
        #region ValueRegion

        #region GameObjectRegion
        [Header("GameObjectRegion")]
        [Inject] StatusManage statusManage;
        [SerializeField] private AnimancerComponent animancer;
        [SerializeField] private CharacterBehaviorData characterBehavior;
        CAIHandler cai;
        Eyes eyes;

        #endregion
        #region Setting    
        [Header("RandomMotionSetting")]
        [MinMaxSlider(0, 10)][SerializeField] Vector2 changeSpeed = new Vector2(3, 5);
        [MinMaxSlider(0, 10)][SerializeField] Vector2 stayTime = new Vector2(1, 5);
        [SerializeField] Ease motionChangeEase = Ease.InOutSine;
        [SerializeField] private AvatarMask upperBodyMask;
        #endregion
        #region PrivateValues
        [Header("AnimationsClip")]
        //assets
        [SerializeField] ClipTransition currentClip;
        [SerializeField] MixerTransition2DAsset moveMotion;
        [SerializeField] Dictionary<string, ClipTransition> clips;
        [SerializeField] Dictionary<string, LinearMixerTransitionAsset> randomMotions;

        //methods
        private Dictionary<ActionType, Func<ActionData, Task>> actionMethods = new Dictionary<ActionType, Func<ActionData, Task>>();
        private Dictionary<ActionType, Action> stopActionMethods = new Dictionary<ActionType, Action>();
        //layers
        private AnimancerLayer baseLayer;
        private AnimancerLayer upperBodyLayer;
        private AnimancerLayer animationLayer;
        private AnimancerState currentState;
        private AnimancerState lastState;

        #endregion
        public UnityEvent onStartEvent, onEndEvent, onSpeechEvent, endSpeechEvent, tagEvent, onListenEvent, endListenEvent, idleEvent, onThinkingEvent, endThinkEvent, onWaitEvent, endWaitEvent;

        #region CTS
        CancellationTokenSource ctsRandomMotion = new CancellationTokenSource();
        CancellationTokenSource ctsAnimation = new CancellationTokenSource();
        private CancellationTokenSource ctsEmotion = new CancellationTokenSource();
        private CancellationTokenSource ctsAiResponse = new CancellationTokenSource();
        private CancellationTokenSource ctsTextResponse = new CancellationTokenSource();



        #endregion

        #endregion

        #region Initial

        [Button]
        void StatusTest(Status status)
        {
            if (!statusManage.CheckStatus(status))
            {
                statusManage.EnableStatus(status);


            }
            else
            {
                statusManage.DisableStatus(status);
            }

        }
        public async Task Initial(CAIHandler cAIHandler, CharacterBehaviorData behaviorDatas = null)
        {
            cai = cAIHandler;
            animancer.TryGetComponent<Eyes>(out eyes);

            if (behaviorDatas != null)
            {
                characterBehavior = behaviorDatas;
            }
            RegionBehavior();
            RegionActionMethod();
            InitialLayer();
            await InitialBehaviorData(characterBehavior);
            statusManage.EnableStatus(Status.Idle);
            Idle();


        }

        /// <summary>
        /// 創建Layer，啟用FootIK
        /// </summary>
        private void InitialLayer()
        {
            //創建BaseLayer
            baseLayer = animancer.Layers[0];
            baseLayer.SetDebugName("BaseLayer Layer");
            baseLayer.ApplyFootIK = true;
            //創建SpeechLayer
            upperBodyLayer = animancer.Layers[1];
            upperBodyLayer.SetMask(upperBodyMask);
            upperBodyLayer.ApplyFootIK = true;
            upperBodyLayer.SetDebugName("UpperBody Layer");
        }

        /// <summary>
        /// 預加載Addressable Asset
        /// </summary>
        /// <param name="characterData"></param>
        async Task InitialBehaviorData(CharacterBehaviorData characterData)
        {
            //加載讀取需要的動畫        
            clips = new Dictionary<string, ClipTransition>();
            randomMotions = new Dictionary<string, LinearMixerTransitionAsset>();
            await ResolveAssets(characterData.onStart);
            await ResolveAssets(characterData.onEnd);
            await ResolveAssets(characterData.speech);
            characterData.tag.ForEach(async x => await ResolveAssets(x));
            await ResolveAssets(characterData.listen);
            await ResolveAssets(characterData.idle);
            await ResolveAssets(characterData.thinking);
            await ResolveAssets(characterData.wait);


        }

        /// <summary>
        /// 從ActionData 解讀取得資源位址儲存至PrivateValues
        /// </summary>
        /// <param name="behaviorData"></param>
        /// <returns></returns>
        async Task ResolveAssets(BehaviorData behaviorData)
        {
            if (behaviorData != null && behaviorData.actionList.Count != 0)
            {
                string assetPath = "";
                foreach (var action in behaviorData.actionList)
                {
                    switch (action.actionType)
                    {
                        case ActionType.RandomMotion:
                            assetPath = $"Assets/Character/Animations/RandomMotion/{action.content}.asset";
                            var motions = await assetPath.LoadFromAddressable<LinearMixerTransitionAsset>();
                            randomMotions.Add(action.content, motions);

                            break;
                        case ActionType.Animation:
                            assetPath = $"Assets/Character/Animations/AnimationClip/{action.content}.anim";
                            var clip = await assetPath.LoadFromAddressable<AnimationClip>();
                            ClipTransition clipTransition = new ClipTransition();
                            clipTransition.Clip = clip;
                            clips.Add(action.content, clipTransition);
                            break;
                        case ActionType.Emotion:
                            break;


                    }

                }

            }

        }
        #endregion


        #region BehaviorMethod
        void RegionBehavior()
        {
            OnStart();
            OnEnd();
            Speech();
            Tag();
            Listen();
            Idle();
            Thinking();
            Wait();

        }
        void OnStart()
        {
            var behavior = characterBehavior.onStart;
            if (behavior != null)
            {
                statusManage.AddOnStartListener<string>(Status.OnStart, async (text) =>
                {
                    onStartEvent?.Invoke();
                    await ExecuteAction(behavior.actionList);
                });
            }


        }
        void OnEnd()
        {
            var behavior = characterBehavior.onEnd;
            if (behavior != null)
            {
                statusManage.AddOnStartListener<string>(Status.OnEnd, async (text) =>
                {
                    onEndEvent?.Invoke();
                    await ExecuteAction(behavior.actionList);
                });
            }
        }
        void Speech()
        {
            var behavior = characterBehavior.speech;
            if (behavior != null)
            {
                statusManage.AddOnStartListener<string>(Status.SpeechAudioSource, async (text) =>
                {
                    if (eyes != null)
                        eyes.useAffinity = false;
                    onSpeechEvent?.Invoke();
                    await ExecuteAction(behavior.actionList);

                });
                statusManage.AddOnEndListener<string>(Status.SpeechAudioSource, async (text) =>
                {
                    endSpeechEvent?.Invoke();
                    await StopAction(behavior.actionList);
                    if (eyes != null)
                        eyes.useAffinity = true;
                });
            }

        }
        void Tag()
        {
            var behaviors = characterBehavior.tag;
            if (behaviors != null & !behaviors.IsEmpty())
            {
                statusManage.AddOnStartListener<string>(Status.SpeechBookMark, async (text) =>
                    {
                        var behavior = behaviors.FirstOrDefault(x => x.condition == text);
                        if (behavior != null)
                        {
                            tagEvent?.Invoke();
                            await ExecuteAction(behavior.actionList);
                        }
                    }
                );
            }
        }
        void Listen()
        {
            var behavior = characterBehavior.listen;
            if (behavior != null)
            {
                statusManage.AddOnStartListener<string>(Status.Recording, async (text) =>
                {
                    onListenEvent?.Invoke();
                    await ExecuteAction(behavior.actionList);
                });
                statusManage.AddOnEndListener<string>(Status.Recording, async (text) =>
                {
                    endListenEvent?.Invoke();
                    await StopAction(behavior.actionList);
                });
            }
        }
        void Idle()
        {
            var behavior = characterBehavior.idle;
            idleEvent?.Invoke();
            ExecuteAction(behavior.actionList).ConfigureAwait(false);

        }
        void Thinking()
        {
            var behavior = characterBehavior.listen;
            if (behavior != null)
            {
                statusManage.AddOnStartListener<string>(Status.SendToAI, async (text) =>
                {
                    onThinkingEvent?.Invoke();
                    await ExecuteAction(behavior.actionList);
                });
                statusManage.AddOnEndListener<string>(Status.SendToAI, async (text) =>
                {
                    endThinkEvent?.Invoke();
                    await StopAction(behavior.actionList);
                });
            }
        }
        void Wait()
        {
            var behavior = characterBehavior.wait;
            if (behavior != null && !behavior.actionList.IsEmpty())
            {
                statusManage.AddOnStartListener<string>(Status.Idle, async (text) =>
                {
                    onWaitEvent?.Invoke();
                    await UniTask.WaitForSeconds(float.Parse(behavior.condition));
                    await ExecuteAction(behavior.actionList);
                    statusManage.EnableStatus(Status.Wait);
                });
                statusManage.AddOnEndListener<string>(Status.Idle, async (text) =>
                {
                    endWaitEvent?.Invoke();
                    await StopAction(behavior.actionList);
                    statusManage.DisableStatus(Status.Wait);

                });
            }
        }
        #endregion

        #region TriggerActions   
        /// <summary>
        /// 從ActionList依序執行每一個Action
        /// </summary>
        /// <param name="actionDatas"></param>
        /// <returns></returns>
        async Task ExecuteAction(List<ActionData> actionDatas)
        {
            if (this.actionMethods.Count == 0)
                RegionActionMethod();

            if (actionDatas != null && !actionDatas.IsEmpty())
            {
                foreach (var actionData in actionDatas)
                {
                    if (this.actionMethods.TryGetValue(actionData.actionType, out var handler))
                    {
                        await handler(actionData).ConfigureAwait(actionData.wait);
                    }
                    else
                    {
                        Debug.LogError($"Unhandled action type: {actionData.actionType}");
                    }
                    if (statusManage.CheckStatus(Status.Idle))
                    {
                        statusManage.DisableStatus(Status.Idle);

                    }
                }

            }

        }
        /// <summary>
        /// 從ActionList 取消執行每個Action
        /// </summary>
        /// <param name="actionDatas"></param>
        /// <returns></returns>
        async Task StopAction(List<ActionData> actionDatas)
        {
            if (stopActionMethods.Count == 0)
                RegionActionMethod();
            if (actionDatas != null && !actionDatas.IsEmpty())
            {
                foreach (var action in actionDatas)
                {
                    if (stopActionMethods.TryGetValue(action.actionType, out var handler))
                    {
                        handler();
                    }
                    else
                    {
                        Debug.LogError($"Unhandled action type: {action.actionType}");
                    }
                }
                if (!statusManage.CheckStatus(Status.Idle))
                {
                    statusManage.EnableStatus(Status.Idle);

                }

            }



        }
        private void RegionActionMethod()
        {
            // 初始化字典，映射每种 ActionType 到相应的处理方法
            actionMethods = new Dictionary<ActionType, Func<ActionData, Task>>
            {
                { ActionType.RandomMotion, RandomMotion},
                { ActionType.Animation, Animation },
                { ActionType.Emotion, Emotion },
                { ActionType.AiResponse, AiResponse },
                { ActionType.TextResponse, TextResponse },

            };

            stopActionMethods = new Dictionary<ActionType, Action>
            {
                { ActionType.RandomMotion,()=> ForceStop(ref ctsRandomMotion)},
                { ActionType.Animation,()=> ForceStop(ref ctsAnimation) },
                { ActionType.Emotion,()=> ForceStop(ref ctsEmotion) },
                { ActionType.AiResponse,()=> ForceStop(ref ctsAiResponse) },
                { ActionType.TextResponse,()=> ForceStop(ref ctsTextResponse) },

            };
        }

        #endregion

        #region ActionMethod  

        public async Task MoveToSpot(Transform target, float speed = 1)
        {
            if (ctsRandomMotion == null)
                ctsRandomMotion = new CancellationTokenSource();

            var characterTransform = animancer.gameObject.transform;
            Vector3 targetPosition = target.position;
            Vector3 currentPosition = characterTransform.position;
            float angleDifference = Mathf.DeltaAngle(target.eulerAngles.y, characterTransform.eulerAngles.y);

            float turn = Mathf.Sin(Mathf.Deg2Rad * angleDifference) > 0 ? -0.5f : 0.5f;
            float dis = Vector3.Distance(targetPosition, currentPosition);


            var turnMixer = moveMotion.Transition.Animations[0] as LinearMixerTransitionAsset;
            baseLayer.Play(moveMotion);


            if (turnMixer != null)
            {
                turnMixer.Transition.State.Parameter = turn;
            }

            while (dis > 0.1 || math.abs(angleDifference) > 5)
            {
                currentPosition = characterTransform.position;
                dis = Vector3.Distance(targetPosition, currentPosition);
                angleDifference = Mathf.DeltaAngle(target.eulerAngles.y, characterTransform.eulerAngles.y);

                Vector3 dir;

                if (math.abs(angleDifference) > 5)
                {
                    dir = (targetPosition - currentPosition).normalized * speed * math.sqrt(dis);
                }
                else
                {
                    dir = (targetPosition - currentPosition).normalized * speed;
                    turnMixer.Transition.State.Parameter = 0;

                }
                float forwardAmount = Vector3.Dot(dir, characterTransform.right);
                float rightAmount = Vector3.Dot(dir, characterTransform.forward);
                moveMotion.Transition.State.Parameter = new Vector2(forwardAmount, rightAmount);

                await Task.Yield();

            }
            Idle();


        }
        async Task RandomMotion(ActionData action)
        {
            var motions = randomMotions[action.content];
            await RandomChangePose(motions);
            upperBodyLayer.StartFade(0, 0.5f);

        }
        async Task RandomIdleSpeechPose(LinearMixerTransitionAsset motions, Ease ease = Ease.InOutSine)
        {
            if (ctsRandomMotion != null)
            {
                ForceStop(ref ctsRandomMotion);
            }

            ctsRandomMotion = new CancellationTokenSource();

            while (true)
            {
                //觸發隨機motion 或Idle

                bool isMotion = Random.Range(0, 2) == 0;
                Debug.Log(isMotion);
                if (isMotion)
                {

                    float motionIndex = Random.Range(0.0f, 1.0f);
                    upperBodyLayer.Play(motions, 0.5f);
                    motions.Transition.State.Parameter = motionIndex;
                    await UniTask.WaitUntil(() => motions.Transition.State.NormalizedTime % 1 > 0.95f, cancellationToken: ctsRandomMotion.Token);


                }
                else
                {
                    if (upperBodyLayer.Weight != 0)
                        upperBodyLayer.StartFade(0, 0.5f);
                    baseLayer.Play(currentClip, 0.5f);
                    await UniTask.WaitForSeconds(Random.Range(stayTime.x, stayTime.y), cancellationToken: ctsRandomMotion.Token);
                }



                if (ctsRandomMotion == null)
                {

                    return;
                }

            }

        }
        async Task RandomChangePose(LinearMixerTransitionAsset motions, Ease ease = Ease.InOutSine)
        {

            if (ctsRandomMotion == null)
                ctsRandomMotion = new CancellationTokenSource();


            upperBodyLayer.Play(motions, 0.5f);

            while (true)
            {

                float motionIndex = Random.Range(0.0f, 1.0f);
                float speed = Random.Range(changeSpeed.x, changeSpeed.y);
                Tween tween = DOTween.To(() => motions.Transition.State.Parameter, x => motions.Transition.State.Parameter = x, motionIndex, speed).SetEase(ease);
                try
                {
                    await UniTask.WaitUntil(() => !tween.IsPlaying(), cancellationToken: ctsRandomMotion.Token);
                }
                catch
                {
                    Debug.Log("Stop RandomAnimation");
                }
                if (ctsRandomMotion == null)
                {

                    return;
                }

            }



        }
        async Task Animation(ActionData action)//考慮loop 以及添加持續時間
        {
            if (ctsAnimation == null)
                ctsAnimation = new CancellationTokenSource();

            var clip = clips[action.content];

            if (action.loop == true)
            {
                currentState = animancer.Layers[action.layer].Play(clip);
                currentState.NormalizedEndTime = float.PositiveInfinity;

                if (action.duration > 0)
                {
                    await UniTask.WaitForSeconds(action.duration, cancellationToken: ctsAnimation.Token);
                }
                else
                {
                    await UniTask.WaitUntil(() => ctsAnimation == null);
                }


            }
            else
            {
                for (int t = 0; t < action.time; t++)
                {
                    currentState = animancer.Layers[action.layer].Play(clip);
                    currentState.NormalizedTime = 0;
                    await UniTask.WaitUntil(() => currentState.NormalizedTime > 0.95f, cancellationToken: ctsAnimation.Token);
                    Debug.Log($"play time {t}");

                }


            }
            if (action.layer != 0)
            {
                animancer.Layers[action.layer].StartFade(0, 0.5f);
            }


        }
        async Task TagState(string tag)
        {



        }
        async Task Emotion(ActionData action)
        {


        }
        async Task AiResponse(ActionData action)
        {




        }
        async Task TextResponse(ActionData action)
        {


        }


        private void ForceStop(ref CancellationTokenSource cts)
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;

            }

        }

        #endregion
    }
}