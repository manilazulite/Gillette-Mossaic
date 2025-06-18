using UnityEngine;
using UnityEngine.UI;

public class BoxController : MonoBehaviour
{
    public InputField fldX;
    public InputField fldY;
    public InputField fldWidth;
    public InputField fldHeight;
    public GameObject myObjc;

    // Update is called once per frame
    void Update()
    {
        int x = getIntValue(fldX.text) - 1075;
        int y = getIntValue(fldY.text) - 516;
        int width = getIntValue(fldWidth.text);
        int height = getIntValue(fldHeight.text);
        myObjc.GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        if(width > 0 && height > 0)
        {
            myObjc.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        }
    }

    public int getIntValue(string integer)
    {
        int x;
        int.TryParse(integer, out x);
        return x;
    }
}
