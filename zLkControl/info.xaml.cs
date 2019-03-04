using MahApps.Metro.Controls;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
namespace zLkControl
{

    /// <summary>
    /// info.xaml 的交互逻辑
    /// </summary>
    public partial class info : MetroWindow, IComponentConnector
    {
        internal info infoWindow =null ;
        public info(MainWindow MyMainWindow)
        {
            this.InitializeComponent();
        }
    }
}
