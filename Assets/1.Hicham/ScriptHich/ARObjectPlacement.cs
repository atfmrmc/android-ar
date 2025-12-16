using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARObjectPlacement : MonoBehaviour
{
    [Header("AR References")]
    public ARRaycastManager raycastManager;
    public ObjectSpawner objectSpawner;

    [Header("Reticle & UI")]
    public GameObject reticle;
    public Button validateButton;
    public GameObject objectMenuUI;
    public Button openMenuButton;
    public Button[] inventoryButtons;

    [Header("Preview Manipulation Settings")]
    public float rotationSpeed = 0.5f;
    public float minScale = 0.5f;
    public float maxScale = 3f;

    private GameObject previewObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject selectedPrefab = null;

    void Start()
    {
        if (objectMenuUI != null)
        {
            objectMenuUI.SetActive(true);
            Animator anim = objectMenuUI.GetComponent<Animator>();
            if (anim != null) anim.Play("Open");
        }

        if (reticle != null) reticle.SetActive(false);
        if (validateButton != null) validateButton.gameObject.SetActive(false);
        if (openMenuButton != null) openMenuButton.gameObject.SetActive(false);

        foreach (Button btn in inventoryButtons)
            btn.onClick.AddListener(() => OnSelectInventoryObject(btn));

        if (validateButton != null)
            validateButton.onClick.AddListener(OnValidateButtonClicked);

        if (openMenuButton != null)
            openMenuButton.onClick.AddListener(() =>
            {
                if (objectMenuUI != null) objectMenuUI.SetActive(true);
                openMenuButton.gameObject.SetActive(false);
                Animator anim = objectMenuUI.GetComponent<Animator>();
                if (anim != null) anim.Play("Open");
            });
    }

    void Update()
    {
        if (selectedPrefab == null) return;
        UpdatePreview();
    }

    void UpdatePreview()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (previewObject == null)
            {
                previewObject = Instantiate(selectedPrefab);
                previewObject.name = selectedPrefab.name + "_Preview";

                foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;

                foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>())
                    rb.isKinematic = true;

                // Ajoute la manipulation
                ARPreviewManipulation manipulator = previewObject.AddComponent<ARPreviewManipulation>();
                manipulator.rotationSpeed = rotationSpeed;
                manipulator.minScale = minScale;
                manipulator.maxScale = maxScale;
            }

            previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            previewObject.SetActive(true);

            if (!reticle.activeSelf) reticle.SetActive(true);
            if (validateButton != null && !validateButton.gameObject.activeSelf)
                validateButton.gameObject.SetActive(true);
        }
        else
        {
            if (previewObject != null) previewObject.SetActive(false);
            if (reticle.activeSelf) reticle.SetActive(false);
        }
    }

    void OnSelectInventoryObject(Button btn)
    {
        int index = System.Array.IndexOf(inventoryButtons, btn);
        if (index >= 0 && index < objectSpawner.objectPrefabs.Count)
        {
            selectedPrefab = objectSpawner.objectPrefabs[index];

            if (objectMenuUI != null) objectMenuUI.SetActive(false);
            if (openMenuButton != null) openMenuButton.gameObject.SetActive(true);

            if (previewObject != null)
            {
                Destroy(previewObject);
                previewObject = null;
            }
        }
    }

    void OnValidateButtonClicked()
    {
        if (previewObject == null) return;

        // Supprime le script de manipulation et restaure l'apparence
        ARPreviewManipulation manipulator = previewObject.GetComponent<ARPreviewManipulation>();
        if (manipulator != null)
        {
            manipulator.RestoreOriginalMaterials();
            Destroy(manipulator);
        }

        // Active colliders et Rigidbody
        foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
            c.enabled = true;
        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // La preview devient l'objet final
        previewObject.name = previewObject.name.Replace("_Preview", "");
        previewObject = null;

        if (validateButton != null) validateButton.gameObject.SetActive(false);
        if (!reticle.activeSelf) reticle.SetActive(true);

        selectedPrefab = null;
    }
}
