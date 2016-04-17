using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

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
        private RegistryKey autoReg = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        private string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketOnlineController.BinaryConfig");
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
           
            GZipStream WriteStream = new GZipStream(new FileStream(FilePath, FileMode.Create),CompressionLevel.Optimal); 
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

            if (!File.Exists(FilePath)) return;
            try
            {
                GZipStream ReadStream = new GZipStream(new FileStream(FilePath, FileMode.Open), CompressionMode.Decompress);
                BinaryFormatter Loader = new BinaryFormatter();
                dat = (Data)Loader.Deserialize(ReadStream);
                ReadStream.Close();
            }
            catch(EndOfStreamException e)
            {
                MessageBox.Show("Found and removed invalid config file");
                File.Delete(FilePath);

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
                autoReg.DeleteValue(IDENTIFIER);
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

    }



}
