namespace Core.Pooling
{
    /// <summary>
    /// Contract for any object recycled through an object pool.
    /// Manages internal state reset and cleanup without garbage collection.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called immediately after being retrieved from the pool.
        /// Re-enable components, reset health, and restore baseline visual state.
        /// </summary>
        void OnSpawnedFromPool();

        /// <summary>
        /// Called immediately before being returned to the pool.
        /// Stop active coroutines/tweens, clear target locks, and unregister from spatial registries.
        /// </summary>
        void OnReturnedToPool();
    }
}