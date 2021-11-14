using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
            var instance = GetInstance<T>();
            return instance;
        }


        private T GetInstance<T>() where T : class
        {
            var type = typeof(T);
            if (!_map.ContainsKey(type))
            {
                if (!AutoResolve)
                {
                    Debug.LogError($"Type {type.Name} is not registered");
                    return default;
                }
                else
                {
                    Register<T>();
                }
            }

            if (!_map.TryGetValue(type, out var instance))
            {
                Debug.LogError($"Could not get type {type.Name}");
                return default;
            }

            var t = (T) instance;
            if (t == null)
            {
                return default;
            }

            InjectFields(t);
            InjectProperties(t);
            return t;
        }

        private object GetInstance(Type type)
        {
            if (!_map.TryGetValue(type, out var instance))
            {
                Debug.LogError($"Type {type.Name} is not registered");
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

        public void Register<T>() where T : class
        {
            var instance = CreateInstance<T>();
            var type = typeof(T);
            RegisterInstance(type, instance);
        }

        private static T CreateInstance<T>() where T : class
        {
            return (T) Activator.CreateInstance(typeof(T));
        }

        public void Register<I, T>() where T : class
        {
            var type = typeof(I);
            if (!type.IsInterface)
            {
                Debug.LogError($"Generic type I is not an interface!");
                return;
            }

            var instance = CreateInstance<T>();
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
                if (!_map.TryGetValue(targetType, out var instance))
                {
                    Debug.LogError($"Could not set value: {targetType.Name} in type: {type.Name}");
                    continue;
                }

                info.SetValue(obj, instance);
            }
        }
    }
}