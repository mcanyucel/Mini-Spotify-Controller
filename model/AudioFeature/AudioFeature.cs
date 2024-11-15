namespace MiniSpotifyController.model.AudioFeature;

public record AudioFeature(string FeatureName, double FeatureValue, double FeatureMin, double FeatureMax, FeatureType FeatureType = FeatureType.Number)
{
    public virtual bool Equals(AudioFeature? other) => other?.FeatureName == FeatureName;
    
    public override int GetHashCode() => FeatureName.GetHashCode();

    public AudioFeature CloneWithNewValue(double newValue) => this with { FeatureValue = newValue };
    public override string ToString()
    {
        string result;

        if (FeatureType == FeatureType.Number)
            result = $"{FeatureName}: {FeatureValue}";
        else
        {
            result = FeatureName switch
            {
                "key" => $"{FeatureName}: {KeyNumberToKey()}",
                "mode" => $"{FeatureName}: {ModeNumberToMode()}",
                _ => $"{FeatureName}: {FeatureValue}"
            };
        }

        return result;

    }

    private string ModeNumberToMode()
    {
        return FeatureValue switch
        {
            0 => "Minor",
            1 => "Major",
            _ => "Unknown"
        };
    }

    private string KeyNumberToKey()
    {
        return FeatureValue switch
        {
            0 => "C",
            1 => "C♯, D♭",
            2 => "D",
            3 => "D♯, E♭",
            4 => "E",
            5 => "F",
            6 => "F♯, G♭",
            7 => "G",
            8 => "G♯, A♭",
            9 => "A",
            10 => "A♯, B♭",
            11 => "B",
            _ => "Unknown"
        };
    }
}
