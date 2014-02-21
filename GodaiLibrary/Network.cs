using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace GodaiLibrary
{
    /*
     * network転送用ラッパークラス
     */
    public class Network
    {
        private NetworkStream mStream;
        private bool mClosed = false;
        private ImageConverter mConv = new ImageConverter();

        public Network(NetworkStream ns)
        {
            this.mStream = ns;
        }

        public int receiveByte()
        {
            int nByte = this.mStream.ReadByte();
            if (nByte < 0)
            {
                this.mClosed = true;
            }
            return nByte;
        }

        public bool isClosed()
        {
            return this.mClosed;
        }

        public void sendByte(byte data)
        {
            this.mStream.WriteByte(data);
        }

        public int receiveWORD()
        {
            int nb1 = receiveByte();
            int nb2 = receiveByte();
            if (isClosed())
                return 0;
            else
                return (nb1 << 8) + nb2;
        }

        public void sendWORD(int nData)
        {
            this.mStream.WriteByte((byte)(nData >> 8));
            this.mStream.WriteByte((byte)(nData & 0xff));
        }

        public int receiveDWORD()
        {
            int nb1 = receiveByte();
            int nb2 = receiveByte();
            int nb3 = receiveByte();
            int nb4 = receiveByte();
            if (isClosed())
                return 0;
            else
                return (nb1 << 24) + (nb2 << 16) + (nb3 << 8) + nb4;
        }

        public void sendDWORD(int nData)
        {
            this.mStream.WriteByte((byte)(nData >> 24));
            this.mStream.WriteByte((byte)(nData >> 16));
            this.mStream.WriteByte((byte)(nData >> 8));
            this.mStream.WriteByte((byte)(nData & 0xff));
        }

        public double receiveDouble()
        {
            
            long nb1 = receiveByte();
            long nb2 = receiveByte();
            long nb3 = receiveByte();
            long nb4 = receiveByte();
            int nb5 = receiveByte();
            int nb6 = receiveByte();
            int nb7 = receiveByte();
            int nb8 = receiveByte();

            if (isClosed())
                return 0;
            else {
                long value = (nb1 << 56) + (nb2 << 48) + (nb3 << 40) + (nb4 << 32) + (nb5 << 24) + (nb6 << 16) + (nb7 << 8) + nb8;
                return BitConverter.Int64BitsToDouble(value);
            }
        }

        public void sendDouble(double dData)
        {
            long nVal = BitConverter.DoubleToInt64Bits(dData);
            sendDWORD((int)(nVal >> 32));
            sendDWORD((int)(nVal & 0xffff));
        }

        public long receiveLength()
        {
            int nByte = receiveByte();
            if ((nByte & 0xf0) == 0)
            {
                return nByte & 0x0f;
            }
            int nLen = nByte >> 4;
            long nRet = nByte & 0x0f;
            for (int it = 0; it < nLen; ++it)
            {
                nRet = (nRet << 8) + receiveByte();
            }
            return nRet;
        }

        public void sendString(string str)
        {
            byte [] byteArray = System.Text.Encoding.Unicode.GetBytes(str);
            sendLength(byteArray.Length);
            this.mStream.Write(byteArray, 0, byteArray.Length);
        }

        public String receiveString()
        {
            long nLen = receiveLength();
            if (nLen == 0)
                return "";
            byte[] byteArray = new byte[nLen];

            int nPos = 0;
            while (nLen > 0)
            {
                int nReadByte = this.mStream.Read(byteArray, nPos, (int)nLen);
                if (nReadByte <= 0)
                    break;
                nPos += nReadByte;
                nLen -= nReadByte;
            }
            return System.Text.Encoding.Unicode.GetString(byteArray);
        }

        public void sendLength(long nLength)
        {
            if (nLength < 0x10)
            {
                sendByte((byte)nLength);
            }
            else if (nLength < 0xfff)
            {
                sendByte((byte)((nLength >> 8) | 0x10));
                sendByte((byte)(nLength & 0xff));
            }
            else if (nLength < 0xfffff)
            {
                sendByte((byte)((nLength >> 16) | 0x20));
                sendByte((byte)((nLength >> 8) & 0xff));
                sendByte((byte)(nLength & 0xff));
            }
            else if (nLength < 0xfffffff)
            {
                sendByte((byte)((nLength >> 24) | 0x30));
                sendByte((byte)((nLength >> 16) & 0xff));
                sendByte((byte)((nLength >> 8) & 0xff));
                sendByte((byte)((nLength) & 0xff));
            }
            else
                throw new ArgumentException("Too large data. It cannot transfer");
        }

        public Image receiveImage()
        {
            byte[] byteImage = receiveBinary();
            if (byteImage == null)
                return null;
            return (Image)mConv.ConvertFrom(byteImage);
        }

        public byte[] receiveBinary()
        {
            int nLen = (int) this.receiveLength();
            if (nLen == 0)
                return null;
            byte[] ret = new byte[nLen];
            int size = nLen;
            int offset = 0;
            while (size > 0)
            {
                int nReadLen = this.mStream.Read(ret, offset, size);
                if (nReadLen < 0)
                    return null;
                size -= nReadLen;
                offset += nReadLen;
            }
            return ret;
        }

        public void sendImage(Image image)
        {
            byte[] byteImage = (byte[])mConv.ConvertTo(image, typeof(byte[]));
            this.sendBinary(byteImage);
        }

        public void sendBinary(byte[] data)
        {
            this.sendLength(data.Length);
            this.mStream.Write(data, 0, data.Length);
        }


        public void flush()
        {
            this.mStream.Flush();
        }

        public void disconnect()
        {
            this.mStream.Close();
        }

        /// ローカルのファイル一覧を送信する
        public static void sendLocalFilesInfo(Network network, String strDirectory)
        {
            string strBaseDir = new DirectoryInfo(strDirectory).FullName;
			sendLocalFilesInfo(network, strBaseDir, strBaseDir);
			network.sendDWORD(-1);
        }
        private static void sendLocalFilesInfo(Network network, String strDirectory, string strBaseDir) {

			DirectoryInfo info = new DirectoryInfo(strDirectory);
            foreach (var file in info.GetFiles())
            {
                int nSize = (int)file.Length; // 注：巨大すぎるファイルは考えない
                //string strName = file.FullName.Substring(strBaseDir.Length + 1);
                network.sendDWORD(nSize);
				network.sendString(file.Name);
            }
            foreach (var dir in info.GetDirectories())
            {
				sendLocalFilesInfo(network, dir.FullName, strBaseDir);               
            }			
        }

		// ローカルのファイル情報
        public struct LocalFileInfo
        {
            public int Size;
            public string FilePath;
        }

		// ローカルファイルの一覧を受信する
        public static List<LocalFileInfo> receiveLocalFilesInfo(Network network, String strDirectory)
        {
            var listRet = new List<LocalFileInfo>();
            while (true)
            {
                int nFileSize = network.receiveDWORD();
                if (nFileSize < 0)
                    break;
                string strFileName = network.receiveString();

                var localFileInfo = new LocalFileInfo();
                localFileInfo.FilePath = Path.Combine(strDirectory, strFileName);
                localFileInfo.Size = nFileSize;
				listRet.Add(localFileInfo);
            }
            return listRet;
        }

        /// ファイルを受信する
        public static void receiveFiles(Network network, String strDirectory)
        {
            Directory.CreateDirectory(strDirectory);

            while (true)
            {
                byte nFlag = (byte)network.receiveByte();
                if (nFlag == 0)
                    break;

                String strFilePath = network.receiveString();
                strFilePath = Path.Combine(strDirectory, strFilePath);
                Directory.CreateDirectory(Path.GetDirectoryName(strFilePath));

                byte[] data = network.receiveBinary();
                if (data != null)
                {
                    System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(new MemoryStream(data), System.IO.Compression.CompressionMode.Decompress);
                    System.IO.FileStream wout = new System.IO.FileStream(strFilePath, FileMode.Create);
                    byte[] buffer = new byte[1024 * 1024];
                    while (true)
                    {
                        int nReadSize = gzip.Read(buffer, 0, buffer.Length);
                        if (nReadSize == 0)
                            break;
                        wout.Write(buffer, 0, nReadSize);
                    }
                    gzip.Close();
                    wout.Close();
                }
            }
        }

        /// ファイルリストを送信する
        public void sendFileList(String strDirectoryOrFile)
        {
            try
            {
                if (File.Exists(strDirectoryOrFile))
                    this.sendFileInfo(Path.GetFileName(strDirectoryOrFile), Path.GetFullPath(strDirectoryOrFile), "");
                else
                    this.sendFileListSub(strDirectoryOrFile, "");
            }
            catch (Exception)
            {
                this.sendByte(0);
            }
        }

        /// ファイルリストを送信する
        public void sendFileListSub(String strDirectory, String strSubPath)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(strDirectory);
            foreach (var file in dirinfo.GetFiles())
            {
                this.sendFileInfo(file.Name, file.FullName, strSubPath);
            }
            foreach (var dir in dirinfo.GetDirectories())
            {
                this.sendFileListSub(dir.FullName, Path.Combine(strSubPath, dir.Name));
            }
        }

        ///  ファイルの情報を送る
        public void sendFileInfo(String strFileName, String strFullPath, String strSubPath)
        {
            if (!File.Exists(strFullPath))
                return;

            FileInfo info = new FileInfo(strFullPath);
            this.sendByte(1);

            // ファイル名を送る
            String strTransferPath = Path.Combine(strSubPath, strFileName);
            this.sendString(strTransferPath);

            // ファイル長さ
            this.sendLength(info.Length);

            // ファイル更新日付送信
            var time = info.LastAccessTime;
            MemoryStream memory = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memory, time);
            this.sendBinary(memory.ToArray());
        }


        /// ファイルを送信する
        public static void sendFiles(Network network, String strDirectoryOrFile)
        {
			sendFiles( network, strDirectoryOrFile, null);
        }

        /// ファイルを送信する
        public static void sendFiles(Network network, String strDirectoryOrFile, List<LocalFileInfo> listFile ) 
        {
            try
            {
                if (File.Exists(strDirectoryOrFile))
                {
                    sendFile(network, Path.GetFileName(strDirectoryOrFile), Path.GetFullPath(strDirectoryOrFile), "", listFile);
                }
                else
                {
                    sendFilesSub(network, strDirectoryOrFile, "",listFile);
                }
            }
            finally
            {
                network.sendByte(0);
            }
        }

        private static void sendFilesSub(Network network, String strDirectory, String strSubPath, List<LocalFileInfo> listFile)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(strDirectory);
            if (!dirinfo.Exists)
                return;

            foreach (var file in dirinfo.GetFiles())
            {
                sendFile(network, file.Name, file.FullName, strSubPath, listFile);
            }
            foreach (var dir in dirinfo.GetDirectories())
            {
                sendFilesSub(network, dir.FullName, Path.Combine(strSubPath, dir.Name), listFile);
            }
        }

        // ファイル転送 strSubPathは付加するフォルダ名
        public static void sendFile(Network network, String strFileName, String strFullPath, String strSubPath, List<LocalFileInfo> listFile)
        {
            if (listFile != null)
            {
                for (int it = 0; it < listFile.Count; ++it)
                {
                    if (listFile[it].FilePath == strFullPath)
                    {
                        int nRealSize = (int)new FileInfo(strFullPath).Length;
                        if (nRealSize == listFile[it].Size)
                            return;
                    }
                }
            }
            try
            {
                System.IO.FileStream win = new System.IO.FileStream(strFullPath, FileMode.Open, FileAccess.Read);

                network.sendByte(1);
                String strTransferPath = Path.Combine(strSubPath, strFileName);
                network.sendString(strTransferPath);

                MemoryStream memory = new MemoryStream();
                System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(memory, System.IO.Compression.CompressionMode.Compress);
                byte[] buffer = new byte[1024 * 1024];
                while (true)
                {
                    int nReadSize = win.Read(buffer, 0, buffer.Length);
                    if (nReadSize == 0)
                        break;
                    gzip.Write(buffer, 0, nReadSize);
                }
                gzip.Close();

                byte[] data = memory.ToArray();
                network.sendBinary(data);

                memory.Close();
                win.Close();
            }
            catch (Exception ex)
            {
                GodaiLibrary.MessageBox2.Show(ex.Message);
            }
        }

		// シリアライズする
        public void Serialize<T>(T obj)
        {
            //ProtoBuf.Serializer.Serialize( this.mStream, obj);
            ProtoBuf.Serializer.SerializeWithLengthPrefix( this.mStream, obj, ProtoBuf.PrefixStyle.Fixed32, 0);
        }

		// デシリアライズする
        public T Deserialize<T>()
        {
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(this.mStream, ProtoBuf.PrefixStyle.Fixed32, 0 );
            //return ProtoBuf.Serializer.Deserialize<T>(this.mStream);
        }

        // http://www.atmarkit.co.jp/fdotnet/dotnettips/603byteimage/byteimage.html
        // バイト配列をImageオブジェクトに変換
        public static Image ByteArrayToImage(byte[] b)
        {
            if (b.Length == 0)
                return null;

            ImageConverter imgconv = new ImageConverter();
            Image img = (Image)imgconv.ConvertFrom(b);
#if false
            MemoryStream ms = new MemoryStream(b);
            Image img = Bitmap.FromStream(ms);
            ms.Close();
#endif
            return img;
        }

        // Imageオブジェクトをバイト配列に変換
        public static byte[] ImageToByteArray(Image img)
        {
            if (img == null)
                return new byte[0];

            ImageConverter imgconv = new ImageConverter();
            byte[] b = (byte[])imgconv.ConvertTo(img, typeof(byte[]));
#if false
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] ret = ms.ToArray();
            ms.Close();
#endif
            return b;
        }

        public static byte[] ConvertUlongToByte(ulong[] data)
        {
            byte[] ret = new byte[data.Length * 8];
            for (int it = 0; it < data.Length; ++it)
            {
                var tmp = BitConverter.GetBytes(data[it]);
                ret[it * 8 + 0] = tmp[0];
                ret[it * 8 + 1] = tmp[1];
                ret[it * 8 + 2] = tmp[2];
                ret[it * 8 + 3] = tmp[3];
                ret[it * 8 + 4] = tmp[4];
                ret[it * 8 + 5] = tmp[5];
                ret[it * 8 + 6] = tmp[6];
                ret[it * 8 + 7] = tmp[7];
            }
            return ret;
        }
        public static ulong[] ConvertByteToUlong(byte[] data)
        {
            ulong[] ret = new ulong[data.Length/8];
            for (int it = 0; it < data.Length; it += 8)
            {
                ret[it / 8] = BitConverter.ToUInt64(data, it);
            }
            return ret;
        }
    }
}
