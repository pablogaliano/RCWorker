namespace JMFamily.Automation.RCWorker
{
	using StructureMap;
	using StructureMap.Pipeline;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Linq.Expressions;
	using IContainer = StructureMap.IContainer;

	public interface IDependencyResolver : IDisposable
	{
		void AddRegistry<T>() where T : Registry, new();
		void RegisterTransientPlugin<T>(Expression<Func<T>> concreteObject);
		void AddRegistry(Registry registry);

		T GetService<T>();
		T GetService<T>(ExplicitArguments explicitArguments);
		T GetService<T>(string serviceName);
		T GetService<T>(string serviceName, ExplicitArguments explicitArguments);

		object GetService(Type serviceType);
		object GetService(Type serviceType, ExplicitArguments explicitArguments);
		object GetService(Type serviceType, string serviceName);
		object GetService(Type serviceType, string serviceName, ExplicitArguments explicitArguments);

		IEnumerable<T> GetServices<T>();
		IEnumerable<object> GetServices(Type serviceType);

		void BuildUp(object @object);
		bool ThrowAllResolveErrors { get; set; }
	}

	public sealed class DependencyResolver : IDependencyResolver
	{
		internal readonly IContainer _container;

		static DependencyResolver()
		{
			Current = new DependencyResolver(new Container(g =>
			{
				g.For<IDependencyResolver>().Use(() => Current);
			}));
		}

		public bool ThrowAllResolveErrors { get; set; }

		internal DependencyResolver(IContainer container)
		{
			_container = container;
		}

		public static IDependencyResolver Current { get; internal set; }

		public void AddRegistry<T>() where T : Registry, new()
		{
			_container.Configure(c => c.AddRegistry<T>());
		}

		public void RegisterTransientPlugin<T>(Expression<Func<T>> concreteObject)
		{
			_container.Configure(c => c.For<T>().Transient().Use(concreteObject));
		}

		public void AddRegistry(Registry registry)
		{
			_container.Configure(c => c.AddRegistry(registry));
		}

		public T GetService<T>()
		{
			return (T)GetService(typeof(T));
		}

		public T GetService<T>(ExplicitArguments explicitArguments)
		{
			return (T)GetService(typeof(T), explicitArguments);
		}

		public T GetService<T>(string serviceName)
		{
			return (T)GetService(typeof(T), serviceName);
		}

		public T GetService<T>(string serviceName, ExplicitArguments explicitArguments)
		{
			return (T)GetService(typeof(T), serviceName, explicitArguments);
		}

		public object GetService(Type serviceType)
		{
			Exceptions.ThrowIfNull(serviceType, "serviceType");

			IContainer container = _container;

			if (container == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			try
			{
				bool useTry = (serviceType.IsAbstract || serviceType.IsInterface) && !ThrowAllResolveErrors;

				return (useTry)
					? container.TryGetInstance(serviceType)
					: container.GetInstance(serviceType);
			}
			catch (StructureMapConfigurationException ex)
			{
				Trace.TraceError("CrmDependencyResolver Error : " + ex.ToString());

				if (ThrowAllResolveErrors)
				{
					throw;
				}

				return null;
			}
		}

		public object GetService(Type serviceType, ExplicitArguments explicitArguments)
		{
			Exceptions.ThrowIfNull(serviceType, "serviceType");
			Exceptions.ThrowIfNull(explicitArguments, "explicitArguments");

			IContainer container = _container;

			if (container == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			try
			{
				return container.GetInstance(serviceType, explicitArguments);
			}
			catch (StructureMapConfigurationException ex)
			{
				Trace.TraceError("CrmDependencyResolver Error : " + ex.ToString());

				if (ThrowAllResolveErrors)
				{
					throw;
				}

				return null;
			}
		}

		public object GetService(Type serviceType, string serviceName)
		{
			Exceptions.ThrowIfNull(serviceType, "serviceType");
			Exceptions.ThrowIfNullOrEmpty(serviceName, "serviceName");

			IContainer container = _container;

			if (container == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			try
			{
				bool useTry = (serviceType.IsAbstract || serviceType.IsInterface) && !ThrowAllResolveErrors;

				return (useTry)
					? container.TryGetInstance(serviceType, serviceName)
					: container.GetInstance(serviceType, serviceName);
			}
			catch (StructureMapConfigurationException ex)
			{
				Trace.TraceError("CrmDependencyResolver Error : " + ex.ToString());

				if (ThrowAllResolveErrors)
				{
					throw;
				}

				return null;
			}
		}

		public object GetService(Type serviceType, string serviceName, ExplicitArguments explicitArguments)
		{
			Exceptions.ThrowIfNull(serviceType, "serviceType");
			Exceptions.ThrowIfNullOrEmpty(serviceName, "serviceName");
			Exceptions.ThrowIfNull(explicitArguments, "explicitArguments");

			IContainer container = _container;

			if (container == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			try
			{
				return container.GetInstance(serviceType, explicitArguments, serviceName);
			}
			catch (StructureMapConfigurationException ex)
			{
				Trace.TraceError("CrmDependencyResolver Error : " + ex.ToString());

				if (ThrowAllResolveErrors)
				{
					throw;
				}

				return null;
			}
		}

		public IEnumerable<T> GetServices<T>()
		{
			return GetServices(typeof(T)).Cast<T>();
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			Exceptions.ThrowIfNull(serviceType, "serviceType");

			IContainer container = _container;

			if (container == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			try
			{
				return container.GetAllInstances(serviceType)
								.Cast<object>();
			}
			catch (StructureMapConfigurationException ex)
			{
				Trace.TraceError("CrmDependencyResolver Error : " + ex.ToString());

				if (ThrowAllResolveErrors)
				{
					throw;
				}

				return new List<object>();
			}
		}

		public void BuildUp(object target)
		{
			IContainer container = _container;

			if (container == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			try
			{
				container.BuildUp(target);
			}
			catch (StructureMapConfigurationException ex)
			{
				Trace.TraceError("CrmDependencyResolver Error : " + ex.ToString());

				if (ThrowAllResolveErrors)
				{
					throw;
				}
			}
		}

		public void Dispose()
		{
			bool disposeContainer = true;

			if (disposeContainer)
			{
				_container.Dispose();
			}
		}
	}
}
