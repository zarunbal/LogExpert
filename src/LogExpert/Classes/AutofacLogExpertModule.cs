using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

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
        }
    }
}