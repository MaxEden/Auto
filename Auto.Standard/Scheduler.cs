using System;
using System.Collections.Concurrent;

namespace Auto
{
    public interface IScheduler
    {
        void Subscribe<T>(T        obj, Action<T> update);
        void Unsubscribe(object    obj);
        void Update(Action<Action> executeSafe);
    }

    public class Scheduler : IScheduler
    {
        ConcurrentDictionary<object, Delegate> _queue = new ConcurrentDictionary<object, Delegate>();

        public void Subscribe<T>(T obj, Action<T> update)
        {
            _queue.AddOrUpdate(obj, update, (key, @delegate) => update);
        }

        public void Unsubscribe(object obj)
        {
            _queue.TryRemove(obj, out _);
        }

        public void Update(Action<Action> executeSafe)
        {
            foreach(var pair in _queue)
            {
                executeSafe(() => pair.Value.DynamicInvoke(pair.Key));
            }
        }
    }
}