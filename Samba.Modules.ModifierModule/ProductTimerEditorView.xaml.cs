﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Samba.Modules.ModifierModule
{
    /// <summary>
    /// Interaction logic for ProductTimerEditorView.xaml
    /// </summary>
    
    [Export]
    public partial class ProductTimerEditorView : UserControl
    {
        [ImportingConstructor]
        public ProductTimerEditorView(ProductTimerEditorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
