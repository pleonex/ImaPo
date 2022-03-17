// Copyright (c) 2022 Benito Palacios Sánchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using Eto.GtkSharp.Forms.Controls;
using ImaPo.UI.Main;

namespace ImaPo.Gtk
{
    /// <summary>
    /// Main program class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry-point.
        /// </summary>
        /// <param name="args">Application arguments.</param>
        public static void Main(string[] args)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                string gtkLibs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

                string path = Environment.GetEnvironmentVariable("Path");
                Environment.SetEnvironmentVariable("Path", $"{gtkLibs};{path}");
            }

            Eto.Style.Add<TreeGridViewHandler>(
                "analyze-tree",
                handler => handler.Font = new Font("Ubuntu Nerd Font", 10));

            new Application(Eto.Platforms.Gtk).Run(new MainWindow());
        }
    }
}
