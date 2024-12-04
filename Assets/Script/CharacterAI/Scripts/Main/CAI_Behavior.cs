using Animancer;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System.Threading.Tasks;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Zenject;
using System;
using Random = UnityEngine.Random;
using ModestTree;
using Unity.Mathematics;
using UnityEngine.Events;
using CrazyMinnow.SALSA;
/// <summary>
/// 處理腳色觸發行為，說話姿態，Idle行為等
/// </summary>
public class CAI_Behavior : MonoBehaviour
{
    #region ValueRegion

    #region GameObjectRegion
    [Header("GameObjectRegion")]
    [Inject] Status_Manage status_Manage;
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private CharacterBehaviorData CharacterBehavior;
    CAI_handler CAI;
    Eyes eyes;

    #endregion
    #region Setting    
    [Header("RandomMotionSetting")]
    [MinMaxSlider(0, 10)][SerializeField] Vector2 changeSpeed = new Vector2(3, 5);
    [MinMaxSlider(0, 10)][SerializeField] Vector2 stayTime = new Vector2(1, 5);
    [SerializeField] Ease MotionChangeEase = Ease.InOutSine;
    [SerializeField] private AvatarMask UpperBodyMask;
    #endregion
    #region PrivateValues
    [Header("AnimationsClip")]
    //assets
    [SerializeField] ClipTransition currentClip;
    [SerializeField] MixerTransition2DAsset moveMotion;
    [SerializeField] Dictionary<string, ClipTransition> Clips;
    [SerializeField] Dictionary<string, LinearMixerTransitionAsset> RandomMotions;

    //methods
    private Dictionary<ActionType, Func<ActionData, Task>> ActionMethods = new Dictionary<ActionType, Func<ActionData, Task>>();
    private Dictionary<ActionType, Action> stopActionMethods = new Dictionary<ActionType, Action>();
    //layers
    private AnimancerLayer BaseLayer;
    private AnimancerLayer UpperBodyLayer;
    private AnimancerLayer AnimationLayer;
    private AnimancerState currentState;
    private AnimancerState LastState;

    #endregion
    public UnityEvent OnStartEvent, OnEndEvent, OnSpeechEvent, EndSpeechEvent, TagEvent, OnListenEvent, EndListenEvent, IdleEvent, OnThinkingEvent, EndThinkEvent, OnWaitEvent, EndWaitEvent;

    #region CTS
    CancellationTokenSource cts_randomMotion = new CancellationTokenSource();
    CancellationTokenSource cts_Animation = new CancellationTokenSource();
    private CancellationTokenSource cts_Emotion = new CancellationTokenSource();
    private CancellationTokenSource cts_AiResponse = new CancellationTokenSource();
    private CancellationTokenSource cts_TextResponse = new CancellationTokenSource();



    #endregion

    #endregion

    #region Initial

    [Button]
    void StatusTest(Status status)
    {
        if (!status_Manage.CheckStatus(status))
        {
            status_Manage.EnableStatus(status);


        }
        else
        {
            status_Manage.DisableStatus(status);
        }

    }
    public async Task Initial(CAI_handler cAI_Handler, CharacterBehaviorData behaviorDatas = null)
    {
        CAI = cAI_Handler;
        animancer.TryGetComponent<Eyes>(out eyes);

        if (behaviorDatas != null)
        {
            CharacterBehavior = behaviorDatas;
        }
        RegionBehavior();
        RegionActionMethod();
        InitialLayer();
        await InitialBehaviorData(CharacterBehavior);
        status_Manage.EnableStatus(Status.Idle);
        Idle();


    }

    /// <summary>
    /// 創建Layer，啟用FootIK
    /// </summary>
    private void InitialLayer()
    {
        //創建BaseLayer
        BaseLayer = animancer.Layers[0];
        BaseLayer.SetDebugName("BaseLayer Layer");
        BaseLayer.ApplyFootIK = true;
        //創建SpeechLayer
        UpperBodyLayer = animancer.Layers[1];
        UpperBodyLayer.SetMask(UpperBodyMask);
        UpperBodyLayer.ApplyFootIK = true;
        UpperBodyLayer.SetDebugName("UpperBody Layer");
    }

    /// <summary>
    /// 預加載Addressable Asset
    /// </summary>
    /// <param name="characterData"></param>
    async Task InitialBehaviorData(CharacterBehaviorData characterData)
    {
        //加載讀取需要的動畫        
        Clips = new Dictionary<string, ClipTransition>();
        RandomMotions = new Dictionary<string, LinearMixerTransitionAsset>();
        await ResolveAssets(characterData.OnStart);
        await ResolveAssets(characterData.OnEnd);
        await ResolveAssets(characterData.Speech);
        characterData.Tag.ForEach(async x => await ResolveAssets(x));
        await ResolveAssets(characterData.Listen);
        await ResolveAssets(characterData.Idle);
        await ResolveAssets(characterData.Thinking);
        await ResolveAssets(characterData.Wait);


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
                        assetPath = $"Assets/Character/Animations/RandomMotion/{action.Content}.asset";
                        var motions = await assetPath.LoadFromAddressable<LinearMixerTransitionAsset>();
                        RandomMotions.Add(action.Content, motions);

                        break;
                    case ActionType.Animation:
                        assetPath = $"Assets/Character/Animations/AnimationClip/{action.Content}.anim";
                        var clip = await assetPath.LoadFromAddressable<AnimationClip>();
                        ClipTransition clipTransition = new ClipTransition();
                        clipTransition.Clip = clip;
                        Clips.Add(action.Content, clipTransition);
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
        var behavior = CharacterBehavior.OnStart;
        if (behavior != null)
        {
            status_Manage.AddOnStartListener<string>(Status.OnStart, async (text) =>
            {
                OnStartEvent?.Invoke();
                await ExecuteAction(behavior.actionList);
            });
        }


    }
    void OnEnd()
    {
        var behavior = CharacterBehavior.OnEnd;
        if (behavior != null)
        {
            status_Manage.AddOnStartListener<string>(Status.OnEnd, async (text) =>
            {
                OnEndEvent?.Invoke();
                await ExecuteAction(behavior.actionList);
            });
        }
    }
    void Speech()
    {
        var behavior = CharacterBehavior.Speech;
        if (behavior != null)
        {
            status_Manage.AddOnStartListener<string>(Status.SpeechAudioSource, async (text) =>
            {
                if (eyes != null)
                    eyes.useAffinity = false;
                OnSpeechEvent?.Invoke();
                await ExecuteAction(behavior.actionList);

            });
            status_Manage.AddOnEndListener<string>(Status.SpeechAudioSource, async (text) =>
            {
                EndSpeechEvent?.Invoke();
                await StopAction(behavior.actionList);
                if (eyes != null)
                    eyes.useAffinity = true;
            });
        }

    }
    void Tag()
    {
        var behaviors = CharacterBehavior.Tag;
        if (behaviors != null & !behaviors.IsEmpty())
        {
            status_Manage.AddOnStartListener<string>(Status.SpeechBookMark, async (text) =>
                {
                    var behavior = behaviors.FirstOrDefault(x => x.condition == text);
                    if (behavior != null)
                    {
                        TagEvent?.Invoke();
                        await ExecuteAction(behavior.actionList);
                    }
                }
            );
        }
    }
    void Listen()
    {
        var behavior = CharacterBehavior.Listen;
        if (behavior != null)
        {
            status_Manage.AddOnStartListener<string>(Status.Recording, async (text) =>
            {
                OnListenEvent?.Invoke();
                await ExecuteAction(behavior.actionList);
            });
            status_Manage.AddOnEndListener<string>(Status.Recording, async (text) =>
            {
                EndListenEvent?.Invoke();
                await StopAction(behavior.actionList);
            });
        }
    }
    void Idle()
    {
        var behavior = CharacterBehavior.Idle;
        IdleEvent?.Invoke();
        ExecuteAction(behavior.actionList).ConfigureAwait(false);

    }
    void Thinking()
    {
        var behavior = CharacterBehavior.Listen;
        if (behavior != null)
        {
            status_Manage.AddOnStartListener<string>(Status.SendToAI, async (text) =>
            {
                OnThinkingEvent?.Invoke();
                await ExecuteAction(behavior.actionList);
            });
            status_Manage.AddOnEndListener<string>(Status.SendToAI, async (text) =>
            {
                EndThinkEvent?.Invoke();
                await StopAction(behavior.actionList);
            });
        }
    }
    void Wait()
    {
        var behavior = CharacterBehavior.Wait;
        if (behavior != null && !behavior.actionList.IsEmpty())
        {
            status_Manage.AddOnStartListener<string>(Status.Idle, async (text) =>
            {
                OnWaitEvent?.Invoke();
                await UniTask.WaitForSeconds(float.Parse(behavior.condition));
                await ExecuteAction(behavior.actionList);
                status_Manage.EnableStatus(Status.Wait);
            });
            status_Manage.AddOnEndListener<string>(Status.Idle, async (text) =>
            {
                EndWaitEvent?.Invoke();
                await StopAction(behavior.actionList);
                status_Manage.DisableStatus(Status.Wait);

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
        if (this.ActionMethods.Count == 0)
            RegionActionMethod();

        if (actionDatas != null && !actionDatas.IsEmpty())
        {
            foreach (var actionData in actionDatas)
            {
                if (this.ActionMethods.TryGetValue(actionData.actionType, out var handler))
                {
                    await handler(actionData).ConfigureAwait(actionData.wait);
                }
                else
                {
                    Debug.LogError($"Unhandled action type: {actionData.actionType}");
                }
                if (status_Manage.CheckStatus(Status.Idle))
                {
                    status_Manage.DisableStatus(Status.Idle);

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
            if (!status_Manage.CheckStatus(Status.Idle))
            {
                status_Manage.EnableStatus(Status.Idle);

            }

        }



    }
    private void RegionActionMethod()
    {
        // 初始化字典，映射每种 ActionType 到相应的处理方法
        ActionMethods = new Dictionary<ActionType, Func<ActionData, Task>>
        {
            { ActionType.RandomMotion, RandomMotion},
            { ActionType.Animation, Animation },
            { ActionType.Emotion, Emotion },
            { ActionType.AiResponse, AiResponse },
            { ActionType.TextResponse, TextResponse },

        };

        stopActionMethods = new Dictionary<ActionType, Action>
        {
            { ActionType.RandomMotion,()=> ForceStop(ref cts_randomMotion)},
            { ActionType.Animation,()=> ForceStop(ref cts_Animation) },
            { ActionType.Emotion,()=> ForceStop(ref cts_Emotion) },
            { ActionType.AiResponse,()=> ForceStop(ref cts_AiResponse) },
            { ActionType.TextResponse,()=> ForceStop(ref cts_TextResponse) },

        };
    }

    #endregion

    #region ActionMethod  

    public async Task MoveToSpot(Transform Target, float speed = 1)
    {
        if (cts_randomMotion == null)
            cts_randomMotion = new CancellationTokenSource();

        var characterTransform = animancer.gameObject.transform;
        Vector3 TargetPosition = Target.position;
        Vector3 CurrentPosition = characterTransform.position;
        float angleDifference = Mathf.DeltaAngle(Target.eulerAngles.y, characterTransform.eulerAngles.y);

        float turn = Mathf.Sin(Mathf.Deg2Rad * angleDifference) > 0 ? -0.5f : 0.5f;
        float dis = Vector3.Distance(TargetPosition, CurrentPosition);


        var turnMixer = moveMotion.Transition.Animations[0] as LinearMixerTransitionAsset;
        BaseLayer.Play(moveMotion);


        if (turnMixer != null)
        {
            turnMixer.Transition.State.Parameter = turn;
        }

        while (dis > 0.1 || math.abs(angleDifference) > 5)
        {
            CurrentPosition = characterTransform.position;
            dis = Vector3.Distance(TargetPosition, CurrentPosition);
            angleDifference = Mathf.DeltaAngle(Target.eulerAngles.y, characterTransform.eulerAngles.y);

            Vector3 dir;

            if (math.abs(angleDifference) > 5)
            {
                dir = (TargetPosition - CurrentPosition).normalized * speed * math.sqrt(dis);
            }
            else
            {
                dir = (TargetPosition - CurrentPosition).normalized * speed;
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
        var motions = RandomMotions[action.Content];
        await RandomChangePose(motions);
        UpperBodyLayer.StartFade(0, 0.5f);

    }
    async Task RandomIdleSpeechPose(LinearMixerTransitionAsset motions, Ease ease = Ease.InOutSine)
    {
        if (cts_randomMotion != null)
        {
            ForceStop(ref cts_randomMotion);
        }

        cts_randomMotion = new CancellationTokenSource();

        while (true)
        {
            //觸發隨機motion 或Idle

            bool isMotion = Random.Range(0, 2) == 0;
            Debug.Log(isMotion);
            if (isMotion)
            {

                float motionIndex = Random.Range(0.0f, 1.0f);
                UpperBodyLayer.Play(motions, 0.5f);
                motions.Transition.State.Parameter = motionIndex;
                await UniTask.WaitUntil(() => motions.Transition.State.NormalizedTime % 1 > 0.95f, cancellationToken: cts_randomMotion.Token);


            }
            else
            {
                if (UpperBodyLayer.Weight != 0)
                    UpperBodyLayer.StartFade(0, 0.5f);
                BaseLayer.Play(currentClip, 0.5f);
                await UniTask.WaitForSeconds(Random.Range(stayTime.x, stayTime.y), cancellationToken: cts_randomMotion.Token);
            }



            if (cts_randomMotion == null)
            {

                return;
            }

        }

    }
    async Task RandomChangePose(LinearMixerTransitionAsset motions, Ease ease = Ease.InOutSine)
    {

        if (cts_randomMotion == null)
            cts_randomMotion = new CancellationTokenSource();


        UpperBodyLayer.Play(motions, 0.5f);

        while (true)
        {

            float motionIndex = Random.Range(0.0f, 1.0f);
            float speed = Random.Range(changeSpeed.x, changeSpeed.y);
            Tween tween = DOTween.To(() => motions.Transition.State.Parameter, x => motions.Transition.State.Parameter = x, motionIndex, speed).SetEase(ease);
            try
            {
                await UniTask.WaitUntil(() => !tween.IsPlaying(), cancellationToken: cts_randomMotion.Token);
            }
            catch
            {
                Debug.Log("Stop RandomAnimation");
            }
            if (cts_randomMotion == null)
            {

                return;
            }

        }



    }
    async Task Animation(ActionData action)//考慮loop 以及添加持續時間
    {
        if (cts_Animation == null)
            cts_Animation = new CancellationTokenSource();

        var clip = Clips[action.Content];

        if (action.loop == true)
        {
            currentState = animancer.Layers[action.layer].Play(clip);
            currentState.NormalizedEndTime = float.PositiveInfinity;

            if (action.duration > 0)
            {
                await UniTask.WaitForSeconds(action.duration, cancellationToken: cts_Animation.Token);
            }
            else
            {
                await UniTask.WaitUntil(() => cts_Animation == null);
            }


        }
        else
        {
            for (int t = 0; t < action.time; t++)
            {
                currentState = animancer.Layers[action.layer].Play(clip);
                currentState.NormalizedTime = 0;
                await UniTask.WaitUntil(() => currentState.NormalizedTime > 0.95f, cancellationToken: cts_Animation.Token);
                Debug.Log($"play time {t}");

            }


        }
        if (action.layer != 0)
        {
            animancer.Layers[action.layer].StartFade(0, 0.5f);
        }


    }
    async Task TagState(string Tag)
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