using System;
using System.Collections.Generic;
using Ninject;
using Ninject.Syntax;
using Ninject.Parameters;
using Ninject.Modules;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Ninject.Selection.Heuristics;
using Ninject.Components;
using System.Reflection;

namespace FourRoads.Common
{
    /// <summary>
    /// Provides Configuration information for Dependency Injection
    /// </summary>
    /// <remarks>
    /// This class is required for loading bindings into the Injector class. Each instance of InjectionSettings should have a unique Key. Duplicate keys will be ignored.
    /// The bindingModulesFilePathPattern property can be null, and if it is not must point to a valid folder, but the pattern does not have to return any config files.
    /// NInject requires default bindings of type INinjectModule, so the array must not be null.
    /// </remarks> 
    public class InjectionSettings
    {
        private readonly string _key;
        private readonly string _bindingModulesFilePathPattern;
        private readonly INinjectModule[] _initialModules;

        /// <summary>
        /// Initializes a new InjectionSettings object for passing to Injector.LoadBindings
        /// </summary>
        /// <param name="key">A unique key for this binding instance.</param>
        /// <param name="bindingModulesFilePathPattern">The location path for module override configuration files.</param>
        /// <param name="initialModules">An array of INinjectModule for default bindings.</param>
        public InjectionSettings(string key, string bindingModulesFilePathPattern, params INinjectModule[] initialModules)
        {
            _key = key;
            _bindingModulesFilePathPattern = bindingModulesFilePathPattern;
            _initialModules = initialModules;
        }

        /// <summary>
        /// Gets the unique key for this binding instance.
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the location path for module override configuration files.
        /// </summary>
        public string BindingModulesFilePathPattern
        {
            get { return _bindingModulesFilePathPattern; }
        }

        /// <summary>
        /// Gets the array of default bindings for this binding instance.
        /// </summary>
        public INinjectModule[] InitialModules
        {
            get { return _initialModules; }
        }
    }
}
