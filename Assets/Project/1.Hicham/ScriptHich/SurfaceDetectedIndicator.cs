using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SurfaceDetectedIndicator : MonoBehaviour
{
    [Header("AR References")]
    public ARRaycastManager raycastManager;

    [Header("Indicator")]
    public GameObject indicatorPrefab;  // Une flèche ou un objet pour indiquer la surface
    public float displayTime = 2f;      // Durée d'affichage en secondes

    private GameObject indicatorInstance;
    private bool surfaceDetected = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // On regarde le centre de l'écran pour détecter une surface
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (!surfaceDetected && raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            surfaceDetected = true;

            // Crée l'indicateur à la position du plan
            indicatorInstance = Instantiate(indicatorPrefab, hitPose.position, hitPose.rotation);

            // Oriente la flèche vers le haut de la surface
            indicatorInstance.transform.up = hitPose.up;

            // Lancer la coroutine pour le faire disparaître après un temps
            StartCoroutine(HideIndicatorAfterTime(displayTime));
        }
    }

    private IEnumerator HideIndicatorAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        if (indicatorInstance != null)
            Destroy(indicatorInstance);
    }
}
