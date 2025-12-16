using UnityEngine;
using UnityEngine.UI;

public class ARSceneManager : MonoBehaviour
{
    [Header("UI")]
    public Button deleteAllButton;

    void Start()
    {
        if (deleteAllButton != null)
            deleteAllButton.onClick.AddListener(DeleteAllPlacedObjects);
    }

    public void DeleteAllPlacedObjects()
    {
        // Récupère tous les objets avec le tag "PlacedObject"
        GameObject[] placedObjects = GameObject.FindGameObjectsWithTag("PlacedObject");

        foreach (GameObject obj in placedObjects)
        {
            Destroy(obj);
        }

        Debug.Log("Tous les objets posés ont été supprimés !");
    }
}
