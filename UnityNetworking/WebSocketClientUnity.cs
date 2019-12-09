using CSharpNetworking;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace UnityNetworking
{
    public class WebSocketClientUnity : MonoBehaviour
    {
        const int CONNECTING = 0;
        const int OPEN = 1;
        const int CLOSING = 2;
        const int CLOSED = 3;

        public static bool webgl;

        public WebSocketClient client;
        public string uri = "wss://echo.websocket.org";
        public Queue mainThread = Queue.Synchronized(new Queue());

        private int websocketId = -1;
        private int readyState = CLOSED;

        public UnityEventObject OnOpen = new UnityEventObject();
        public UnityEventObjectMessage OnMessage = new UnityEventObjectMessage();
        public UnityEventObject OnClose = new UnityEventObject();
        public UnityEventObjectException OnError = new UnityEventObjectException();

        private void Awake()
        {
            if (!webgl)
                client = new WebSocketClient(uri);
        }

        private void OnEnable()
        {
            if (!webgl)
            {
                client.OnOpen += InvokeOnOpen;
                client.OnMessage += InvokeOnMessage;
                client.OnClose += InvokeOnClose;
                client.OnError += InvokeOnError;
                client.Open();
            }
            else
            {
                websocketId = SocketCreate(uri);
                readyState = CONNECTING;
            }
        }

        private void OnDisable()
        {
            if (!webgl)
            {
                client.Close();
                client.OnOpen -= InvokeOnOpen;
                client.OnMessage -= InvokeOnMessage;
                client.OnClose -= InvokeOnClose;
                client.OnError -= InvokeOnError;
            }
            else
            {
                SocketClose(websocketId);
                websocketId = -1;
                readyState = CLOSED;
            }
        }

        private void OnDestroy()
        {
            if (!webgl)
                client = null;
        }

        private void Update()
        {
            if (!webgl)
                while (mainThread.Count > 0)
                    ((Action)mainThread.Dequeue()).Invoke();
            
            else
            {
                if (websocketId == -1) return;

                if (SocketState(websocketId) == CLOSING && readyState == OPEN)
                {
                    readyState = CLOSING;
                    OnClose.Invoke(this);
                    readyState = CLOSED;
                    return;
                }

                if (readyState == CLOSED || readyState == CONNECTING)
                    if (SocketState(websocketId) == OPEN)
                    {
                        OnOpen.Invoke(this);
                        readyState = OPEN;
                    }

                if (SocketState(websocketId) == OPEN)
                {
                    var length = SocketRecvLength(websocketId);
                    if (length > 0)
                    {
                        var bytes = new byte[length];
                        SocketRecv(websocketId, bytes, bytes.Length);
                        OnMessage.Invoke(this, new Message(bytes));
                    }
                }
            }
        }

        private void InvokeOnOpen(object sender, EventArgs e)
        {
            mainThread.Enqueue(new Action(() => OnOpen.Invoke(this)));
        }

        private void InvokeOnMessage(object sender, Message e)
        {
            mainThread.Enqueue(new Action(() => OnMessage.Invoke(this, e)));
        }

        private void InvokeOnClose(object sender, EventArgs e)
        {
            mainThread.Enqueue(new Action(() => OnClose.Invoke(this)));
        }

        private void InvokeOnError(object sender, Exception e)
        {
            mainThread.Enqueue(new Action(() => OnError.Invoke(this, e)));
        }

        public void Send(string message)
        {
            if (!webgl) client.Send(message);
            else Send(Encoding.UTF8.GetBytes(message));
        }

        public void Send(byte[] bytes)
        {
            if (!webgl) client.Send(bytes);
            else SocketSend(websocketId, bytes, bytes.Length);
        }

        [DllImport("__Internal")] private static extern int SocketCreate(string url);
        [DllImport("__Internal")] private static extern string SocketUrl(int websocket);
        [DllImport("__Internal")] private static extern int SocketState(int websocket);
        [DllImport("__Internal")] private static extern void SocketError(int websocket, byte[] ptr, int length);
        [DllImport("__Internal")] private static extern void SocketSend(int websocket, byte[] ptr, int length);
        [DllImport("__Internal")] private static extern int SocketRecvLength(int websocket);
        [DllImport("__Internal")] private static extern void SocketRecv(int websocket, byte[] ptr, int length);
        [DllImport("__Internal")] private static extern void SocketClose(int websocket);
    }
}
