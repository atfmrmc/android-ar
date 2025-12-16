using UnityEngine;

public class ARPreviewManipulator : MonoBehaviour
{
    [Header("Manipulation Settings")]
    public float rotationSpeed = 0.2f;
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
            // Si on était en train de scale, on arrête
            isScaling = false;
            HandleRotation();
        }
        else if (Input.touchCount == 2)
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

            // Rotation autour de Y monde (horizontal)
            transform.Rotate(Vector3.up, delta.x * rotationSpeed, Space.World);

            // Rotation autour de X caméra (vertical)
            if (Camera.main != null)
                transform.Rotate(Camera.main.transform.right, -delta.y * rotationSpeed, Space.World);
        }
    }

    void HandleScale()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        if (!isScaling)
        {
            // Début du pinch
            initialPinchDistance = Vector2.Distance(t0.position, t1.position);
            initialScale = transform.localScale;
            isScaling = true;
            return;
        }

        float currentDistance = Vector2.Distance(t0.position, t1.position);
        if (Mathf.Approximately(initialPinchDistance, 0)) return;

        float scaleFactor = currentDistance / initialPinchDistance;
        Vector3 targetScale = initialScale * scaleFactor;

        // Clamp par axe
        targetScale.x = Mathf.Clamp(targetScale.x, minScale, maxScale);
        targetScale.y = Mathf.Clamp(targetScale.y, minScale, maxScale);
        targetScale.z = Mathf.Clamp(targetScale.z, minScale, maxScale);

        transform.localScale = targetScale;
    }

    void ApplyPreviewEffect()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        previewMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
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
