using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DependencyInjection.Runtime
{
    public class Container
    {
        private readonly BindingFlags _bindingFlags;
        private readonly Dictionary<Type, object> _mapSingletons;
        private readonly Dictionary<Type, Type> _mapInterfaces;

        public Container()
        {
            AutoResolve = true;
            _bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            _mapSingletons = new Dictionary<Type, object>();
            _mapInterfaces = new Dictionary<Type, Type>();
        }

        public bool AutoResolve { get; set; }


        public T Get<T>() where T : class
        {
            var obj = GetInstance(typeof(T));
            if (obj is T t)
            {
                return t;
            }

            return default;
        }

        public object Make(Type type)
        {
            return GetInstance(type);
        }

        public void RegisterSingleton(Type type)
        {
            var instance = CreateInstance(type);
            _mapSingletons[type] = instance;
        }


        public void RegisterSingleton<T>()
        {
            RegisterSingleton(typeof(T));
        }

        public void Register<I, T>() where T : class
        {
            var typeInterface = typeof(I);
            if (!typeInterface.IsInterface)
            {
                Debug.LogWarning($"Generic type I is not an interface!");
                return;
            }

            var type = typeof(T);
            _mapInterfaces[typeInterface] = type;
        }

        public void RegisterSingleton<I, T>() where T : class
        {
            var typeInterface = typeof(I);
            if (!typeInterface.IsInterface)
            {
                Debug.LogWarning($"Generic type I is not an interface!");
                return;
            }

            var type = typeof(T);
            _mapInterfaces[typeInterface] = type;
            RegisterSingleton(type);
        }

        public void RegisterObject<T>(T instance) where T : class
        {
            var type = typeof(T);
            _mapSingletons[type] = instance;
        }

        public void RegisterObjectToInterface<I, T>(I instance) where T : class
        {
            Register<I, T>();
            var type = typeof(T);
            _mapSingletons[type] = instance;
        }

        public object[] GetRegisteredInstances()
        {
            return _mapSingletons.Values.ToArray();
        }

        public bool Verify()
        {
            foreach (var kv in _mapSingletons)
            {
                var type = kv.Key;
                var instance = kv.Value;
                if (type == null)
                {
                    continue;
                }

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

        protected virtual object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        private bool AutoResolveType(Type type)
        {
            if (!_mapSingletons.ContainsKey(type))
            {
                if (!AutoResolve)
                {
                    Debug.LogWarning($"Type {type.Name} is not registered");
                    return false;
                }

                RegisterSingleton(type);
            }

            return true;
        }

        private IEnumerable<FieldInfo> GetFields(Type type)
        {
            if (type?.BaseType == null)
            {
                return new List<FieldInfo>();
            }

            return type.GetFields(_bindingFlags).Concat(type.BaseType.GetFields(_bindingFlags))
                .Concat(type.BaseType.GetFields(_bindingFlags))
                .Where(t => t.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);
        }

        private object GetInstance(Type type)
        {
            if (type.IsInterface)
            {
                if (_mapInterfaces.ContainsKey(type))
                {
                    type = _mapInterfaces[type];
                }
                else
                {
                    return default;
                }
            }

            if (!_mapSingletons.TryGetValue(type, out var instance))
            {
                if (AutoResolve)
                {
                    instance = CreateInstance(type);
                }
            }


            var t = instance;
            if (t == null)
            {
                return default;
            }

            InjectDependencies(t);

            if (t is IInitializable initializable)
            {
                initializable.Init();
            }

            return t;
        }

        public void InjectDependencies(object obj)
        {
            InjectFields(obj);
            InjectProperties(obj);
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            if (type?.BaseType == null)
            {
                return new List<PropertyInfo>();
            }

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
    }
}