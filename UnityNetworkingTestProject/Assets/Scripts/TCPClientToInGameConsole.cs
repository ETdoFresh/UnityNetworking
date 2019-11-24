using CSharpNetworking;
using TMPro;
using UnityEngine;
using UnityNetworking;

public class TCPClientToInGameConsole : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public TCPServerUnity server;
    public TCPClientUnity client;

    void Awake()
    {
        server.OnServerOpen.AddListener(OnServerOpen);
        server.OnServerOpen.AddListener(OnServerClose);
    }

    private void OnDestroy()
    {
        server.OnServerOpen.RemoveListener(OnServerOpen);
        server.OnServerOpen.RemoveListener(OnServerClose);
    }

    private void OnServerOpen(Object arg0)
    {
        client.OnOpen.AddListener(OnOpen);
        client.OnMessage.AddListener(OnMessage);
        client.OnClose.AddListener(OnClose);
        client.enabled = true;
    }

    private void OnServerClose(Object arg0)
    {
        client.enabled = false;
        client.OnOpen.RemoveListener(OnOpen);
        client.OnMessage.RemoveListener(OnMessage);
        client.OnClose.RemoveListener(OnClose);
    }

    private void OnOpen(Object arg0)
    {
        textMesh.text += $"Client connected!\n";
    }

    private void OnMessage(Object arg0, Message arg1)
    {
        textMesh.text += $"Client Received: {arg1.data}\n";
    }

    private void OnClose(Object arg0)
    {
        textMesh.text += $"Client disconnected!\n";
    }
}