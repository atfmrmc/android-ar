using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARObjectPreviewWithValidation : MonoBehaviour
{
    [Header("AR")]
    public ARRaycastManager raycastManager;
    public ObjectSpawner objectSpawner;

    [Header("UI")]
    public GameObject reticle;
    public GameObject validateButton;
    public GameObject cancelButton;

    private GameObject previewObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool waitingForValidation = false;

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    void Start()
    {
        validateButton.SetActive(false);
        cancelButton.SetActive(false);

        validateButton.GetComponent<Button>()
            .onClick.AddListener(OnValidate);

        cancelButton.GetComponent<Button>()
            .onClick.AddListener(OnCancel);
    }

    void Update()
    {
        if (!waitingForValidation)
        {
            UpdatePreview();
            DetectTouchToFreeze();
        }
    }

    // ---------------- PREVIEW ----------------
    void UpdatePreview()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (!raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            if (previewObject) previewObject.SetActive(false);
            reticle.SetActive(false);
            return;
        }

        Pose hitPose = hits[0].pose;
        reticle.SetActive(true);

        if (previewObject == null)
            CreatePreview();

        previewObject.SetActive(true);
        previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
    }

    void CreatePreview()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return; // NE RIEN FAIRE en Edit Mode
#endif

        GameObject prefab = objectSpawner.objectPrefabs[objectSpawner.spawnOptionIndex];
        if (prefab == null) return; // sécurité

        previewObject = Instantiate(prefab);
        previewObject.name = prefab.name + "_Preview";

        foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
            c.enabled = false;

        foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;
    }


    // ---------------- FREEZE ----------------
    void DetectTouchToFreeze()
    {
#if UNITY_EDITOR
        if (!Input.GetMouseButtonDown(0)) return;
#else
        if (Input.touchCount == 0 || Input.GetTouch(0).phase != TouchPhase.Began) return;
#endif

        if (previewObject == null || !previewObject.activeSelf) return;

        waitingForValidation = true;

        savedPosition = previewObject.transform.position;
        savedRotation = previewObject.transform.rotation;

        previewObject.transform.SetPositionAndRotation(savedPosition, savedRotation);

        validateButton.SetActive(true);
        cancelButton.SetActive(true);
        reticle.SetActive(false);
    }

    // ---------------- VALIDATE ----------------
    void OnValidate()
    {
        GameObject prefab = objectSpawner.objectPrefabs[objectSpawner.spawnOptionIndex];
        Instantiate(prefab, savedPosition, savedRotation);

        ResetState();
    }

    // ---------------- CANCEL ----------------
    void OnCancel()
    {
        ResetState();
    }

    // ---------------- RESET ----------------
    void ResetState()
    {
        if (previewObject)
            Destroy(previewObject);

        previewObject = null;
        waitingForValidation = false;

        validateButton.SetActive(false);
        cancelButton.SetActive(false);
        reticle.SetActive(true);
    }
}
