using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows.Media.Imaging;

namespace Verde
{
    //外部キャッシュ管理
    public class ExternalCacheDatabase
    {
        private Dictionary<string, Queue<Action<string>>> tasks = null;

        public ExternalCacheDatabase()
        {
            tasks = new Dictionary<string, Queue<Action<string>>>();
        }

        public void GetCache(string url, Action<string> action)
        {
            GetCacheMethod(fileName => fileName)(url, action);
        }

        public void GetImageCache(string imageUrl, Action<BitmapImage> action)
        {
            Func<string, BitmapImage> func = (fileName) => {
                try {
                    return new BitmapImage(new Uri(fileName));
                } catch (NotSupportedException) {
                    MessageBox.Show("画像ファイルでないか対応していない画像ファイルです。");
                    return null;
                }
            };
            GetCacheMethod(func)(imageUrl, action);
        }

        //キャッシュを削除する
        public void DeleteCache(string url)
        {
            //URLのハッシュ
            string hexString = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(url)).ToHexString();

            //ファイル名
            string fileName = Path.Combine(Core.ExternalsCacheFolder, hexString);
            lock (tasks) {
                if (tasks.ContainsKey(hexString)) {
                    MessageBox.Show("ファイルのダウンロード中は削除できません。");
                    return;
                }
            }

            if (File.Exists(fileName)) {
                try {
                    File.Delete(fileName);
                } catch (IOException) {
                    MessageBox.Show("ファイルの使用中は削除できません。");
                } catch (Exception ex)                 {
                    MessageBox.Show("不明なエラーが発生しました：" + Environment.NewLine + ex.CreateMessage(0));
                }
            } else {
                MessageBox.Show("ファイルが存在しません。");
            }

        }

        private Action<string, Action<T>> GetCacheMethod<T>(Func<string, T> func)
        {
            return (url, action) => {
                string hexString = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(url)).ToHexString();

                string fileName = Path.Combine(Core.ExternalsCacheFolder, hexString);

                lock (tasks) {
                    Queue<Action<string>> task = null;
                    if (tasks.ContainsKey(hexString)) {
                        task = tasks[hexString];
                    }
                    if (task != null) {
                        lock (task) {
                            task.Enqueue(fileName2 => action(func(fileName2)));
                        }
                        return;
                    }

                    if (task == null && !File.Exists(fileName)) {
                        task = new Queue<Action<string>>();
                        task.Enqueue(fileName2 => action(func(fileName2)));
                        tasks.Add(hexString, task);
                        CreateFileDownloader(url, fileName, hexString).StartDownload();
                        return;
                    }
                }
                action(func(fileName));
            };
        }

        private WebFileDownloader CreateFileDownloader(string url, string fileName, string hexString)
        {
            int key = StatusProgressService.StartProgress();

            WebFileDownloader webFileDownloader = new WebFileDownloader(url, fileName);
            webFileDownloader.DownloadProgressChanged = (e) => {
                StatusProgressService.ChangeProgress(key, e.ProgressPercentage);
            };

            webFileDownloader.DownloadFileCompleted = (e) => {
                StatusProgressService.CompleteProgress(key);

                Queue<Action<string>> task = null;
                lock (tasks) {
                    if (tasks.ContainsKey(hexString)) {
                        task = tasks[hexString];
                    } else {
                        return;
                    }
                }

                Action<string> action = null;
                while (true) {
                    lock (task) {
                        if (task.Count > 0) {
                            action = task.Dequeue();
                        } else {
                            action = null;
                        }
                    }

                    if (action != null) {
                        action(fileName);
                    } else {
                        lock (tasks) {
                            lock (task) {
                                if (task.Count == 0) {
                                    tasks.Remove(hexString);
                                    break;
                                }
                            }
                        }
                    }
                }
            };

            webFileDownloader.DownloadFileFailed = (e) => {
                MessageBox.Show(e.Error.Message);
            };
            return webFileDownloader;
        }
    }

    //Webファイルダウンローダ
    //Web上のファイルをダウンロードする
    public class WebFileDownloader
    {
        //コンストラクタ（URL／保存先）
        public WebFileDownloader(string _url, string _saveFile)
        {
            url = _url;
            saveFile = _saveFile;
        }

        //Webクライアント
        private WebClient webClient = null;

        //ダウンロードするファイルを指し示すURL
        private string url = null;

        //保存先
        private string saveFile = null;

        //進捗状況が変化した時に実行する処理
        private Action<DownloadProgressChangedEventArgs> downloadProgressChanged = null;
        public Action<DownloadProgressChangedEventArgs> DownloadProgressChanged
        {
            get { return downloadProgressChanged; }
            set { downloadProgressChanged = value; }
        }

        //ファイルのダウンロードが完了した時に実行する処理
        private Action<AsyncCompletedEventArgs> downloadFileCompleted = null;
        public Action<AsyncCompletedEventArgs> DownloadFileCompleted
        {
            get { return downloadFileCompleted; }
            set { downloadFileCompleted = value; }
        }

        //ファイルのダウンロードが失敗した時に実行する処理
        private Action<AsyncCompletedEventArgs> downloadFileFailed = null;
        public Action<AsyncCompletedEventArgs> DownloadFileFailed
        {
            get { return downloadFileFailed; }
            set { downloadFileFailed = value; }
        }

        //非同期ダウンロードを開始する
        public void StartDownload()
        {
            //WebClientを作成
            if (webClient == null) {
                webClient = new WebClient();
                webClient.DownloadProgressChanged += (sender, e) => {
                    if (downloadProgressChanged != null) {
                        downloadProgressChanged(e);
                    }
                };

                webClient.DownloadFileCompleted += (sender, e) => {
                    if (e.Error != null) {
                        if (downloadFileFailed != null)
                            downloadFileFailed(e);
                        return;
                    }
                    if (downloadFileCompleted != null) {
                        downloadFileCompleted(e);
                    }
                };
            }
            webClient.DownloadFileAsync(new Uri(url), saveFile);
        }

        //非同期ダウンロードをキャンセルする
        public void CancelDownload()
        {
            if (webClient != null) {
                webClient.CancelAsync();
            }
        }
    }
}
