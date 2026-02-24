using Unity.Collections;

namespace Combat
{
    public interface IResourceInterceptorBeforeApplication : IInterceptor { }
    public interface IResourceInterceptorBeforeResist : IInterceptor { }
    public interface IResourceInterceptorAfterApplication : IInterceptor { }
    public interface IStatInterceptorBeforeApplication : IInterceptor { }
    public interface IStatInterceptorAfterApplication : IInterceptor { }
    
    public interface IInterceptor
    {
        public void Initialize(int initialSize);
        public void Update(CombatWorld combatWorld);
        public void Add(EntityID entityId, InterceptorData data);
        public void Remove(EntityID entityId);
        public NativeList<InterceptorData> GetInterceptorData(EntityID entityID);
    }
}