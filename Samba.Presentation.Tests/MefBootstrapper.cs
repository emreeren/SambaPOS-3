using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Samba.Presentation.Tests
{
    /// <summary>Mef Bootstrapper class.</summary>
    public static class MefBootstrapper
    {
        /// <summary>Composition IOC/DI container.</summary>
        public static readonly CompositionContainer Container;

        /// <summary>Initializes static members of the MefBootstrapper class.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope",
            Justification = "The catalog lives as long as the container lives")]
        static MefBootstrapper()
        {
            var catalog = new AggregateCatalog();

            //// Add This assembly's catalog parts

            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            var lp = new Uri(ass.CodeBase);
            string path = System.IO.Path.GetDirectoryName(lp.LocalPath);

            catalog.Catalogs.Add(new AssemblyCatalog(ass));
            catalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Persistance.dll"));
            catalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Presentation*"));
            catalog.Catalogs.Add(new DirectoryCatalog(path, "Samba.Services*"));
            //// Additional catalog modification could be done here.

            Container = new CompositionContainer(catalog);
        }

        /// <summary>
        /// Composes the parts of the application
        /// </summary>
        /// <param name="attributedPart">Attributed part to compose</param>
        public static void ComposeParts(object attributedPart)
        {
            Container.ComposeParts(attributedPart);
        }

        /// <summary>
        /// Composes the parts of the application
        /// </summary>
        /// <param name="attributedParts">Attributed parts to compose</param>
        public static void ComposeParts(object[] attributedParts)
        {
            Container.ComposeParts(attributedParts);
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>Returns an instance of the type</returns>
        public static T Resolve<T>()
        {
            return Container.GetExportedValue<T>();
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="contractName">Contract name</param>
        /// <returns>Returns an instance of the type</returns>
        public static T Resolve<T>(string contractName)
        {
            return Container.GetExportedValue<T>(contractName);
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>Returns one or more instances of the type</returns>
        public static IEnumerable<T> ResolveMany<T>()
        {
            return Container.GetExportedValues<T>();
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="contractName">Contract name</param>
        /// <returns>Returns one or more instances of the type</returns>
        public static IEnumerable<T> ResolveMany<T>(string contractName)
        {
            return Container.GetExportedValues<T>(contractName);
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>Returns an instance of the type</returns>
        public static Lazy<T> ResolveLazy<T>()
        {
            return Container.GetExport<T>();
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="contractName">Contract name</param>
        /// <returns>Returns an instance of the type</returns>
        public static Lazy<T> ResolveLazy<T>(string contractName)
        {
            return Container.GetExport<T>(contractName);
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <typeparam name="U">Metadata for the exported type</typeparam>
        /// <returns>Returns an instance of the type</returns>
        public static Lazy<T, U> ResolveLazy<T, U>()
        {
            return Container.GetExport<T, U>();
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <typeparam name="U">Metadata for the exported type</typeparam>
        /// <param name="contractName">Contract name</param>
        /// <returns>Returns an instance of the type</returns>
        public static Lazy<T, U> ResolveLazy<T, U>(string contractName)
        {
            return Container.GetExport<T, U>(contractName);
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>Returns one or more instances of the type</returns>
        public static IEnumerable<Lazy<T>> ResolveManyLazy<T>()
        {
            return Container.GetExports<T>();
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="contractName">Contract name</param>
        /// <returns>Returns one or more instances of the type</returns>
        public static IEnumerable<Lazy<T>> ResolveManyLazy<T>(string contractName)
        {
            return Container.GetExports<T>(contractName);
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <typeparam name="U">Metadata for the exported type</typeparam>
        /// <returns>Returns one or more instances of the type</returns>
        public static IEnumerable<Lazy<T, U>> ResolveManyLazy<T, U>()
        {
            return Container.GetExports<T, U>();
        }

        /// <summary>
        /// Resolves an instance of the type
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <typeparam name="U">Metadata for the exported type</typeparam>
        /// <param name="contractName">Contract name</param>
        /// <returns>Returns one or more instances of the type</returns>
        public static IEnumerable<Lazy<T, U>> ResolveManyLazy<T, U>(string contractName)
        {
            return Container.GetExports<T, U>(contractName);
        }

        /// <summary>
        /// Releases an export
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="export">Object instance to release</param>
        public static void Release<T>(Lazy<T> export)
        {
            Container.ReleaseExport<T>(export);
        }

        /// <summary>
        /// Releases an export
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <typeparam name="U">Metadata for the exported type</typeparam>
        /// <param name="export">Object instance to release</param>
        public static void Release<T, U>(Lazy<T, U> export)
        {
            Container.ReleaseExport<T>(export);
        }

        /// <summary>
        /// Releases many exports
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <param name="exports">Object instances to release</param>
        public static void ReleaseMany<T>(IEnumerable<Lazy<T>> exports)
        {
            Container.ReleaseExports<T>(exports);
        }

        /// <summary>
        /// Releases many exports
        /// </summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <typeparam name="U">Metadata for the exported type</typeparam>
        /// <param name="exports">Object instances to release</param>
        public static void ReleaseMany<T, U>(IEnumerable<Lazy<T, U>> exports)
        {
            Container.ReleaseExports<T>(exports);
        }

        public static void ComposeParts()
        {
            Container.ComposeParts();
        }
    }
}