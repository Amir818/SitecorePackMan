﻿namespace Unic.PackMan.Core.PackageGeneration
{
    using System;
    using System.Collections.Generic;
    using Sitecore.Configuration;
    using Sitecore.Data.Items;
    using Sitecore.Install;
    using Sitecore.Install.Items;
    using Sitecore.Install.Zip;
    using Sitecore.IO;

    /// <summary>
    /// The package generation service
    /// </summary>
    public class PackageGenerationService : IPackageGenerationService
    {
        /// <summary>
        /// Generates the package.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="packageAuthor">The package author.</param>
        /// <param name="downloadPath">The download path.</param>
        /// <returns>The result of the generation</returns>
        public bool GeneratePackage(IEnumerable<Item> items, string packageName, string packageAuthor, out string downloadPath)
        {
            downloadPath = null;
            
            try
            {
                var project = new PackageProject
                                  {
                                      Metadata =
                                          {
                                              PackageName = packageName,
                                              Author = packageAuthor,
                                              Version = DateTime.Now.ToString("yyyyMMddHHmmss")
                                          }
                                  };

                var filePath = FileUtil.MakePath(Settings.PackagePath, string.Format("{0}-{1}.{2}", project.Metadata.Version, PackageUtils.CleanupFileName(project.Metadata.PackageName), "zip"));
                var source = new ExplicitItemSource { Name = "PackMan added Items" };

                foreach (var item in items)
                {
                    source.Entries.Add(new ItemReference(item.Uri, false).ToString());
                }

                project.Sources.Add(source);
                using (new Sitecore.SecurityModel.SecurityDisabler())
                {
                    WritePackage(project, filePath);
                    downloadPath = filePath;
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(ex.Message, ex, this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes the package.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="filePath">The file path.</param>
        private static void WritePackage(PackageProject project, string filePath)
        { 
            using (var writer = new PackageWriter(filePath))
            {
                writer.Initialize(Installer.CreateInstallationContext());
                PackageGenerator.GeneratePackage(project, writer);
            }
        }
    }
}
