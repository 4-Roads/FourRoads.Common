using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using FourRoads.Common.Base;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Xml;
using Ninject.Extensions.Xml.Extensions;
using Ninject.Extensions.Xml.Processors;
using Ninject.Infrastructure.Language;
using Ninject.Planning.Bindings;
using Ninject.Planning.Directives;
using Ninject.Planning.Targets;
using Ninject.Syntax;
using Ninject.Parameters;
using Ninject.Modules;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Ninject.Selection.Heuristics;
using Ninject.Components;

namespace FourRoads.Common
{

    /// <summary>
    /// Provides dependency injection functionality to applications. This static class loads specified implementation bindings for your application's Interfaces,
    /// and allows your code to separate implementation from consumption of your business objects.
    /// </summary>
    public static class Injector
    {
        private static ChildKernel _root;
        private static readonly ReaderWriterLockSlim _lock;
        private static readonly ThreadSafeDictionary<string, InjectionSettings> _bindingsLookup;

        public class XmlModuleLoaderPluginEx :  XmlModuleLoaderPlugin ,IModuleLoaderPlugin
        {
            public XmlModuleLoaderPluginEx(IKernel kernel, IEnumerable<IModuleChildXmlElementProcessor> elementProcessors) : base(kernel, elementProcessors) {}

            IEnumerable<string> IModuleLoaderPlugin.SupportedExtensions
            {
                get { return base.SupportedExtensions.Concat(new[] {".config"}); }

            }
        }

        public class ReverseConstructorScorer : NinjectComponent, IConstructorScorer
        {
            public int Score(IContext context, ConstructorInjectionDirective directive)
        	{
				if (context == null) throw new ArgumentNullException("context", "Cannot be null");
				if (directive == null) throw new ArgumentNullException("directive", "Cannot be null");

				if (directive.Constructor.HasAttribute(Settings.InjectAttribute))
					return Int32.MaxValue;

				int score = 0;
				foreach (ITarget target in directive.Targets)
				{
					ITarget target1 = target;
					foreach (IParameter parameter in context.Parameters.Where(parameter => string.Equals(target1.GetType(), parameter.GetType())))
					{
						score++;
					}

					Type targetType = target.Type;
					if (targetType.IsArray)
						targetType = targetType.GetElementType();

					if (targetType.IsGenericType && targetType.GetInterfaces().Any(type => type == typeof(IEnumerable)))
						targetType = targetType.GetGenericArguments()[0];

					if (context.Kernel.GetBindings(targetType).Count() > 0)
						score--;
				}

				return score;
        	}
        }

        public class BindElementHandler : BindXmlElementProcessor, IModuleChildXmlElementProcessor
        {
            public BindElementHandler(IBindingBuilderFactory bindingBuilderFactory, IChildElementProcessor childElementProcessor)
                : base(bindingBuilderFactory, childElementProcessor)
            {
            }

            void IModuleChildXmlElementProcessor.Handle(IBindingRoot module, XElement element)
            {
                //System.Diagnostics.Debugger.Break();

                if (ReadReBind(element))
                {
                    
                    IEnumerable<IBinding> currentBindings = ((IKernel)Injector.ResolutionRoot).GetBindings(GetTypeFromAttributeValue(ExtensionsForXElement.RequiredAttribute(element, "service")));

                    foreach (IBinding currentBinding in currentBindings)
                        module.RemoveBinding(currentBinding); 

                    //Remove the rebind attribute as this causes issues now in the new injector
                    element.Attribute("rebind").Remove();
                }
                Handle(module, element);
            }


            private static Type GetTypeFromAttributeValue(XAttribute attribute)
            {
                Type type = Type.GetType(attribute.Value, false);
                if (type == (Type)null)
                    throw new Exception(string.Format("Couldn't resolve type '{0}' defined in '{1}' attribute.", (object)attribute.Value, (object)attribute.Name));
                else
                    return type;
            }

            private static bool ReadReBind(XElement element)
            {
                XAttribute xattribute = element.Attribute("rebind");
        
                if (xattribute == null)
                    return false;

                return string.Compare(xattribute.Value, bool.TrueString, true) == 0;
            }
        }

        static Injector()
        {
        	_lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _bindingsLookup = new ThreadSafeDictionary<string, InjectionSettings>();
        }

        private static void VerifyInitialized()
        {
            if (Provider == null)
            {
                //HACK because the CRON job starts up and fires all threads off at once there is an opportunity for things to not be initialized
                SpinWait.SpinUntil(() => Provider != null, new TimeSpan(0, 1, 0));

                //If its still null then we have a real problem
                if (Provider == null)
                {
                    throw new InvalidOperationException("The dependency injector has not been initialized");
                }
            }
        }

        /// <summary>
        /// Provides access to the standard .NET IServiceProvider interface of the NInject Standard Kernel
        /// </summary>
        public static IServiceProvider Provider
        {
            get
            {
                return _root;
            }
        }

        /// <summary>
        /// Provides access to the IResolutionRoot interface of the NInject Standard Kernel
        /// </summary>
        public static IResolutionRoot ResolutionRoot
        {
            get
            {
                return _root;
            }
        }

		/// <summary>
		/// Load a <see cref="InjectionSettings"/> instance from configuration
		/// </summary>
		/// <param name="configuration">The configuration.</param>
        public static void LoadBindingsFromSettings(ISettings configuration)
		{
			_lock.EnterUpgradeableReadLock();
			try
			{
				// Ensure this settings instance has not already been loaded
                if (!Contains(configuration.FileName))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        LoadBindings(configuration);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
			}
			finally
			{
				_lock.ExitUpgradeableReadLock();
			}
		}

        private static void LoadBindings(ISettings configuration)
		{
			List<NinjectModule> modules = new List<NinjectModule>();
			string path = string.Empty;

			if (configuration.InjectionModules == null)
				return;

			#region Load Modules

			if (configuration.InjectionModules.Modules != null)
			{
				foreach (InjectionModule moduleInj in configuration.InjectionModules.Modules)
				{
					// Ensure a type has been defined for this module
					if (String.IsNullOrEmpty(moduleInj.Type))
					{
						throw new InjectionModuleLoadException(configuration.FileName, "No type was defined");
					}

					Type type = Type.GetType(moduleInj.Type);

					// Ensure an existing type has been defined
					if (type == null)
					{
						throw new InjectionModuleLoadException(configuration.FileName,
						                                       string.Format("The type {0} does not exist", moduleInj.Type));
					}

					NinjectModule module = Activator.CreateInstance(type) as NinjectModule;

					// Ensure the defined type inherits from Ninject.NinjectModule
					if (module == null)
					{
						throw new InjectionModuleLoadException(configuration.FileName,
						                                       string.Format("The type {0} could not be instantiated", moduleInj.Type));
					}

					modules.Add(module);
				}

			}

			#endregion

			if (!string.IsNullOrEmpty(configuration.InjectionModules.OverridesPath))
			{
				path = GetPath(configuration.InjectionModules.OverridesPath);
			}

            LoadBindings(new InjectionSettings(configuration.FileName, path, modules.ToArray()), configuration.ParentKernel);

            if (!string.IsNullOrEmpty(configuration.InjectionModules.SharedModulePath))
            {
                LoadBindings(new InjectionSettings("FourRoadsSharedBindings", GetPath(configuration.InjectionModules.SharedModulePath), new INinjectModule[0]), configuration.ParentKernel);
            }
		}

    	public static string GetPath(string source)
        {
            if (source.Contains("/"))
                source = source.Replace("/", "\\");

            if (source.StartsWith("~"))
                source = source.Substring(1);

            if (source.StartsWith("\\"))
                source = source.Substring(1);

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, source);
        }

        /// <summary>
        /// Loads dependency injection bindings. This should be called only once for any set of bindings.
        /// </summary>
        /// <param name="settings">An InjectionSettings instance with the required values for initializing bindings.</param>
        /// <param name="parentKernel"></param>
        public static void LoadBindings(InjectionSettings settings , IKernel parentKernel=null)
        {
            _lock.EnterWriteLock();

            try
            {
                //System.Diagnostics.Debugger.Break();

                if (!_bindingsLookup.ContainsKey(settings.Key))
                {
                    // Create the NInject Kernel
                    NinjectSettings nsettings = new NinjectSettings
                    {
                        InjectAttribute = typeof(InjectAttribute),
                    };

                    //That Creates the main kernel
                    if (Provider == null)
                    {
                        //System.Diagnostics.Debugger.Break();

                        _root = new ChildKernel(parentKernel, nsettings);   

                        // Score class constructors based on old Ninject pattern of calling default constructor first
                        _root.Components.RemoveAll(typeof(IConstructorScorer));
                        _root.Components.Add<IConstructorScorer, ReverseConstructorScorer>();

                        _root.Components.RemoveAll(typeof(IModuleChildXmlElementProcessor));
                        _root.Components.Add<IModuleChildXmlElementProcessor, BindElementHandler>();

                        _root.Components.RemoveAll(typeof(XmlModuleLoaderPlugin));
                        _root.Components.Add<IModuleLoaderPlugin, XmlModuleLoaderPluginEx>();
                    }

                    
                    //For each sub product create a child kernel
                    // Get the main NInject Kernel
                    if (parentKernel != null && _root.ParentResolutionRoot == null)
                        _root.ParentResolutionRoot = parentKernel;

                    _root.Load(settings.InitialModules);

                    // Load any overrides
                    if (!string.IsNullOrEmpty(settings.BindingModulesFilePathPattern) && Directory.Exists(Path.GetDirectoryName(settings.BindingModulesFilePathPattern)))
                        _root.Load(new[] { settings.BindingModulesFilePathPattern });

                    // Add the Bindings to the Lookup table
                    _bindingsLookup.Add(settings.Key, settings);
                }
            }
            catch (Exception ex)
            {
                throw new InjectionModuleLoadException(settings.Key, "Unable to load module", ex);
            }
            finally
            {
               _lock.ExitWriteLock(); 
            }
		}

		/// <summary>
		/// Unloads all dependency injection bindings.
		/// </summary>
		public static void Unload()
		{
			_lock.EnterWriteLock();

			try
			{
				//System.Diagnostics.Debugger.Break();
				while (_bindingsLookup.Keys.Count > 0)
				{
					_bindingsLookup.Remove(_bindingsLookup.Keys.Last());
				}

				_root = null;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

        private static bool Contains(string key)
		{
            _lock.EnterReadLock();
            try
            {
                return _bindingsLookup.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
		}

        /// <summary>
        /// Gets an object binding by type.
        /// </summary>
        /// <typeparam name="T">The type representing the object binding you want returned.</typeparam>
        /// <returns>A typed instance of the requested Type</returns>
        public static T Get<T>()
        {
            VerifyInitialized();
            return (T)Provider.GetService(typeof(T));
        }

        /// <summary>
        /// Gets all objects by binding by type.
        /// </summary>
        /// <typeparam name="T">The type representing the object binding you want returned.</typeparam>
        /// <returns>A typed instance of the requested Type</returns>
        public static IEnumerable<T> GetAll<T>()
        {
            VerifyInitialized();
            IResolutionRoot root = Provider as IResolutionRoot;
            return root.GetAll<T>();
        }


        /// <summary>
        /// Gets all ojects binding by type with parameters.
        /// </summary>
        /// <typeparam name="T">The type representing the object binding you want returned.</typeparam>
        /// <param name="arguments">an array of IParameter objects representing the constructor parameters.</param>
        /// <returns>A typed instance of the requested Type</returns>
        public static IEnumerable<T> GetAll<T>(params IParameter[] arguments)
        {
            VerifyInitialized();
            IResolutionRoot root = Provider as IResolutionRoot;
            return root.GetAll<T>(arguments);
        }

        /// <summary>
        /// Gets an object binding by type with parameters.
        /// </summary>
        /// <typeparam name="T">The type representing the object binding you want returned.</typeparam>
        /// <param name="arguments">an array of IParameter objects representing the constructor parameters.</param>
        /// <returns>A typed instance of the requested Type</returns>
        public static T Get<T>(params IParameter[] arguments)
        {
            VerifyInitialized();
            IResolutionRoot root = Provider as IResolutionRoot;
            return root.Get<T>(arguments); 
        }

    /// <summary>
        /// Gets an object binding by type.
        /// </summary>
        /// <param name="type">The type representing the object binding you want returned.</param>
        /// <returns>Object instance of specified type.</returns>
        public static object Get(Type type)
        {
            VerifyInitialized();
            return Provider.GetService(type);
        }

        public static T Get<T>(string name)
        {
            VerifyInitialized();
            IResolutionRoot root = Provider as IResolutionRoot;
            return root.Get<T>(name, new IParameter[] {});
        }

        public static T Get<T>(string name, params IParameter[] arguments)
        {
            VerifyInitialized();
            IResolutionRoot root = Provider as IResolutionRoot;
            return root.Get<T>(name, arguments); 
        }
        /// <summary>
        /// Serializes an instance of a generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <returns>A serialized XML string</returns>
        public static string XmlSerialize<T>(T obj)
        {
            StringBuilder resultStrBuilder = new StringBuilder(500);
            using (StringWriter sw = new StringWriter(resultStrBuilder))
            {
                XmlSerializer xs = XmlInjectedSerializationFactory.Create(typeof (T));
                xs.Serialize(sw, obj);
            }

            return resultStrBuilder.ToString();
        }

        /// <summary>
        /// Deserializes an XML string into an instance of a generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">The serialized XML string.</param>
        /// <returns>An instance of the specified type</returns>
        public static T XmlDeserialize<T>(string xml)
        {
            XmlSerializer xs = XmlInjectedSerializationFactory.Create(typeof(T));
            StringReader sr = new StringReader(xml);
            XmlTextReader tr = new XmlTextReader(sr);

            return (T)xs.Deserialize(tr);
        }
    }

    
}
