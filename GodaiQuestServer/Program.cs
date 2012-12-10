using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GodaiQuestServer
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

            FormServer form = new FormServer();
            ServerWorker worker = new ServerWorker();
            worker.startThread(form);
            form.setWorker(worker);
            Application.Run(form);
            worker.setStopThread();
        }
    }
}
