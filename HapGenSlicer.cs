using System.Text;

namespace HapGenSlicer;

public class Slicer
{
    private readonly string _initialLegendPath;
    private readonly string _initialHapPath;
    private readonly string _resultGenPath;
    private readonly string _resultHapPath;
    private readonly string _expectedGenPath;
    private readonly double _snpProportion;
    private readonly double _samplesToGenProportion;
    private readonly string _newLineCharacter;

    public Slicer(
        string initialLegendPath,
        string initialHapPath,
        string resultGenPath,
        string resultHapPath,
        string expectedGenPath,
        double snpProportion,
        double samplesToGenProportion,
        string newLineCharacter)
    {
        _initialLegendPath = initialLegendPath ?? throw new ArgumentNullException(nameof(initialLegendPath));
        _initialHapPath = initialHapPath ?? throw new ArgumentNullException(nameof(initialHapPath));
        _resultGenPath = resultGenPath ?? throw new ArgumentNullException(nameof(resultGenPath));
        _resultHapPath = resultHapPath ?? throw new ArgumentNullException(nameof(resultGenPath));
        _expectedGenPath = expectedGenPath ?? throw new ArgumentNullException(nameof(expectedGenPath));
        _snpProportion = snpProportion;
        _samplesToGenProportion = samplesToGenProportion;
        _newLineCharacter = newLineCharacter ?? throw new ArgumentNullException(nameof(newLineCharacter));
    }

    public void Slice()
    {
        var hapReader = new StreamReader(_initialHapPath);
        var legendReader = new StreamReader(_initialLegendPath);
        var resultGenWriter = new StreamWriter(_resultGenPath);
        var resultHapWriter = new StreamWriter(_resultHapPath);
        var expectedGenWriter = new StreamWriter(_expectedGenPath);

        var sampleIndexes = GetSamplesToGenIndexes(hapReader, out int lastHapIndex);

        string currentLegendLine = legendReader.ReadLine(); // skip header
        currentLegendLine = legendReader.ReadLine().TrimEnd('\r', '\n');
        while (currentLegendLine != null && currentLegendLine != string.Empty)
        {
            currentLegendLine = currentLegendLine.TrimEnd('\r', '\n');
            var isSnpInGen = RandomizeWithProportion(_snpProportion);
            expectedGenWriter.Write(currentLegendLine);
            if (isSnpInGen)
            {
                resultGenWriter.Write(currentLegendLine.Split(' ')[0] + " " + currentLegendLine);
            }

            var hapCurrentChar = (char)hapReader.Read();
            var columnIndex = 0;
            while (hapCurrentChar != '\n' && hapCurrentChar != '\r')
            {
                var hapValue = ReadToSpaceOrEnd(hapReader, ref hapCurrentChar);
                if (sampleIndexes.Contains(columnIndex))
                {
                    expectedGenWriter.Write(' ');
                    expectedGenWriter.Write(hapValue);
                    if (isSnpInGen)
                    {
                        resultGenWriter.Write(' ');
                        resultGenWriter.Write(hapValue);
                    };
                }
                else
                {
                    resultHapWriter.Write(hapValue);
                    if (columnIndex != lastHapIndex)
                    {
                        resultHapWriter.Write(' ');
                    }
                }

                if (hapCurrentChar == '\r')
                {
                    hapCurrentChar = (char)hapReader.Read();
                }

                if (hapCurrentChar == '\n')
                {
                    break;
                }

                columnIndex++;
                hapCurrentChar = (char)hapReader.Read();
            }

            while (hapCurrentChar != '\n')
            {
                hapCurrentChar = (char)hapReader.Read();
            }
            
            expectedGenWriter.Write(_newLineCharacter);
            if (isSnpInGen)
            {
                resultGenWriter.Write(_newLineCharacter);
            }

            resultHapWriter.Write(_newLineCharacter);

            currentLegendLine = legendReader.ReadLine();
        }

        legendReader.Close();
        hapReader.Close();
        resultGenWriter.Close();
        resultHapWriter.Close();
        expectedGenWriter.Close();
    }

    private bool RandomizeWithProportion(double proportionOfTrue)
    {
        return new Random().NextDouble() < proportionOfTrue;
    }

    private string ReadToSpaceOrEnd(StreamReader reader, ref char currentChar)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(currentChar);
        while (currentChar != ' ' && currentChar != '\r' && currentChar != '\n')
        {
            currentChar = (char)reader.Read();
            if (currentChar == ' ' || currentChar == '\r' || currentChar == '\n')
            {
                break;
            }
            else
            {
                stringBuilder.Append(currentChar);
            }
        }

        return stringBuilder.ToString();
    }

    private HashSet<int> GetSamplesToGenIndexes(StreamReader hapReader, out int lastHapIndex)
    {
        var sampleIndexes = new HashSet<int>();
        lastHapIndex = 0;
        var hapCurrentChar = (char)hapReader.Read();
        var columnIndex = 0;
        while (hapCurrentChar != '\n' && hapCurrentChar != '\r')
        {
            ReadToSpaceOrEnd(hapReader, ref hapCurrentChar);
            if (hapCurrentChar == '\r')
            {
                hapCurrentChar = (char)hapReader.Read();
            }

            if (hapCurrentChar == '\n')
            {
                break;
            }

            if (RandomizeWithProportion(_samplesToGenProportion))
            {
                sampleIndexes.Add(columnIndex);
            }

            columnIndex++;
            lastHapIndex++;
            hapCurrentChar = (char)hapReader.Read();
        }

        sampleIndexes.Add(lastHapIndex - 1);
        
        hapReader.DiscardBufferedData();
        hapReader.BaseStream.Seek(0, SeekOrigin.Begin);

        return sampleIndexes;
    }
}