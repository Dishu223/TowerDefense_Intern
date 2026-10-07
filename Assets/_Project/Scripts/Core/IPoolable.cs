namespace Core.Pooling
{
    public interface IPoolable
    {
        /// Called immediately after being retrieved from the pool.
        /// Re-enable components, reset health, and restore baseline visual state.
        void OnSpawnedFromPool();
        
        /// Called immediately before being returned to the pool.
        /// Stop active coroutines/tweens, clear target locks, and unregister from spatial registries.
        void OnReturnedToPool();
    }
}