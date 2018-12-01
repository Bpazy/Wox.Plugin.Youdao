using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Wox.Infrastructure.Http;
using System.Security.Cryptography;
using System.Text;

namespace Wox.Plugin.Youdao
{
    public class TranslateResult
    {
        public int errorCode { get; set; }
        public List<string> translation { get; set; }
        public BasicTranslation basic { get; set; }
        public List<WebTranslation> web { get; set; }
    }

    // 有道词典-基本词典
    public class BasicTranslation
    {
        public string phonetic { get; set; }
        public List<string> explains { get; set; }
    }

    public class WebTranslation
    {
        public string key { get; set; }
        public List<string> value { get; set; }
    }

    public class Main : IPlugin, ISettingProvider
    {
        private const string TranslateUrl = "http://fanyi.youdao.com/openapi.do?keyfrom=WoxLauncher&key=1247918016&type=data&doctype=json&version=1.1&q=";
        private const string youdaoApiUrl = "http://openapi.youdao.com/api";
        private PluginInitContext _context;
        private readonly Settings _settings;
        private readonly SettingsViewModel _viewModel;

        public Main()
        {
            _viewModel = new SettingsViewModel();
            _settings = _viewModel.Settings;
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            const string ico = "Images\\youdao.ico";
            if (string.IsNullOrWhiteSpace(_settings.AppKey) || string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                results.Add(new Result()
                {
                    Title = "请设置有道翻译API Key",
                    SubTitle = "申请Key: http://ai.youdao.com/",
                    IcoPath = ico
                });
                return results;
            }
            if (query.Search.Length == 0)
            {
                results.Add(new Result
                {
                    Title = "开始有道中英互译",
                    SubTitle = "基于有道智云 API",
                    IcoPath = ico
                });
                return results;
            }
            string q = query.Search;
            string from = "auto";
            string to = "auto";
            string appKey = _settings.AppKey;
            string salt = "6";
            string sign = CalculateMD5Hash(appKey + q + salt + _settings.SecretKey);
            string url = $"{youdaoApiUrl}?q={q}&from={from}&to={to}&appKey={appKey}&salt={salt}&sign={sign}";
            var json = Http.Get(url).Result;
            TranslateResult o = JsonConvert.DeserializeObject<TranslateResult>(json);
            if (o.errorCode == 0)
            {
                if (o.translation != null)
                {
                    var translation = string.Join(", ", o.translation.ToArray());
                    var title = translation;
                    if (o.basic?.phonetic != null)
                    {
                        title += " [" + o.basic.phonetic + "]";
                    }
                    results.Add(new Result
                    {
                        Title = title,
                        SubTitle = "翻译结果",
                        IcoPath = ico,
                        Action = this.copyToClipboardFunc(translation)
                    });
                }

                if (o.basic?.explains != null)
                {
                    var explantion = string.Join(",", o.basic.explains.ToArray());
                    results.Add(new Result
                    {
                        Title = explantion,
                        SubTitle = "简明释义",
                        IcoPath = ico,
                        Action = this.copyToClipboardFunc(explantion)
                    });
                }

                if (o.web != null)
                {
                    foreach (WebTranslation t in o.web)
                    {
                        var translation = string.Join(",", t.value.ToArray());
                        results.Add(new Result
                        {
                            Title = translation,
                            SubTitle = "网络释义：" + t.key,
                            IcoPath = ico,
                            Action = this.copyToClipboardFunc(translation)
                        });
                    }
                }
            }
            else
            {
                string error = string.Empty;
                switch (o.errorCode)
                {
                    case 20:
                        error = "要翻译的文本过长";
                        break;
                    case 30:
                        error = "无法进行有效的翻译";
                        break;
                    case 40:
                        error = "不支持的语言类型";
                        break;
                    case 50:
                        error = "无效的key";
                        break;
                    case 101:
                        error = "缺少必填的参数，出现这个情况还可能是et的值和实际加密方式不对应";
                        break;
                    case 102:
                        error = "不支持的语言类型";
                        break;
                    case 103:
                        error = "翻译文本过长";
                        break;
                    case 104:
                        error = "不支持的API类型";
                        break;
                    case 105:
                        error = "不支持的签名类型";
                        break;
                    case 106:
                        error = "不支持的响应类型";
                        break;
                    case 107:
                        error = "不支持的传输加密类型";
                        break;
                    case 108:
                        error = "appKey无效";
                        break;
                    case 109:
                        error = "batchLog格式不正确";
                        break;
                    case 110:
                        error = "无相关服务的有效实例";
                        break;
                    case 111:
                        error = "开发者账号无效，可能是账号为欠费状态";
                        break;
                    case 201:
                        error = "解密失败，可能为DES,BASE64,URLDecode的错误";
                        break;
                    case 202:
                        error = "签名检验失败";
                        break;
                    case 203:
                        error = "访问IP地址不在可访问IP列表";
                        break;
                    case 301:
                        error = "辞典查询失败";
                        break;
                    case 302:
                        error = "翻译查询失败";
                        break;
                    case 303:
                        error = "服务端的其它异常";
                        break;
                    case 401:
                        error = "账户已经欠费停";
                        break;
                    default:
                        error = "未知错误, errorCode = " + o.errorCode;
                        break;
                }

                results.Add(new Result
                {
                    Title = error,
                    IcoPath = ico
                });
            }

            return results;
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
        }

        private System.Func<ActionContext, bool> copyToClipboardFunc(string text)
        {
            return c =>
            {
                if (this.copyToClipboard(text))
                {
                    _context.API.ShowMsg("翻译已被存入剪贴板");
                }
                else
                {
                    _context.API.ShowMsg("剪贴板打开失败，请稍后再试");
                }
                return false;
            };
        }

        private string CalculateMD5Hash(string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private bool copyToClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        public Control CreateSettingPanel()
        {
            return new SettingsControl(_viewModel);
        }
    }
}
