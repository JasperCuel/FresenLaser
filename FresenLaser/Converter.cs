using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FresenLaser;

public static class Converter
{
    private const int NumberOfLinesToRemoveAtEnd = 4;

    private static readonly char[] CodeOrder = { 'G', 'X', 'Y', 'I', 'J', 'M', 'S', 'F', 'T' };
    private static readonly Regex CompiledRegex = new(@"([MGXYZIJFST])([\-0-9.]+)*", RegexOptions.Compiled);

    private static List<string> _allNewLines = new();
    private static string _lastComment = string.Empty;

    //Current states
    private static int _stateM;
    private static int _stateS;
    private static bool _homed;

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

        PostProcess(_allNewLines);

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

        if (lineCodes.TryGetValue('Z', out var zValue))
        {
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

        if (lineCodes.ContainsKey('X') && lineCodes['X'] == 0.0 && lineCodes.ContainsKey('Y') &&
            lineCodes['Y'] == 0.0) _homed = true;

        if (lineCodes.ContainsKey('Z'))
            if (_stateM != desiredM || _stateS != desiredS)
            {
                _stateS = desiredS;
                _stateM = desiredM;
                lineCodes['M'] = desiredM;
                if (desiredM != 5) lineCodes['S'] = desiredS;
            }

        if (lineCodes.ContainsKey('F')) lineCodes['F'] = FoverMs(_stateM);

        if (!justAdd)
            return;

        var relineBuilder = new StringBuilder();

        foreach (var code in CodeOrder)
            if (lineCodes.TryGetValue(code, out var lineCode))
                relineBuilder.Append($"{code}{lineCode} ");

        var reline = relineBuilder.ToString().Trim();
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

    private static void PostProcess(List<string> lines)
    {
        var blocks = new List<Block>();
        Block currentBlock = null;
        var isInBlock = false;
        var nonBlockLines = new List<string>();

        foreach (var line in lines)
            if (line.Contains("M3", StringComparison.OrdinalIgnoreCase))
            {
                if (isInBlock && currentBlock != null)
                    blocks.Add(currentBlock);

                currentBlock = new Block();
                currentBlock.Lines.Add(line);
                isInBlock = true;
            }
            else if (line.Contains("M5", StringComparison.OrdinalIgnoreCase))
            {
                if (isInBlock && currentBlock != null)
                {
                    currentBlock.Lines.Add(line);
                    blocks.Add(currentBlock);
                    currentBlock = null;
                    isInBlock = false;
                }
            }
            else
            {
                if (isInBlock && currentBlock != null)
                {
                    currentBlock.Lines.Add(line);
                    ParseLineValues(line, currentBlock);
                }
                else
                {
                    nonBlockLines.Add(line);
                }
            }

        if (isInBlock && currentBlock != null)
            blocks.Add(currentBlock);

        ProcessBlocks(blocks);
        _allNewLines.Clear();
        _allNewLines.AddRange(nonBlockLines);
        AddBlocksToOutput(blocks);
        _allNewLines.Add("M30");
    }

    private static void ParseLineValues(string line, Block block)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
            if (part.StartsWith("X", StringComparison.OrdinalIgnoreCase))
                block.XValues.Add(double.Parse(part[1..]));
            else if (part.StartsWith("Y", StringComparison.OrdinalIgnoreCase))
                block.YValues.Add(double.Parse(part[1..]));
            else if (part.StartsWith("S", StringComparison.OrdinalIgnoreCase))
                block.SValues.Add(int.Parse(part[1..]));
    }

    private static void ProcessBlocks(List<Block> blocks)
    {
        foreach (var block in blocks)
            if (block.SValues.Contains(15) && block.SValues.Contains(100))
            {
                var indexOfS100 =
                    block.Lines.FindIndex(line => line.Contains("S100", StringComparison.OrdinalIgnoreCase));
                if (indexOfS100 >= 0)
                {
                    block.Lines = block.Lines.Skip(indexOfS100).ToList();
                    block.SValues.Remove(15);
                }
            }
    }

    private static void AddBlocksToOutput(List<Block> blocks)
    {
        var blockNumber = 1;
        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var nextBlock = i < blocks.Count - 1 ? blocks[i + 1] : null;
            if (nextBlock != null && block.HasSameXyValues(nextBlock))
                continue;

            if (block.Lines.Count > 0 && block.Lines[0].Contains("M3", StringComparison.OrdinalIgnoreCase))
            {
                var parts = block.Lines[0].Split("M3", 2);
                if (parts.Length == 2)
                {
                    block.Lines[0] = parts[0].Trim();
                    block.Lines.Insert(1, "M3 " + parts[1].Trim());
                }
            }

            block.Lines[^1] = block.Lines[^1].Replace("M5", "").Trim();
            block.Lines.Add("M5");

            _allNewLines.Add($"(Block {blockNumber++})");
            _allNewLines.AddRange(block.Lines);
        }
    }

    private class Block
    {
        public List<string> Lines { get; set; } = new();
        public List<double> XValues { get; } = new();
        public List<double> YValues { get; } = new();
        public List<int> SValues { get; } = new();

        public bool HasSameXyValues(Block other, double tolerance = 9.0)
        {
            var sameXCount = XValues.Intersect(other.XValues).Count();
            var sameYCount = YValues.Intersect(other.YValues).Count();

            var xMatchPercentage = XValues.Count > 0 ? (double)sameXCount / XValues.Count * 100 : 0;
            var yMatchPercentage = YValues.Count > 0 ? (double)sameYCount / YValues.Count * 100 : 0;

            var combinedMatchPercentage = (xMatchPercentage + yMatchPercentage) / 2;
            return combinedMatchPercentage >= 100 - tolerance;
        }
    }
}