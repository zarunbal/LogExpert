using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using LogExpert.Enteties;
using LogExpert.Interface;

namespace LogExpert.Classes
{
    public class AutofacLogExpertModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LogTabWindow>().InstancePerLifetimeScope();
            builder.RegisterType<LogExpertProxy>().SingleInstance();
            builder.RegisterType<LogExpertApplicationContext>().SingleInstance();
            builder.RegisterType<LogWindow>().InstancePerLifetimeScope();

            builder.RegisterType<LogWindow.ColumnizerCallback>().InstancePerLifetimeScope();
            builder.RegisterType<LogfileReader>().InstancePerLifetimeScope();

            //builder.RegisterType<LogfileReader>().InstancePerLifetimeScope().UsingConstructor(new MatchingSignatureConstructorSelector(typeof(string)));
            //builder.RegisterType<LogfileReader>().InstancePerLifetimeScope().UsingConstructor(new MatchingSignatureConstructorSelector(typeof(string[])));

            builder.RegisterType<LogReaderOptions>().InstancePerLifetimeScope().As<ILogReaderOptions>();
        }
    }
}