namespace Core.Pooling
{
    /// <summary>
    /// Generic object pool contract enforcing zero-allocation instance reuse.
    /// </summary>
    /// <typeparam name="T">Type of object managed by this pool.</typeparam>
    public interface IPool<T> where T : class
    {
        /// <summary>
        /// Retrieves an active instance from the pool.
        /// </summary>
        T Get();

        /// <summary>
        /// Recycles an active instance back into the pool.
        /// </summary>
        void Release(T instance);

        /// <summary>
        /// Total count of instances currently active in the scene.
        /// </summary>
        int ActiveCount { get; }

        /// <summary>
        /// Total count of standby instances waiting in reserve.
        /// </summary>
        int InactiveCount { get; }
    }
}