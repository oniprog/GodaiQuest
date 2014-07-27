using System;
using Gtk;
using GodaiLibrary;
using GodaiLibrary.GodaiQuest;
using System.Linq;

namespace GodaiQuestServer
{
	public partial class FormServerMono : Gtk.Dialog
	{
		private ServerWorker _serverWorker;

		public FormServerMono ()
		{
			this.Build ();
			GodaiLibrary.MessageBox2.SetParentWindow( this );
		}
		public void setWorker (ServerWorker worker)
		{
			_serverWorker = worker;
		}

		public void addLog( string strLog ) {
			Gtk.Application.Invoke(delegate {
				var dateTime = DateTime.Now;
				textLog.Buffer.Text = "["+dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString()+ "]:"+ strLog + "\r\n" + textLog.Buffer.Text;
				if ( textLog.Buffer.LineCount > 1000 ) {
					String[] lines = textLog.Buffer.Text.Split('\n');
					textLog.Buffer.Text = ""; // 重いけれどもとりあえず
					for( int it=0; it<500; ++it ) {
						textLog.Buffer.Text += lines[it] + "\n";
					}
				}
			});
		}
		protected void OnButtonTestClicked (object sender, EventArgs e)
		{
		}
		protected void OnClose(object o, System.EventArgs e) {
			Application.Quit();
		}
		protected void OnBtnInitClicked (object sender, EventArgs e)
		{
			using( var dlgYesNo = new GodaiLibrary.MessageBox2("質問", "完全に初期化します。よろしいですか？", MessageBox2.GQButtonType.YesNo) ) {
				if ( dlgYesNo.ShowModal() != GodaiLibrary.MessageBox2.GQResponseType.Yes )
					return;
			}

			DungeonBlockImageInfo dungeonimages;
			_serverWorker.getDungeonBlockImage(out dungeonimages);
			if (dungeonimages.Count() != 0)
			{
				MessageBox2.Show("mongodbを停止したあと，mongodbの中身を空にしてください(/usr/lib/mongodb?)．その後，再びこのボタンを押してください");
				return;
			}

			_serverWorker.ForceInitializeMongoDB();
		}
	}
}

