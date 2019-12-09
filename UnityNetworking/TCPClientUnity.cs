using CSharpNetworking;
using System;
using System.Collections;
using UnityEngine;

namespace UnityNetworking
{
    public class TCPClientUnity : MonoBehaviour
    {
        public TCPClient client;
        public string host = "localhost";
        public int port = 11000;

        public Queue mainThread = Queue.Synchronized(new Queue());

        public UnityEventObject OnOpen = new UnityEventObject();
        public UnityEventObjectMessage OnMessage = new UnityEventObjectMessage();
        public UnityEventObject OnClose = new UnityEventObject();
        public UnityEventObjectException OnError = new UnityEventObjectException();

        private void Awake()
        {
            client = new TCPClient(host, port);
        }

        private void OnEnable()
        {
            client.OnOpen += InvokeOnOpen;
            client.OnMessage += InvokeOnMessage;
            client.OnClose += InvokeOnClose;
            client.OnError += InvokeOnError;
            client.Open();
        }

        private void OnDisable()
        {
            client.Close();
            client.OnOpen -= InvokeOnOpen;
            client.OnMessage -= InvokeOnMessage;
            client.OnClose -= InvokeOnClose;
            client.OnError -= InvokeOnError;
        }

        private void OnDestroy()
        {
            client = null;
        }

        private void Update()
        {
            while (mainThread.Count > 0)
                ((Action)mainThread.Dequeue()).Invoke();
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

        public void Send(string message) => client.Send(message);

        public void Send(byte[] bytes) => client.Send(bytes);
    }
}
