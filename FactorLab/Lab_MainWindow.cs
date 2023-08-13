using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FactorLab;

// Pete, Aug 2023.  
// I've chosen wpf to build this as a Windows GUI app, I'm sure I might want to visualize or graph some things.
// Initially, though, I'll drive and develop my logic using the test suite.  Work In Progress

class Lab_MainWindow : Window
{

    Canvas topLevelCanvas;

    public Lab_MainWindow(string[] args)
    {

        // This is responsible for creating containing the main menu, any toolbars, 
        // and a region for the Board, board scaling and window sizing.

        Title = "FactorLab v0.1";

        this.Width = 600;
        this.Height = 400;
        this.Margin = new Thickness(0);

        topLevelCanvas = new Canvas()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ClipToBounds = true,

            Margin = new Thickness(10),
            Background = Brushes.LightGreen
        };
        this.Content = topLevelCanvas;
    }
}




// "How to create a WPF Application without XAML, from scratch"
// https://www.youtube.com/watch?v=EF3U2YIf9v8
public class MyMain
{
    [STAThread]
    public static void Main(string[] args)
    {
        System.Windows.Application wpfApp = new();
        Lab_MainWindow main = new Lab_MainWindow(args);
        wpfApp.Run(main);
    }
}

