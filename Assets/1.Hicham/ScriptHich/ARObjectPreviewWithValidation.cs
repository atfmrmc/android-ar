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
    public GameObject reticle;           // Reticle dans la sc�ne
    public Button validateButton;        // Bouton Valider
    public GameObject inventoryUI;       // Panel inventaire
    public Button[] inventoryButtons;    // Boutons d�objet � s�lectionner

    private GameObject previewObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject selectedPrefab = null;
    private GameObject currentPreviewPrefab = null; // Prefab utilis� pour la preview actuelle

    void Start()
    {
        // Valider d�sactiv� au d�part
        if (validateButton != null)
        {
            validateButton.gameObject.SetActive(false);
            validateButton.onClick.AddListener(OnValidateButtonClicked);
        }

        // Inventaire actif au d�part
        if (inventoryUI != null)
            inventoryUI.SetActive(true);

        // Associer chaque bouton � la s�lection d�un objet
        foreach (Button btn in inventoryButtons)
            btn.onClick.AddListener(() => OnSelectInventoryObject(btn));

        if (reticle != null)
            reticle.SetActive(false);
    }

    void Update()
    {
        // Si aucun objet s�lectionn�, masquer tout
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

            // Recr�er la preview si le prefab a chang� ou si elle n'existe pas
            if (previewObject == null || currentPreviewPrefab != selectedPrefab)
            {
                if (previewObject != null) Destroy(previewObject);

                previewObject = Instantiate(selectedPrefab);
                previewObject.name = selectedPrefab.name + "_Preview";

                // D�sactiver interactions physiques
                foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;
                foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>())
                    rb.isKinematic = true;

                currentPreviewPrefab = selectedPrefab;
            }

            previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            previewObject.SetActive(true);

            if (reticle != null && !reticle.activeSelf)
                reticle.SetActive(true);

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

            // D�truire la preview actuelle pour recr�er la nouvelle
            if (previewObject != null)
            {
                Destroy(previewObject);
                previewObject = null;
            }
        }
    }

    void OnValidateButtonClicked()
    {
        if (previewObject == null || selectedPrefab == null)
            return;

        // Instancier l'objet final � la position de la preview
        Instantiate(selectedPrefab, previewObject.transform.position, previewObject.transform.rotation);

        // Nettoyer preview
        Destroy(previewObject);
        previewObject = null;
        currentPreviewPrefab = null;
        selectedPrefab = null;

        if (validateButton != null)
            validateButton.gameObject.SetActive(false);
        if (reticle != null)
            reticle.SetActive(false);

        // Inventaire peut r�appara�tre si besoin
        if (inventoryUI != null)
            inventoryUI.SetActive(true);
    }
}
