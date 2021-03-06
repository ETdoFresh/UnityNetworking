﻿using CSharpNetworking;
using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;

namespace UnityNetworking
{
    public class TCPServerUnity : MonoBehaviour
    {
        public TCPServer server;
        public string host = "localhost";
        public int port = 11000;

        public Queue mainThread = Queue.Synchronized(new Queue());

        public UnityEventObject OnServerOpen = new UnityEventObject();
        public UnityEventObject OnServerClose = new UnityEventObject();
        public UnityEventObjectSocket OnOpen = new UnityEventObjectSocket();
        public UnityEventObjectMessageSocket OnMessage = new UnityEventObjectMessageSocket();
        public UnityEventObjectSocket OnClose = new UnityEventObjectSocket();
        public UnityEventObjectException OnError = new UnityEventObjectException();

        private void Awake()
        {
            server = new TCPServer(host, port);
        }

        private void OnEnable()
        {
            server.hostNameOrAddress = host;
            server.port = port;
            server.OnServerOpen += InvokeOnServerOpen;
            server.OnServerClose += InvokeOnServerClose;
            server.OnOpen += InvokeOnOpen;
            server.OnMessage += InvokeOnMessage;
            server.OnClose += InvokeOnClose;
            server.OnError += InvokeOnError;
            server.Open();
        }

        private void OnDisable()
        {
            server.Close();
            server.OnServerOpen -= InvokeOnServerOpen;
            server.OnServerClose -= InvokeOnServerClose;
            server.OnOpen -= InvokeOnOpen;
            server.OnMessage -= InvokeOnMessage;
            server.OnClose -= InvokeOnClose;
            server.OnError -= InvokeOnError;
        }

        private void OnDestroy()
        {
            server = null;
        }

        private void Update()
        {
            while (mainThread.Count > 0)
                ((Action)mainThread.Dequeue()).Invoke();
        }

        private void InvokeOnServerOpen(object sender, EventArgs e)
        {
            mainThread.Enqueue(new Action(() => OnServerOpen.Invoke(this)));
        }

        private void InvokeOnServerClose(object sender, EventArgs e)
        {
            mainThread.Enqueue(new Action(() => OnServerClose.Invoke(this)));
        }

        private void InvokeOnOpen(object sender, Socket e)
        {
            mainThread.Enqueue(new Action(() => OnOpen.Invoke(this, e)));
        }

        private void InvokeOnMessage(object sender, Message<Socket> e)
        {
            mainThread.Enqueue(new Action(() => OnMessage.Invoke(this, e)));
        }

        private void InvokeOnClose(object sender, Socket e)
        {
            mainThread.Enqueue(new Action(() => OnClose.Invoke(this, e)));
        }

        private void InvokeOnError(object sender, Exception e)
        {
            mainThread.Enqueue(new Action(() => OnError.Invoke(this, e)));
        }

        public void Send(Socket socket, string message) => server.Send(socket, message);

        public void Send(Socket socket, byte[] bytes) => server.Send(socket, bytes);

        public void Disconnect(Socket socket) => server.Disconnect(socket);
    }
}
