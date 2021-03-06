//-----------------------------------------------------------------------
// <copyright file="PlayerXRayModule.cs" company="Ace Poker Solutions">
// Copyright © 2017 Ace Poker Solutions. All Rights Reserved.
// Unless otherwise noted, all materials contained in this Site are copyrights, 
// trademarks, trade dress and/or other intellectual properties, owned, 
// controlled or licensed by Ace Poker Solutions and may not be used without 
// written consent except as provided in these terms and conditions or in the 
// copyright notice (documents and software) or other proprietary notices 
// provided with the relevant materials.
// </copyright>
//----------------------------------------------------------------------

using DriveHUD.Common.Exceptions;
using DriveHUD.Common.Infrastructure.CustomServices;
using DriveHUD.Common.Infrastructure.Modules;
using DriveHUD.Common.Log;
using DriveHUD.Common.Resources;
using DriveHUD.Common.Security;
using DriveHUD.Common.Wpf.Actions;
using DriveHUD.Common.Wpf.Mvvm;
using DriveHUD.PlayerXRay.Licensing;
using DriveHUD.PlayerXRay.Security;
using DriveHUD.PlayerXRay.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Model;
using System;
using System.IO;
using System.Reflection;

namespace DriveHUD.PlayerXRay
{
    public class PlayerXRayModule : IDHModule
    {
        private const string binDirectory = "bin";

        static readonly FileResourceManager resourceManager = new FileResourceManager("XRay_", "DriveHUD.PlayerXRay.Resources.Common", Assembly.GetAssembly(typeof(PlayerXRayMainViewModel)));

        internal bool IsLicenseValid { get; private set; }

        public void ConfigureContainer(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            LogProvider.Log.Info(CustomModulesNames.PlayerXRay,
                string.Format("---------------=============== Initialize PlayerXRay (v.{0}) ===============---------------",
                Assembly.GetAssembly(typeof(PlayerXRayMainViewModel)).GetName().Version));

#if !DEBUG
            ValidateLicenseAssemblies();
#endif

            CommonResourceManager.Instance.RegisterResourceManager(resourceManager);

            container.RegisterType<ILicenseService, LicenseService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPlayerNotesService, PlayerXRayNoteService>(new ContainerControlledLifetimeManager());
            container.RegisterType<INoteProcessingService, NoteProcessingService>();

            container.RegisterType<IViewModelContainer, PlayerXRayMainView>(CustomModulesNames.PlayerXRay);
            container.RegisterType<IPlayerXRayMainViewModel, PlayerXRayMainViewModel>();

            container.RegisterType<ILicenseManager, XRTReg>(LicenseType.XRayTrial.ToString());
            container.RegisterType<ILicenseManager, XRHReg>(LicenseType.XRayHoldem.ToString());
            container.RegisterType<ILicenseManager, XROReg>(LicenseType.XRayOmaha.ToString());
            container.RegisterType<ILicenseManager, XRCReg>(LicenseType.XRayCombo.ToString());

            InitalizeLicenseService();
        }

        private void ValidateLicenseAssemblies()
        {
            var assemblies = new string[] { "DeployLX.Licensing.v5.dll", "XRCReg.dll", "XRHReg.dll", "XROReg.dll" };
            var assembliesHashes = new string[] { "c1d67b8e8d38540630872e9d4e44450ce2944700", "54035cdd3d0e9cf42f525018d04916bc06dcfe78", "2630a026414426d495e6477eecc1229a22e6bb26", "96ba322c0bebc2f417714f874e698bdf7fafdf2a" };
            var assemblySizes = new int[] { 1032192, 51032, 52056, 52056 };

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assemblyInfo = new FileInfo(assemblies[i]);

                if (!assemblyInfo.Exists && assemblies[i].StartsWith("DeployLX", StringComparison.OrdinalIgnoreCase))
                {
                    assemblyInfo = new FileInfo(Path.Combine(binDirectory, assemblies[i]));
                }

                var isValid = SecurityUtils.ValidateFileHash(assemblyInfo.FullName, assembliesHashes[i]) && assemblyInfo.Length == assemblySizes[i];

                if (!isValid)
                {
                    LogProvider.Log.Error(CustomModulesNames.PlayerXRay, "Module is invalid");
                    throw new DHInternalException(new NonLocalizableString("PlayerXRay module is invalid, so it cannot be initialized"));
                }
            }
        }

        private void InitalizeLicenseService()
        {
            var licenseService = ServiceLocator.Current.GetInstance<ILicenseService>();
            IsLicenseValid = licenseService.Validate();
        }
    }
}
