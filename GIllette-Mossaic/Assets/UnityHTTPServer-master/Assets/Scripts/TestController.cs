using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    public void setVideo(int videoIndex)
    {
        Debug.Log("Video Index = " + videoIndex);
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ScreenController.Instance.setScreen(videoIndex);
        });
    }
    public void SimpleMethod()
    {
        Debug.Log("Cool, fire via http connect");
    }

    public string[] SimpleStringMethod()
    {
        return new string[]{
            "result","result2"
        };
    }
    public int[] SimpleIntMethod()
    {
        return new int[]{
            1,2
        };
    }

    public ReturnResult CustomObjectReturnMethod()
    {
        ReturnResult result = new ReturnResult
        {
            code = 1,
            msg = "testing"
        };
        return result;
    }
    public ReturnResult CustomObjectReturnMethodWithQuery(int code, string msg)
    {
        ReturnResult result = new ReturnResult
        {
            code = code,
            msg = msg
        };
        return result;
    }

    //Mark as Serializable to make Unity's JsonUtility works.
    [System.Serializable]
    public class ReturnResult
    {
        public string msg;
        public int code;
    }

}
