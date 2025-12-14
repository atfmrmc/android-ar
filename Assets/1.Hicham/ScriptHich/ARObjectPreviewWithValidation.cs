using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARObjectPreviewWithValidation : MonoBehaviour
{
    [Header("AR References")]
    public ARRaycastManager raycastManager;
    public ObjectSpawner objectSpawner;

    [Header("UI")]
    public GameObject validateButton;
    public GameObject cancelButton;

    [Header("Reticle")]
    public GameObject reticle;

    private GameObject previewObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool surfaceFound = false;
    private bool waitingForValidation = false;

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    void Start()
    {
        // Cacher boutons au départ
        if (validateButton != null)
        {
            validateButton.SetActive(false);
            validateButton.GetComponent<Button>().onClick.AddListener(OnValidateButtonClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.SetActive(false);
            cancelButton.GetComponent<Button>().onClick.AddListener(OnCancelButtonClicked);
        }
    }

    void Update()
    {
        if (!waitingForValidation)
            UpdatePreview();

        DetectTap();
    }

    private void UpdatePreview()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            surfaceFound = true;

            GameObject prefab = objectSpawner.objectPrefabs[objectSpawner.spawnOptionIndex];

            // Créer preview si nécessaire
            if (previewObject == null || previewObject.name != prefab.name + "(Preview)")
            {
                if (previewObject != null)
                    Destroy(previewObject);

                previewObject = Instantiate(prefab, hitPose.position, hitPose.rotation);
                previewObject.name = prefab.name + "(Preview)";

                // Preview = pas d’interactions
                foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;
                foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>())
                    rb.isKinematic = true;
            }

            previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

            if (!previewObject.activeSelf) previewObject.SetActive(true);
            if (!reticle.activeSelf) reticle.SetActive(true);
        }
        else
        {
            surfaceFound = false;

            if (previewObject) previewObject.SetActive(false);
            if (reticle.activeSelf) reticle.SetActive(false);
        }
    }

    private void DetectTap()
    {
        if (!surfaceFound || waitingForValidation)
            return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartValidation();
        }
    }

    private void StartValidation()
    {
        if (previewObject == null) return;

        waitingForValidation = true;

        savedPosition = previewObject.transform.position;
        savedRotation = previewObject.transform.rotation;

        SetPreviewTransparency(0.5f);

        if (validateButton != null) validateButton.SetActive(true);
        if (cancelButton != null) cancelButton.SetActive(true);
        if (reticle.activeSelf) reticle.SetActive(false);
    }

    private void OnValidateButtonClicked()
    {
        if (previewObject == null) return;

        // Poser l’objet final
        objectSpawner.TrySpawnObject(savedPosition, Vector3.up);

        Destroy(previewObject);
        previewObject = null;

        validateButton.SetActive(false);
        cancelButton.SetActive(false);
        waitingForValidation = false;

        if (!reticle.activeSelf) reticle.SetActive(true);
    }

    private void OnCancelButtonClicked()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        waitingForValidation = false;

        if (validateButton != null) validateButton.SetActive(false);
        if (cancelButton != null) cancelButton.SetActive(false);
        if (!reticle.activeSelf) reticle.SetActive(true);
    }

    private void SetPreviewTransparency(float alpha)
    {
        if (previewObject == null) return;

        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                Color c = m.color;
                c.a = alpha;
                m.color = c;
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = 3000;
            }
        }
    }
}
