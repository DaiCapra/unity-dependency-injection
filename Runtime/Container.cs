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
        private readonly BindingFlags _bindingFlags;
        private readonly Dictionary<Type, object> _map;

        public Container()
        {
            AutoResolve = true;
            _bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            _map = new Dictionary<Type, object>();
        }


        public T Get<T>() where T : class
        {
            var obj = GetInstance(typeof(T));
            if (obj is T t)
            {
                return t;
            }

            return default;
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

        public void Register<I, T>() where T : class
        {
            var type = typeof(I);
            if (!type.IsInterface)
            {
                Debug.LogWarning($"Generic type I is not an interface!");
                return;
            }

            var instance = CreateInstance(typeof(T));
            RegisterInstance(type, instance);
        }

        public void Register<T>(T instance) where T : class
        {
            RegisterInstance(typeof(T), instance);
        }

        public bool Verify()
        {
            foreach (var kv in _map)
            {
                var type = kv.Key;
                var instance = kv.Value;
                if (instance == null)
                {
                    Debug.LogWarning($"Instance null for type: {type.Name}!");
                    return false;
                }

                var obj = GetInstance(type);
                if (obj == null)
                {
                    Debug.LogWarning($"Could not create instance for type: {type.Name}!");
                    return false;
                }

                var properties = GetProperties(type);
                foreach (var property in properties)
                {
                    var value = property.GetValue(obj);
                    if (value == null)
                    {
                        Debug.LogWarning($"Could not set property: {property.Name}, type: {type.Name}!");
                        return false;
                    }
                }

                var fields = GetFields(type);
                foreach (var field in fields)
                {
                    var value = field.GetValue(obj);
                    if (value == null)
                    {
                        Debug.LogWarning($"Could not set field: {field.Name}, type: {type.Name}!");
                        return false;
                    }
                }
            }

            return true;
        }

        private bool AutoResolveType(Type type)
        {
            if (!_map.ContainsKey(type))
            {
                if (!AutoResolve)
                {
                    Debug.LogWarning($"Type {type.Name} is not registered");
                    return false;
                }

                Register(type);
            }

            return true;
        }

        protected virtual object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<FieldInfo> GetFields(Type type)
        {
            return type.GetFields(_bindingFlags).Concat(type.BaseType.GetFields(_bindingFlags))
                .Concat(type.BaseType.GetFields(_bindingFlags))
                .Where(t => t.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);
        }


        private object GetInstance(Type type)
        {
            if (!AutoResolveType(type))
            {
                return default;
            }

            if (!_map.TryGetValue(type, out var instance))
            {
                Debug.LogWarning($"Could not get type {type.Name}");
                return default;
            }

            var t = instance;
            if (t == null)
            {
                return default;
            }

            InjectDependencies(t);
            return t;
        }

        public void InjectDependencies(object t)
        {
            InjectFields(t);
            InjectProperties(t);
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties(_bindingFlags)
                .Concat(type.BaseType.GetProperties(_bindingFlags))
                .Where(t => t.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);
        }

        private void InjectFields(object obj)
        {
            var type = obj.GetType();
            var fields = GetFields(type);

            foreach (var field in fields)
            {
                var targetType = field.FieldType;
                var instance = GetInstance(targetType);
                field.SetValue(obj, instance);
            }
        }

        private void InjectProperties(object obj)
        {
            var type = obj.GetType();

            var properties = GetProperties(type);

            foreach (var property in properties)
            {
                var targetType = property.PropertyType;
                var instance = GetInstance(targetType);
                property.SetValue(obj, instance);
            }
        }

        private void RegisterInstance<T>(Type type, T obj) where T : class
        {
            bool didAdd = _map.TryAdd(type, obj);
            if (!didAdd)
            {
                Debug.LogWarning($"Could not register type {type.Name}");
            }
        }
    }
}