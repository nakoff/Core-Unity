using System.Collections.Generic;
using UnityEngine;

namespace Core.LLEcs
{
    public class EcsEntity : MonoBehaviour
    {
        public int Id => gameObject.GetInstanceID();

        private Dictionary<System.Type, Component> _components = new();

        private void Awake() {
            for (int i = 0; i < gameObject.GetComponentCount(); i++)
            {
                var c = gameObject.GetComponentAtIndex(i);
                _components.Add(c.GetType(), c);
            }

            EcsWorld.Instance.AddEntity(this);
        }

        private void Start() {
        }

        public new T GetComponent<T>() where T: MonoBehaviour
        {
            if (_components.ContainsKey(typeof(T)))
                return (T)_components[typeof(T)];
            return default;
        }

        public new Component GetComponent(System.Type type)
        {
            if (_components.ContainsKey(type))
                return _components[type];
            return default;
        }

        public T AddComponent<T>() where T: MonoBehaviour
        {
            if (_components.ContainsKey(typeof(T)))
                RemoveComponent<T>();

            var c = gameObject.AddComponent<T>();
            _components.Add(c.GetType(), c);
            return c;
        }

        public void RemoveComponent<T>() where T: MonoBehaviour
        {
            var type = typeof(T);
            if (!_components.ContainsKey(type))
                return;
            Destroy(_components[type]);
            _components.Remove(type);
        }

        private void OnDestroy()
        {
            EcsWorld.Instance.RemoveEntity(this);
            // Destroy(gameObject);
        }
    }
}
