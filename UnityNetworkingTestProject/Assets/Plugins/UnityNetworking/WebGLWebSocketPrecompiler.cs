using UnityEngine;
using UnityNetworking;

public static class WebGLWebSocketPrecompiler
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void DetectPlatformForWebSocketClientUnity()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebSocketClientUnity.webgl = true;
#else
        WebSocketClientUnity.webgl = false;
#endif
    }
}
