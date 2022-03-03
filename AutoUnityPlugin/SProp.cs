using System;
using UnityEditor;

namespace AutoUnityPlugin
{
    public abstract class SProp
    {
        public readonly string Name;

        public SProp(string name)
        {
            Name = name;
        }
            
        public abstract Type Type { get; }

        private bool Is<TV>()
        {
            return Type == typeof(TV);
        }

        private object _value;
        public object ValueObj
        {
            get
            {
                if(_value != null) return _value;
                if(Is<bool>()) _value = EditorPrefs.GetBool(Name, false);
                if(Is<int>()) _value = EditorPrefs.GetInt(Name, 0);
                if(Is<string>()) _value = EditorPrefs.GetString(Name, null);
                if(Is<float>()) _value = EditorPrefs.GetFloat(Name, 0);
                if(_value != null) return _value;
                throw new InvalidCastException();
            }
            set
            {
                _value = value;
                if(Is<bool>()) EditorPrefs.SetBool(Name, (bool)value);
                if(Is<int>()) EditorPrefs.SetInt(Name, (int)value);
                if(Is<string>()) EditorPrefs.SetString(Name, value as string);
                if(Is<float>()) EditorPrefs.SetFloat(Name, (float)value);
            }
        }
    }

    public class SProp<T> : SProp
    {
        public T Value { get => (T)ValueObj; set => ValueObj = value; }

        public static implicit operator T(SProp<T> prop)
        {
            return prop.Value;
        }

        public SProp(string name) : base(name) {}
        public override Type Type => typeof(T);
    }
}