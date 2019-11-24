using CSharpNetworking;
using System;
using System.Net.Sockets;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace UnityNetworking
{
    [Serializable] public class UnityEventObject : UnityEvent<Object> { }
    [Serializable] public class UnityEventObjectSocket : UnityEvent<Object, Socket> { }
    [Serializable] public class UnityEventObjectMessageSocket : UnityEvent<Object, Message<Socket>> { }
}
