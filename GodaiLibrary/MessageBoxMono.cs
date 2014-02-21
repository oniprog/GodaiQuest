using System;
using Gtk;

/* やむなく作った。WindowsとMonoの差を埋めるための汎用メッセージボックス */

namespace GodaiLibrary
{
	public partial class MessageBox2 : Gtk.Dialog
	{
		public enum GQButtonType {
			YesNo,
			Close
		}
		public enum GQResponseType {
			None,
			Close = 1,
			No,
			Yes
		}

		private Gtk.Label _label;
		private GQResponseType _result;

		private static Gtk.Window _parentWindow;

		public static void SetParentWindow(Gtk.Window parent)
		{
			_parentWindow = parent;
		}

		public static void Show (String message)
		{
			using (var dlg = new MessageBox2( "info", message, GQButtonType.Close )) {
				dlg.ShowModal (); 
			}
		}

		public MessageBox2 (string title, string message, GQButtonType bt) : base(title, _parentWindow, DialogFlags.Modal | DialogFlags.DestroyWithParent, "" )
		{
			SetDefaultSize (300, 80);

			_label = new Label (message);
			VBox.BorderWidth = 1;
			VBox.PackStart (_label, true, true, 2);

			var hbox = new HBox (false, 2);

			if (bt == GQButtonType.YesNo) {
				var btnYes = Button.NewWithLabel ("Yes");
				btnYes.Clicked += HandleClickedYes;
				hbox.PackStart(btnYes, true, false, 1);
				var btnNo = Button.NewWithLabel ("No");
				btnNo.Clicked += HandleClickedNo;
				hbox.PackStart(btnNo, true, false, 1);
			}
			else if (bt == GQButtonType.Close ) {
				var btnClose = Button.NewWithLabel ("閉じる");
				btnClose.Clicked += HandleClickedClose;
				hbox.PackStart(btnClose, true, false, 1);
			}
			VBox.PackEnd(hbox, false, false, 0);
		}

		void HandleClickedYes (object sender, EventArgs e)
		{
			Destroy();
			_result = GQResponseType.Yes;
		}
		void HandleClickedNo(object sender, EventArgs e)
		{
			Destroy();
			_result = GQResponseType.No;
		}
		void HandleClickedClose (object sender, EventArgs e)
		{
			Destroy();
			_result = GQResponseType.Close;
		}

		public GQResponseType ShowModal() {
			ShowAll();
			Run ();
			return _result;
		}
	}
}

