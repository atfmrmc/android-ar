using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PlaceObjectAtReticle : MonoBehaviour
{
    [Header("References")]
    public ARReticleController reticleController;
    public ObjectSpawner objectSpawner;

    [Header("UI")]
    public GameObject validateButton;

    private bool canPlace = false;

    void Start()
    {
        validateButton.SetActive(false);
    }

    void Update()
    {
        // Quand l'utilisateur touche = on affiche le bouton
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began &&
            reticleController.reticle.activeSelf)
        {
            validateButton.SetActive(true);
            canPlace = true;
        }
    }

    public void ValidatePlacement()
    {
        if (!canPlace)
            return;

        GameObject prefab =
            objectSpawner.objectPrefabs[objectSpawner.spawnOptionIndex];

        Instantiate(
            prefab,
            reticleController.reticle.transform.position,
            reticleController.reticle.transform.rotation
        );

        validateButton.SetActive(false);
        canPlace = false;
    }
}
