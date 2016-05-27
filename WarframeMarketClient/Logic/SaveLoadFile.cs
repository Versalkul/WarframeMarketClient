using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.Threading;

namespace WarframeMarketClient.Logic
{
    class SaveLoadFile
    {
        private static String IDENTIFIER = "WarframeMarketClient";

        #region dataclass

        [Serializable()]
        private class Data
        {
            public Dictionary<string, bool> bools = new Dictionary<string, bool>();
            public Dictionary<string, int> ints = new Dictionary<string, int>();
            public Dictionary<string, string> strings = new Dictionary<string,string>();



        }
        #endregion

        private Data dat = new Data();
        private RegistryKey autoReg;
        private string folderPath;
        private string filePath;
        string soundFolderPath;
        public SaveLoadFile()
        {
            autoReg = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient");

            filePath = Path.Combine(folderPath, "Config.BinaryConfig");
            soundFolderPath = Path.Combine(folderPath, "Sounds");
            Directory.CreateDirectory(folderPath);
            Directory.CreateDirectory(soundFolderPath);
        }

        public bool FileExists()
        {
            return File.Exists(filePath);
        }

        #region Integer

        /// <summary>
        /// saves a int to the regestry
        /// </summary>
        /// <param name="name"></param>
        /// <param name="i"></param>
        public void saveInt(String name, int i)
        {
            dat.ints.Add(name, i);
        }
        /// <summary>
        /// loads a bool from the regestry
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int loadInt(String name)
        {
            if (!dat.ints.ContainsKey(name))
            {
                return 0;
            }
            return dat.ints[name];

        }

        #endregion Integer

        #region booleans

        /// <summary>
        /// saves a bool to the regestry
        /// </summary>
        /// <param name="name"></param>
        /// <param name="b"></param>
        public void saveBool(String name, bool b)
        {
            dat.bools.Add(name, b);
        }
        /// <summary>
        /// loads a bool from the regestry
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool loadBool(String name)
        {

            if (!dat.bools.ContainsKey(name))
            {
                return false;
            }
          
            return dat.bools[name];
        }

        #endregion

        #region strings

        public void saveString(String name, string s)
        {
            dat.strings.Add(name, s);
        }

        public string loadString(String name)
        {

            if (!dat.strings.ContainsKey(name))
            {
                return "";
            }

            return dat.strings[name];
        }


        #endregion

        #region mainSave/Load
        /// <summary>
        /// Does the saving
        /// </summary>
        public void Save()
        {
           
            GZipStream WriteStream = new GZipStream(new FileStream(filePath, FileMode.Create),CompressionLevel.Optimal); 
            BinaryFormatter saver = new BinaryFormatter();
            saver.Serialize(WriteStream, dat);
            WriteStream.Close();
            

        }



        /// <summary>
        /// reads the config file to DATA array 
        /// MUST BE CALLED
        /// </summary>
        public void Read()
        {

            if (!File.Exists(filePath)) return;
            try
            {
                GZipStream ReadStream = new GZipStream(new FileStream(filePath, FileMode.Open), CompressionMode.Decompress);
                BinaryFormatter Loader = new BinaryFormatter();
                dat = (Data)Loader.Deserialize(ReadStream);
                ReadStream.Close();
            }
            catch(EndOfStreamException e)
            {
                File.Delete(filePath);

            }


        }

        #endregion

        #region autostart
        /// <summary>
        /// Manages the autostart
        /// </summary>
        /// <param name="set"></param>
        public void autostart(bool set)
        {
            if (set)
            {
                autoReg.SetValue(IDENTIFIER, Assembly.GetEntryAssembly().Location);
            }
            else
            {
                if(isAutostart()) autoReg.DeleteValue(IDENTIFIER);
            }
        }


        /// <summary>
        /// is the Autostart enabled
        /// </summary>
        /// <returns></returns>
        public bool isAutostart()
        {
            return ((string)autoReg.GetValue(IDENTIFIER) == Assembly.GetEntryAssembly().Location);
        }


        /// <summary>
        /// updates the autostart aka updates the programm path 
        /// </summary>
        public void updateAutostart()
        {
            if ((string)autoReg.GetValue(IDENTIFIER) != null
                && (string)autoReg.GetValue(IDENTIFIER) != Assembly.GetEntryAssembly().Location)
            {
                autostart(false);
                autostart(true);
            }
        }

        #endregion autostart

        #region SoundStuff

        public void ExtractStandartSounds()
        {

           
            File.WriteAllBytes(Path.Combine(soundFolderPath, "NewMessage.mp3"),Properties.Resources.NewMessage);
            File.WriteAllBytes(Path.Combine(soundFolderPath, "NewMessage2.mp3"), Properties.Resources.NewMessage2);
            File.WriteAllBytes(Path.Combine(soundFolderPath, "NewMessage3.mp3"), Properties.Resources.NewMessage3);
            File.WriteAllBytes(Path.Combine(soundFolderPath, "NewMessage4.mp3"), Properties.Resources.NewMessage4);
            File.WriteAllBytes(Path.Combine(soundFolderPath, "NewMessage5.mp3"), Properties.Resources.NewMessage5);
            File.WriteAllBytes(Path.Combine(soundFolderPath, "NewMessage6.mp3"), Properties.Resources.NewMessage6);
            File.WriteAllBytes(Path.Combine(soundFolderPath, "NewMessage7.mp3"), Properties.Resources.NewMessage7);
            File.WriteAllBytes(Path.Combine(soundFolderPath, "NewMessage8.mp3"), Properties.Resources.NewMessage8);


        }



        private bool IsPlayable(string file)
        {
            return file.ToLower().Contains(".wav") || file.ToLower().Contains(".mp3");

        }

        public bool ImportSound(string path)
        {
            if(IsPlayable(path))
            {

                string copyTo = Path.Combine(soundFolderPath, Path.GetFileName(path));
                File.Copy(path, copyTo);
                return true;
            }
            return false;
        }

        public List<string> GetSounds()
        {
            return Directory.GetFiles(soundFolderPath).ToList().Select(x=>Path.GetFileName(x)).ToList();
        }

        #endregion

    }



}
