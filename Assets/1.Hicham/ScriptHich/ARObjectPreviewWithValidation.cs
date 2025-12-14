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

    [Header("Reticle & UI")]
    public GameObject reticle;           // Référence au reticle
    public GameObject validateButton;    // Référence au bouton Valider

    private GameObject previewObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool waitingForValidation = false;

    // Position et rotation figées pour l'objet final
    private Vector3 savedPosition;
    private Quaternion savedRotation;

    void Start()
    {
        if (validateButton != null)
        {
            validateButton.SetActive(false);
            validateButton.GetComponent<Button>().onClick.AddListener(OnValidateButtonClicked);
        }
    }

    void Update()
    {
        // Mise à jour de la preview uniquement si on n'attend pas la validation
        if (!waitingForValidation)
            UpdatePreview();

        // Détecte le touch pour commencer la validation, uniquement si la preview est active sur une surface
        if (!waitingForValidation && previewObject != null && previewObject.activeSelf && hits.Count > 0)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
                StartValidation();
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                StartValidation();
#endif
        }
    }

    void UpdatePreview()
    {
        // Si on attend la validation, on fige la preview
        if (waitingForValidation)
        {
            if (previewObject != null)
                previewObject.transform.SetPositionAndRotation(savedPosition, savedRotation);
            return; // On sort pour ne pas suivre le reticle
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            GameObject prefab = objectSpawner.objectPrefabs[objectSpawner.spawnOptionIndex];

            if (previewObject == null || previewObject.name != prefab.name + "_Preview")
            {
                if (previewObject != null)
                    Destroy(previewObject);

                previewObject = Instantiate(prefab);
                previewObject.name = prefab.name + "_Preview";

                // Désactiver interactions physiques
                foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;
                foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>())
                    rb.isKinematic = true;
            }

            previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            previewObject.SetActive(true);

            if (!reticle.activeSelf)
                reticle.SetActive(true);
        }
        else
        {
            if (previewObject)
                previewObject.SetActive(false);
            if (reticle.activeSelf)
                reticle.SetActive(false);
        }
    }

    void StartValidation()
    {
        if (previewObject == null) return;

        waitingForValidation = true;

        // Figer la preview à la position et rotation actuelles
        savedPosition = previewObject.transform.position;
        savedRotation = previewObject.transform.rotation;

        // Optionnel : rendre la preview semi-transparente
        SetPreviewTransparency(0.5f);

        if (validateButton != null)
            validateButton.SetActive(true);

        if (reticle.activeSelf)
            reticle.SetActive(false);
    }

    void OnValidateButtonClicked()
    {
        if (previewObject == null) return;

        // Instancier l'objet final à la position sauvegardée
        GameObject prefab = objectSpawner.objectPrefabs[objectSpawner.spawnOptionIndex];
        Instantiate(prefab, savedPosition, savedRotation);

        Destroy(previewObject);
        previewObject = null;
        waitingForValidation = false;

        if (validateButton != null)
            validateButton.SetActive(false);

        if (!reticle.activeSelf)
            reticle.SetActive(true);
    }

    void SetPreviewTransparency(float alpha)
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
                m.renderQueue = 3000;
            }
        }
    }
}
