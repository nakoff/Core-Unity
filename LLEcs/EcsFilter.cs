using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.LLEcs
{
    public class EcsFilter
    {
        public readonly string Id;
        public readonly HashSet<System.Type> WithComponents = new();
        public readonly HashSet<System.Type> WithoutComponents = new();

        public EcsFilter()
        {
            Id = Guid.NewGuid().ToString();
        }

        public EcsFilter With<T>() where T: MonoBehaviour
        {
            WithComponents.Add(typeof(T));
            return this;
        }

        public EcsFilter Without<T>() where T: MonoBehaviour
        {
            WithoutComponents.Add(typeof(T));
            return this;
        }
    }
}
