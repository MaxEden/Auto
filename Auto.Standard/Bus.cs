using System;
using System.Collections.Generic;
using System.Linq;

namespace Auto
{
    public interface IBus
    {
        void Subscribe<T>(Action<T>   receive);
        void Unsubscribe<T>(Action<T> receive);
        void Shout<T>(T               msg);
    }

    public class InternalBus : IBus
    {
        Dictionary<Type, List<Delegate>> _subscriptions = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> receive)
        {
            if(!_subscriptions.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>();
                _subscriptions.Add(typeof(T), list);
            }

            list.Add(receive);
        }

        public void Unsubscribe<T>(Action<T> receive)
        {
            if(!_subscriptions.TryGetValue(typeof(T), out var list)) return;
            list.Remove(receive);
        }

        public void Shout<T>(T msg)
        {
            if(!_subscriptions.TryGetValue(typeof(T), out var list)) return;
            list.ToList().ForEach(p => p.DynamicInvoke(msg));
        }
    }
}