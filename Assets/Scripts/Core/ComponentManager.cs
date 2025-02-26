// ComponentManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace ECS
{

    // 基础组件实现
    public abstract class Component
    {
        // 组件所属实体ID（自动注入）
        public int EntityId { get; internal set; } = -1;

        // 组件激活状态
        public bool Enabled { get; set; } = true;

        // 虚方法供子类重写
        public virtual void Reset() { }

    }

    public class ComponentManager : Singleton<ComponentManager>
    {
        private class ComponentStorage<T> where T : Component
        {
            public readonly IDictionary<int, T> components = new Dictionary<int, T>();
        }

        private readonly Dictionary<Type, object> componentStorages = new Dictionary<Type, object>();

        public void AddComponent<T>(int entityId, T component) where T : Component
        {
            component.EntityId = entityId; // 自动注入实体ID
            GetStorage<T>().components[entityId] = component;
        }

        public T GetComponent<T>(int entityId) where T : Component
        {
            return GetStorage<T>().components.TryGetValue(entityId, out T component) ? component : null;
        }

        public bool HasComponent<T>(int entityId) where T : Component
        {
            return GetStorage<T>().components.ContainsKey(entityId);
        }

        public bool HasComponentWithType(int entityId, Type type)
        {
            // 获取 ComponentManager 实例
            ComponentManager componentManager = ComponentManager.Instance;

            // 使用反射动态调用 HasComponent<T> 方法
            MethodInfo hasComponentMethod = typeof(ComponentManager).GetMethod("HasComponent");
            MethodInfo genericMethod = hasComponentMethod.MakeGenericMethod(type);
            bool result = (bool)genericMethod.Invoke(componentManager, new object[] { entityId });
            return result;
        }

        public void RemoveComponent<T>(int entityId) where T : Component
        {
            GetStorage<T>().components.Remove(entityId);
        }

        public IEnumerable<int> GetEntitiesWithComponent<T>() where T : Component
        {
            return GetStorage<T>().components.Keys;
        }

        private ComponentStorage<T> GetStorage<T>() where T : Component
        {
            Type type = typeof(T);
            if (!componentStorages.TryGetValue(type, out object storage))
            {
                storage = new ComponentStorage<T>();
                componentStorages[type] = storage;
            }
            return (ComponentStorage<T>)storage;
        }
    }
}