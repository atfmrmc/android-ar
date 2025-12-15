using UnityEngine;

public class InformationUiButton : MonoBehaviour
{
    [SerializeField]
    objectInteraction cameraInformation;

    [SerializeField]
    GameObject UiPanelInformation;

    GameObject theCreation;

    public void buttonPressed()
    {
        if (theCreation)
        {
            Destroy(theCreation);
            theCreation = null;
        }
        theCreation = Instantiate(UiPanelInformation, cameraInformation.targetGo.transform);
        explainationData theDataGame = cameraInformation.targetGo.GetComponent<explainationData>();
        theCreation.GetComponent<UITextControl>().display(theDataGame.title, theDataGame.description);
    }
}
