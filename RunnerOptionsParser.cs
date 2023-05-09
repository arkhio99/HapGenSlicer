using System.Globalization;
using FastMember;
using NDesk.Options;

namespace HapGenSlicer;

public class RunnerOptionsParser
{
    private static readonly Dictionary<string, string> _paramNameByRunnerOptionsName = new()
    {
        { nameof(RunnerOptions.SnpProportion), "sp|snp-prop|snp-proportion" },
        { nameof(RunnerOptions.SamplesToGenProportion), "stgp|samples-to-gen-prop|samples-to-gen-proportion" },
        { nameof(RunnerOptions.LegendPath), "lp|legend-path" },
        { nameof(RunnerOptions.HapPath), "hp|hap-path" },
        { nameof(RunnerOptions.ResultGenPath), "rgp|result-gen-path" },
        { nameof(RunnerOptions.ResultHapPath), "rhp|result-hap-path" },
        { nameof(RunnerOptions.ExpectedGenPath), "egp|expected-gen-path" },
    };

    private readonly List<(Predicate<RunnerOptions> Check, string ErrorMessage)> _checks = new()
    {
        (ro => ro.SnpProportion > 0 && ro.SnpProportion < 1, $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.SnpProportion)]} should be in (0, 1)"),
        (ro => ro.SamplesToGenProportion > 0 && ro.SamplesToGenProportion < 1, $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.SamplesToGenProportion)]} should be in (0, 1)"),
        (ro => ro.LegendPath != null, $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.LegendPath)]} wasn't defined"),
        (ro => ro.HapPath != null, $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.HapPath)]} wasn't defined"),
        (ro => ro.ResultGenPath != null, $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.ResultGenPath)]} wasn't defined"),
        (ro => ro.ResultHapPath != null, $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.ResultHapPath)]} wasn't defined"),
        (ro => ro.ExpectedGenPath != null, $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.ExpectedGenPath)]} wasn't defined" ),
    };

    private readonly RunnerOptions _options;
    private readonly OptionSet _optionSet;
    private bool _showHelp = false;

    public RunnerOptionsParser()
    {
        _options = new RunnerOptions
        {
            SystemType = SystemType.Linux,
        };

        _optionSet = new OptionSet
        {
            { $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.SnpProportion)]}=", "Proportion of SNP that goes to the .gens file, value in (0, 1)", v => _options.SnpProportion = TryParseDouble(v, "sp|snp-prop|snp-proportion") },
            { $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.SamplesToGenProportion)]}=", "Proportion of samples that goes to the .gens file, value in (0, 1)", v => _options.SamplesToGenProportion = TryParseDouble(v, "gp|gen-prop|gen-proportion") },
            { $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.LegendPath)]}=", "Path to initial .legend file", v => _options.LegendPath = v },
            { $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.HapPath)]}=", "Path to initial .hap file", v => _options.HapPath = v },
            { $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.ResultGenPath)]}=", "Path to resulting .gen file", v => _options.ResultGenPath = v },
            { $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.ResultHapPath)]}=", "Path to resulting .hap file", v => _options.ResultHapPath = v },
            { $"{_paramNameByRunnerOptionsName[nameof(RunnerOptions.ExpectedGenPath)]}=", "", v => _options.ExpectedGenPath = v },
            { "linux", "If result of program should be used on linux system (by default no of --mac or --windows wasn't defined)", _ => _options.SystemType = SystemType.Linux },
            { "mac", "If result of program should be used on Mac system", _ => _options.SystemType = SystemType.Mac },
            { "windows", "If result of program should be used on windows system", _ => _options.SystemType = SystemType.Windows },
            { "h|help", "Show help", _ => _showHelp = true },
        };
    }

    public bool TryParse(string[] args, out RunnerOptions options)
    {
        options = default(RunnerOptions);
        _optionSet.Parse(args);
        
        if (_showHelp)
        {
            _optionSet.WriteOptionDescriptions(Console.Out);
            return false;
        }

        foreach (var check in _checks)
        {
            if (!check.Check(_options))
            {
                System.Console.WriteLine(check.ErrorMessage);
                return false;
            }
        }

        options = _options;
        return true;
    }

    private double TryParseDouble(string value, string paramName)
    {
        if (double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double parsed))
        {
            return parsed;
        }
        else
        {
            throw new Exception($"Can't parse {paramName}. Number should be in format \"<integer>.<floating>\"");
        }
    }
}