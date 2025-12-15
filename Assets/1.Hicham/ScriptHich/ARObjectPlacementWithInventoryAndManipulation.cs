using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARObjectPlacementWithInventoryAndManipulation : MonoBehaviour
{
    [Header("AR References")]
    public ARRaycastManager raycastManager;
    public ObjectSpawner objectSpawner;

    [Header("UI & Reticle")]
    public GameObject reticle;             // Reticle AR
    public Button validateButton;          // Bouton Valider
    public GameObject objectMenuUI;        // Panel du menu "Object Menu"
    public Button[] inventoryButtons;      // Boutons des objets à sélectionner
    public Button openMenuButton;          // Bouton pour ré-ouvrir le menu

    [Header("Manipulation Settings")]
    public float rotationSpeed = 100f;
    public float scaleSpeed = 0.01f;

    private GameObject previewObject;      // Preview de l’objet
    private GameObject selectedPrefab;     // Objet choisi dans le menu
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        // Reticle et bouton Valider désactivés au départ
        if (reticle != null) reticle.SetActive(false);
        if (validateButton != null) validateButton.gameObject.SetActive(false);

        // Menu actif par défaut
        if (objectMenuUI != null) objectMenuUI.SetActive(true);

        // Bouton pour rouvrir le menu désactivé
        if (openMenuButton != null) openMenuButton.gameObject.SetActive(false);
        if (openMenuButton != null) openMenuButton.onClick.AddListener(() =>
        {
            if (objectMenuUI != null) objectMenuUI.SetActive(true);
            openMenuButton.gameObject.SetActive(false);
        });

        // Associer chaque bouton du menu à la sélection
        foreach (Button btn in inventoryButtons)
        {
            btn.onClick.AddListener(() => OnSelectInventoryObject(btn));
        }

        // Bouton Valider
        if (validateButton != null)
        {
            validateButton.onClick.AddListener(OnValidateButtonClicked);
        }
    }

    void Update()
    {
        // Si aucun objet sélectionné, rien ne se passe
        if (selectedPrefab == null) return;

        UpdatePreview();
        HandleManipulation();
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
                    c.enabled = true;

                // Ajouter XRGrabInteractable pour manipuler
                if (previewObject.GetComponent<XRGrabInteractable>() == null)
                    previewObject.AddComponent<XRGrabInteractable>();
            }

            previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            previewObject.SetActive(true);

            if (reticle != null && !reticle.activeSelf) reticle.SetActive(true);
            if (validateButton != null && !validateButton.gameObject.activeSelf)
                validateButton.gameObject.SetActive(true);
        }
        else
        {
            if (previewObject != null) previewObject.SetActive(false);
            if (reticle != null && reticle.activeSelf) reticle.SetActive(false);
            if (validateButton != null && validateButton.gameObject.activeSelf)
                validateButton.gameObject.SetActive(false);
        }
    }

    void HandleManipulation()
    {
        if (previewObject == null) return;

        // Rotation avec deux doigts (similaire à pinch rotation)
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 prevDir = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).normalized;
            Vector2 curDir = (touch0.position - touch1.position).normalized;

            float angle = Vector2.SignedAngle(prevDir, curDir);
            previewObject.transform.Rotate(Vector3.up, -angle * rotationSpeed * Time.deltaTime, Space.World);

            // Scale avec distance des doigts
            float prevDistance = (touch0.position - touch0.deltaPosition - (touch1.position - touch1.deltaPosition)).magnitude;
            float curDistance = (touch0.position - touch1.position).magnitude;
            float delta = (curDistance - prevDistance) * scaleSpeed;
            previewObject.transform.localScale += Vector3.one * delta;
        }
    }

    void OnSelectInventoryObject(Button btn)
    {
        int index = System.Array.IndexOf(inventoryButtons, btn);
        if (index >= 0 && index < objectSpawner.objectPrefabs.Count)
        {
            selectedPrefab = objectSpawner.objectPrefabs[index];

            // Masquer le menu après sélection
            if (objectMenuUI != null) objectMenuUI.SetActive(false);

            // Activer le bouton pour rouvrir le menu si défini
            if (openMenuButton != null) openMenuButton.gameObject.SetActive(true);

            // Supprimer l’ancienne preview
            if (previewObject != null) Destroy(previewObject);
            previewObject = null;
        }
    }

    void OnValidateButtonClicked()
    {
        if (previewObject == null || selectedPrefab == null) return;

        GameObject finalObject = Instantiate(
            selectedPrefab,
            previewObject.transform.position,
            previewObject.transform.rotation
        );
        finalObject.transform.localScale = previewObject.transform.localScale;

        Destroy(previewObject);
        previewObject = null;

        if (validateButton != null) validateButton.gameObject.SetActive(false);
        if (reticle != null && !reticle.activeSelf) reticle.SetActive(true);
    }
}
