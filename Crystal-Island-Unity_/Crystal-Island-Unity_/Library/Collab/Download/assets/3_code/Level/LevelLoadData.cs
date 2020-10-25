using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class LevelLoadData : VCBehaviour<Level>
    {
        public string levelDataJson = "leveldata.json";
        private LevelData loadedLevelData = null;

        public new IEnumerator Start()
        {
            //load json in start
            //load homes
            yield return StartCoroutine(loadText(
                    levelDataJson,
                    (json) => { loadedLevelData = JsonUtility.FromJson<LevelData>(json); RootLogger.Info(this, "Loaded level data from {0}", this.levelDataJson); }
                ));

            //call base start
            base.Start();
        }

        public override void onModelChanged()
        {
            if (loadedLevelData != null)
            {
                model.levelData = loadedLevelData;
            }
        }

        /// <summary>
        /// loads a text from the streaming asset path
        /// </summary>
        /// <param name="path">the path</param>
        /// <param name="callback">callback method with json string</param>
        /// <returns></returns>
        private IEnumerator loadText(string path, System.Action<string> callback)
        {
            string url = System.IO.Path.Combine(Application.streamingAssetsPath, path);

            if (url.Contains("://"))
            {
                WWW www = new WWW(url);
                yield return www;
                if (string.IsNullOrEmpty(www.error))
                {
                    callback(www.text);
                }
                else
                {
                    RootLogger.Error(this, "{0}", www.error);
                }
            }
            else
            {
                callback(System.IO.File.ReadAllText(url));
            }
        }

    }

}
