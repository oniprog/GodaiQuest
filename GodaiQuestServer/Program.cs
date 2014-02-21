using System;
#if __MonoCS__
using Gtk;
#else
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
#endif


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
#if __MonoCS__
			Application.Init();

			FormServerMono form = new FormServerMono();
            ServerWorker worker = new ServerWorker();
            worker.startThread(form);
            worker.EventWeakUp.WaitOne();
            if (!worker.WakeUpFailed )
            {
				form.setWorker(worker);
				form.Run();
			}
			worker.setStopThread();
#else
            // Keep aliveパケットを有効にする
            System.Net.ServicePointManager.SetTcpKeepAlive(true, 2 * 60 * 60 * 1000, 1000);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormServer form = new FormServer();
            ServerWorker worker = new ServerWorker();
            worker.startThread(form);
            worker.EventWeakUp.WaitOne();
            if (!worker.WakeUpFailed )
            {
                form.setWorker(worker);
                Application.Run(form);
            }
            worker.setStopThread();
#endif
        }
    }
}
