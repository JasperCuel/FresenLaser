using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FresenLaser;

public static class Converter
{
    private const int NumberOfLinesToRemoveAtEnd = 4;
    
    private static List<string> _allNewLines = new();
    private static bool _skipUntilS100;
    private static string _lastComment = string.Empty;
    private static readonly Regex CompiledRegex = new(@"([MGXYZIJFST])([\-0-9.]+)*", RegexOptions.Compiled);

    //Current states
    private static int _stateM;
    private static int _stateS;
    private static bool _homed;
    
    private static readonly List<string> TempLines = new();

    private static void Reset()
    {
        _stateM = 0;
        _stateS = 0;
        _homed = false;
        _allNewLines = new List<string>();
    }

    public static void Convert(string filePath)
    {
        Reset();
        var lines = GetLinesFromFile(filePath);

        Dictionary<char, double> states = new();
        for (var i = 0; i < lines.Length - NumberOfLinesToRemoveAtEnd; i++)
        {
            var line = lines[i];
            HandleLine(line, states);
        }

        if (_skipUntilS100)
        {
            _allNewLines.AddRange(TempLines);
            TempLines.Clear();
            _skipUntilS100 = false;
        }
        
        FileInfo fileInfo = new(filePath);
        if (fileInfo.DirectoryName == null)
            throw new Exception("File path is invalid");

        var newFileName = $"{fileInfo.DirectoryName}/Laser_{fileInfo.Name}";
        File.WriteAllLines(newFileName, _allNewLines);
        Process.Start("explorer.exe", fileInfo.DirectoryName);
    }

    private static void HandleLine(string line, Dictionary<char, double> states)
    {
        if (line.StartsWith('(') || line.StartsWith('#'))
        {
            if (line == _lastComment) return;
            _allNewLines.Add(line);
            _lastComment = line;
            return;
        }

        //Remove all spaces
        var gLine = line.Replace(" ", "");
        Dictionary<char, double> lineCodes = new();

        var matches = CompiledRegex.Matches(gLine);
        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var code = match.Groups[1].Value[0];
            var valueStr = match.Groups[2].Value;
            var value = double.Parse(valueStr);

            lineCodes.TryAdd(code, value);

            if (code != 'F' && code != 'S')
                continue;

            states.TryAdd(code, value);
            states[code] = value;
        }

        //Rules
        var justAdd = true;
        var desiredM = 5;
        var desiredS = 0;

        if (lineCodes.ContainsKey('Z'))
        {
            var zValue = lineCodes['Z'];
            if (Math.Abs(zValue - 2.0) < 0.01)
                justAdd = false;
            if (Math.Abs(zValue - 10.0) < 0.01)
            {
                desiredM = 5;
                desiredS = 0;
                lineCodes.Add('M', 5);
                lineCodes['F'] = FoverMs(_stateM);
            }

            if (zValue == 0.0)
            {
                desiredM = 3;
                desiredS = 100;
            }

            if (Math.Abs(zValue - 1.0) < 0.01)
            {
                desiredM = 3;
                desiredS = 15;
            }
        }

        if (!_homed)
        {
            desiredM = 5;
            desiredS = 0;
            if (lineCodes.ContainsKey('M') && Math.Abs(lineCodes['M'] - 3) < 0.01)
            {
                lineCodes['M'] = 5;
                lineCodes.Remove('S');
            }
        }

        if (lineCodes.ContainsKey('X') && lineCodes['X'] == 0.0 && lineCodes.ContainsKey('Y') && lineCodes['Y'] == 0.0)
        {
            _homed = true;
            if (_skipUntilS100)
                TempLines.Add("(Homing to 0,0)");
            else
                _allNewLines.Add("(Homing to 0,0)");
        }

        if (lineCodes.ContainsKey('Z'))
            if (_stateM != desiredM || _stateS != desiredS)
            {
                var action = desiredM == 5 ? "off" : "on";
                if (_skipUntilS100)
                    TempLines.Add($"(Turning laser {action})");
                else
                    _allNewLines.Add($"(Turning laser {action})");
                _stateS = desiredS;
                _stateM = desiredM;
                lineCodes['M'] = desiredM;
                if (desiredM != 5)
                    lineCodes['S'] = desiredS;
            }

        if (lineCodes.ContainsKey('F')) lineCodes['F'] = FoverMs(_stateM);

        if (!justAdd)
            return;

        var relineBuilder = new StringBuilder();
        char[] codeOrder = { 'G', 'X', 'Y', 'I', 'J', 'M', 'S', 'F', 'T' };

        foreach (var code in codeOrder)
            if (lineCodes.TryGetValue(code, out var lineCode))
                relineBuilder.Append($"{code}{lineCode} ");

        var reline = relineBuilder.ToString().Trim();
        
        // Skip lines from S15 until S100 is encountered
        if (_skipUntilS100)
        {
            if (reline.Contains("S100"))
            {
                _skipUntilS100 = false;
                TempLines.Clear();
            }
            else
            {
                TempLines.Add(reline);
                return;
            }
        }

        if (reline.Contains("S15"))
        {
            _skipUntilS100 = true;
            return;
        }
        
        _allNewLines.Add(reline);
    }

    private static int FoverMs(int mode)
    {
        return mode == 5 ? ConversionSettings.TravelSpeed : ConversionSettings.CutSpeed;
    }

    private static string[] GetLinesFromFile(string filePath)
    {
        var fileContent = File.ReadAllText(filePath);
        var lines = fileContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        return lines;
    }
}