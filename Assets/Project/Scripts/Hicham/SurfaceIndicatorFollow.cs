using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SurfaceIndicatorFollow : MonoBehaviour
{
    [Header("AR References")]
    public ARRaycastManager raycastManager;

    [Header("Indicator")]
    public GameObject indicatorPrefab;  // Flèche ou objet visuel
    public float maxDisplayTime = 5f;   // Durée max pendant laquelle la flèche suit le plan
    public float yOffset = 0.05f;       // Hauteur au-dessus du plan pour que la flèche soit visible

    private GameObject indicatorInstance;
    private bool indicatorActive = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private float timer = 0f;

    void Update()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Raycast sur le plan
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (!indicatorActive)
            {
                // Crée l'indicateur avec offset
                indicatorInstance = Instantiate(
                    indicatorPrefab,
                    hitPose.position + new Vector3(0, yOffset, 0),
                    hitPose.rotation
                );
                indicatorActive = true;
                timer = 0f;
            }
            else
            {
                // Met à jour la position pour suivre le plan avec offset
                indicatorInstance.transform.position = hitPose.position + new Vector3(0, yOffset, 0);
                indicatorInstance.transform.rotation = hitPose.rotation;
            }

            // Oriente la flèche vers le haut de la surface
            indicatorInstance.transform.up = hitPose.up;

            // Timer pour auto-destruction
            timer += Time.deltaTime;
            if (timer >= maxDisplayTime)
            {
                Destroy(indicatorInstance);
                indicatorActive = false;
            }
        }
    }

    // Appeler cette fonction si l'utilisateur pose un objet pour enlever la flèche immédiatement
    public void HideIndicator()
    {
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
            indicatorActive = false;
        }
    }
}
