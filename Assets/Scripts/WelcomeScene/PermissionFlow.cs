using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class PermissionFlow : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] private Animator successPopup;
    [SerializeField] private Animator failurePopup;
    [SerializeField] private GameObject retryButton; 
    [SerializeField] private GameObject settingsButton; 

    [Header("Scene Configuration")]
    [SerializeField] private string nextSceneName;

    // State tracking
    private bool _pausedForPermission = false;
    private bool _hasRetried = false; // Tracks if user already clicked retry once

    private void Start()
    {
        Application.targetFrameRate = 60;

        successPopup.gameObject.SetActive(true);
        failurePopup.gameObject.SetActive(true);
        
        // Initial check on startup
        RequestCameraPermission();
    }

    // --- AUTOMATIC CHECK ON RETURN ---
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && _pausedForPermission)
        {
            _pausedForPermission = false;
            // When returning from settings, we act as if we are checking fresh
            RequestCameraPermission();
        }
    }

    // --- PUBLIC METHODS FOR BUTTONS ---

    public void OnRetryClicked()
    {
        // The user clicked retry, so next time it fails, we show settings
        _hasRetried = true; 
        RequestCameraPermission();
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnEnterSceneClicked()
    {
        if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene(nextSceneName);
    }

    public void OnOpenSettingsClicked()
    {
        _pausedForPermission = true; 
        OpenSystemSettings();
    }

    // --- CORE LOGIC ---

    private void RequestCameraPermission()
    {
        #if UNITY_EDITOR
            ShowSuccessUI();
        #elif UNITY_ANDROID
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                ShowSuccessUI();
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += (msg) => ShowFailureUI();
                callbacks.PermissionGranted += (msg) => ShowSuccessUI();
                Permission.RequestUserPermission(Permission.Camera, callbacks);
            }
        #elif UNITY_IOS
            StartCoroutine(RequestCameraIOS());
        #endif
    }

    #if UNITY_IOS
    IEnumerator RequestCameraIOS()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            ShowSuccessUI();
        }
        else
        {
            ShowFailureUI();
        }
    }
    #endif

    // --- UI STATE MANAGEMENT ---

    private void ShowSuccessUI()
    {
        failurePopup.ResetTrigger("Show");
        failurePopup.Play("PopUp_Idle");
        successPopup.SetTrigger("Show");
    }

    private void ShowFailureUI()
    {
        successPopup.ResetTrigger("Show");
        successPopup.Play("PopUp_Idle"); 
        failurePopup.SetTrigger("Show");
        
        // LOGIC: Decide which buttons to show
        if (!_hasRetried)
        {
            // CASE 1: First time failing. Show Retry.
            if(retryButton != null) retryButton.SetActive(true);
            if(settingsButton != null) settingsButton.SetActive(false);
        }
        else
        {
            // CASE 2: Already retried once. Show Settings.
            // (Because if they failed twice, the OS likely won't show the popup anymore)
            if(retryButton != null) retryButton.SetActive(false);
            if(settingsButton != null) settingsButton.SetActive(true);
        }
    }

    // --- SYSTEM SETTINGS ---

    private void OpenSystemSettings()
    {
        Debug.Log("Attempting to open System Settings...");
        #if UNITY_EDITOR
            Debug.Log("Open Settings only works on device.");
        #elif UNITY_ANDROID
            try
            {
                using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    string packageName = currentActivity.Call<string>("getPackageName");
                    using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS"))
                    {
                        string uriString = "package:" + packageName;
                        using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                        using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", uriString))
                        {
                            intentObject.Call<AndroidJavaObject>("setData", uriObject);
                        }
                        intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                        intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                        currentActivity.Call("startActivity", intentObject);
                    }
                }
            }
            catch (System.Exception ex) { Debug.LogError("Failed to open Settings: " + ex.Message); }
        #elif UNITY_IOS
            Application.OpenURL("app-settings:");
        #endif
    }
}