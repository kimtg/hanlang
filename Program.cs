using System;
using System.Linq;
using System.Windows.Forms;
using HanlangGame.Engine;
using HanlangGame.Tests;

namespace HanlangGame;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args != null && args.Contains("--test"))
        {
            bool success = GameTests.RunAllTests();
            Environment.Exit(success ? 0 : 1);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new GameForm());
    }    
}