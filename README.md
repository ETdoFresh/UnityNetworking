# Unity Networking
This is a low level library to create Client/Server connections in Unity using protocols
like TCP, UDP, or WebSockets. This is done by wrapping the CSharpNetworking library and
using JSLIB libraries for WebGL Websockets.

## Subtrees [Dependencies]
Included in this repository are the following subtree repositories:

- **CSharpNetworking** https://github.com/ETdoFresh/CSharpNetworking

*\* I use subtrees so that you don't have to worry about pulling and building from multiple repositories*

## Usage
Include the following two files in your Unity Project:

- **`UnityNetworking.dll`** Unity Networking MonoBehaviours
- **`CSharpNetworking.dll`** Network Communication Libraries using C#

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