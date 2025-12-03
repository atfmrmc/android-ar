using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif


public class askConsent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CheckCameraPermission()
    {
    #if UNITY_IOS
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) {
            // PERMISSION NOT AVAILABLE ON IOS, DISPLAY POPUP MESSAGE
        }
    #endif
    #if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) {
            // PERMISSION NOT AVAILABLE ON ANDROID, DISPLAY POPUP MESSAGE
        }
    #endif
    }
}

