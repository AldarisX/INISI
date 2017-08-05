using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace INISI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        JObject jsonObj;
        int iniPropCount;

        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        public void init()
        {
            string jsontext = File.ReadAllText(@"config.json");
            jsonObj = JObject.Parse(jsontext);
            //取得配置总数
            int gameCount = jsonObj["gamelist"].Count();
            if (gameCount >= 1)
            {
                for (int i = 0; i < gameCount; i++)
                {
                    cb_game.Items.Add(jsonObj["gamelist"][i]["name"]);
                }
                cb_game.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("程序数据文件为空!!!\n确定config.json存在?");
            }
        }

        private void cb_game_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            JToken selectGame = jsonObj["gamelist"][cb_game.SelectedIndex];
            tb_tips.Text = selectGame["desc"].ToString() + "\n";

            string iniTips = "";
            //取得要修改的配置文件总数
            iniPropCount = selectGame["iniData"].Count();
            for (int i = 0; i < iniPropCount; i++)
            {
                //取得ini的属性
                JToken gameIni = selectGame["iniData"][i];
                iniTips += gameIni["iniName"].ToString() + " : ";
                iniTips += getRealPath(gameIni["iniPath"].ToString()) + "\n";
            }
            //显示提示信息
            if (cb_tips.IsChecked == true)
            {
                iniTips = "修改的ini有\n" + iniTips;
                tb_tips.Text = iniTips;
            }
        }

        private void btn_about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("程序作者:AldarisX(bilibili)\n提出者:夜猫无心");
            MessageBox.Show("关于一些操作\n%userprofile%为用户目录\n%mydocuments%为用的文档目录");
        }

        private void btn_import_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < iniPropCount; i++)
            {
                //取得ini的属性
                JToken gameIni = jsonObj["gamelist"][cb_game.SelectedIndex]["iniData"][i];
                string iniPath = getRealPath(gameIni["iniPath"].ToString());
                //修改ini文件权限,确保可写
                File.SetAttributes(iniPath, FileAttributes.Normal);
                if (cb_tips.IsChecked == true)
                {
                    tb_tips.AppendText("\n" + iniPath + "\n");
                }
                if (File.Exists(iniPath))
                {
                    int sectionCount = gameIni["iniProp"].Count();
                    for (int j = 0; j < sectionCount; j++)
                    {
                        //解析ini的内容
                        JToken iniProp = gameIni["iniProp"][j];
                        string section = iniProp["section"].ToString();
                        string[] inis = iniProp["text"].ToString().Split(';');
                        if (cb_tips.IsChecked == true)
                        {
                            tb_tips.AppendText("[" + section + "]" + "\n");
                        }
                        for (int k = 0; k < inis.Count(); k++)
                        {
                            if (inis[k].Length > 0)
                            {
                                string[] kys = inis[k].Split('=');
                                string key = kys[0];
                                string value = kys[1];
                                //写入ini
                                Kernel32.WriteIniKeys(section, key, value, iniPath);
                                if (cb_tips.IsChecked == true)
                                {
                                    tb_tips.AppendText(trimString(inis[k]) + "\n");
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("文件不存在\n" + iniPath);
                }
            }
            tb_tips.AppendText("修改完毕\n");
            tb_tips.ScrollToEnd();
        }

        private string getUserDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        private string getDocDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public string getRealPath(string str)
        {
            return str.Replace("%userprofile%", getUserDir()).Replace("%mydocuments%", getDocDir());
        }

        public string trimString(string str)
        {
            return str.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace(" ", "");
        }
    }

}
