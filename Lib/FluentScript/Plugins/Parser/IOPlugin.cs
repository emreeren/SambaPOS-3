using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Provides basic File/Directory operations.
         
    // Case 1: Fluent-mode ( files ) - with Uri plugin and FluentMember plugin
    create   file  c:\temp\fs.txt   contents: 'testing'
    append   file  c:\temp\fs.txt   contents: 'testing'
    copy     file  c:\temp\fs.txt   to: c:\temp\fs.log
    move     file  c:\temp\fs.txt   to: c:\temp\fs2.txt
    rename   file  c:\temp\fs.txt   to: 'fs1.txt'
    delete   file  c:\temp\fs.txt 
    var exists = file exists c:\temp\fs.txt
    
     
    // Case 2: Fluent-mode ( directories ) - with Uri plugin and FluentMember plugin
    create   dir   c:\temp\fs
    copy     dir   c:\temp\fs,       to: c:\temp\fs1
    move     dir   c:\temp\fs2,      to: c:\temp\fs
    rename   dir   c:\temp\fs1,      to: 'fs2'
    delete   dir   c:\temp\fs
    var exists = dir exists       c:\temp\fs 
    
   
    // Case 3: Files  ( Explicit calls )
    File.Create( 'c:\\build\\log.txt', 'contents' )
    File.Append( 'c:\\build\\log.txt', 'contents' )
    File.Copy  ( 'c:\\build\\log.txt', 'c:\\build\\file2.txt')
    File.Rename( 'c:\\temp\\fs.txt',  'fs-io-test2.txt', true );
    File.Delete( 'c:\\temp\\fs.txt' )
    File.Move  ( 'c:\\temp\\fs.txt',  'c:\\temp\\fs-io-test.txt' )
    
   
    // Case 4: Directories ( Explicit calls )
    // NOTE: This is similar to case 3 where each call is "Dir.<method>( <paramlist> )"
    
    </doc:example>
    ***************************************************************************/
    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class FileIOPlugin : SetupPlugin
    {        
        /// <summary>
        /// Executes further Registration actions
        /// </summary>
        /// <param name="ctx">The context of the interperter</param>
        public override void Setup(Context ctx)
        {
            ctx.Types.Register(typeof(File), null);
            ctx.Types.Register(typeof(Files), null);
            ctx.Types.Register(typeof(Dir),  null);
            ctx.Types.Register(typeof(Dirs), null);
        }
    }



    /// <summary>
    /// API for File based IO.
    /// </summary>
    public class File
    {
        /// <summary>
        /// Creates to a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        public static void Create(string path, string contents)
        {
            global::System.IO.File.WriteAllText(path, contents);
        }


        /// <summary>
        /// Writes to a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="contents"></param>
        public static void Write(string path, string contents)
        {
            global::System.IO.File.WriteAllText(path, contents);
        }


        /// <summary>
        /// Appends text to a file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="contents"></param>
        public static void Append(string path, string contents)
        {
            global::System.IO.File.AppendAllText(path, contents);
        }


        /// <summary>
        /// Copy a file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="to">The path to copy to</param>
        /// <param name="overwrite">Whether or not to overwrite any existing files</param>
        public static void Copy(string path, string to, bool overwrite)
        {
            global::System.IO.File.Copy(path, to, overwrite);
        }


        /// <summary>
        /// Appends text to a file
        /// </summary>
        /// <param name="path">The path to the file to append to</param>
        /// <param name="to">The new name of the file</param>
        /// <param name="overwrite">Whether or not to overwrite any existing files</param>
        public static void Rename(string path, string to, bool overwrite)
        {
            var dirpath = Path.GetDirectoryName(path);
            var newpath = dirpath + Path.DirectorySeparatorChar + to;
            global::System.IO.File.Move(path, newpath);
        }


        /// <summary>
        /// Moves a file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="to">The new location of the file</param>
        public static void Move(string path, string to)
        {
            global::System.IO.File.Move(path, to);
        }


        /// <summary>
        /// Appends text to a file
        /// </summary>
        /// <param name="path">The path to the file to append to</param>
        public static void Delete(string path)
        {
            if(File.Exists(path))
                global::System.IO.File.Delete(path);
        }


        /// <summary>
        /// whether or not the file exists
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns></returns>
        public static bool Exists(string path)
        {
            return global::System.IO.File.Exists(path);
        }


        /// <summary>
        /// Gets the version of the file specified.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static LVersion GetVersion(string path)
        {
            // ? This is a problem. Do not have the debug/source code information here.
            if (!global::System.IO.File.Exists(path))
                throw new LangException("File not found", "File : " + path + " not found", string.Empty, 0, 0);

            // Throw exception?
            var ext = Path.GetExtension(path);
            if (ext != "dll" && ext != "exe")
                return new LVersion(Version.Parse("0.0.0.0"));

            var asm = Assembly.LoadFrom(path);
            var version = asm.GetName().Version;
            var lversion = new LVersion(version);
            return lversion;
        }
    }



    /// <summary>
    /// API for File based IO.
    /// </summary>
    public class Dir
    {
        /// <summary>
        /// Creates the directory
        /// </summary>
        /// <param name="path"></param>
        public static void Create(string path)
        {
            global::System.IO.Directory.CreateDirectory(path);
        }


        /// <summary>
        /// Creates the directory
        /// </summary>
        /// <param name="path"></param>
        public static void Make(string path)
        {
            global::System.IO.Directory.CreateDirectory(path);
        }


        /// <summary>
        /// Copy a file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="to">The path to copy to</param>
        /// <param name="overwrite">Whether or not to overwrite any existing files</param>
        public static void Copy(string path, string to, bool overwrite)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string name = Path.GetFileName( file );
                string dest = Path.Combine(to, name);
                global::System.IO.File.Copy( file, dest );
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName( folder );
                string dest = Path.Combine(to, name);
                Copy( folder, dest, overwrite );
            }
        }


        /// <summary>
        /// Copy a file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="recurse">Whether or not to recurse into each folder</param>
        /// <param name="callback">The callback to call</param>
        public static void ForEachFile(string path, bool recurse, Action<string> callback)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                callback(file);
            }
            if (recurse)
            {
                string[] folders = Directory.GetDirectories(path);
                foreach (string folder in folders)
                {
                    ForEachFile(folder, recurse, callback);
                }
            }
        }


        /// <summary>
        /// Copy a file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="recurse">Whether or not to recurse into each folder</param>
        /// <param name="callback">The callback to call</param>
        public static void ForEachDir(string path, bool recurse, Action<string> callback)
        {
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                if(recurse)
                    ForEachFile(folder, recurse, callback);
                callback(folder);
            }
        }
        

        /// <summary>
        /// Renames a directory
        /// </summary>
        /// <param name="path">The path to the directory to rename</param>
        /// <param name="to">The new name of the directory</param>
        public static void Rename(string path, string to)
        {
            var dirpath = Path.GetDirectoryName(path);
            var newpath = dirpath + Path.DirectorySeparatorChar + to;
            global::System.IO.Directory.Move(path, newpath);
        }


        /// <summary>
        /// Moves a directory
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <param name="to">The new location of the directory</param>
        public static void Move(string path, string to)
        {
            global::System.IO.Directory.Move(path, to);
        }


        /// <summary>
        /// Appends text to a directory
        /// </summary>
        /// <param name="path">The path to the directory to delete</param>
        public static void Delete(string path)
        {            
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                global::System.IO.File.Delete(file);
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                Delete(folder);
            }
            global::System.IO.Directory.Delete(path);
        }


        /// <summary>
        /// whether or not the directory exists
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <returns></returns>
        public static bool Exists(string path)
        {
            return global::System.IO.Directory.Exists(path);
        }


        /// <summary>
        /// Zips a directory
        /// </summary>
        /// <param name="path"></param>
        /// <param name="to"></param>
        public static void Zip(string path, string to)
        {
            //System.IO.Compression.
        }
    }



    /// <summary>
    /// Helper class to perform operations find/delete on files.
    /// </summary>
    public class Files
    {
        /// <summary>
        /// Finds files that match the filters supplied.
        /// </summary>
        /// <param name="path">The base path</param>
        /// <param name="named">The names to match</param>
        /// <param name="recursive">Whether or not to recurse folders</param>
        /// <param name="exts">The names of the extensions to match</param>
        /// <returns></returns>
        public static List<object> Find(string path, string[] named, string[] exts, bool recursive)
        {
            var files = FindInternal(path, named, exts, recursive);
            return files;
        }       


        /// <summary>
        /// Finds files that match the filters supplied.
        /// </summary>
        /// <param name="path">The base path</param>
        /// <param name="named">The names to match</param>
        /// <param name="recursive">Whether or not to recurse folders</param>
        /// <param name="exts">The names of the extensions to match</param>
        /// <returns></returns>
        public static void Delete(string path, string[] named, string[] exts, bool recursive)
        {
            var files = FindInternal(path, named, exts, recursive);
            if (files.Count == 0) return;

            foreach (var file in files)
                global::System.IO.File.Delete((string)file);
        }        


        private static List<object> FindInternal(string path, string[] named, string[] exts, bool recursive)
        {
            var files = new List<object>();
            var hasExtFilter = exts != null && exts.Length > 0;
            var hasNameFilter = named != null && named.Length > 0;
            IDictionary<string, string> extMap = hasExtFilter ? LangHelper.ToDictionary(exts) : null;
            IDictionary<string, string> nameMap = hasNameFilter ? LangHelper.ToDictionary(named) : null;

            // Match the files.
            Dir.ForEachFile(path, recursive, (filepath) =>
            {
                var ext = Path.GetExtension(filepath);
                var name = Path.GetFileNameWithoutExtension(filepath);
                bool matchesExtFilter = true;
                bool matchesNameFilter = true;
                if (hasExtFilter && !extMap.ContainsKey(ext))
                    matchesExtFilter = false;
                if (hasNameFilter && !nameMap.ContainsKey(name))
                    matchesNameFilter = false;

                // Add if both filters match or if both filter were not applicable.
                // or if 1 matches and the other one was not applicable.
                if (matchesNameFilter && matchesExtFilter)
                    files.Add(filepath);
            });
            return files;
        }
    }



    /// <summary>
    /// Directory class to find delete multiple directories
    /// </summary>
    public class Dirs
    {
        /// <summary>
        /// Finds files that match the filters supplied.
        /// </summary>
        /// <param name="path">The base path</param>
        /// <param name="named">The names to match</param>
        /// <param name="recursive">Whether or not to recurse folders</param>
        /// <returns></returns>
        public static List<object> Find(string path, string[] named, bool recursive)
        {
            var files = FindInternal(path, named, recursive);
            return files;
        }


        /// <summary>
        /// Finds files that match the filters supplied.
        /// </summary>
        /// <param name="path">The base path</param>
        /// <param name="named">The names to match</param>
        /// <param name="recursive">Whether or not to recurse folders</param>
        /// <returns></returns>
        public static void Delete(string path, string[] named, bool recursive)
        {
            var files = FindInternal(path, named, recursive);
            if (files.Count == 0) return;

            foreach (var file in files)
                global::System.IO.File.Delete((string)file);
        }


        private static List<object> FindInternal(string path, string[] named, bool recursive)
        {
            var files = new List<object>();
            var hasNameFilter = named != null && named.Length > 0;
            IDictionary<string, string> nameMap = hasNameFilter ? LangHelper.ToDictionary(named) : null;

            // Match the files.
            Dir.ForEachDir(path, recursive, (filepath) =>
            {
                var name = Path.GetFileNameWithoutExtension(filepath);
                bool matchesNameFilter = true;
                if (hasNameFilter && !nameMap.ContainsKey(name))
                    matchesNameFilter = false;

                // Add if both filters match or if both filter were not applicable.
                // or if 1 matches and the other one was not applicable.
                if (matchesNameFilter)
                    files.Add(filepath);
            });
            return files;
        }
    }
}

