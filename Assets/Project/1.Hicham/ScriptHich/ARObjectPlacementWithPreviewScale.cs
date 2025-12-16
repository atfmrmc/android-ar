using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARObjectPlacementWithPreviewScale : MonoBehaviour
{
    [Header("AR References")]
    public ARRaycastManager raycastManager;
    public ObjectSpawner objectSpawner;

    [Header("UI & Reticle")]
    public GameObject reticle;
    public Button validateButton;
    public Button[] inventoryButtons;

    private GameObject previewObject;
    private GameObject selectedPrefab = null;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        if (reticle != null) reticle.SetActive(false);
        if (validateButton != null) validateButton.gameObject.SetActive(false);

        foreach (Button btn in inventoryButtons)
            btn.onClick.AddListener(() => SelectPrefab(btn));

        if (validateButton != null)
            validateButton.onClick.AddListener(ValidatePlacement);
    }

    void Update()
    {
        if (selectedPrefab == null)
            return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (previewObject == null)
            {
                previewObject = Instantiate(selectedPrefab);
                previewObject.name = selectedPrefab.name + "_Preview";

                // Désactiver collisions
                foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;

                // Ajouter le script pour scale
                previewObject.AddComponent<PreviewScaler>();
            }

            previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            previewObject.SetActive(true);

            if (reticle != null) reticle.SetActive(true);
            if (validateButton != null) validateButton.gameObject.SetActive(true);
        }
        else
        {
            if (previewObject != null) previewObject.SetActive(false);
            if (reticle != null) reticle.SetActive(false);
        }
    }

    void SelectPrefab(Button btn)
    {
        int index = System.Array.IndexOf(inventoryButtons, btn);
        if (index >= 0 && index < objectSpawner.objectPrefabs.Count)
        {
            selectedPrefab = objectSpawner.objectPrefabs[index];

            if (previewObject != null)
                Destroy(previewObject);
        }
    }

    void ValidatePlacement()
    {
        if (previewObject == null || selectedPrefab == null)
            return;

        // Activer colliders
        foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
            c.enabled = true;

        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // Supprimer le script de scale
        PreviewScaler scaler = previewObject.GetComponent<PreviewScaler>();
        if (scaler != null)
            Destroy(scaler);

        previewObject.name = previewObject.name.Replace("_Preview", "");
        previewObject = null;
        selectedPrefab = null;

        if (validateButton != null) validateButton.gameObject.SetActive(false);
    }
}

// Script séparé pour gérer le scale de la preview
public class PreviewScaler : MonoBehaviour
{
    public float minScale = 0.5f;
    public float maxScale = 3f;

    private Vector3 initialScale;
    private float initialDistance;
    private bool scaling = false;

    void Update()
    {
        if (Input.touchCount >= 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(t0.position, t1.position);
                initialScale = transform.localScale;
                scaling = true;
            }
            else if (scaling && (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
            {
                float currentDistance = Vector2.Distance(t0.position, t1.position);
                float scaleFactor = currentDistance / initialDistance;

                Vector3 newScale = initialScale * scaleFactor;
                float clamped = Mathf.Clamp(newScale.x, minScale, maxScale);
                transform.localScale = Vector3.one * clamped;
            }
        }
        else
        {
            scaling = false;
        }
    }
}
