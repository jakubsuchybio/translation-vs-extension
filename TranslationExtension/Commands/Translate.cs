using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using System;
using System.ComponentModel.Design;
using System.IO;
using TranslationExtension.Helpers;
using Task = System.Threading.Tasks.Task;

namespace TranslationExtension.Commands
{
    internal sealed class Translate
    {
        public static Translate Instance { get; private set; }

        private readonly AsyncPackage _package;
        private readonly DTE2 _dte2;
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;
        //private readonly IFileToContentTypeService _fileToContentTypeService;

        private Translate(AsyncPackage package, DTE2 dte2, ITextDocumentFactoryService textDocumentFactoryService, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _dte2 = dte2 ?? throw new ArgumentNullException(nameof(dte2));
            _textDocumentFactoryService = textDocumentFactoryService ?? throw new ArgumentNullException(nameof(textDocumentFactoryService));
            //_fileToContentTypeService = fileToContentTypeService ?? throw new ArgumentNullException(nameof(fileToContentTypeService));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.guidTranslationExtensionPackageCmdSet, PackageIds.TranslateId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte2 = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            var textDocumentFactoryService = await package.GetServiceAsync(typeof(ITextDocumentFactoryService2)) as ITextDocumentFactoryService2;
            //var fileToContentTypeService = await package.GetServiceAsync(typeof(IFileToContentTypeService)) as IFileToContentTypeService;
            Instance = new Translate(package, dte2, textDocumentFactoryService, commandService);
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

            if (view.Selection.SelectedSpans.Count > 1)
            {
                ShowMessage("Jou, takhle ne...", "Hele sorry jako, ale vice nasobnou selekci neumim a umet nejspis jen tak nebudu.", OLEMSGICON.OLEMSGICON_WARNING);
                return;
            }

            var selectedString = view.Selection.SelectedSpans[0].GetText();
            if (!selectedString.StartsWith("\"") || !selectedString.EndsWith("\""))
            {
                ShowMessage("Jou, takhle ne...", "Hele sorry jako, ale selekce musi obsahovat uvozovky na zacatku a i na konci.", OLEMSGICON.OLEMSGICON_WARNING);
                return;
            }

            var currentBuffer = view.TextBuffer;
            if (!_textDocumentFactoryService.TryGetTextDocument(currentBuffer, out var document))
            {
                ShowMessage("Jou, neco se pokazilo...", "Sorry jako, nemuzu najit dokument z aktualniho souboru.", OLEMSGICON.OLEMSGICON_WARNING);
                return;
            }

            var gitRoot = GetGitRoot(document.FilePath);
            if (gitRoot == null)
            {
                ShowMessage("Jou, neco se pokazilo...", "Sorry jako, nemuzu najit root git repozitare.", OLEMSGICON.OLEMSGICON_WARNING);
                return;
            }
#if DEBUG
            gitRoot = @"c:\DokumentyGIT\";
#endif

            var translationProjectPath = Path.Combine(gitRoot, "CardioPoint3", "CMS.Translation");
            var localizationCsPath = Path.Combine(translationProjectPath, "Localizations.cs");
            var translationsJsonPath = Path.Combine(translationProjectPath, "Translation.json");
            if (!File.Exists(localizationCsPath) || !File.Exists(translationsJsonPath))
            {
                ShowMessage("Jou, neco se pokazilo...", "Sorry jako, nemuzu najit Localizations.cs nebo Translation.json. Ses v cms repu?", OLEMSGICON.OLEMSGICON_WARNING);
                return;
            }

            //var csharpContentType = _fileToContentTypeService.GetContentTypeForExtension(".cs");
            //var localizationCsBuffer = _textDocumentFactoryService.CreateAndLoadTextDocument(localizationCsPath, csharpContentType);
            //var jsonContentType = _fileToContentTypeService.GetContentTypeForExtension(".json");
            //var translationsJsonBuffer = _textDocumentFactoryService.CreateAndLoadTextDocument(translationsJsonPath, jsonContentType);

            ShowMessage("Jou, winner!!!", "Dostal jsi se daleko muj padavane.", OLEMSGICON.OLEMSGICON_WARNING);
        }

        private void ShowMessage(string title, string message, OLEMSGICON icon) =>
            VsShellUtilities.ShowMessageBox(_package, message, title, icon, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

        private static string GetGitRoot(string path)
        {

            path = Path.GetDirectoryName(path);

            while (path != null)
            {
                if (Directory.Exists(Path.Combine(path, ".git")))
                    return path;

                path = Directory.GetParent(path).FullName;
            }

            return null;
        }
    }
}
