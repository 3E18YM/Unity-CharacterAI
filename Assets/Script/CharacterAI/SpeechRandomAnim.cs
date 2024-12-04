using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Cysharp;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;



public class SpeechRandomAnim : MonoBehaviour
{

    public AudioSource audioSource;
    [Header("Pose")]
    public float ChangeSpeed_Min = 3;
    public float ChangeSpeed_Max = 5;
    public Ease MotionChangeEase = Ease.InOutSine;
    [Header("Layer")]
    public float LayerLerpTime = 0.5f;
    public Ease layerLerpEase = Ease.InOutSine;
    Animator animator;

    bool speechMotion = false;
    CancellationTokenSource cts = new CancellationTokenSource();


    // Start is called before the first frame update

    async void Start()
    {

        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    public async void Update()
    {

        if (audioSource.isPlaying && !speechMotion)
        {
            speechMotion = true;
            StartRandomAnim().ConfigureAwait(false);

        }
        else if (!audioSource.isPlaying && speechMotion)
        {
            speechMotion = false;
            ForceStop();
        }

    }
    public async Task StartRandomAnim()
    {

        SetAnimatorLayerWeight(1, 1, LayerLerpTime, layerLerpEase).ConfigureAwait(false);
        await RandomChangePose(MotionChangeEase);
        SetAnimatorLayerWeight(1, 0, LayerLerpTime, layerLerpEase).ConfigureAwait(false);

    }
    [Button]
    private void ForceStop()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;

        }

    }
    async Task RandomChangePose(Ease ease = Ease.InOutSine)
    {

        if (cts != null)
        {
            ForceStop();
        }
        cts = new CancellationTokenSource();
        while (true)
        {

            float motionIndex = Random.Range(0.0f, 1.0f);
            float ChangeSpeed = Random.Range(ChangeSpeed_Min, ChangeSpeed_Max);
            Tween tween = DOTween.To(() => animator.GetFloat("Speech_Random"), x => animator.SetFloat("Speech_Random", x), motionIndex, ChangeSpeed).SetEase(ease);
            try
            {
                await UniTask.WaitUntil(() => !tween.IsPlaying(), cancellationToken: cts.Token);
            }
            catch
            {
                Debug.Log("Stop RandomAnimation");
            }
            if (cts == null)
            {

                return;
            }

        }


    }
    async Task SetAnimatorLayerWeight(int animatorLayer, float Weight, float Duration, Ease ease)
    {
        await DOTween.To(() => animator.GetLayerWeight(animatorLayer), x => animator.SetLayerWeight(animatorLayer, x), Weight, Duration).SetEase(ease).AsyncWaitForCompletion();
    }

}
