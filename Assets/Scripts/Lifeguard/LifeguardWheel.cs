using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LifeguardWheel : MonoBehaviour
{
    public static LifeguardWheel Instance { get; private set; }

    [Header("Wheel")]
    public GameObject wheelRoot;
    public GameObject slicePrefab;
    public float wheelRadius = 80f;

    private Swimmer targetSwimmer;
    private LifeguardWheelSlice[] slices;
    private LifeguardWheelSlice hoveredSlice;
    private bool isOpen = false;
    private int frameOpened = -1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        wheelRoot.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (targetSwimmer != null)
            wheelRoot.transform.position = targetSwimmer.transform.position;

        HandleHover();

        if (Time.frameCount <= frameOpened) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hoveredSlice != null && hoveredSlice.IsAvailable())
            {
                SelectLifeguard(hoveredSlice.GetLifeguard());
            }
            else
            {
                Close();
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Open(Swimmer swimmer)
    {
        targetSwimmer = swimmer;
        wheelRoot.transform.position = swimmer.transform.position;
        wheelRoot.SetActive(true);
        isOpen = true;
        frameOpened = Time.frameCount;
        BuildSlices();
    }

    void BuildSlices()
    {
        foreach (Transform child in wheelRoot.transform)
            Destroy(child.gameObject);

        slices = null;
        hoveredSlice = null;

        Lifeguard[] lifeguards = LifeguardManager.Instance.GetLifeguards();
        slices = new LifeguardWheelSlice[lifeguards.Length];

        float angleStep = 360f / lifeguards.Length;

        for (int i = 0; i < lifeguards.Length; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * wheelRadius;

            GameObject sliceGO = Instantiate(slicePrefab, wheelRoot.transform);
            RectTransform rt = sliceGO.GetComponent<RectTransform>();
            rt.anchoredPosition = offset;

            LifeguardWheelSlice slice = sliceGO.GetComponent<LifeguardWheelSlice>();
            slice.Setup(lifeguards[i]);
            slices[i] = slice;
        }
    }

    void HandleHover()
    {
        if (slices == null) return;

        Vector2 rawMouse = Mouse.current.position.ReadValue();
        Vector2 mousePos = new Vector2(rawMouse.x, Screen.height - rawMouse.y);

        LifeguardWheelSlice newHovered = null;
        float closestDist = float.MaxValue;
        float maxHoverDistance = 60f;

        foreach (LifeguardWheelSlice slice in slices)
        {
            if (slice == null) continue;

            Vector2 sliceScreenPos = Camera.main.WorldToScreenPoint(slice.transform.position);
            float dist = Vector2.Distance(mousePos, sliceScreenPos);

            if (dist < closestDist && dist < maxHoverDistance)
            {
                closestDist = dist;
                newHovered = slice;
            }
        }

        if (newHovered != hoveredSlice)
        {
            if (hoveredSlice != null) hoveredSlice.OnUnhover();
            hoveredSlice = newHovered;
            if (hoveredSlice != null) hoveredSlice.OnHover();
        }
    }

    void SelectLifeguard(Lifeguard lifeguard)
    {
        lifeguard.AssignTarget(targetSwimmer);
        Close();
    }

    void Close()
    {
        if (hoveredSlice != null) hoveredSlice.OnUnhover();
        hoveredSlice = null;
        slices = null;
        targetSwimmer = null;
        wheelRoot.SetActive(false);
        isOpen = false;
    }
}