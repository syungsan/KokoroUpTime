using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace FileIOUtils
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

        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            //コピー先のディレクトリがないときは作る
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);

                //属性もコピー
                File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                destDirName = destDirName + Path.DirectorySeparatorChar;

            //コピー元のディレクトリにあるファイルをコピー
            string[] files = Directory.GetFiles(sourceDirName);

            foreach (string file in files)
                File.Copy(file, destDirName + Path.GetFileName(file), true);

            //コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
            string[] dirs = Directory.GetDirectories(sourceDirName);

            foreach (string dir in dirs)
                CopyDirectory(dir, destDirName + Path.GetFileName(dir));
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
