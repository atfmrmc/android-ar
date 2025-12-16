using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARReticleController : MonoBehaviour
{
    [Header("References")]
    public ARRaycastManager raycastManager; // ton ARRaycastManager
    public GameObject reticle;              // la Sphere ou PNG

    [Header("Reticle Settings")]
    public float reticleScreenSize = 0.05f; // taille apparente à l'écran
    public bool pulseEffect = true;         // activer/désactiver pulsation
    public float pulseSpeed = 2f;           // vitesse de pulsation
    public float pulseAmount = 0.05f;       // amplitude de pulsation

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // Centre de l'écran
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Raycast strict : PlanWithinPolygon = uniquement zones placables
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // Activer le reticle
            if (!reticle.activeSelf)
                reticle.SetActive(true);

            // Position et orientation
            reticle.transform.position = hitPose.position;
            reticle.transform.LookAt(Camera.main.transform);
            reticle.transform.Rotate(0, 180, 0); // corrige l’inversion

            // Scale constant à l’écran
            float distance = Vector3.Distance(Camera.main.transform.position, hitPose.position);
            float baseScale = distance * reticleScreenSize;

            // Ajouter pulsation si activé
            if (pulseEffect)
            {
                float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                reticle.transform.localScale = Vector3.one * baseScale * pulse;
            }
            else
            {
                reticle.transform.localScale = Vector3.one * baseScale;
            }
        }
        else
        {
            // Aucun plan détecté → cache le reticle
            if (reticle.activeSelf)
                reticle.SetActive(false);
        }
    }
}
