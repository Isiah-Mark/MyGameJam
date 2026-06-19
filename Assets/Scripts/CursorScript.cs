using UnityEngine;
using UnityEngine.InputSystem; 

public class CursorScript : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private AudioClip clickAudio; 
    private Vector2 cursorHotspot;                   

    void Start()
    {
        if (cursorTexture != null)
        {
            cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (clickAudio != null)
            {
                AudioSource.PlayClipAtPoint(clickAudio, transform.position);
            }
        }
    }
}