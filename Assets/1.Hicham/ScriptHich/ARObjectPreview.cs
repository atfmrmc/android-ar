using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARObjectPreview : MonoBehaviour
{
    [Header("AR References")]
    public ARRaycastManager raycastManager;       // ARRaycastManager
    public ObjectSpawner objectSpawner;           // Sert uniquement à récupérer les prefabs

    [Header("Reticle")]
    public GameObject reticle;                     // Sphere ou PNG qui montre où l'objet sera posé

    [Header("Preview Settings")]
    private GameObject previewObject;             // Objet de preview
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool surfaceFound = false;

    void Update()
    {
        UpdatePreview();

        // Détecte le touch ou clic (pour PC et mobile)
        if (surfaceFound)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                PlaceObjectAtPreview();
            }
#else
            if (Input.touchCount > 0 &&
                Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceObjectAtPreview();
            }
#endif
        }
    }

    /// <summary>
    /// Met à jour la position de la preview selon le réticule
    /// </summary>
    void UpdatePreview()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            surfaceFound = true;

            // Choix du prefab actuel
            GameObject prefab = objectSpawner.objectPrefabs[objectSpawner.spawnOptionIndex];

            // Crée le preview si nécessaire
            if (previewObject == null || previewObject.name != prefab.name + "_Preview")
            {
                if (previewObject != null)
                    Destroy(previewObject);

                previewObject = Instantiate(prefab);
                previewObject.name = prefab.name + "_Preview";

                // Désactive interactions et physiques pour le preview
                foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
                    c.enabled = false;

                foreach (Rigidbody rb in previewObject.GetComponentsInChildren<Rigidbody>())
                    rb.isKinematic = true;
            }

            // Déplace et oriente la preview
            previewObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

            // Affiche preview et réticule
            previewObject.SetActive(true);
            if (!reticle.activeSelf)
                reticle.SetActive(true);
        }
        else
        {
            surfaceFound = false;
            if (previewObject)
                previewObject.SetActive(false);
            if (reticle.activeSelf)
                reticle.SetActive(false);
        }
    }

    /// <summary>
    /// Instancie l'objet final à la position de la preview
    /// </summary>
    void PlaceObjectAtPreview()
    {
        if (previewObject == null)
            return;

        GameObject prefab = objectSpawner.objectPrefabs[objectSpawner.spawnOptionIndex];

        Instantiate(prefab,
                    previewObject.transform.position,
                    previewObject.transform.rotation);
    }
}
