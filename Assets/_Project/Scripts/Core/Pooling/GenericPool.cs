using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Pooling
{
    /// <summary>
    /// High-performance pool implementation wrapping Unity's low-overhead ObjectPool.
    /// Automatically invokes IPoolable lifecycle methods on Component instances.
    /// </summary>
    public class GenericPool<T> : IPool<T> where T : Component
    {
        private readonly ObjectPool<T> internalPool;
        private readonly Func<T> factoryMethod;

        public int ActiveCount => internalPool.CountActive;
        public int InactiveCount => internalPool.CountInactive;

        public GenericPool(Func<T> createFunc, int defaultCapacity = 10, int maxSize = 50)
        {
            factoryMethod = createFunc ?? throw new ArgumentNullException(nameof(createFunc));

            internalPool = new ObjectPool<T>(
                createFunc: CreateInstance,
                actionOnGet: OnGetInstance,
                actionOnRelease: OnReleaseInstance,
                actionOnDestroy: OnDestroyInstance,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private T CreateInstance()
        {
            return factoryMethod();
        }

        private void OnGetInstance(T instance)
        {
            if (instance == null) return;

            instance.gameObject.SetActive(true);

            if (instance is IPoolable poolable)
            {
                poolable.OnSpawnedFromPool();
            }
        }

        private void OnReleaseInstance(T instance)
        {
            if (instance == null) return;

            if (instance is IPoolable poolable)
            {
                poolable.OnReturnedToPool();
            }

            instance.gameObject.SetActive(false);
        }

        private void OnDestroyInstance(T instance)
        {
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance.gameObject);
            }
        }

        public T Get() => internalPool.Get();

        public void Release(T instance)
        {
            if (instance == null) return;
            internalPool.Release(instance);
        }
    }
}