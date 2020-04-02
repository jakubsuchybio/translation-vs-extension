using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using TranslationExtension.Helpers;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace TranslationExtension.Commands
{
    internal sealed class Translate
    {
        public static Translate Instance { get; private set; }

        private readonly AsyncPackage package;
        private IAsyncServiceProvider ServiceProvider => package;

        private Translate(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.guidTranslationExtensionPackageCmdSet, PackageIds.TranslateId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new Translate(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();


            var view = ProjectHelpers.GetCurentTextView();
            if (view.Selection.IsEmpty)
            {
                ShowMessage("Jou, takhle ne...", "Hele sorry jako, ale bez selekce to jeste neumim.", OLEMSGICON.OLEMSGICON_WARNING);
                return;
            }

            if(view.Selection.SelectedSpans.Count > 1)
            {
                ShowMessage("Jou, takhle ne...", "Hele sorry jako, ale vice nasobnou selekci neumim a umet nebudu.", OLEMSGICON.OLEMSGICON_WARNING);
                return;
            }

            var selectedString = view.Selection.SelectedSpans[0].GetText();
            if(!selectedString.StartsWith("\"") || !selectedString.EndsWith("\""))
            {
                ShowMessage("Jou, takhle ne...", "Hele sorry jako, ale selekce musi obsahovat uvozovky na zacatku a i na konci.", OLEMSGICON.OLEMSGICON_WARNING);
                return;
            }


        }

        private void ShowMessage(string title, string message, OLEMSGICON icon) =>
            VsShellUtilities.ShowMessageBox(package, title, message, icon, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
}
