using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARTrackingStatus : MonoBehaviour
{
    public Text statusText;

    void Update()
    {
        if (statusText == null) return;

        switch (ARSession.state)
        {
            case ARSessionState.SessionTracking:
                statusText.text = "Tracking OK";
                break;

            case ARSessionState.SessionInitializing:
                statusText.text = "Initialisation...";
                break;

            default:
                statusText.text = "Tracking perdu, ajustez l’appareil";
                break;
        }
    }
}
