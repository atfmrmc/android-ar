using UnityEngine;

public class objectInteraction : MonoBehaviour
{
    [SerializeField]
    Camera arCamera;

    [SerializeField]
    public GameObject targetGo;

    [SerializeField]
    public GameObject interactionButton;

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit = new RaycastHit();
        Vector2 mousePos = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = arCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out hit))
        {
            // Echo the Raycast transform
            Vector3 forward = transform.TransformDirection(Vector3.forward) * 50;

            // Tell me what object is hit
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Interact"))
                {
                    targetGo = hit.collider.gameObject.transform.parent?.gameObject;
                    //idTextToPrint = hit.collider.gameObject.GetComponent<Transform>().localPosition.x;
                    interactionButton.SetActive(true);
                }
                
            }

        }
    }
}
