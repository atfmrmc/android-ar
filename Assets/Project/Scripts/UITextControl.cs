using UnityEngine;
using TMPro;
using System.Collections;

public class UITextControl : MonoBehaviour
{
    [SerializeField]
    TextMeshPro title;

    [SerializeField]
    TextMeshPro description;

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }
        Renderer r = GetComponentInParent<Renderer>();
        if (r != null)
        {
            transform.localPosition = new Vector3(0, r.bounds.size.y + 0.2f, 0);
        }
    }

    private void Update()
    {
        transform.localPosition = Vector3.up * .7f;

    }

    public void display(string titleText, string descText)
    {
        title.text = titleText;
        description.text = descText;
    }
}
