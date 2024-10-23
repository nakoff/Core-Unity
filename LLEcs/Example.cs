using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.LLEcs.Example
{
    public class Main : MonoBehaviour
    {
        private void Start()
        {
            var world = EcsWorld.Instance;
            world.RegisterSystem(new TestSystem(world));
        }

        private void Update() => EcsWorld.Instance.Tick();
        private void OnDestroy() => EcsWorld.Instance.UnregisterAllSystems();
    }

    public class TestComponent : MonoBehaviour { }

    public class TestSystem : IEcsSystem, IEntityAdded, IEntitiesUpdate
    {
        public EcsFilter Filter => _filter;
        private EcsFilter _filter;

        public TestSystem(EcsWorld world) : base()
        {
            _filter = new EcsFilter().With<TestComponent>();
        }

        public void EntitiesAdded(List<EcsEntity> entities)
        {
            foreach (var entity in entities)
            {
                var testComponent = entity.GetComponent<TestComponent>();
                Debug.Log("Added: " + entity);
            }
        }

        public void EntitiesUpdate(Dictionary<int, EcsEntity> entities, float deltaTime)
        {
            foreach (var (id, entity) in entities)
            {
                Debug.Log("Update: " + entity);
                entity.RemoveComponent<TestComponent>();
            }
        }
    }
}
