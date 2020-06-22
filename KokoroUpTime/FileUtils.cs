using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace KokoroUpTime
{
    /// <summary>
    /// Directory クラスに関する汎用関数を管理するクラス
    /// </summary>
    public static class DirectoryUtils
    {
        /// <summary>
        /// 指定したパスにディレクトリが存在しない場合
        /// すべてのディレクトリとサブディレクトリを作成します
        /// </summary>
        public static DirectoryInfo SafeCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
        }
    }

    // ファイル操作に関するユーティリティクラス
    public static class FileUtils
    {
        // 実行プログラムのルートディレクトリの絶対パスを返す
        public static string GetStartupPath()
        {
            string exePath = Environment.GetCommandLineArgs()[0];
            string exeFullPath = Path.GetFullPath(exePath);
            string startupPath = Path.GetDirectoryName(exeFullPath);

            return startupPath;
        }
    }
}
