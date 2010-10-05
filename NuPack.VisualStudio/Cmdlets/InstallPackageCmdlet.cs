﻿using System;
using System.Globalization;
using System.Management.Automation;
using NuPack.VisualStudio.Resources;

namespace NuPack.VisualStudio.Cmdlets {
    /// <summary>
    /// This command installs the specified package into the specified project.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Install, "Package")]
    public class InstallPackageCmdlet : ProcessPackageBaseCmdlet {
        #region Parameters

        [Parameter(Position = 2)]
        public Version Version { get; set; }

        [Parameter(Position = 3)]
        public SwitchParameter IgnoreDependencies { get; set; }

        #endregion

        protected override void ProcessRecordCore() {
            if (!IsSolutionOpen) {
                WriteError(VsResources.Cmdlet_NoSolution);
                return;
            }

            var packageManager = PackageManager;
            bool isSolutionLevelPackage = IsSolutionOnlyPackage(packageManager.SourceRepository, Id, Version);

            if (isSolutionLevelPackage) {
                if (!String.IsNullOrEmpty(Project)) {
                    WriteError(String.Format(
                        CultureInfo.CurrentCulture,
                        VsResources.Cmdlet_PackageForSolutionOnly,
                        Id));
                }
                else {
                    using (new LoggerDisposer(packageManager, this)) {
                        packageManager.InstallPackage(Id, Version, IgnoreDependencies.IsPresent);
                    }
                }
            }
            else {
                var projectManager = ProjectManager;
                if (projectManager != null) {
                    using (new LoggerDisposer(projectManager, this)) {
                        projectManager.AddPackageReference(Id, Version, IgnoreDependencies.IsPresent);
                    }
                }
                else {
                    WriteError(VsResources.Cmdlet_MissingProjectParameter);
                }
            }
        }
    }
}