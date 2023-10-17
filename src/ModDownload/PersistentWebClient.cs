using System.Net;
using System;
using System.ComponentModel;

// class that can repeatedly download a file when it fails to download the first time
public class PersistentWebClient : WebClient
{
    public event Action<bool> OnPersistentDownloadFileComplete;
    public event Action<bool, string> OnPersistentDownloadStringComplete;

    private bool _busy;
    private readonly int _maxRetries;
    private int _currentRetries;

    private DownloadData _currentDownloadData;

    private enum DownloadType
    {
        String,
        File
    }

    private class DownloadData
    {
        public readonly DownloadType type;
        public readonly string link;
        public readonly string path;

        public DownloadData(DownloadType type, string link, string path = null)
        {
            this.type = type;
            this.link = link;
            this.path = path;
        }
    }

    protected override WebRequest GetWebRequest(Uri uri)
    {
        WebRequest w = base.GetWebRequest(uri);
        w.Timeout = 20 * 60 * 1000;
        return w;
    }

    public PersistentWebClient(int maxRetries = 3) : base()
    {
        _maxRetries = maxRetries;
        Proxy = null;
        DownloadFileCompleted += DownloadCompletedForFile;
        DownloadStringCompleted += DownloadCompletedForString;
    }

    public void DownloadFilePersistent(string link, string path)
    {
        if (_busy)
        {
            return;
        }

        _currentRetries = 0;
        _busy = true;

        _currentDownloadData = new DownloadData(DownloadType.File, link, path);
        DownloadFileTaskAsync(new Uri(link), path);
    }

    public void DownloadStringPersistent(string link)
    {
        if (_busy)
        {
            return;
        }

        _currentRetries = 0;
        _busy = true;

        _currentDownloadData = new DownloadData(DownloadType.String, link);
        DownloadStringAsync(new Uri(link));
    }

    private void Redownload()
    {
        switch (_currentDownloadData.type)
        {
            case DownloadType.File:
                DownloadFileTaskAsync(new Uri(_currentDownloadData.link), _currentDownloadData.path);
                break;
            case DownloadType.String:
                DownloadStringAsync(new Uri(_currentDownloadData.link));
                break;
        }
    }

    private void DownloadCompletedFailed()
    {

        _currentRetries++;
        if (_currentRetries > _maxRetries)
        {
            _busy = false;
            switch (_currentDownloadData.type)
            {
                case DownloadType.File:
                    OnPersistentDownloadFileComplete(false);
                    break;
                case DownloadType.String:
                    OnPersistentDownloadStringComplete(false, null);
                    break;
            }
        }
        else
        {
            Redownload();
        }

    }

    private void DownloadCompletedForFile(object sender, AsyncCompletedEventArgs eventArgs)
    {
        if (eventArgs.Error == null)
        {
            OnPersistentDownloadFileComplete(true);
        }
        else
        {
            DownloadCompletedFailed();
        }
    }

    private void DownloadCompletedForString(object sender, DownloadStringCompletedEventArgs eventArgs)
    {
        if (eventArgs.Error == null)
        {
            OnPersistentDownloadStringComplete(true, eventArgs.Result);
        }
        else
        {
            DownloadCompletedFailed();
        }
    }
}