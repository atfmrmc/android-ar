using UnityEngine;

public class InformationUiButton : MonoBehaviour
{
    [SerializeField]
    objectInteraction cameraInformation;

    [SerializeField]
    GameObject UiPanelInformation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void buttonPressed()
    {
        GameObject theCreation = Instantiate(UiPanelInformation, cameraInformation.targetGo.transform);
        explainationData theDataGame = cameraInformation.targetGo.GetComponent<explainationData>();
        theCreation.GetComponent<UITextControl>().display(theDataGame.title, theDataGame.description);
    }
}
