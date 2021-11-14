using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DependencyInjection.Runtime
{
    public class Container
    {
        public bool AutoResolve { get; set; }
        private readonly Dictionary<Type, object> _map;

        public Container()
        {
            _map = new Dictionary<Type, object>();
        }


        public T Get<T>() where T : class
        {
            var instance = (T) GetInstance(typeof(T));
            return instance;
        }


        private object GetInstance(Type type)
        {
            if (!AutoResolveType(type))
            {
                return default;
            }

            if (!_map.TryGetValue(type, out var instance))
            {
                Debug.LogError($"Could not get type {type.Name}");
                return default;
            }

            var t = instance;
            if (t == null)
            {
                return default;
            }

            InjectFields(t);
            InjectProperties(t);
            return t;
        }

        private bool AutoResolveType(Type type)
        {
            if (!_map.ContainsKey(type))
            {
                if (!AutoResolve)
                {
                    Debug.LogError($"Type {type.Name} is not registered");
                    return false;
                }

                Register(type);
            }

            return true;
        }


        public void Register(Type type)
        {
            var instance = CreateInstance(type);
            RegisterInstance(type, instance);
        }

        public void Register<T>()
        {
            Register(typeof(T));
        }

        private object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public void Register<I, T>() where T : class
        {
            var type = typeof(I);
            if (!type.IsInterface)
            {
                Debug.LogError($"Generic type I is not an interface!");
                return;
            }

            var instance = CreateInstance(typeof(T));
            RegisterInstance(type, instance);
        }

        private void RegisterInstance<T>(Type type, T obj) where T : class
        {
            bool didAdd = _map.TryAdd(type, obj);
            if (!didAdd)
            {
                Debug.LogError($"Could not register type {type.Name}");
            }
        }

        private void InjectFields(object obj)
        {
            var type = obj.GetType();
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var fieldInfos = type.GetFields(bindingFlags).Concat(type.BaseType.GetFields(bindingFlags));

            foreach (var info in fieldInfos)
            {
                var attribs = info.GetCustomAttributes(typeof(InjectAttribute), true);
                if (attribs.Length <= 0)
                {
                    continue;
                }

                var targetType = info.FieldType;
                var instance = GetInstance(targetType);
                info.SetValue(obj, instance);
            }
        }

        private void InjectProperties(object obj)
        {
            var type = obj.GetType();
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var propertyInfos = type.GetProperties(bindingFlags).Concat(type.BaseType.GetProperties(bindingFlags));

            foreach (var info in propertyInfos)
            {
                var attribs = info.GetCustomAttributes(typeof(InjectAttribute), true);
                if (attribs.Length <= 0)
                {
                    continue;
                }

                var targetType = info.PropertyType;
                var instance = GetInstance(targetType);
                info.SetValue(obj, instance);
            }
        }
    }
}