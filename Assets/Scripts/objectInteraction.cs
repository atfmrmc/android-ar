using UnityEngine;

public class objectInteraction : MonoBehaviour
{
    [SerializeField]
    Camera arCamera;

    [SerializeField]
    public GameObject targetGo;

    [SerializeField]
    public GameObject interactionButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit = new RaycastHit();
        Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Ray ray = arCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out hit))
        {
            // Echo the Raycast transform
            Vector3 forward = transform.TransformDirection(Vector3.forward) * 50;
            Debug.DrawRay(transform.position, forward, Color.green);

            // Tell me what object is hit
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("interact"))
                {
                    print(hit.collider.gameObject.tag);
                    targetGo = hit.collider.gameObject.transform.parent?.gameObject;
                    //idTextToPrint = hit.collider.gameObject.GetComponent<Transform>().localPosition.x;
                    interactionButton.SetActive(true);
                }
                
            }

        }
    }
}
