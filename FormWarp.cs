using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuest
{
    public partial class FormWarp : Form
    {
        private GQCommandMaster mGQCom;
        private UserInfo mUserInfo;
        private List<AUser> mUserList = new List<AUser>();

        public ALocation mLocation;

        public FormWarp(GQCommandMaster com_)
        {
            InitializeComponent();

            this.mGQCom = com_;
        }

        public ALocation getNewLocation()
        {
            return this.mLocation;
        }

        private void FormWarp_Load(object sender, EventArgs e)
        {
            this.mGQCom.getUserInfo(out this.mUserInfo);

            this.cmbTarget.Items.Clear();
            this.mUserList.Clear();

            foreach (var user in this.mUserInfo)
            {
                this.cmbTarget.Items.Add(user.getName());
                this.mUserList.Add(user);
            }
        }

        private void btnWarp_Click(object sender, EventArgs e)
        {
            int nIndex = this.cmbTarget.SelectedIndex;
            if (nIndex < 0)
            {
                MessageBox.Show("ワープ先の場所を指定してください");
                return;
            }

            int nUserID = this.mUserList[nIndex].getUserID();

            LocationInfo locinfo;
            this.mGQCom.getLocationInfo(out locinfo);
            var loc = locinfo.getLocationByUserID(nUserID);

            if (loc != null)
            {
                if (loc.getDungeonUserID() == 0x40000000)
                {
                    MessageBox.Show("ランダムダンジョンの中にはワープできません");
                    return;
                }

                this.mLocation = loc;

                DialogResult = DialogResult.OK;
            }
            else
            {

                DialogResult = DialogResult.Cancel;
            }
            Close();
        }

    }
}
