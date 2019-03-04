namespace zLkControl
{
    using MahApps.Metro.Controls;
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Markup;

    public class About : MetroWindow, IComponentConnector
    {
        private bool _contentLoaded;
        internal About AboutWindow;

        public About(MainWindow MyMainWindow)
        {
            this.InitializeComponent();
            this.AboutWindow.Owner = MyMainWindow;
            this.AboutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        [DebuggerNonUserCode, GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            if (!this._contentLoaded)
            {
                this._contentLoaded = true;
                Uri resourceLocator = new Uri("/WINCC_TF;component/about.xaml", UriKind.Relative);
                Application.LoadComponent(this, resourceLocator);
            }
        }

        [DebuggerNonUserCode, GeneratedCode("PresentationBuildTasks", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            if (connectionId == 1)
            {
                this.AboutWindow = (About) target;
            }
            else
            {
                this._contentLoaded = true;
            }
        }
    }
}

