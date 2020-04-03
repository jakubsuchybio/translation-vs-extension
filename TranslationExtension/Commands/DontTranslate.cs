using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using TranslationExtension.Helpers;
using Task = System.Threading.Tasks.Task;

namespace TranslationExtension.Commands
{
    internal sealed class DontTranslate
    {
        public static DontTranslate Instance { get; private set; }

        private readonly AsyncPackage _package;
        private readonly DTE2 _dte2;

        private DontTranslate(AsyncPackage package, DTE2 dte2, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _dte2 = dte2 ?? throw new ArgumentNullException(nameof(dte2));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));


            var menuCommandID = new CommandID(PackageGuids.guidTranslationExtensionPackageCmdSet, PackageIds.DontTranslateId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte2 = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Instance = new DontTranslate(package, dte2, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var view = ProjectHelpers.GetCurentTextView();

            if (view != null)
                InsertTextToEnd(view, _dte2, " //!-!");
        }

        private static void InsertTextToEnd(IWpfTextView view, DTE2 dte, string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                dte.UndoContext.Open("Add NonTranslate Mark");

                using (var edit = view.TextBuffer.CreateEdit())
                {
                    edit.Insert(view.Caret.ContainingTextViewLine.End, text);
                    edit.Apply();
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
            finally
            {
                dte.UndoContext.Close();
            }
        }
    }
}
