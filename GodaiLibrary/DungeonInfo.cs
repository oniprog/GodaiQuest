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

        public DungeonInfo() { }
        public DungeonInfo(godaiquest.DungeonInfo info)
        {
            mDungeon = Network.ConvertByteToUlong(info.dungeon);
            mSizeX = info.size_x;
            mSizeY = info.size_y;
            mDungeonNumber = info.dungeon_number;
        }
        public godaiquest.DungeonInfo getSerialize()
        {
            var ret = new godaiquest.DungeonInfo();
            ret.dungeon = Network.ConvertUlongToByte(mDungeon);
            ret.size_x = mSizeX;
            ret.size_y = mSizeY;
            ret.dungeon_number = mDungeonNumber;
            return ret;
        }

        public void setInit(int nDugeonNumber, ulong[] dungeon, int nSizeX, int nSizeY)
        {
            mDungeonNumber = nDugeonNumber;
            mSizeX = nSizeX;
            mSizeY = nSizeY;
            mDungeon = dungeon;
        }

        public ulong[] getDungeon()
        {
            return mDungeon;
        }
        public int getSizeX()
        {
            return mSizeX;
        }
        public int getSizeY()
        {
            return mSizeY;
        }
        public int getDungeonNumber()
        {
            return mDungeonNumber;
        }
        private int index(int ix, int iy)
        {
            return mSizeX * iy + ix;
        }
        public uint getDungeonImageAt(int ix, int iy)
        {
            return (uint)(mDungeon[index(ix,iy)] >> 32);
        }
        public void setDungeonImageAt(int ix, int iy, uint nImage)
        {
            ulong nNum = mDungeon[index(ix,iy)] & 0xffffffff;
            mDungeon[index(ix, iy)] = nNum | ((ulong)nImage << 32);
        }
        public uint getDungeonContentAt(int ix, int iy)
        {
            return (uint)(mDungeon[index(ix, iy)]) & 0xffffffff;
        }
        
        public void setDungeonContentAt(int ix, int iy, uint nContent)
        {
            ulong nNum = mDungeon[index(ix, iy)] & 0xffffffff00000000;
            mDungeon[index(ix, iy)] = nNum | nContent;
        }
        public ulong getDungeonTileAt(int ix, int iy)
        {
            return mDungeon[index(ix, iy)];
        }
        public void setDungeonTileAt(int ix, int iy, ulong nTileID)
        {
            mDungeon[index(ix, iy)] = nTileID;
        }

        public void resize(int nNewX, int nNewY)
        {
            ulong[] newdungeon = new ulong[nNewX * nNewY];
            for (int iy = 0; iy < mSizeY; ++iy)
            {
                if ( iy >= nNewY )
                    continue;

                for (int ix = 0; ix < mSizeX; ++ix)
                {
                    if ( ix >= nNewX )
                        continue;

                    newdungeon[ix + iy * nNewX] = mDungeon[index(ix, iy)];
                }
            }

            mSizeX = nNewX;
            mSizeY = nNewY;
            mDungeon = newdungeon;
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
            mNumber = nNumber_;
            mCanItemImage = bCanItemImage;
            mName = name_;
            mImage = image_;
            mNewImage = bNewImage;
            mOwner = nOwner;
            mCreated = dateCreate;
        }
        public ImagePair(godaiquest.ImagePair imagepair)
        {
            mNumber = imagepair.number;
            mCanItemImage = imagepair.can_item_image;
            mName = imagepair.name;
            mImage = Network.ByteArrayToImage(imagepair.image);
            mNewImage = imagepair.new_image;
            mOwner = imagepair.owner;
            mCreated = DateTime.FromBinary(imagepair.created);
        }

        public godaiquest.ImagePair getSerialize()
        {
            var ret = new godaiquest.ImagePair();
            ret.number = mNumber;
            ret.can_item_image = mCanItemImage;
            ret.name = mName;
            ret.image = Network.ImageToByteArray(mImage);
            ret.new_image = mNewImage;
            ret.owner = mOwner;
            ret.created = mCreated.ToBinary();
            return ret;
        }

        public void setNumber(int nNumber)
        {
            mNumber = nNumber;
        }
        public bool canItemImage()
        {
            return mCanItemImage;
        }
        public int getNumber()
        {
            return mNumber;
        }
        public void setImage(Image image)
        {
            mImage = image;
        }
        public Image getImage()
        {
            return mImage;
        }
        public bool isNewImage()
        {
            return mNewImage;
        }
        public void setNewImage(bool bNew)
        {
            mNewImage = bNew;
        }
        public String getName()
        {
            return mName;
        }
        public void setName(String name_)
        {
            mName = name_;
        }
        public DateTime getCreateTime()
        {
            return mCreated;
        }
        public void setCreateTime(DateTime time_)
        {
            mCreated = time_;
        }
        public int getOwner()
        {
            return mOwner;
        }
        public void setOwner(int nOwner)
        {
            mOwner = nOwner;
        }
    }

    [Serializable()]
    public class DungeonBlockImageInfo : IEnumerable<ImagePair>
    {
        private uint mMaxImageNum;
        private Dictionary<uint, ImagePair> mImageDic = new Dictionary<uint, ImagePair>();

        public DungeonBlockImageInfo() { }

        public DungeonBlockImageInfo(godaiquest.DungeonBlockImageInfo info)
        {
            mMaxImageNum = (uint)info.max_image_num;
            foreach (var pair0 in info.image_dic) 
			{
				ImagePair newpair = new ImagePair( pair0.imagepair );
                mImageDic.Add(pair0.index, newpair);
			}
        }

        public godaiquest.DungeonBlockImageInfo getSerialize()
        {
            var ret = new godaiquest.DungeonBlockImageInfo();
            ret.max_image_num = mMaxImageNum;
			foreach (var dic in mImageDic )
			{
                var imagepairdic = new godaiquest.ImagePairDic();
                imagepairdic.index = dic.Key;
                imagepairdic.imagepair = dic.Value.getSerialize();
                ret.image_dic.Add(imagepairdic);
			}
            return ret;
        }

        // 
        // bNewは新規作成したデータに対してtrueとすること
        public void addImage( uint nNum, bool canItemImage_, Image image, String name_, int nOwner_, DateTime dateCreate_, bool bNew ) {
            mImageDic.Add((uint)nNum, new ImagePair((int)nNum, canItemImage_, image, name_, nOwner_, dateCreate_, bNew));
            if (mMaxImageNum < nNum) mMaxImageNum = nNum;
        }

        public IEnumerator<ImagePair> GetEnumerator()
        {
            return mImageDic.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ImagePair getImagePairAt(uint nIndex)
        {
            ImagePair imagepair;
            mImageDic.TryGetValue(nIndex, out imagepair);
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
            mImageDic.TryGetValue(nIndex, out imagepair);
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
            if (mImageDic.Count == 0)
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
            mUserID = nUserID;
        }
        public TilePalette(godaiquest.TilePalette tile_palette)
        {
            mUserID = tile_palette.user_id;
			foreach (var tile_id in tile_palette.image_set )
			{
                mImageSet.Add(tile_id.tile_id);
            }
        }
        public godaiquest.TilePalette getSerialize()
        {
            var ret = new godaiquest.TilePalette();
			foreach (var tile_id in mImageSet)
			{
                var new_tile_id = new godaiquest.TilePaletteImageSet();
                new_tile_id.tile_id = tile_id;
                ret.image_set.Add(new_tile_id);
			}
            return ret;
        }

        public TilePalette copy()
        {
            TilePalette ret = new TilePalette(mUserID);
            foreach (var nTileID in mImageSet) 
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
            return mImageSet.GetEnumerator();
        }

        public int getUesrID()
        {
            return mUserID;
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
            return mImageSet.Contains(nImageID);
        }
    }
}
