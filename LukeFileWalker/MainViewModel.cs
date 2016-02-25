using MVVMBaseLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LukeFileWalker
{
    public class MainViewModel:ViewModel
    {
        CancellationTokenSource ctsrc; 

        #region properties
        public string CurrentDir
        {
            get { return m_CurrentDir; }
            set
            {
                if (value != m_CurrentDir)
                {
                    m_CurrentDir = value;
                    OnPropertyChanged(GetPropertyName(() => this.CurrentDir));
                }
            }
        }string m_CurrentDir = "";

        public string CurrentFile
        {
            get { return m_CurrentFile; }
            set
            {
                if (value != m_CurrentFile)
                {
                    m_CurrentFile = value;
                    OnPropertyChanged(GetPropertyName(() => this.CurrentFile));
                }
            }
        }string m_CurrentFile = "";

        public long DirectoryCount
        {
            get { return m_DirectoryCount; }
            set {
                if (m_DirectoryCount != value)
                {
                    m_DirectoryCount = value;
                    OnPropertyChanged(GetPropertyName(() => this.DirectoryCount));
                }
            }
        }long m_DirectoryCount = default(long);


        public long FileCount
        {
            get { return m_FileCount; }
            set
            {
                if (m_FileCount != value)
                {
                    m_FileCount = value;
                    OnPropertyChanged(GetPropertyName(() => this.FileCount));
                }
            }
        }long m_FileCount = default(long);

        public bool IsWalking
        {
            get { return m_IsWalking; }
            set
            {
                if (m_IsWalking != value)
                {
                    m_IsWalking = value;
                    OnPropertyChanged(GetPropertyName(() => this.IsWalking));
                }
            }
        }bool m_IsWalking = default(bool);
        #endregion

        #region commands
        public RelayCommand Go { get; set; }
        public RelayCommand Stop { get; set; }

        #endregion

        StreamWriter sw;

        public MainViewModel()
        {
            Go = new RelayCommand(p=>Start(),p=>!IsWalking);

            Stop = new RelayCommand(p => Cancel());
        }

        private void Cancel()
        {
            ctsrc.Cancel();
        }

        public void Start()
        {
            ctsrc = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                IsWalking = true;
                var luke = new FileWalker();
                StartFileWalking(ctsrc.Token);
            }, ctsrc.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        void StartFileWalking(CancellationToken cancellationToken)
        {
            var storePath = Path.Combine(Environment.CurrentDirectory,"LukesJournal.csv");
            if(sw !=null)
                sw.Dispose();
            sw = new StreamWriter(storePath,true);
            var drives = System.Environment.GetLogicalDrives();//get drives

            try
            {
                foreach (var drive in drives)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        sw.Close();
                        sw = null;
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    var driveInfo = new DriveInfo(drive);
                    if (!driveInfo.IsReady)
                        continue;
                    walk(cancellationToken, driveInfo.RootDirectory);
                }
            }
            catch (OperationCanceledException)
            {
                IsWalking = false;
                return;
            }
            
        }

        internal void walk(CancellationToken cancellationToken, DirectoryInfo rootDir)
        {
            FileInfo[] FilesInCurrDir = null;
            DirectoryInfo[] SubDirsInCurrDir = null;
            CurrentDir = rootDir.Name;
            DirectoryCount++;
            try
            {
                FilesInCurrDir = rootDir.GetFiles();//first get file in current directory
                if (FilesInCurrDir != null)
                {
                    //log these files
                    foreach (var file in FilesInCurrDir)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            sw.Close();
                            sw = null;
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                        try
                        {
                            sw.WriteLine(string.Format("{0},{1},{2},{3}", file.Name, file.Length, file.CreationTime, file.LastWriteTime));
                            CurrentFile = Path.GetFileName(file.Name);
                            FileCount++;
                        }
                        catch (Exception iex)
                        {
                            //here just so others aren't skipped if theres a problem with one file
                            Debug.WriteLine(iex.Message);
                        }
                    }
                }
                SubDirsInCurrDir = rootDir.GetDirectories();
            }
            catch (IOException io)
            {
                Debug.WriteLine(io.Message);
            }
            catch (UnauthorizedAccessException uae)
            {
                Debug.WriteLine(uae.Message);
            }
            
            //recurse sub directories 
            if (SubDirsInCurrDir != null)
            {
                foreach (var subdir in SubDirsInCurrDir)
                {
                    walk(cancellationToken, subdir);
                }
            }
            
        }
    }
}
