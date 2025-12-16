using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ARResetButton : MonoBehaviour
{
    public ARSession arSession; // r�f�rence au ARSession de la sc�ne

    /// <summary>
    /// R�initialise la sc�ne AR compl�tement
    /// </summary>
    public void ResetARScene()
    {
        // Option 1 : reset ARSession pour repartir � z�ro
        if (arSession != null)
        {
            arSession.Reset();
        }

        // Option 2 : recharger compl�tement la sc�ne pour repartir propre
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
