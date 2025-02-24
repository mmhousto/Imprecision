using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class sceCode
{
    private class ErrorEntry
    {
        public string shortCode;
        public string longCode;
        public string descriptionJap;
        public string description;

        public ErrorEntry(string _shortCode, string _longCode, string _descriptionJap, string _description)
        {
            shortCode = _shortCode;
            longCode = _longCode;
            descriptionJap = _descriptionJap;
            description = _description;
        }
    }
    private uint errorCode;
    private static Dictionary<uint, ErrorEntry> errorDictionary;
    private static bool checkedErrorFile = false;
     

    public static implicit operator sceCode(long code)
    {
        return new sceCode(code);
    }

    private static void readErrorFile()
    {
        checkedErrorFile = true;
        string[] lines = new string[] { };
        
        // first try reading from PC connected via Neighborhood for PS4, then attempt from StreamingAssets folder ... if neither of them hold the file, then we drop back to minimal reporting
        try
        {
#if UNITY_PS4
            lines = File.ReadAllLines(@"/host/%SCE_ORBIS_SDK_DIR%\host_tools\debugging\error_code\error_table.csv"); // read complete error file from PC sdk folder
#else            
            lines = File.ReadAllLines(@"/host/%SCE_PROSPERO_SDK_DIR%\host_tools\debugging\error_code\error_table.csv"); // read complete error file from PC sdk folder
#endif            
        }
        catch(Exception)
        {
            try
            {
                lines = File.ReadAllLines(System.IO.Path.Combine(Application.streamingAssetsPath, "error_table.csv")); // read complete error file from PC sdk folder
            }
            catch(Exception) { }
        }

        errorDictionary = new Dictionary<uint, ErrorEntry>();
        foreach (string s in lines)
        {
            MatchCollection  match = Regex.Matches(s,"(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)");    // standard regex for handling csv files
            string[] column = new string[match.Count];
            for (int i = 0; i < match.Count; i++)
                column[i] = match[i].Groups[1].Value;
            if (column.Length==6)
                errorDictionary[Convert.ToUInt32(column[1].Substring(2),16)] = new ErrorEntry(column[0],column[2],column[3],column[4]);
        }
    }

    public sceCode(uint code)
    {
        errorCode = code;
    }

    public sceCode(long code)
    {
        errorCode = Convert.ToUInt32(code);
    }

    public override string ToString()
    {
        if (!checkedErrorFile)
            readErrorFile();
        try
        {
            return string.Format("0x{0:X} {1}/{2}/{3}/{4}", errorCode, errorDictionary[errorCode].shortCode,errorDictionary[errorCode].longCode,errorDictionary[errorCode].description,errorDictionary[errorCode].descriptionJap );
        }
        catch(Exception)
        {
            return string.Format("0x{0:X} ", errorCode);    // no extended error info found, so we drop back to just the error code
        }
    }
}
