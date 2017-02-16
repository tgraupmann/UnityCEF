using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Xilium.CefGlue.Demo.Browser;

namespace Xilium.CefGlue.Demo
{
    public class CEFPlugin : MonoBehaviour
    {
        public string _mGoogleApiKey;
        public string _mGoogleDefaultClientId;
        public string _mGoogleDefaultClientSecret;

        private bool _mDoUpdates = false;

        private WebBrowser _mCore = null;

        // Use this for initialization
        void Start()
        {
            if (!string.IsNullOrEmpty(_mGoogleApiKey))
            {
                System.Environment.SetEnvironmentVariable("GOOGLE_API_KEY", _mGoogleApiKey);
            }
            if (!string.IsNullOrEmpty(_mGoogleDefaultClientId))
            {
                System.Environment.SetEnvironmentVariable("GOOGLE_DEFAULT_CLIENT_ID", _mGoogleDefaultClientId);
            }
            if (!string.IsNullOrEmpty(_mGoogleDefaultClientSecret))
            {
                System.Environment.SetEnvironmentVariable("GOOGLE_DEFAULT_CLIENT_SECRET", _mGoogleDefaultClientSecret);
            }

            string[] args = new string[] {
                    "--enable-speech-input",
                };

            Load(args);
        }

        class DemoApp : CefApp
        {
            protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
            {
                Debug.Log("OnBeforeCommandLineProcessing:");
            }
        }

        private void Load(string[] args)
        {
            try
            {
                CefRuntime.Load();
            }
            catch (DllNotFoundException ex)
            {
                Debug.LogException(ex);
                return;
            }
            catch (CefRuntimeException ex)
            {
                Debug.LogException(ex);
                return;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }

            var mainArgs = new CefMainArgs(args);
            var app = new DemoApp();

            var exitCode = CefRuntime.ExecuteProcess(mainArgs, app, IntPtr.Zero);
            if (exitCode != -1)
            {
                Debug.LogErrorFormat("ExitCode={0}", exitCode);
                return;
            }

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            Debug.LogFormat("codeBase: {0}", codeBase);

            string localFolder = Path.GetDirectoryName(new Uri(codeBase).LocalPath);
            Debug.LogFormat("localFolder: {0}", localFolder);

            string browserProcessPath = CombinePaths(localFolder, "..", "..", "..",
                "CefGlue.Demo.WinForms", "bin", "Release", "Xilium.CefGlue.Demo.WinForms.exe");
            Debug.LogFormat("browserProcessPath: {0}", browserProcessPath);

            CefSettings settings = new CefSettings
            {
                BrowserSubprocessPath = browserProcessPath,
                SingleProcess = false,
                MultiThreadedMessageLoop = true,
                LogSeverity = CefLogSeverity.Disable,
                LogFile = "CefGlue.log",
            };

            CefRuntime.Initialize(mainArgs, settings, app, IntPtr.Zero);

            if (!settings.MultiThreadedMessageLoop)
            {
                _mDoUpdates = true;
            }

            string url = "https://www.youtube.com/watch?v=qjb1oZk3mNM";

            _mCore = new WebBrowser(this, new CefBrowserSettings(), url);
            _mCore.Created += new EventHandler(BrowserCreated);
            _mCore.StartUrl = url;

            var windowInfo = CefWindowInfo.Create();
            windowInfo.Name = url;
            _mCore.Create(windowInfo);

            Debug.Log("Load: Done");
        }

        private void Update()
        {
            if (_mDoUpdates)
            {
                CefRuntime.DoMessageLoopWork();
            }
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit:");
            CefRuntime.Shutdown();
        }

        private void BrowserCreated(object sender, EventArgs e)
        {
            Debug.Log("BrowserCreated:");
            Debug.Log(_mCore.CefBrowser.GetHost().GetWindowHandle());
        }

        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(Path.Combine);
        }
    }
}
