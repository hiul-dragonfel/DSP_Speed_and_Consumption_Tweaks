del "ManifesteInfos.cs"
$json = Get-Content "manifest.json" | ConvertFrom-Json;
$cs = @"
namespace DSP_Speed_and_Consumption_Tweaks {
    public static class ManifesteInfos {
        public const string GUID = "$($json.GUID)";
        public const string PluginName = "$($json.namespace)";
        public const string VersionString = "$($json.version_number)";
    }
}
"@;
Set-Content -Path "ManifesteInfos.cs" -Value $cs -Encoding UTF8;