using System;
using System.IO.Abstractions;
using Castle.Windsor;
using Castle.MicroKernel.Registration;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    public static class WindsorContainerExtensions
    {
        /// <summary>
        /// Registers AvisynthScriptBuilder classes into the IoC container.
        /// </summary>
        /// <param name="services">The IoC services container.</param>
        public static IWindsorContainer AddAvisynthScriptBuilder(this IWindsorContainer services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Register(Component.For<IChangePitchBusiness>().ImplementedBy<ChangePitchBusiness>().LifeStyle.Transient);
            services.Register(Component.For<IAvisynthTools>().ImplementedBy<AvisynthTools>().LifeStyle.Transient);
            services.Register(Component.For<IScriptFactory>().ImplementedBy<ScriptFactory>().LifeStyle.Transient);
            services.Register(Component.For<IScriptPathService>().ImplementedBy<ScriptPathService>().LifeStyle.Transient);
            services.Register(Component.For<IShortFileNameService>().ImplementedBy<ShortFileNameService>().LifeStyle.Transient);

            return services;
        }
    }
}
