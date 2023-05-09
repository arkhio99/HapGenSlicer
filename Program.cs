using HapGenSlicer;

if (!new RunnerOptionsParser().TryParse(args, out RunnerOptions options))
{
    return;
}

var slicer = new HapGenSlicer.Slicer(
    options.LegendPath,
    options.HapPath,
    options.ResultGenPath,
    options.ResultHapPath,
    options.ExpectedGenPath,
    options.SnpProportion,
    options.SamplesToGenProportion);

slicer.Slice();