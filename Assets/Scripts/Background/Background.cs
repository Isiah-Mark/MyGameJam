using UnityEngine;

public class Background : MonoBehaviour
{
    void Start()
    {
        transform.localScale = new Vector3(36f, 20f, 1f);
        transform.position = new Vector3(0f, 0f, 1f);
    }
}