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
            int jsonCount = jsonObj["gamelist"].Count();
            if (jsonCount >= 1)
            {
                for (int i = 0; i < jsonCount; i++)
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
            tb_desc.Text = jsonObj["gamelist"][cb_game.SelectedIndex]["desc"].ToString();

            string iniTips = "";
            iniPropCount = jsonObj["gamelist"][cb_game.SelectedIndex]["iniData"].Count();
            for(int i = 0; i < iniPropCount; i++)
            {
                iniTips += jsonObj["gamelist"][cb_game.SelectedIndex]["iniData"][i]["iniName"].ToString() + " : ";
                iniTips += jsonObj["gamelist"][cb_game.SelectedIndex]["iniData"][i]["iniPath"].ToString().Replace("%userprofile%", getUserDir()).Replace("%mydocuments%", getDocDir()) + "\n";
            }
            iniTips = "修改的ini有\n" + iniTips;
            tb_tips.Text = iniTips;
        }

        private string getUserDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        private string getDocDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void btn_about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("程序作者:AldarisX(bilibili)\n提出者:夜猫无心");
            MessageBox.Show("关于一些操作\n%userprofile%为用户目录\n%mydocuments%为用的文档目录");
        }

        private void btn_import_Click(object sender, RoutedEventArgs e)
        {
            tb_tips.Text = "";
            for(int i = 0; i < iniPropCount; i++)
            {
                string iniPath = jsonObj["gamelist"][cb_game.SelectedIndex]["iniData"][i]["iniPath"].ToString().Replace("%userprofile%", getUserDir()).Replace("%mydocuments%", getDocDir());
                if (File.Exists(iniPath))
                {
                    int sectionCount = jsonObj["gamelist"][cb_game.SelectedIndex]["iniData"][i]["iniProp"].Count();
                    for (int j = 0; j < sectionCount; j++)
                    {
                        string section = jsonObj["gamelist"][cb_game.SelectedIndex]["iniData"][i]["iniProp"][j]["section"].ToString();
                        string[] inis = jsonObj["gamelist"][cb_game.SelectedIndex]["iniData"][i]["iniProp"][j]["text"].ToString().Split(';');
                        for (int k = 0; k < inis.Count(); k++)
                        {
                            if (inis[k].Length > 0)
                            {
                                tb_tips.AppendText(inis[k] + "\n");
                                string[] kys = inis[k].Split('=');
                                string key = kys[0];
                                string value = kys[1];
                                Kernel32.WriteIniKeys(section, key, value, iniPath);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("文件不存在\n" + iniPath);
                }
            }
            MessageBox.Show("操作成功完成");
        }
    }
}
