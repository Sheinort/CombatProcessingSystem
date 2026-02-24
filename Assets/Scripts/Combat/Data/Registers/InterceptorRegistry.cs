using System;
using System.Collections.Generic;
using System.Linq;

namespace Combat
{
    /// <summary>
    /// Instantiates and hold instances of Interceptors.
    /// Provides methods to Add and Remove entities from them.
    /// </summary>
    public sealed class InterceptorRegistry: IDisposable
    {
        private readonly Dictionary<Type, List<IInterceptor>> _interceptorsBySubInterface = new();
        private readonly Dictionary<Type, IInterceptor> _concreteToInstance = new();
        public InterceptorRegistry(int initialSize)
        {
            var subInterfaces = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsInterface
                            && t != typeof(IInterceptor)
                            && typeof(IInterceptor).IsAssignableFrom(t))
                .ToArray();

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IInterceptor).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                var instance = (IInterceptor)Activator.CreateInstance(type);
                instance.Initialize(initialSize);
                
                foreach (var subInterface in subInterfaces)
                {
                    if (!subInterface.IsAssignableFrom(type)) continue;
                    _concreteToInstance[type] = instance;
                    if (!_interceptorsBySubInterface.TryGetValue(subInterface, out var list))
                    {
                        list = new List<IInterceptor>();
                        _interceptorsBySubInterface[subInterface] = list;
                    }
                    list.Add(instance);
                }
            }
        }
        public TInterceptor GetInterceptor<TInterceptor>() where TInterceptor : IInterceptor
        {
            if (_concreteToInstance.TryGetValue(typeof(TInterceptor), out var interceptor))
                return (TInterceptor)interceptor;
            return default;
        }
        public IReadOnlyList<TInterceptor> GetInterceptors<TInterceptor>() where TInterceptor : IInterceptor
        {
            if (_interceptorsBySubInterface.TryGetValue(typeof(TInterceptor), out var list))
                return list.Cast<TInterceptor>().ToList();
            return Array.Empty<TInterceptor>();
        }

        public void AddToInterceptors<TInterceptor>(EntityID entityID, InterceptorData data) where TInterceptor : IInterceptor
        {
            if (entityID.Value < 0) return;
            if (_concreteToInstance.TryGetValue(typeof(TInterceptor), out var interceptor))
                    interceptor.Add(entityID, data);
        }

        public void RemoveFromInterceptors<TInterceptor>(EntityID entityID) where TInterceptor : IInterceptor
        {
            if (_concreteToInstance.TryGetValue(typeof(TInterceptor), out var interceptor))
                interceptor.Remove(entityID);
        }

        public void Dispose()
        {
            foreach (var interceptor in _concreteToInstance) {
                (interceptor.Value as IDisposable)?.Dispose();
            }
        }
    }
}