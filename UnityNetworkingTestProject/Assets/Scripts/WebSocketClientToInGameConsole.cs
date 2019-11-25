using CSharpNetworking;
using TMPro;
using UnityEngine;
using UnityNetworking;

public class WebSocketClientToInGameConsole : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public WebSocketClientUnity client;

    void Awake()
    {
        client.OnOpen.AddListener(OnOpen);
        client.OnMessage.AddListener(OnMessage);
        client.OnClose.AddListener(OnClose);
    }

    private void OnDestroy()
    {
        client.OnOpen.RemoveListener(OnOpen);
        client.OnMessage.RemoveListener(OnMessage);
        client.OnClose.RemoveListener(OnClose);
    }

    private void OnOpen(Object arg0)
    {
        textMesh.text += $"Client: Connected to Server!\n";
    }

    private void OnMessage(Object arg0, Message arg1)
    {
        textMesh.text += $"Client: Received from Server: {arg1.data}\n";
    }

    private void OnClose(Object arg0)
    {
        textMesh.text += $"Client: Disconnected!\n";
    }

    public void OnSend(string message)
    {
        textMesh.text += $"<#808080>Client: Sent to Server: {message}</color>\n";
    }
}