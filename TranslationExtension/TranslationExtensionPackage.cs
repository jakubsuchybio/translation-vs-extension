using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using TranslationExtension.Commands;
using Task = System.Threading.Tasks.Task;

namespace TranslationExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(TranslationExtensionPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class TranslationExtensionPackage : AsyncPackage
    {
        public const string PackageGuidString = "0d6bc74b-746f-4204-82b6-c808a74be8dd";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await DontTranslate.InitializeAsync(this);
            await Translate.InitializeAsync(this);
        }
    }
}
