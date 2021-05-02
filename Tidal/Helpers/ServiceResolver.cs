using System;
using System.Windows;
using Prism;

namespace Tidal.Helpers
{
    /// <summary>
    ///   Provides an easier way to get modules from the Container when the code
    ///   can't be initialized by direct injection.
    /// </summary>
    internal static class ServiceResolver
    {
        /// <summary>
        ///   Find (resolve) the specified service interface in the
        ///   application's Container.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns>
        ///   The implementation of the interface.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///   Thrown if the <typeparamref name="TInterface"/> cannot be
        ///   resolved.
        /// </exception>
        public static TInterface Resolve<TInterface>()
        {
            // This check is needed for design time:
            if (Application.Current is PrismApplicationBase app)
                return (TInterface)app.Container.Resolve(typeof(TInterface));

            return default;
        }
    }
}
