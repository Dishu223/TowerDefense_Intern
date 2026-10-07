namespace Core.Pooling
{
    /// Generic object pool contract enforcing zero-allocation instance reuse.
    /// <typeparam name="T">Type of object managed by this pool.</typeparam>
    public interface IPool<T> where T : class
    {
        /// Retrieves an active instance from the pool.
        T Get();
        
        /// Recycles an active instance back into the pool.
        void Release(T instance);

        /// Total count of instances currently active in the scene.
        int ActiveCount { get; }

        /// Total count of standby instances waiting in reserve.
        int InactiveCount { get; }
    }
}