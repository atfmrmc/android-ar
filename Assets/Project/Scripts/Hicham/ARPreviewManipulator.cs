using UnityEngine;

public class ARPreviewManipulation : MonoBehaviour
{
    [Header("Manipulation Settings")]
    public float rotationSpeed = 0.5f;
    public float minScale = 0.5f;
    public float maxScale = 3f;

    [Header("Visual Feedback")]
    public Color previewColor = new Color(1f, 1f, 1f, 0.7f);

    private float initialPinchDistance;
    private Vector3 initialScale;
    private bool isScaling = false;

    private Material[] originalMaterials;
    private Material[] previewMaterials;

    void Start()
    {
        initialScale = transform.localScale;
        ApplyPreviewEffect();
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            HandleRotation();
        }
        else if (Input.touchCount >= 2)
        {
            HandleScale();
        }
        else
        {
            isScaling = false;
        }
    }

    void HandleRotation()
    {
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Moved)
        {
            Vector2 delta = touch.deltaPosition;

            transform.Rotate(Vector3.up, delta.x * rotationSpeed, Space.World);
            transform.Rotate(Camera.main.transform.right, -delta.y * rotationSpeed, Space.World);
        }
    }

    void HandleScale()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
            initialScale = transform.localScale;
            isScaling = true;
        }
        else if ((touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved) && isScaling)
        {
            float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);
            float scaleFactor = currentPinchDistance / initialPinchDistance;

            float clamped = Mathf.Clamp(initialScale.x * scaleFactor, minScale, maxScale);
            transform.localScale = Vector3.one * clamped;
        }
    }

    void ApplyPreviewEffect()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        previewMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;

            Material previewMat = new Material(renderers[i].material);
            if (previewMat.HasProperty("_Color"))
            {
                Color c = previewMat.color;
                c.a = previewColor.a;
                previewMat.color = c;
            }

            previewMaterials[i] = previewMat;
            renderers[i].material = previewMat;
        }
    }

    public void RestoreOriginalMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length && i < originalMaterials.Length; i++)
        {
            if (renderers[i] != null && originalMaterials[i] != null)
                renderers[i].material = originalMaterials[i];
        }
    }

    void OnDestroy()
    {
        if (previewMaterials != null)
        {
            foreach (Material mat in previewMaterials)
            {
                if (mat != null)
                    Destroy(mat);
            }
        }
    }
}
