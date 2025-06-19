using Pellychan.GUI.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutTesting.Tests
{
    internal class Testy : MainWindow
    {
        public Testy()
        {
            new ScrollBar(this)
            {
                X = 180,
                Y = 120,
                Width = 16,
                Height = 291
            };
        }
    }
}
