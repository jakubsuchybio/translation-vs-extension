using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using TranslationExtension.Commands;
using Task = System.Threading.Tasks.Task;

namespace TranslationExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidTranslationExtensionPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class AsyncVSPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await DontTranslate.InitializeAsync(this);
            await Translate.InitializeAsync(this);
        }
    }
}
