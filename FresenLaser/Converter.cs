using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FresenLaser
{
    public static class Converter
    {
        private static List<string> allNewLines = new();

        private static readonly string regex = @"([MGXYZIJFST])([\-0-9.]+)*";

        //Current states
        private static int stateM = 0;
        private static int stateS = 0;
        private static bool homed = false;

        private static void Reset()
        {
            stateM = 0;
            stateS = 0;
            homed = false;
            allNewLines = new();
        }

        public static void Convert(string filePath)
        {
            Reset();
            string[] lines = GetLinesFromFile(filePath);

            Dictionary<char, double> states = new();
            foreach (string line in lines)
            {
                HandleLine(line, states);
            }

            FileInfo fileInfo = new(filePath);
            string newFileName = $"{fileInfo.DirectoryName}/Laser_{fileInfo.Name}";
            File.WriteAllLines(newFileName, allNewLines);
            Process.Start("explorer.exe", fileInfo.DirectoryName);
        }

        private static void HandleLine(string line, Dictionary<char, double> states)
        {
            if (line.Contains('(') || line.Contains('#'))
            {
                allNewLines.Add(line);
                return;
            }

            //Remove all spaces
            string gline = line.Replace(" ", "");
            Dictionary<char, double> lineCodes = new();

            MatchCollection matches = Regex.Matches(gline, regex);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                char code = match.Groups[1].Value[0];
                string valueStr = match.Groups[2].Value;
                double value = double.Parse(valueStr);

                if (!lineCodes.ContainsKey(code))
                    lineCodes[code] = value;

                if (code == 'F' || code == 'S')
                {
                    if (!states.ContainsKey(code))
                        states.Add(code, value);
                    states[code] = value;
                }
            }

            //Rules
            bool justAdd = true;
            int desired_m = 5;
            int desired_s = 0;

            if (lineCodes.ContainsKey('Z'))
            {
                double zValue = lineCodes['Z'];
                if (zValue == 2.0)
                    justAdd = false;
                if (zValue == 10.0)
                {
                    desired_m = 5;
                    desired_s = 0;
                    lineCodes.Add('M', 5);
                    lineCodes['F'] = FoverMS(stateM);
                }
                if (zValue == 0.0)
                {
                    desired_m = 3;
                    desired_s = 100;
                }
                if (zValue == 1.0)
                {
                    desired_m = 3;
                    desired_s = 15;
                }
            }
            if (!homed)
            {
                desired_m = 5;
                desired_s = 0;
                if (lineCodes.ContainsKey('M') && lineCodes['M'] == 3)
                {
                    lineCodes['M'] = 5;
                    lineCodes.Remove('S');
                }
            }
            if (lineCodes.ContainsKey('X') && lineCodes['X'] == 0.0 && lineCodes.ContainsKey('Y') && lineCodes['Y'] == 0.0)
            {
                homed = true;
                allNewLines.Add("(Homing to 0,0)");
            }

            if (lineCodes.ContainsKey('Z'))
            {
                if (stateM != desired_m || stateS != desired_s)
                {
                    string action = desired_m == 5 ? "off" : "on";
                    allNewLines.Add($"(Turning laser {action})");
                    stateS = desired_s;
                    stateM = desired_m;
                    lineCodes['M'] = desired_m;
                    if (desired_m != 5)
                        lineCodes['S'] = desired_s;
                }
            }
            if (lineCodes.ContainsKey('F'))
            {
                lineCodes['F'] = FoverMS(stateM);
            }

            if (justAdd)
            {
                string reline = "";
                char[] codeOrder = { 'G', 'X', 'Y', 'I', 'J', 'M', 'S', 'F', 'T' };

                foreach (char code in codeOrder)
                {
                    if (lineCodes.ContainsKey(code))
                    {
                        if (reline.Length > 0)
                            reline += " ";
                        reline += $"{code}{lineCodes[code]}";
                    }
                }
                allNewLines.Add(reline);
            }
        }

        private static int FoverMS(int mode) => mode == 5 ? ConversionSettings.TravelSpeed : ConversionSettings.CutSpeed;

        private static string[] GetLinesFromFile(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            string[] lines = fileContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            return lines;
        }
    }
}