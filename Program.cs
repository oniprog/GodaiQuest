using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GodaiQuest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Keep aliveパケットを有効にする
            System.Net.ServicePointManager.SetTcpKeepAlive(true, 2 * 60 * 60 * 1000, 1000);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormScreen());
        }
    }
}
