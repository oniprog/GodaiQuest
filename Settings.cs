using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GodaiQuest { 

    [Serializable()]
    public class Settings
    {
        //設定を保存するフィールド
        public string Mail { get; set;}
        public string Password { get; set; }
        public bool Memory { get; set; }
        public String DownloadFolder { get;  set; }

        //コンストラクタ
        public Settings()
        {
            Memory = true;
        }

        //Settingsクラスのただ一つのインスタンス
        [NonSerialized()]
        private static Settings _instance;

        [System.Xml.Serialization.XmlIgnore]
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Settings();
                return _instance;
            }
            set {_instance = value;}
        }

        /// <summary>
        /// 設定をXMLファイルから読み込み復元する
        /// </summary>
        public static void LoadFromXmlFile()
        {
            string path = GetSettingPath();

            FileStream fs = new FileStream(path,
                FileMode.Open,
                FileAccess.Read);
            System.Xml.Serialization.XmlSerializer xs =
                new System.Xml.Serialization.XmlSerializer(
                    typeof(Settings));
            //読み込んで逆シリアル化する
            object obj = xs.Deserialize(fs);
            fs.Close();

            Instance = (Settings) obj;
        }

        /// <summary>
        /// 現在の設定をXMLファイルに保存する
        /// </summary>
        public static void SaveToXmlFile()
        {
            string path = GetSettingPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            FileStream fs = new FileStream(path,
                FileMode.Create,
                FileAccess.Write);
            System.Xml.Serialization.XmlSerializer xs =
                new System.Xml.Serialization.XmlSerializer(
                typeof(Settings));
            //シリアル化して書き込む
            xs.Serialize(fs, Instance);
            fs.Close();
        }

        /// <summary>
        /// 設定をバイナリファイルから読み込み復元する
        /// </summary>
        public static void LoadFromBinaryFile()
        {
            string path = GetSettingPath();

            FileStream fs = new FileStream(path,
                FileMode.Open,
                FileAccess.Read);
            BinaryFormatter bf = new BinaryFormatter();
            //読み込んで逆シリアル化する
            object obj = bf.Deserialize(fs);
            fs.Close();

            Instance = (Settings) obj;
        }

        /// <summary>
        /// 現在の設定をバイナリファイルに保存する
        /// </summary>
        public static void SaveToBinaryFile()
        {
            string path = GetSettingPath();

            FileStream fs = new FileStream(path,
                FileMode.Create,
                FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            //シリアル化して書き込む
            bf.Serialize(fs, Instance);
            fs.Close();
        }

        /// <summary>
        /// 設定をレジストリから読み込み復元する
        /// </summary>
        public static void LoadFromRegistry()
        {
            BinaryFormatter bf = new BinaryFormatter();
            //レジストリから読み込む
            RegistryKey reg = GetSettingRegistry();
            byte[] bs = (byte[]) reg.GetValue("");
            //逆シリアル化して復元
            MemoryStream ms = new MemoryStream(bs, false);
            Instance = (Settings) bf.Deserialize(ms);
            //閉じる
            ms.Close();
            reg.Close();
        }

        /// <summary>
        /// 現在の設定をレジストリに保存する
        /// </summary>
        public static void SaveToRegistry()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            //シリアル化してMemoryStreamに書き込む
            bf.Serialize(ms, Instance);
            //レジストリへ保存する
            RegistryKey reg = GetSettingRegistry();
            reg.SetValue("", ms.ToArray());
            //閉じる
            ms.Close();
            reg.Close();
        }

        private static string GetSettingPath()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                Application.CompanyName + "\\" + Application.ProductName +
                "\\" + Application.ProductName + ".config");
            return path;
        }

        private static RegistryKey GetSettingRegistry()
        {
            RegistryKey reg = Registry.CurrentUser.CreateSubKey(
                "Software\\" + Application.CompanyName +
                "\\" + Application.ProductName);
            return reg;

        }
    }
}