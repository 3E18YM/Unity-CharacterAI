using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharacterAI
{
    public class SpeechRandomAnim : MonoBehaviour
    {

        public AudioSource audioSource;
        [Header("Pose")]
        public float changeSpeedMin = 3;
        public float changeSpeedMax = 5;
        public Ease motionChangeEase = Ease.InOutSine;
        [Header("Layer")]
        public float layerLerpTime = 0.5f;
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

            SetAnimatorLayerWeight(1, 1, layerLerpTime, layerLerpEase).ConfigureAwait(false);
            await RandomChangePose(motionChangeEase);
            SetAnimatorLayerWeight(1, 0, layerLerpTime, layerLerpEase).ConfigureAwait(false);

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
                float changeSpeed = Random.Range(changeSpeedMin, changeSpeedMax);
                Tween tween = DOTween.To(() => animator.GetFloat("Speech_Random"), x => animator.SetFloat("Speech_Random", x), motionIndex, changeSpeed).SetEase(ease);
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
        async Task SetAnimatorLayerWeight(int animatorLayer, float weight, float duration, Ease ease)
        {
            await DOTween.To(() => animator.GetLayerWeight(animatorLayer), x => animator.SetLayerWeight(animatorLayer, x), weight, duration).SetEase(ease).AsyncWaitForCompletion();
        }

    }
}
