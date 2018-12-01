$files = "Images","plugin.json"
[System.Collections.ArrayList]$files = $files
$files.Add("Newtonsoft.Json.dll")
$files.Add("Newtonsoft.Json.xml")
$files.Add("NLog.dll")
$files.Add("NLog.xml")
$files.Add("Pinyin4Net.dll")
$files.Add("Wox.Infrastructure.dll")
$files.Add("Wox.Infrastructure.pdb")
$files.Add("Wox.Plugin.dll")
$files.Add("Wox.Plugin.pdb")
$files.Add("Wox.Plugin.Youdao.dll")
$files.Add("Wox.Plugin.Youdao.pdb")

Compress-Archive $files youdao-pk-full.zip -Force

Rename-Item youdao-pk-full.zip youdao-pk-full.wox
