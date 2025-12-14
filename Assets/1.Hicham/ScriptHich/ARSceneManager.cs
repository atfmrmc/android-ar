using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ARSceneManager : MonoBehaviour
{
    [Header("UI")]
    public Button deleteAllButton;

    [Header("Spawner")]
    public ObjectSpawner objectSpawner;

    void Start()
    {
        if (deleteAllButton != null)
            deleteAllButton.onClick.AddListener(DeleteAllSpawnedObjects);
    }

    void DeleteAllSpawnedObjects()
    {
        if (objectSpawner == null) return;

        foreach (Transform child in objectSpawner.transform)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("Tous les objets spawnés ont été supprimés !");
    }
}
