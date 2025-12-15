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

    [Header("UI & Reticle")]
    public GameObject reticle;           // Reticle dans la scène
    public Button validateButton;        // Bouton Valider
    public GameObject inventoryUI;       // Panel inventaire
    public Button[] inventoryButtons;    // Boutons d’objet à sélectionner

    private GameObject previewObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject selectedPrefab = null;

    void Start()
    {
        // Le bouton Valider commence désactivé
        if (validateButton != null)
        {
            validateButton.gameObject.SetActive(false);
            validateButton.onClick.AddListener(OnValidateButtonClicked);
        }

        // Afficher inventaire au démarrage
        if (inventoryUI != null)
            inventoryUI.SetActive(true);

        // Associer chaque bouton de l’inventaire à la sélection d’un objet
        foreach (Button btn in inventoryButtons)
            btn.onClick.AddListener(() => OnSelectInventoryObject(btn));

        if (reticle != null)
            reticle.SetActive(false);
    }

    void Update()
    {
        // Si aucun objet sélectionné, cacher preview, reticle et bouton Valider
        if (selectedPrefab == null)
        {
            if (previewObject != null)
                previewObject.SetActive(false);
            if (reticle != null)
                reticle.SetActive(false);
            if (validateButton != null)
                validateButton.gameObject.SetActive(false);
            return;
        }

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

                // Désactiver interactions physiques
                foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;
                foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>())
                    rb.isKinematic = true;
            }

            previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            previewObject.SetActive(true);

            if (reticle != null && !reticle.activeSelf)
                reticle.SetActive(true);

            // Afficher le bouton Valider uniquement si surface détectée
            if (validateButton != null && !validateButton.gameObject.activeSelf)
                validateButton.gameObject.SetActive(true);
        }
        else
        {
            if (previewObject != null)
                previewObject.SetActive(false);
            if (reticle != null && reticle.activeSelf)
                reticle.SetActive(false);
            if (validateButton != null && validateButton.gameObject.activeSelf)
                validateButton.gameObject.SetActive(false);
        }
    }

    void OnSelectInventoryObject(Button btn)
    {
        int index = System.Array.IndexOf(inventoryButtons, btn);
        if (index >= 0 && index < objectSpawner.objectPrefabs.Count)
        {
            selectedPrefab = objectSpawner.objectPrefabs[index];

            // Cacher le panel inventaire une fois un objet choisi
            if (inventoryUI != null)
                inventoryUI.SetActive(false);
        }
    }

    void OnValidateButtonClicked()
    {
        if (previewObject == null || selectedPrefab == null)
            return;

        // Instancier l'objet final à la position de la preview
        Instantiate(selectedPrefab, previewObject.transform.position, previewObject.transform.rotation);
    }
}
