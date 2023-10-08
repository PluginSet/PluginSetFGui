using System.IO;
using UnityEngine;
using FairyGUI;
using PluginSet.Core;
using UnityEngine.Networking;
using Logger = PluginSet.Core.Logger;

namespace PluginSet.FGUI
{
    public class GCustomLoader: GLoader
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("GCustomLoader");

        private static string ImageCachePath;
        
        public static bool EnableCacheImage = true;

#if UNITY_EDITOR_OSX
        private const string FILE_URL_PREFIX = "file://";
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
        private const string FILE_URL_PREFIX = "file:///";
#else
        private const string FILE_URL_PREFIX = "file://";
#endif

        private bool IsValid()
        {
            if (_disposed)
                return false;

            if (!displayObject.cachedTransform)
                return false;
            
            return true;
        }

        static GCustomLoader()
        {
            ImageCachePath = Path.Combine(Application.persistentDataPath, "caches", "images");
        }

        private string _requestUrl = null;

        private GComponent _boardFrame;
        
        public GCustomLoader()
        {
            onAddedToStage.Add(__onAddToStage);
            onRemovedFromStage.Add(__onRemoveFromStage);
        }
        
        private void __onAddToStage()
        {
            CheckReload();
            this.AddOnBranchChange(CheckReload);
        }

        private void __onRemoveFromStage()
        {
            this.RemoveOnBranchChange(CheckReload);
        }

        public GComponent AddBoardFrame()
        {
            if (_boardFrame != null)
                return _boardFrame;
            
            _boardFrame = new GComponent();
            _boardFrame.SetSize(width, height);
            _boardFrame.AddRelation(this, RelationType.Size);
            _boardFrame.opaque = true;
            ((Container) displayObject).AddChildAt(_boardFrame.displayObject, 0);
            return _boardFrame;
        }

        private void CheckReload()
        {
            if (_contentItem == null)
                return;

            if (string.IsNullOrEmpty(_url))
                return;
            
            var item = UIPackage.GetItemByURL(_url);
            if (item?.getBranch() == _contentItem)
                return;
            
            LoadContent();
        }

        protected override void LoadExternal()
        {
            if (_url.StartsWith("https://")
                || _url.StartsWith("http://")
                || _url.StartsWith("file://")
                || _url.StartsWith("ftp://"))
            {
                DownloadTexture();
            }
            else
            {
                _requestUrl = null;
                Texture2D tex = ResourcesManager.Instance.Load<Texture2D>(_url);
                if (tex != null)
                    onExternalLoadSuccess(new NTexture(tex));
                else
                    onExternalLoadFailed();
            }
        }

        protected override void FreeExternal(NTexture texture)
        {
            _requestUrl = null;
        }

        private void DownloadTexture()
        {
            string downloadUrl = _url;
            bool readable = true;
            if (EnableCacheImage && !downloadUrl.StartsWith(FILE_URL_PREFIX))
            {
                var path = GetTextureCachePath();
                if (File.Exists(path))
                {
                    readable = false;
                    downloadUrl = FILE_URL_PREFIX + path;
                }
            }
            else
            {
                readable = false;
            }

            _requestUrl = downloadUrl;
            onExternalLoadSuccess(NTexture.Empty);
            var downloader = new DownloadHandlerTexture(readable);
            var request = new UnityWebRequest(downloadUrl, "GET", downloader, null);
            var async = request.SendWebRequest();
            async.completed += OnTextureResponse;
        }

        private void OnTextureResponse(AsyncOperation asyncOperation)
        {
            var async = (UnityWebRequestAsyncOperation) asyncOperation;
            if (async == null)
                return;

            async.completed -= OnTextureResponse;
            var request = async.webRequest;
            if (!request.url.Equals(_requestUrl))
                return;
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.Warn("Fail download texture with url " + _requestUrl);
                if (IsValid())
                    onExternalLoadFailed();
                return;
            }

            var downloader = (DownloadHandlerTexture) request.downloadHandler;
            var downloadTexture = downloader.texture;
            if (downloadTexture == null)
            {
                Logger.Warn("Fail parse texture with url " + _requestUrl);
                if (IsValid())
                    onExternalLoadFailed();
                return;
            }
                
            
            if (IsValid())
                onExternalLoadSuccess(new NTexture(downloadTexture));
            
            if (downloadTexture.isReadable)
            {
                var fileName = GetTextureCachePath();
                if (File.Exists(fileName))
                    return;
                    
                var filePath = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(filePath) && !Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                
                File.WriteAllBytes(fileName, downloadTexture.EncodeToPNG());
            }
        }

        private string GetTextureCachePath()
        {
            return Path.Combine(ImageCachePath, _url.GetHashCode() + ".png");
        }

        public override void Dispose()
        {
            if (_boardFrame != null)
            {
                ((Container) displayObject).RemoveChild(_boardFrame.displayObject, false);
                _boardFrame.Dispose();
            }
            _boardFrame = null;
            base.Dispose();
        }
    }
}