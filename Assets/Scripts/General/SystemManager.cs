// SystemManager.cs
using System.Collections.Generic;
using System;
using UnityEngine;

namespace ECS
{
    public enum SystemPriority
    {
        Low = 0,
        Normal = 100,
        High = 200
    }

    public enum SystemType
    {
        Audio,
        Rendering,
        Physics,
        Script
    }

    public abstract class System
    {
        public SystemType Type { get; }
        public SystemPriority Priority { get; protected set; } = SystemPriority.Normal;

        protected readonly EntityManager EntityManager;
        protected readonly ComponentManager ComponentManager;

        protected System(SystemType type, EntityManager em, ComponentManager cm)
        {
            Type = type;
            EntityManager = em;
            ComponentManager = cm;
        }

        // 主更新入口
        public virtual void Update() { }

        // 获取符合组件要求的实体
        protected IEnumerable<int> GetEntities(params Type[] componentTypes)
        {
            var entities = new List<int>(EntityManager.GetActiveEntities());

            foreach (var entity in entities)
            {
                bool isValid = true;
                foreach (var type in componentTypes)
                {
                    if (!ComponentManager.HasComponentWithType(entity, type))
                    {
                        isValid = false;
                        break;
                    }
                }
                if (isValid) yield return entity;
            }
        }

        // 类型安全的组件查询
        protected bool HasComponent<T>(int entity) where T : Component
            => ComponentManager.HasComponent<T>(entity);

        protected T GetComponent<T>(int entity) where T : Component
            => ComponentManager.GetComponent<T>(entity);
    }

    public class SystemManager : MonoBehaviour
    {
        private List<System> _systems = new List<System>();
        private readonly object _lock = new object();
        public static SystemManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterSystem(System system)
        {
            lock (_lock)
            {
                if (!_systems.Contains(system))
                {
                    _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                    _systems.Add(system);
                }
            }
        }

        public void UnregisterSystem(System system)
        {
            lock (_lock)
            {
                if (_systems.Contains(system))
                {
                    _systems.Remove(system);
                }
            }
        }

        void Update()
        {
            lock (_lock)
            {
                // 每帧最多更新3个中等及以上优先级系统
                int count = 0;
                foreach (var system in _systems)
                {
                    if (system.Priority >= SystemPriority.Normal && count++ >= 3) break;
                    system.Update();
                }
            }
        }
    }
}