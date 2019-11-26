# Unity Networking
This is a low level library to create Client/Server connections in Unity using protocols
like TCP, UDP, or WebSockets. This is done by wrapping the CSharpNetworking library and
using JSLIB libraries for WebGL Websockets.

## Try it yourself!
Here is a WebSocketClient example running in WebGL:

https://etdofresh.github.io/UnityNetworking/

## Example Code
Create a new scene with a TextMeshPro Textbox. Also add the WebScoketClientUnity MonoBehaviour (provided in dll) and the following script to create the above example.
```
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
```

## Usage
Include the following four files in your Unity Project found as a *.unitypackage* in [Releases](https://github.com/ETdoFresh/UnityNetworking/releases):

- **`UnityNetworking.dll`** Unity Networking MonoBehaviours
- **`CSharpNetworking.dll`** Network Communication Libraries using C#
- **`WebGLWebSocket.jslib`** WebGL Websocket Library
- **`WebGLWebSocketPrecompiler.cs`** Workaround for UnityNetworking.dll not being able to access precompiler tags (since it's already compiled)

This will allow you to use MonoBehaviours

- TcpServerUnity
- TcpClientUnity
- WebSocketClientUnity

Then it is up to you to implement Event Listeners and use Send methods
to create communication between servers and clients.

## Server\<client> Objects
### Events
- OnServerOpen(sender)
- OnServerClose(sender)
- OnOpen(sender, client)
- OnMessage(sender, clientMessage)
- OnClose(sender, client)
- OnError(exception)
### Methods
- Send(client, string)
- Send(client, byte[])

## Client Objects
### Events
- OnOpen()
- OnMessage(message)
- OnClose()
- OnError(exceoption)
### Methods
- Send(string)
- Send(byte[])

## Message Objects
- string data
- byte[] bytes

## Subtrees [Dependencies]
Included in this repository are the following subtree repositories:

- **CSharpNetworking** https://github.com/ETdoFresh/CSharpNetworking
