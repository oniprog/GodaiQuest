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
    public partial class FormUseExperience : Form
    {
        private GQCommandMaster mGQCom;
        private int[] mCost = new int[3];
        private int mDungeonNumber;

        public FormUseExperience(GQCommandMaster gqcom, int nDungeonNumber)
        {
            InitializeComponent();

            this.mGQCom = gqcom;
            this.mDungeonNumber = nDungeonNumber;
        }

        private void FormUseExperience_Load(object sender, EventArgs e)
        {
            DungeonInfo dungeon;
            this.mGQCom.getDungeon(out dungeon, this.mGQCom.getUserID(), this.mDungeonNumber);

            int nDungeonDetph = this.mGQCom.getDungeonDepth(this.mGQCom.getUserID());

            this.mCost[0] = (dungeon.getSizeY()) * 10 * (this.mDungeonNumber + 1);
            this.mCost[1] = (dungeon.getSizeX()) * 10 * (this.mDungeonNumber + 1);
            this.mCost[2] = (nDungeonDetph + 1) * 10 * 11 * 11;

            this.labExpandX.Text = "必要経験値：" + ("" + this.mCost[0]);
            this.labExpandY.Text = "必要経験値：" + ("" + this.mCost[1]);
            this.labCreateNewFloor.Text = "必要経験値：" + ("" + this.mCost[2]);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

            if (this.radExpandX.Checked)
            {
                if (MessageBox.Show("実行してよろしいですか？", "Notice", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
                if ( !this.mGQCom.useExperience(EUseExp.ExpandX, this.mDungeonNumber) ) {
                    MessageBox.Show(this.mGQCom.getErrorReasonString());
                }
                else {
                    MessageBox.Show("成功しました");
                }
            }
            else if (this.radExpandY.Checked)
            {
                if (MessageBox.Show("実行してよろしいですか？", "Notice", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
                if ( !this.mGQCom.useExperience(EUseExp.ExpandY, this.mDungeonNumber) ) {
                    MessageBox.Show(this.mGQCom.getErrorReasonString());
                }
                else {
                    MessageBox.Show("成功しました");
                }
            }
            else if (this.radMakeNewDungeon.Checked ) {

                if (MessageBox.Show("実行してよろしいですか？", "Notice", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;
                if ( !this.mGQCom.useExperience(EUseExp.CreateNewFloor, this.mDungeonNumber) ) {
                    MessageBox.Show(this.mGQCom.getErrorReasonString());
                }
                else {
                    MessageBox.Show("成功しました");
                }
            }

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
