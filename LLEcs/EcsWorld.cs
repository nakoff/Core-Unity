using System.Collections.Generic;
using UnityEngine;

namespace Core.LLEcs
{
    public class EcsWorld : MonoBehaviour
    {
        public static EcsWorld Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<EcsWorld>();
                if (_instance != null) return _instance;

                GameObject singletonObject = new GameObject("EcsWorld");
                _instance = singletonObject.AddComponent<EcsWorld>();

                // DontDestroyOnLoad(singletonObject);
                return _instance;
            }
        }

        [SerializeField] private List<EcsEntity> entities = new();

        private static EcsWorld _instance;
        private List<EcsSystem> _allSystems = new();
        private Dictionary<int, IEntityAdded> _initableSystems = new();
        private Dictionary<int, IEntityRemoved> _removableSystems = new();
        private Dictionary<int, IEntitiesUpdate> _updatableSystems = new();

        private List<EcsEntity> _entities = new();
        private List<EcsFilter> _filters = new();
        private Dictionary<string, Dictionary<int, EcsEntity>> _filteredEntities = new();
        private Dictionary<string, List<EcsEntity>> _addedEntities = new();
        private Dictionary<string, List<int>> _removedEntities = new();

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);
        }

        public void Init() {
            if (Instance == null)
            {
                Debug.LogError("EcsWorld is null");
                return;
            }

            foreach (var system in _allSystems)
            {
                if (_filters.Contains(system.Filter))
                    continue;

                _filters.Add(system.Filter);
                UpdateFilteredEntities(system.Filter);
            }
        }

        public void Tick()
        {
            foreach (var f in _filters)
                UpdateFilteredEntities(f);

            var dt = Time.deltaTime;
            foreach (var system in _allSystems)
            {
                var filteredEntities = _filteredEntities[system.Filter.Id];

                // added
                if (_initableSystems.ContainsKey(system.Id)
                        && _addedEntities.ContainsKey(system.Filter.Id)
                        && _addedEntities[system.Filter.Id].Count > 0)
                {
                    var entities = _addedEntities[system.Filter.Id];
                    _initableSystems[system.Id].EntitiesAdded(entities);
                }

                // update
                if (system is IEntitiesUpdate updateSystem)
                    updateSystem.EntitiesUpdate(filteredEntities, dt);
            }

            _addedEntities.Clear();
            _removedEntities.Clear();
        }

        public EcsEntity CreateEntity(string name)
        {
            var go = new GameObject(name, new System.Type[] { typeof(EcsEntity) });
            return go.GetComponent<EcsEntity>();
        }

        public EcsEntity AddEntity(GameObject gameObject)
        {
            var entity = gameObject.GetComponent<EcsEntity>();
            if (entity == null)
                entity = gameObject.AddComponent<EcsEntity>();

            return entity;
        }

        public EcsEntity AddEntity(EcsEntity entity)
        {
            _entities.Add(entity);
            entities.Add(entity);
            return entity;
        }

        public void RemoveEntity(EcsEntity entity)
        {
            _entities.Remove(entity);
            entities.Remove(entity);
            Destroy(entity.gameObject);
        }

        public Dictionary<int, EcsEntity> GetFilteredEntities(EcsFilter filter)
        {
            var math = false;
            foreach (var c in filter.WithComponents)
            {
                foreach (var f in _filters)
                {
                    if (!f.WithComponents.Contains(c) || f.WithoutComponents.Contains(c))
                    {
                        math = false;
                        break;
                    }

                    math = true;
                }
            }

            if (!math)
                UpdateFilteredEntities(filter);

            return _filteredEntities[filter.Id];
        }

        public void RegisterSystem(IEcsSystem system)
        {
            var ecsSystem = (EcsSystem)system;
            _allSystems.Add(ecsSystem);
            if (system is IEntityAdded entityAdded) _initableSystems.Add(ecsSystem.Id, entityAdded);
            if (system is IEntitiesUpdate entitiesUpdate) _updatableSystems.Add(ecsSystem.Id, entitiesUpdate);
            if (system is IEntityRemoved entitiesRemoved) _removableSystems.Add(ecsSystem.Id, entitiesRemoved);
        }

        public void UnregisterAllSystems()
        {
            _allSystems.Clear();
            _updatableSystems.Clear();
            _initableSystems.Clear();
            _removableSystems.Clear();
        }

        private void UpdateFilteredEntities(EcsFilter filter)
        {
            if (!_filteredEntities.ContainsKey(filter.Id))
                _filteredEntities.Add(filter.Id, new());

            Dictionary<int, EcsEntity> newFilteredEntities = new();
            foreach (var entity in _entities)
            {
                var match = false;
                foreach (var c in filter.WithComponents)
                {
                    match = false;

                    if (newFilteredEntities.ContainsKey(entity.Id))
                        break;

                    if (entity.GetComponent(c) == null)
                        break;

                    match = true;
                }

                if (match)
                {
                    foreach (var c in filter.WithoutComponents)
                    {
                        if (entity.GetComponent(c) != null)
                        {
                            match = false;
                            break;
                        }
                    }
                }

                if (match)
                    newFilteredEntities.Add(entity.Id, entity);
            }

            foreach (var c in filter.WithoutComponents)
            {
            }

            // check added entityIds
            List<EcsEntity> addedEntities = new();
            var oldFilteredEntities = _filteredEntities[filter.Id];
            foreach (var (id, entity) in newFilteredEntities)
            {
                if (!oldFilteredEntities.ContainsKey(id))
                    addedEntities.Add(entity);
            }

            if (addedEntities.Count > 0)
            {
                if (!_addedEntities.ContainsKey(filter.Id))
                    _addedEntities.Add(filter.Id, new List<EcsEntity>());
                _addedEntities[filter.Id] = addedEntities;
            }

            _filteredEntities[filter.Id] = newFilteredEntities;
        }
    }
}
