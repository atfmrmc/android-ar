using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ARResetButton : MonoBehaviour
{
    public ARSession arSession; // référence au ARSession de la scène

    /// <summary>
    /// Réinitialise la scène AR complètement
    /// </summary>
    public void ResetARScene()
    {
        // Option 1 : reset ARSession pour repartir à zéro
        if (arSession != null)
        {
            arSession.Reset();
            Debug.Log("Incroyable ça, la scene est reset !");
        }

        // Option 2 : recharger complètement la scène pour repartir propre
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
