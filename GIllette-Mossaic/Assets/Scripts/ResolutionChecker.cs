using UnityEngine;
using UnityEngine.UI;

public class ResolutionChecker : MonoBehaviour
{
    public Text myText;

    private void Start()
    {
        myText.text = "Width = " + myText.GetComponent<RectTransform>().rect.width + " Height = " + myText.GetComponent<RectTransform>().rect.height;
    }
}
