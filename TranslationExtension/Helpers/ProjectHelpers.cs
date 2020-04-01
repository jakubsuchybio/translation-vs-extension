using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace TranslationExtension.Helpers
{
    internal static class ProjectHelpers
    {
        ///<summary>Gets the TextView for the active document.</summary>
        internal static IWpfTextView GetCurentTextView()
        {
            IComponentModel componentModel = GetComponentModel();
            if (componentModel == null) return null;
            IVsEditorAdaptersFactoryService editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            IVsTextView nativeView = GetCurrentNativeTextView();

            if (nativeView != null)
                return editorAdapter.GetWpfTextView(nativeView);

            return null;
        }

        internal static IVsTextView GetCurrentNativeTextView()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var textManager = (IVsTextManager)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
            Assumes.Present(textManager);

            textManager.GetActiveView(1, null, out IVsTextView activeView);
            return activeView;
        }

        internal static IComponentModel GetComponentModel()
        {
            return (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
        }
    }
}
