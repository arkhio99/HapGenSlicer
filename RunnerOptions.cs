namespace HapGenSlicer;

public class RunnerOptions
{
    public double SnpProportion { get; set; }
    public double SamplesToGenProportion { get; set; }
    public string LegendPath { get; set; }
    public string HapPath { get; set; }
    public string ResultGenPath { get; set; }
    public string ResultHapPath { get; set; }
    public string ExpectedGenPath { get; set; }
    public SystemType SystemType { get; set; }
}