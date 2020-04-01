using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using TranslationExtension.Helpers;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace TranslationExtension.Commands
{
    internal sealed class DontTranslate
    {
        public static DontTranslate Instance { get; private set; }

        private readonly AsyncPackage _package;
        private IAsyncServiceProvider ServiceProvider => _package;

        private DTE2 Dte2 { get; set; }

        private DontTranslate(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));


            var menuCommandID = new CommandID(PackageGuids.guidTranslationExtensionPackageCmdSet, PackageIds.DontTranslateId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new DontTranslate(package, commandService);

            Instance.Dte2 = await package.GetServiceAsync(typeof(DTE)) as DTE2;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IWpfTextView view = ProjectHelpers.GetCurentTextView();

            if (view != null)
                InsertTextToEnd(view, Dte2, " //!-!");
        }

        private static void InsertTextToEnd(IWpfTextView view, DTE2 dte, string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                dte.UndoContext.Open("Add NonTranslate Mark");

                using (ITextEdit edit = view.TextBuffer.CreateEdit())
                {
                    if (!view.Selection.IsEmpty)
                        view.Selection.Clear();

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
