using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows.Media.Imaging;

namespace Verde
{
    //�O���L���b�V���Ǘ�
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
                    MessageBox.Show("�摜�t�@�C���łȂ����Ή����Ă��Ȃ��摜�t�@�C���ł��B");
                    return null;
                }
            };
            GetCacheMethod(func)(imageUrl, action);
        }

        //�L���b�V�����폜����
        public void DeleteCache(string url)
        {
            //URL�̃n�b�V��
            string hexString = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(url)).ToHexString();

            //�t�@�C����
            string fileName = Path.Combine(Core.ExternalsCacheFolder, hexString);
            lock (tasks) {
                if (tasks.ContainsKey(hexString)) {
                    MessageBox.Show("�t�@�C���̃_�E�����[�h���͍폜�ł��܂���B");
                    return;
                }
            }

            if (File.Exists(fileName)) {
                try {
                    File.Delete(fileName);
                } catch (IOException) {
                    MessageBox.Show("�t�@�C���̎g�p���͍폜�ł��܂���B");
                } catch (Exception ex)                 {
                    MessageBox.Show("�s���ȃG���[���������܂����F" + Environment.NewLine + ex.CreateMessage(0));
                }
            } else {
                MessageBox.Show("�t�@�C�������݂��܂���B");
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

    //Web�t�@�C���_�E�����[�_
    //Web��̃t�@�C�����_�E�����[�h����
    public class WebFileDownloader
    {
        //�R���X�g���N�^�iURL�^�ۑ���j
        public WebFileDownloader(string _url, string _saveFile)
        {
            url = _url;
            saveFile = _saveFile;
        }

        //Web�N���C�A���g
        private WebClient webClient = null;

        //�_�E�����[�h����t�@�C�����w������URL
        private string url = null;

        //�ۑ���
        private string saveFile = null;

        //�i���󋵂��ω��������Ɏ��s���鏈��
        private Action<DownloadProgressChangedEventArgs> downloadProgressChanged = null;
        public Action<DownloadProgressChangedEventArgs> DownloadProgressChanged
        {
            get { return downloadProgressChanged; }
            set { downloadProgressChanged = value; }
        }

        //�t�@�C���̃_�E�����[�h�������������Ɏ��s���鏈��
        private Action<AsyncCompletedEventArgs> downloadFileCompleted = null;
        public Action<AsyncCompletedEventArgs> DownloadFileCompleted
        {
            get { return downloadFileCompleted; }
            set { downloadFileCompleted = value; }
        }

        //�t�@�C���̃_�E�����[�h�����s�������Ɏ��s���鏈��
        private Action<AsyncCompletedEventArgs> downloadFileFailed = null;
        public Action<AsyncCompletedEventArgs> DownloadFileFailed
        {
            get { return downloadFileFailed; }
            set { downloadFileFailed = value; }
        }

        //�񓯊��_�E�����[�h���J�n����
        public void StartDownload()
        {
            //WebClient���쐬
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

        //�񓯊��_�E�����[�h���L�����Z������
        public void CancelDownload()
        {
            if (webClient != null) {
                webClient.CancelAsync();
            }
        }
    }
}
