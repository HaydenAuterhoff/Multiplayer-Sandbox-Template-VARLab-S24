using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VARLab.SCORM.Editor
{
    /// <summary>This class handles the editor window for the Unity-SCORM Integration Kit</summary>
    public class SCORMBuilder
    {
        // Relative path for plugin when deployed as a Unity package
        public const string PluginContentsPath = "Packages/com.varlab.scorm/Plugins/2004";

        /// <summary>Copies files from one directory to another.</summary>
        /// <param name="source">Source directory to copy from.</param>
        /// <param name="target">Target director to copy to.</param>
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (FileInfo file in source.GetFiles())
            {
                if (!IsFileHiddenOrMetadata(file))
                {
                    file.CopyTo(Path.Combine(target.FullName, file.Name));
                }
            }
        }

        /// <summary>Checks if a file is hidden or metadata.</summary>
        /// <returns><see langword="true"/> if the file is hidden or metadata, <see langword="false"/> otherwise.</returns>
        /// <param name="file">File to check.</param>
        private static bool IsFileHiddenOrMetadata(FileInfo file)
        {
            return file.Name.Substring(0, 1) == "." || file.Name.Substring(file.Name.Length - 4) == "meta";
        }

        /// <summary>Convert Seconds to the SCORM timeInterval.</summary>
        /// <returns>timeInterval string.</returns>
        /// <param name="seconds">Seconds.</param>
        private static string SecondsToTimeInterval(float seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return string.Format("P{0:D}DT{1:D}H{2:D}M{3:F}S", t.Days, t.Hours, t.Minutes, t.Seconds);
            //This is good enough to feed into SCORM, no need to include Years and Months
        }

        /// <summary>Parses a string into a float.</summary>
        /// <remarks>float.Parse fails on an empty string, so we use this to return a 0 if an empty string is encountered.</remarks>
        /// <returns>The float represented by the input <paramref name="str"/>.</returns>
        /// <param name="str">String representation of the float.</param>
        private static float ParseFloat(string str)
        {
            float result = 0f;
            float.TryParse(str, out result);
            return result;
        }

        public static void Publish()
        {
            SCORMExportData data = new()
            {
                ManifestIdentifier = "PM",// PlayerPrefs.GetString("Manifest_Identifier"),
                CourseTitle = "PM",//PlayerPrefs.GetString("Course_Title"),
                CourseDescription = "PM",//PlayerPrefs.GetString("Course_Description"),
                SCOTitle = "PM",//PlayerPrefs.GetString("SCO_Title"),
                DataFromLMS = "",//PlayerPrefs.GetString("Data_From_Lms"),
                CompletedByProgressAmount = false,//Convert.ToBoolean(PlayerPrefs.GetInt("completedByMeasure")),
                ProgressAmountForCompletion = 0.0f,// PlayerPrefs.GetFloat("minProgressMeasure"),
                TimeLimitAction = TimeLimitAction.None,//(TimeLimitAction)PlayerPrefs.GetInt("Time_Limit_Action"),
                TimeLimit = ""//PlayerPrefs.GetString("Time_Limit_Secs")
            };

            Publish("Builds/WebGL", "Builds/SCORM", data);
        }

        /// <summary>Publish this SCORM package to a conformant zip file.</summary>
        public static void Publish(string exportDirectory, string exportFilePath, SCORMExportData data)
        {
            var timeLimitAction = "";
            switch (data.TimeLimitAction)
            {
                case TimeLimitAction.None:
                    timeLimitAction = "";
                    break;
                case TimeLimitAction.ExitWithMessage:
                    timeLimitAction = "exit,message";
                    break;
                case TimeLimitAction.ExitNoMessage:
                    timeLimitAction = "exit,no message";
                    break;
                case TimeLimitAction.ContinueWithMessage:
                    timeLimitAction = "continue,message";
                    break;
                case TimeLimitAction.ContinueNoMessage:
                    timeLimitAction = "continue,no message";
                    break;
            }

            var timeLimit = SecondsToTimeInterval(ParseFloat(data.TimeLimit));

            string tempdir = Path.GetTempPath() + Path.GetRandomFileName();
            _ = Directory.CreateDirectory(tempdir);
            CopyFilesRecursively(new DirectoryInfo(exportDirectory), new DirectoryInfo(tempdir));

            if (!string.IsNullOrEmpty(exportFilePath))
            {
                if (File.Exists(exportFilePath))
                    File.Delete(exportFilePath);

                var manifest = GetManifest(timeLimitAction, timeLimit, data);

                var zip = new Ionic.Zip.ZipFile(exportFilePath);
                zip.AddDirectory(tempdir);
                zip.AddItem(PluginContentsPath);
                zip.AddEntry("imsmanifest.xml", ".", System.Text.Encoding.ASCII.GetBytes(manifest));
                zip.Save();
            }
        }

        private static string GetManifest(string timeLimitAction, string timeLimit, SCORMExportData data)
        {
            return "<?xml version=\"1.0\" standalone=\"no\" ?>\n" +
                "<manifest identifier=\"" + data.ManifestIdentifier + "\" version=\"1\"\n" +
                "\t\txmlns = \"http://www.imsglobal.org/xsd/imscp_v1p1\"\n" +
                "\t\txmlns:adlcp = \"http://www.adlnet.org/xsd/adlcp_v1p3\"\n" +
                "\t\txmlns:adlseq = \"http://www.adlnet.org/xsd/adlseq_v1p3\"\n" +
                "\t\txmlns:adlnav = \"http://www.adlnet.org/xsd/adlnav_v1p3\"\n" +
                "\t\txmlns:imsss = \"http://www.imsglobal.org/xsd/imsss\"\n" +
                "\t\txmlns:xsi = \"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                "\t\txmlns:lom=\"http://ltsc.ieee.org/xsd/LOM\"\n" +
                "\t\txsi:schemaLocation = \"http://www.imsglobal.org/xsd/imscp_v1p1 imscp_v1p1.xsd\n" +
                "\t\t\thttp://www.adlnet.org/xsd/adlcp_v1p3 adlcp_v1p3.xsd\n" +
                "\t\t\thttp://www.adlnet.org/xsd/adlseq_v1p3 adlseq_v1p3.xsd\n" +
                "\t\t\thttp://www.adlnet.org/xsd/adlnav_v1p3 adlnav_v1p3.xsd\n" +
                "\t\t\thttp://www.imsglobal.org/xsd/imsss imsss_v1p0.xsd\n" +
                "\t\t\thttp://ltsc.ieee.org/xsd/LOM lom.xsd\" >\n" +
                "<metadata>\n" +
                "\t<schema>ADL SCORM</schema>\n" +
                "\t<schemaversion>2004 4th Edition</schemaversion>\n" +
                "<lom:lom>\n" +
                "\t<lom:general>\n" +
                "\t\t<lom:description>\n" +
                "\t\t\t<lom:string language=\"en-US\">" + data.CourseDescription + "</lom:string>\n" +
                "\t\t</lom:description>\n" +
                "\t</lom:general>\n" +
                "\t</lom:lom>\n" +
                "</metadata>\n" +
                "<organizations default=\"B0\">\n" +
                "\t<organization identifier=\"B0\" adlseq:objectivesGlobalToSystem=\"false\">\n" +
                "\t\t<title>" + data.CourseTitle + "</title>\n" +
                "\t\t<item identifier=\"i1\" identifierref=\"r1\" isvisible=\"true\">\n" +
                "\t\t\t<title>" + data.SCOTitle + "</title>\n" +
                "\t\t\t<adlcp:timeLimitAction>" + timeLimitAction + "</adlcp:timeLimitAction>\n" +
                "\t\t\t<adlcp:dataFromLMS>" + data.DataFromLMS + "</adlcp:dataFromLMS> \n" +
                "\t\t\t<adlcp:completionThreshold completedByMeasure = \"" + data.CompletedByProgressAmount.ToString().ToLower() + "\" minProgressMeasure= \"" + data.ProgressAmountForCompletion + "\" />\n" +
                "\t\t\t<imsss:sequencing>\n" +
                "\t\t\t<imsss:limitConditions attemptAbsoluteDurationLimit=\"" + timeLimit + "\"/>\n" +
                "\t\t\t</imsss:sequencing>\n" +
                "\t\t</item>\n" +
                "\t</organization>\n" +
                "</organizations>\n" +
                "<resources>\n" +
                "\t<resource identifier=\"r1\" type=\"webcontent\" adlcp:scormType=\"sco\" href=\"index.html\">\n" +
                "\t\t<file href=\"index.html\" />\n" +
                "\t\t<file href=\"TemplateData/scorm.js\" />\n" +
                "\t</resource>\n" +
                "</resources>\n" +
                "</manifest>";
        }
    }
}