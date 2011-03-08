using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Reflection;

namespace Verde.Utility
{
    public class ExternalCacheDatabase
    {
        private string strCachePath;
        private Dictionary<string, Queue<Action<string>>> dicTasks = null;

        public ExternalCacheDatabase()
        {
            this.dicTasks = new Dictionary<string, Queue<Action<string>>>();

            this.strCachePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + "cache";
            if (Directory.Exists(this.strCachePath) == false) {
                Directory.CreateDirectory(this.strCachePath);
            }
        }

        public void GetCache(string strUrl, Action<string> action)
        {
            this.GetCacheMethod(strFilename => strFilename)(strUrl, action);
        }

        public void GetImageCache(string strUrl, Action<BitmapImage> action)
        {
            Func<string, BitmapImage> func = (strFilename) => {
                try {
                    return new BitmapImage(new Uri(strFilename));
                } catch (NotSupportedException) {
                    MessageBox.Show(strFilename + " is not an image file or not supported format.");
                    return null;
                }
            };
            this.GetCacheMethod(func)(strUrl, action);
        }

        public void DeleteCache(string strUrl)
        {
            // Make hash code for the URL
            string strHashHex = StringProcessing.ToHexString(new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(strUrl)));

            // Make Filename to save the image
            string strFilename = Path.Combine(this.strCachePath, strHashHex);
            lock (this.dicTasks) {
                if (this.dicTasks.ContainsKey(strHashHex)) {
                    MessageBox.Show("Can not delete the cache while in downloading.");
                    return;
                }
            }

            if (File.Exists(strFilename)) {
                try {
                    File.Delete(strFilename);
                } catch (IOException) {
                    MessageBox.Show("Can not delete the cache file \"" + strFilename  + "\" while in using.");
                } catch (Exception ex)                 {
                    MessageBox.Show("Unknown error was occurred.: " + Environment.NewLine + ex.Message);
                }
            } else {
                MessageBox.Show("The file \"" + strFilename + "\" does not exist.");
            }

        }

        private Action<string, Action<T>> GetCacheMethod<T>(Func<string, T> funcGetCacheFilename)
        {
            return (strUrl, actGetCacheFilename) => {
                string strHashHex = StringProcessing.ToHexString(new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(strUrl)));

                string strFilename = Path.Combine(this.strCachePath, strHashHex);

                lock (this.dicTasks) {
                    Queue<Action<string>> queTask = null;
                    if (this.dicTasks.ContainsKey(strHashHex)) {
                        queTask = this.dicTasks[strHashHex];
                    }
                    if (queTask != null) {
                        lock (queTask) {
                            queTask.Enqueue(fileName2 => actGetCacheFilename(funcGetCacheFilename(fileName2)));
                        }
                        return;
                    }

                    if (queTask == null && !File.Exists(strFilename)) {
                        queTask = new Queue<Action<string>>();
                        queTask.Enqueue(strCacheFilename => actGetCacheFilename(funcGetCacheFilename(strCacheFilename)));
                        this.dicTasks.Add(strHashHex, queTask);
                        this.CreateFileDownloader(strUrl, strFilename, strHashHex).StartDownload();
                        return;
                    }
                }

                actGetCacheFilename(funcGetCacheFilename(strFilename));
            };
        }

        private WebFileDownloader CreateFileDownloader(string strUrl, string strFilename, string strHashHex)
        {
            //int key = StatusProgressService.StartProgress();

            WebFileDownloader dlerWebFile = new WebFileDownloader(strUrl, strFilename);
            dlerWebFile.DownloadProgressChanged = (e) => {
                //StatusProgressService.ChangeProgress(key, e.ProgressPercentage);
            };

            dlerWebFile.DownloadFileCompleted = (e) => {
                //StatusProgressService.CompleteProgress(key);

                Queue<Action<string>> queTask = null;
                lock (this.dicTasks) {
                    if (this.dicTasks.ContainsKey(strHashHex)) {
                        queTask = this.dicTasks[strHashHex];
                    } else {
                        return;
                    }
                }

                Action<string> actGetDownloadFilename = null;
                while (true) {
                    lock (queTask) {
                        if (queTask.Count > 0) {
                            actGetDownloadFilename = queTask.Dequeue();
                        } else {
                            actGetDownloadFilename = null;
                        }
                    }

                    if (actGetDownloadFilename != null) {
                        actGetDownloadFilename(strFilename);
                    } else {
                        lock (this.dicTasks) {
                            lock (queTask) {
                                if (queTask.Count == 0) {
                                    this.dicTasks.Remove(strHashHex);
                                    break;
                                }
                            }
                        }
                    }
                }
            };

            dlerWebFile.DownloadFileFailed = (e) => {
                MessageBox.Show(e.Error.Message);
            };
            return dlerWebFile;
        }
    }

    public class WebFileDownloader
    {
        private WebClient webClient = null;
        private string strUrl = null;           // URL to download
        private string strSaveFilename = null;  // Filename to save

        public WebFileDownloader(string _url, string _saveFile)
        {
            this.strUrl = _url;
            this.strSaveFilename = _saveFile;
        }

        private Action<DownloadProgressChangedEventArgs> actDownloadProgressChanged = null;
        public Action<DownloadProgressChangedEventArgs> DownloadProgressChanged
        {
            get { return this.actDownloadProgressChanged; }
            set { this.actDownloadProgressChanged = value; }
        }

        private Action<AsyncCompletedEventArgs> actDownloadFileCompleted = null;
        public Action<AsyncCompletedEventArgs> DownloadFileCompleted
        {
            get { return this.actDownloadFileCompleted; }
            set { this.actDownloadFileCompleted = value; }
        }

        private Action<AsyncCompletedEventArgs> actDownloadFileFailed = null;
        public Action<AsyncCompletedEventArgs> DownloadFileFailed
        {
            get { return this.actDownloadFileFailed; }
            set { this.actDownloadFileFailed = value; }
        }

        // Start to downlaod as async
        public void StartDownload()
        {
            if (webClient == null) {
                webClient = new WebClient();
                webClient.DownloadProgressChanged += (sender, e) => {
                    if (this.actDownloadProgressChanged != null) {
                        this.actDownloadProgressChanged(e);
                    }
                };

                webClient.DownloadFileCompleted += (sender, e) => {
                    if (e.Error != null) {
                        if (this.actDownloadFileFailed != null) {
                            this.actDownloadFileFailed(e);
                    }
                        return;
                    }
                    if (this.actDownloadFileCompleted != null) {
                        this.actDownloadFileCompleted(e);
                    }
                };
            }
            webClient.DownloadFileAsync(new Uri(this.strUrl), this.strSaveFilename);
        }

        public void CancelDownload()
        {
            if (webClient != null) {
                webClient.CancelAsync();
            }
        }
    }
}
