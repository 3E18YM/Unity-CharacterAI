using Unity.Mathematics;
using UnityEngine;
using Sirenix.OdinInspector;
public class ModelController : MonoBehaviour
{
    public Animator animator;
    public CAI_Behavior cAI_Behavior;
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
        cAI_Behavior = GetComponentInChildren<CAI_Behavior>();
        audioSource = GetComponentInChildren<AudioSource>();

    }
}

