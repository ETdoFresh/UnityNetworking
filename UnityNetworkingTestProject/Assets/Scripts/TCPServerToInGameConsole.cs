using CSharpNetworking;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityNetworking;

public class TCPServerToInGameConsole : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public TCPServerUnity server;

    void Awake()
    {
        server.OnServerOpen.AddListener(OnServerOpen);
        server.OnServerClose.AddListener(OnServerClose);
        server.OnOpen.AddListener(OnOpen);
        server.OnMessage.AddListener(OnMessage);
        server.OnClose.AddListener(OnClose);
    }

    private void OnDestroy()
    {
        server.OnServerOpen.RemoveListener(OnServerOpen);
        server.OnServerClose.RemoveListener(OnServerClose);
        server.OnOpen.RemoveListener(OnOpen);
        server.OnMessage.RemoveListener(OnMessage);
        server.OnClose.RemoveListener(OnClose);
    }

    private void OnServerOpen(Object arg0)
    {
        textMesh.text += "Server Open\n";
    }

    private void OnServerClose(Object arg0)
    {
        textMesh.text += "Server Closed\n";
    }

    private void OnOpen(Object arg0, Socket arg1)
    {
        var ip = ((IPEndPoint)arg1.RemoteEndPoint).Address;
        var port = ((IPEndPoint)arg1.RemoteEndPoint).Port;
        textMesh.text += $"Client {ip}:{port} connected!\n";
    }

    private void OnMessage(Object arg0, Message<Socket> arg1)
    {
        var ip = ((IPEndPoint)arg1.client.RemoteEndPoint).Address;
        var port = ((IPEndPoint)arg1.client.RemoteEndPoint).Port;
        textMesh.text += $"Received from Client {ip}:{port}: {arg1.data}\n";
    }

    private void OnClose(Object arg0, Socket arg1)
    {
        var ip = ((IPEndPoint)arg1?.RemoteEndPoint)?.Address;
        var port = arg1.RemoteEndPoint != null ? ((IPEndPoint)arg1.RemoteEndPoint).Port : -1;
        var clientInfo = ip != null ? $" {ip}:{port}" : "";
        textMesh.text += $"Client{clientInfo} disconnected!\n";
    }
}
