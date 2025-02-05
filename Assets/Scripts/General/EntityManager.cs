// EntityManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECS
{

    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance { get; private set; }

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

        private int nextEntityId = 1; // 从1开始避免0的歧义
        private readonly HashSet<int> activeEntities = new HashSet<int>();
        private readonly Queue<int> recycledIds = new Queue<int>();

        public int GetEntityCount()
        {
            return activeEntities.Count;
        }

        public int CreateEntity()
        {
            int id = recycledIds.Count > 0 ? recycledIds.Dequeue() : nextEntityId++;
            activeEntities.Add(id);
            return id;
        }

        public void RemoveEntity(int entityId)
        {
            if (!activeEntities.Contains(entityId))
            {
                return;
            }
            OnEntityDestroying?.Invoke(entityId);
            if (activeEntities.Remove(entityId))
            {
                if (recycledIds.Count > 1000)
                    return;
                recycledIds.Enqueue(entityId);
            }
        }

        public event Action<int> OnEntityDestroying;

        public IEnumerable<int> GetActiveEntities() => activeEntities;

        public List<int> GetActiveEntitiesSnapshot()
        {
            return new List<int>(activeEntities); // 创建副本
        }
    }
}