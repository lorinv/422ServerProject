using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS422
{    
    //******** Abstract Base Classes ************************//

    public abstract class Dir422
    {
        public abstract string Name { get; }
        public abstract IList<Dir422> GetDirs();
        public abstract IList<File422> GetFiles();
        public abstract Dir422 Parent { get; }
        public abstract bool ContainsFile(string fileName, bool recursive);
        public abstract bool ContainsDir(string dirName, bool recursive);
        public abstract Dir422 GetDir(string name);
        public abstract File422 GetFile(string name);
        public abstract File422 CreateFile(string name);
        public abstract Dir422 CreateDir(string name);
    }

    public abstract class File422
    {
        public abstract string Name { get; }
        public abstract Dir422 Parent { get; }
        public abstract Stream OpenReadOnly(); //Should not support writing
        public abstract Stream OpenReadWrite();
    }

    public abstract class FileSys422
    {
        public abstract Dir422 GetRoot();

        public virtual bool Contains(File422 file)
        {
            return Contains(file.Parent);
        }

        public virtual bool Contains(Dir422 dir)
        {
            if (dir == null) { return false; }

            if (dir.Name == GetRoot().Name) { return true; }
            return Contains(dir.Parent);
        }
    }

    //************** Standard FS Stuff *****************//
        
        public class StdFSDir : Dir422
        {
            private string m_path;
            private string _name;
            private Dir422 _parent;

            public StdFSDir(string path, Dir422 parent)
            {
                if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
                if (path.EndsWith("\\")) path = path.Substring(0, path.Length - 1);
                m_path = path;
                _parent = parent;
                string[] parts = path.Split('/', '\\');
                _name = parts.Last();                
            }

            public override string Name
            {
                get
                {
                    return _name;
                }
            }

            public override Dir422 Parent
            {
                get
                {
                    //Is only null if root
                    return _parent;
                }
            }

            public override bool ContainsDir(string dirName, bool recursive)
            {
                //chapter 2 - 4.2
                //Same as Contains file, but for DIR.
                if (dirName.Contains("/") || dirName.Contains("\\"))
                    return false;

            IList<Dir422> dirs = GetDirs();
            foreach (var dir in dirs)
            {
                if (dir.Name.EndsWith(dirName)) return true;
            }

            if (recursive)
                {
                    foreach (var d in GetDirs())
                    {
                        if (d.ContainsDir(dirName, recursive))
                            return true;
                    }
                }

                return false;                                
            }

            public override bool ContainsFile(string fileName, bool recursive)
            {
                //return false if path characters are found.
                //Searches for file within the current DIR, recurse if specified.
                //chapter 2 - 4.2
                //Same as Contains file, but for DIR.
                if (fileName.Contains("/") || fileName.Contains("\\") || fileName == "")
                    return false;

                IList<File422> files = GetFiles();                
                foreach (var file in files)
                {
                    if (file.Name.EndsWith(fileName)) return true;
                }
                

                if (recursive)
                {
                    foreach (var d in GetDirs())
                    {
                        if (d.ContainsFile(fileName, recursive))
                            return true;
                    }
                }

                return false;
            }

            public override Dir422 CreateDir(string name)
            {
                //Check for any invalid characters in the file name ('/','\')
                //If filename has these characters, is null, or empty then return null
                //If a directory with the same name already exists, just return that directory.
                //If it does not exist, create it and return it.                    
                if (name.Contains("/") || name.Contains("\\"))
                    return null;

                if (ContainsDir(name, false)) return GetDir(name);
                else
                {
                    try
                    {
                        Directory.CreateDirectory(m_path + "/" + name);
                        StdFSDir newDir = new StdFSDir(m_path + "/" + name, this);
                        return newDir;
                    }
                    catch { return null; }                    
                }                
            }

            public override File422 CreateFile(string name)
            {
                // Check for invalid characters, same as CreateDir
                // If not, create the file within the DIR and set size to 0
                // If it already exists, truncate it back to size 0, ereasing all content
                if (name.Contains("/") || name.Contains("\\"))
                    return null;
                
                try
                {
                    Stream file = File.Create(m_path + "/" + name);
                    file.Close();
                    StdFSFile newFile = new StdFSFile(m_path + "/" + name, this);
                    return newFile;
                }
                catch { return null; }
                

        }

            public override Dir422 GetDir(string name)
            {
            //Non recursive Search
            //Return null if name contains path characters (/\)
            //Return null if no directory with the given name is found.
            //Returns non-null Dir422 object if found         
            if (name.Contains("/") || name.Contains("\\"))
                return null;

            if (ContainsDir(name, false))
            {
                StdFSDir d = new StdFSDir(m_path + "\\" + name, this);
                return d;
            }
            else { return null; }
            
        }

            public override IList<Dir422> GetDirs()
            {
                //Not recursive
                //Returns a list of all DIRs inside this one.
                //Same as GetDirs
                List<Dir422> dirs = new List<Dir422>();
                foreach (string dir in Directory.GetDirectories(m_path))
                {
                    if (Directory.Exists(dir)) dirs.Add(new StdFSDir(dir, this));
                }
                return dirs;            
            }

            public override File422 GetFile(string name)
            {
                if (name.Contains("/") || name.Contains("\\") || name == "")
                    return null;

                if (ContainsFile(name, false))
                {
                    StdFSFile file = new StdFSFile(m_path + "/" + name, this);
                    return file;
                }
                else { return null; }                        
            }

            public override IList<File422> GetFiles()
            {
                //Same as GetDirs
                List<File422> files = new List<File422>();
                foreach (string file in Directory.GetFiles(m_path))
                {
                    files.Add(new StdFSFile(file, this));
                }
                return files;
            }            
        }

        //*****************************//

        public class StdFSFile : File422
        {
            private string m_path;
            private string _name;
            private Dir422 _parent;
            

            public StdFSFile(string path, StdFSDir parent)
            {
                m_path = path;
                _parent = parent;
                _name = path.Split('/', '\\').Last();
            }

        public override string Name
            {
                get
                {
                    return _name;
                }
            }

            public override Dir422 Parent
            {
                get
                {
                    return _parent;                    
                }
            }

            public override Stream OpenReadOnly()
            {
            //Opens for readonly and returns the stream.
            //Return null if failed. (Note comments below)
            try { FileStream fs = new FileStream(this.m_path, FileMode.Open, FileAccess.Read); return fs; }
            catch { return null; }            
            }            

            public override Stream OpenReadWrite()
            {
            //Opens the file for reading and writing and returns the read/write stream
            //returns null if failed (Note comments below)
            
            try { FileStream fs = new FileStream(this.m_path, FileMode.Open, FileAccess.ReadWrite); return fs; }
            catch { return null; }
        }
            //Cannot Read if anyone else is writing
            //As many people can write if no one else is writing
            //Can't write while others are reading.
            //Only one can write at a time.
    }    

    /*
     * In class questions:
     *  For memoryFS: count how many threads are reading and writing and check those before?
     *  Use an event? Or a check?
     *  
     */

    public class StandardFileSystem : FileSys422
    {
        private static Dir422 _root;
        
        private StandardFileSystem(string rootDir)
        {
            _root = new StdFSDir(rootDir, null);
        }

        public static StandardFileSystem Create(string rootDir)
        {
            //Create the filesystem using existing folder on disk.
            //Root must have null parent
            //Return Null if failed.            
            try { return new StandardFileSystem(rootDir); }
            catch { return null; }
        }

        public override Dir422 GetRoot() 
        {
            //Gets the root directory for this filesystem
            return _root;
        }
    }

    //********** Mem File System *****************//

    //Must be thread safe!!!!!
    public class MemFSFile : File422
    {
        private string _name;
        private Dir422 _parent;
        private int readAccessCounter = 0;
        private int writeAccessCounter = 0;
        private byte[] _data = new byte[10000]; //TODO: Fix this;
        public event PropertyChangedEventHandler PropertyChanged;
            

        public MemFSFile(string name, MemFSDir parent)
        {
            _name = name;
            _parent = parent;            
        }

        private void OnPropertyChanged(string name)
        {

        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override Dir422 Parent
        {
            get
            {
                return _parent;
            }
        }

        public override Stream OpenReadOnly()
        {
            if (writeAccessCounter == 0)
            {
                readAccessCounter += 1;
                MemoryStream ms = new MemoryStream(_data);
                return ms;
            }
            else
            {
                return null;
            }
        }

        public override Stream OpenReadWrite()
        {
            if (writeAccessCounter == 0 && readAccessCounter == 0)
            {
                readAccessCounter += 1;
                writeAccessCounter += 1;
                MemoryStream ms = new MemoryStream(_data);
                return ms;
            }
            else { return null; }
        }        

        //TODO: Listener for when the stream closes, decrement the counters;
    }

    public class MemFSDir : Dir422
    {        
        private string _name;
        private Dir422 _parent;
        private List<Dir422> _directoryList;
        private List<File422> _fileList;

        public MemFSDir(string name, Dir422 parent)
        {
            _name = name;
            _parent = parent;
        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override Dir422 Parent
        {
            get
            {
                return _parent;
            }
        }

        public override bool ContainsDir(string dirName, bool recursive)
        {
            if (dirName.Contains("/") || dirName.Contains("\\"))
                return false;

            if (_directoryList == null) { return false; }
            foreach (var dir in _directoryList)
            {
                if(dir.Name == dirName) { return true; }
            }

            if (recursive)
            {
                foreach (var dir in _directoryList)
                {
                    if(dir.ContainsDir(dirName, recursive)) { return true; }
                }
            }

            return false;
        }

        public override bool ContainsFile(string fileName, bool recursive)
        {
            if (fileName.Contains("/") || fileName.Contains("\\"))
                return false;

            if (_fileList == null) { return false; }
            foreach (var file in _fileList)
            {
                if (file.Name == fileName) { return true; }
            }

            if (recursive)
            {
                foreach (var dir in _directoryList)
                {
                    if (dir.ContainsFile(fileName, recursive)) { return true; }
                }
            }

            return false;
        }

        public override Dir422 CreateDir(string name)
        {
            MemFSDir newDir = new MemFSDir(name, this);
            if (_directoryList == null)
                _directoryList = new List<Dir422>();
            _directoryList.Add(newDir);
            return newDir;
        }

        public override File422 CreateFile(string name)
        {
            MemFSFile newFile = new MemFSFile(name, this);
            if (_fileList == null)
                _fileList = new List<File422>();
            _fileList.Add(newFile);
            return newFile;
        }

        public override Dir422 GetDir(string name)
        {
            foreach (var dir in _directoryList)
            {
                if (dir.Name == name) { return dir; }
            }
            return null;
        }

        public override IList<Dir422> GetDirs()
        {
            return _directoryList;
        }

        public override File422 GetFile(string name)
        {
            foreach (var file in _fileList)
            {
                if (file.Name == name) { return file; }
            }
            return null;
        }

        public override IList<File422> GetFiles()
        {
            return _fileList;
        }
    }

    public class MemoryFileSystem : FileSys422
    {
        private MemFSDir _root;

        public MemoryFileSystem()
        {
            //Create new and empty file system
            _root = new MemFSDir("root", null);
        }

        public override Dir422 GetRoot()
        {
            return _root;
        }
    }
}
