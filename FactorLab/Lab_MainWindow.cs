using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
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

    enum Outcome { Success, LuckyHit, Trivial, ZeroFactor }

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

        makeButton("Test1", (s, e) => {
            StringBuilder sb = new StringBuilder();
            int k = 0;
            for (int i=2; i < 1000; i++)
            {
                if (Utils.IsPrime(i))
                {
                    Debug.Write($"{i},");
                    if (++k == 20)
                    {
                        Debug.WriteLine("");
                        k = 0;
                    }
                }
            }
            Debug.WriteLine("");
        });
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
        //  findXFor(35); 
        List<(int n, int x, List<(int period, int factor)> periods)> workingPeriods = findWorkingPeriods(109 * 29); //(419 * 821);// // (419 * 821); // (59*37);  //91
     //   showWorkingPeriods(workingPeriods);
    }

    private List<(int n, int x, List<(int period, int factor)> periods)> findWorkingPeriods(int n)
    {
        List<(int n, int x, List<(int period, int factor)> periods)> result = new();
        for (int x = 20; x < 1000; x++)
          //  for (int x = 2; x < n; x++)
            {
            List< (int period, int factor)> working = new ();
            for (int p = 2; p < n; p+=2)
            {
                int factor = periodFindsFactors(n, x, p);
                if (factor >= 2)
                {
                    working.Add((p,factor));
                }
            }
            if (working.Count > 0)
            {
                var entry = (n, x, working);
                result.Add(entry);
                //  string seq = String.Join(", ", entry.periods);
                string seq = String.Join(", ", working.Select(u => { return (u.period, u.factor); }));
                Debug.Write($"N={entry.n}, x={entry.x},  periods: {seq}\n");

                if (result.Count > 2) return result;
            }
        }
        return result;
    }

    void showWorkingPeriods(List<(int n, int x, List<(int period, int factor)> periods)> workingPeriods)
    {
        foreach (var entry in workingPeriods)
        {
            //  string seq = String.Join(", ", entry.periods);
            string seq = String.Join(", ", entry.periods.Select(u => { return ( u.period, u.factor); }));
            Debug.Write($"N={entry.n}, x={entry.x},  periods: {seq}\n");
        }
    }

    private void findXFor(int n)
    {
          
        for (int x = 0; x < n; x++)
        {
            List<int> modExps = Utils.PowersModN(x, n, 100);
            showResults(n, x, modExps);
        }
    }

    private void showResults(int n, int x, List<int> modExps)
    {
        string seq = String.Join(", ", modExps);
        Debug.Write($"N={n}, x={x}: {seq}\n");
 
    }

    // returns a positive factor, or -1, 0, 1 for degenerate cases.
    static int periodFindsFactors(int n, int x, int den)
    {
        //if (den % 2 == 1) den *= 2;
        int crossCheck = Utils.ModExp(x, den, n);
      

        int halfDen = den / 2;
        int v =  Utils.ModExp(x, halfDen, n);
        int a = (v + 1) % n;
        int b = (v - 1) % n;
        if (a == 0 || b == 0)
        {
            // Console.WriteLine($"Our pick for x={x} is leads to a zero which cannot work.  Trying again.");
            return -1;
        }

        int g1 = Utils.GCD(n, a);
        int g2 = Utils.GCD(n, b);
       // Console.Write($" GCD({n},{a})={g1}, GCD({n},{b})={g2}. ");
        int factor = (int)Math.Max(g1, g2);


        if ((factor == n || factor == 1))
        {
            //Console.WriteLine($" Found trivial factors 1 and {n}.  Trying again.");
            return 1;      
        }

        if (factor != 0)
        {
            return factor;
        }
        else  
        {
            // Console.WriteLine("   Found factor to be 0, error. We'll have to try again.");
            return 0;
        }
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

