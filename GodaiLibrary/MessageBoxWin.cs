using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GodaiLibrary
{
    public class MessageBox2
    {
        public enum GQButtonType
        {
            YesNo,
            Close
        }
        public enum GQResponseType
        {
            None,
            Close = 1,
            No,
            Yes
        }

        public static void Show(String message)
        {
            MessageBox.Show(message);
        }


    }
}
