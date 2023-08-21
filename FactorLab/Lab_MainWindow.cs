using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Helpers;

namespace FactorLab;

// Pete, Aug 2023.  
// I've chosen wpf to build this as a Windows GUI app, I'm sure I might want to visualize or graph some things.
// Initially, though, I'll drive and develop my logic using the test suite.  Work In Progress

class Lab_MainWindow : Window
{

    Canvas topLevelCanvas;
    StackPanel buttonPanel;

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
            ClipToBounds = false,

            Margin = new Thickness(10),
            Background = Brushes.LightGreen
        };
        this.Content = topLevelCanvas;

        buttonPanel = new StackPanel()
        {
            Height = 30,  Width=600,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            FlowDirection = FlowDirection.LeftToRight,
            Background = Brushes.Lavender, 
            Visibility = Visibility.Visible,
            Orientation = Orientation.Horizontal,
        };
        Canvas.SetLeft(buttonPanel, 5);
        Canvas.SetTop(buttonPanel, 5);
        topLevelCanvas.Children.Add(buttonPanel);

        makeButton("Test1", (s, e) => { MessageBox.Show("Hello1"); });
        makeButton("Test2",  B_Click); 
    }

    void makeButton(string caption, RoutedEventHandler handler )
    {
        Button b = new Button()
        {
            Content = caption,
            Margin = new Thickness(4,1,4,1),
        };
        b.Click += handler;  
        buttonPanel.Children.Add(b);
    }

    private void B_Click(object sender, RoutedEventArgs e)
    {
        findXFor(35); 

    }

    private void findXFor(int n)
    {
 
            
        for (int x = 0; x < n; x++)
        {
            List<int> modExps = Utils.PowersModN(x, n, 30);
            showResults(n, x, modExps);
        }
    }

    private void showResults(int n, int x, List<int> modExps)
    {
        string seq = String.Join(", ", modExps);
        Debug.Write($"N={n}, x={x}: {seq}\n");
 
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

