namespace UnityEngine.Events
{
    public class UnityEvent 
    {
        public void Invoke() { }
    }

    public class UnityEvent<T0> 
    {
        public void Invoke(T0 arg0) { }
    }

    public class UnityEvent<T0, T1> 
    {
        public void Invoke(T0 arg0, T1 arg1) { }
    }

    public class UnityEvent<T0, T1, T2> 
    {
        public void Invoke(T0 arg0, T1 arg1, T2 arg2) { }
    }

    public class UnityEvent<T0, T1, T2, T3> 
    {
        public void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3) { }
    }
}
