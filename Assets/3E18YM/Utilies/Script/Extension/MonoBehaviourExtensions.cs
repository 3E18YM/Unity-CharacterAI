using UnityEngine;

public static class MonoBehaviourExtensions
{
    public static void DestroyAllChild(this MonoBehaviour monoBehaviour)
    {
        Transform transform = monoBehaviour.transform;

        if (transform.childCount != 0)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}