using CSharpNetworking;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityNetworking;

public class TCPServerToDebugLog : MonoBehaviour
{
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
        Debug.Log("Server Open");
    }

    private void OnServerClose(Object arg0)
    {
        Debug.Log("Server Closed");
    }

    private void OnOpen(Object arg0, Socket arg1)
    {
        var ip = ((IPEndPoint)arg1.RemoteEndPoint).Address;
        var port = ((IPEndPoint)arg1.RemoteEndPoint).Port;
        Debug.Log($"Client {ip}:{port} connected!");
    }

    private void OnMessage(Object arg0, Message<Socket> arg1)
    {
        var ip = ((IPEndPoint)arg1.client.RemoteEndPoint).Address;
        var port = ((IPEndPoint)arg1.client.RemoteEndPoint).Port;
        Debug.Log($"Received from Client {ip}:{port}: {arg1.data}");
    }

    private void OnClose(Object arg0, Socket arg1)
    {
        var ip = ((IPEndPoint)arg1?.RemoteEndPoint)?.Address;
        var port = arg1.RemoteEndPoint != null ? ((IPEndPoint)arg1.RemoteEndPoint).Port : -1;
        var clientInfo = ip != null ? $" {ip}:{port}" : "";
        Debug.Log($"Client{clientInfo} disconnected!");
    }
}
