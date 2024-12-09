using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace CharacterAI
{
    public class ModelController : MonoBehaviour
    {
        public Animator animator;
        public CAIBehavior cAIBehavior;
        public AudioSource audioSource;


        // Start is called before the first frame update
        [Button]
        public void Initial()
        {
            animator.Play("DefaultState");
            animator.transform.localPosition = Vector3.zero;
            animator.transform.localRotation = quaternion.Euler(Vector3.zero);
        }
        [Button]
        public void GetReference()
        {
            animator = GetComponentInChildren<Animator>();
            cAIBehavior = GetComponentInChildren<CAIBehavior>();
            audioSource = GetComponentInChildren<AudioSource>();

        }
    }
}

