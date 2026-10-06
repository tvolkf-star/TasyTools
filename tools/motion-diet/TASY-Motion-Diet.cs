using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

class TasyMotionDiet : Form
{
    const string SiteUrl = "https://tasy.pro/";
    readonly List<string> files = new List<string>();
    readonly ListBox list = new ListBox();
    readonly TextBox outBox = new TextBox();
    readonly Label status = new Label();
    readonly Label modeInfo = new Label();
    readonly Label formatInfo = new Label();
    readonly Button go = new Button();

    readonly RadioButton gif = new RadioButton();
    readonly RadioButton webp = new RadioButton();
    readonly RadioButton ultra = new RadioButton();
    readonly RadioButton beauty = new RadioButton();
    readonly RadioButton light = new RadioButton();
    readonly RadioButton ui = new RadioButton();

    string ffmpeg;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TasyMotionDiet());
    }

    TasyMotionDiet()
    {
        Text = "TASY Motion Diet";
        ClientSize = new Size(720, 680);
        MinimumSize = new Size(650, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        BackColor = Color.White;
        AllowDrop = true;
        ffmpeg = FindFFmpeg();

        var title = new Label { Text="TASY Motion Diet", Font=new Font("Segoe UI Semibold",22), AutoSize=true, Location=new Point(24,18) };
        var sub = new Label { Text="Превращает видео в лёгкую интернет-анимацию.", AutoSize=true, Location=new Point(27,62) };
        var spec = new Label { Text="GIF / Animated WebP · до 480 px · без лишних данных", AutoSize=true, ForeColor=Color.DimGray, Location=new Point(27,88) };

        var drop = new Panel { Location=new Point(28,125), Size=new Size(664,88), BorderStyle=BorderStyle.FixedSingle, AllowDrop=true, Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right };
        var dropText = new Label { Text="Перетащите видео сюда\r\nили нажмите «Добавить файлы»", Dock=DockStyle.Fill, TextAlign=ContentAlignment.MiddleCenter, Font=new Font("Segoe UI",12), AllowDrop=true };
        drop.Controls.Add(dropText);

        list.Location=new Point(28,228);
        list.Size=new Size(664,85);
        list.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;
        list.HorizontalScrollbar=true;

        var add = new Button { Text="Добавить файлы", Location=new Point(28,326), Size=new Size(145,34), Anchor=AnchorStyles.Bottom|AnchorStyles.Left };
        var clear = new Button { Text="Очистить список", Location=new Point(181,326), Size=new Size(145,34), Anchor=AnchorStyles.Bottom|AnchorStyles.Left };

        var formats = new GroupBox { Text="Формат", Location=new Point(28,374), Size=new Size(664,72), Anchor=AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right };
        gif.Text="GIF";
        gif.Location=new Point(18,27);
        gif.AutoSize=true;
        webp.Text="Animated WebP";
        webp.Location=new Point(100,27);
        webp.AutoSize=true;
        webp.Checked=true;
        formatInfo.Location=new Point(260,24);
        formatInfo.Size=new Size(390,38);
        formatInfo.ForeColor=Color.DimGray;
        formats.Controls.AddRange(new Control[]{gif,webp,formatInfo});

        var modes = new GroupBox { Text="Режим", Location=new Point(28,458), Size=new Size(664,78), Anchor=AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right };
        ultra.Text="ULTRA BEAUTY";
        ultra.Location=new Point(18,27);
        ultra.AutoSize=true;
        ultra.Checked=true;
        beauty.Text="BEAUTY";
        beauty.Location=new Point(160,27);
        beauty.AutoSize=true;
        light.Text="LIGHT";
        light.Location=new Point(275,27);
        light.AutoSize=true;
        ui.Text="UI";
        ui.Location=new Point(365,27);
        ui.AutoSize=true;
        modeInfo.Location=new Point(445,18);
        modeInfo.Size=new Size(205,52);
        modeInfo.ForeColor=Color.DimGray;
        modes.Controls.AddRange(new Control[]{ultra,beauty,light,ui,modeInfo});

        var outLabel = new Label { Text="Сохранить в:", AutoSize=true, Location=new Point(28,557), Anchor=AnchorStyles.Bottom|AnchorStyles.Left };
        outBox.Location=new Point(120,553);
        outBox.Size=new Size(463,28);
        outBox.Anchor=AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;
        var browse = new Button { Text="Выбрать", Location=new Point(592,551), Size=new Size(100,32), Anchor=AnchorStyles.Bottom|AnchorStyles.Right };

        go.Text="СДЕЛАТЬ";
        go.Font=new Font("Segoe UI Semibold",11);
        go.Location=new Point(28,611);
        go.Size=new Size(190,40);
        go.Anchor=AnchorStyles.Bottom|AnchorStyles.Left;

        status.Text = ffmpeg != null ? "Готово к работе." : "FFmpeg не найден.";
        status.Location=new Point(235,621);
        status.Size=new Size(300,24);
        status.Anchor=AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;

        var link = new LinkLabel {
            Text="TASY.PRO ★★★★★",
            AutoSize=true,
            Location=new Point(555,621),
            Anchor=AnchorStyles.Bottom|AnchorStyles.Right,
            LinkColor=Color.Black,
            ActiveLinkColor=Color.DimGray
        };

        Controls.AddRange(new Control[]{title,sub,spec,drop,list,add,clear,formats,modes,outLabel,outBox,browse,go,status,link});

        DragEnter += DragEnterHandler;
        DragDrop += DropHandler;
        drop.DragEnter += DragEnterHandler;
        drop.DragDrop += DropHandler;
        dropText.DragEnter += DragEnterHandler;
        dropText.DragDrop += DropHandler;

        add.Click += delegate {
            using(var d=new OpenFileDialog { Multiselect=true, Filter="Видео|*.mp4;*.mov;*.mkv;*.webm;*.avi;*.m4v|Все файлы|*.*" })
                if(d.ShowDialog()==DialogResult.OK) AddVideos(d.FileNames);
        };

        clear.Click += delegate {
            files.Clear();
            list.Items.Clear();
            status.Text="Список очищен.";
        };

        browse.Click += delegate {
            using(var d=new FolderBrowserDialog()) {
                if(Directory.Exists(outBox.Text)) d.SelectedPath=outBox.Text;
                if(d.ShowDialog()==DialogResult.OK) outBox.Text=d.SelectedPath;
            }
        };

        link.LinkClicked += delegate {
            Process.Start(new ProcessStartInfo(SiteUrl){UseShellExecute=true});
        };

        gif.CheckedChanged += delegate { UpdateInfo(); };
        webp.CheckedChanged += delegate { UpdateInfo(); };
        ultra.CheckedChanged += delegate { UpdateInfo(); };
        beauty.CheckedChanged += delegate { UpdateInfo(); };
        light.CheckedChanged += delegate { UpdateInfo(); };
        ui.CheckedChanged += delegate { UpdateInfo(); };
        go.Click += ProcessVideos;

        UpdateInfo();
    }

    void UpdateInfo()
    {
        if(webp.Checked)
            formatInfo.Text="WebP: полноцветная анимация\nЛучше для рендеров и интернета";
        else
            formatInfo.Text="GIF: максимальная совместимость\nПалитровая анимация";

        if(ultra.Checked)
            modeInfo.Text=webp.Checked ? "480 px · 12 fps · quality 100\nМаксимум красоты" : "480 px · 12 fps · 256 цветов\nМаксимум GIF";
        else if(beauty.Checked)
            modeInfo.Text=webp.Checked ? "480 px · 12 fps · quality 95\nРендеры и градиенты" : "480 px · 12 fps · 256 цветов\nРендеры и градиенты";
        else if(light.Checked)
            modeInfo.Text=webp.Checked ? "480 px · 8 fps · quality 90\nКрасота при меньшем весе" : "480 px · 8 fps · 256 цветов\nМедленные пролёты";
        else
            modeInfo.Text=webp.Checked ? "480 px · 10 fps · quality 70\nИнтерфейсы" : "480 px · 10 fps · 128 цветов\nИнтерфейсы";
    }

    void DragEnterHandler(object sender, DragEventArgs e)
    {
        if(e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect=DragDropEffects.Copy;
    }

    void DropHandler(object sender, DragEventArgs e)
    {
        AddVideos((string[])e.Data.GetData(DataFormats.FileDrop));
    }

    void AddVideos(IEnumerable<string> paths)
    {
        foreach(var p in paths) {
            if(Directory.Exists(p)) AddVideos(Directory.GetFiles(p).Where(IsVideo));
            else if(File.Exists(p) && IsVideo(p) && !files.Contains(p,StringComparer.OrdinalIgnoreCase)) {
                files.Add(p);
                list.Items.Add(p);
            }
        }
        if(files.Count>0 && String.IsNullOrWhiteSpace(outBox.Text))
            outBox.Text=Path.GetDirectoryName(files[0]);
    }

    static bool IsVideo(string p)
    {
        var e=Path.GetExtension(p).ToLowerInvariant();
        return e==".mp4" || e==".mov" || e==".mkv" || e==".webm" || e==".avi" || e==".m4v";
    }

    string FindFFmpeg()
    {
        string baseDir=AppDomain.CurrentDomain.BaseDirectory;
        string[] candidates = {
            Path.Combine(baseDir,"tools","ffmpeg.exe"),
            Path.Combine(baseDir,"ffmpeg.exe")
        };
        foreach(var c in candidates) if(File.Exists(c)) return c;

        try {
            var psi=new ProcessStartInfo("where.exe","ffmpeg.exe"){
                UseShellExecute=false, RedirectStandardOutput=true, CreateNoWindow=true
            };
            using(var p=Process.Start(psi)) {
                var line=p.StandardOutput.ReadLine();
                p.WaitForExit();
                if(!String.IsNullOrWhiteSpace(line) && File.Exists(line)) return line;
            }
        } catch {}
        return null;
    }

    void ProcessVideos(object sender, EventArgs e)
    {
        ffmpeg=FindFFmpeg();
        if(ffmpeg==null) {
            MessageBox.Show("FFmpeg не найден. Положите ffmpeg.exe в папку tools, рядом с программой или добавьте FFmpeg в PATH.","TASY Motion Diet");
            return;
        }
        if(files.Count==0) {
            MessageBox.Show("Добавьте хотя бы одно видео.","TASY Motion Diet");
            return;
        }

        var dest=outBox.Text.Trim();
        if(dest=="") {
            MessageBox.Show("Выберите папку назначения.","TASY Motion Diet");
            return;
        }
        Directory.CreateDirectory(dest);

        int fps=12;
        string mode="ultra";
        if(beauty.Checked) { fps=12; mode="beauty"; }
        else if(light.Checked) { fps=8; mode="light"; }
        else if(ui.Checked) { fps=10; mode="ui"; }

        int ok=0, failed=0;
        long before=0, after=0;
        go.Enabled=false;
        UseWaitCursor=true;

        foreach(var src in files.ToArray()) {
            try {
                before += new FileInfo(src).Length;
                string dst, args;

                if(webp.Checked) {
                    int quality = ultra.Checked ? 100 : (beauty.Checked ? 95 : (light.Checked ? 90 : 70));
                    dst=Path.Combine(dest,Path.GetFileNameWithoutExtension(src)+"_"+mode+".webp");
                    string vf="fps="+fps+",scale='if(gte(iw,ih),min(480,iw),-2)':'if(gte(iw,ih),-2,min(480,ih))':flags=lanczos";
                    args="-loglevel error -y -i \""+src+"\" -map_metadata -1 -an -vf \""+vf+"\" -c:v libwebp_anim -lossless 0 -compression_level 6 -q:v "+quality+" -loop 0 \""+dst+"\"";
                } else {
                    int colors=ui.Checked ? 128 : 256;
                    dst=Path.Combine(dest,Path.GetFileNameWithoutExtension(src)+"_"+mode+".gif");
                    string filter="fps="+fps+",scale='if(gte(iw,ih),min(480,iw),-2)':'if(gte(iw,ih),-2,min(480,ih))':flags=lanczos,split[s0][s1];[s0]palettegen=max_colors="+colors+":stats_mode=diff[p];[s1][p]paletteuse=dither=sierra2_4a:diff_mode=rectangle";
                    args="-loglevel error -y -i \""+src+"\" -map_metadata -1 -an -filter_complex \""+filter+"\" -loop 0 \""+dst+"\"";
                }

                var psi=new ProcessStartInfo(ffmpeg,args){ UseShellExecute=false, CreateNoWindow=true };
                using(var p=Process.Start(psi)) {
                    p.WaitForExit();
                    if(p.ExitCode!=0) throw new Exception();
                }

                if(!File.Exists(dst)) throw new Exception();
                after += new FileInfo(dst).Length;
                ok++;
            } catch { failed++; }

            status.Text="Обработано: "+ok+" · ошибок: "+failed;
            Application.DoEvents();
        }

        go.Enabled=true;
        UseWaitCursor=false;
        status.Text=String.Format("Готово: {0} · {1:0.0} МБ → {2:0.0} МБ",ok,before/1048576.0,after/1048576.0);
    }
}
