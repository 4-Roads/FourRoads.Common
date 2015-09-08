using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FourRoads.Common;
using Ninject;
using Ninject.Components;
using Ninject.Modules;

namespace TestDependencyInjectionAndSettings
{
    public interface ICodeBased {}
    public interface IModuleBased { }
    public interface IModuleCommonBased { }

    public class CodeBased : ICodeBased { }
    public class ModuleBased : IModuleBased { }
    public class ModuleCommonBased : IModuleCommonBased { }

    public class Program
    {
        static void Main(string[] args)
        {
            SettingsTest settings = SettingsTest.Instance();

            Injector.LoadBindingsFromSettings(settings);

            var a = Injector.Get<ICodeBased>();
            var b = Injector.Get<IModuleBased>();
            var c = Injector.Get<IModuleCommonBased>();
        }
    }

    public class TestBindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ICodeBased>().To<CodeBased>();
        }
    }
}
