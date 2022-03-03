using System;
using System.Collections.Generic;

namespace Auto
{
    public abstract class Setup
    {
        public virtual void DefaultCommand(){}

        Dictionary<Type, object> _lazyDict = new Dictionary<Type, object>();
        public T Lazy<T>(Func<T> ctor)
        {
            if(!_lazyDict.TryGetValue(typeof(T), out var obj))
            {
                obj = ctor();
                _lazyDict.Add(typeof(T), obj);
            }

            return (T)obj;
        }
    }
}