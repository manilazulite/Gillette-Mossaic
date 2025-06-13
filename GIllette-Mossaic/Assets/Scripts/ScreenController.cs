using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class ScreenController : MonoBehaviour
{
    public static ScreenController Instance;
    public List<GameObject> screens = new List<GameObject>();
    private void Awake()
    {
        Instance = this;
    }

    public void callURL(int index)
    {
        Task task = setPageAsync(index);
    }

    public async Task setPageAsync(int index)
    {
        string ip = "127.0.0.1";
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://" + ip + ":3000/setVideo?videoIndex=" + index);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    public void setScreen(int index)
    {
        screens[index - 1].SetActive(true);
        for(int i = 0; i < screens.Count; i++)
        {
            if(i != index - 1)
            {
                screens[i].SetActive(false);
            }
        }
    }
}
