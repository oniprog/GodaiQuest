using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GodaiLibrary.GodaiQuest
{
    [Serializable()]
    public class DungeonInfo
    {
        private ulong[] mDungeon;
        private int mSizeX;
        private int mSizeY;
        private int mDungeonNumber;

        public void setInit(int nDugeonNumber, ulong[] dungeon, int nSizeX, int nSizeY)
        {
            this.mDungeonNumber = nDugeonNumber;
            this.mSizeX = nSizeX;
            this.mSizeY = nSizeY;
            this.mDungeon = dungeon;
        }

        public ulong[] getDungeon()
        {
            return this.mDungeon;
        }
        public int getSizeX()
        {
            return this.mSizeX;
        }
        public int getSizeY()
        {
            return this.mSizeY;
        }
        public int getDungeonNumber()
        {
            return this.mDungeonNumber;
        }
        private int index(int ix, int iy)
        {
            return this.mSizeX * iy + ix;
        }
        public uint getDungeonImageAt(int ix, int iy)
        {
            return (uint)(this.mDungeon[index(ix,iy)] >> 32);
        }
        public void setDungeonImageAt(int ix, int iy, uint nImage)
        {
            ulong nNum = this.mDungeon[index(ix,iy)] & 0xffffffff;
            this.mDungeon[index(ix, iy)] = nNum | ((ulong)nImage << 32);
        }
        public uint getDungeonContentAt(int ix, int iy)
        {
            return (uint)(this.mDungeon[index(ix, iy)]) & 0xffffffff;
        }
        
        public void setDungeonContentAt(int ix, int iy, uint nContent)
        {
            ulong nNum = this.mDungeon[index(ix, iy)] & 0xffffffff00000000;
            this.mDungeon[index(ix, iy)] = nNum | nContent;
        }
        public ulong getDungeonTileAt(int ix, int iy)
        {
            return this.mDungeon[index(ix, iy)];
        }
        public void setDungeonTileAt(int ix, int iy, ulong nTileID)
        {
            this.mDungeon[index(ix, iy)] = nTileID;
        }

        public void resize(int nNewX, int nNewY)
        {
            ulong[] newdungeon = new ulong[nNewX * nNewY];
            for (int iy = 0; iy < this.mSizeY; ++iy)
            {
                if ( iy >= nNewY )
                    continue;

                for (int ix = 0; ix < this.mSizeX; ++ix)
                {
                    if ( ix >= nNewX )
                        continue;

                    newdungeon[ix + iy * nNewX] = this.mDungeon[index(ix, iy)];
                }
            }

            this.mSizeX = nNewX;
            this.mSizeY = nNewY;
            this.mDungeon = newdungeon;
        }
    }

    [Serializable()]
    public class ImagePair
    {
        private int mNumber;
        private Image mImage;
        private String mName;
        private int mOwner;
        private DateTime mCreated;
        private bool mCanItemImage;  // アイテム画像足りうるか？
        private bool mNewImage;

        public ImagePair() { }
        public ImagePair(int nNumber_, bool bCanItemImage, Image image_, String name_, int nOwner, DateTime dateCreate, bool bNewImage)
        {
            this.mNumber = nNumber_;
            this.mCanItemImage = bCanItemImage;
            this.mName = name_;
            this.mImage = image_;
            this.mNewImage = bNewImage;
            this.mOwner = nOwner;
            this.mCreated = dateCreate;
        }

        public void setNumber(int nNumber)
        {
            this.mNumber = nNumber;
        }
        public bool canItemImage()
        {
            return this.mCanItemImage;
        }
        public int getNumber()
        {
            return this.mNumber;
        }
        public void setImage(Image image)
        {
            this.mImage = image;
        }
        public Image getImage()
        {
            return this.mImage;
        }
        public bool isNewImage()
        {
            return this.mNewImage;
        }
        public void setNewImage(bool bNew)
        {
            this.mNewImage = bNew;
        }
        public String getName()
        {
            return this.mName;
        }
        public void setName(String name_)
        {
            this.mName = name_;
        }
        public DateTime getCreateTime()
        {
            return this.mCreated;
        }
        public void setCreateTime(DateTime time_)
        {
            this.mCreated = time_;
        }
        public int getOwner()
        {
            return this.mOwner;
        }
        public void setOwner(int nOwner)
        {
            this.mOwner = nOwner;
        }
    }

    [Serializable()]
    public class DungeonBlockImageInfo : IEnumerable<ImagePair>
    {
        private uint mMaxImageNum;
        private Dictionary<uint, ImagePair> mImageDic = new Dictionary<uint, ImagePair>();

        // 
        // bNewは新規作成したデータに対してtrueとすること
        public void addImage( uint nNum, bool canItemImage_, Image image, String name_, int nOwner_, DateTime dateCreate_, bool bNew ) {
            this.mImageDic.Add((uint)nNum, new ImagePair((int)nNum, canItemImage_, image, name_, nOwner_, dateCreate_, bNew));
            if (this.mMaxImageNum < nNum) this.mMaxImageNum = nNum;
        }

        public IEnumerator<ImagePair> GetEnumerator()
        {
            return this.mImageDic.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ImagePair getImagePairAt(uint nIndex)
        {
            ImagePair imagepair;
            this.mImageDic.TryGetValue(nIndex, out imagepair);
            if (imagepair != null)
            {
                return imagepair;
            }
            else
            {
                return null;
            }

        }

        public Image getImageAt(uint nIndex)
        {
            ImagePair imagepair;
            this.mImageDic.TryGetValue(nIndex, out imagepair);
            if (imagepair != null)
            {
                return imagepair.getImage();
            }
            else
            {
                return null;
            }
        }

        ///  未使用の番号を返す
        public uint newImageNum()
        {
            if (this.mImageDic.Count == 0)
                return 0;
            else
                return ++mMaxImageNum;
        }
    }

    /// ブロックイメージのパレット
    [Serializable()]
    public class TilePalette : IEnumerable<ulong>
    {
        private int mUserID;
        private HashSet<ulong> mImageSet = new HashSet<ulong>();

        public TilePalette(int nUserID)
        {
            this.mUserID = nUserID;
        }

        public TilePalette copy()
        {
            TilePalette ret = new TilePalette(this.mUserID);
            foreach (var nTileID in this.mImageSet) 
            {
                ret.addTileID(nTileID);
            }
            return ret;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ulong> GetEnumerator()
        {
            return this.mImageSet.GetEnumerator();
        }

        public int getUesrID()
        {
            return this.mUserID;
        }

        public void addTileID(ulong nNum)
        {
            mImageSet.Add(nNum);
        }

        public void removeTileID(ulong nNum)
        {
            mImageSet.Remove(nNum);
        }

        public bool containsTileID(ulong nImageID)
        {
            return this.mImageSet.Contains(nImageID);
        }
    }
}
