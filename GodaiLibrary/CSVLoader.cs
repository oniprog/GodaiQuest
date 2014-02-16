using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.IO;

namespace GodaiLibrary 
{
	/**
     * CSV読み込み
     */ 
    public class CSVLoader : IDisposable
    {
        private StreamReader _sr;
        private string _strLine;
        private int _index;

		// ファイルを開く
        public bool OpenFile(string strPath, Encoding enc )
        {
            try
            {
                _sr = new StreamReader(strPath, enc);
                return true;
            }
            catch
            {
                return false;
            }
        }

		// コメント行かの判定
        public bool IsCommentLine()
        {
            if (_strLine == null) return false;

            int nIndex = 0;
            for (; nIndex < _strLine.Length; ++nIndex)
            {
                if (_strLine[nIndex] != ' ' && _strLine[nIndex] != '\t')
                    break;
            }
            if (nIndex == _strLine.Length)
                return true; // 空行

            if (nIndex + 1 < _strLine.Length && _strLine[nIndex + 0] == '/' && _strLine[nIndex + 1] == '/')
            {
                return true;
            }
            if (nIndex < _strLine.Length && _strLine[nIndex] == '#')
            {
                return true;
            }
            return false;
        }

		// 行を読む
        public bool ReadLine()
        {
            _index = 0;
            while (true)
            {
                _strLine = _sr.ReadLine();
                if (_strLine == null)
                    return false;
                if (!IsCommentLine())
                    break;
            }
            return true;
        }

		// 文字列を読み込む
        public string GetString()
        {
            if (_strLine == null)
                return "";

            bool bInString = false;

			var sb = new StringBuilder();
            for (; _index < _strLine.Length; ++_index)
            {
                char ch = _strLine[_index];
                if (ch == '"')
                {
                    bInString = !bInString;
                }
                else
                {
                    if (!bInString)
                    {
                        if (ch == ',' || ch == '\0')
                        {
                            ++_index;
                            break;
                        }
                        continue;
                    }
                    if ( ch == '\\')
                    {
                        if (bInString)
                        {
                            ++_index;
                            if (_index >= _strLine.Length)
                            {
                                sb.Append(ch);
                                break;
                            }
                            char ch2 = _strLine[_index];
                            ch = ch2;
                        }
                    }
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        public string GetPlainString()
        {
            var sb = new StringBuilder();
            for (; _index < _strLine.Length; ++_index)
            {
                if (_strLine[_index] == ',' || _strLine[_index] == '\0')
                {
                    ++_index;
                    break;
                }
                sb.Append(_strLine[_index]);
            }
            return sb.ToString();
        }
		// 数値を得る
        public double GetDouble()
        {
            string strValue = GetPlainString();
            double dRet = 0.0;
            Double.TryParse(strValue, out dRet);
            return dRet;
        }

		// 数値を得る
        public int GetInt()
        {
            string strValue = GetPlainString();
            int nRet = 0;
            int.TryParse(strValue, out nRet);
            return nRet;
        }
		// ファイル閉じる
        public void CloseFile()
        {
            if (_sr == null)
                return;
			_sr.Close();
            _sr = null;
        }

        public void Dispose()
        {
            CloseFile();
        }
    }
}
