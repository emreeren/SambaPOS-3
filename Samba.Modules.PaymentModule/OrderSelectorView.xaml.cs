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

namespace Samba.Modules.PaymentModule
{
    /// <summary>
    /// Interaction logic for OrderSelectorView.xaml
    /// </summary>
    
    [Export]
    public partial class OrderSelectorView : UserControl
    {
        [ImportingConstructor]
        public OrderSelectorView(OrderSelectorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
