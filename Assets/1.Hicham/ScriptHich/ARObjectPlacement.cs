using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
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
    [Tooltip("Sensibilité de la rotation")]
    public float rotationSpeed = 0.5f;
    [Tooltip("Scale minimum")]
    public float minScale = 0.5f;
    [Tooltip("Scale maximum")]
    public float maxScale = 3f;

    private GameObject previewObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool waitingForValidation = false;

    private GameObject selectedPrefab = null;

    void Start()
    {
        // Menu visible au démarrage
        if (objectMenuUI != null)
        {
            objectMenuUI.SetActive(true);
            Animator anim = objectMenuUI.GetComponent<Animator>();
            if (anim != null) anim.Play("Open");
        }

        if (reticle != null) reticle.SetActive(false);
        if (validateButton != null) validateButton.gameObject.SetActive(false);
        if (openMenuButton != null) openMenuButton.gameObject.SetActive(false);

        // Inventaire
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
        // Si aucun objet sélectionné, rien ne faire
        if (selectedPrefab == null)
            return;

        // Mise à jour de la preview
        if (!waitingForValidation)
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

                // Désactiver collisions physiques initialement
                foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;

                // Ajouter le script de manipulation
                ARPreviewManipulator manipulator = previewObject.AddComponent<ARPreviewManipulator>();
                manipulator.rotationSpeed = rotationSpeed;
                manipulator.minScale = minScale;
                manipulator.maxScale = maxScale;
            }

            // Position + rotation au centre de l'écran
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

            if (previewObject != null) Destroy(previewObject);
            previewObject = null;
        }
    }

    void OnValidateButtonClicked()
    {
        if (previewObject == null) return;

        // Restaurer les matériaux et supprimer le manipulateur
        ARPreviewManipulator manipulator = previewObject.GetComponent<ARPreviewManipulator>();
        if (manipulator != null)
        {
            manipulator.RestoreOriginalMaterials();
            Destroy(manipulator);
        }

        // Activer colliders pour l'objet final
        foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
            c.enabled = true;

        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        // Laisser l'objet tel quel et réinitialiser
        previewObject.name = previewObject.name.Replace("_Preview", "");
        previewObject = null;
        waitingForValidation = false;

        if (validateButton != null) validateButton.gameObject.SetActive(false);
        if (!reticle.activeSelf) reticle.SetActive(true);

        // Plus d'objet sélectionné
        selectedPrefab = null;
    }
}